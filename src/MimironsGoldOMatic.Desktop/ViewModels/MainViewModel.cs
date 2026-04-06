using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MimironsGoldOMatic.Desktop.Services;
using MimironsGoldOMatic.Desktop.Services.Updates;
using MimironsGoldOMatic.Desktop.Win32;
using MimironsGoldOMatic.Shared;

namespace MimironsGoldOMatic.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IEbsDesktopClient _api;
    private readonly WoWInjectionCoordinator _inject;
    private readonly PayoutSnapshotCache _cache;
    private readonly NotifiedWhisperStore _whisperStore;
    private readonly DesktopConnectionContext _connection;
    private readonly DesktopSettingsStore _settingsStore;
    private readonly IUpdateService _updateService;
    private readonly WoWChatLogTailService _tail;
    private readonly DispatcherTimer _pollTimer;
    private readonly DispatcherTimer _wowTimer;
    private HashSet<Guid> _whisperNotified = new();

    public ObservableCollection<PayoutDto> Payouts { get; } = new();

    public ObservableCollection<string> DeliveryLog { get; } = new();

    [ObservableProperty]
    private string _wowStatus = "WoW: (not focused)";

    [ObservableProperty]
    private bool _wowForegroundDetected;

    [ObservableProperty]
    private string _apiStatus = "API: not tested";

    [ObservableProperty]
    private PayoutDto? _selectedPayout;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    private string _currentVersion = "-";

    [ObservableProperty]
    private string _latestVersion = "-";

    [ObservableProperty]
    private string? _releaseNotesUrl;

    [ObservableProperty]
    private string _updateStatusMessage = "Проверка обновлений не выполнялась.";

    public MainViewModel(
        IEbsDesktopClient api,
        WoWInjectionCoordinator inject,
        PayoutSnapshotCache cache,
        NotifiedWhisperStore whisperStore,
        DesktopConnectionContext connection,
        DesktopSettingsStore settingsStore,
        IUpdateService updateService)
    {
        _api = api;
        _inject = inject;
        _cache = cache;
        _whisperStore = whisperStore;
        _connection = connection;
        _settingsStore = settingsStore;
        _updateService = updateService;
        _whisperNotified = whisperStore.Load();

        Payouts.CollectionChanged += OnPayoutsCollectionChanged;

        _tail = new WoWChatLogTailService(connection, api, cache, updateService, inject, AppendLog);
        _tail.Start();

        _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(connection.GetClampedPollIntervalSeconds()) };
        _pollTimer.Tick += (_, _) => _ = PollRefreshAsync();
        _pollTimer.Start();

        _wowTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _wowTimer.Tick += (_, _) => UpdateWowStatus();
        _wowTimer.Start();

        UpdateWowStatus();
        _ = RefreshAsync();
        _ = CheckForUpdatesInternalAsync(logFailureToDeliveryLog: false);
    }

    private void OnPayoutsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        SyncInjectCommand.NotifyCanExecuteChanged();

    public void ApplyPollIntervalFromSettings()
    {
        _pollTimer.Interval = TimeSpan.FromSeconds(_connection.GetClampedPollIntervalSeconds());
    }

    public void ReloadTailAfterSettingsChanged()
    {
        _tail.ResetPosition();
    }

    public DesktopConnectionContext Connection => _connection;

    public DesktopSettingsStore SettingsStore => _settingsStore;

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        await CheckForUpdatesInternalAsync(logFailureToDeliveryLog: true).ConfigureAwait(true);
    }

    [RelayCommand(CanExecute = nameof(CanOpenUpdatePage))]
    private void OpenUpdatePage()
    {
        if (string.IsNullOrWhiteSpace(ReleaseNotesUrl))
            return;
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = ReleaseNotesUrl,
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            AppendLog($"Open update page failed: {ex.Message}");
        }
    }

    /// <summary>Quick connectivity check (<c>GET /api/payouts/pending</c>) for Settings UI.</summary>
    public async Task<string?> TestApiConnectionAsync()
    {
        try
        {
            _ = await _api.GetPendingAsync(CancellationToken.None).ConfigureAwait(true);
            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private void UpdateWowStatus()
    {
        var ok = WoWForegroundLocator.TryGetForegroundWoWWindow() != IntPtr.Zero;
        WowForegroundDetected = ok;
        WowStatus = ok ? "WoW: foreground target OK" : "WoW: focus WoW (foreground WoW.exe)";
    }

    private void AppendLog(string line)
    {
        var d = Application.Current?.Dispatcher;
        if (d is null)
            return;
        _ = d.BeginInvoke(() =>
        {
            DeliveryLog.Insert(0, $"{DateTime.Now:HH:mm:ss}  {line}");
            while (DeliveryLog.Count > 400)
                DeliveryLog.RemoveAt(DeliveryLog.Count - 1);
        });
    }

    /// <summary>Core pending fetch + UI list + winner whisper injection (no <see cref="IsBusy"/>).</summary>
    private async Task RefreshCoreAsync(CancellationToken ct = default)
    {
        var list = await _api.GetPendingAsync(ct).ConfigureAwait(true);
        _cache.UpdateFromPending(list);

        Payouts.Clear();
        foreach (var p in list.OrderByDescending(x => x.CreatedAt))
            Payouts.Add(p);

        ApiStatus = $"API: OK ({list.Count} in queue)";
        AppendLog($"Pending refresh: {list.Count} payout(s).");
        await TryNotifyPendingWinnersAsync(list).ConfigureAwait(true);
    }

    private async Task PollRefreshAsync()
    {
        if (IsBusy)
            return;
        IsBusy = true;
        try
        {
            await RefreshCoreAsync(CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ApiStatus = "API: error";
            AppendLog($"Poll failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            SyncInjectCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
            return;
        IsBusy = true;
        try
        {
            await RefreshCoreAsync(CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ApiStatus = "API: error";
            AppendLog($"Refresh failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            SyncInjectCommand.NotifyCanExecuteChanged();
        }
    }

    private async Task TryNotifyPendingWinnersAsync(IReadOnlyList<PayoutDto> list)
    {
        await Task.Yield();
        foreach (var p in list.Where(x => x.Status == PayoutStatus.Pending))
        {
            if (_whisperNotified.Contains(p.Id))
                continue;
            if (WoWForegroundLocator.TryGetForegroundWoWWindow() == IntPtr.Zero)
            {
                AppendLog($"NotifyWinnerWhisper skipped {p.Id:D} — WoW not foreground.");
                continue;
            }

            try
            {
                var line = WoWRunCommands.NotifyWinnerWhisper(p.Id, p.CharacterName);
                _inject.InjectChatLine(line, CancellationToken.None);
                _whisperNotified.Add(p.Id);
                _whisperStore.Save(_whisperNotified);
                AppendLog($"NotifyWinnerWhisper injected {p.Id:D} ({p.CharacterName})");
            }
            catch (Exception ex)
            {
                AppendLog($"NotifyWinnerWhisper failed {p.Id:D}: {ex.Message}");
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanSyncInject))]
    private async Task SyncInjectAsync()
    {
        if (WoWForegroundLocator.TryGetForegroundWoWWindow() == IntPtr.Zero)
        {
            AppendLog("Sync/Inject: WoW not foreground.");
            return;
        }

        var pending = Payouts.Where(x => x.Status == PayoutStatus.Pending).ToList();
        if (pending.Count == 0)
        {
            AppendLog("Sync/Inject: no Pending payouts.");
            return;
        }

        IsBusy = true;
        try
        {
            foreach (var p in pending)
            {
                await _api.PatchPayoutStatusAsync(p.Id, PayoutStatus.InProgress, CancellationToken.None).ConfigureAwait(true);
                AppendLog($"PATCH InProgress {p.Id:D}");
            }

            var commands = ReceiveGoldCommandChunker.BuildRunCommands(pending);
            foreach (var cmd in commands)
            {
                _inject.InjectChatLine(cmd, CancellationToken.None);
                AppendLog($"Injected ReceiveGold chunk ({cmd.Length} chars).");
            }

            await RefreshCoreAsync(CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            AppendLog($"Sync/Inject failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            SyncInjectCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanSyncInject() =>
        !IsBusy && WowForegroundDetected && Payouts.Any(x => x.Status == PayoutStatus.Pending);

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task MarkSentAsync()
    {
        if (SelectedPayout is null)
            return;
        try
        {
            await _api.PatchPayoutStatusAsync(SelectedPayout.Id, PayoutStatus.Sent, CancellationToken.None).ConfigureAwait(true);
            AppendLog($"Manual Mark Sent {SelectedPayout.Id:D}");
            await RefreshCoreAsync(CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            AppendLog($"Mark Sent failed: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task MarkFailedAsync()
    {
        if (SelectedPayout is null)
            return;
        try
        {
            await _api.PatchPayoutStatusAsync(SelectedPayout.Id, PayoutStatus.Failed, CancellationToken.None).ConfigureAwait(true);
            AppendLog($"Manual Failed {SelectedPayout.Id:D}");
            await RefreshCoreAsync(CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            AppendLog($"Mark Failed failed: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task MarkCancelledAsync()
    {
        if (SelectedPayout is null)
            return;
        try
        {
            await _api.PatchPayoutStatusAsync(SelectedPayout.Id, PayoutStatus.Cancelled, CancellationToken.None)
                .ConfigureAwait(true);
            AppendLog($"Manual Cancelled {SelectedPayout.Id:D}");
            await RefreshCoreAsync(CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            AppendLog($"Cancel failed: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanUnlock))]
    private async Task UnlockToPendingAsync()
    {
        if (SelectedPayout is null || SelectedPayout.Status != PayoutStatus.InProgress)
            return;
        try
        {
            await _api.PatchPayoutStatusAsync(SelectedPayout.Id, PayoutStatus.Pending, CancellationToken.None).ConfigureAwait(true);
            AppendLog($"Unlocked InProgress → Pending {SelectedPayout.Id:D}");
            await RefreshCoreAsync(CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            AppendLog($"Unlock failed: {ex.Message}");
        }
    }

    private bool HasSelection() => SelectedPayout is not null;

    private bool CanUnlock() => SelectedPayout?.Status == PayoutStatus.InProgress;

    partial void OnIsBusyChanged(bool value) => SyncInjectCommand.NotifyCanExecuteChanged();

    partial void OnWowForegroundDetectedChanged(bool value) => SyncInjectCommand.NotifyCanExecuteChanged();

    partial void OnSelectedPayoutChanged(PayoutDto? value)
    {
        MarkSentCommand.NotifyCanExecuteChanged();
        MarkFailedCommand.NotifyCanExecuteChanged();
        MarkCancelledCommand.NotifyCanExecuteChanged();
        UnlockToPendingCommand.NotifyCanExecuteChanged();
    }

    partial void OnReleaseNotesUrlChanged(string? value) => OpenUpdatePageCommand.NotifyCanExecuteChanged();

    private bool CanOpenUpdatePage() => !string.IsNullOrWhiteSpace(ReleaseNotesUrl);

    private async Task CheckForUpdatesInternalAsync(bool logFailureToDeliveryLog)
    {
        var result = await _updateService.CheckForUpdatesAsync(CancellationToken.None).ConfigureAwait(true);
        IsUpdateAvailable = result.IsUpdateAvailable;
        CurrentVersion = result.CurrentVersion;
        LatestVersion = result.LatestVersion;
        ReleaseNotesUrl = result.ReleaseNotesUrl;
        UpdateStatusMessage = result.StatusMessage;

        if (!result.IsSuccess && logFailureToDeliveryLog)
            AppendLog("Update check failed.");
    }

    public void Dispose()
    {
        Payouts.CollectionChanged -= OnPayoutsCollectionChanged;
        _tail.Dispose();
        _pollTimer.Stop();
        _wowTimer.Stop();
    }
}

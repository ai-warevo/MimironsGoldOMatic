using System.IO;
using System.Text;
using System.Threading;
using System.Text.Json;
using System.Text.RegularExpressions;
using MimironsGoldOMatic.Desktop.Services.Updates;
using MimironsGoldOMatic.Desktop.Win32;

namespace MimironsGoldOMatic.Desktop.Services;

/// <summary>Single tail of <c>WoWChatLog.txt</c> for <c>[MGM_WHO]</c>, <c>[MGM_ACCEPT]</c>, <c>[MGM_CONFIRM]</c> (<c>docs/overview/SPEC.md</c> В§10).</summary>
public sealed partial class WoWChatLogTailService : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly DesktopConnectionContext _connection;
    private readonly IEbsDesktopClient _api;
    private readonly PayoutSnapshotCache _payouts;
    private readonly IUpdateService? _updateService;
    private readonly WoWInjectionCoordinator? _injector;
    private readonly Action<string> _deliveryLog;
    private readonly System.Timers.Timer _timer = new(750) { AutoReset = true };
    private readonly object _ioLock = new();
    private long _position;
    private string _lineCarry = "";
    private readonly HashSet<string> _seen = new(StringComparer.Ordinal);
    private readonly HashSet<Guid> _giftInventoryRequested = new();
    private const int SeenTrim = 1500;
    private int _tickGate;

    public WoWChatLogTailService(
        DesktopConnectionContext connection,
        IEbsDesktopClient api,
        PayoutSnapshotCache payouts,
        Action<string> deliveryLog)
        : this(connection, api, payouts, null, null, deliveryLog)
    {
    }

    public WoWChatLogTailService(
        DesktopConnectionContext connection,
        IEbsDesktopClient api,
        PayoutSnapshotCache payouts,
        IUpdateService? updateService,
        WoWInjectionCoordinator? injector,
        Action<string> deliveryLog)
    {
        _connection = connection;
        _api = api;
        _payouts = payouts;
        _updateService = updateService;
        _injector = injector;
        _deliveryLog = deliveryLog;
        _timer.Elapsed += (_, _) => _ = Task.Run(OnTickAsync);
    }

    public void Start() => _timer.Start();

    public void Stop() => _timer.Stop();

    /// <summary>Call after operator changes log path so we re-read from start of file.</summary>
    public void ResetPosition()
    {
        lock (_ioLock)
        {
            _position = 0;
            _lineCarry = "";
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    private async Task OnTickAsync()
    {
        if (Interlocked.CompareExchange(ref _tickGate, 1, 0) != 0)
            return;
        try
        {
            var path = WoWChatLogPathResolver.Resolve(_connection.Settings);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            string[] linesToProcess;
            lock (_ioLock)
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                if (fs.Length < _position)
                {
                    _position = 0;
                    _lineCarry = "";
                }

                fs.Seek(_position, SeekOrigin.Begin);
                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096,
                    leaveOpen: true);
                var text = reader.ReadToEnd();
                _position = fs.Length;

                if (text.Length == 0)
                {
                    linesToProcess = [];
                    goto AfterRead;
                }

                var combined = _lineCarry + text;
                var parts = combined.Split('\n');
                _lineCarry = parts[^1];
                linesToProcess = parts[..^1];
            }

        AfterRead:
            foreach (var raw in linesToProcess)
                await ProcessLineAsync(raw.TrimEnd('\r'), CancellationToken.None).ConfigureAwait(false);
            await TryDriveGiftSelectionAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _deliveryLog($"[tail error] {ex.Message}");
        }
        finally
        {
            Interlocked.Exchange(ref _tickGate, 0);
        }
    }

    internal async Task ProcessLineAsync(string line, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(line))
            return;

        if (line.Contains("[MGM_UPDATE_CHECK]", StringComparison.Ordinal))
        {
            if (!MarkSeen("update:" + line))
                return;

            await HandleUpdateCheckAsync(ct).ConfigureAwait(false);
            return;
        }

        if (line.Contains("[MGM_WHO]", StringComparison.Ordinal))
        {
            var json = ExtractWhoJson(line);
            if (json is null)
            {
                _deliveryLog("[MGM_WHO] parse skip (no JSON)");
                return;
            }

            if (!MarkSeen("who:" + json))
                return;

            VerifyCandidateRequest? dto;
            try
            {
                dto = JsonSerializer.Deserialize<VerifyCandidateRequest>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                _deliveryLog($"[MGM_WHO] JSON error: {ex.Message}");
                return;
            }

            if (dto is null)
                return;

            try
            {
                await _api.VerifyCandidateAsync(dto, ct).ConfigureAwait(false);
                _deliveryLog($"[MGM_WHO] verify-candidate OK spin={dto.SpinCycleId:D} online={dto.Online}");
            }
            catch (Exception ex)
            {
                _deliveryLog($"[MGM_WHO] verify-candidate failed: {ex.Message}");
            }

            return;
        }

        var items = GiftItemsRegex().Match(line);
        if (items.Success && Guid.TryParse(items.Groups[1].Value, out var giftRequestId))
        {
            if (!MarkSeen("gift-items:" + giftRequestId))
                return;
            var json = items.Groups[2].Value;
            List<GiftSelectedItemDto>? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<List<GiftSelectedItemDto>>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                _deliveryLog($"[MGM_ITEMS] JSON error: {ex.Message}");
                return;
            }

            if (parsed == null || parsed.Count == 0)
            {
                try
                {
                    await _api.PatchGiftRequestStateAsync(giftRequestId, GiftRequestState.Failed, "no_items_available", ct).ConfigureAwait(false);
                    _deliveryLog($"[MGM_ITEMS] no items available, marked Failed {giftRequestId:D}");
                }
                catch (Exception ex)
                {
                    _deliveryLog($"[MGM_ITEMS] fail-state update failed: {ex.Message}");
                }
                return;
            }

            var spinMs = Random.Shared.Next(3000, 5001);
            _deliveryLog($"[MGM_ITEMS] roulette spin {spinMs}ms, items={parsed.Count}");
            await Task.Delay(spinMs, ct).ConfigureAwait(false);
            var selected = parsed[Random.Shared.Next(parsed.Count)];
            try
            {
                await _api.SelectGiftItemAsync(giftRequestId, selected, ct).ConfigureAwait(false);
                _deliveryLog($"[MGM_ITEMS] selected item '{selected.Name}' for {giftRequestId:D}");

                var queue = await _api.GetGiftQueueAsync(ct).ConfigureAwait(false);
                var req = queue.FirstOrDefault(x => x.Id == giftRequestId);
                if (req != null && _injector != null)
                {
                    _injector.InjectChatLine(WoWRunCommands.RequestGiftConfirmation(giftRequestId, req.CharacterName), ct);
                    _deliveryLog($"[MGM_GIFT] confirmation whisper requested {giftRequestId:D} ({req.CharacterName})");
                }
            }
            catch (Exception ex)
            {
                _deliveryLog($"[MGM_ITEMS] select-item failed: {ex.Message}");
            }
            return;
        }

        var giftAccept = GiftAcceptRegex().Match(line);
        if (giftAccept.Success && Guid.TryParse(giftAccept.Groups[1].Value, out var giftAcceptId))
        {
            if (!MarkSeen("gift-accept:" + giftAcceptId))
                return;
            try
            {
                await _api.ConfirmGiftAsync(giftAcceptId, true, ct).ConfigureAwait(false);
                _deliveryLog($"[MGM_GIFT_ACCEPT] confirm OK {giftAcceptId:D}");
            }
            catch (Exception ex)
            {
                _deliveryLog($"[MGM_GIFT_ACCEPT] failed: {ex.Message}");
            }
            return;
        }

        var accept = AcceptRegex().Match(line);
        if (accept.Success && Guid.TryParse(accept.Groups[1].Value, out var acceptId))
        {
            if (!MarkSeen("accept:" + acceptId))
                return;
            if (!_payouts.TryGetCharacterName(acceptId, out var characterName))
            {
                _deliveryLog($"[MGM_ACCEPT] unknown payout {acceptId:D} вЂ” refresh pending list");
                return;
            }

            try
            {
                await _api.ConfirmAcceptanceAsync(acceptId, characterName, ct).ConfigureAwait(false);
                _deliveryLog($"[MGM_ACCEPT] confirm-acceptance OK {acceptId:D} ({characterName})");
            }
            catch (Exception ex)
            {
                _deliveryLog($"[MGM_ACCEPT] failed: {ex.Message}");
            }

            return;
        }

        var confirm = ConfirmRegex().Match(line);
        if (confirm.Success && Guid.TryParse(confirm.Groups[1].Value, out var confirmId))
        {
            if (!MarkSeen("confirm:" + confirmId))
                return;
            try
            {
                await _api.PatchPayoutStatusAsync(confirmId, PayoutStatus.Sent, ct).ConfigureAwait(false);
                _deliveryLog($"[MGM_CONFIRM] PATCH Sent OK {confirmId:D}");
            }
            catch (Exception ex)
            {
                _deliveryLog($"[MGM_CONFIRM] failed: {ex.Message}");
            }
        }

    }

    private async Task TryDriveGiftSelectionAsync(CancellationToken ct)
    {
        if (_injector is null)
            return;
        IReadOnlyList<GiftRequestDto> queue;
        try
        {
            queue = await _api.GetGiftQueueAsync(ct).ConfigureAwait(false);
        }
        catch
        {
            return;
        }

        var selecting = queue
            .Where(x => x.State == GiftRequestState.SelectingItem)
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefault();
        if (selecting == null)
            return;
        if (_giftInventoryRequested.Contains(selecting.Id))
            return;

        _injector.InjectChatLine(WoWRunCommands.RequestAllInventoryItems(selecting.Id), ct);
        _giftInventoryRequested.Add(selecting.Id);
        _deliveryLog($"[MGM_GIFT] requested inventory for {selecting.Id:D}");
    }

    private async Task HandleUpdateCheckAsync(CancellationToken ct)
    {
        if (_updateService is null)
        {
            _deliveryLog("[MGM_UPDATE_CHECK] update service is not configured.");
            return;
        }

        var result = await _updateService.CheckForUpdatesAsync(ct).ConfigureAwait(false);
        var message = BuildUpdateChatMessage(result);

        if (_injector is null)
        {
            _deliveryLog($"[MGM_UPDATE_CHECK] {message}");
            return;
        }

        try
        {
            _injector.InjectChatLine(WoWRunCommands.ChatFrameMessage(message), ct);
            _deliveryLog($"[MGM_UPDATE_CHECK] delivered: {message}");
        }
        catch (Exception ex)
        {
            _deliveryLog($"[MGM_UPDATE_CHECK] delivery failed: {ex.Message}");
        }
    }

    private static string BuildUpdateChatMessage(VersionCheckResult result)
    {
        if (!result.IsSuccess)
        {
            return "Mimiron's Gold-o-Matic: РќРµ СѓРґР°Р»РѕСЃСЊ РїСЂРѕРІРµСЂРёС‚СЊ РѕР±РЅРѕРІР»РµРЅРёСЏ. РЈР±РµРґРёС‚РµСЃСЊ, С‡С‚Рѕ Desktop-РїСЂРёР»РѕР¶РµРЅРёРµ РїРѕРґРєР»СЋС‡РµРЅРѕ Рє РёРЅС‚РµСЂРЅРµС‚Сѓ.";
        }

        if (result.IsUpdateAvailable)
        {
            var prefix =
                $"Mimiron's Gold-o-Matic: Р”РѕСЃС‚СѓРїРЅР° РЅРѕРІР°СЏ РІРµСЂСЃРёСЏ v{result.LatestVersion} (Сѓ РІР°СЃ v{result.CurrentVersion}).";
            if (string.IsNullOrWhiteSpace(result.ReleaseNotesUrl))
                return prefix;

            var cappedUrl = result.ReleaseNotesUrl.Length > 80
                ? result.ReleaseNotesUrl[..77] + "..."
                : result.ReleaseNotesUrl;
            return $"{prefix} РџРѕРґСЂРѕР±РЅРµРµ: {cappedUrl}";
        }

        return $"Mimiron's Gold-o-Matic: Р’С‹ РёСЃРїРѕР»СЊР·СѓРµС‚Рµ Р°РєС‚СѓР°Р»СЊРЅСѓСЋ РІРµСЂСЃРёСЋ (v{result.CurrentVersion}).";
    }

    private bool MarkSeen(string key)
    {
        lock (_seen)
        {
            if (_seen.Contains(key))
                return false;
            _seen.Add(key);
            if (_seen.Count > SeenTrim)
                _seen.Clear();
            return true;
        }
    }

    private static string? ExtractWhoJson(string line)
    {
        var i = line.IndexOf("[MGM_WHO]", StringComparison.Ordinal);
        if (i < 0)
            return null;
        var start = line.IndexOf('{');
        var end = line.LastIndexOf('}');
        if (start < 0 || end <= start)
            return null;
        return line.Substring(start, end - start + 1);
    }

    [GeneratedRegex(@"\[MGM_ACCEPT:([0-9a-fA-F-]{36})\]")]
    private static partial Regex AcceptRegex();

    [GeneratedRegex(@"\[MGM_CONFIRM:([0-9a-fA-F-]{36})\]")]
    private static partial Regex ConfirmRegex();

    [GeneratedRegex(@"\[MGM_GIFT_ACCEPT:([0-9a-fA-F-]{36})\]")]
    private static partial Regex GiftAcceptRegex();

    [GeneratedRegex(@"\[MGM_ITEMS:([0-9a-fA-F-]{36})\](\[.*\])")]
    private static partial Regex GiftItemsRegex();
}

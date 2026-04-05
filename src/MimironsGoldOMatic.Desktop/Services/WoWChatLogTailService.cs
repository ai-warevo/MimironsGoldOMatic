using System.IO;
using System.Text;
using System.Threading;
using System.Text.Json;
using System.Text.RegularExpressions;
using MimironsGoldOMatic.Desktop.Api;
using MimironsGoldOMatic.Shared;

namespace MimironsGoldOMatic.Desktop.Services;

/// <summary>Single tail of <c>WoWChatLog.txt</c> for <c>[MGM_WHO]</c>, <c>[MGM_ACCEPT]</c>, <c>[MGM_CONFIRM]</c> (<c>docs/SPEC.md</c> §10).</summary>
public sealed partial class WoWChatLogTailService : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly DesktopConnectionContext _connection;
    private readonly EbsDesktopClient _api;
    private readonly PayoutSnapshotCache _payouts;
    private readonly Action<string> _deliveryLog;
    private readonly System.Timers.Timer _timer = new(750) { AutoReset = true };
    private readonly object _ioLock = new();
    private long _position;
    private string _lineCarry = "";
    private readonly HashSet<string> _seen = new(StringComparer.Ordinal);
    private const int SeenTrim = 1500;
    private int _tickGate;

    public WoWChatLogTailService(
        DesktopConnectionContext connection,
        EbsDesktopClient api,
        PayoutSnapshotCache payouts,
        Action<string> deliveryLog)
    {
        _connection = connection;
        _api = api;
        _payouts = payouts;
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

    private async Task ProcessLineAsync(string line, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(line))
            return;

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

            VerifyCandidateRequestDto? dto;
            try
            {
                dto = JsonSerializer.Deserialize<VerifyCandidateRequestDto>(json, JsonOptions);
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

        var accept = AcceptRegex().Match(line);
        if (accept.Success && Guid.TryParse(accept.Groups[1].Value, out var acceptId))
        {
            if (!MarkSeen("accept:" + acceptId))
                return;
            if (!_payouts.TryGetCharacterName(acceptId, out var characterName))
            {
                _deliveryLog($"[MGM_ACCEPT] unknown payout {acceptId:D} — refresh pending list");
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
}

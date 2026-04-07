using System.Text;
using MimironsGoldOMatic.Mocks.WoWMock.Configuration;
using Microsoft.Extensions.Options;

namespace MimironsGoldOMatic.Mocks.WoWMock;

public sealed class ChatLogSimulator
{
    private readonly MockSettings _settings;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly string _resolvedLogPath;

    public ChatLogSimulator(IOptions<MockSettings> settings)
    {
        _settings = settings.Value;
        _resolvedLogPath = Path.GetFullPath(_settings.LogFilePath);
    }

    public string LogFilePath => _resolvedLogPath;

    public async Task EnsureCreatedAsync(CancellationToken ct)
    {
        var dir = Path.GetDirectoryName(_resolvedLogPath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await _writeLock.WaitAsync(ct);
        try
        {
            if (!File.Exists(_resolvedLogPath))
            {
                await File.WriteAllTextAsync(_resolvedLogPath, string.Empty, Encoding.UTF8, ct);
            }
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task ResetAsync(CancellationToken ct)
    {
        await EnsureCreatedAsync(ct);

        await _writeLock.WaitAsync(ct);
        try
        {
            using var fs = new FileStream(_resolvedLogPath, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task AppendAsync(string message, DateTimeOffset timestamp, CancellationToken ct)
    {
        await EnsureCreatedAsync(ct);

        // WoWChatLog is commonly tailed by external tools; we flush each line for determinism in tests.
        var line = $"{FormatTimestamp(timestamp)} {message}{Environment.NewLine}";

        await _writeLock.WaitAsync(ct);
        try
        {
            await using var stream = new FileStream(
                _resolvedLogPath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.ReadWrite,
                bufferSize: 4096,
                useAsync: true);

            await using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            await writer.WriteAsync(line.AsMemory(), ct);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private static string FormatTimestamp(DateTimeOffset ts)
    {
        // Keep it stable and easy to parse in Desktop tailers.
        // Example: 2026-04-06 18:40:12.345
        return ts.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff");
    }
}


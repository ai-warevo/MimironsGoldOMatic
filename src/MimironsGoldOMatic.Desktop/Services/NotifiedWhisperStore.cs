using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MimironsGoldOMatic.Desktop.Services;

/// <summary>Persists payout ids for which <c>NotifyWinnerWhisper</c> was already injected (avoid duplicate §9 whispers after restart).</summary>
public sealed class NotifiedWhisperStore
{
    private readonly string _dataDirectory;

    /// <param name="dataDirectory">Optional override for tests; defaults to <see cref="DesktopSettingsStore.DataDirectory"/>.</param>
    public NotifiedWhisperStore(string? dataDirectory = null)
    {
        _dataDirectory = dataDirectory ?? DesktopSettingsStore.DataDirectory;
    }

    private string StorePath => Path.Combine(_dataDirectory, "notified-whisper-ids.json");

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    public HashSet<Guid> Load()
    {
        try
        {
            if (!File.Exists(StorePath))
                return [];
            var json = File.ReadAllText(StorePath);
            var list = JsonSerializer.Deserialize<List<Guid>>(json);
            return list is null ? [] : [..list];
        }
        catch
        {
            return [];
        }
    }

    public void Save(HashSet<Guid> ids)
    {
        Directory.CreateDirectory(_dataDirectory);
        File.WriteAllText(StorePath, JsonSerializer.Serialize(ids.OrderBy(x => x).ToList(), JsonOpts));
    }
}

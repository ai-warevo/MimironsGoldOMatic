using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MimironsGoldOMatic.Desktop.Services;

/// <summary>Persists payout ids for which <c>NotifyWinnerWhisper</c> was already injected (avoid duplicate §9 whispers after restart).</summary>
public sealed class NotifiedWhisperStore
{
    private static string StorePath =>
        Path.Combine(DesktopSettingsStore.DataDirectory, "notified-whisper-ids.json");

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
        Directory.CreateDirectory(DesktopSettingsStore.DataDirectory);
        File.WriteAllText(StorePath, JsonSerializer.Serialize(ids.OrderBy(x => x).ToList(), JsonOpts));
    }
}

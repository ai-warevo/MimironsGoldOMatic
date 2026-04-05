using MimironsGoldOMatic.Desktop.Services;
using Xunit;

namespace MimironsGoldOMatic.Desktop.UnitTests;

public sealed class NotifiedWhisperStoreTests
{
    [Fact]
    public void Load_missing_file_returns_empty_set()
    {
        var dir = Path.Combine(Path.GetTempPath(), "mgm-whisper-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var store = new NotifiedWhisperStore(dir);
            var set = store.Load();
            Assert.Empty(set);
        }
        finally
        {
            try
            {
                Directory.Delete(dir, recursive: true);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public void Save_and_Load_roundtrip()
    {
        var dir = Path.Combine(Path.GetTempPath(), "mgm-whisper-rt-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var a = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var b = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            var store = new NotifiedWhisperStore(dir);
            store.Save(new HashSet<Guid> { b, a });
            var loaded = store.Load();
            Assert.Equal(2, loaded.Count);
            Assert.Contains(a, loaded);
            Assert.Contains(b, loaded);
        }
        finally
        {
            try
            {
                Directory.Delete(dir, recursive: true);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public void Load_corrupt_json_returns_empty_set()
    {
        var dir = Path.Combine(Path.GetTempPath(), "mgm-whisper-bad-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "notified-whisper-ids.json"), "[ not valid");
        try
        {
            var store = new NotifiedWhisperStore(dir);
            Assert.Empty(store.Load());
        }
        finally
        {
            try
            {
                Directory.Delete(dir, recursive: true);
            }
            catch
            {
            }
        }
    }
}

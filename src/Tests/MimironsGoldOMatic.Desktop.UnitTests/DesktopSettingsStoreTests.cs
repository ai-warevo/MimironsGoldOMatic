using MimironsGoldOMatic.Desktop.Services;
using Xunit;

namespace MimironsGoldOMatic.Desktop.UnitTests;

public sealed class DesktopSettingsStoreTests
{
    [Fact]
    public void LoadSettings_missing_file_returns_defaults()
    {
        var dir = Path.Combine(Path.GetTempPath(), "mgm-settings-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var store = new DesktopSettingsStore(dir);
            var s = store.LoadSettings();
            Assert.Equal("https://localhost:5001", s.BaseUrl);
            Assert.Equal(15, s.PollIntervalSeconds);
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
    public void SaveSettings_roundtrip_preserves_values()
    {
        var dir = Path.Combine(Path.GetTempPath(), "mgm-settings-rt-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var store = new DesktopSettingsStore(dir);
            var original = new DesktopUserSettings
            {
                BaseUrl = "https://unit.test/",
                PollIntervalSeconds = 42,
                HttpRetryCount = 7,
                InjectionStrategy = "SendInput",
                WoWInstallDirectory = @"C:\WoW",
                WoWChatLogPathOverride = @"D:\Logs",
            };
            store.SaveSettings(original);
            var loaded = store.LoadSettings();
            Assert.Equal(original.BaseUrl, loaded.BaseUrl);
            Assert.Equal(original.PollIntervalSeconds, loaded.PollIntervalSeconds);
            Assert.Equal(original.HttpRetryCount, loaded.HttpRetryCount);
            Assert.Equal(original.InjectionStrategy, loaded.InjectionStrategy);
            Assert.Equal(original.WoWInstallDirectory, loaded.WoWInstallDirectory);
            Assert.Equal(original.WoWChatLogPathOverride, loaded.WoWChatLogPathOverride);
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
    public void LoadSettings_corrupt_json_falls_back_to_defaults()
    {
        var dir = Path.Combine(Path.GetTempPath(), "mgm-settings-bad-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "settings.json");
        File.WriteAllText(path, "{ not json");
        try
        {
            var store = new DesktopSettingsStore(dir);
            var s = store.LoadSettings();
            Assert.Equal("https://localhost:5001", s.BaseUrl);
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

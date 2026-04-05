using MimironsGoldOMatic.Desktop.Services;
using Xunit;

namespace MimironsGoldOMatic.Desktop.UnitTests;

public sealed class WoWChatLogPathResolverTests
{
    [Fact]
    public void Resolve_returns_null_when_no_install_dir_and_no_override()
    {
        var s = new DesktopUserSettings { WoWInstallDirectory = null, WoWChatLogPathOverride = null };
        Assert.Null(WoWChatLogPathResolver.Resolve(s));
    }

    [Fact]
    public void Resolve_combines_install_dir_with_Logs_WoWChatLog_txt()
    {
        var root = Path.Combine(Path.GetTempPath(), "mgm-wow-root-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "Logs"));
        try
        {
            var s = new DesktopUserSettings { WoWInstallDirectory = root };
            var path = WoWChatLogPathResolver.Resolve(s);
            Assert.Equal(Path.Combine(root, "Logs", "WoWChatLog.txt"), path);
        }
        finally
        {
            try
            {
                Directory.Delete(root, recursive: true);
            }
            catch
            {
                // best-effort cleanup
            }
        }
    }

    [Fact]
    public void Resolve_override_directory_appends_WoWChatLog_txt()
    {
        var dir = Path.Combine(Path.GetTempPath(), "mgm-logdir-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var s = new DesktopUserSettings { WoWChatLogPathOverride = dir };
            var path = WoWChatLogPathResolver.Resolve(s);
            Assert.Equal(Path.Combine(dir, "WoWChatLog.txt"), path);
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
    public void Resolve_override_existing_file_returns_full_path()
    {
        var file = Path.Combine(Path.GetTempPath(), "mgm-chatlog-" + Guid.NewGuid().ToString("N") + ".txt");
        File.WriteAllText(file, "");
        try
        {
            var s = new DesktopUserSettings { WoWChatLogPathOverride = file };
            var path = WoWChatLogPathResolver.Resolve(s);
            Assert.Equal(Path.GetFullPath(file), path);
        }
        finally
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public void Resolve_trims_override_whitespace()
    {
        var dir = Path.Combine(Path.GetTempPath(), "mgm-trim-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var s = new DesktopUserSettings { WoWChatLogPathOverride = "  " + dir + "  " };
            var path = WoWChatLogPathResolver.Resolve(s);
            Assert.Equal(Path.Combine(dir, "WoWChatLog.txt"), path);
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

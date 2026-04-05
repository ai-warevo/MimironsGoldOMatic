using System.IO;

namespace MimironsGoldOMatic.Desktop.Services;

public static class WoWChatLogPathResolver
{
    /// <summary>Resolves absolute path to <c>WoWChatLog.txt</c> per <c>docs/overview/SPEC.md</c> §10.</summary>
    public static string? Resolve(DesktopUserSettings s)
    {
        var o = s.WoWChatLogPathOverride?.Trim();
        if (!string.IsNullOrEmpty(o))
        {
            if (Directory.Exists(o))
                return Path.Combine(o, "WoWChatLog.txt");
            if (File.Exists(o))
                return Path.GetFullPath(o);
        }

        var root = s.WoWInstallDirectory?.Trim();
        if (string.IsNullOrEmpty(root))
            return null;
        return Path.Combine(root, "Logs", "WoWChatLog.txt");
    }
}

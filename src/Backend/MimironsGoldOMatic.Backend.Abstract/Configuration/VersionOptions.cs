namespace MimironsGoldOMatic.Backend.Configuration;

public sealed class VersionOptions
{
    public const string SectionName = "Version";

    /// <summary>
    /// Public app version returned by GET /api/version.
    /// When empty, backend falls back to assembly informational/version metadata.
    /// </summary>
    public string CurrentVersion { get; set; } = "";

    /// <summary>Optional release notes or download page URL.</summary>
    public string? ReleaseNotesUrl { get; set; }

    /// <summary>Optional minimum compatible Desktop version.</summary>
    public string? MinimumDesktopVersion { get; set; }

    /// <summary>Optional minimum compatible WoW Addon version.</summary>
    public string? MinimumAddonVersion { get; set; }

    /// <summary>Optional minimum compatible Twitch Extension version.</summary>
    public string? MinimumExtensionVersion { get; set; }
}


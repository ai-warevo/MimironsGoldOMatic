namespace MimironsGoldOMatic.Shared.Versioning;

public sealed record VersionInfoDto(
    string Version,
    string? ReleaseNotesUrl,
    string? MinimumDesktopVersion,
    string? MinimumAddonVersion,
    string? MinimumExtensionVersion);

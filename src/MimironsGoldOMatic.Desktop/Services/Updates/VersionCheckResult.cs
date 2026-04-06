namespace MimironsGoldOMatic.Desktop.Services.Updates;

public sealed record VersionCheckResult(
    bool IsSuccess,
    bool IsUpdateAvailable,
    string CurrentVersion,
    string LatestVersion,
    string? ReleaseNotesUrl,
    bool IsDesktopCompatible,
    string StatusMessage)
{
    public static VersionCheckResult Failure(string currentVersion) =>
        new(
            IsSuccess: false,
            IsUpdateAvailable: false,
            CurrentVersion: currentVersion,
            LatestVersion: currentVersion,
            ReleaseNotesUrl: null,
            IsDesktopCompatible: true,
            StatusMessage: "Не удалось проверить обновления.");
}

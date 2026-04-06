namespace MimironsGoldOMatic.Desktop.Services.Updates;

public interface IUpdateService
{
    Task<VersionCheckResult> CheckForUpdatesAsync(CancellationToken ct = default);
}

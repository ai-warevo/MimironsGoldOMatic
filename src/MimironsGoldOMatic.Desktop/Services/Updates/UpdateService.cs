
namespace MimironsGoldOMatic.Desktop.Services.Updates;

public sealed class UpdateService(IEbsDesktopClient api, IAppVersionProvider versionProvider) : IUpdateService
{
    public async Task<VersionCheckResult> CheckForUpdatesAsync(CancellationToken ct = default)
    {
        var currentVersion = versionProvider.GetCurrentVersion();
        try
        {
            var remote = await api.GetVersionInfoAsync(ct).ConfigureAwait(false);
            return BuildResult(currentVersion, remote);
        }
        catch
        {
            return VersionCheckResult.Failure(currentVersion);
        }
    }

    private static VersionCheckResult BuildResult(string currentVersion, VersionInfoDto remote)
    {
        var latestVersion = remote.Version.Trim();
        var compare = CompareSemanticVersion(latestVersion, currentVersion);
        var isUpdateAvailable = compare > 0;
        var isDesktopCompatible = true;
        if (!string.IsNullOrWhiteSpace(remote.MinimumDesktopVersion))
        {
            isDesktopCompatible = CompareSemanticVersion(currentVersion, remote.MinimumDesktopVersion) >= 0;
        }

        var status = isUpdateAvailable
            ? $"Р”РѕСЃС‚СѓРїРЅР° РЅРѕРІР°СЏ РІРµСЂСЃРёСЏ: v{latestVersion} (Сѓ РІР°СЃ v{currentVersion})."
            : $"Р’РµСЂСЃРёСЏ Р°РєС‚СѓР°Р»СЊРЅР°: v{currentVersion}.";

        if (!isDesktopCompatible)
        {
            status = $"РўРµРєСѓС‰Р°СЏ РІРµСЂСЃРёСЏ v{currentVersion} РЅРёР¶Рµ РјРёРЅРёРјР°Р»СЊРЅРѕР№ РїРѕРґРґРµСЂР¶РёРІР°РµРјРѕР№.";
        }

        return new VersionCheckResult(
            IsSuccess: true,
            IsUpdateAvailable: isUpdateAvailable,
            CurrentVersion: currentVersion,
            LatestVersion: latestVersion,
            ReleaseNotesUrl: remote.ReleaseNotesUrl,
            IsDesktopCompatible: isDesktopCompatible,
            StatusMessage: status);
    }

    internal static int CompareSemanticVersion(string left, string right)
    {
        if (TryParseSemanticVersion(left, out var leftParts) && TryParseSemanticVersion(right, out var rightParts))
        {
            for (var i = 0; i < 3; i++)
            {
                var cmp = leftParts[i].CompareTo(rightParts[i]);
                if (cmp != 0)
                {
                    return cmp;
                }
            }

            return 0;
        }

        return string.Compare(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseSemanticVersion(string version, out int[] parts)
    {
        parts = [0, 0, 0];
        if (string.IsNullOrWhiteSpace(version))
        {
            return false;
        }

        var normalized = version.Trim();
        if (normalized.StartsWith('v') || normalized.StartsWith('V'))
        {
            normalized = normalized[1..];
        }

        var dash = normalized.IndexOf('-');
        if (dash >= 0)
        {
            normalized = normalized[..dash];
        }

        var plus = normalized.IndexOf('+');
        if (plus >= 0)
        {
            normalized = normalized[..plus];
        }

        var split = normalized.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length is < 1 or > 3)
        {
            return false;
        }

        for (var i = 0; i < split.Length; i++)
        {
            if (!int.TryParse(split[i], out parts[i]))
            {
                return false;
            }
        }

        return true;
    }
}

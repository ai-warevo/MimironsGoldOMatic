namespace MimironsGoldOMatic.Desktop.Services;

public sealed class DesktopUserSettings
{
    public string BaseUrl { get; set; } = "https://localhost:5001";

    /// <summary>Polling interval for <c>GET /api/payouts/pending</c> (<c>docs/components/desktop/UI_SPEC.md</c> UI-306 default 15s).</summary>
    public int PollIntervalSeconds { get; set; } = 15;

    public int HttpRetryCount { get; set; } = 3;

    /// <summary><c>PostMessage</c> or <c>SendInput</c> (<c>docs/overview/SPEC.md</c> §8).</summary>
    public string InjectionStrategy { get; set; } = "PostMessage";

    /// <summary>WoW root folder (contains <c>Logs\WoWChatLog.txt</c>).</summary>
    public string? WoWInstallDirectory { get; set; }

    /// <summary>Full path to <c>WoWChatLog.txt</c> or a directory containing it.</summary>
    public string? WoWChatLogPathOverride { get; set; }
}

namespace MimironsGoldOMatic.Desktop.Services;

/// <summary>Live operator connection (mutable when Settings UI saves).</summary>
public sealed class DesktopConnectionContext
{
    public DesktopUserSettings Settings { get; set; } = new();

    public string? ApiKey { get; set; }

    public (string BaseUrl, string ApiKey) GetConnection() => (Settings.BaseUrl, ApiKey ?? "");

    /// <summary>UI-306 / SPEC: poll interval clamped to [5, 600] seconds.</summary>
    public int GetClampedPollIntervalSeconds() => Math.Clamp(Settings.PollIntervalSeconds, 5, 600);
}

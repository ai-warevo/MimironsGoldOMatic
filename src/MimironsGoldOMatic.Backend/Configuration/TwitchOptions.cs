namespace MimironsGoldOMatic.Backend.Configuration;

public sealed class TwitchOptions
{
    public const string SectionName = "Twitch";

    /// <summary>Extension client id (JWT aud).</summary>
    public string ExtensionClientId { get; set; } = "";

    /// <summary>Extension secret (base64) for HS256 JWT validation.</summary>
    public string ExtensionSecret { get; set; } = "";

    /// <summary>EventSub webhook secret (for HMAC verification).</summary>
    public string EventSubSecret { get; set; } = "";

    public string HelixClientId { get; set; } = "";
    public string HelixClientSecret { get; set; } = "";

    /// <summary>Broadcaster OAuth token with channel:bot or moderator scope for Send Chat Message.</summary>
    public string BroadcasterAccessToken { get; set; } = "";

    public string BroadcasterUserId { get; set; } = "";
}

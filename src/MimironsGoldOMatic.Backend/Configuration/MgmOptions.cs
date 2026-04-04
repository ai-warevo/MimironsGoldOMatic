namespace MimironsGoldOMatic.Backend.Configuration;

public sealed class MgmOptions
{
    public const string SectionName = "Mgm";

    /// <summary>Pre-shared Desktop key (header X-MGM-ApiKey).</summary>
    public string ApiKey { get; set; } = "";

    /// <summary>When true, Helix subscriber check for POST /api/payouts/claim is skipped (local dev only).</summary>
    public bool DevSkipSubscriberCheck { get; set; }
}

namespace MimironsGoldOMatic.Backend.Services;

/// <summary>Abstraction for EventSub chat ingestion (mockable in controller tests).</summary>
public interface IChatEnrollmentIngest
{
    Task IngestAsync(
        string messageId,
        string twitchUserId,
        string twitchDisplayName,
        string chatText,
        bool isSubscriber,
        CancellationToken ct);
}

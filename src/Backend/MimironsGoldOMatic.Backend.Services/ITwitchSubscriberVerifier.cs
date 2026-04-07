namespace MimironsGoldOMatic.Backend.Services;

public interface ITwitchSubscriberVerifier
{
    Task<bool> IsSubscriberAsync(string streamerId, string viewerId, CancellationToken ct);
}


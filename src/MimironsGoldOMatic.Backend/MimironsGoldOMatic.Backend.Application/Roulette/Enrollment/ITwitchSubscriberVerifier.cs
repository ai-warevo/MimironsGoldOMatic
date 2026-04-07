namespace MimironsGoldOMatic.Backend.Application.Roulette.Enrollment;

public interface ITwitchSubscriberVerifier
{
    Task<bool> IsSubscriberAsync(string streamerId, string viewerId, CancellationToken ct);
}


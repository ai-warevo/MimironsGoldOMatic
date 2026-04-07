namespace MimironsGoldOMatic.Backend.Infrastructure.Persistence;

public record GiftRequestStateChanged(
    GiftRequestState From,
    GiftRequestState To,
    DateTime Utc,
    string? Reason = null);


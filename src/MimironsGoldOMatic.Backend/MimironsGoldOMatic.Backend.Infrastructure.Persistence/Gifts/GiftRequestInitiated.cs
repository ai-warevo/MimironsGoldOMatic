namespace MimironsGoldOMatic.Backend.Infrastructure.Persistence;

public record GiftRequestInitiated(
    Guid Id,
    string StreamerId,
    string ViewerId,
    string ViewerDisplayName,
    string CharacterName,
    DateTime CreatedAtUtc);


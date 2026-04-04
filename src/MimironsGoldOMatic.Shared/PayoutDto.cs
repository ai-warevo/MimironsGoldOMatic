namespace MimironsGoldOMatic.Shared;

public sealed record PayoutDto(
    Guid Id,
    string TwitchUserId,
    string TwitchDisplayName,
    string CharacterName,
    long GoldAmount,
    string EnrollmentRequestId,
    PayoutStatus Status,
    DateTime CreatedAt);

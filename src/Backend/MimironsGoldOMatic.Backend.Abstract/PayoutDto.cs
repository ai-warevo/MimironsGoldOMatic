namespace MimironsGoldOMatic.Backend.Abstract;

public sealed record PayoutDto(
    Guid Id,
    string TwitchUserId,
    string TwitchDisplayName,
    string CharacterName,
    long GoldAmount,
    string EnrollmentRequestId,
    PayoutStatus Status,
    DateTime CreatedAt,
    bool IsRewardSentAnnouncedToChat = false);

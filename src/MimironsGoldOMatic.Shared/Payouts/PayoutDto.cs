namespace MimironsGoldOMatic.Shared.Payouts;

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

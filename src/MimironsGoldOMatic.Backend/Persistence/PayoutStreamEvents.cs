namespace MimironsGoldOMatic.Backend.Persistence;

public record PayoutCreated(
    Guid Id,
    string TwitchUserId,
    string TwitchDisplayName,
    string CharacterName,
    long GoldAmount,
    string EnrollmentRequestId,
    MimironsGoldOMatic.Shared.PayoutStatus Status,
    DateTime CreatedAtUtc,
    Guid SpinCycleId);

public record PayoutStatusChanged(
    MimironsGoldOMatic.Shared.PayoutStatus From,
    MimironsGoldOMatic.Shared.PayoutStatus To,
    DateTime Utc);

public record WinnerAcceptanceRecorded(DateTime Utc);

public record HelixRewardSentAnnouncementSucceeded(DateTime Utc);

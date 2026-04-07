using MimironsGoldOMatic.Shared;

namespace MimironsGoldOMatic.Backend.Persistence;

public record PayoutCreated(
    Guid Id,
    string TwitchUserId,
    string TwitchDisplayName,
    string CharacterName,
    long GoldAmount,
    string EnrollmentRequestId,
    PayoutStatus Status,
    DateTime CreatedAtUtc,
    Guid SpinCycleId);

public record PayoutStatusChanged(
    PayoutStatus From,
    PayoutStatus To,
    DateTime Utc);

public record WinnerAcceptanceRecorded(DateTime Utc);

public record HelixRewardSentAnnouncementSucceeded(DateTime Utc);

public record GiftRequestInitiated(
    Guid Id,
    string StreamerId,
    string ViewerId,
    string ViewerDisplayName,
    string CharacterName,
    DateTime CreatedAtUtc);

public record GiftRequestStateChanged(
    GiftRequestState From,
    GiftRequestState To,
    DateTime Utc,
    string? Reason = null);

public record GiftItemSelected(
    string Name,
    int Id,
    int Count,
    string Link,
    string Texture,
    int BagId,
    int SlotId,
    DateTime Utc);


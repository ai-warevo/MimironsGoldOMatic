using MimironsGoldOMatic.Shared.Payouts;

namespace MimironsGoldOMatic.Backend.Infrastructure.Persistence;

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


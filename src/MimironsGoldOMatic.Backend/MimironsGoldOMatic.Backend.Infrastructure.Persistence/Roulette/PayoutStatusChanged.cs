using MimironsGoldOMatic.Shared.Payouts;

namespace MimironsGoldOMatic.Backend.Infrastructure.Persistence;

public record PayoutStatusChanged(
    PayoutStatus From,
    PayoutStatus To,
    DateTime Utc);


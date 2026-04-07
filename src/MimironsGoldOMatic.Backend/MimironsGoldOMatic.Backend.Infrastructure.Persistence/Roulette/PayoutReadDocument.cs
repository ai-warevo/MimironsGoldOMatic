using MimironsGoldOMatic.Shared.Payouts;

namespace MimironsGoldOMatic.Backend.Infrastructure.Persistence;

public sealed class PayoutReadDocument
{
    public Guid Id { get; set; }
    public string TwitchUserId { get; set; } = "";
    public string TwitchDisplayName { get; set; } = "";
    public string CharacterName { get; set; } = "";
    public long GoldAmount { get; set; }
    public string EnrollmentRequestId { get; set; } = "";
    public PayoutStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? WinnerAcceptedWillingToReceiveAt { get; set; }
    public bool IsRewardSentAnnouncedToChat { get; set; }
    public Guid? SpinCycleId { get; set; }
}


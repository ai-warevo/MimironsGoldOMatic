
namespace MimironsGoldOMatic.Backend.Persistence;

public sealed class PoolMemberEntry
{
    public string TwitchUserId { get; set; } = "";
    public string TwitchDisplayName { get; set; } = "";
    public string CharacterName { get; set; } = "";
}

public sealed class PoolDocument
{
    public Guid Id { get; set; } = EbsIds.PoolDocumentId;
    public List<PoolMemberEntry> Members { get; set; } = [];
}

/// <summary>Authoritative roulette cycle state for single-channel MVP.</summary>
public sealed class SpinStateDocument
{
    public Guid Id { get; set; } = EbsIds.SpinStateDocumentId;

    /// <summary>UTC start of the current 5-minute cycle (floor).</summary>
    public DateTime CycleStartUtc { get; set; }

    public Guid SpinCycleId { get; set; }

    public string? CandidateTwitchUserId { get; set; }
    public string? CandidateCharacterName { get; set; }
    public string? CandidateTwitchDisplayName { get; set; }
    public DateTime? CandidateSelectedAtUtc { get; set; }

    /// <summary>Inclusive deadline for verify-candidate (cycle start + 5 min + 30s grace).</summary>
    public DateTime VerificationDeadlineUtc { get; set; }

    public bool PayoutCreatedForCycle { get; set; }
    public bool PoolWasEmptyAtCycleStart { get; set; }
}

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

public sealed class ChatMessageDedupDocument
{
    public string Id { get; set; } = "";
    public DateTime ProcessedAtUtc { get; set; }
}

public sealed class EnrollmentIdempotencyDocument
{
    public string Id { get; set; } = "";
    public string TwitchUserId { get; set; } = "";
    public string TwitchDisplayName { get; set; } = "";
    public string CharacterName { get; set; } = "";
    public DateTime EnrolledAtUtc { get; set; }
}

public sealed class GiftSelectedItemDocument
{
    public string Name { get; set; } = "";
    public int Id { get; set; }
    public int Count { get; set; }
    public string Link { get; set; } = "";
    public string Texture { get; set; } = "";
    public int BagId { get; set; }
    public int SlotId { get; set; }
}

public sealed class GiftRequestReadDocument
{
    public Guid Id { get; set; }
    public string StreamerId { get; set; } = "";
    public string ViewerId { get; set; } = "";
    public string ViewerDisplayName { get; set; } = "";
    public string CharacterName { get; set; } = "";
    public GiftRequestState State { get; set; } = GiftRequestState.Pending;
    public GiftSelectedItemDocument? SelectedItem { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? TimeoutAt { get; set; }
    public string? FailureReason { get; set; }
}

public sealed class GiftCommandUsageDocument
{
    public string Id { get; set; } = "";
    public string StreamerId { get; set; } = "";
    public string ViewerId { get; set; } = "";
    public DateTime CompletedAtUtc { get; set; }
    public Guid GiftRequestId { get; set; }
}


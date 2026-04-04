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
    public MimironsGoldOMatic.Shared.PayoutStatus Status { get; set; }
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

namespace MimironsGoldOMatic.Backend.Infrastructure.Persistence;

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


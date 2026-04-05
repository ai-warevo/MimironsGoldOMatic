using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Services;
using Xunit;

namespace MimironsGoldOMatic.Backend.Tests.Unit;

[Trait("Category", "Unit")]
public sealed class SpinPhaseResolverTests
{
    private static readonly DateTime T0 = new(2026, 4, 5, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime TSelect = T0.AddSeconds(RouletteTime.CollectingSeconds);
    private static readonly DateTime TVerifyStart = TSelect.AddSeconds(RouletteTime.SpinningSeconds);
    private static readonly DateTime TCycleEnd = T0.AddMinutes(5);
    private static readonly DateTime TVerifyDeadline = T0.AddMinutes(5).AddSeconds(30);

    private static PoolDocument PoolNonEmpty() => new()
    {
        Id = EbsIds.PoolDocumentId,
        Members = [new PoolMemberEntry { TwitchUserId = "u1", CharacterName = "Abcd" }],
    };

    private static SpinStateDocument Spin(bool poolWasEmptyAtStart, DateTime? candidateSelectedAtUtc) => new()
    {
        Id = EbsIds.SpinStateDocumentId,
        CycleStartUtc = T0,
        SpinCycleId = Guid.NewGuid(),
        PoolWasEmptyAtCycleStart = poolWasEmptyAtStart,
        CandidateSelectedAtUtc = candidateSelectedAtUtc,
        VerificationDeadlineUtc = TVerifyDeadline,
    };

    [Fact]
    public void Resolve_idle_when_pool_was_empty_at_cycle_start()
    {
        var phase = SpinPhaseResolver.Resolve(TSelect.AddMinutes(1), PoolNonEmpty(), Spin(poolWasEmptyAtStart: true, TSelect));
        Assert.Equal("idle", phase);
    }

    [Fact]
    public void Resolve_idle_when_pool_has_no_members()
    {
        var pool = new PoolDocument { Id = EbsIds.PoolDocumentId, Members = [] };
        var phase = SpinPhaseResolver.Resolve(TSelect.AddMinutes(1), pool, Spin(poolWasEmptyAtStart: false, TSelect));
        Assert.Equal("idle", phase);
    }

    [Fact]
    public void Resolve_collecting_before_selection_window()
    {
        var phase = SpinPhaseResolver.Resolve(TSelect.AddSeconds(-1), PoolNonEmpty(), Spin(false, null));
        Assert.Equal("collecting", phase);
    }

    [Fact]
    public void Resolve_completed_when_past_selection_window_but_no_candidate_yet()
    {
        var phase = SpinPhaseResolver.Resolve(TSelect.AddSeconds(10), PoolNonEmpty(), Spin(false, null));
        Assert.Equal("completed", phase);
    }

    [Fact]
    public void Resolve_spinning_after_candidate_until_verify_start()
    {
        var picked = TSelect.AddSeconds(5);
        var phase = SpinPhaseResolver.Resolve(TVerifyStart.AddSeconds(-1), PoolNonEmpty(), Spin(false, picked));
        Assert.Equal("spinning", phase);
    }

    [Fact]
    public void Resolve_verification_from_verify_start_until_deadline()
    {
        var picked = TSelect.AddSeconds(5);
        Assert.Equal("verification",
            SpinPhaseResolver.Resolve(TVerifyStart.AddSeconds(1), PoolNonEmpty(), Spin(false, picked)));
        Assert.Equal("verification",
            SpinPhaseResolver.Resolve(TCycleEnd.AddSeconds(1), PoolNonEmpty(), Spin(false, picked)));
    }

    [Fact]
    public void Resolve_completed_after_verification_deadline()
    {
        var picked = TSelect.AddSeconds(5);
        var phase = SpinPhaseResolver.Resolve(TVerifyDeadline.AddSeconds(1), PoolNonEmpty(), Spin(false, picked));
        Assert.Equal("completed", phase);
    }
}

using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Services;
using Xunit;

namespace MimironsGoldOMatic.Backend.UnitTests.Unit;

/// <summary>Spin phase FSM from pool + spin documents and wall clock (UTC).</summary>
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
    public void Should_ReturnIdle_WhenPoolWasEmptyAtCycleStart()
    {
        var phase = SpinPhaseResolver.Resolve(TSelect.AddMinutes(1), PoolNonEmpty(), Spin(poolWasEmptyAtStart: true, TSelect));
        Assert.Equal("idle", phase);
    }

    [Fact]
    public void Should_ReturnIdle_WhenPoolHasNoMembers()
    {
        var pool = new PoolDocument { Id = EbsIds.PoolDocumentId, Members = [] };
        var phase = SpinPhaseResolver.Resolve(TSelect.AddMinutes(1), pool, Spin(poolWasEmptyAtStart: false, TSelect));
        Assert.Equal("idle", phase);
    }

    [Fact]
    public void Should_ReturnCollecting_WhenBeforeSelectionInstant()
    {
        var phase = SpinPhaseResolver.Resolve(TSelect.AddSeconds(-1), PoolNonEmpty(), Spin(false, null));
        Assert.Equal("collecting", phase);
    }

    /// <summary>Boundary: at <c>tSelect</c> exactly, collecting window has closed.</summary>
    [Fact]
    public void Should_ReturnCompleted_WhenAtSelectionInstantAndNoCandidateYet()
    {
        var phase = SpinPhaseResolver.Resolve(TSelect, PoolNonEmpty(), Spin(false, null));
        Assert.Equal("completed", phase);
    }

    [Fact]
    public void Should_ReturnCompleted_WhenPastSelectionWindowButNoCandidateYet()
    {
        var phase = SpinPhaseResolver.Resolve(TSelect.AddSeconds(10), PoolNonEmpty(), Spin(false, null));
        Assert.Equal("completed", phase);
    }

    [Fact]
    public void Should_ReturnSpinning_AfterCandidateUntilVerifyStart()
    {
        var picked = TSelect.AddSeconds(5);
        var phase = SpinPhaseResolver.Resolve(TVerifyStart.AddSeconds(-1), PoolNonEmpty(), Spin(false, picked));
        Assert.Equal("spinning", phase);
    }

    /// <summary>Boundary: at verify start instant, phase becomes verification (not spinning).</summary>
    [Fact]
    public void Should_ReturnVerification_WhenExactlyAtVerifyStart()
    {
        var picked = TSelect.AddSeconds(5);
        Assert.Equal("verification", SpinPhaseResolver.Resolve(TVerifyStart, PoolNonEmpty(), Spin(false, picked)));
    }

    [Fact]
    public void Should_ReturnVerification_FromVerifyStartUntilDeadline()
    {
        var picked = TSelect.AddSeconds(5);
        Assert.Equal("verification",
            SpinPhaseResolver.Resolve(TVerifyStart.AddSeconds(1), PoolNonEmpty(), Spin(false, picked)));
        Assert.Equal("verification",
            SpinPhaseResolver.Resolve(TCycleEnd.AddSeconds(1), PoolNonEmpty(), Spin(false, picked)));
    }

    /// <summary>Boundary: after cycle end but before verification grace deadline, still verification.</summary>
    [Fact]
    public void Should_ReturnVerification_WhenAfterCycleEndButBeforeVerificationDeadline()
    {
        var picked = TSelect.AddSeconds(5);
        var phase = SpinPhaseResolver.Resolve(TCycleEnd, PoolNonEmpty(), Spin(false, picked));
        Assert.Equal("verification", phase);
    }

    [Fact]
    public void Should_ReturnCompleted_AfterVerificationDeadline()
    {
        var picked = TSelect.AddSeconds(5);
        var phase = SpinPhaseResolver.Resolve(TVerifyDeadline.AddSeconds(1), PoolNonEmpty(), Spin(false, picked));
        Assert.Equal("completed", phase);
    }

    /// <summary>Non-UTC input is normalized to UTC kind for comparisons.</summary>
    [Fact]
    public void Should_NormalizeUnspecifiedKind_ToUtcForComparison()
    {
        var picked = TSelect.AddSeconds(5);
        var localLike = DateTime.SpecifyKind(TVerifyStart.AddSeconds(1), DateTimeKind.Unspecified);
        Assert.Equal("verification", SpinPhaseResolver.Resolve(localLike, PoolNonEmpty(), Spin(false, picked)));
    }
}

using MimironsGoldOMatic.Backend.Services;
using Xunit;

namespace MimironsGoldOMatic.Backend.UnitTests.Unit;

/// <summary>Five-minute roulette grid anchored to Unix epoch (UTC).</summary>
[Trait("Category", "Unit")]
public sealed class RouletteTimeTests
{
    private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Should_BeIdempotent_WhenFlooringToFiveMinuteUtc()
    {
        var t = new DateTime(2026, 4, 5, 14, 37, 22, DateTimeKind.Utc);
        var f = RouletteTime.FloorToFiveMinuteUtc(t);
        Assert.Equal(f, RouletteTime.FloorToFiveMinuteUtc(f));
    }

    [Fact]
    public void Should_AlignToEpochBasedFiveMinuteGrid_WhenFlooring()
    {
        var onGrid = Epoch.AddMinutes(500);
        Assert.Equal(onGrid, RouletteTime.FloorToFiveMinuteUtc(onGrid));
        var inside = onGrid.AddMinutes(3).AddSeconds(17);
        Assert.Equal(onGrid, RouletteTime.FloorToFiveMinuteUtc(inside));
    }

    [Fact]
    public void Should_ReturnStrictlyAfterInput_WhenNextSpinBoundaryUtc()
    {
        var t = new DateTime(2026, 4, 5, 14, 37, 22, DateTimeKind.Utc);
        var next = RouletteTime.NextSpinBoundaryUtc(t);
        Assert.True(next > t);
        var deltaMin = (long)(next - Epoch).TotalMinutes;
        Assert.Equal(0, deltaMin % 5);
    }

    [Fact]
    public void Should_ReturnNextBlock_WhenInputExactlyOnBoundary()
    {
        var onGrid = Epoch.AddMinutes(1000);
        var next = RouletteTime.NextSpinBoundaryUtc(onGrid);
        Assert.Equal(onGrid.AddMinutes(5), next);
    }

    /// <summary>Constants used by spin phase and synchronizer must stay in sync with product spec (4m collect, 30s spin).</summary>
    [Fact]
    public void Should_MatchSpecDurations_ForCollectingAndSpinningSeconds()
    {
        Assert.Equal(4 * 60, RouletteTime.CollectingSeconds);
        Assert.Equal(30, RouletteTime.SpinningSeconds);
    }
}

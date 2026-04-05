using MimironsGoldOMatic.Backend.Services;
using Xunit;

namespace MimironsGoldOMatic.Backend.Tests.Unit;

[Trait("Category", "Unit")]
public sealed class RouletteTimeTests
{
    private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void FloorToFiveMinuteUtc_idempotent()
    {
        var t = new DateTime(2026, 4, 5, 14, 37, 22, DateTimeKind.Utc);
        var f = RouletteTime.FloorToFiveMinuteUtc(t);
        Assert.Equal(f, RouletteTime.FloorToFiveMinuteUtc(f));
    }

    [Fact]
    public void FloorToFiveMinuteUtc_aligns_to_epoch_based_five_minute_grid()
    {
        var onGrid = Epoch.AddMinutes(500);
        Assert.Equal(onGrid, RouletteTime.FloorToFiveMinuteUtc(onGrid));
        var inside = onGrid.AddMinutes(3).AddSeconds(17);
        Assert.Equal(onGrid, RouletteTime.FloorToFiveMinuteUtc(inside));
    }

    [Fact]
    public void NextSpinBoundaryUtc_is_strictly_after_input_and_on_grid()
    {
        var t = new DateTime(2026, 4, 5, 14, 37, 22, DateTimeKind.Utc);
        var next = RouletteTime.NextSpinBoundaryUtc(t);
        Assert.True(next > t);
        var deltaMin = (long)(next - Epoch).TotalMinutes;
        Assert.Equal(0, deltaMin % 5);
    }

    [Fact]
    public void NextSpinBoundaryUtc_when_exactly_on_boundary_returns_next_block()
    {
        var onGrid = Epoch.AddMinutes(1000);
        var next = RouletteTime.NextSpinBoundaryUtc(onGrid);
        Assert.Equal(onGrid.AddMinutes(5), next);
    }
}

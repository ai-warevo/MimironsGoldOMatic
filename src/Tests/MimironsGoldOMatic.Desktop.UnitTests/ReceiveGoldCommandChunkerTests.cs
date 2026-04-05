using MimironsGoldOMatic.Desktop.Services;
using MimironsGoldOMatic.Desktop.UnitTests.TestSupport;
using MimironsGoldOMatic.Shared;
using Xunit;

namespace MimironsGoldOMatic.Desktop.UnitTests;

public sealed class ReceiveGoldCommandChunkerTests
{
    [Fact]
    public void GoldToCopper_multiplies_by_10_000()
    {
        Assert.Equal(50_000L, ReceiveGoldCommandChunker.GoldToCopper(5));
    }

    [Fact]
    public void BuildRunCommands_empty_enumerable_returns_empty()
    {
        var cmds = ReceiveGoldCommandChunker.BuildRunCommands(Array.Empty<PayoutDto>());
        Assert.Empty(cmds);
    }

    [Fact]
    public void BuildRunCommands_single_short_entry_one_line_under_255_chars()
    {
        var p = PayoutTestData.Pending(characterName: "A", goldAmount: 1);
        var cmds = ReceiveGoldCommandChunker.BuildRunCommands([p]);
        Assert.Single(cmds);
        Assert.True(cmds[0].Length <= 254, $"Line length {cmds[0].Length}");
        Assert.StartsWith("/run ReceiveGold(\"", cmds[0], StringComparison.Ordinal);
        Assert.EndsWith("\")", cmds[0], StringComparison.Ordinal);
        Assert.Contains(p.Id.ToString("D"), cmds[0], StringComparison.Ordinal);
        Assert.Contains(":A:10000;", cmds[0], StringComparison.Ordinal);
    }

    [Fact]
    public void BuildRunCommands_splits_when_chunk_would_exceed_limit()
    {
        var payouts = Enumerable.Range(0, 12)
            .Select(_ => PayoutTestData.Pending(characterName: "Longname", goldAmount: 999))
            .ToList();
        var cmds = ReceiveGoldCommandChunker.BuildRunCommands(payouts);
        Assert.True(cmds.Count >= 2);
        foreach (var c in cmds)
            Assert.True(c.Length <= 254, c);
    }

    [Fact]
    public void BuildRunCommands_throws_when_single_entry_exceeds_wow_line_limit()
    {
        var hugeName = new string('x', 400);
        var p = PayoutTestData.Pending(characterName: hugeName, goldAmount: 1);
        var ex = Assert.Throws<InvalidOperationException>(() => ReceiveGoldCommandChunker.BuildRunCommands([p]));
        Assert.Contains("line limit", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}

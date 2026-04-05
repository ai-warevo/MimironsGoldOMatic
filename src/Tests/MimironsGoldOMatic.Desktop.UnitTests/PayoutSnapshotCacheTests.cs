using MimironsGoldOMatic.Desktop.Services;
using MimironsGoldOMatic.Desktop.UnitTests.TestSupport;
using Xunit;

namespace MimironsGoldOMatic.Desktop.UnitTests;

public sealed class PayoutSnapshotCacheTests
{
    [Fact]
    public void TryGetCharacterName_miss_before_update()
    {
        var cache = new PayoutSnapshotCache();
        Assert.False(cache.TryGetCharacterName(Guid.NewGuid(), out _));
    }

    [Fact]
    public void UpdateFromPending_then_TryGetCharacterName_succeeds()
    {
        var id = Guid.NewGuid();
        var cache = new PayoutSnapshotCache();
        cache.UpdateFromPending([PayoutTestData.Pending(id, characterName: "Jaina")]);
        Assert.True(cache.TryGetCharacterName(id, out var name));
        Assert.Equal("Jaina", name);
    }

    [Fact]
    public void UpdateFromPending_replaces_prior_map()
    {
        var id = Guid.NewGuid();
        var cache = new PayoutSnapshotCache();
        cache.UpdateFromPending([PayoutTestData.Pending(id, characterName: "A")]);
        cache.UpdateFromPending([PayoutTestData.Pending(id, characterName: "B")]);
        Assert.True(cache.TryGetCharacterName(id, out var name));
        Assert.Equal("B", name);
    }
}

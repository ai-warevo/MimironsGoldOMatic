using MimironsGoldOMatic.Desktop.IntegrationTests.Support;
using MimironsGoldOMatic.Desktop.Services;
using MimironsGoldOMatic.IntegrationTesting;
using Xunit;

namespace MimironsGoldOMatic.Desktop.IntegrationTests;

/// <summary>
/// Polling <c>GET /api/payouts/pending</c> via <see cref="EbsDesktopClient"/> (Desktop refresh) and backing cache used for MGM_ACCEPT resolution.
/// </summary>
[Collection(nameof(DesktopIntegrationPostgresCollection))]
[Trait("Category", "Integration")]
public sealed class DesktopPayoutPollingAndSyncIntegrationTests : HttpApiFixtureBase
{
    public DesktopPayoutPollingAndSyncIntegrationTests(PostgresContainerFixture pg) : base(pg)
    {
    }

    [Fact]
    public async Task Second_poll_sees_new_pending_row_simulating_refresh()
    {
        await ResetDatabaseAndRestartHostAsync();
        var ebs = DesktopEbsClientFactory.Create(Host);
        Assert.Empty(await ebs.GetPendingAsync(CancellationToken.None));

        var id1 = Guid.NewGuid();
        await PayoutDocumentSeed.InsertPendingAsync(Host.Services, PayoutDocumentSeed.CreatePending(id1, "A", "t1"));

        var first = await ebs.GetPendingAsync(CancellationToken.None);
        Assert.Single(first);
        Assert.Equal(id1, first[0].Id);

        var id2 = Guid.NewGuid();
        await PayoutDocumentSeed.InsertPendingAsync(Host.Services, PayoutDocumentSeed.CreatePending(id2, "B", "t2"));

        var second = await ebs.GetPendingAsync(CancellationToken.None);
        Assert.Equal(2, second.Count);
        Assert.Contains(second, p => p.Id == id1);
        Assert.Contains(second, p => p.Id == id2);
    }

    [Fact]
    public async Task PayoutSnapshotCache_reflects_pending_list_after_poll_like_view_backing_state()
    {
        await ResetDatabaseAndRestartHostAsync();
        var id = Guid.NewGuid();
        await PayoutDocumentSeed.InsertPendingAsync(Host.Services, PayoutDocumentSeed.CreatePending(id, "Jaina", "tj"));

        var ebs = DesktopEbsClientFactory.Create(Host);
        var list = await ebs.GetPendingAsync(CancellationToken.None);
        var cache = new PayoutSnapshotCache();
        cache.UpdateFromPending(list);

        Assert.True(cache.TryGetCharacterName(id, out var name));
        Assert.Equal("Jaina", name);
    }
}

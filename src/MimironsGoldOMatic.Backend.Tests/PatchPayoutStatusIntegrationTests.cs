using MimironsGoldOMatic.Backend.Application;
using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Tests.Support;
using MimironsGoldOMatic.Shared;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MimironsGoldOMatic.Backend.Tests;

[Collection(nameof(PostgresCollection))]
public sealed class PatchPayoutStatusIntegrationTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _pg;
    private ServiceProvider? _services;

    public PatchPayoutStatusIntegrationTests(PostgresContainerFixture pg) => _pg = pg;

    public async Task InitializeAsync()
    {
        _services = BackendTestHost.CreateServiceProvider(_pg.ConnectionString);
        var store = _services.GetRequiredService<IDocumentStore>();
        await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
        await PostgresMgmTruncate.TruncateAllAsync(_pg.ConnectionString);
    }

    public async Task DisposeAsync()
    {
        if (_services != null)
            await _services.DisposeAsync();
    }

    [Fact]
    public async Task Patch_to_Sent_removes_winner_from_pool()
    {
        var sp = _services!;
        var store = sp.GetRequiredService<IDocumentStore>();
        var payoutId = Guid.NewGuid();
        const string uid = "pool-winner";

        await using (var s = store.LightweightSession())
        {
            s.Store(new PoolDocument
            {
                Id = EbsIds.PoolDocumentId,
                Members =
                [
                    new PoolMemberEntry { TwitchUserId = uid, TwitchDisplayName = "W", CharacterName = "Abcd" },
                ],
            });
            s.Store(new PayoutReadDocument
            {
                Id = payoutId,
                TwitchUserId = uid,
                TwitchDisplayName = "W",
                CharacterName = "Abcd",
                GoldAmount = PayoutEconomics.MvpWinningPayoutGold,
                EnrollmentRequestId = "spin:test",
                Status = PayoutStatus.InProgress,
                CreatedAt = DateTime.UtcNow,
            });
            await s.SaveChangesAsync();
        }

        var m = sp.GetRequiredService<IMediator>();
        var r = await m.Send(new PatchPayoutStatusCommand(payoutId, PayoutStatus.Sent));
        Assert.True(r.Ok);

        await using var q = store.QuerySession();
        var pool = await q.LoadAsync<PoolDocument>(EbsIds.PoolDocumentId, CancellationToken.None);
        Assert.NotNull(pool);
        Assert.DoesNotContain(pool.Members, x => x.TwitchUserId == uid);
    }
}

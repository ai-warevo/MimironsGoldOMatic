using MimironsGoldOMatic.Backend.Abstract;
using MimironsGoldOMatic.Backend.Domain;
using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Shared;
using MimironsGoldOMatic.Backend.IntegrationTests.Support;
using MimironsGoldOMatic.IntegrationTesting;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MimironsGoldOMatic.Backend.IntegrationTests;

[Collection(nameof(PostgresCollection))]
[Trait("Category", "Integration")]
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
    public async Task Should_RemoveWinnerFromPool_WhenPatchToSent()
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

    [Fact]
    public async Task Should_Return409_WhenDisallowedPendingToSent()
    {
        var sp = _services!;
        var store = sp.GetRequiredService<IDocumentStore>();
        var payoutId = Guid.NewGuid();
        await using (var s = store.LightweightSession())
        {
            s.Store(new PayoutReadDocument
            {
                Id = payoutId,
                TwitchUserId = "u",
                TwitchDisplayName = "d",
                CharacterName = "Abcd",
                GoldAmount = PayoutEconomics.MvpWinningPayoutGold,
                EnrollmentRequestId = "spin:x",
                Status = PayoutStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            });
            await s.SaveChangesAsync();
        }

        var m = sp.GetRequiredService<IMediator>();
        var r = await m.Send(new PatchPayoutStatusCommand(payoutId, PayoutStatus.Sent));
        Assert.False(r.Ok);
        Assert.Equal(409, r.StatusCode);
    }

    [Fact]
    public async Task Should_AllowInProgressToCancelled()
    {
        var sp = _services!;
        var store = sp.GetRequiredService<IDocumentStore>();
        var payoutId = Guid.NewGuid();
        await using (var s = store.LightweightSession())
        {
            s.Store(new PayoutReadDocument
            {
                Id = payoutId,
                TwitchUserId = "u",
                TwitchDisplayName = "d",
                CharacterName = "Abcd",
                GoldAmount = PayoutEconomics.MvpWinningPayoutGold,
                EnrollmentRequestId = "spin:x",
                Status = PayoutStatus.InProgress,
                CreatedAt = DateTime.UtcNow,
            });
            await s.SaveChangesAsync();
        }

        var m = sp.GetRequiredService<IMediator>();
        var r = await m.Send(new PatchPayoutStatusCommand(payoutId, PayoutStatus.Cancelled));
        Assert.True(r.Ok);
        Assert.Equal(PayoutStatus.Cancelled, r.Value!.Status);
    }
}

using MimironsGoldOMatic.Backend.Application;
using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.IntegrationTests.Support;
using MimironsGoldOMatic.Shared;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MimironsGoldOMatic.Backend.IntegrationTests;

[Collection(nameof(PostgresCollection))]
[Trait("Category", "Integration")]
public sealed class PostClaimRulesIntegrationTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _pg;
    private ServiceProvider? _services;

    public PostClaimRulesIntegrationTests(PostgresContainerFixture pg) => _pg = pg;

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
    public async Task Should_Return201Then200_WhenSameEnrollmentId_Idempotent()
    {
        var m = _services!.GetRequiredService<IMediator>();
        var body = new CreatePayoutRequest("Abcd", "enroll-idem-1");
        var r1 = await m.Send(new PostClaimCommand("twitch-user-a", "DisplayA", body));
        Assert.True(r1.Ok);
        Assert.Equal(201, r1.StatusCode);

        var r2 = await m.Send(new PostClaimCommand("twitch-user-a", "DisplayA", body));
        Assert.True(r2.Ok);
        Assert.Equal(200, r2.StatusCode);
    }

    [Fact]
    public async Task Should_Return409_WhenEnrollmentIdBoundToDifferentTwitchUser()
    {
        var m = _services!.GetRequiredService<IMediator>();
        var body = new CreatePayoutRequest("Abcd", "enroll-shared-key");
        Assert.True((await m.Send(new PostClaimCommand("user-one", "D1", body))).Ok);
        var r2 = await m.Send(new PostClaimCommand("user-two", "D2", body));
        Assert.False(r2.Ok);
        Assert.Equal(409, r2.StatusCode);
    }

    [Fact]
    public async Task Should_Return409_WhenPendingPayoutExistsForUser()
    {
        var sp = _services!;
        var store = sp.GetRequiredService<IDocumentStore>();
        var payoutId = Guid.NewGuid();
        await using (var s = store.LightweightSession())
        {
            s.Store(new PayoutReadDocument
            {
                Id = payoutId,
                TwitchUserId = "blocked-user",
                TwitchDisplayName = "B",
                CharacterName = "Xyzz",
                GoldAmount = PayoutEconomics.MvpWinningPayoutGold,
                EnrollmentRequestId = "spin:00000000-0000-0000-0000-000000000001",
                Status = PayoutStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            });
            await s.SaveChangesAsync();
        }

        var m = sp.GetRequiredService<IMediator>();
        var r = await m.Send(new PostClaimCommand("blocked-user", "B",
            new CreatePayoutRequest("Abcd", "new-enroll-1")));
        Assert.False(r.Ok);
        Assert.Equal(409, r.StatusCode);
    }

    [Fact]
    public async Task Should_Return409_WhenLifetimeSentCapWouldBeExceeded()
    {
        var sp = _services!;
        var store = sp.GetRequiredService<IDocumentStore>();
        await using (var s = store.LightweightSession())
        {
            for (var i = 0; i < 10; i++)
            {
                s.Store(new PayoutReadDocument
                {
                    Id = Guid.NewGuid(),
                    TwitchUserId = "capped-user",
                    TwitchDisplayName = "C",
                    CharacterName = "Abcd",
                    GoldAmount = PayoutEconomics.MvpWinningPayoutGold,
                    EnrollmentRequestId = $"hist-{i}",
                    Status = PayoutStatus.Sent,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                });
            }

            await s.SaveChangesAsync();
        }

        var m = sp.GetRequiredService<IMediator>();
        var r = await m.Send(new PostClaimCommand("capped-user", "C",
            new CreatePayoutRequest("Abcd", "fresh-enroll")));
        Assert.False(r.Ok);
        Assert.Equal(409, r.StatusCode);
    }
}

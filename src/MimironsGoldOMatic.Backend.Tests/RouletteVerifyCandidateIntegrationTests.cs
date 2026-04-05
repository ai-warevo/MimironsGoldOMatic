using MimironsGoldOMatic.Backend.Api;
using MimironsGoldOMatic.Backend.Application;
using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Services;
using MimironsGoldOMatic.Backend.Tests.Support;
using MimironsGoldOMatic.Shared;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MimironsGoldOMatic.Backend.Tests;

[Collection(nameof(PostgresCollection))]
[Trait("Category", "Integration")]
public sealed class RouletteVerifyCandidateIntegrationTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _pg;
    private ServiceProvider? _services;

    public RouletteVerifyCandidateIntegrationTests(PostgresContainerFixture pg) => _pg = pg;

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

    /// <summary>
    /// One pool member, spin in a closed cycle: tick picks candidate; verify-candidate (online) creates Pending payout.
    /// Uses a past pick instant with a verification deadline still in the future so the handler accepts real wall-clock time.
    /// </summary>
    [Fact]
    public async Task Roulette_single_participant_verify_online_creates_pending_payout()
    {
        var sp = _services!;
        var store = sp.GetRequiredService<IDocumentStore>();
        var spinCycleId = Guid.NewGuid();
        var anchor = DateTime.UtcNow;
        var cycleStart = RouletteTime.FloorToFiveMinuteUtc(anchor).AddMinutes(-5);
        var pickTime = cycleStart.AddSeconds(RouletteTime.CollectingSeconds + 1);

        await using (var s = store.LightweightSession())
        {
            s.Store(new PoolDocument
            {
                Id = EbsIds.PoolDocumentId,
                Members =
                [
                    new PoolMemberEntry
                    {
                        TwitchUserId = "winner-twitch",
                        TwitchDisplayName = "WinnerDisp",
                        CharacterName = "Hero",
                    },
                ],
            });
            s.Store(new SpinStateDocument
            {
                Id = EbsIds.SpinStateDocumentId,
                CycleStartUtc = cycleStart,
                SpinCycleId = spinCycleId,
                VerificationDeadlineUtc = anchor.AddHours(1),
                PoolWasEmptyAtCycleStart = false,
                PayoutCreatedForCycle = false,
            });
            await s.SaveChangesAsync();
        }

        await RouletteCycleTick.ApplyAsync(store, pickTime, CancellationToken.None);

        await using (var check = store.QuerySession())
        {
            var spin = await check.LoadAsync<SpinStateDocument>(EbsIds.SpinStateDocumentId, CancellationToken.None);
            Assert.NotNull(spin);
            Assert.Equal("winner-twitch", spin.CandidateTwitchUserId);
            Assert.Equal("Hero", spin.CandidateCharacterName);
        }

        var m = sp.GetRequiredService<IMediator>();
        var vr = await m.Send(new VerifyCandidateCommand(
            new VerifyCandidateRequest(1, spinCycleId, "Hero", true, DateTime.UtcNow)));
        Assert.True(vr.Ok);
        Assert.Equal(200, vr.StatusCode);

        await using var q = store.QuerySession();
        var pending = await q.Query<PayoutReadDocument>()
            .Where(x => x.TwitchUserId == "winner-twitch" && x.Status == PayoutStatus.Pending)
            .SingleAsync(CancellationToken.None);
        Assert.Equal("Hero", pending.CharacterName);
        Assert.Equal(PayoutEconomics.MvpWinningPayoutGold, pending.GoldAmount);
    }
}

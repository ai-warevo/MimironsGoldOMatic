using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Services;
using MimironsGoldOMatic.Backend.IntegrationTests.Support;
using MimironsGoldOMatic.IntegrationTesting;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MimironsGoldOMatic.Backend.IntegrationTests;

[Collection(nameof(PostgresCollection))]
[Trait("Category", "Integration")]
public sealed class RouletteCycleTickIntegrationTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _pg;
    private ServiceProvider? _services;

    public RouletteCycleTickIntegrationTests(PostgresContainerFixture pg) => _pg = pg;

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
    public async Task Should_ResetSpinFields_WhenFiveMinuteCycleAdvances()
    {
        var store = _services!.GetRequiredService<IDocumentStore>();
        var oldCycle = Guid.NewGuid();
        var t0 = RouletteTime.FloorToFiveMinuteUtc(DateTime.UtcNow).AddMinutes(-10);

        await using (var s = store.LightweightSession())
        {
            s.Store(new PoolDocument
            {
                Id = EbsIds.PoolDocumentId,
                Members = [new PoolMemberEntry { TwitchUserId = "u1", CharacterName = "A", TwitchDisplayName = "d" }],
            });
            s.Store(new SpinStateDocument
            {
                Id = EbsIds.SpinStateDocumentId,
                CycleStartUtc = t0,
                SpinCycleId = oldCycle,
                CandidateTwitchUserId = "u1",
                CandidateCharacterName = "A",
                CandidateSelectedAtUtc = DateTime.UtcNow,
                PayoutCreatedForCycle = true,
                PoolWasEmptyAtCycleStart = false,
                VerificationDeadlineUtc = t0.AddMinutes(5).AddSeconds(30),
            });
            await s.SaveChangesAsync();
        }

        var utcNow = RouletteTime.FloorToFiveMinuteUtc(DateTime.UtcNow);
        await RouletteCycleTick.ApplyAsync(store, utcNow, CancellationToken.None);

        await using var q = store.QuerySession();
        var spin = await q.LoadAsync<SpinStateDocument>(EbsIds.SpinStateDocumentId, CancellationToken.None);
        Assert.NotNull(spin);
        Assert.Equal(utcNow, spin.CycleStartUtc);
        Assert.NotEqual(oldCycle, spin.SpinCycleId);
        Assert.Null(spin.CandidateTwitchUserId);
        Assert.False(spin.PayoutCreatedForCycle);
    }
}

using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Services;
using MimironsGoldOMatic.Backend.Tests.Support;
using MimironsGoldOMatic.Shared;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MimironsGoldOMatic.Backend.Tests;

[Collection(nameof(PostgresCollection))]
[Trait("Category", "Integration")]
public sealed class PayoutExpirationIntegrationTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _pg;
    private ServiceProvider? _services;

    public PayoutExpirationIntegrationTests(PostgresContainerFixture pg) => _pg = pg;

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
    public async Task ExpireStalePayouts_marks_pending_older_than_24h_as_expired()
    {
        var store = _services!.GetRequiredService<IDocumentStore>();
        var id = Guid.NewGuid();
        await using (var s = store.LightweightSession())
        {
            s.Store(new PayoutReadDocument
            {
                Id = id,
                TwitchUserId = "exp-user",
                TwitchDisplayName = "E",
                CharacterName = "Abcd",
                GoldAmount = PayoutEconomics.MvpWinningPayoutGold,
                EnrollmentRequestId = "spin:dead",
                Status = PayoutStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddHours(-30),
            });
            await s.SaveChangesAsync();
        }

        var n = await PayoutExpirationProcessor.ExpireStalePayoutsAsync(store, CancellationToken.None);
        Assert.Equal(1, n);

        await using var read = store.QuerySession();
        var p = await read.LoadAsync<PayoutReadDocument>(id, CancellationToken.None);
        Assert.NotNull(p);
        Assert.Equal(PayoutStatus.Expired, p.Status);
    }
}

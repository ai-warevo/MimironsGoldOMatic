using MimironsGoldOMatic.Backend.Application;
using MimironsGoldOMatic.Backend.UnitTests.Support;
using MimironsGoldOMatic.Shared;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MimironsGoldOMatic.Backend.UnitTests;

/// <summary>Isolated host with <c>DevSkipSubscriberCheck=false</c> (does not share <see cref="PostClaimRulesIntegrationTests"/> DI).</summary>
[Collection(nameof(PostgresCollection))]
[Trait("Category", "Integration")]
public sealed class PostClaimSubscriberGateIntegrationTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _pg;
    private ServiceProvider? _services;

    public PostClaimSubscriberGateIntegrationTests(PostgresContainerFixture pg) => _pg = pg;

    public async Task InitializeAsync()
    {
        _services = BackendTestHost.CreateServiceProvider(_pg.ConnectionString, devSkipSubscriberCheck: false);
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
    public async Task Should_Return403_WhenSubscriberCheckNotSkipped()
    {
        var m = _services!.GetRequiredService<IMediator>();
        var r = await m.Send(new PostClaimCommand("u", "D", new CreatePayoutRequest("Abcd", "e-new")));
        Assert.False(r.Ok);
        Assert.Equal(403, r.StatusCode);
    }
}

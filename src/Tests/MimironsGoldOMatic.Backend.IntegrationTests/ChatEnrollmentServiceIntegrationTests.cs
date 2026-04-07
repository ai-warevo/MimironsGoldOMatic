using MimironsGoldOMatic.Backend.Infrastructure.Persistence;
using MimironsGoldOMatic.Backend.Application.Roulette.Enrollment;
using MimironsGoldOMatic.Backend.IntegrationTests.Support;
using MimironsGoldOMatic.IntegrationTesting;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MimironsGoldOMatic.Backend.IntegrationTests;

[Collection(nameof(PostgresCollection))]
[Trait("Category", "Integration")]
public sealed class ChatEnrollmentServiceIntegrationTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _pg;
    private ServiceProvider? _services;

    public ChatEnrollmentServiceIntegrationTests(PostgresContainerFixture pg) => _pg = pg;

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
    public async Task Should_NoOp_WhenMessageIdEmpty()
    {
        var sut = _services!.GetRequiredService<ChatEnrollmentService>();
        await sut.IngestAsync("", "u1", "d", "!twgold Abcd", true, CancellationToken.None);

        await using var q = _services!.GetRequiredService<IDocumentStore>().QuerySession();
        var pool = await q.LoadAsync<PoolDocument>(EbsIds.PoolDocumentId, CancellationToken.None);
        Assert.True(pool == null || pool.Members.Count == 0);
    }

    [Fact]
    public async Task Should_NoOp_WhenDuplicateMessageId()
    {
        var store = _services!.GetRequiredService<IDocumentStore>();
        await using (var s = store.LightweightSession())
        {
            s.Store(new ChatMessageDedupDocument { Id = "mid-1", ProcessedAtUtc = DateTime.UtcNow });
            await s.SaveChangesAsync();
        }

        var sut = _services!.GetRequiredService<ChatEnrollmentService>();
        await sut.IngestAsync("mid-1", "u1", "d", "!twgold Abcd", true, CancellationToken.None);

        await using var q = store.QuerySession();
        var pool = await q.LoadAsync<PoolDocument>(EbsIds.PoolDocumentId, CancellationToken.None);
        Assert.True(pool == null || pool.Members.Count == 0);
    }

    [Fact]
    public async Task Should_NoOp_WhenNotSubscriber()
    {
        var sut = _services!.GetRequiredService<ChatEnrollmentService>();
        await sut.IngestAsync("m-sub", "u1", "d", "!twgold Abcd", false, CancellationToken.None);

        await using var q = _services!.GetRequiredService<IDocumentStore>().QuerySession();
        var pool = await q.LoadAsync<PoolDocument>(EbsIds.PoolDocumentId, CancellationToken.None);
        Assert.True(pool == null || pool.Members.Count == 0);
    }

    [Fact]
    public async Task Should_AddMember_WhenSubscriberAndValidTwGold()
    {
        var sut = _services!.GetRequiredService<ChatEnrollmentService>();
        await sut.IngestAsync("m-new", "viewer-1", "Disp", "!twgold Abcd", true, CancellationToken.None);

        await using var q = _services!.GetRequiredService<IDocumentStore>().QuerySession();
        var pool = await q.LoadAsync<PoolDocument>(EbsIds.PoolDocumentId, CancellationToken.None);
        Assert.NotNull(pool);
        var row = Assert.Single(pool.Members);
        Assert.Equal("viewer-1", row.TwitchUserId);
        Assert.Equal("Abcd", row.CharacterName);
    }

    [Fact]
    public async Task Should_NoOp_WhenCharacterNameHeldByAnotherViewer()
    {
        var store = _services!.GetRequiredService<IDocumentStore>();
        await using (var s = store.LightweightSession())
        {
            s.Store(new PoolDocument
            {
                Id = EbsIds.PoolDocumentId,
                Members =
                [
                    new PoolMemberEntry { TwitchUserId = "owner", TwitchDisplayName = "O", CharacterName = "Taken" },
                ],
            });
            await s.SaveChangesAsync();
        }

        var sut = _services!.GetRequiredService<ChatEnrollmentService>();
        await sut.IngestAsync("m-taken", "other-viewer", "D", "!twgold Taken", true, CancellationToken.None);

        await using var q = store.QuerySession();
        var pool = await q.LoadAsync<PoolDocument>(EbsIds.PoolDocumentId, CancellationToken.None);
        Assert.NotNull(pool);
        Assert.Single(pool.Members);
    }
}

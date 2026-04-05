using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MimironsGoldOMatic.IntegrationTesting;
using MimironsGoldOMatic.Backend.Persistence;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MimironsGoldOMatic.Backend.IntegrationTests;

/// <summary>
/// <see cref="docs/overview/INTERACTION_SCENARIOS.md"/> <b>SC-005</b> — HTTP <c>POST /api/twitch/eventsub</c> through Kestrel + Marten (empty webhook secret for signature bypass in test host).
/// </summary>
[Collection(nameof(PostgresCollection))]
[Trait("Category", "Integration")]
public sealed class EventSubHttpEnrollmentIntegrationTests : HttpApiFixtureBase
{
    public EventSubHttpEnrollmentIntegrationTests(PostgresContainerFixture pg) : base(pg)
    {
    }

    [Fact]
    public async Task SC005_Should_EnrollSubscriber_WhenChannelChatMessageMatchesTwGold()
    {
        var client = await CreateCleanClientAsync();
        var messageId = "msg-sc005-" + Guid.NewGuid().ToString("N");
        var chatterId = "twitch-sub-" + Guid.NewGuid().ToString("N");
        var ev = new
        {
            message_id = messageId,
            chatter_user_id = chatterId,
            chatter_user_login = "sublogin",
            message = new { text = "!twgold Norinn" },
            badges = new[] { new { set_id = "subscriber", id = "0" } },
        };
        var evJson = JsonSerializer.Serialize(ev);
        var body = "{\"subscription\":{\"type\":\"channel.chat.message\"},\"event\":" + evJson + "}";
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        var res = await client.PostAsync("/api/twitch/eventsub", content);
        res.EnsureSuccessStatusCode();

        await using var scope = Host.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        await using var q = store.QuerySession();
        var pool = await q.LoadAsync<PoolDocument>(EbsIds.PoolDocumentId, CancellationToken.None);
        Assert.NotNull(pool);
        Assert.Contains(pool.Members,
            m => m.TwitchUserId == chatterId && m.CharacterName == "Norinn");
    }
}

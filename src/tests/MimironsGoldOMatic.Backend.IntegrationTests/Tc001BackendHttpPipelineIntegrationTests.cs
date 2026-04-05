using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MimironsGoldOMatic.Backend.Api;
using MimironsGoldOMatic.Backend.IntegrationTests.Support;
using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Services;
using MimironsGoldOMatic.Shared;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MimironsGoldOMatic.Backend.IntegrationTests;

/// <summary>
/// Backend-visible slice of <see cref="docs/INTERACTION_SCENARIOS.md"/> <b>TC-001</b> / <b>SC-001</b> (WoW/Desktop segments remain manual).
/// Covers Extension claim, pool read, Desktop verify-candidate over HTTP, and payout row in PostgreSQL.
/// </summary>
[Collection(nameof(PostgresCollection))]
[Trait("Category", "Integration")]
public sealed class Tc001BackendHttpPipelineIntegrationTests : HttpApiFixtureBase
{
    public Tc001BackendHttpPipelineIntegrationTests(PostgresContainerFixture pg) : base(pg)
    {
    }

    [Fact]
    public async Task TC001_Step1to2_Should_EnrollViaClaimAndShowPoolMe()
    {
        var client = await CreateCleanClientAsync();
        var jwt = ExtensionJwtTestHelper.CreateViewerToken("viewer-tc001", "ViewerDisp");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var enrollId = $"enroll-tc001-http-{Guid.NewGuid():N}";
        const string letters = "abcdefghijklmnopqrstuvwxyz";
        var suffix = new string(Enumerable.Range(0, 4).Select(_ => letters[Random.Shared.Next(letters.Length)]).ToArray());
        var characterName = "Nori" + suffix;
        var claim = await client.PostAsJsonAsync("/api/payouts/claim",
            new CreatePayoutRequest(characterName, enrollId));
        Assert.Equal(HttpStatusCode.Created, claim.StatusCode);

        var me = await client.GetAsync("/api/pool/me");
        me.EnsureSuccessStatusCode();
        var poolMe = await me.Content.ReadFromJsonAsync<PoolMeResponse>();
        Assert.True(poolMe!.IsEnrolled);
        Assert.Equal(characterName, poolMe.CharacterName);
    }

    [Fact]
    public async Task TC001_VerifyCandidateViaHttp_Should_CreatePendingPayout_WhenOnline()
    {
        _ = await CreateCleanClientAsync();
        var winnerTwitchUserId = "winner-http-" + Guid.NewGuid().ToString("N");

        await using (var scope = Host.Services.CreateAsyncScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            var spinCycleId = Guid.NewGuid();
            var anchor = DateTime.UtcNow;
            var cycleStart = RouletteTime.FloorToFiveMinuteUtc(anchor).AddMinutes(-5);
            await using (var s = store.LightweightSession())
            {
                s.Store(new PoolDocument
                {
                    Id = EbsIds.PoolDocumentId,
                    Members =
                    [
                        new PoolMemberEntry
                        {
                            TwitchUserId = winnerTwitchUserId,
                            TwitchDisplayName = "W",
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

            await RouletteCycleTick.ApplyAsync(store, cycleStart.AddSeconds(RouletteTime.CollectingSeconds + 1),
                CancellationToken.None);
        }

        var spinId = await GetSpinCycleIdAsync();
        var desk = Host.CreateClient();
        desk.DefaultRequestHeaders.Add("X-MGM-ApiKey", "integration-test-desktop-api-key");
        var vr = await desk.PostAsJsonAsync("/api/roulette/verify-candidate",
            new VerifyCandidateRequest(1, spinId, "Hero", true, DateTime.UtcNow));
        Assert.Equal(HttpStatusCode.OK, vr.StatusCode);

        await using var scope2 = Host.Services.CreateAsyncScope();
        var store2 = scope2.ServiceProvider.GetRequiredService<IDocumentStore>();
        await using var q = store2.QuerySession();
        var pending = await q.Query<PayoutReadDocument>()
            .Where(x => x.TwitchUserId == winnerTwitchUserId && x.Status == PayoutStatus.Pending)
            .SingleAsync(CancellationToken.None);
        Assert.Equal("Hero", pending.CharacterName);
    }

    private async Task<Guid> GetSpinCycleIdAsync()
    {
        await using var scope = Host.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        await using var q = store.QuerySession();
        var spin = await q.LoadAsync<SpinStateDocument>(EbsIds.SpinStateDocumentId, CancellationToken.None);
        return spin!.SpinCycleId;
    }
}

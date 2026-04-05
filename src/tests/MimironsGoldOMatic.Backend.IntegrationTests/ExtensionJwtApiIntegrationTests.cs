using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MimironsGoldOMatic.Backend.Api;
using MimironsGoldOMatic.Backend.IntegrationTests.Support;
using MimironsGoldOMatic.Shared;
using Xunit;

namespace MimironsGoldOMatic.Backend.IntegrationTests;

/// <summary>
/// <see cref="docs/INTERACTION_SCENARIOS.md"/> <b>TC-016</b>, <b>TC-017</b> — Extension JWT Bearer vs roulette/pool routes (Development signing key).
/// </summary>
[Collection(nameof(PostgresCollection))]
[Trait("Category", "Integration")]
public sealed class ExtensionJwtApiIntegrationTests : HttpApiFixtureBase
{
    public ExtensionJwtApiIntegrationTests(PostgresContainerFixture pg) : base(pg)
    {
    }

    [Fact]
    public async Task TC016_Should_Return401_WhenClaimWithoutBearer()
    {
        var client = await CreateCleanClientAsync();
        var res = await client.PostAsJsonAsync("/api/payouts/claim", new CreatePayoutRequest("Abcd", "tc016-enroll"));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task TC017_Should_ReturnOk_WhenValidJwtOnPoolMe()
    {
        var client = await CreateCleanClientAsync();
        var jwt = ExtensionJwtTestHelper.CreateViewerToken("viewer-tc017", "DisplayTc017");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var res = await client.GetAsync("/api/pool/me");
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<PoolMeResponse>();
        Assert.NotNull(body);
        Assert.False(body.IsEnrolled);
    }

    [Fact]
    public async Task TC017_Should_AllowRouletteState_WhenValidJwt()
    {
        var client = await CreateCleanClientAsync();
        var jwt = ExtensionJwtTestHelper.CreateViewerToken("viewer-state", "D");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var res = await client.GetAsync("/api/roulette/state");
        res.EnsureSuccessStatusCode();
        var state = await res.Content.ReadFromJsonAsync<RouletteStateResponse>();
        Assert.NotNull(state);
        Assert.Equal(300, state.SpinIntervalSeconds);
    }
}

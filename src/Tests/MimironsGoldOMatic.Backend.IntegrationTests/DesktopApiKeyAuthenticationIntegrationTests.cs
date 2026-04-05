using System.Net;
using MimironsGoldOMatic.IntegrationTesting;
using Xunit;

namespace MimironsGoldOMatic.Backend.IntegrationTests;

/// <summary>
/// <see cref="docs/overview/INTERACTION_SCENARIOS.md"/> <b>SC-002</b>, <b>TC-003</b>, <b>TC-004</b> — Desktop <c>X-MGM-ApiKey</c> against real host + PostgreSQL.
/// </summary>
[Collection(nameof(PostgresCollection))]
[Trait("Category", "Integration")]
public sealed class DesktopApiKeyAuthenticationIntegrationTests : HttpApiFixtureBase
{
    public DesktopApiKeyAuthenticationIntegrationTests(PostgresContainerFixture pg) : base(pg)
    {
    }

    [Fact]
    public async Task TC003_SC002_Should_ReturnOk_WhenValidApiKeyOnPending()
    {
        var client = await CreateCleanClientAsync();
        client.DefaultRequestHeaders.Add("X-MGM-ApiKey", IntegrationTestConstants.DesktopApiKey);
        var res = await client.GetAsync("/api/payouts/pending");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task TC004_Should_Reject_WhenApiKeyWrong()
    {
        var client = await CreateCleanClientAsync();
        client.DefaultRequestHeaders.Add("X-MGM-ApiKey", "not-the-configured-key");
        var res = await client.GetAsync("/api/payouts/pending");
        Assert.False(res.IsSuccessStatusCode);
        Assert.True(res.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }
}

using MimironsGoldOMatic.Desktop.IntegrationTests.Support;
using MimironsGoldOMatic.Desktop.Services;
using MimironsGoldOMatic.IntegrationTesting;
using Xunit;

namespace MimironsGoldOMatic.Desktop.IntegrationTests;

/// <summary>
/// Desktop "login" = configured base URL + <c>X-MGM-ApiKey</c> (<c>docs/overview/SPEC.md</c>, <c>docs/overview/INTERACTION_SCENARIOS.md</c> SC-002 / TC-003–004).
/// </summary>
[Collection(nameof(DesktopIntegrationPostgresCollection))]
[Trait("Category", "Integration")]
public sealed class DesktopAuthenticationFlowIntegrationTests : HttpApiFixtureBase
{
    public DesktopAuthenticationFlowIntegrationTests(PostgresContainerFixture pg) : base(pg)
    {
    }

    [Fact]
    public async Task Valid_api_key_GetPending_succeeds_with_empty_queue()
    {
        await ResetDatabaseAndRestartHostAsync();
        var ebs = DesktopEbsClientFactory.Create(Host);
        var list = await ebs.GetPendingAsync(CancellationToken.None);
        Assert.Empty(list);
    }

    [Fact]
    public async Task Wrong_api_key_GetPending_fails()
    {
        await ResetDatabaseAndRestartHostAsync();
        var ebs = DesktopEbsClientFactory.Create(Host, apiKeyOverride: "not-the-integration-key");
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => ebs.GetPendingAsync(CancellationToken.None));
        Assert.True(
            ex.Message.Contains("401", StringComparison.Ordinal) || ex.Message.Contains("403", StringComparison.Ordinal),
            ex.Message);
    }

    [Fact]
    public void Missing_api_key_throws_before_http()
    {
        var factory = new WebApplicationHttpClientFactory(() => Host.CreateClient());
        var abs = Host.Server.BaseAddress!.AbsoluteUri.TrimEnd('/');
        var ebs = new EbsDesktopClient(factory, () => (abs, "  "));
        Assert.Throws<InvalidOperationException>(() => ebs.GetPendingAsync(CancellationToken.None).GetAwaiter().GetResult());
    }
}

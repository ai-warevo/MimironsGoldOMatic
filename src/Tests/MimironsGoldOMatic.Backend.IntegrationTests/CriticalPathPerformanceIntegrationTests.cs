using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MimironsGoldOMatic.Backend.Domain;
using MimironsGoldOMatic.Backend.IntegrationTests.Support;
using MimironsGoldOMatic.IntegrationTesting;
using Xunit;
using Xunit.Abstractions;

namespace MimironsGoldOMatic.Backend.IntegrationTests;

/// <summary>
/// Lightweight timing smoke (not BenchmarkDotNet). Guards against accidental regressions that add seconds of latency on hot paths.
/// </summary>
[Collection(nameof(PostgresCollection))]
[Trait("Category", "Integration")]
[Trait("Kind", "Performance")]
public sealed class CriticalPathPerformanceIntegrationTests : HttpApiFixtureBase
{
    private readonly ITestOutputHelper _out;

    public CriticalPathPerformanceIntegrationTests(PostgresContainerFixture pg, ITestOutputHelper outHelper) : base(pg) =>
        _out = outHelper;

    [Fact]
    public async Task RouletteState_Should_CompleteFourCalls_UnderFiveSeconds()
    {
        var client = await CreateCleanClientAsync();
        var jwt = ExtensionJwtTestHelper.CreateViewerToken("perf-user", "P");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Fixed window is 5 requests/min per authenticated Extension user; stay under the cap.
        const int iterations = 4;
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            var res = await client.GetAsync("/api/roulette/state");
            res.EnsureSuccessStatusCode();
            _ = await res.Content.ReadFromJsonAsync<RouletteStateResponse>();
        }

        sw.Stop();
        _out.WriteLine("ElapsedMs={0}", sw.ElapsedMilliseconds);
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(5), $"{iterations}x GET /api/roulette/state took {sw.Elapsed}");
    }
}

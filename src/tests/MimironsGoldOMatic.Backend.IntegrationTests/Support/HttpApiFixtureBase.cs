using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MimironsGoldOMatic.Backend.IntegrationTests.Support;

/// <summary>Boots <see cref="BackendWebApplicationFactory"/> once per test class, truncates Marten schema before each HTTP call that uses <see cref="CreateCleanClientAsync"/>.</summary>
public abstract class HttpApiFixtureBase : IAsyncLifetime
{
    private readonly PostgresContainerFixture _pg;
    private BackendWebApplicationFactory? _factory;

    protected HttpApiFixtureBase(PostgresContainerFixture pg) => _pg = pg;

    protected BackendWebApplicationFactory Host => _factory!;

    public async Task InitializeAsync()
    {
        await PostgresMgmTruncate.TruncateAllAsync(_pg.ConnectionString);
        _factory = new BackendWebApplicationFactory(_pg.ConnectionString);
        using var _ = _factory.CreateClient();
        await PostgresMgmTruncate.TruncateAllAsync(_pg.ConnectionString);
    }

    public async Task DisposeAsync()
    {
        if (_factory != null)
            await _factory.DisposeAsync();
    }

    /// <summary>Clears <c>mgm</c> tables and returns a new <see cref="HttpClient"/> for the running host.</summary>
    protected async Task<HttpClient> CreateCleanClientAsync()
    {
        await PostgresMgmTruncate.TruncateAllAsync(_pg.ConnectionString);
        return _factory!.CreateClient();
    }
}

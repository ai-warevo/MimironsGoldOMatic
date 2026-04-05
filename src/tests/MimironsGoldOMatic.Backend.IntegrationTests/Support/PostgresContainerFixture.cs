using Testcontainers.PostgreSql;
using Xunit;

namespace MimironsGoldOMatic.Backend.IntegrationTests.Support;

/// <summary>One PostgreSQL container per test collection (requires Docker).</summary>
public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync() => await _container.StartAsync();

    public async Task DisposeAsync() => await _container.DisposeAsync();
}

[CollectionDefinition(nameof(PostgresCollection), DisableParallelization = true)]
public sealed class PostgresCollection : ICollectionFixture<PostgresContainerFixture>;

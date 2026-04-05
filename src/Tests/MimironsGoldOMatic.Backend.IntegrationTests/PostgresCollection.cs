using MimironsGoldOMatic.IntegrationTesting;
using Xunit;

namespace MimironsGoldOMatic.Backend.IntegrationTests;

/// <summary>
/// xUnit discovers <see cref="ICollectionFixture{T}"/> definitions in the test assembly only; bind the shared container here.
/// </summary>
[CollectionDefinition(nameof(PostgresCollection), DisableParallelization = true)]
public sealed class PostgresCollection : ICollectionFixture<PostgresContainerFixture>;

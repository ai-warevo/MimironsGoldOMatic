using MimironsGoldOMatic.IntegrationTesting;
using Xunit;

namespace MimironsGoldOMatic.Desktop.IntegrationTests;

/// <summary>
/// xUnit requires the <see cref="ICollectionFixture{T}"/> definition in the **same assembly** as Desktop integration tests
/// (see xUnit1041). Reuses <see cref="PostgresContainerFixture"/> from <c>MimironsGoldOMatic.IntegrationTesting</c>.
/// </summary>
[CollectionDefinition(nameof(DesktopIntegrationPostgresCollection), DisableParallelization = true)]
public sealed class DesktopIntegrationPostgresCollection : ICollectionFixture<PostgresContainerFixture>;

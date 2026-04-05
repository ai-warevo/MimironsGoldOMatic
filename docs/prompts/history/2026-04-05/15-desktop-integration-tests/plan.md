# Plan

- Add `MimironsGoldOMatic.IntegrationTesting` library: shared `BackendWebApplicationFactory`, `PostgresContainerFixture`, `PostgresMgmTruncate`, `HttpApiFixtureBase`, `IntegrationTestConstants`.
- Refactor Backend.IntegrationTests to reference the library; keep `PostgresCollection` definition in Backend test assembly for xUnit discovery.
- Add `MimironsGoldOMatic.Desktop.IntegrationTests` (`net10.0`) with linked Desktop EBS sources; `DesktopIntegrationPostgresCollection`; tests via `EbsDesktopClient` against the factory host.
- Fix `ConnectionStrings:PostgreSQL` override with `WebHostBuilder.UseSetting` so tests never hit a local dev database.
- Solution + README updates.

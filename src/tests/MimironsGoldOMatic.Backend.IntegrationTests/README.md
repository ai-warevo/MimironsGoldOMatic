# MimironsGoldOMatic.Backend.IntegrationTests

End-to-end Backend tests: **Testcontainers PostgreSQL**, real **Marten** persistence, **ASP.NET Core** via `WebApplicationFactory<Program>`, and **MediatR** / services where tests target handlers directly.

Shared infrastructure (`BackendWebApplicationFactory`, Postgres fixture, `PostgresMgmTruncate`, `HttpApiFixtureBase`) lives in **`MimironsGoldOMatic.IntegrationTesting`** and is reused by **`MimironsGoldOMatic.Desktop.IntegrationTests`** (Desktop EBS client vs the same host).

## Prerequisites

- **Docker** running locally (Testcontainers starts `postgres:16-alpine`).
- **.NET SDK** matching the solution (see repo `global.json` / CI `DOTNET_VERSION`).

No Twitch credentials are required: the test host overrides configuration (empty `Twitch:EventSubSecret` for signature bypass, dev Extension JWT signing, `Mgm:DevSkipSubscriberCheck`, and a fixed Desktop `Mgm:ApiKey`).

## Environment variables

| Variable | Purpose |
|----------|---------|
| `DOCKER_HOST` | Optional; set if Docker is not on the default socket (remote Docker, Colima, etc.). |
| `TESTCONTAINERS_RYUK_DISABLED` | Optional; set to `true` only if your environment cannot run Ryuk (not recommended). |

Typical local runs need **no** extra variables.

## How to run

From the repository root:

```bash
dotnet test src/tests/MimironsGoldOMatic.Backend.IntegrationTests/MimironsGoldOMatic.Backend.IntegrationTests.csproj
```

Integration-only filter (same project; all tests here are integration-style):

```bash
dotnet test src/tests/MimironsGoldOMatic.Backend.IntegrationTests/MimironsGoldOMatic.Backend.IntegrationTests.csproj --filter "Category=Integration"
```

From `src/` using the solution:

```bash
dotnet test MimironsGoldOMatic.slnx --filter "FullyQualifiedName~MimironsGoldOMatic.Backend.IntegrationTests"
```

Performance smoke tests are tagged `Kind=Performance` (see `CriticalPathPerformanceIntegrationTests`; timing assertion, not BenchmarkDotNet).

## Isolation and cleanup

- **One shared Postgres container** per test collection (`PostgresCollection`, defined in this assembly as `ICollectionFixture<PostgresContainerFixture>` from `MimironsGoldOMatic.IntegrationTesting`); **xunit.runner.json** forces **single-threaded** execution so DB state does not race between tests.
- **`PostgresMgmTruncate`** runs before/after host boot (`HttpApiFixtureBase`) and again via **`CreateCleanClientAsync()`** for HTTP scenarios.
- JWT / EventSub / pool rows use **unique IDs** where collisions would otherwise occur across the ordered suite.

## Coverage (optional)

```bash
dotnet test src/tests/MimironsGoldOMatic.Backend.IntegrationTests/MimironsGoldOMatic.Backend.IntegrationTests.csproj \
  --configuration Release \
  --settings src/tests/MimironsGoldOMatic.Backend.IntegrationTests/coverlet.runsettings \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults/coverage-integration
```

## Logging

Integration host uses **Debug** minimum level for Microsoft and application categories. Run with `--logger "console;verbosity=detailed"` to see more output in the console.

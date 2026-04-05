# MimironsGoldOMatic.Desktop.IntegrationTests

End-to-end checks that the **Desktop HTTP client** (`EbsDesktopClient` / `IEbsDesktopClient`) works against a **real ASP.NET Core Backend** host and **PostgreSQL** (Testcontainers), matching `docs/SPEC.md` and `docs/INTERACTION_SCENARIOS.md` (Desktop API key, pending polling, verify-candidate, patch/confirm).

## How this project is wired

- **No WPF UI** is started. The same C# sources as the Desktop app for the EBS client are **linked** into this `net10.0` assembly so Linux CI can compile them without referencing the WPF `.csproj`.
- **Shared test host** lives in `MimironsGoldOMatic.IntegrationTesting` (`BackendWebApplicationFactory`, Postgres fixture, truncate helper) and is also used by `MimironsGoldOMatic.Backend.IntegrationTests`.

## Scenarios covered

| Area | Tests |
|------|--------|
| Authentication / “login” (base URL + `X-MGM-ApiKey`) | `DesktopAuthenticationFlowIntegrationTests` |
| Polling / refresh (GET pending) + cache backing state | `DesktopPayoutPollingAndSyncIntegrationTests` |
| Patch / confirm / verify-candidate / error recovery | `DesktopPatchConfirmErrorRecoveryIntegrationTests` |

“UI state” here means **view backing data**: `PayoutSnapshotCache` after a successful pending fetch, as used for `[MGM_ACCEPT:…]` resolution in the real app.

## Prerequisites

- **Docker** (same as Backend integration tests): Testcontainers runs `postgres:16-alpine`.
- **.NET 10 SDK**.

## Run

From the repository root:

```bash
dotnet test src/tests/MimironsGoldOMatic.Desktop.IntegrationTests/MimironsGoldOMatic.Desktop.IntegrationTests.csproj
```

Filter:

```bash
dotnet test src/tests/MimironsGoldOMatic.Desktop.IntegrationTests/MimironsGoldOMatic.Desktop.IntegrationTests.csproj --filter "Category=Integration"
```

With the solution (Backend + Desktop integration together on a Docker-capable agent):

```bash
dotnet test src/MimironsGoldOMatic.slnx --filter "FullyQualifiedName~MimironsGoldOMatic.Desktop.IntegrationTests"
```

## Configuration

The integration host sets `Mgm:ApiKey` to `IntegrationTestConstants.DesktopApiKey` in `MimironsGoldOMatic.IntegrationTesting` — keep Desktop client tests aligned with that constant.

The shared `BackendWebApplicationFactory` uses `WebHostBuilder.UseSetting("ConnectionStrings:PostgreSQL", …)` so the in-test host always uses the Testcontainers connection string (not `appsettings.Development.json` on your machine).

Tests that need a clean database call `ResetDatabaseAndRestartHostAsync()` (truncates `mgm` and disposes/recreates the Kestrel factory) before exercising the Desktop client.

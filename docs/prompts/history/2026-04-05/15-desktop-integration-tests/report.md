# Report

## Added

- `src/tests/MimironsGoldOMatic.IntegrationTesting/` — shared integration host + Postgres fixture + truncate + `HttpApiFixtureBase` + `ResetDatabaseAndRestartHostAsync`.
- `src/tests/MimironsGoldOMatic.Desktop.IntegrationTests/` — 9 tests, linked `EbsDesktopClient` / API DTOs / `PayoutSnapshotCache`, `README.md`.
- `src/tests/MimironsGoldOMatic.Backend.IntegrationTests/PostgresCollection.cs` — xUnit collection definition (fixture type from IntegrationTesting).
- `docs/prompts/history/2026-04-05/15-desktop-integration-tests/*`

## Modified

- Backend.IntegrationTests: removed inlined factory/fixture/truncate; project reference + usings; README note.
- `BackendWebApplicationFactory`: `UseSetting(ConnectionStrings:PostgreSQL)` so Testcontainers DB wins over appsettings.

## Verification

- `dotnet test ...Backend.IntegrationTests` — 44 passed.
- `dotnet test ...Desktop.IntegrationTests` — 9 passed.

## Notes

- Full WPF UI is not exercised; “UI state” is `PayoutSnapshotCache` after `GetPendingAsync`.
- Desktop integration uses `net10.0` + linked sources so it runs on the same Linux + Docker CI job as Backend integration.

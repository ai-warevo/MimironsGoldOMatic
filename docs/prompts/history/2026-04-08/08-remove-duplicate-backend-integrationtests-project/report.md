## Summary

Removed the duplicate root-level integration test project `src/MimironsGoldOMatic.Backend.IntegrationTests` and kept the active test project under `src/Tests/MimironsGoldOMatic.Backend.IntegrationTests`.

## Modified files

- `src/MimironsGoldOMatic.sln`
- `src/MimironsGoldOMatic.Backend.IntegrationTests/MimironsGoldOMatic.Backend.IntegrationTests.csproj` (deleted)
- `src/MimironsGoldOMatic.Backend.IntegrationTests/UnitTest1.cs` (deleted)
- `docs/prompts/history/2026-04-08/08-remove-duplicate-backend-integrationtests-project/*`

## Verification

- `dotnet build src/MimironsGoldOMatic.sln -c Debug` -> success (`0 warnings`, `0 errors`).

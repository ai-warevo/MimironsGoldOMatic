## Summary

Resolved the integration-test runtime failure and the remaining package warnings by moving backend projects to the current Marten line and aligning dependent API/package references (`Npgsql` and `StreamIdentity` namespace changes).

## Modified files

- `src/MimironsGoldOMatic.Backend.DataAccess/MimironsGoldOMatic.Backend.DataAccess.csproj`
- `src/MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.csproj`
- `src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.csproj`
- `src/MimironsGoldOMatic.Backend.DataAccess/Persistence/MgmMartenDocumentConfiguration.cs`
- `src/MimironsGoldOMatic.Backend/Persistence/MgmMartenDocumentConfiguration.cs`
- `src/MimironsGoldOMatic.Backend.Infrastructure/DependencyInjection/BackendCompositionExtensions.cs`
- `src/Tests/MimironsGoldOMatic.Backend.IntegrationTests/MimironsGoldOMatic.Backend.IntegrationTests.csproj`
- `src/Tests/MimironsGoldOMatic.IntegrationTesting/MimironsGoldOMatic.IntegrationTesting.csproj`
- `docs/prompts/history/2026-04-08/06-fix-marten-jasperfx-tests/*`

## Verification

- `dotnet build src/Tests/MimironsGoldOMatic.Backend.IntegrationTests/MimironsGoldOMatic.Backend.IntegrationTests.csproj -c Debug` -> success.
- `dotnet test src/Tests/MimironsGoldOMatic.Backend.IntegrationTests/MimironsGoldOMatic.Backend.IntegrationTests.csproj -c Debug --no-build` -> passed (`44/44`).
- `dotnet test src/Tests/MimironsGoldOMatic.Desktop.IntegrationTests/MimironsGoldOMatic.Desktop.IntegrationTests.csproj -c Debug` -> passed (`9/9`).
- Build output after these changes is clean in this path (`0 warnings`, `0 errors`) for backend integration build.

## Remaining notes

- No additional blockers observed in this validation run.

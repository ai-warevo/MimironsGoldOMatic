## Summary

Removed the unused `MimironsGoldOMatic.Backend.Cli` stub project from the solution and deleted its project files.

## Modified files

- `src/MimironsGoldOMatic.sln`
- `src/MimironsGoldOMatic.Backend.Cli/MimironsGoldOMatic.Backend.Cli.csproj` (deleted)
- `src/MimironsGoldOMatic.Backend.Cli/Program.cs` (deleted)
- `docs/prompts/history/2026-04-08/07-remove-backend-cli-stub/*`

## Verification

- `dotnet build src/MimironsGoldOMatic.sln -c Debug` -> success (`0 warnings`, `0 errors`).

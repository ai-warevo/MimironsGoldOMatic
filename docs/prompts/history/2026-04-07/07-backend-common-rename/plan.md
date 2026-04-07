# Plan

1. Rename directory and `.csproj` to `MimironsGoldOMatic.Backend.Common`.
2. Replace `MimironsGoldOMatic.Backend.Shared` with `MimironsGoldOMatic.Backend.Common` in source, solutions, Docker, and active docs (bulk text replace; does not affect `MimironsGoldOMatic.Shared`).
3. Refresh stray comments (`Backend.Common` in composition DI).
4. `dotnet build src/MimironsGoldOMatic.slnx`.

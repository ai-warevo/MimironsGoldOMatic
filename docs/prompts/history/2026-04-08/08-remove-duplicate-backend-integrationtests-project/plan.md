## Goal

Remove the duplicate legacy test project under `src/MimironsGoldOMatic.Backend.IntegrationTests` and keep only the active test project under `src/Tests/MimironsGoldOMatic.Backend.IntegrationTests`.

## Steps

1. Remove duplicate project entry from `src/MimironsGoldOMatic.sln`.
2. Remove duplicate project GUID configuration mappings.
3. Delete files under `src/MimironsGoldOMatic.Backend.IntegrationTests/`.
4. Build solution to confirm there are no remaining dependencies.

## Risk

Low risk. The active test suite is in `src/Tests/...` and remains in the solution.

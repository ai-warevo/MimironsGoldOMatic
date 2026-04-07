## Goal

Remove the unused placeholder project `MimironsGoldOMatic.Backend.Cli`.

## Steps

1. Remove the project entry and configuration mappings from `src/MimironsGoldOMatic.sln`.
2. Delete the project files under `src/MimironsGoldOMatic.Backend.Cli/`.
3. Build the solution to verify no references remain.

## Risk

Low risk. Project is a standalone stub with no inbound project references.

## Goal

Stabilize backend integration tests by resolving the Marten/JasperFx runtime incompatibility that throws `MissingMethodException` for `CodeFileExtensions.InitializeSynchronously(...)`.

## Approach

1. Remove explicit `JasperFx.RuntimeCompiler` pin from `MimironsGoldOMatic.Backend.DataAccess.csproj` so Marten's compatible transitive dependency set is used.
2. Rebuild and run backend integration tests.
3. If failures remain, inspect the next dominant error and apply a minimal follow-up fix.

## Files expected

- `src/MimironsGoldOMatic.Backend.DataAccess/MimironsGoldOMatic.Backend.DataAccess.csproj`
- `docs/prompts/history/2026-04-08/06-fix-marten-jasperfx-tests/checks.md`
- `docs/prompts/history/2026-04-08/06-fix-marten-jasperfx-tests/report.md`

## Risks

- Additional package mismatches may surface after the first fix.
- Integration tests depend on Docker/Testcontainers availability.

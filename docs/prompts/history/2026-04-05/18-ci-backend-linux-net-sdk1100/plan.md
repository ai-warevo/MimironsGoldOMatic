# Plan

1. Stop restoring/testing the full solution on Linux for the Backend job — Desktop and Desktop test projects are Windows-only (WPF); Desktop is already covered by `test-desktop` on `windows-latest`.
2. Restore and run `dotnet test` only for:
   - `MimironsGoldOMatic.Backend.UnitTests`
   - `MimironsGoldOMatic.Backend.IntegrationTests`
3. Use separate `dotnet restore` / `dotnet test` invocations per project (CLI accepts one project per command).
4. Emit two TRX files: `backend-unit-tests.trx`, `backend-integration-tests.trx` under `TestResults/backend`.

Risks: none — aligns job scope with runnable targets on Ubuntu.

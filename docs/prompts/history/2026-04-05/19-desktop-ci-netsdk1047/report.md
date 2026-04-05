# Report

## Modified files

- `.github/workflows/unit-integration-tests.yml` — Desktop job: one restore with `-r win-x64` on the unit test project; comment explains overwrite issue.

## Verification

- Local clean restore + Release `dotnet test` on Desktop.UnitTests: 48 tests passed.

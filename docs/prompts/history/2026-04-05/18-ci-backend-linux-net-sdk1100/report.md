# Report

## Modified files

- `.github/workflows/unit-integration-tests.yml` — Backend job restores/tests only Backend unit + integration test projects; header comment updated.

## Verification

- Local: `dotnet restore` + `dotnet test` on each backend test project (Release): unit 58 passed, integration 44 passed.

## Notes

- `dotnet test` / `dotnet restore` accept only one project per invocation (.NET SDK 10.0.201); two TRX files replace single `backend-tests.trx`.

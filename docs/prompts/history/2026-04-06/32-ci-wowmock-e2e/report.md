## Summary

Added a dedicated CI workflow and Python E2E suite to validate `WoWMock`
behavior (chat log writes, MGM-tag flows, `/run` capture) on both
`ubuntu-latest` and `windows-latest`, with a health endpoint and artifact
upload.

## Modified files

- `src/Mocks/WoWMock/Api/MockController.cs` (add `GET /api/mock/health`)

## Added files

- `.github/workflows/e2e-wowmock-desktop.yml`
- `.github/scripts/e2e/requirements.txt`
- `.github/scripts/e2e/README.md`
- `.github/scripts/e2e/lib.py`
- `.github/scripts/e2e/conftest.py`
- `.github/scripts/e2e/test_wowmock_e2e.py`
- `.github/scripts/e2e/run_e2e.py`
- `.github/scripts/e2e/appsettings.Test.json`
- `.github/scripts/e2e/testsettings.json`

## Verification

- `dotnet build src/Mocks/WoWMock/MimironsGoldOMatic.Mocks.WoWMock.csproj`
- `python .github/scripts/e2e/run_e2e.py` (6 passing tests, emits JUnit XML)

## Potential technical debt

- Desktop↔WoWMock “true” WinAPI-driven flow is not executed in CI yet;
  current E2E suite validates the WoWMock contract and log behavior.
  If/when Desktop adds a CI-friendly “test mode” injection path, expand
  the Windows job to run full Desktop-driven end-to-end scenarios.


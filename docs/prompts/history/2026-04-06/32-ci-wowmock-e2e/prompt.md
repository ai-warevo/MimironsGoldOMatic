## Request

Update GitHub Actions workflows to include end-to-end (E2E) testing of integration between:

- WPF Desktop app: `src/MimironsGoldOMatic.Desktop/MimironsGoldOMatic.Desktop.csproj`
- WoW mock service: `src/Mocks/WoWMock/MimironsGoldOMatic.Mocks.WoWMock.csproj`

## Requirements (summary)

- Build Desktop and WoWMock in CI.
- Start WoWMock as a background service (port 5001).
- Orchestrate E2E via Python 3.8+ scripts under `.github/scripts/e2e/`.
- Cross-platform: workflow should run on `ubuntu-latest` and `windows-latest`.
- Reliability: retries, health checks (`/api/mock/health`), cleanup.
- Artifacts: upload test logs/results.
- Provide config files:
  - `.github/scripts/e2e/testsettings.json` (Desktop)
  - `.github/scripts/e2e/appsettings.Test.json` (WoWMock)
- Provide `.github/scripts/e2e/README.md` with local run instructions.

## Scenarios (high-level)

- Happy path: successful payout flow with confirmation.
- Error: command timeout (WoWMock delays).
- Robustness: malformed/unexpected log messages.
- Recovery: restart Desktop during payout; resume and complete.


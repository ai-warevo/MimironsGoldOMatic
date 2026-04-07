## Goal

Add CI automation that runs a WoWMock-backed E2E verification suite on
Windows and Linux runners, using Python orchestration scripts located in
`.github/scripts/e2e/`, with robust startup/health checks and artifact
collection.

## Approach

- Add a WoWMock health endpoint `GET /api/mock/health` for CI readiness.
- Add Python runner + pytest suite that:
  - Starts WoWMock via `dotnet run` with test config overrides.
  - Waits for `/api/mock/health` with retry/backoff.
  - Exercises required behaviors via the existing mock endpoints:
    - reset state
    - add-message (including `/run ...`)
    - set-response (delay + auto-confirm)
    - commands list verification
  - Validates `WoWChatLog.txt` writes, MGM tag flows, robustness to
    malformed entries.
  - Produces structured logs and a JUnit XML (pytest) for CI.
- Add a new GitHub Actions workflow that runs on a matrix:
  - `ubuntu-latest`: build Desktop (with Windows targeting enabled),
    build WoWMock, run python WoWMock E2E suite.
  - `windows-latest`: build Desktop + WoWMock, run same python suite,
    plus optional Desktop smoke start (best-effort) if feasible.
- Upload artifacts (logs, WoWChatLog.txt, pytest report).

## Files to change/add

- `src/Mocks/WoWMock/Api/MockController.cs` (add `/health` endpoint)
- `.github/workflows/e2e-wowmock-desktop.yml` (new)
- `.github/scripts/e2e/**` (new scripts, tests, configs, README)

## Risks / mitigations

- WPF execution on Linux: only build on Linux; run runtime E2E on
  Windows. On Linux, verify WoWMock behavior and Desktop buildability.
- Flaky service startup: health endpoint + retries; ensure cleanup via
  finally blocks and workflow `if: always()` stop steps.


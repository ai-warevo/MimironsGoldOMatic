## WoWMock E2E (CI orchestration)

This folder contains Python scripts used by GitHub Actions to run a
portable E2E-style verification of `WoWMock` behavior and (on Windows)
smoke the Desktop/WoW log integration.

### Prerequisites (local)

- **Python** 3.8+
- **.NET SDK** (repo uses `10.0.x` in CI)

### Install dependencies

From repo root:

```bash
python -m pip install -r .github/scripts/e2e/requirements.txt
```

### Run the suite locally

```bash
python .github/scripts/e2e/run_e2e.py
```

You can override paths/ports with environment variables:

- `WOWMOCK_E2E_PORT` (default 5001)
- `WOWMOCK_E2E_LOG_DIR` (default `.e2e-artifacts/logs`)

### What it does

- starts `WoWMock` (`dotnet run`) in the background
- waits for `GET /api/mock/health`
- runs pytest scenarios that verify:
  - chat log creation + real-time append
  - `/run` command capture
  - auto-confirm behavior after `[MGM_ACCEPT:...]`
  - robustness to malformed lines
- writes logs to `.e2e-artifacts/` for upload in CI


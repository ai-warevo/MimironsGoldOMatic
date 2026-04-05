# Plan (2026-04-05)

1. **Backend:** Add `TwitchOptions.HelixApiBaseUrl`; register `HttpClient("Helix")` with `BaseAddress`; `HelixChatService` POST relative `helix/chat/messages`. Fix unit tests with `BaseAddress`.
2. **CI harness:** `MgmOptions.EnableE2eHarness` + `POST /api/e2e/prepare-pending-payout` (Development + ApiKey) to avoid roulette wall-clock blocking Pending in CI.
3. **Workflow:** Build/run MockHelixApi (9053) and SyntheticDesktop (9054) after Tier A mocks; Backend env for Helix + harness; `pip install` + `check_workflow_integration.py`; `run_e2e_tier_b.py` after Tier A gate.
4. **Mocks:** SyntheticDesktop `Results.Json(..., apiJson)` for camelCase `ok`.
5. **Scripts:** `run_e2e_tier_b.py`; extend `send_e2e_eventsub.py` with optional `--probe-mock-helix`.
6. **Docs:** E2E plan (Tier B results, troubleshooting), TIER_B_* files, POSTLAUNCH verification.

**Risks:** Roulette synchronizer racing harness (low probability); wrong `HelixApiBaseUrl` including `/helix` path.

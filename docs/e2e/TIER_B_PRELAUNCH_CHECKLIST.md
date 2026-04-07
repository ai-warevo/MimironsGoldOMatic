<!-- Updated: 2026-04-05 (Tier B integration & first run) -->

# Tier B E2E — pre-launch checklist

Use this list before merging **CI Tier B** changes into [`.github/workflows/e2e-test.yml`](../../.github/workflows/e2e-test.yml) and before declaring the first green **Tier B** run. **Normative plan:** [`docs/e2e/E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md) (**[Tier B Readiness Verification](E2E_AUTOMATION_PLAN.md#tier-b-readiness-verification)**, **[First Run Guide](E2E_AUTOMATION_PLAN.md#tier-b-first-run-guide)**). **Tasks:** [`docs/e2e/TIER_B_IMPLEMENTATION_TASKS.md`](TIER_B_IMPLEMENTATION_TASKS.md). **Post-merge:** [`docs/e2e/TIER_B_POSTLAUNCH_VERIFICATION.md`](TIER_B_POSTLAUNCH_VERIFICATION.md).

---

## Implementation

- [x] **A1–A3:** Configurable Helix base URL in Backend ([`HelixChatService`](../../src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Services/HelixChatService.cs), [`TwitchOptions`](../../src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Abstract/Configuration/TwitchOptions.cs), docs/appsettings).
- [x] **MockHelixApi** project created and builds (`src/Mocks/MockHelixApi/`).
- [x] **SyntheticDesktop** project created and builds (`src/Mocks/SyntheticDesktop/`).
- [x] **`GET /health`** on **MockHelixApi** returns **`status: healthy`**, **`component: MockHelixApi`**.
- [x] **`GET /health`** on **SyntheticDesktop** returns **`status: healthy`**, **`component: SyntheticDesktop`**.
- [x] **`python3 .github/scripts/tier_b_verification/check_mockhelixapi.py`** passes against running **MockHelixApi** (local/CI).
- [x] **`python3 .github/scripts/tier_b_verification/check_syntheticdesktop.py`** passes (health); **`--payout-id`** path covered by **CI Tier B** orchestrator.
- [x] **`python3 .github/scripts/tier_b_verification/check_workflow_integration.py`** passes with all services up (no **`--skip-tier-b`**) in **Actions** (requires **`pip install -r .github/scripts/tier_b_verification/requirements.txt`**).
- [x] All services start without port conflicts (**8080**, **9051**–**9054**, **5432** — see [port map](E2E_AUTOMATION_PLAN.md#workflow-integration-ports--order)).
- [x] **D1–D3:** Workflow starts Tier B mocks, sets Backend Twitch env + **`Mgm__EnableE2eHarness`**, asserts **Helix** capture + pool rule ([`TIER_B_IMPLEMENTATION_TASKS.md`](TIER_B_IMPLEMENTATION_TASKS.md)).

---

## Documentation

- [x] Operators know to `pip install -r .github/scripts/tier_b_verification/requirements.txt` before running verification scripts locally (workflow installs automatically).
- [x] **Tier B First Run** / **Integration Results** documented in [`docs/e2e/E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md).

---

## Document control

| Version | Date | Note |
|---------|------|------|
| 1.0 | 2026-04-05 | Initial checklist aligned with readiness scripts and mock projects |
| 1.1 | 2026-04-05 | Pre-launch items marked complete after workflow + harness integration |

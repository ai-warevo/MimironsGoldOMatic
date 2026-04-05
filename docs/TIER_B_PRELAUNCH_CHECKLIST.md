<!-- Updated: 2026-04-05 (Tier B readiness preparation) -->

# Tier B E2E — pre-launch checklist

Use this list before merging **CI Tier B** changes into [`.github/workflows/e2e-test.yml`](.github/workflows/e2e-test.yml) and before declaring the first green **Tier B** run. **Normative plan:** [`docs/E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md) (**[Tier B Readiness Verification](E2E_AUTOMATION_PLAN.md#tier-b-readiness-verification)**, **[First Run Guide](E2E_AUTOMATION_PLAN.md#tier-b-first-run-guide)**). **Tasks:** [`docs/TIER_B_IMPLEMENTATION_TASKS.md`](TIER_B_IMPLEMENTATION_TASKS.md).

---

## Implementation

- [ ] **A1–A3:** Configurable Helix base URL in Backend ([`HelixChatService`](../src/MimironsGoldOMatic.Backend/Services/HelixChatService.cs), [`TwitchOptions`](../src/MimironsGoldOMatic.Backend/Configuration/TwitchOptions.cs), docs/appsettings).
- [ ] **MockHelixApi** project created and builds (`src/Mocks/MockHelixApi/`).
- [ ] **SyntheticDesktop** project created and builds (`src/Mocks/SyntheticDesktop/`).
- [ ] **`GET /health`** on **MockHelixApi** returns **`status: healthy`**, **`component: MockHelixApi`**.
- [ ] **`GET /health`** on **SyntheticDesktop** returns **`status: healthy`**, **`component: SyntheticDesktop`**.
- [ ] **`python3 scripts/tier_b_verification/check_mockhelixapi.py`** passes against running **MockHelixApi**.
- [ ] **`python3 scripts/tier_b_verification/check_syntheticdesktop.py`** passes (health only is enough until a **`Pending`** payout exists).
- [ ] **`python3 scripts/tier_b_verification/check_syntheticdesktop.py --payout-id <GUID>`** passes with seeded **`Pending`** payout and matching **`characterName`**.
- [ ] **`python3 scripts/tier_b_verification/check_workflow_integration.py`** passes with all services up (no **`--skip-tier-b`**).
- [ ] All services start without port conflicts (**8080**, **9051**–**9054**, **5432** — see [port map](E2E_AUTOMATION_PLAN.md#workflow-integration-ports--order)).
- [ ] **D1–D3:** Workflow starts Tier B mocks, sets Backend Twitch env, asserts **Helix** capture + pool rule (tracked on [`TIER_B_IMPLEMENTATION_TASKS.md`](TIER_B_IMPLEMENTATION_TASKS.md)).

---

## Documentation

- [ ] Operators know to `pip install -r scripts/tier_b_verification/requirements.txt` before running verification scripts.
- [ ] **Tier B First Run** rehearsed once per [`docs/E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md) (or blocked items documented).

---

## Document control

| Version | Date | Note |
|---------|------|------|
| 1.0 | 2026-04-05 | Initial checklist aligned with readiness scripts and mock projects |

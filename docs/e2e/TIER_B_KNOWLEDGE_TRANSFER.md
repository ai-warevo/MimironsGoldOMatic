<!-- Updated: 2026-04-06 (Transition complete & Tier C launch) -->

# Tier B — knowledge transfer (demo script + environment checklist)

**Primary docs:** [`TIER_B_HANDOVER.md`](TIER_B_HANDOVER.md), [`E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md), [`TIER_B_MAINTENANCE_CHECKLIST.md`](TIER_B_MAINTENANCE_CHECKLIST.md)

---

## 1. Session goal (45–60 min)

Enable maintainers to:
- Understand the Tier B CI architecture (mocks + harness).
- Debug common failures using artifacts and the troubleshooting matrix.
- Operate monitoring and respond to the consecutive-failure alert.
- Know what Tier C adds and what remains out-of-scope for Tier B CI.

---

## 2. Demo script (suggested flow)

### Part A — Architecture overview (5–10 min)

- Open `docs/e2e/TIER_B_HANDOVER.md` and walk through:
  - Components + ports table
  - Mermaid sequence diagram (Backend ↔ SyntheticDesktop ↔ MockHelixApi)
  - Troubleshooting matrix (where to look first)

### Part B — Show the workflow and the Tier B boundary (10–15 min)

- Open `.github/workflows/e2e-test.yml`.
- Call out:
  - Tier A gate: `send_e2e_eventsub.py` → `GET /api/pool/me`
  - Tier B step: `run_e2e_tier_b.py`
  - **Job summary**: “E2E performance (this run)” table (total wall time + Tier B boundary time)
  - Always-on artifact upload: `e2e-service-logs`

### Part C — Show MockHelixApi + SyntheticDesktop interaction (10–15 min)

- Explain the critical assertion points:
  - `GET http://127.0.0.1:9053/last-request` proves Helix send was attempted and captured
  - `GET http://127.0.0.1:9054/last-run` proves Desktop-equivalent choreography succeeded
- Tie these back to:
  - Backend config `Twitch:HelixApiBaseUrl` (root URL, not `.../helix`)
  - Desktop API key header `X-MGM-ApiKey`

### Part D — Log artifacts and post-mortem workflow (10 min)

- From a failing run (or a saved artifact), walk through:
  - `mgm-backend.log` (startup and API errors)
  - `mgm-tier-b-orchestrator.log` (Python orchestration and assertions)
  - `mgm-mock-helix.log` (POST capture / auth errors)
  - `mgm-synthetic-desktop.log` (step-by-step status codes)

### Part E — Monitoring + alert response (5–10 min)

- Weekly health report:
  - `.github/workflows/e2e-weekly-health-report.yml` → Summary table (rolling 30‑day stats)
- Consecutive failure alert:
  - `.github/workflows/e2e-consecutive-failure-alert.yml` → opens exactly one issue
  - Response: download artifacts from the **two** linked failed runs, triage, document root cause, close issue when green again

---

## 3. Demo environment checklist

### Minimal requirements (docs-only walkthrough)

- Access to GitHub repo Actions page
- A recent successful `e2e-test.yml` run to open (for Summary + artifact shape)
- A past failing run (optional) or an archived artifact for log walkthrough

### Local runnable environment (optional)

If you want to run the Tier B flow locally (mirrors CI):

- **Software**
  - .NET SDK (per repo standard)
  - Python 3 + pip
  - PostgreSQL 16 (Docker or local install)
- **Services and ports**
  - Backend: `http://127.0.0.1:8080`
  - MockEventSubWebhook: `:9051`
  - MockExtensionJwt: `:9052`
  - MockHelixApi: `:9053`
  - SyntheticDesktop: `:9054`
- **Config**
  - `ASPNETCORE_ENVIRONMENT=Development`
  - `Mgm:EnableE2eHarness=true`
  - `Mgm:ApiKey` set consistently for Backend + SyntheticDesktop + orchestrator
  - `Twitch:HelixApiBaseUrl=http://127.0.0.1:9053`
  - Dummy `Twitch:BroadcasterAccessToken`, `Twitch:BroadcasterUserId`, `Twitch:HelixClientId` (non-empty)
- **Verification scripts**
  - `pip install -r .github/scripts/tier_b_verification/requirements.txt`
  - Run `python3 .github/scripts/tier_b_verification/check_workflow_integration.py`
  - Run `python3 .github/scripts/run_e2e_tier_b.py --api-key <key>`

---

## 4. Scheduling / invite template (fill in)

- **Title:** Tier B E2E — maintainer knowledge transfer
- **Duration:** 60 min
- **Presenter:** Anatoly Ivanov (`ai.vibeqodez@vk.com`)
- **Audience:** CI maintainers, Backend maintainers, future Tier C implementers
- **Proposed time:** *TBD*
- **Meeting link:** *TBD*
- **Preread:**
  - `docs/e2e/TIER_B_HANDOVER.md`
  - `docs/e2e/TIER_B_MAINTENANCE_CHECKLIST.md`
  - `docs/e2e/TIER_C_KICKOFF_PLAN.md`

### Live demo prep (if doing a live Actions walkthrough)

- Pick a **known green** `e2e-test.yml` run URL ahead of time.
- Pick a **known red** run URL (or download and save `e2e-service-logs`) for artifact walkthrough.
- Have the port matrix handy: **8080 / 9051–9054**.

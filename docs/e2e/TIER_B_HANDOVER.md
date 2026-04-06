<!-- Updated: 2026-04-06 (Transition complete & Tier C launch) -->

# Tier B — maintainer handover

**Parent:** [`E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md) · **Issue tracking:** [GitHub issue #16](https://github.com/ai-warevo/MimironsGoldOMatic/issues/16) · **Structure map:** [`PROJECT_STRUCTURE.md`](../reference/PROJECT_STRUCTURE.md) · **Recurring tasks:** [`TIER_B_MAINTENANCE_CHECKLIST.md`](TIER_B_MAINTENANCE_CHECKLIST.md)

This package summarizes **CI Tier B** (Linux-hosted mocks, no real WoW) so new maintainers can operate, extend, and debug the pipeline without re-reading the full planning history.

---

## 1. Architecture overview

### Components

| Role | Project / path | Loopback port (CI) |
|------|----------------|--------------------|
| **EBS** (Development + E2E harness) | [`src/MimironsGoldOMatic.Backend/`](../../src/MimironsGoldOMatic.Backend/) | **8080** |
| **MockEventSubWebhook** | [`src/Mocks/MockEventSubWebhook/`](../../src/Mocks/MockEventSubWebhook/) | **9051** |
| **MockExtensionJwt** | [`src/Mocks/MockExtensionJwt/`](../../src/Mocks/MockExtensionJwt/) | **9052** |
| **MockHelixApi** | [`src/Mocks/MockHelixApi/`](../../src/Mocks/MockHelixApi/) | **9053** |
| **SyntheticDesktop** | [`src/Mocks/SyntheticDesktop/`](../../src/Mocks/SyntheticDesktop/) | **9054** |
| **PostgreSQL** | service container `postgres:16-alpine` | **5432** |

### Control flow (Tier B slice)

SyntheticDesktop performs the same REST sequence as [`MimironsGoldOMatic.Desktop`](../../src/MimironsGoldOMatic.Desktop/): **confirm-acceptance** → **PATCH** `InProgress` → **PATCH** `Sent`. On **`Sent`**, Backend calls **Helix Send Chat Message**; in CI, **`Twitch:HelixApiBaseUrl`** points at **MockHelixApi**, which records **`POST`** bodies for assertions.

```mermaid
sequenceDiagram
    participant WF as e2e-test.yml job
    participant EBS as Backend :8080
    participant SD as SyntheticDesktop :9054
    participant HX as MockHelixApi :9053
    participant PG as PostgreSQL

    WF->>EBS: POST /api/e2e/prepare-pending-payout (harness)
    WF->>SD: POST /run-sequence (orchestrator)
    SD->>EBS: POST confirm-acceptance
    SD->>EBS: PATCH payout InProgress
    SD->>EBS: PATCH payout Sent
    EBS->>PG: Marten / pool removal
    EBS->>HX: POST helix/chat/messages
    WF->>HX: GET /last-request (assert)
    WF->>SD: GET /last-run (assert)
```

### Component interaction (static)

```text
                    ┌─────────────────────┐
  MockEventSub ───► │      Backend        │ ◄─── SyntheticDesktop
   (9051)           │  (8080, Marten,     │       (9054)
                    │   HelixChatService) │
  MockExtensionJwt ─┤                     ├────► MockHelixApi (9053)
   (9052)           │                     │
                    └──────────┬──────────┘
                               │
                         PostgreSQL :5432
```

---

## 2. Local development setup

Mirror [`.github/workflows/e2e-test.yml`](../../.github/workflows/e2e-test.yml) on **Linux/macOS/WSL** or multiple terminals on **Windows**:

1. **PostgreSQL 16** with database **`mgm`**, user/password aligned with Backend `ConnectionStrings:PostgreSQL`.
2. **Scoped build:** Shared → Backend → all mock projects (`dotnet build` `-c Release`), same order as CI.
3. **Environment:** `ASPNETCORE_ENVIRONMENT=Development`, **`Mgm:EnableE2eHarness=true`**, **`Mgm:ApiKey`**, **`Twitch:HelixApiBaseUrl=http://127.0.0.1:9053`**, dummy **`Twitch:*`** token fields (see workflow `env`).
4. Start **Backend**, then **9051–9054** mocks; run [`.github/scripts/tier_b_verification/check_workflow_integration.py`](../../.github/scripts/tier_b_verification/check_workflow_integration.py).
5. **Tier B orchestrator:** [`.github/scripts/run_e2e_tier_b.py`](../../.github/scripts/run_e2e_tier_b.py) with **`--api-key`** matching **`Mgm:ApiKey`**.

Detailed walkthrough: [**Tier B First Run Guide** in `E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md#tier-b-first-run-guide) and [**Setting up Tier B Environment** in the Backend README](../components/backend/ReadME.md).

---

## 3. Configuration guide

| Variable / setting | Where | Purpose |
|--------------------|-------|---------|
| **`Mgm:ApiKey`** | Backend + SyntheticDesktop | Authenticates Desktop/Synthetic routes (`X-MGM-ApiKey`). |
| **`Mgm:EnableE2eHarness`** | Backend (Development only) | Enables **`POST /api/e2e/prepare-pending-payout`**. |
| **`Twitch:HelixApiBaseUrl`** | Backend | **Empty** = production Helix root; CI uses **`http://127.0.0.1:9053`** (mock root, not `.../helix`). |
| **`Twitch:EventSubSecret`** | Backend + MockEventSub + `send_e2e_eventsub.py` | HMAC for synthetic EventSub. |
| **`SyntheticDesktop:BackendBaseUrl`** | SyntheticDesktop | EBS base URL (**`http://127.0.0.1:8080`** in CI). |
| **Ports** | All mock `ASPNETCORE_URLS` | Fixed **9051–9054**, **8080** — change only with coordinated doc updates. |

Source of truth for options: [`TwitchOptions.cs`](../../src/MimironsGoldOMatic.Backend/Configuration/TwitchOptions.cs), Backend [`appsettings.Development.json`](../../src/MimironsGoldOMatic.Backend/appsettings.Development.json) (if present), workflow `env` block.

---

## 4. Troubleshooting matrix

| Symptom | Likely cause | What to do |
|---------|--------------|------------|
| **`404`** on **`prepare-pending-payout`** | Harness off or not Development | Set **`Mgm__EnableE2eHarness`** + **`ASPNETCORE_ENVIRONMENT=Development`**. |
| **`400`** not in pool | Tier A enrollment missing / wrong user id | Run **`send_e2e_eventsub.py`** first; align **`twitchUserId`** with orchestrator. |
| Helix mock never receives POST | **Base URL** includes `/helix`, or tokens empty | Use mock **root** only; ensure Broadcaster token/UserId/ClientId non-empty so **`HelixChatService`** does not skip. |
| **Connection refused** on **905x** | Process not started or wrong order | Start Backend before SyntheticDesktop; verify **`GET /health`** on each mock. |
| **`check_workflow_integration`** timeout | Slow runner / Postgres | Increase wait loops in workflow only after confirming bind time in **`mgm-*.log`** artifacts. |
| Python **`requests`** error | pip deps missing locally | **`pip install -r .github/scripts/tier_b_verification/requirements.txt`**. |

Extended tables: [`E2E_AUTOMATION_PLAN.md` — Tier B Troubleshooting Guide](E2E_AUTOMATION_PLAN.md#tier-b-troubleshooting-guide).

---

## 5. Maintenance procedures

| Task | Guidance |
|------|----------|
| **Update mocks** | Change [`src/Mocks/`](../../src/Mocks/); keep **`GET /health`** and Tier B contract endpoints stable or version the orchestrator (`run_e2e_tier_b.py`). |
| **Change HTTP choreography** | Update **SyntheticDesktop** and [`run_e2e_tier_b.py`](../../.github/scripts/run_e2e_tier_b.py) together; align with [`DesktopPayoutsController`](../../src/MimironsGoldOMatic.Backend/Controllers/DesktopPayoutsController.cs). |
| **Add assertion** | Prefer extending Python orchestrator or Tier B verification scripts under [`.github/scripts/tier_b_verification/`](../../.github/scripts/tier_b_verification/). |
| **Rotate CI-only secrets** | Today inline `env` in workflow; if moving to GitHub **Secrets**, update **MockEventSub** + **`send_e2e_eventsub.py`** + Backend in one PR. |

Full checklist: [`TIER_B_MAINTENANCE_CHECKLIST.md`](TIER_B_MAINTENANCE_CHECKLIST.md). Pipeline deep-dive: [`E2E_AUTOMATION_PLAN.md` — E2E Pipeline Maintenance Guide](E2E_AUTOMATION_PLAN.md#e2e-pipeline-maintenance-guide).

---

## 6. Subject-matter experts (update in-repo)

| Area | SME (name / contact) |
|------|-----------------------|
| **Backend / Marten / Helix integration** | **Anatoly Ivanov** (`ai.vibeqodez@vk.com`) |
| **GitHub Actions / mocks / Python harness** | **Anatoly Ivanov** (`ai.vibeqodez@vk.com`) |
| **WPF Desktop (parity with SyntheticDesktop)** | **Anatoly Ivanov** (`ai.vibeqodez@vk.com`) |
| **WoW addon / `[MGM_*]` tags** | **Anatoly Ivanov** (`ai.vibeqodez@vk.com`) |
| **Twitch Extension / Helix product behavior** | **Anatoly Ivanov** (`ai.vibeqodez@vk.com`) |

---

*Tier B CI does not replace manual **WinAPI + WoW** validation; see [`TIER_C_REQUIREMENTS.md`](TIER_C_REQUIREMENTS.md).*

---

## 7. Retrospective Summary & Lessons Learned

This section summarizes the Tier B implementation retrospective (agenda template: [`TIER_B_TEAM_ANNOUNCEMENT.md`](TIER_B_TEAM_ANNOUNCEMENT.md)).

### Successes

- **MockHelixApi + SyntheticDesktop integration**: exercised the Desktop→EBS→Helix path in CI without real Twitch/WoW.
- **Pipeline optimization**: NuGet + pip caching, `concurrency` cancellation for PRs, and **always-on artifacts** (`e2e-service-logs`) for post-mortem.
- **Operational clarity**: troubleshooting matrix and runbooks reduced “tribal knowledge” requirements.

### Challenges

- **Backend startup time variance** (cold restore/JIT + DB readiness): required conservative wait loops.
- **Missing `tee` logs initially**: harder to debug Tier B without an orchestrator log file; fixed by writing `mgm-tier-b-orchestrator.log`.
- **Lockfile strategy**: cache key behavior when `packages.lock.json` is absent can surprise maintainers; mitigated by including `src/**/*.csproj` hashes.

### Lessons learned

- **Monitoring early pays off**: weekly health report + consecutive-failure alert reduces mean time to notice regressions.
- **Troubleshooting matrices need concrete artifacts**: “download `e2e-service-logs` and read X/Y/Z files” is more actionable than general advice.

### Improvement ideas (Tier C candidates)

- **Docker for mocks (optional)**: reduce cold `dotnet run` startup; trade-off is image build/publish maintenance.
- **`docs/`-only path filters (policy decision)**: save Actions minutes, but requires risk acceptance for workflow-only changes.
- **Nightly Tier B runs (optional)**: keep Tier A+B on PRs or move Tier B to scheduled gating depending on minute budget.

---

## 8. Maintainer quick-start (Tier B)

### When something fails in PR CI

1. Open the failing run for [`.github/workflows/e2e-test.yml`](../../.github/workflows/e2e-test.yml) → job **`e2e-tier-a-b`**.
2. Check **Job summary** timing table (helps spot startup regressions).
3. Download artifact **`e2e-service-logs`** and inspect:
   - `mgm-backend.log`
   - `mgm-tier-b-orchestrator.log`
   - `mgm-mock-helix.log`
   - `mgm-synthetic-desktop.log`
4. Use **Troubleshooting matrix** (§4) to map the first failing symptom to the likely cause.

### When you change mocks / orchestrator

- Update **both**:
  - the mock/project (under `src/Mocks/`)
  - the Python harness/assertions (under `.github/scripts/`)
- Keep the port matrix **8080 / 9051–9054** aligned in:
  - `.github/workflows/e2e-test.yml`
  - this handover doc
  - `E2E_AUTOMATION_PLAN.md` (port map + first-run guide)

---

## 9. FAQ

### Why does Tier B use `POST /api/e2e/prepare-pending-payout`?

Because real roulette timing is wall-clock aligned (5‑minute boundaries). The Development-only harness makes Tier B deterministic for CI while still exercising the same Desktop REST contracts and payout transitions.

### What exactly is “Tier B” here—does it run WoW?

No. Tier B in CI uses **MockHelixApi** + **SyntheticDesktop** on GitHub-hosted Linux runners. Real **WinAPI + WoW 3.3.5a** is planned for Tier C (self-hosted Windows or manual).

### Why must `Twitch:HelixApiBaseUrl` be the service root (not `.../helix`)?

The Backend posts to a **relative path** `helix/chat/messages`. The base URL must be the mock’s root (e.g. `http://127.0.0.1:9053`) so the combined URL resolves correctly.

### How do we verify monitoring is working?

Follow `TIER_B_MAINTENANCE_CHECKLIST.md` section **Verification of monitoring & alerting** to run the weekly report, simulate consecutive failures, and confirm the `e2e-test.yml` Summary timing table.

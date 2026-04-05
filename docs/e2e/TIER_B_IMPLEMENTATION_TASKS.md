<!-- Updated: 2026-04-05 (Tier B integration & first run) -->

# Tier B E2E implementation tasks (CI extension)

Actionable work items for **CI Tier B**: **configurable Helix base URL**, **MockHelixApi**, **SyntheticDesktop**, and **workflow** integration. Normative architecture: [`docs/e2e/E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md) (**[Tier B Implementation Plan](E2E_AUTOMATION_PLAN.md#tier-b-implementation-plan-ci-extension)**, **[Tier B Troubleshooting Guide](E2E_AUTOMATION_PLAN.md#tier-b-troubleshooting-guide)**). Parent checklist: [`docs/e2e/E2E_AUTOMATION_TASKS.md`](E2E_AUTOMATION_TASKS.md).

---

## Task table

| Task ID | Task | Description | Owner | Est. time | Dependencies | Status |
|---------|------|-------------|-------|-----------|--------------|--------|
| **A1** | Helix URL in `HelixChatService` | Refactor [`HelixChatService.cs`](../../src/MimironsGoldOMatic.Backend/Services/HelixChatService.cs) to use **`HttpClient`** **`BaseAddress`** + relative **`helix/chat/messages`**; preserve default production behavior when **`HelixApiBaseUrl`** unset. | Backend Dev | 0.5–1 d | — | **Done** |
| **A2** | `TwitchOptions` + validation | Add **`HelixApiBaseUrl`** to [`TwitchOptions.cs`](../../src/MimironsGoldOMatic.Backend/Configuration/TwitchOptions.cs); wire named **`HttpClient`** in [`Program.cs`](../../src/MimironsGoldOMatic.Backend/Program.cs). | Backend Dev | 0.5 d | A1 | **Done** |
| **A3** | `appsettings` + docs | Document **`Twitch:HelixApiBaseUrl`** in [`appsettings.Development.json`](../../src/MimironsGoldOMatic.Backend/appsettings.Development.json); align with [`docs/e2e/E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md). | Backend Dev | 0.25 d | A2 | **Done** |
| **B1** | Create `MockHelixApi` project | New **`src/Mocks/MockHelixApi/`**, .NET 10.0, add to [`MimironsGoldOMatic.slnx`](../../src/MimironsGoldOMatic.slnx). | Backend Dev | 0.5 d | A1 (for integration test against mock) | Done (readiness) |
| **B2** | `POST /helix/chat/messages` | Implement Helix-shaped handler; optional strict `Authorization` / `Client-Id` checks (`MockHelix:StrictAuth`). | Backend Dev | 0.5–1 d | B1 | Done (readiness) |
| **B3** | Response templates + last request | **204** success; `GET /last-request` for CI assertions (error branches optional). | Backend Dev | 0.5 d | B2 | Done (readiness) |
| **B4** | `GET /health` | JSON **`healthy` + `component: MockHelixApi`** (Tier B convention; see [E2E plan](E2E_AUTOMATION_PLAN.md#tier-b-readiness-verification)). | Backend Dev | 0.25 d | B1 | Done (readiness) |
| **B5** | Wire mock into E2E | Start on **9053**; Backend env in CI — paired with **D1**. | DevOps / Backend Dev | 0.5 d | A2, B2–B4 | **Done** |
| **C1** | Create `SyntheticDesktop` project | **`src/Mocks/SyntheticDesktop/`**, .NET 10.0, HTTP host on **9054**. | Backend Dev | 0.5 d | — | Done (readiness) |
| **C2** | Desktop API sequence | `HttpClient` + `X-MGM-ApiKey`: `POST .../confirm-acceptance`, `PATCH .../status` (`InProgress`, `Sent`) per [`DesktopPayoutsController.cs`](../../src/MimironsGoldOMatic.Backend/Controllers/DesktopPayoutsController.cs). | Backend Dev | 1 d | C1 | Done (readiness) |
| **C3** | Seed / chain | **Done (CI path):** Development **`POST /api/e2e/prepare-pending-payout`** replaces wall-clock roulette + Marten-only seeding for GitHub-hosted runners; optional future: natural tick rehearsal on long-timeout agents. | Backend Dev | 1 d | C2 | **Done** |
| **C4** | Verification hooks | `GET /last-run` after `POST /run-sequence`; scripts in [`.github/scripts/tier_b_verification/`](../../.github/scripts/tier_b_verification/). | Backend Dev | 0.5 d | C2 | Done (readiness) |
| **D1** | Extend `e2e-test.yml` | Background **MockHelixApi** + **SyntheticDesktop**; Backend Twitch env for Helix outbound + **`Mgm__EnableE2eHarness`**. | DevOps | 0.5–1 d | B5, C3 | **Done** |
| **D2** | Script: enrollment → Tier B | [`.github/scripts/run_e2e_tier_b.py`](../../.github/scripts/run_e2e_tier_b.py); **`send_e2e_eventsub.py`** optional **`--probe-mock-helix`**. | DevOps / Backend Dev | 0.5 d | D1 | **Done** |
| **D3** | Assertions | MockHelix **`GET /last-request`** + SyntheticDesktop **`GET /last-run`** + **`GET /api/pool/me`**. | DevOps / Backend Dev | 0.5 d | D2 | **Done** |

**Suggested sequence:** **A1 → A2 → A3** → **B1–B4** → **C1–C4** (local) → **D1–D3** (CI).

---

## Success criteria (per component)

| Component | Done when |
|-----------|-----------|
| **A (Helix URL)** | Unset config → identical URL to today; tests pass. Set to mock → all Helix traffic hits mock only. |
| **B (MockHelixApi)** | Health **200**; **`POST`** recorded; Tier B assertion can read winner message text. |
| **C (SyntheticDesktop)** | Full API sequence **2xx**; payout **`Sent`**; pool rule satisfied. |
| **D (Workflow)** | PR job green; failure mode identifiable from logs (see [Troubleshooting](E2E_AUTOMATION_PLAN.md#tier-b-troubleshooting-guide)). |

---

## Document control

| Version | Date | Note |
|---------|------|------|
| 1.0 | 2026-04-05 | Initial Tier B task table from E2E automation plan |
| 1.1 | 2026-04-05 | Readiness: **B1–B4**, **C1–C2**, **C4** implemented; **C3**, **B5**, **D\*** remain; link scripts + checklist |
| 1.2 | 2026-04-05 | **Tier B integrated:** **A\***, **B5**, **C3** (harness), **D\*** marked **Done**; **`run_e2e_tier_b.py`** |

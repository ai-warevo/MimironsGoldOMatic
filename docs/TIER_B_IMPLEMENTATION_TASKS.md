<!-- Updated: 2026-04-05 (Tier A finalization + Tier B detailed plan) -->

# Tier B E2E implementation tasks (CI extension)

Actionable work items for **CI Tier B**: **configurable Helix base URL**, **MockHelixApi**, **SyntheticDesktop**, and **workflow** integration. Normative architecture: [`docs/E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md) (**[Tier B Implementation Plan](E2E_AUTOMATION_PLAN.md#tier-b-implementation-plan-ci-extension)**, **[Tier B Troubleshooting Guide](E2E_AUTOMATION_PLAN.md#tier-b-troubleshooting-guide)**). Parent checklist: [`docs/E2E_AUTOMATION_TASKS.md`](E2E_AUTOMATION_TASKS.md).

---

## Task table

| Task ID | Task | Description | Owner | Est. time | Dependencies | Status |
|---------|------|-------------|-------|-----------|--------------|--------|
| **A1** | Helix URL in `HelixChatService` | Refactor [`HelixChatService.cs`](../src/MimironsGoldOMatic.Backend/Services/HelixChatService.cs) to use configurable base + relative `/helix/chat/messages`; preserve default production behavior when unset. | Backend Dev | 0.5–1 d | — | Not started |
| **A2** | `TwitchOptions` + validation | Add `HelixApiBaseUrl` to [`TwitchOptions.cs`](../src/MimironsGoldOMatic.Backend/Configuration/TwitchOptions.cs); validate absolute URI when set; wire named `HttpClient` in [`Program.cs`](../src/MimironsGoldOMatic.Backend/Program.cs). | Backend Dev | 0.5 d | A1 | Not started |
| **A3** | `appsettings` + docs | Document `Twitch:HelixApiBaseUrl` in [`appsettings.Development.json`](../src/MimironsGoldOMatic.Backend/appsettings.Development.json) (comment or example); align with [`docs/MimironsGoldOMatic.Backend/ReadME.md`](MimironsGoldOMatic.Backend/ReadME.md). | Backend Dev | 0.25 d | A2 | Not started |
| **B1** | Create `MockHelixApi` project | New **`src/Mocks/MockHelixApi/`**, .NET 10.0, add to [`MimironsGoldOMatic.slnx`](../src/MimironsGoldOMatic.slnx). | Backend Dev | 0.5 d | A1 (for integration test against mock) | Not started |
| **B2** | `POST /helix/chat/messages` | Implement Helix-shaped handler; optional strict `Authorization` / `Client-Id` checks. | Backend Dev | 0.5–1 d | B1 | Not started |
| **B3** | Response templates + last request | Success + error templates; `GET /last-request` (or equivalent) for CI assertions. | Backend Dev | 0.5 d | B2 | Not started |
| **B4** | `GET /health` | Health endpoint consistent with other mocks. | Backend Dev | 0.25 d | B1 | Not started |
| **B5** | Wire mock into E2E | Start on **9053** (or chosen port); Backend env in CI — paired with **D1**. | DevOps / Backend Dev | 0.5 d | A2, B2–B4 | Not started |
| **C1** | Create `SyntheticDesktop` project | **`src/Mocks/SyntheticDesktop/`**, .NET 10.0, HTTP-only harness. | Backend Dev | 0.5 d | — | Not started |
| **C2** | Desktop API sequence | `HttpClient` + `X-MGM-ApiKey`: `POST .../confirm-acceptance`, `PATCH .../status` (`InProgress`, `Sent`) per [`DesktopPayoutsController.cs`](../src/MimironsGoldOMatic.Backend/Controllers/DesktopPayoutsController.cs). | Backend Dev | 1 d | C1 | Not started |
| **C3** | Seed / chain | Reuse patterns from [`RouletteVerifyCandidateIntegrationTests.cs`](../src/Tests/MimironsGoldOMatic.Backend.UnitTests/RouletteVerifyCandidateIntegrationTests.cs), [`PatchPayoutStatusIntegrationTests.cs`](../src/Tests/MimironsGoldOMatic.Backend.UnitTests/PatchPayoutStatusIntegrationTests.cs). | Backend Dev | 1 d | C2 | Not started |
| **C4** | Verification hooks | Exit codes or `GET /last-run` for test assertions. | Backend Dev | 0.5 d | C2 | Not started |
| **D1** | Extend `e2e-test.yml` | Background **MockHelixApi** + **SyntheticDesktop**; Backend Twitch env for Helix outbound. | DevOps | 0.5–1 d | B5, C3 | Not started |
| **D2** | Script: enrollment → Tier B | Orchestrate after Tier A enrollment (or Marten seed): run SyntheticDesktop or expand Python/bash. | DevOps / Backend Dev | 0.5 d | D1 | Not started |
| **D3** | Assertions | Assert MockHelix payload + pool removal / `GET /api/pool/me`. | DevOps / Backend Dev | 0.5 d | D2 | Not started |

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

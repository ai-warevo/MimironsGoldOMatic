<!-- Created: 2026-04-05 (E2E automation tasks) -->

# E2E automation tasks (MVP-6)

## 1. Overview

Tasks to implement the E2E automation plan described in [E2E Automation Plan](E2E_AUTOMATION_PLAN.md).

- **Current status:** Plan created; **Tier A** (**CI**) automation **pending** (mocks + chained tests + workflow).
- **Target completion:** Full **Tier A** flow **Automated** in **CI/CD** (Backend + Postgres + mocked Helix + **SyntheticDesktop** HTTP sequence). **Tier B** (real WoW + Desktop + Twitch) remains optional / self-hosted per the plan.

**Normative product behavior:** unchanged — still defined in **`docs/SPEC.md`**. This file is execution tracking only.

---

## 2. Task breakdown by component

The EBS already exposes **`POST /api/twitch/eventsub`** ([`TwitchEventSubController`](../src/MimironsGoldOMatic.Backend/Controllers/TwitchEventSubController.cs)). **MockEventSubWebhook** means **test-side HTTP clients + payloads**, not a duplicate webhook service.

### A. MockEventSubWebhook (test harness)

| # | Task | Owner | Est. | Notes |
|---|------|--------|------|--------|
| A1 | Add **`EventSubSignatureHelper`** (or equivalent) in **`MimironsGoldOMatic.Backend.Tests`** that computes Twitch **`Twitch-Eventsub-Message-Signature`** (`sha256=` HMAC-SHA256 over `message-id + timestamp + body`) matching [`VerifySignature`](../src/MimironsGoldOMatic.Backend/Controllers/TwitchEventSubController.cs). | Backend Dev | 0.5–1 day | Use a known `Twitch:EventSubSecret` in test configuration. |
| A2 | Add golden JSON bodies for **`channel.chat.message`** under e.g. `Backend.Tests/Fixtures/EventSub/` (`subscription`, `event.message_id`, `chatter_user_id`, `message.text`, subscriber **`badges`**). | Backend Dev | 0.5 day | Align with Twitch EventSub reference; version fixtures when schema changes. |
| A3 | Integration test: **`HttpClient`** **`POST`** to **`/api/twitch/eventsub`** with headers + body → assert pool enrollment via Marten or follow-up **`GET`** (if test host exposes full pipeline). | Backend Dev | 1 day | Cover both empty secret (dev bypass) and signed path. |
| A4 | Document fixture maintenance in **`docs/MimironsGoldOMatic.Backend/ReadME.md`** or link this file. | Backend Dev | 0.25 day | Pairs with risk task R1. |

### B. MockExtensionJwt

| # | Task | Owner | Est. | Notes |
|---|------|--------|------|--------|
| B1 | Implement a small **test JWT builder** (HS256) using the same signing material as [`Program.cs`](../src/MimironsGoldOMatic.Backend/Program.cs) (**Extension** secret / dev key) with claims **`user_id`**, optional **`display_name`**. | Backend Dev | 0.5 day | Not Twitch-issued; label tests **harness-only** per **`docs/INTERACTION_SCENARIOS.md`** auth notes. |
| B2 | Wire JWT into shared test host factory (same pattern as [`BackendTestHost`](../src/MimironsGoldOMatic.Backend.Tests/Support/) / existing integration setup). | Backend Dev | 0.5 day | |
| B3 | Tests calling **`GET /api/pool/me`**, **`POST /api/payouts/claim`** ([`RouletteController`](../src/MimironsGoldOMatic.Backend/Controllers/RouletteController.cs)) with **`Authorization: Bearer`**. | Backend Dev | 1 day | Respect **`Mgm:DevSkipSubscriberCheck`** / subscriber rules when exercising **claim**. |

### C. MockHelixApi

| # | Task | Owner | Est. | Notes |
|---|------|--------|------|--------|
| C1 | Refactor [`HelixChatService.cs`](../src/MimironsGoldOMatic.Backend/Services/HelixChatService.cs) to use a **configurable Helix API base URI** (e.g. **`Twitch:HelixApiBaseUrl`** in [`TwitchOptions.cs`](../src/MimironsGoldOMatic.Backend/Configuration/TwitchOptions.cs)); default **`https://api.twitch.tv`** when unset. Register named **`HttpClient`** with that base address in [`Program.cs`](../src/MimironsGoldOMatic.Backend/Program.cs). | Backend Dev | 1 day | **Product code change** — add regression test that production URL still works when option empty. |
| C2 | In tests, use **`DelegatingHandler`** stub (or **WireMock.NET**) listening on loopback to capture **`POST .../helix/chat/messages`**; return **2xx** with documented Helix response shape. | Backend Dev | 1 day | Assert JSON body includes Russian §11 template from existing service. |
| C3 | Add templates for **failure** paths (4xx/5xx) to verify **3× retry** logging / behavior without flaking real network. | Backend Dev | 0.5 day | Keep tests deterministic. |

### D. SyntheticDesktop

| # | Task | Owner | Est. | Notes |
|---|------|--------|------|--------|
| D1 | Define ordered **`HttpClient`** calls mirroring **SC-001** steps 10–15: **`POST /api/payouts/{id}/confirm-acceptance`**, **`PATCH .../status`** **`InProgress`**, **`PATCH .../status`** **`Sent`** ([`DesktopPayoutsController`](../src/MimironsGoldOMatic.Backend/Controllers/DesktopPayoutsController.cs)); header **`X-MGM-ApiKey`**. | Backend Dev | 1–2 days | Optional: thin **`Mgm.Desktop.E2EHarness`** console — only if tests need a subprocess; prefer in-proc test helper first. |
| D2 | Seed Marten state (pool + spin + **`Pending`**) before synthetic calls, or chain after **verify-candidate** test (reuse [`RouletteVerifyCandidateIntegrationTests`](../src/MimironsGoldOMatic.Backend.Tests/RouletteVerifyCandidateIntegrationTests.cs) patterns). | Backend Dev | 1–2 days | |
| D3 | Assert final state: payout **`Sent`**, pool row removed, **MockHelix** received exactly one announcement. | Backend Dev | 0.5 day | Success criteria align with [E2E Automation Plan §6](E2E_AUTOMATION_PLAN.md#6-success-criteria). |

---

## 3. Shared infrastructure tasks

| # | Task | Owner | Est. | Notes |
|---|------|--------|------|--------|
| S1 | **Configurable Helix URL** (same as **C1**). | Backend Dev | (see C1) | Single implementation; listed here as shared dependency for **CI**. |
| S2 | **Evaluate Docker Compose** for mocks vs **in-process** stubs + **Testcontainers** Postgres only. | DevOps / Backend Dev | 0.5 day | Plan default: **no** separate Compose required for Tier A if **`DelegatingHandler`** suffices; Compose optional for WireMock sidecar. |
| S3 | Add **`.github/workflows/e2e-test.yml`**: **`dotnet build`**, **`--filter Category=Unit`**, **`Category=Integration`**, then **`Category=E2E`** (or chosen filter) with **Docker** and longer timeout. | DevOps | 1 day | Replace `.gitkeep` placeholder pattern in [`.github/workflows/`](../.github/workflows/). |
| S4 | Introduce **`[Trait("Category", "E2E")]`** (or reuse **Integration**) on new chained tests; document in **`docs/MimironsGoldOMatic.Backend/ReadME.md`**. | Backend Dev | 0.25 day | |

---

## 4. Validation tasks

| # | Task | Owner | Est. | Notes |
|---|------|--------|------|--------|
| V1 | Encode [E2E Automation Plan §6 success criteria](E2E_AUTOMATION_PLAN.md#6-success-criteria) as assertions in one **Tier A** test class. | Backend Dev | 0.5 day | |
| V2 | CI job: publish **trx** / test logs as artifacts; fail job on unhandled host exceptions (standard xUnit + `--logger`). | DevOps | 0.5 day | |

---

## 5. Risk mitigation tasks

| # | Task | Owner | Est. | Notes |
|---|------|--------|------|--------|
| R1 | **EventSub fixture maintenance:** add **README** in `Fixtures/EventSub/` + calendar reminder / PR checklist when Twitch API changelog touches **`channel.chat.message`**. | Backend Dev / Team lead | 0.25 day | |
| R2 | **CI cost:** decide **PR** vs **`main` only** vs **nightly** for **E2E** job; document in workflow comments and **`docs/ROADMAP.md`** if policy changes. | DevOps | 0.25 day | |

---

## 6. Ownership and timeline

| Role | Primary responsibilities |
|------|-------------------------|
| **Backend Dev** | **A**–**D**, **S1**, **S4**, **V1**, **R1** (EBS, tests, Helix refactor). |
| **DevOps** | **S3**, **V2**, **R2** (workflows, runners, artifacts). |
| **Frontend Dev** | Optional: Extension JWT from **Dev Rig** for **manual** Tier B only; **Tier A** uses test-signed JWT (**B**). |
| **Game Dev** | **Tier B** only: WoW 3.3.5a + addon + log visibility on target clients; not required for **Tier A** **CI**. |

**Rough order:** **C1/S1** → **C2/C3** → **D\*** chained test → **A\*** EventSub signed path → **B\*** if claim path needed in chain → **S3** workflow.

**Clarification needed (team discussion):**

- Whether **Tier A** enrollment uses **EventSub only**, **`POST /api/payouts/claim` only**, or **both** in separate tests.
- Whether to add **`WireMock.NET`** package vs **pure** `DelegatingHandler` (policy + maintenance).
- **Self-hosted Windows** runner budget for **Tier B** (if any).

---

## Document control

| Version | Date | Note |
|---------|------|------|
| 1.0 | 2026-04-05 | Initial tasks from [E2E Automation Plan](E2E_AUTOMATION_PLAN.md) |

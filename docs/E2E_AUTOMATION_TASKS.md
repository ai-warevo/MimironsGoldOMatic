<!-- Created: 2026-04-05 (E2E automation tasks) -->
<!-- Updated: 2026-04-05 (Tier B integration & first run) -->

# E2E automation tasks (MVP-6)

## 1. Overview

Tasks to implement the E2E automation plan described in [E2E Automation Plan](E2E_AUTOMATION_PLAN.md).

- **Current status:** **CI Tier A + B (same workflow):** **`.github/workflows/e2e-test.yml`** job **`e2e-tier-a-b`** — **Shared + Backend + four mocks** (**MockEventSubWebhook**, **MockExtensionJwt**, **MockHelixApi**, **SyntheticDesktop**), **`.github/scripts/send_e2e_eventsub.py`**, **`.github/scripts/run_e2e_tier_b.py`**, **`pip install`** for **`scripts/tier_b_verification`**. **Tier A slice:** synthetic chat → **`GET /api/pool/me`**. **Tier B slice:** **`POST /api/e2e/prepare-pending-payout`** → **`/run-sequence`** → **MockHelix** **`/last-request`** → **`GET /api/pool/me`** after **`Sent`**. **Records:** [Tier A Test Results](E2E_AUTOMATION_PLAN.md#tier-a-test-results--verification), [Tier B Integration Results](E2E_AUTOMATION_PLAN.md#tier-b-integration-results). **CD:** **`.github/workflows/release.yml`** — parallel ZIPs + **Backend** GHCR + **`create-release`**. Optional in-repo **xUnit** E2E chain remains future work.
- **Target completion:** **Operational** real WoW + Desktop validation remains optional (see plan **§1** / [Optimization](E2E_AUTOMATION_PLAN.md#optimization-and-scalability-ci)).

**Normative product behavior:** unchanged — still defined in **`docs/SPEC.md`**. This file is execution tracking only.

---

## Tier A Validation Checklist

Use this before relying on **CI Tier A** as a gate or when debugging a red workflow. Details and mitigations: [Predictive issue analysis](E2E_AUTOMATION_PLAN.md#predictive-issue-analysis-tier-a-ci).

- [x] Confirm the workflow run is a **`pull_request`** targeting **`main`** (Tier A does **not** run on arbitrary branches unless **`on:`** is extended).
- [x] Confirm **PR validation** builds only **Shared + Backend + four mocks** (no **Desktop**, **WoW addon**, **Twitch Extension**, **Backend.UnitTests**) — see **`Restore and build (Backend + all E2E mocks)`** in [`.github/workflows/e2e-test.yml`](../.github/workflows/e2e-test.yml).
- [x] Confirm **PostgreSQL 16** runs in the job via the **`services.postgres`** container (**`postgres:16-alpine`**) and **`pg_isready`** health checks succeed (not the host image—**`ubuntu-latest`** does not need a local `postgres` package).
- [x] Verify **mock services** start: **`GET http://127.0.0.1:9051/health`** (**MockEventSubWebhook**) and **`GET http://127.0.0.1:9052/health`** (**MockExtensionJwt**) return **200** with JSON **`status`** / **`service`** fields.
- [x] Test **HMAC** end-to-end: run [`.github/scripts/send_e2e_eventsub.py`](../.github/scripts/send_e2e_eventsub.py) with the **same** `--secret` as **`Twitch__EventSubSecret`** on mock + Backend; expect **no** **401** from mock or EBS.
- [x] Validate **JWT**: **`GET http://127.0.0.1:9052/token?userId=…&displayName=…`** returns **`access_token`**; Backend in **Development** with empty **`Twitch:ExtensionSecret`** must share the **dev** signing material with **MockExtensionJwt** (see [ReadME](MimironsGoldOMatic.Backend/ReadME.md)).
- [x] Confirm **event forwarding**: mock logs show forward to **`{Backend}/api/twitch/eventsub`**; EBS returns success for synthetic **`channel.chat.message`**.
- [x] Verify **`GET /api/pool/me`**: Bearer from the mock token for **`e2e-viewer-1`** yields **`isEnrolled: true`** and expected **`characterName`** after a synthetic **`!twgold …`** line (workflow uses **`!twgold Etoehero`**).

---

## 2. Task breakdown by component

The EBS already exposes **`POST /api/twitch/eventsub`** ([`TwitchEventSubController`](../src/MimironsGoldOMatic.Backend/Controllers/TwitchEventSubController.cs)). **Implemented:** standalone relay **`src/Mocks/MockEventSubWebhook`** + **Python** sender in **CI** (see [Tier A implementation](E2E_AUTOMATION_PLAN.md#tier-a-implementation-repository)). **Optional:** duplicate signing logic in **xUnit** (A1–A3).

### A. MockEventSubWebhook (test harness)

| # | Task | Owner | Est. | Notes |
|---|------|--------|------|--------|
| A1 | Add **`EventSubSignatureHelper`** (or equivalent) in **`MimironsGoldOMatic.Backend.UnitTests`** that computes Twitch **`Twitch-Eventsub-Message-Signature`** (`sha256=` HMAC-SHA256 over `message-id + timestamp + body`) matching [`VerifySignature`](../src/MimironsGoldOMatic.Backend/Controllers/TwitchEventSubController.cs). | Backend Dev | 0.5–1 day | **Optional** — **CI** already signs in **Python**; in-repo helper reduces drift. |
| A2 | Add golden JSON bodies for **`channel.chat.message`** under e.g. `src/Tests/MimironsGoldOMatic.Backend.UnitTests/Fixtures/EventSub/` (`subscription`, `event.message_id`, `chatter_user_id`, `message.text`, subscriber **`badges`**). | Backend Dev | 0.5 day | Align with Twitch EventSub reference; version fixtures when schema changes. |
| A3 | Integration test: **`HttpClient`** **`POST`** to **`/api/twitch/eventsub`** with headers + body → assert pool enrollment via Marten or follow-up **`GET`** (if test host exposes full pipeline). | Backend Dev | 1 day | Cover both empty secret (dev bypass) and signed path. |
| A4 | Document fixture maintenance in **`docs/MimironsGoldOMatic.Backend/ReadME.md`** or link this file. | Backend Dev | 0.25 day | Pairs with risk task R1. |

### B. MockExtensionJwt

| # | Task | Owner | Est. | Notes |
|---|------|--------|------|--------|
| B1 | Implement a small **test JWT builder** (HS256) using the same signing material as [`Program.cs`](../src/MimironsGoldOMatic.Backend/Program.cs) (**Extension** secret / dev key) with claims **`user_id`**, optional **`display_name`**. | Backend Dev | 0.5 day | **Done** as **`MockExtensionJwt`** **`GET /token`** — keep for parity or retire if service-only approach wins. |
| B2 | Wire JWT into shared test host factory (same pattern as [`BackendTestHost`](../src/Tests/MimironsGoldOMatic.Backend.UnitTests/Support/) / existing integration setup). | Backend Dev | 0.5 day | |
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
| D2 | Seed Marten state (pool + spin + **`Pending`**) before synthetic calls, or chain after **verify-candidate** test (reuse [`RouletteVerifyCandidateIntegrationTests`](../src/Tests/MimironsGoldOMatic.Backend.UnitTests/RouletteVerifyCandidateIntegrationTests.cs) patterns). | Backend Dev | 1–2 days | |
| D3 | Assert final state: payout **`Sent`**, pool row removed, **MockHelix** received exactly one announcement. | Backend Dev | 0.5 day | Success criteria align with [E2E Automation Plan §6](E2E_AUTOMATION_PLAN.md#6-success-criteria). |

---

## 3. Shared infrastructure tasks

| # | Task | Owner | Est. | Notes |
|---|------|--------|------|--------|
| S1 | **Configurable Helix URL** (same as **C1**). | Backend Dev | (see C1) | Single implementation; listed here as shared dependency for **CI**. |
| S2 | **Evaluate Docker Compose** for mocks vs **in-process** stubs + **Testcontainers** Postgres only. | DevOps / Backend Dev | 0.5 day | Plan default: **no** separate Compose required for Tier A if **`DelegatingHandler`** suffices; Compose optional for WireMock sidecar. |
| S3 | Add **`.github/workflows/e2e-test.yml`**: **`dotnet build`**, **`--filter Category=Unit`**, **`Category=Integration`**, then **`Category=E2E`** (or chosen filter) with **Docker** and longer timeout. | DevOps | 1 day | **Tier A job done:** [`.github/workflows/e2e-test.yml`](../.github/workflows/e2e-test.yml) (PR → **main**, Postgres + mocks). Extend later with **`dotnet test`** stages if desired. |
| S4 | Introduce **`[Trait("Category", "E2E")]`** (or reuse **Integration**) on new chained tests; document in **`docs/MimironsGoldOMatic.Backend/ReadME.md`**. | Backend Dev | 0.25 day | |

---

## 4. Validation tasks

| # | Task | Owner | Est. | Notes |
|---|------|--------|------|--------|
| V1 | Encode [E2E Automation Plan §6 success criteria](E2E_AUTOMATION_PLAN.md#6-success-criteria) as assertions in one **Tier A** test class. | Backend Dev | 0.5 day | |
| V2 | CI job: publish **trx** / test logs as artifacts; fail job on unhandled host exceptions (standard xUnit + `--logger`). | DevOps | 0.5 day | |
| V3 | **Release pipeline:** verify **`build-wowaddon`** produces **`wow-addon-release`** with **`MimironsGoldOMatic.toc`**, **`MimironsGoldOMatic.lua`**, **`README.txt`**, and sane addon folder layout inside the ZIP. | DevOps | 0.25 day | See [`.github/workflows/release.yml`](../.github/workflows/release.yml). |
| V4 | **Release pipeline:** verify **`build-twitch-extension`** runs **`npm ci`** + **`npm run build`** and **`twitch-extension-release`** contains built **`dist/`** assets + **`README.txt`**. | DevOps / Frontend Dev | 0.25 day | |
| V5 | **Release pipeline:** verify **`build-backend-docker`** pushes **`ghcr.io/<owner-lowercase>/mimirons-goldomatic-backend:<tag>`** and **`:latest`**, and that **`create-release`** notes include the image lines + digest when present. | DevOps | 0.25 day | Confirm org **Packages** visibility and **`GITHUB_TOKEN`** scopes. |
| V6 | **Release pipeline:** validate **GitHub Release** assets: all three ZIPs, **`SHA256SUMS.txt`**, tag matches **`RELEASE_VERSION`**, notes list commits since last tag. | DevOps / Release manager | 0.25 day | |
| V7 | **Release pipeline:** confirm **`create-release`** shows **`needs:`** all four build jobs — a failed **Desktop**, **addon**, **Extension**, or **Docker** job must **skip** release creation. | DevOps | 0.1 day | |

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

- **Tier A CI** currently uses **EventSub** only (via mock relay). Whether to add a parallel job for **`POST /api/payouts/claim`** + JWT.
- Whether to add **`WireMock.NET`** package vs **pure** `DelegatingHandler` (policy + maintenance).
- **Self-hosted Windows** runner budget for **Tier B** (if any).

---

## Document control

| Version | Date | Note |
|---------|------|------|
| 1.0 | 2026-04-05 | Initial tasks from [E2E Automation Plan](E2E_AUTOMATION_PLAN.md) |
| 1.1 | 2026-04-05 | Tier A mocks + **`e2e-test.yml`** + Python sender; tasks file status updated |
| 1.2 | 2026-04-05 | **Tier A Validation Checklist**; **CI Tier A / CI Tier B** wording aligned with [E2E_AUTOMATION_PLAN.md](E2E_AUTOMATION_PLAN.md) |
| 1.3 | 2026-04-05 | PR scoped Tier A build; **`release.yml`** validation tasks (**V3–V7**); cross-link to [CI/CD Pipeline Architecture](E2E_AUTOMATION_PLAN.md#cicd-pipeline-architecture) |
| 1.4 | 2026-04-05 | **Tier A Validation Checklist** marked complete; link to [Tier A Test Results](E2E_AUTOMATION_PLAN.md#tier-a-test-results--verification) and [`TIER_B_IMPLEMENTATION_TASKS.md`](TIER_B_IMPLEMENTATION_TASKS.md) |
| 1.5 | 2026-04-05 | **Tier A + B** same workflow; overview + checklist item for **four mocks**; link [Tier B Integration Results](E2E_AUTOMATION_PLAN.md#tier-b-integration-results) |

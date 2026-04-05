<!-- Created: 2026-04-05 (E2E automation plan) -->
<!-- Updated: 2026-04-05 (Tier A implementation) -->

# E2E automation plan (MVP-6): Chat → WoW → Helix

This document proposes how to automate the **full operator workflow** currently described manually in [`docs/INTERACTION_SCENARIOS.md`](INTERACTION_SCENARIOS.md) (**SC-001**, **SC-005**, and [Automated E2E Scenarios (MVP-6)](INTERACTION_SCENARIOS.md#automated-e2e-scenarios-mvp-6)). It is **planning only**; it does not change product behavior in **`docs/SPEC.md`**.

**Related:** [`docs/ROADMAP.md`](ROADMAP.md) MVP-6, [`docs/IMPLEMENTATION_READINESS.md`](IMPLEMENTATION_READINESS.md) (MVP-6 verification status), [`docs/MimironsGoldOMatic.Backend/ReadME.md`](MimironsGoldOMatic.Backend/ReadME.md) (automated tests). **Implementation checklist / ownership:** [E2E Automation Tasks](E2E_AUTOMATION_TASKS.md).

**Code roots (actual repository layout):**

- Backend (EBS): `src/MimironsGoldOMatic.Backend/` — **not** `MimironsGoldOMatic.WEBAPI.Backend`.
- WoW addon: `src/MimironsGoldOMatic.WoWAddon/`.
- Desktop: `src/MimironsGoldOMatic.Desktop/`.
- CI: `.github/workflows/e2e-test.yml` — **Tier A** (Backend + Postgres + mocks + synthetic EventSub). Other workflows may be added later.
- Tier A mocks: `src/Mocks/MockEventSubWebhook/`, `src/Mocks/MockExtensionJwt/`.

---

## 1. Overview

### Target flow (normative narrative)

End-to-end pipe to automate:

**Twitch chat message (`!twgold`) → Backend processing (pool, spin, payout state) → WoW addon + Desktop bridge (log tags, inject, mail) → Helix Send Chat Message (reward-sent announcement).**

This aligns with **SC-001** steps 1–16 and the four segments in **Automated E2E Scenarios (MVP-6)**.

### Current vs target

| Aspect | Current | Target |
|--------|---------|--------|
| Backend persistence + MediatR rules | **Automated** (Integration tests + Testcontainers) | Keep in **CI**; extend coverage where gaps exist |
| Live Twitch EventSub stream | **Manual** | **Mock** signed webhook posts in **CI**, or optional **staging** job with secrets |
| WoW 3.3.5a client + addon UI | **Manual** | **Mock** or **self-hosted runner** (no practical GitHub-hosted WoW) |
| Desktop WinAPI → WoW | **Manual** | **Mock** via **API-only choreography** in **CI**; real WinAPI on **manual/nightly** |
| Helix `POST .../chat/messages` | **Manual** / real token | **Mock** HTTP server in **CI**; optional **live** job with broadcaster token |

**Recommendation:** Treat **“fully automated in CI/CD”** as **two tiers**:

1. **Tier A — CI (GitHub-hosted):** Single process: Backend + Postgres (container) + **mocked outbound Helix** + **synthetic Desktop** (HTTP calls only, no WoW). Verifies **EBS state machine** through **`Sent`** and **Helix call attempted** (capture request body).
2. **Tier B — Optional pipeline:** Self-hosted runner or manual **nightly**: real **WoW** + **Desktop** + **Dev Rig / test channel** for true UI and WinAPI validation.

---

## 2. Step-by-step automation breakdown

Each row maps to **Automated E2E** steps 1–4 and the middle of **SC-001**.

### Step 1 — Twitch chat → Backend pool enrollment

| Field | Content |
|--------|---------|
| **Action** | Subscriber sends `!twgold <CharacterName>`; Backend records pool row (dedupe by `message_id` or idempotent claim). |
| **Trigger** | **CI:** test code sends HTTP request. Either **`POST /api/twitch/eventsub`** with a JSON body matching **`channel.chat.message`** (see [`TwitchEventSubController`](../src/MimironsGoldOMatic.Backend/Controllers/TwitchEventSubController.cs)) or **`POST /api/payouts/claim`** with Extension JWT ([`RouletteController`](../src/MimironsGoldOMatic.Backend/Controllers/RouletteController.cs)). |
| **Mock / stub** | **Mock Twitch:** no IRC; use **synthetic EventSub payload** (and HMAC when `Twitch:EventSubSecret` is set — mirror [`VerifySignature`](../src/MimironsGoldOMatic.Backend/Controllers/TwitchEventSubController.cs)). **Stub:** Helix subscriber check if enabled (`Mgm:DevSkipSubscriberCheck` / product flags per Backend README). |
| **Verification** | **GET** pool read-model or **`GET /api/pool/me`** (JWT) / assert Marten **`PoolDocument`** after enrollment path used in test. |
| **Test data** | `message_id`: unique string; `chatter_user_id`: `"123456789"`; `message.text`: `"!twgold Testhero"`; badges include subscriber `set_id` (see `HasSubscriberBadge` in `TwitchEventSubController`). |

### Step 2 — Backend spin / verify-candidate / payout lifecycle

| Field | Content |
|--------|---------|
| **Action** | Spin cycle selects candidate; **`POST /api/roulette/verify-candidate`** with **`online: true`** creates **`Pending`** payout when rules pass. |
| **Trigger** | **CI:** existing pattern: seed Marten docs + call [`RouletteCycleTick`](../src/MimironsGoldOMatic.Backend/Services/RouletteCycleTick.cs), then **`POST /api/roulette/verify-candidate`** via [`DesktopRouletteController`](../src/MimironsGoldOMatic.Backend/Controllers/DesktopRouletteController.cs) with header **`X-MGM-ApiKey`** ([`ApiKeyAuthenticationHandler`](../src/MimironsGoldOMatic.Backend/Auth/ApiKeyAuthenticationHandler.cs)). Extend or compose one **linear E2E test** that chains enrollment → tick → verify (today partially split across [`PostClaimRulesIntegrationTests`](../src/MimironsGoldOMatic.Backend.Tests/PostClaimRulesIntegrationTests.cs), [`RouletteVerifyCandidateIntegrationTests`](../src/MimironsGoldOMatic.Backend.Tests/RouletteVerifyCandidateIntegrationTests.cs)). |
| **Mock / stub** | **Time:** use injectable **`TimeProvider`** / fixed **`DateTime`** only if introduced; otherwise keep “wall-clock safe” windows like existing roulette tests. **No real Desktop.** |
| **Verification** | HTTP **`200`** on verify; payout **`Pending`**; **`GET /api/payouts/pending`** returns row; optional **`GET /api/roulette/state`** ([`GetRouletteStateQuery`](../src/MimironsGoldOMatic.Backend/Application/EbsMediator.cs)) with JWT. |
| **Test data** | [`VerifyCandidateRequest`](../src/MimironsGoldOMatic.Backend/Api/ApiContracts.cs): `schemaVersion`, `spinCycleId`, `characterName`, `online: true`, `capturedAt` within verification window per **`docs/SPEC.md`**. |

### Step 3 — WoW addon / `WoWChatLog.txt` / Desktop WinAPI

| Field | Content |
|--------|---------|
| **Action** | Addon emits **`[MGM_WHO]`**, **`[MGM_ACCEPT:UUID]`**, **`[MGM_CONFIRM:UUID]`**; Desktop tails log and calls **`confirm-acceptance`**, **`PATCH` status**, inject **`/run`**. |
| **Trigger** | **Tier A (CI):** **do not** launch WoW. Use a **synthetic bridge**: same HTTP sequence Desktop would perform, driven from test code (or a thin **`Mgm.Desktop.E2EHarness`** console). **Tier B:** run real **`MimironsGoldOMatic.Desktop`** against **`WoW.exe`** + addon under a self-hosted agent. |
| **Mock / stub** | **MockWoWClient:** not a process — **omit** and replace with **direct API calls** that Desktop would make: [`DesktopPayoutsController`](../src/MimironsGoldOMatic.Backend/Controllers/DesktopPayoutsController.cs) **`POST .../confirm-acceptance`**, **`PATCH .../status`**. **Optional:** temp file appending lines and a **shared log-parser** library extracted from Desktop (future refactor) to prove tag regexes match addon output. |
| **Verification** | After **`confirm-acceptance`**: domain state matches SPEC; after **`PATCH` `Sent`**: pool removal (existing tests). **Tier B:** assert lines in real `WoWChatLog.txt`. |
| **Test data** | `characterName` consistent with enrollment; payout `id` GUID from **`GET /api/payouts/pending`** response. |

### Step 4 — Helix API (reward-sent chat line)

| Field | Content |
|--------|---------|
| **Action** | On transition to **`Sent`**, Backend calls Helix **`Send Chat Message`** ([`HelixChatService`](../src/MimironsGoldOMatic.Backend/Services/HelixChatService.cs)) with Russian copy per **SPEC** §11. |
| **Trigger** | **CI:** **`PATCH /api/payouts/{id}/status`** with **`Sent`** in Tier A test (see [`PatchPayoutStatusIntegrationTests`](../src/MimironsGoldOMatic.Backend.Tests/PatchPayoutStatusIntegrationTests.cs) for pool removal; extend to assert Helix outbound). |
| **Mock / stub** | **MockHelixAPI:** `HttpMessageHandler` fake or **WireMock.NET** listening on loopback; inject **`HttpClient`** base address for **`Helix`** named client. **Today** URL is hardcoded to `https://api.twitch.tv/helix/chat/messages` in `HelixChatService` — **requires a small product change**: e.g. optional **`Twitch:HelixApiBaseUrl`** (empty = production URL) so tests can point to **`http://localhost:9xxx`**. |
| **Verification** | Mock receives **POST** with JSON containing `broadcaster_id`, `sender_id`, `message` matching **`Награда отправлена персонажу {name} на почту, проверяй ящик!`**. |
| **Test data** | [`TwitchOptions`](../src/MimironsGoldOMatic.Backend/Configuration/TwitchOptions.cs): `BroadcasterUserId`, `BroadcasterAccessToken` set to dummy values; mock returns **`204`** / documented Helix success shape. |

---

## 3. Mock services specification

| Mock | Purpose | Simulate | Expected behavior |
|------|---------|----------|-------------------|
| **MockEventSubWebhook** | Replace live Twitch → EBS delivery | **`POST /api/twitch/eventsub`** | Body includes `subscription.type` = `channel.chat.message`, `event.message_id`, `event.chatter_user_id`, `event.message.text`, `event.badges`. Headers: `Twitch-Eventsub-Message-Id`, `Timestamp`, `Signature` when secret configured. |
| **MockExtensionJwt** | Auth for **`/api/pool/me`**, **`/api/payouts/claim`**, etc. | HS256 JWT | Signed with same key as [`Program.cs`](../src/MimironsGoldOMatic.Backend/Program.cs) dev derivation or test secret; claims: `user_id`, optional `display_name`. |
| **MockHelixApi** | No real Twitch chat send in CI | **`POST /helix/chat/messages`** (path as configured) | Return **`200`** with Helix JSON; allow test to assert request body and **`Authorization: Bearer`**, **`Client-Id`** header. |
| **MockWoWClient** (conceptual) | Replace WoW + WinAPI in Tier A | N/A in process | **Not implemented as a service** in Tier A — replaced by **API choreography**. Tier B only: optional scripted window focus tools (out of scope for default **CI**). |
| **SyntheticDesktop** | Replace Desktop executable in Tier A | Sequences of **`HttpClient`** calls | Implements the same order as **SC-001** steps 10–15 **without** log tail: call **`confirm-acceptance`**, **`PATCH InProgress`**, **`PATCH Sent`** (or minimal subset proving **`Sent`** + Helix). |

### Suggested code structure (Backend tests)

- **`HelixChatService` refactor (small):** inject `IOptions<TwitchOptions>` and optional **`Uri HelixApiBase`** (default `https://api.twitch.tv`). Tests register **`HttpClient`** with **`PrimaryHttpMessageHandler`** = **`StubHelixHandler`**.
- **`EventSubSignatureHelper` (test project):** static method to compute `sha256=` HMAC from Twitch headers + body for golden tests.
- **`E2EApiTierATests` (new):** one test class, **`[Trait("Category","E2E")`** or **`Integration`**, building `WebApplicationFactory` **or** reusing [`BackendTestHost`](../src/MimironsGoldOMatic.Backend.Tests/Support/) pattern with mocked `IHttpClientFactory` for **`Helix`**.

---

## 4. CI/CD pipeline design

### Proposed workflow: `.github/workflows/e2e-test.yml`

High-level **jobs** (all **Ubuntu** unless self-hosted Tier B):

| Job | Purpose | Needs Docker | Notes |
|-----|---------|--------------|--------|
| **build** | `dotnet build src/MimironsGoldOMatic.slnx` | No | Fast gate. |
| **test-unit** | `dotnet test ... --filter Category=Unit` | No | Matches [`docs/MimironsGoldOMatic.Backend/ReadME.md`](MimironsGoldOMatic.Backend/ReadME.md). |
| **test-integration** | `dotnet test ... --filter Category=Integration` | **Yes** (Testcontainers) | Same as today’s integration slice. |
| **test-e2e-api** (new) | Tier A: Postgres + Backend host in test process + mock Helix | **Yes** | Runs after **test-integration** or in parallel if runners allow; longer timeout (e.g. 15–20 min). |
| **test-e2e-full** (optional, `workflow_dispatch`) | Tier B | Self-hosted + Windows | Disabled by default; document secrets in repo settings. |

**Dependencies:** `test-e2e-api` **needs** `build` (and ideally `test-integration` passing first to reuse confidence).

**Mocks vs real:**

| Component | GitHub-hosted **CI** | Optional **staging** / **self-hosted** |
|-----------|----------------------|----------------------------------------|
| PostgreSQL | Testcontainers (**real** container) | Real managed Postgres |
| Twitch EventSub | **Mock** POST | Real subscription + **ngrok** (manual) |
| Helix | **Mock** HTTP | **Real** `BroadcasterAccessToken` (secret) |
| WoW + Desktop | **Skipped** | **Real** |

### Companion workflow

- **`ci.yml`** (optional split): **build + unit + integration** on every PR; **`e2e-test.yml`** on **`main`** only or **nightly** to save minutes.

---

## 5. Prerequisites

### Accounts and identities (Tier B / live)

- **Twitch:** broadcaster test account; **Extension** in **Testing** or **Released**; **Dev Rig** or hosted Extension; **OAuth** token with scopes for **Send Chat Message** (per Twitch docs and [`TwitchOptions`](../src/MimironsGoldOMatic.Backend/Configuration/TwitchOptions.cs) comments).
- **WoW:** 3.3.5a client + test characters (subscriber flow is Twitch-side; in-game only needs whisper + mail).

### Environment variables / configuration

| Variable / config | Used by | Tier A in CI |
|-------------------|---------|----------------|
| `ConnectionStrings__PostgreSQL` | Marten | From Testcontainers |
| `Mgm__ApiKey` | Desktop + tests | Fixed test secret in workflow env |
| `Twitch__EventSubSecret` | EventSub HMAC | Optional: set to known string + compute signature in test |
| `Twitch__BroadcasterAccessToken` / `BroadcasterUserId` | Helix | Dummy + mock server |
| `Twitch__HelixApiBaseUrl` (proposed) | Helix client | `http://127.0.0.1:<port>` |

### Setup scripts

- **None required** for Tier A beyond **`dotnet restore` / `dotnet test`**.
- **Tier B:** PowerShell or **README** checklist: start Backend, configure Desktop `appsettings`, launch WoW + addon, run **manual** script — keep in [`docs/SETUP.md`](SETUP.md) cross-link.

---

## 6. Success criteria

A **passed** Tier A **E2E** run should demonstrate:

1. **Enrollment:** Pool contains test character (via EventSub mock or claim + JWT).
2. **Spin + verify:** **`Pending`** payout exists with expected `TwitchUserId` / `CharacterName`.
3. **Desktop-equivalent API path:** **`confirm-acceptance`** succeeds when preconditions met; **`PATCH InProgress`** then **`PATCH Sent`** succeed (or minimal path defined in test contract).
4. **Pool on Sent:** Winner removed from pool (assert Marten / HTTP as in existing integration tests).
5. **Helix:** Mock received exactly one **`POST`** with message body matching SPEC §11 template for the winner name.
6. **No unhandled exceptions** in test host logs.

**Tier B** success additionally requires: visible **`[MGM_*]`** tags in **`WoWChatLog.txt`**, mail send, and optional visible Twitch chat line (live Helix).

---

## 7. Risks and mitigations

| Risk | Mitigation |
|------|------------|
| **Mock EventSub diverges** from Twitch payload schema | Versioned JSON **golden files** under `Backend.Tests/Fixtures/`; update when Twitch changelog affects `channel.chat.message`. |
| **Flaky time** in roulette / verify window | Use patterns from [`RouletteVerifyCandidateIntegrationTests`](../src/MimironsGoldOMatic.Backend.Tests/RouletteVerifyCandidateIntegrationTests.cs) (bounded `capturedAt`, known cycle anchors). |
| **Helix URL hardcoded** blocks mock | Add configurable base URL (**small refactor**) — see §3. |
| **Tier A skips real WoW** — false confidence | Label tests **`E2E-API`** vs **`E2E-Full`**; keep **IMPLEMENTATION_READINESS** matrix honest; run Tier B before major releases. |
| **Secrets leak** in workflows | Use GitHub **Environments** + **OIDC** where possible; never log tokens; mock Helix in default **CI**. |
| **GitHub runner Docker limits** | Pin Testcontainers reuse; single Postgres container per test collection (existing **collection** pattern). |

---

## Tier A implementation (repository)

**Status:** Partial — **EventSub relay + Extension JWT issuer + GitHub Actions** are implemented. **MockHelixApi** and **SyntheticDesktop** remain **out of scope** for this slice (see §8).

### MockEventSubWebhook (`src/Mocks/MockEventSubWebhook/`)

- **Purpose:** Stand-in for the **Twitch → EBS** edge. Accepts the same **`POST /api/twitch/eventsub`** shape the real [`TwitchEventSubController`](../src/MimironsGoldOMatic.Backend/Controllers/TwitchEventSubController.cs) expects, verifies **HMAC-SHA256** (`Twitch-Eventsub-Message-*` headers) when `Twitch:EventSubSecret` is set (same algorithm as EBS), logs, then **forwards** the raw body and headers to **`{Backend:BaseUrl}/api/twitch/eventsub`**.
- **Endpoints:** `GET /health`, **`POST /api/twitch/eventsub`**.
- **Configuration:** `Backend:BaseUrl`, `Twitch:EventSubSecret`, **`ASPNETCORE_URLS`** (default local profile **9051** in `Properties/launchSettings.json`).
- **Run:** `dotnet run --project src/Mocks/MockEventSubWebhook/MimironsGoldOMatic.Mocks.MockEventSubWebhook.csproj`

### MockExtensionJwt (`src/Mocks/MockExtensionJwt/`)

- **Purpose:** Issues **HS256** Extension **Bearer** tokens using the **same signing material** as the Backend ([`Program.cs`](../src/MimironsGoldOMatic.Backend/Program.cs): base64 `Twitch:ExtensionSecret`, or Development fallback `SHA256("mgm-dev-extension-secret-change-me")` when secret empty).
- **Endpoints:** `GET /health`, **`GET /token?userId=…&displayName=…`** → JSON `{ "access_token", "token_type", "expires_in" }` for **`GET /api/pool/me`**, **`POST /api/payouts/claim`**, etc.
- **Configuration:** `Twitch:ExtensionSecret`, optional `Twitch:ExtensionClientId` (**aud**). **`ASPNETCORE_URLS`** (default **9052**).
- **Run:** `dotnet run --project src/Mocks/MockExtensionJwt/MimironsGoldOMatic.Mocks.MockExtensionJwt.csproj`

### CI workflow (`.github/workflows/e2e-test.yml`)

- **Trigger:** `pull_request` to **`main`**.
- **Steps (summary):** Start **PostgreSQL 16** service → build **`MimironsGoldOMatic.slnx`** → run **Backend** (`Development`, shared `Twitch:EventSubSecret`) → run both mocks → **Python** [`.github/scripts/send_e2e_eventsub.py`](../.github/scripts/send_e2e_eventsub.py) posts a synthetic **`channel.chat.message`** to the mock → assert **`GET /api/pool/me`** with JWT shows **`isEnrolled: true`** and expected **`characterName`** (`!twgold E2EHero`).

### E2E script

- [`.github/scripts/send_e2e_eventsub.py`](../.github/scripts/send_e2e_eventsub.py) — builds JSON + Twitch HMAC; can target the mock or the EBS directly for debugging.

---

## 8. Next steps (implementation checklist)

1. **Refactor `HelixChatService`** to use configurable Helix base URI (default production). **Verify** existing integration behavior unchanged.
2. **Add `MockHelixHandler`** (or WireMock) and **one** **`E2E`/`Integration`** test: **`Sent`** → outbound HTTP asserted.
3. **Optional:** Add in-repo **`EventSubSignatureHelper`** in **`Backend.Tests`** (or reuse **Python** script logic) for xUnit coverage of **`POST /api/twitch/eventsub`** with **non-empty** `EventSubSecret` — **Tier A CI** already exercises the full path via mocks + script.
4. **Compose chained test** (or harness): enroll → spin tick → verify-candidate → confirm-acceptance → patch statuses → assert Helix mock + pool (**SyntheticDesktop** + **MockHelixApi**).
5. ~~**Create `.github/workflows/e2e-test.yml`**~~ **Done (Tier A).** Optionally add **`ci.yml`** for PR **build/test** only; add **`Category=E2E`** **dotnet test** when in-process E2E tests exist.
6. **Document** test filters in **`docs/MimironsGoldOMatic.Backend/ReadME.md`** (updated for Tier A CI pointer).
7. **Tier B (later):** self-hosted Windows job spec + operator runbook in **`docs/SETUP.md`** (no code change required in this plan).

---

## Document control

| Version | Date | Note |
|---------|------|------|
| 1.0 | 2026-04-05 | Initial plan from **SC-001** + current Backend layout |
| 1.1 | 2026-04-05 | **Tier A:** `MockEventSubWebhook`, `MockExtensionJwt`, `e2e-test.yml`, `send_e2e_eventsub.py` |

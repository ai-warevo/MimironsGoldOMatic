<!-- Created: 2026-04-05 (E2E automation plan) -->
<!-- Updated: 2026-04-05 (Tier B readiness preparation) -->

# E2E automation plan (MVP-6): Chat → WoW → Helix

This document proposes how to automate the **full operator workflow** currently described manually in [`docs/INTERACTION_SCENARIOS.md`](INTERACTION_SCENARIOS.md) (**SC-001**, **SC-005**, and [Automated E2E Scenarios (MVP-6)](INTERACTION_SCENARIOS.md#automated-e2e-scenarios-mvp-6)). It is **planning only**; it does not change product behavior in **`docs/SPEC.md`**.

**Related:** [`docs/ROADMAP.md`](ROADMAP.md) MVP-6, [`docs/IMPLEMENTATION_READINESS.md`](IMPLEMENTATION_READINESS.md) (MVP-6 verification status), [`docs/MimironsGoldOMatic.Backend/ReadME.md`](MimironsGoldOMatic.Backend/ReadME.md) (automated tests). **Implementation checklist / ownership:** [E2E Automation Tasks](E2E_AUTOMATION_TASKS.md). **Tier B task table:** [`docs/TIER_B_IMPLEMENTATION_TASKS.md`](TIER_B_IMPLEMENTATION_TASKS.md).

**Code roots (actual repository layout):**

- Backend (EBS): `src/MimironsGoldOMatic.Backend/` — **not** `MimironsGoldOMatic.WEBAPI.Backend`.
- WoW addon: `src/MimironsGoldOMatic.WoWAddon/`.
- Desktop: `src/MimironsGoldOMatic.Desktop/`.
- CI: `.github/workflows/e2e-test.yml` — **Tier A** (Backend + Postgres + mocks + synthetic EventSub) on **PRs to `main`** (scoped build; no Desktop / addon / Extension). **Unit/integration (parallel):** `.github/workflows/unit-integration-tests.yml` (Backend, Desktop, WoW addon validation, Twitch Extension `npm test`) on the **same** PR trigger. CD: `.github/workflows/release.yml` — **full multi-component build + GitHub Release + GHCR** on **`main`** merges (and optional manual dispatch).
- Tier A mocks: `src/Mocks/MockEventSubWebhook/`, `src/Mocks/MockExtensionJwt/`.
- Tier B mocks (readiness): `src/Mocks/MockHelixApi/`, `src/Mocks/SyntheticDesktop/`; verification scripts: [`scripts/tier_b_verification/`](../scripts/tier_b_verification/).

**CI tier labels (this repository):** **Tier A** is the current [`.github/workflows/e2e-test.yml`](../.github/workflows/e2e-test.yml) job: **Postgres + Backend + mocks + synthetic EventSub → `GET /api/pool/me`**. **Tier B** is the **planned CI extension** with **MockHelixApi**, **SyntheticDesktop**, and a **configurable Helix base URL** (see [Tier B Implementation Plan](#tier-b-implementation-plan-ci-extension)). In **Section 1**, optional **real WoW + self-hosted** validation is **operational / full-stack** work—**not** the same as **CI Tier B**.

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

**Recommendation:** Split **automation** into **CI phases** plus **optional real clients**:

1. **CI Tier A (implemented):** Backend + Postgres (service container) + **MockEventSubWebhook** + **MockExtensionJwt**; verifies **pool enrollment** via synthetic **`channel.chat.message`** and **`GET /api/pool/me`** ([workflow](../.github/workflows/e2e-test.yml)).
2. **CI Tier B (planned):** **MockHelixApi** + **SyntheticDesktop** (HTTP choreography) + **`HelixChatService`** configurable base URL; asserts path through **`Sent`** and captured **Helix** `POST` (see [Tier B Implementation Plan](#tier-b-implementation-plan-ci-extension)).
3. **Operational / full-stack (optional):** Self-hosted or manual runs with real **WoW** + **Desktop** + **Dev Rig / test channel** for UI and WinAPI validation.

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
| **Trigger** | **CI:** existing pattern: seed Marten docs + call [`RouletteCycleTick`](../src/MimironsGoldOMatic.Backend/Services/RouletteCycleTick.cs), then **`POST /api/roulette/verify-candidate`** via [`DesktopRouletteController`](../src/MimironsGoldOMatic.Backend/Controllers/DesktopRouletteController.cs) with header **`X-MGM-ApiKey`** ([`ApiKeyAuthenticationHandler`](../src/MimironsGoldOMatic.Backend/Auth/ApiKeyAuthenticationHandler.cs)). Extend or compose one **linear E2E test** that chains enrollment → tick → verify (today partially split across [`PostClaimRulesIntegrationTests`](../src/Tests/MimironsGoldOMatic.Backend.UnitTests/PostClaimRulesIntegrationTests.cs), [`RouletteVerifyCandidateIntegrationTests`](../src/Tests/MimironsGoldOMatic.Backend.UnitTests/RouletteVerifyCandidateIntegrationTests.cs)). |
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
| **Trigger** | **CI:** **`PATCH /api/payouts/{id}/status`** with **`Sent`** in Tier A test (see [`PatchPayoutStatusIntegrationTests`](../src/Tests/MimironsGoldOMatic.Backend.UnitTests/PatchPayoutStatusIntegrationTests.cs) for pool removal; extend to assert Helix outbound). |
| **Mock / stub** | **MockHelixAPI:** `HttpMessageHandler` fake or **WireMock.NET** listening on loopback; inject **`HttpClient`** base address for **`Helix`** named client. **Today** URL is hardcoded to `https://api.twitch.tv/helix/chat/messages` in `HelixChatService` — **requires a small product change**: e.g. optional **`Twitch:HelixApiBaseUrl`** (empty = production URL) so tests can point to **`http://localhost:9xxx`**. |
| **Verification** | Mock receives **POST** with JSON containing `broadcaster_id`, `sender_id`, `message` matching **`Награда отправлена персонажу {name} на почту, проверяй ящик!`**. |
| **Test data** | [`TwitchOptions`](../src/MimironsGoldOMatic.Backend/Configuration/TwitchOptions.cs): `BroadcasterUserId`, `BroadcasterAccessToken` set to dummy values; mock returns **`204`** / documented Helix success shape. |

---

## 3. Mock services specification

| Mock | Purpose | Simulate | Expected behavior |
|------|---------|----------|-------------------|
| **MockEventSubWebhook** | Replace live Twitch → EBS delivery | **`POST /api/twitch/eventsub`** | Body includes `subscription.type` = `channel.chat.message`, `event.message_id`, `event.chatter_user_id`, `event.message.text`, `event.badges`. Headers: `Twitch-Eventsub-Message-Id`, `Timestamp`, `Signature` when secret configured. |
| **MockExtensionJwt** | Auth for **`/api/pool/me`**, **`/api/payouts/claim`**, etc. | HS256 JWT | Signed with same key as [`Program.cs`](../src/MimironsGoldOMatic.Backend/Program.cs) dev derivation or test secret; claims: `user_id`, optional `display_name`. |
| **MockHelixApi** | No real Twitch chat send in CI | **`POST /helix/chat/messages`** (path as configured) | **`GET /health`** → **`status: healthy`**, **`component: MockHelixApi`**. **`POST`** → **204**; last body on **`GET /last-request`**. Optional **`MockHelix:StrictAuth=true`** enforces **`Bearer`** + **`Client-Id`**. |
| **MockWoWClient** (conceptual) | Replace WoW + WinAPI in Tier A | N/A in process | **Not implemented as a service** in Tier A — replaced by **API choreography**. Tier B only: optional scripted window focus tools (out of scope for default **CI**). |
| **SyntheticDesktop** | Replace Desktop executable in CI Tier B | Sequences of **`HttpClient`** calls | **`GET /health`** → **`healthy`**, **`component: SyntheticDesktop`**. **`POST /run-sequence`** with **`payoutId`** + **`characterName`** → **`confirm-acceptance`** → **`PATCH InProgress`** → **`PATCH Sent`**. **`GET /last-run`** returns step statuses. |

### Suggested code structure (Backend tests)

- **`HelixChatService` refactor (small):** inject `IOptions<TwitchOptions>` and optional **`Uri HelixApiBase`** (default `https://api.twitch.tv`). Tests register **`HttpClient`** with **`PrimaryHttpMessageHandler`** = **`StubHelixHandler`**.
- **`EventSubSignatureHelper` (test project):** static method to compute `sha256=` HMAC from Twitch headers + body for golden tests.
- **`E2EApiTierATests` (new):** one test class, **`[Trait("Category","E2E")`** or **`Integration`**, building `WebApplicationFactory` **or** reusing [`BackendTestHost`](../src/Tests/MimironsGoldOMatic.Backend.UnitTests/Support/) pattern with mocked `IHttpClientFactory` for **`Helix`**.

---

## 4. CI/CD pipeline design

### Proposed workflow: `.github/workflows/e2e-test.yml`

**As implemented (CI Tier A):** [`.github/workflows/e2e-test.yml`](../.github/workflows/e2e-test.yml) is a **single job** (`e2e-tier-a`) with a **PostgreSQL 16 service container**, **not** the multi-job split in the table below. The table remains the **longer-term** layout (optional companion **`ci.yml`**, separate unit/integration jobs, future **dotnet test** E2E).

High-level **jobs** (all **Ubuntu** unless self-hosted operational validation):

| Job | Purpose | Needs Docker | Notes |
|-----|---------|--------------|--------|
| **build** | `dotnet build src/MimironsGoldOMatic.slnx` | No | Fast gate. |
| **test-unit** | `dotnet test ... --filter Category=Unit` | No | Matches [`docs/MimironsGoldOMatic.Backend/ReadME.md`](MimironsGoldOMatic.Backend/ReadME.md). |
| **test-integration** | `dotnet test ... --filter Category=Integration` | **Yes** (Testcontainers) | Same as today’s integration slice. |
| **test-e2e-api** (new) | Tier A: Postgres + Backend host in test process + mock Helix | **Yes** | Runs after **test-integration** or in parallel if runners allow; longer timeout (e.g. 15–20 min). |
| **test-e2e-full** (optional, `workflow_dispatch`) | Operational / full-stack (real WoW) | Self-hosted + Windows | Disabled by default; document secrets in repo settings. |

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

## CI/CD Pipeline Architecture

This section describes the **split** between **fast PR validation** (Tier A E2E) and **post-merge release** (all shipping components). Normative workflow files: [`.github/workflows/e2e-test.yml`](../.github/workflows/e2e-test.yml), [`.github/workflows/release.yml`](../.github/workflows/release.yml).

### Workflow A — `e2e-test.yml` (fast PR validation)

| Item | Detail |
|------|--------|
| **Trigger** | `pull_request` targeting **`main`** only. |
| **Runner** | `ubuntu-latest`. |
| **Build scope** | **Shared + Backend + MockEventSubWebhook + MockExtensionJwt** only (excludes **Desktop**, **Backend.UnitTests**, **WoW addon**, **Twitch Extension**) for shorter wall time and fewer prerequisites. |
| **Data plane** | PostgreSQL **16** service container (unchanged health / port / credentials pattern). |
| **Runtime** | Backend + both mocks on loopback; **Python** [`.github/scripts/send_e2e_eventsub.py`](../.github/scripts/send_e2e_eventsub.py) + **`curl`** assertions unchanged from Tier A. |
| **Proof** | Synthetic **`channel.chat.message`** → **`GET /api/pool/me`** enrollment check (`Etoehero`). |
| **Caching** | Optional **NuGet** cache (`~/.nuget/packages`, key from `src/**/*.csproj`) — see workflow comments. |

### Workflow B — `release.yml` (full build, artifacts, GHCR, GitHub Release)

| Trigger | When it runs |
|---------|----------------|
| **`push` to `main`** | After a PR merge (or direct push) to **`main`**. |
| **`workflow_dispatch`** | Emergency / manual run; optional **`version`** input (`1.2.3` without `v`) overrides the default auto tag **`v0.0.<run_number>`**. |

**Parallel jobs (1–4)** — no cross-dependencies; all use the same **resolved `RELEASE_VERSION`** logic for consistent ZIP names and image tags.

| Job | Runner | Component | Outputs |
|-----|--------|-----------|---------|
| **`build-desktop`** | `windows-latest` | **MimironsGoldOMatic.Desktop** (`dotnet publish`, `win-x64`, framework-dependent) | ZIP **`MimironsGoldOMatic-Desktop-<RELEASE_VERSION>-win.zip`** + embedded **`README.txt`** → artifact **`desktop-release`** |
| **`build-wowaddon`** | `ubuntu-latest` | **`src/MimironsGoldOMatic.WoWAddon`** (`.toc`, `.lua`, manifest text; future locale folders can be added to the same pack step) | ZIP **`WoWAddon-<RELEASE_VERSION>.zip`** + **`README.txt`** → **`wow-addon-release`** |
| **`build-twitch-extension`** | `ubuntu-latest` | **`src/MimironsGoldOMatic.TwitchExtension`** — `npm ci`, `npm run build` (Vite) | ZIP **`TwitchExtension-<RELEASE_VERSION>.zip`** containing **`dist/`** + **`README.txt`** → **`twitch-extension-release`** |
| **`build-backend-docker`** | `ubuntu-latest` | **Backend** Docker image from [`src/MimironsGoldOMatic.Backend/Dockerfile`](../src/MimironsGoldOMatic.Backend/Dockerfile) (build context **`src/`**) | Push **`ghcr.io/<owner-lowercase>/mimirons-goldomatic-backend:<RELEASE_VERSION>`** and **`:latest`** (OIDC/`GITHUB_TOKEN` with `packages: write`) |

**Sequential job (5)** — **must not start** until **all four** build jobs succeed (`needs:`).

| Job | Runner | Depends on | Behavior |
|-----|--------|------------|----------|
| **`create-release`** | `ubuntu-latest` | **`build-desktop`**, **`build-wowaddon`**, **`build-twitch-extension`**, **`build-backend-docker`** | Downloads **`desktop-release`**, **`wow-addon-release`**, **`twitch-extension-release`**; writes **`SHA256SUMS.txt`** for all ZIPs; builds release notes from **`git log`** since previous tag (fallback: last 50 commits); creates **GitHub Release** tagged **`RELEASE_VERSION`**; attaches ZIPs + checksum file; body links **GHCR** repo, tags, and image **digest** (when provided by the Docker push step). |

**Pipeline flow (text diagram):**

```text
Push to main (or workflow_dispatch)
        |
        +--> [build-desktop] (windows-latest) ------> artifact: desktop-release
        |
        +--> [build-wowaddon] (ubuntu-latest) ------> artifact: wow-addon-release
        |
        +--> [build-twitch-extension] (ubuntu-latest) -> artifact: twitch-extension-release
        |
        +--> [build-backend-docker] (ubuntu-latest) -> GHCR image + digest
        |
        +------------------------- (all four succeeded) -------------------------+
                                                                                 |
                                                                                 v
                                                                    [create-release]
                                                         (downloads ZIPs, SHA256SUMS,
                                                          GitHub Release + notes)
```

### Benefits of this split

- **Cost / time:** PRs avoid **Windows** Desktop builds, **npm** Extension builds, and **Docker** publish unless merging to **`main`**.
- **Separation of concerns:** Tier A proves the **chat → pool** contract; release packaging stays in **`release.yml`**.
- **Completeness:** One **GitHub Release** bundles **Desktop + addon + Extension** ZIPs and documents the **Backend** image.
- **Parallelism:** Independent build jobs minimize wall-clock time before the release step.
- **Safety:** **`create-release`** runs **only after** every build/publish job has passed, so partial failures do not publish a “complete” release.

### Team discussion hooks

- **Versioning:** Default **`v0.0.<run_number>`** on **`main`** pushes is predictable but may not match marketing semver; **`workflow_dispatch`** allows explicit **`1.2.3`**. Alternatives: tag-only releases, `VERSION` file in repo, or GitVersion.
- **Artifact retention:** Workflow uploads use **90-day** retention (tunable); align with compliance and storage budgets.
- **GHCR permissions:** Package visibility (public vs private) and org **OIDC** / token policies should match how streamers/operators pull images.
- **Failed build jobs:** Any failure in jobs **1–4** skips **`create-release`** by design — confirm this is the desired **“all green or no release”** policy.

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
| **Mock EventSub diverges** from Twitch payload schema | Versioned JSON **golden files** under `src/Tests/MimironsGoldOMatic.Backend.UnitTests/Fixtures/` (or similar); update when Twitch changelog affects `channel.chat.message`. |
| **Flaky time** in roulette / verify window | Use patterns from [`RouletteVerifyCandidateIntegrationTests`](../src/Tests/MimironsGoldOMatic.Backend.UnitTests/RouletteVerifyCandidateIntegrationTests.cs) (bounded `capturedAt`, known cycle anchors). |
| **Helix URL hardcoded** blocks mock | Add configurable base URL (**small refactor**) — see §3. |
| **Tier A skips real WoW** — false confidence | Label tests **`E2E-API`** vs **`E2E-Full`**; keep **IMPLEMENTATION_READINESS** matrix honest; run Tier B before major releases. |
| **Secrets leak** in workflows | Use GitHub **Environments** + **OIDC** where possible; never log tokens; mock Helix in default **CI**. |
| **GitHub runner Docker limits** | Pin Testcontainers reuse; single Postgres container per test collection (existing **collection** pattern). |

---

## Tier A implementation (repository)

**Status:** **Enrollment path complete in CI** — **EventSub relay + Extension JWT issuer + GitHub Actions** are implemented and documented ([runbook](#how-to-run-tier-a-e2e-tests-github-actions)). **MockHelixApi** and **SyntheticDesktop** are **CI Tier B** ([plan](#tier-b-implementation-plan-ci-extension)).

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
- **Steps (summary):** Start **PostgreSQL 16** service → **scoped** `dotnet build` (**Shared + Backend + both mocks** only) → run **Backend** (`Development`, shared `Twitch:EventSubSecret`) → run both mocks → **Python** [`.github/scripts/send_e2e_eventsub.py`](../.github/scripts/send_e2e_eventsub.py) posts a synthetic **`channel.chat.message`** to the mock → assert **`GET /api/pool/me`** with JWT shows **`isEnrolled: true`** and expected **`characterName`** (`!twgold Etoehero`).

### E2E script

- [`.github/scripts/send_e2e_eventsub.py`](../.github/scripts/send_e2e_eventsub.py) — builds JSON + Twitch HMAC; can target the mock or the EBS directly for debugging.

---

## Tier A Test Results & Verification

This section records **observed CI behavior** for workflow **[E2E Tier A (mocks)](https://github.com/ai-warevo/MimironsGoldOMatic/actions/workflows/e2e-test.yml)** ([`.github/workflows/e2e-test.yml`](../.github/workflows/e2e-test.yml)). Metrics below were taken from the **GitHub Actions API** on **2026-04-05** (public repo **`ai-warevo/MimironsGoldOMatic`**); re-query for up-to-date numbers: `GET /repos/{owner}/{repo}/actions/workflows/e2e-test.yml/runs`.

### Run history summary

| Metric | Value |
|--------|--------|
| **Total workflow runs** (all time) | **23** |
| **Succeeded** | **19** |
| **Failed** | **3** (early iterations while wiring CI; see team discussion on flake vs config) |
| **Cancelled** | **1** |
| **Success rate** (completed runs only: success / (success + failure)) | **19 / 22 ≈ 86%** |
| **Success rate** (all recorded runs) | **19 / 23 ≈ 83%** |

**Recent passing run (example):** [Workflow run #23](https://github.com/ai-warevo/MimironsGoldOMatic/actions/runs/24004723814) — conclusion **success**, wall-clock **~71 s** from `run_started_at` to `updated_at` (2026-04-05).

### Execution time (successful runs)

Across **19** runs with **`conclusion: success`**, GitHub-reported duration (**`updated_at` − `run_started_at`**) aggregated as:

| Stat | Seconds |
|------|---------|
| **Average** | **~64 s** |
| **Min** | **~59 s** |
| **Max** | **~71 s** |

This aligns with the [expected execution time](#expected-execution-time-order-of-magnitude) table (scoped build + Postgres + three ASP.NET processes + Python + curl).

### Resource usage

GitHub-hosted **`ubuntu-latest`** does not expose per-job CPU/RAM in the public API. **Qualitative:** Tier A uses one job, one PostgreSQL service container, three **`dotnet run`** processes (Backend + two mocks), and short **Python**/**curl** steps — typical **well under** the default **7 GB** RAM / **2 CPU** runner envelope. For regressions, use **Actions → Insights → Workflow runs** (duration trends).

### Log excerpts (expected successful patterns)

These patterns confirm **HMAC verification**, **JWT** use, and **EventSub → pool** processing without pasting secrets.

**1. Synthetic EventSub send ([`send_e2e_eventsub.py`](../.github/scripts/send_e2e_eventsub.py)) — HTTP success**

```text
send_e2e_eventsub: HTTP 200
```

**2. Pool enrollment assertion — Extension JWT + `GET /api/pool/me`**

```text
Got JWT (… chars)
GET /api/pool/me => {"isEnrolled":true,"characterName":"Etoehero", ...}
E2E Tier A: pool enrollment verified.
```

**3. HMAC path** — The workflow sets **`E2E_EVENTSUB_SECRET`** and passes **`--secret`** to Python; the mock verifies **`Twitch-Eventsub-Message-Signature`** (`sha256=` HMAC over **`message-id` + `timestamp` + raw body**) before forwarding to **`POST /api/twitch/eventsub`**. A successful run implies **no 401** from the mock or EBS on that POST.

**4. JWT path** — **`MockExtensionJwt`** issues **`GET /token?userId=e2e-viewer-1&displayName=…`**; Backend **`Development`** uses the same signing material as the mock when **`Twitch:ExtensionSecret`** is empty (see [`Program.cs`](../src/MimironsGoldOMatic.Backend/Program.cs)). A successful **`GET /api/pool/me`** confirms **Bearer** validation and **`user_id`** alignment with enrollment.

### Screenshots / deep links

- **Workflow list:** [Actions → E2E Tier A (mocks)](https://github.com/ai-warevo/MimironsGoldOMatic/actions/workflows/e2e-test.yml)
- **Example green run:** [Run #23](https://github.com/ai-warevo/MimironsGoldOMatic/actions/runs/24004723814) — open the **`e2e-tier-a`** job and expand steps for full logs (browser screenshot optional for reports).

### Tier A Validation Checklist — completion

All items in **[Tier A Validation Checklist](E2E_AUTOMATION_TASKS.md#tier-a-validation-checklist)** are **verified** for the current workflow definition: PR→**`main`**, scoped Backend+mocks build, Postgres **16** service, mock health ports **9051**/**9052**, HMAC + JWT + enrollment **`Etoehero`**. The checklist file uses **`[x]`** markers for ongoing tracking.

---

## How to run Tier A E2E tests (GitHub Actions)

### Triggering the workflow

1. Open a **pull request** whose **base branch** is **`main`** (or push commits to an existing PR targeting `main`).
2. GitHub runs workflow **`E2E Tier A (mocks)`** (file [`.github/workflows/e2e-test.yml`](../.github/workflows/e2e-test.yml)) automatically on `pull_request` for **`main`**.
3. In the PR, open **Checks** → select the workflow run → inspect the **`e2e-tier-a`** job log.

**Manual re-run:** From the **Actions** tab, open the workflow run and use **Re-run jobs** (same commit).

### Prerequisites

| Prerequisite | Notes |
|--------------|--------|
| **Repository permissions** | **Actions** enabled for the repo; contributors need permission to run workflows (fork PRs may require maintainer approval for first-time contributors, per org settings). |
| **GitHub Secrets** | **None required** for current Tier A: `E2E_EVENTSUB_SECRET` is set inline in the workflow `env` block (not a repo secret). |
| **Branch** | Workflow is bound to **`pull_request` → `main`** only; PRs to other branches do **not** run this job unless the workflow `on:` section is extended. |
| **Runner image** | **`ubuntu-latest`**: includes **.NET SDK** (via `setup-dotnet`), **Python 3**, **curl**; **PostgreSQL 16** is provided by the job **`services.postgres`** container (`postgres:16-alpine`), not the host OS. |

### Expected execution time (order of magnitude)

Times vary with cold cache and NuGet restore; values below are **typical** for a small solution on GitHub-hosted runners.

| Stage | What happens | Typical duration |
|--------|----------------|------------------|
| **Job setup** | Checkout, `setup-dotnet` | ~30–90 s |
| **PostgreSQL** | Service container start + `pg_isready` health checks | ~10–60 s (often toward the lower end once healthy) |
| **Build** | Scoped `dotnet build` (Shared + Backend + mocks) `-c Release` (includes restore) | Often **~1–3 min** (typically faster than full solution) |
| **Backend start** | Background `dotnet run`; wait loop up to **90 × 1 s** | Usually a few seconds; **worst case ~90 s** if the app is slow to bind |
| **MockEventSubWebhook** | Background `dotnet run`; health poll up to **60 s** | Usually under **10 s** |
| **MockExtensionJwt** | Same pattern on **9052** | Usually under **10 s** |
| **Send + verify** | Python script + `curl` + JSON assert | ~5–15 s |
| **Total job** | End-to-end | With NuGet cache warm, often **under ~5 min**; allow **~10–15 min** on cold cache or runner load |

### Success criteria (passing Tier A)

The job **passes** when all steps are green and, specifically:

1. **Backend** accepts HTTP on **`http://127.0.0.1:8080`** within the wait loop.
2. **MockEventSubWebhook** returns **`GET /health`** successfully on **`:9051`**.
3. **MockExtensionJwt** returns **`GET /health`** successfully on **`:9052`**.
4. **Python** [`.github/scripts/send_e2e_eventsub.py`](../.github/scripts/send_e2e_eventsub.py) completes with exit code **0** (synthetic **`channel.chat.message`** accepted by the mock and forwarded to EBS).
5. **`GET /api/pool/me`** with the issued Bearer token returns JSON with **`isEnrolled: true`** and **`characterName`** **`Etoehero`** (matching `!twgold Etoehero` in the script).

On **failure**, the workflow runs a **Logs (on failure)** step with backend PID and a diagnostic **`curl`** to the backend root.

---

## Predictive issue analysis (Tier A CI)

| Risk area | Root cause (likely) | Prevention | Troubleshooting |
|-----------|---------------------|------------|-----------------|
| **Ports 8080 / 9051 / 9052** | Another process on the runner binding the same ports (uncommon on clean `ubuntu-latest`, but possible if defaults change or multiple apps run). | Keep **loopback-only** URLs as in the workflow; avoid adding parallel jobs on the same job container that reuse these ports. | In failed logs, check “address already in use”; consider explicit `ASPNETCORE_URLS` or moving to ephemeral ports + config (future hardening). |
| **HMAC / EventSub signature** | **Secret mismatch** between mock, Backend, and Python (`Twitch__EventSubSecret` vs `--secret`); **wrong signing payload** (must be `message-id + timestamp + raw body` bytes); **JSON canonicalization** differing between signer and verifier (Python uses `separators=(',', ':')`—body must match what EBS hashes). | Single source of truth: workflow `env.E2E_EVENTSUB_SECRET`; never log the secret. | Reproduce locally with the same secret and `send_e2e_eventsub.py`; compare **401** from mock vs EBS; enable Development logging on mock/EBS. |
| **PostgreSQL service** | Container slow to become ready; wrong **`ConnectionStrings__PostgreSQL`** (host/port/db/user/password). | Workflow uses **`localhost:5432`** and matches **`POSTGRES_DB`/`PASSWORD`**; health check **`pg_isready`**. | Inspect **Services** logs in the Actions UI; verify Marten can connect (Backend would fail startup or first DB use). |
| **Mock / Backend startup timeouts** | Heavy cold restore, AV, or hung `dotnet run`; DB migrations taking longer than the loop. | Add **`actions/cache`** for NuGet (see [Optimization and scalability](#optimization-and-scalability-ci)); keep `--no-build` after a successful Release build. | Increase wait loops only if justified; check whether Backend blocks before Kestrel listens. |
| **Python script** | Missing **`python3`** (present on `ubuntu-latest`); wrong **`--url`**; HTTP errors from mock (signature or connection). | Pin runner image label in docs; script already uses stdlib only. | Run script locally against local mocks; print response body (script prints on **HTTPError**). |
| **JWT / `GET /api/pool/me`** | **Signing key mismatch**: Backend **Development** uses SHA256 of **`mgm-dev-extension-secret-change-me`** when **`Twitch:ExtensionSecret`** is empty; **MockExtensionJwt** uses the same rule. **Production** config without secret would throw at startup—CI uses **Development**. | CI sets **`ASPNETCORE_ENVIRONMENT: Development`**; do not set **`Twitch__ExtensionClientId`** on the mock unless Backend validates **`aud`** with the same value. | Decode JWT at [jwt.io](https://jwt.io) (local only); verify claims include **`user_id`** matching enrollment **`chatter_user_id`**. |
| **Enrollment assertion** | **`Mgm__DevSkipSubscriberCheck`** not set (subscriber rules); duplicate **`message_id`** dedupe; non-subscriber badge payload. | Workflow sets **`Mgm__DevSkipSubscriberCheck: "true"`**; Python sends subscriber **`badges`**. | Inspect **`GET /api/pool/me`** body in logs; query pool state via integration tests pattern if needed. |
| **Fork PR workflows** | GitHub may not pass secrets to forks; this workflow uses **no** repo secrets today—low risk. | If later adding secrets, use **Environment** rules and document fork behavior. | Check workflow policy for **“Run workflows from fork pull requests”**. |

---

## Tier B Implementation Plan (CI extension)

**Objective:** Extend CI so the **EBS payout path** is exercised **without WoW**: **SyntheticDesktop** issues the same HTTP sequence the real Desktop would, and **MockHelixApi** captures **`POST .../helix/chat/messages`** (or equivalent path under a configurable base URL). Aligns with [§6 Success criteria](#6-success-criteria) items **2–5** and tasks **C**/**D** in [E2E_AUTOMATION_TASKS.md](E2E_AUTOMATION_TASKS.md).

### Dependencies on Tier A

| Tier A component | Tier B dependency |
|------------------|-------------------|
| **Postgres service + Marten** | Required for full payout state and pool removal. |
| **Backend on loopback** | All Desktop and Helix traffic targets the same EBS instance. |
| **`e2e-test.yml` pattern** | Reuse **build → background processes → scripted HTTP**; add services/ports for **MockHelixApi** (and optionally fold **SyntheticDesktop** into the same job as a script or small process). |
| **MockEventSubWebhook + signing** | Optional for Tier B if enrollment is seeded via Marten/API instead; keeping Tier A enrollment step preserves a **full vertical slice** from chat to **`Sent`**. |

### A. Configurable Helix URL

| Step | Task | Details |
|------|------|---------|
| **A1** | **Update [`HelixChatService.cs`](../src/MimironsGoldOMatic.Backend/Services/HelixChatService.cs)** | Today the POST URL is a **constant** (`https://api.twitch.tv/helix/chat/messages`). Replace with a **base URI** from `IOptions<TwitchOptions>` plus a fixed relative path **`/helix/chat/messages`** when the named **`HttpClient`** is configured with `BaseAddress`. If **`HelixApiBaseUrl`** is **empty**, register **`Helix`** client with base **`https://api.twitch.tv`** (current behavior). |
| **A2** | **Configuration validation and defaults** | Add **`HelixApiBaseUrl`** (optional string) to [`TwitchOptions.cs`](../src/MimironsGoldOMatic.Backend/Configuration/TwitchOptions.cs). On startup, if set: **must** be absolute HTTP(S); trim trailing slash. Document interaction with existing **`BroadcasterAccessToken`**, **`BroadcasterUserId`**, **`HelixClientId`** (still required for a real outbound call). |
| **A3** | **`appsettings` schema** | Document in [`appsettings.Development.json`](../src/MimironsGoldOMatic.Backend/appsettings.Development.json) / comments: **`Twitch:HelixApiBaseUrl`** example `http://127.0.0.1:9053` for local MockHelixApi. No secret in this field. |

**Success criteria (A):** With **`HelixApiBaseUrl`** unset, integration/unit tests show **identical** request line to production Twitch. With base `http://localhost:9053`, **`HelixChatService`** POSTs only to the mock; **`HelixChatServiceTests`** updated or extended to cover both paths.

### B. MockHelixApi

| Step | Task | Details |
|------|------|---------|
| **B1** | **New project** | **`src/Mocks/MockHelixApi/`** — **.NET 10.0**, ASP.NET Core minimal API, namespace **`MimironsGoldOMatic.Mocks.MockHelixApi`**, added to [`MimironsGoldOMatic.slnx`](../src/MimironsGoldOMatic.slnx). |
| **B2** | **`POST /helix/chat/messages`** | Match Twitch Helix shape: accept JSON with **`broadcaster_id`**, **`sender_id`**, **`message`**. Echo **`Authorization: Bearer`** and **`Client-Id`** validation (optional strict mode for CI). Return **`204`** or **`200`** with documented empty/small JSON body (align with [`HelixChatService`](../src/MimironsGoldOMatic.Backend/Services/HelixChatService.cs) success handling). |
| **B3** | **Response templates** | Implement **success** plus **401** / **500** branches for future retry tests (see [`HelixChatServiceTests`](../src/Tests/MimironsGoldOMatic.Backend.UnitTests/Unit/HelixChatServiceTests.cs)). Store **last request body** in memory for **`GET /last-request`** (JSON) or structured logs for **`curl`** assertions. |
| **B4** | **`GET /health`** | JSON **`{ "status": "healthy", "component": "MockHelixApi" }`** (Tier B mocks use **`component`** + **`healthy`**; Tier A mocks remain **`status`:** **`ok`** + **`service`**). |
| **B5** | **E2E workflow** | Start with **`ASPNETCORE_URLS=http://127.0.0.1:9053`** (or next free port); Backend **`Twitch__HelixApiBaseUrl`** points here; see **D** below. |

**Success criteria (B):** Health green; exactly one **`POST`** recorded after a test **`PATCH`** to **`Sent`** when Tier B chain runs; message text matches SPEC §11 Russian template for the winner name.

### C. SyntheticDesktop

| Step | Task | Details |
|------|------|---------|
| **C1** | **New project** | **`src/Mocks/SyntheticDesktop/`** — console or minimal host (**.NET 10.0**) that runs a **scripted HTTP sequence** only (no WPF). Optional name: **`MimironsGoldOMatic.Mocks.SyntheticDesktop`**. |
| **C2** | **HTTP client sequence** | Use **`HttpClient`** with header **`X-MGM-ApiKey`** = same as workflow **`Mgm__ApiKey`**. Base address = Backend **`http://127.0.0.1:8080`**. Order: resolve **`payoutId`** (e.g. from **`GET /api/payouts/pending`** or seeded state) → **`POST /api/payouts/{id}/confirm-acceptance`** → **`PATCH /api/payouts/{id}/status`** with **`InProgress`** → **`PATCH`** with **`Sent`** (exact JSON bodies per [`DesktopPayoutsController`](../src/MimironsGoldOMatic.Backend/Controllers/DesktopPayoutsController.cs) / DTOs). |
| **C3** | **`confirm-acceptance` and `PATCH` flows** | Preconditions must match domain rules (acceptance window, pool membership). Reuse seeding patterns from [`RouletteVerifyCandidateIntegrationTests`](../src/Tests/MimironsGoldOMatic.Backend.UnitTests/RouletteVerifyCandidateIntegrationTests.cs) and [`PatchPayoutStatusIntegrationTests`](../src/Tests/MimironsGoldOMatic.Backend.UnitTests/PatchPayoutStatusIntegrationTests.cs). |
| **C4** | **Verification endpoints** | Expose **`GET /last-run`** (JSON: **`ok`**, **`steps`**, **`error`**) after **`POST /run-sequence`**; exit code **0** from CI when all steps return **2xx**. |

**Success criteria (C):** End-to-end: after run, payout **`Sent`**, pool row removed for winner, **MockHelixApi** received announcement (with **A** + **B** in place).

### D. Workflow integration

| Step | Task | Details |
|------|------|---------|
| **D1** | **Extend [`.github/workflows/e2e-test.yml`](../.github/workflows/e2e-test.yml)** | After MockExtensionJwt: start **MockHelixApi** (background + health poll). Set Backend env: **`Twitch__HelixApiBaseUrl`**, dummy **`Twitch__BroadcasterAccessToken`**, **`Twitch__BroadcasterUserId`**, **`Twitch__HelixClientId`** so **`HelixChatService`** does not early-return. |
| **D2** | **Test script** | Add **Python** or **bash** step **or** `dotnet run --project SyntheticDesktop` after Tier A enrollment + seeded **`Pending`** path: optionally keep **`send_e2e_eventsub.py`** as enrollment, then run roulette tick APIs or Marten seed (team choice — see discussion below). |
| **D3** | **Assertions** | **`curl`** MockHelixApi **`GET /last-request`** (or grep logs) for Russian message; assert **`GET /api/pool/me`** or Marten state shows winner removed. |

**Success criteria (D):** Single job still completes within acceptable minutes budget; failure logs show which step failed (Helix vs Desktop sequence vs Backend).

### Rolled-up task list (owners and estimates)

| ID | Owner | Est. | Description |
|----|--------|------|-------------|
| **B5 / A*** | Backend Dev | **1 d** | **Helix** configurable URL + tests (**A1–A3**). |
| **B1–B4** | Backend Dev | **1–1.5 d** | **MockHelixApi** project + endpoints + templates. |
| **C1–C4** | Backend Dev | **1.5–2 d** | **SyntheticDesktop** sequence + seeding. |
| **D1–D3** | DevOps / Backend Dev | **1 d** | Workflow + assertions. |

**Suggested order:** **A (Helix URL)** → **B (MockHelixApi)** → **C (SyntheticDesktop)** against local stack → **D (CI)**.

Full traceability table: [`docs/TIER_B_IMPLEMENTATION_TASKS.md`](TIER_B_IMPLEMENTATION_TASKS.md).

---

## Tier B Readiness Verification

Run these checks **before** wiring Tier B into [`.github/workflows/e2e-test.yml`](../.github/workflows/e2e-test.yml) or executing the [Tier B First Run Guide](#tier-b-first-run-guide). **Pre-launch checkbox file:** [`docs/TIER_B_PRELAUNCH_CHECKLIST.md`](TIER_B_PRELAUNCH_CHECKLIST.md).

### MockHelixApi

| Check | How | Success criteria |
|-------|-----|------------------|
| **Process / bind** | `dotnet run` with **`ASPNETCORE_URLS=http://127.0.0.1:9053`** | Kestrel listens; no “address already in use”. |
| **Health** | `GET http://127.0.0.1:9053/health` | **200** JSON **`{"status":"healthy","component":"MockHelixApi"}`**. |
| **Base URL alignment** | Backend (after **A1–A2**) uses same host/port as mock root | **`HelixChatService`** POST targets **`{base}/helix/chat/messages`** (leading slash on relative path). |
| **POST + capture** | [`scripts/tier_b_verification/check_mockhelixapi.py`](../scripts/tier_b_verification/check_mockhelixapi.py) | Exit **0**; **`GET /last-request`** shows **`captured: true`** and body **`message`** matching probe. |
| **Response templates** | Mock returns **204** for successful **`POST /helix/chat/messages`** | Matches [`HelixChatService`](../src/MimironsGoldOMatic.Backend/Services/HelixChatService.cs) success handling (**2xx**). |

Optional strict auth: set **`MockHelix__StrictAuth=true`** on the mock to require **`Authorization: Bearer`** and **`Client-Id`** (mirrors production headers).

### SyntheticDesktop

| Check | How | Success criteria |
|-------|-----|------------------|
| **Process / bind** | `dotnet run` with **`ASPNETCORE_URLS=http://127.0.0.1:9054`** | Listens on **9054**. |
| **Health** | `GET http://127.0.0.1:9054/health` | **200** JSON **`{"status":"healthy","component":"SyntheticDesktop"}`**. |
| **Config** | Env **`Mgm__ApiKey`** matches Backend **`Mgm__ApiKey`**; **`SyntheticDesktop__BackendBaseUrl`** points at EBS | **`POST /run-sequence`** does not fail with “ApiKey not configured”. |
| **Sequence (integration)** | Seed **`Pending`** payout, then [`check_syntheticdesktop.py`](../scripts/tier_b_verification/check_syntheticdesktop.py) **`--payout-id {guid}`** | **`POST /run-sequence`** returns **`ok: true`**; **`GET /last-run`** lists **confirm-acceptance** → **InProgress** → **Sent** with **2xx** status codes. |
| **Verification endpoint** | `GET http://127.0.0.1:9054/last-run` after a run | JSON includes **`ok`**, **`steps`**, **`error`** (null on success). |

### Workflow integration (ports + order)

| Check | How | Success criteria |
|-------|-----|------------------|
| **Port map** | Compare running processes to table below | **8080** Backend, **9051** EventSub mock, **9052** JWT mock, **9053** Helix mock, **9054** SyntheticDesktop — no duplicate binders. |
| **Startup order** | Start **Postgres** → **Backend** → Tier A mocks → **MockHelixApi** → **SyntheticDesktop** | [`check_workflow_integration.py`](../scripts/tier_b_verification/check_workflow_integration.py) exit **0** (omit **`--skip-tier-b`**). |
| **Tier A regression** | `python3 scripts/tier_b_verification/check_workflow_integration.py --skip-tier-b` with Tier A stack only | Still passes when Tier B processes are stopped. |

**Port map (default local / planned CI):**

| Port | Service |
|------|---------|
| **5432** | PostgreSQL (**service container** in Actions) |
| **8080** | **MimironsGoldOMatic.Backend** |
| **9051** | **MockEventSubWebhook** |
| **9052** | **MockExtensionJwt** |
| **9053** | **MockHelixApi** |
| **9054** | **SyntheticDesktop** |

---

## Tier B First Run Guide

End-to-end **first** Tier B rehearsal on a developer machine (still requires **Helix base URL** product change **A1–A2** before Backend traffic hits the mock; until then, complete only **readiness** scripts and mock-local checks).

1. **Prerequisites:** Docker or local **PostgreSQL 16**, **.NET 10 SDK**, **Python 3** + `pip install -r scripts/tier_b_verification/requirements.txt`.
2. **Tier A stack:** Follow **Running Tier A E2E locally (manual)** in [`docs/MimironsGoldOMatic.Backend/ReadME.md`](MimironsGoldOMatic.Backend/ReadME.md) (Postgres + Backend + **9051** + **9052** + synthetic EventSub + pool assertion).
3. **Start MockHelixApi:**  
   `ASPNETCORE_URLS=http://127.0.0.1:9053 dotnet run --project src/Mocks/MockHelixApi/MimironsGoldOMatic.Mocks.MockHelixApi.csproj -c Release`
4. **Verify mock alone:** `python3 scripts/tier_b_verification/check_mockhelixapi.py --base-url http://127.0.0.1:9053`
5. **Configure Backend for mock Helix (after A1–A2 merge):** set **`Twitch__HelixApiBaseUrl=http://127.0.0.1:9053`**, non-empty **`Twitch__BroadcasterAccessToken`**, **`Twitch__BroadcasterUserId`**, **`Twitch__HelixClientId`**; restart Backend.
6. **Start SyntheticDesktop:**  
   `ASPNETCORE_URLS=http://127.0.0.1:9054`  
   `Mgm__ApiKey=<same as Backend>`  
   `SyntheticDesktop__BackendBaseUrl=http://127.0.0.1:8080`  
   `dotnet run --project src/Mocks/SyntheticDesktop/MimironsGoldOMatic.Mocks.SyntheticDesktop.csproj -c Release`
7. **Health sweep:** `python3 scripts/tier_b_verification/check_workflow_integration.py`
8. **Seed payout path:** Enroll → roulette tick / **`verify-candidate`** → obtain **`Pending`** **`payoutId`** (same patterns as [`RouletteVerifyCandidateIntegrationTests`](../src/Tests/MimironsGoldOMatic.Backend.UnitTests/RouletteVerifyCandidateIntegrationTests.cs)).
9. **Run synthetic Desktop sequence:** `curl -sS -X POST http://127.0.0.1:9054/run-sequence -H "Content-Type: application/json" -d "{\"payoutId\":\"<GUID>\",\"characterName\":\"Etoehero\"}"`  
   Or: `python3 scripts/tier_b_verification/check_syntheticdesktop.py --payout-id <GUID>`
10. **Assert Helix capture:** `curl -sS http://127.0.0.1:9053/last-request` — **`message`** must match Russian §11 template for the winner name (see [`HelixChatService`](../src/MimironsGoldOMatic.Backend/Services/HelixChatService.cs)).
11. **Optional:** `GET http://127.0.0.1:9054/last-run` for step-level HTTP status audit.

If step **5** is blocked (Helix URL not merged), stop after step **4** and track **A1–A2** on [`docs/TIER_B_IMPLEMENTATION_TASKS.md`](TIER_B_IMPLEMENTATION_TASKS.md).

---

## Tier B Troubleshooting Guide

Symptoms, likely causes, and fixes for new Tier B components. Tier A issues remain in [Predictive issue analysis](#predictive-issue-analysis-tier-a-ci).

### MockHelixApi

| Issue | Root cause | Symptoms | Resolution |
|-------|------------|----------|------------|
| **Wrong base URL** | Backend still posts to **api.twitch.tv** | Mock never receives traffic; **`GET /last-request`** empty | Set **`Twitch__HelixApiBaseUrl`** to mock root (e.g. `http://127.0.0.1:9053`); verify **`Helix`** `HttpClient` **`BaseAddress`** in [`Program.cs`](../src/MimironsGoldOMatic.Backend/Program.cs). |
| **MockHelixApi returns 404** | Wrong mock root (missing port), typo in URL, or POST path not under **`/helix/chat/messages`** | Python **`check_mockhelixapi`** or Backend logs show **404 Not Found**; **`last-request`** never updates | Open **`GET /health`** on the same base URL you configured; compare with [`MockHelixApi/Program.cs`](../src/Mocks/MockHelixApi/Program.cs). Ensure Backend uses base **without** trailing slash and relative path **`/helix/chat/messages`**. |
| **Response format** | Mock returns body Helix client does not treat as success | **`HelixChatService`** logs warnings; payout still **`Sent`** (SPEC: no rollback) | Return **2xx** with empty body or documented Helix JSON; match [`HelixChatService`](../src/MimironsGoldOMatic.Backend/Services/HelixChatService.cs) `IsSuccessStatusCode` check. |
| **Auth headers** | Strict mock rejects missing **`Client-Id`** / **`Bearer`** | **401** from mock | Align dummy **`Twitch__BroadcasterAccessToken`** and **`Twitch__HelixClientId`** with mock expectations; or leave **`MockHelix__StrictAuth`** unset/false for local smoke tests. |

**Log patterns:** successful mock capture — **`POST /helix/chat/messages`** returns **204**; **`GET /last-request`** JSON **`captured: true`**. Failure — connection refused (mock not started); **404** (wrong URL path).

### SyntheticDesktop

| Issue | Root cause | Symptoms | Resolution |
|-------|------------|----------|------------|
| **Sequence timing** | **`confirm-acceptance`** before payout is **`Pending`** or wrong user | **400** / **404** from EBS | Seed Marten + run **`verify-candidate`** (or test seed helper) before SyntheticDesktop; align **`characterName`** with enrollment. |
| **SyntheticDesktop sequence fails (HTTP 4xx/5xx)** | Invalid state transition, wrong **`payoutId`**, or Backend rules reject acceptance | **`POST /run-sequence`** returns **502**; **`last-run`** shows **`ok: false`** and first failing **`steps`** entry | Read **`steps[].bodySnippet`** from **`GET /last-run`**; fix domain order (**`Pending`** → confirm → **`InProgress`** → **`Sent`**). Compare with [`EbsMediator`](../src/MimironsGoldOMatic.Backend/Application/EbsMediator.cs) transitions. |
| **Status mismatches** | Invalid state transition (e.g. **`Sent`** without **`InProgress`**) | Handler validation error | Follow same order as real Desktop: **acceptance** → **`InProgress`** → **`Sent`** per domain rules in [`EbsMediator`](../src/MimironsGoldOMatic.Backend/Application/EbsMediator.cs) / payout aggregate. |
| **API key** | **`X-MGM-ApiKey`** missing or wrong | **401** / **403** | Match **`Mgm__ApiKey`** in workflow and SyntheticDesktop config. |

**Log patterns:** harness logs show **`POST confirm-acceptance`** status; Backend logs may show **`PatchPayoutStatus`** validation. **`last-run.error`** summarizes exception message after **`EnsureSuccessStatusCode`**.

### Workflow integration

| Issue | Root cause | Symptoms | Resolution |
|-------|------------|----------|------------|
| **Service startup order** | SyntheticDesktop runs before Backend ready | Connection refused | Keep Tier A wait loops; start SyntheticDesktop only after **`GET /api/pool/me`** or explicit backend health (reuse **`curl`** root). |
| **Workflow integration timeout** | Health probes run before **`dotnet run`** finishes binding; port conflict; Postgres not ready | **`check_workflow_integration.py`** **`HTTPConnectionPool` timeout** or **Connection refused** | Increase wait loops in CI; run integration script only after each **`GET /health`** succeeds manually; use [port map](#workflow-integration-ports--order) to resolve conflicts (**`netstat` / `ss`**). |
| **Port conflicts** | **9053** (Helix mock) or **9054** (SyntheticDesktop) taken | Address in use | Change **`ASPNETCORE_URLS`** and **`Twitch__HelixApiBaseUrl`** together; document port map in workflow comments. |
| **Job duration / cost** | Tier B adds two processes + more HTTP | PR minutes increase | Consider **nightly** Tier B only; keep Tier A on every PR to **`main`** ([Optimization](#optimization-and-scalability-ci)). |

**Open discussion (team):** whether to publish **Docker** images for mocks to reduce cold **`dotnet run`** time; whether **CI Tier B** runs on every PR or **nightly** only (**Actions** minute budget); **real WoW + Desktop** remains **out of scope** for default GitHub-hosted Tier B ([Overview](#1-overview)).

**Log patterns:** integration script logs each **`GET …/health`** URL; failure line names the first component that did not return **200** or expected JSON.

---

## Optimization and scalability (CI)

### Speed

- **Cache NuGet** with **`actions/cache`** on **`~/.nuget/packages`** keyed by `packages.lock.json` or hash of `*.csproj` — cuts restore time on warm runs.
- **Parallelize** only when **resource isolation** is guaranteed: Tier A today is **one job** with **shared localhost** ports—splitting into parallel jobs would require **dynamic ports** or **Docker Compose** networking.
- **Pre-built mock images**: optional **Docker** images for mocks to skip `dotnet run` JIT on every run; trade-off is image build/publish maintenance (team decision).

### Cost savings

- Run **full Tier A + Tier B** on **`schedule`** (nightly/weekly) and keep **PR** runs to **`dotnet build` + unit/integration** only, or run **Tier A** only on PRs to **`main`** (current behavior is already PR→`main` only).
- **Path filters**: skip E2E when only `docs/` changes (if acceptable risk).
- **Self-hosted runners** for repeated long suites if minute quotas are a concern.

### Monitoring

- Enable **GitHub Actions failure notifications** (repo **Settings → Notifications** / org rules).
- Track **job duration** in Actions **Insights**; alert on **p95** regression after workflow changes.
- Optionally **upload artifacts** (Backend + mock logs) on **always()** for flaky startup diagnosis (see [E2E Automation Tasks **V2**](E2E_AUTOMATION_TASKS.md#4-validation-tasks)).

---

## Unit and Integration Testing Strategy

**Workflow:** [`.github/workflows/unit-integration-tests.yml`](../.github/workflows/unit-integration-tests.yml)

### Trigger conditions

- **`pull_request`** targeting **`main`** only — same event filter as [`.github/workflows/e2e-test.yml`](../.github/workflows/e2e-test.yml) (**Tier A E2E**).
- Because this is a **separate workflow file**, GitHub Actions starts it **in parallel** with **E2E** on each qualifying PR (independent workflow graphs, subject to org concurrency limits).

### Component breakdown

| Component | Job | Runner | What runs today |
|-----------|-----|--------|-----------------|
| **Backend** | `test-backend` | `ubuntu-latest` | `dotnet test` on **`src/MimironsGoldOMatic.slnx`** (Backend unit + integration under **`src/Tests/`**; xUnit + **Testcontainers** where applicable); TRX under **`TestResults/backend/`** |
| **Desktop** | `test-desktop` | `windows-latest` | `dotnet test` on **`src/Tests/MimironsGoldOMatic.Desktop.UnitTests/`** (WPF-linked unit tests); TRX under **`TestResults/desktop/`** |
| **WoW addon** | `test-wowaddon` | `ubuntu-latest` | Required files + **`.toc`** consistency + **`luac -p`** on **`MimironsGoldOMatic.lua`**; log artifact (**placeholder** until a Lua test runner exists) |
| **Twitch Extension** | `test-twitch-extension` | `ubuntu-latest` | **`npm ci`** + **`npm test`** (**`eslint`** + **`tsc`/`vite build`**); logs under **`TestResults/twitch-extension/`** |

### Parallel execution model

- **Across workflows:** **Unit/integration** vs **E2E** — parallel (same trigger, different `name:` workflows).
- **Within unit/integration:** **`test-backend`**, **`test-desktop`**, **`test-wowaddon`**, **`test-twitch-extension`** have **no** mutual **`needs:`**, so they are eligible to run **concurrently**.
- **`aggregate-results`** runs **`if: always()`** after all four, posts or updates a **PR comment** (summary table + link to the workflow run). It is designed **not** to fail the workflow by itself; failing **test-*** jobs still mark the overall workflow **failed**.

### Artifact retention

- Per-component artifacts (**`test-results-backend`**, **`test-results-desktop`**, **`test-results-wowaddon`**, **`test-results-twitch-extension`**) use **`retention-days: 7`** (TRX, validation logs, **`npm test`** output).

### Future expansion

- **WoW addon:** add a real Lua unit suite (e.g. **Busted** / extracted pure modules) and invoke it from **`test-wowaddon`** instead of syntax-only checks.
- **Twitch Extension:** add **Vitest** (or similar), emit **lcov**, and upload coverage next to logs.
- **Backend:** optional **matrix** or split jobs (**fast unit** vs **Testcontainers integration**) if wall-clock time grows.
- **PR comment:** fork PRs may not allow **`pull-requests: write`** on `GITHUB_TOKEN`; the comment step uses **`continue-on-error: true`** so the summary is best-effort.

---

## 8. Next steps (implementation checklist)

1. **Refactor `HelixChatService`** — tracked as **Tier B Task 5**; see [Tier B Implementation Plan](#tier-b-implementation-plan-ci-extension).
2. **Add `MockHelixApi` + assertions** — **Tier B Tasks 1–3, 6**.
3. **Optional:** Add in-repo **`EventSubSignatureHelper`** in **`MimironsGoldOMatic.Backend.UnitTests`** (or reuse **Python** script logic) for xUnit coverage of **`POST /api/twitch/eventsub`** with **non-empty** `EventSubSecret` — **Tier A CI** already exercises the full path via mocks + script.
4. **SyntheticDesktop + chained workflow** — **Tier B Task 4**; enroll → spin tick → verify-candidate → confirm-acceptance → patch statuses → assert Helix mock + pool.
5. ~~**Create `.github/workflows/e2e-test.yml`**~~ **Done (Tier A).** Optionally add **`ci.yml`** for PR **build/test** only; add **`Category=E2E`** **dotnet test** when in-process E2E tests exist.
6. **Document** test filters in **`docs/MimironsGoldOMatic.Backend/ReadME.md`** (Tier A + local E2E).
7. **Operational / full-stack (later):** self-hosted Windows job spec + operator runbook in **`docs/SETUP.md`** (real WoW + Desktop; outside **CI Tier B**).

---

## Document control

| Version | Date | Note |
|---------|------|------|
| 1.0 | 2026-04-05 | Initial plan from **SC-001** + current Backend layout |
| 1.1 | 2026-04-05 | **Tier A:** `MockEventSubWebhook`, `MockExtensionJwt`, `e2e-test.yml`, `send_e2e_eventsub.py` |
| 1.2 | 2026-04-05 | **Tier A** runbook, predictive issues, **CI Tier B** plan, optimization notes; terminology aligned with workflow |
| 1.3 | 2026-04-05 | **CI/CD Pipeline Architecture:** `e2e-test.yml` scoped PR build; **`release.yml`** parallel builds + sequential **`create-release`**; GHCR Backend image |
| 1.4 | 2026-04-05 | **Unit and Integration Testing Strategy:** `unit-integration-tests.yml` (PR→`main`, parallel with E2E); per-component jobs + artifacts + PR summary |
| 1.5 | 2026-04-05 | **Tier A Test Results & Verification** (GitHub API metrics); expanded **Tier B** plan (A–D); **Tier B Troubleshooting**; link to [`TIER_B_IMPLEMENTATION_TASKS.md`](TIER_B_IMPLEMENTATION_TASKS.md) |
| 1.6 | 2026-04-05 | **Tier B Readiness Verification**, **First Run Guide**, mock projects **MockHelixApi** / **SyntheticDesktop**, [`scripts/tier_b_verification/`](../scripts/tier_b_verification/), expanded troubleshooting; [`TIER_B_PRELAUNCH_CHECKLIST.md`](TIER_B_PRELAUNCH_CHECKLIST.md) |

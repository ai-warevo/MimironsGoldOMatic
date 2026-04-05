<!-- Created: 2026-04-05 (E2E automation plan) -->
<!-- Updated: 2026-04-05 (Unit/integration PR workflow + strategy section) -->

# E2E automation plan (MVP-6): Chat → WoW → Helix

This document proposes how to automate the **full operator workflow** currently described manually in [`docs/INTERACTION_SCENARIOS.md`](INTERACTION_SCENARIOS.md) (**SC-001**, **SC-005**, and [Automated E2E Scenarios (MVP-6)](INTERACTION_SCENARIOS.md#automated-e2e-scenarios-mvp-6)). It is **planning only**; it does not change product behavior in **`docs/SPEC.md`**.

**Related:** [`docs/ROADMAP.md`](ROADMAP.md) MVP-6, [`docs/IMPLEMENTATION_READINESS.md`](IMPLEMENTATION_READINESS.md) (MVP-6 verification status), [`docs/MimironsGoldOMatic.Backend/ReadME.md`](MimironsGoldOMatic.Backend/ReadME.md) (automated tests). **Implementation checklist / ownership:** [E2E Automation Tasks](E2E_AUTOMATION_TASKS.md).

**Code roots (actual repository layout):**

- Backend (EBS): `src/MimironsGoldOMatic.Backend/` — **not** `MimironsGoldOMatic.WEBAPI.Backend`.
- WoW addon: `src/MimironsGoldOMatic.WoWAddon/`.
- Desktop: `src/MimironsGoldOMatic.Desktop/`.
- CI: `.github/workflows/e2e-test.yml` — **Tier A** (Backend + Postgres + mocks + synthetic EventSub) on **PRs to `main`** (scoped build; no Desktop / addon / Extension). **Unit/integration (parallel):** `.github/workflows/unit-integration-tests.yml` (Backend, Desktop, WoW addon validation, Twitch Extension `npm test`) on the **same** PR trigger. CD: `.github/workflows/release.yml` — **full multi-component build + GitHub Release + GHCR** on **`main`** merges (and optional manual dispatch).
- Tier A mocks: `src/Mocks/MockEventSubWebhook/`, `src/Mocks/MockExtensionJwt/`.

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
| **Build scope** | **Shared + Backend + MockEventSubWebhook + MockExtensionJwt** only (excludes **Desktop**, **Backend.Tests**, **WoW addon**, **Twitch Extension**) for shorter wall time and fewer prerequisites. |
| **Data plane** | PostgreSQL **16** service container (unchanged health / port / credentials pattern). |
| **Runtime** | Backend + both mocks on loopback; **Python** [`.github/scripts/send_e2e_eventsub.py`](../.github/scripts/send_e2e_eventsub.py) + **`curl`** assertions unchanged from Tier A. |
| **Proof** | Synthetic **`channel.chat.message`** → **`GET /api/pool/me`** enrollment check (`E2EHero`). |
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
| **Mock EventSub diverges** from Twitch payload schema | Versioned JSON **golden files** under `Backend.Tests/Fixtures/`; update when Twitch changelog affects `channel.chat.message`. |
| **Flaky time** in roulette / verify window | Use patterns from [`RouletteVerifyCandidateIntegrationTests`](../src/MimironsGoldOMatic.Backend.Tests/RouletteVerifyCandidateIntegrationTests.cs) (bounded `capturedAt`, known cycle anchors). |
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
- **Steps (summary):** Start **PostgreSQL 16** service → **scoped** `dotnet build` (**Shared + Backend + both mocks** only) → run **Backend** (`Development`, shared `Twitch:EventSubSecret`) → run both mocks → **Python** [`.github/scripts/send_e2e_eventsub.py`](../.github/scripts/send_e2e_eventsub.py) posts a synthetic **`channel.chat.message`** to the mock → assert **`GET /api/pool/me`** with JWT shows **`isEnrolled: true`** and expected **`characterName`** (`!twgold E2EHero`).

### E2E script

- [`.github/scripts/send_e2e_eventsub.py`](../.github/scripts/send_e2e_eventsub.py) — builds JSON + Twitch HMAC; can target the mock or the EBS directly for debugging.

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
5. **`GET /api/pool/me`** with the issued Bearer token returns JSON with **`isEnrolled: true`** and **`characterName`** **`E2EHero`** (matching `!twgold E2EHero` in the script).

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

### Product / code changes

1. **`TwitchOptions` + `HelixChatService`:** Add optional **`HelixApiBaseUrl`** (or **`HelixApiBase`**) — when empty, behavior matches today (**`https://api.twitch.tv/helix/chat/messages`** absolute URL). When set, **`Helix`** `HttpClient` uses that base and the service posts to a **relative** **`/helix/chat/messages`** (or documented path). Register the named client in [`Program.cs`](../src/MimironsGoldOMatic.Backend/Program.cs).
2. **MockHelixApi:** New minimal ASP.NET Core (or console + Kestrel) project under `src/Mocks/`, e.g. **`MockHelixApi`**, exposing **`POST /helix/chat/messages`** (and **`GET /health`**), recording the last request body/headers for assertions (or logging JSON for CI `curl` + Python assert).
3. **SyntheticDesktop:** Either a **`dotnet`** console tool (**`Mgm.Desktop.E2EHarness`**) or **bash/curl** steps in the workflow calling **`X-MGM-ApiKey`** endpoints: **`POST /api/payouts/{id}/confirm-acceptance`**, **`PATCH .../status`** (**`InProgress`**, then **`Sent`**) per [`DesktopPayoutsController`](../src/MimironsGoldOMatic.Backend/Controllers/DesktopPayoutsController.cs) and [tasks D1–D3](E2E_AUTOMATION_TASKS.md#d-syntheticdesktop).

### Task breakdown (owners and estimates)

| Task | Owner | Est. | Description |
|------|--------|------|-------------|
| **1** | Backend Dev | **0.5 d** | Create **`MockHelixApi`** project in **`src/Mocks/`**, wire into **`MimironsGoldOMatic.slnx`**, **`GET /health`**. |
| **2** | Backend Dev | **0.5–1 d** | Implement **`POST /helix/chat/messages`** stub: validate **`Authorization`**, **`Client-Id`**, return **2xx**; expose last payload via **`GET /last-request`** or structured log for CI. |
| **3** | DevOps / Backend Dev | **0.5–1 d** | Integrate into **`.github/workflows/e2e-test.yml`**: start mock on a free port (e.g. **9053**), set **`Twitch__BroadcasterAccessToken`**, **`Twitch__BroadcasterUserId`**, **`Twitch__HelixClientId`** on Backend so **`HelixChatService`** actually calls Helix; assert Russian §11 message template. |
| **4** | Backend Dev | **1–2 d** | **SyntheticDesktop**: ordered **`HttpClient`** calls (or shell) with **`Mgm__ApiKey`**; seed or reuse **pool + spin + `Pending`** (see [`RouletteVerifyCandidateIntegrationTests`](../src/MimironsGoldOMatic.Backend.Tests/RouletteVerifyCandidateIntegrationTests.cs)). |
| **5** | Backend Dev | **1 d** | **`HelixChatService` + `TwitchOptions`**: configurable base URL; regression test that default URL unchanged when option unset. |
| **6** | DevOps | **0.5 d** | Extend workflow: start **MockHelixApi**, run synthetic chain, fail job if Helix mock did not receive exactly one announcement (or match xUnit artifact strategy). |

**Suggested order:** **Task 5** → **Tasks 1–2** → **Task 4** (against local Helix mock) → **Tasks 3 + 6** (CI wiring).

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
| **Backend** | `test-backend` | `ubuntu-latest` | `dotnet test` on **`src/MimironsGoldOMatic.Backend.Tests/`** (xUnit + **Testcontainers** integration tests); TRX under **`TestResults/backend/`** |
| **Desktop** | `test-desktop` | `windows-latest` | `dotnet test` on **`src/MimironsGoldOMatic.Desktop.Tests/`** (WPF smoke / future unit tests); TRX under **`TestResults/desktop/`** |
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
3. **Optional:** Add in-repo **`EventSubSignatureHelper`** in **`Backend.Tests`** (or reuse **Python** script logic) for xUnit coverage of **`POST /api/twitch/eventsub`** with **non-empty** `EventSubSecret` — **Tier A CI** already exercises the full path via mocks + script.
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

<!-- Updated: 2026-04-05 (MVP-6 status sync; E2E cross-refs) -->

# Project Roadmap: Mimiron's Gold-o-Matic

This roadmap reflects the **finalized MVP specification** agreed during design clarification.

**Current stage:** The project is in the **late MVP phase (MVP-6)**. **Core components** — Backend (EBS), WoW addon, WPF Desktop, and Twitch Extension (viewer panel) — are **implemented**. **Current focus:** verification, hardening, and **E2E automation** before the **Beta** milestone (see **Beta** section below).

Canonical implementation contracts live in:

- `docs/SPEC.md`

**Interaction scenarios & test cases (for implementation / verification):** When executing MVP steps below, agents should use [`docs/INTERACTION_SCENARIOS.md`](INTERACTION_SCENARIOS.md) for scenario IDs (SC-001, …), paired test cases (TC-001, …), and the **Component Contracts** section at each boundary.

**UI/UX (screens, element inventory, flows):** Hub [`docs/UI_SPEC.md`](UI_SPEC.md) (tokens, navigation); implement against [`docs/MimironsGoldOMatic.TwitchExtension/UI_SPEC.md`](MimironsGoldOMatic.TwitchExtension/UI_SPEC.md), [`docs/MimironsGoldOMatic.Desktop/UI_SPEC.md`](MimironsGoldOMatic.Desktop/UI_SPEC.md), and [`docs/MimironsGoldOMatic.WoWAddon/UI_SPEC.md`](MimironsGoldOMatic.WoWAddon/UI_SPEC.md) for **UI-1xx–UI-4xx** (Twitch ~318px panel, WPF windows, WoW frames) while building MVP-3 / MVP-4 / MVP-5.

**Implementation snapshot (repository):** The steps below are the **target** MVP sequence. For what is **actually checked in** today versus **MVP-6** (tests, E2E harness) and residual risks, see [`docs/IMPLEMENTATION_READINESS.md`](IMPLEMENTATION_READINESS.md) — *Source code parity (MVP track)*. For **Manual** vs **Automated** E2E mapping, see **[Automated E2E Scenarios (MVP-6)](INTERACTION_SCENARIOS.md#automated-e2e-scenarios-mvp-6)** in `docs/INTERACTION_SCENARIOS.md`.

### Mandatory checklist (every roadmap step)

Each step is executed with a checklist supplied by the project owner (often inline in the prompt). Agents **must**:

- Read the cited **`docs/SPEC.md`** sections (§…).
- Read the cited **`docs/INTERACTION_SCENARIOS.md`** test cases (TC-…) when listed.
- **Not** add HTTP endpoints, DTOs, or product behaviors absent from **`docs/SPEC.md`**.
- **Not** implement **`docs/INTERACTION_SCENARIOS.md`** scenarios marked **future / not MVP** or **placeholder** unless **`docs/SPEC.md`** explicitly adds the behavior (no speculative APIs — e.g. retry tokens, pause endpoints).

**Payout model note:** see **[MVP_PRODUCT_SUMMARY.md](MVP_PRODUCT_SUMMARY.md)** (digest) and **`docs/SPEC.md`** (normative).

<!-- Former inline payout paragraph moved to MVP_PRODUCT_SUMMARY.md. See: docs/MVP_PRODUCT_SUMMARY.md -->

## MVP (End-to-end happy path)

### MVP-0: Repo skeleton

- Solution file: `src/MimironsGoldOMatic.slnx` (includes Shared, Backend, Desktop; extend for remaining projects in MVP-0)
- Add projects under `src/`:
  - `MimironsGoldOMatic.Shared`
  - `MimironsGoldOMatic.Backend`
  - `MimironsGoldOMatic.Desktop`
  - `MimironsGoldOMatic.TwitchExtension`
  - `MimironsGoldOMatic.WoWAddon`

**Status — implemented:** `MimironsGoldOMatic.slnx` lists the three .NET projects. `MimironsGoldOMatic.TwitchExtension` (Vite/React) and `MimironsGoldOMatic.WoWAddon` (30300 addon) exist under `src/` with their own tooling and are **not** MSBuild projects in the solution file (see `docs/IMPLEMENTATION_READINESS.md`).

Spec links:

- `docs/SPEC.md#4-identity-idempotency-and-dtos`

### MVP-1: Shared contracts (`MimironsGoldOMatic.Shared`)

- Define `PayoutStatus`: `Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired`
- Define `CreatePayoutRequest`: `CharacterName`, `EnrollmentRequestId`
- Define `PayoutDto` fields (MVP):
  - `Id`, `TwitchUserId`, `TwitchDisplayName`, `CharacterName`, `GoldAmount` (fixed 1,000g), `EnrollmentRequestId`, `Status`, `CreatedAt`, `IsRewardSentAnnouncedToChat` (Helix §11 read-model flag; `docs/SPEC.md` §6)
- Add shared validation for `CharacterName`

**Status — implemented:** `src/MimironsGoldOMatic.Shared` (.NET 10), namespace `MimironsGoldOMatic.Shared`: `PayoutStatus`, `PayoutDto` (incl. `IsRewardSentAnnouncedToChat`), `CreatePayoutRequest`, `PayoutEconomics.MvpWinningPayoutGold` (1,000g per SPEC §2), `CharacterNameRules` + FluentValidation (`PayoutDtoValidator`, `CreatePayoutRequestValidator`). Details: `docs/MimironsGoldOMatic.Shared/ReadME.md`.

Spec links:

- `docs/SPEC.md#3-statuses--lifecycle-transitions`
- `docs/SPEC.md#4-identity-idempotency-and-dtos`

#### Step-by-step prompt (MVP-1)

Acting as **[EBS/API Expert]**:

- Read `docs/SPEC.md` and `docs/MimironsGoldOMatic.Shared/ReadME.md`.
- Initialize the .NET 10 Class Library project `MimironsGoldOMatic.Shared` inside `/src`.
- Implement the shared contracts **as documented**:
  - `PayoutStatus` enum including: `Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired`
  - `PayoutDto` record including: `Id`, `TwitchUserId`, `TwitchDisplayName`, `CharacterName`, `GoldAmount`, `EnrollmentRequestId`, `Status`, `CreatedAt`, `IsRewardSentAnnouncedToChat`
  - `CreatePayoutRequest` record including: `CharacterName`, `EnrollmentRequestId`
- Ensure the namespace is `MimironsGoldOMatic.Shared`.

### MVP-2: EBS API + persistence (`MimironsGoldOMatic.Backend`)

- PostgreSQL + Marten (Event Store) + CQRS read projections
- Persistence rules:
  - Write-side source of truth: Event Store (Marten)
  - Read-side query models/projections (EF Core optional for query mapping)
  - Defensive uniqueness on read model for `EnrollmentRequestId` (idempotency)
- Business rules (MVP):
  - Fixed 1,000g per **winning** payout; **subscribe + chat** **`!twgold <CharacterName>`** **joins the pool** first
  - **Roulette** on a **5-minute** cadence (min **1** participant); **no** early spin
  - 10,000g lifetime cap per Twitch user
  - One active payout per Twitch user
  - Rate limiting (e.g. ~5 req/min per IP/user)
- Endpoints (MVP):
  - `POST /api/payouts/claim` (`201` new, `200` idempotent replay) — **pool enrollment** (see `docs/SPEC.md`)
  - `GET /api/payouts/pending`
  - `PATCH /api/payouts/{id}/status`
  - `POST /api/payouts/{id}/confirm-acceptance` (or equivalent) — Desktop after **`!twgold`** (willing to accept)
  - **`Sent`** via **`[MGM_CONFIRM:UUID]`** in **`WoWChatLog.txt`** → Desktop → **`PATCH` status** (or equivalent)
  - `GET /api/payouts/my-last` (`404` when none exists)
  - `GET /api/roulette/state` — server-authoritative **`nextSpinAt`** / **`serverNow`** + pool count + spin phase (`docs/SPEC.md` §5.1)
  - `GET /api/pool/me` — viewer enrollment hint for Extension (`docs/SPEC.md` §5.1)
  - `POST /api/roulette/verify-candidate` — Desktop submits **`/who`** result (parsed from **`[MGM_WHO]`** in **`WoWChatLog.txt`**); **EBS** creates **`Pending`** or **no winner** (`docs/SPEC.md` §5, §8)
- Background job:
  - Hourly: mark `Pending`/`InProgress` older than 24h as `Expired` (terminal, no reactivation)
- Auth/security (MVP):
  - Extension **Bearer** JWT: HS256 validation using **`Twitch:ExtensionSecret`** (base64); **`Twitch:ExtensionClientId`** as JWT **`aud`** when set. **Development** may use an empty secret with a fixed dev-derived key (`Program.cs`).
  - Desktop uses a pre-shared **`Mgm:ApiKey`** (header **`X-MGM-ApiKey`**).

**Status — implemented (code):** `src/MimironsGoldOMatic.Backend` — Marten + PostgreSQL (`ConnectionStrings:PostgreSQL`), MVP HTTP routes (Extension JWT + Desktop `X-MGM-ApiKey`), EventSub `channel.chat.message` at `POST /api/twitch/eventsub`, MediatR handlers, roulette sync + payout expiration hosted services, Helix §11 inline retry after `Sent`, global rate limiter (EventSub exempt). Configure `Mgm`, `Twitch`, and Postgres before running; see `docs/MimironsGoldOMatic.Backend/ReadME.md` and `appsettings*.json`. Runtime E2E against Twitch/Helix not verified in CI.

Spec links:

- `docs/SPEC.md#2-mvp-economics--anti-abuse-rules`
- `docs/SPEC.md#3-statuses--lifecycle-transitions`
- `docs/SPEC.md#5-api-contract-mvp` (includes §5.1 pool/roulette GETs)
- `docs/SPEC.md#6-persistence-model-mvp-es-first`
- `docs/SPEC.md#7-expiration-job-mvp`

#### Step-by-step prompt (MVP-2)

Acting as **[EBS/API Expert]**:

- Read `docs/SPEC.md`, `docs/MimironsGoldOMatic.Backend/ReadME.md`, and reference `MimironsGoldOMatic.Shared`.
- Create the ASP.NET Core (.NET 10) Web API project `MimironsGoldOMatic.Backend` in `/src`.
- Configure Marten Event Store with PostgreSQL and implement CQRS persistence:
  - Write-side events are the canonical source of truth
  - Read models include `TwitchUserId` and `TwitchDisplayName`
  - Keep `EnrollmentRequestId` idempotency guarantees in write/read flow
- Implement endpoints:
  - `POST /api/payouts/claim` (pool **enrollment**; enforce caps + idempotency; rate limit; return `201` for new and `200` for idempotent replay)
  - **Pool / roulette** services + **EventSub** chat ingestion for **`!twgold <CharacterName>`** (**`!twgold`** prefix **case-insensitive**; non-subscribers **log only**); **`GET /api/roulette/state`** + **`GET /api/pool/me`** per `docs/SPEC.md` §5.1 (**JWT-only** Extension auth; **real Twitch JWTs**); **`POST /api/roulette/verify-candidate`** (**`[MGM_WHO]`** log line from **`docs/SPEC.md` §8**); **winner whisper** + **`confirm-acceptance`** per `docs/SPEC.md` §9; **UTC** spin boundaries **:00/:05/…**; **no re-draw** same cycle; **min 1** participant; **non-winners stay**; **remove winner on `Sent`**; **`CharacterName`** validation **§4**; **winner notification** payload for Extension; **single broadcaster** MVP (`docs/SPEC.md` deployment scope)
  - `GET /api/payouts/pending` (**winner** payouts)
  - `PATCH /api/payouts/{id}/status`
  - `POST /api/payouts/{id}/confirm-acceptance` after **`!twgold`**; **`Sent`** when log shows **`[MGM_CONFIRM:UUID]`**
  - `GET /api/payouts/my-last` (return `404` when none exists)
  - **§11 Helix:** On transition to **`Sent`**, **`Send Chat Message`** per **`docs/SPEC.md`** (inline **3** retries, **no** Outbox, **no** rollback on Helix failure, **once** per payout id)
- Implement expiration job:
  - Hourly background process transitions `Pending/InProgress` older than 24h to `Expired`
- Implement MVP auth posture:
  - Extension JWT validation (**HS256** + optional **`aud`**); Dev Rig for real tokens
  - Desktop requests must include pre-shared **`X-MGM-ApiKey`**

### MVP-3: WoW Addon (`MimironsGoldOMatic.WoWAddon`)

- Implement `ReceiveGold(dataString)` to enqueue **winner** payouts
- Hook `MAIL_SHOW` and provide a side panel UI
- Implement “Prepare Mail” auto-fill:
  - `SendMailNameEditBox`, subject, gold-to-copper via `MoneyInputFrame_SetCopper`
- Support **roulette `/who`** flow if implemented in-client: execute or surface **`/who <Winner_InGame_Nickname>`** results for Desktop/EBS (normative behavior in `docs/SPEC.md`)
- Intercept **whispers** to the streamer where the message is exactly **`!twgold`** and notify the Desktop utility (**EBS** **acceptance** via Desktop); winner **must** have been **notified** first per product flow
- **Required:** after mail is sent, print **`[MGM_CONFIRM:UUID]`** to chat (UUID is payout id) for **`WoWChatLog.txt`** / **`Sent`**

**Status — implemented (code):** `src/MimironsGoldOMatic.WoWAddon` — **`NotifyWinnerWhisper`**, **`ReceiveGold`**, **`MGM_RunWhoForSpin(spinCycleId, characterName)`** (Desktop `/run` with **`currentSpinCycleId`** from EBS), mail queue panel on **`MAIL_SHOW`**, **Prepare Mail**, **`[MGM_WHO]`** / **`[MGM_ACCEPT]`** / **`[MGM_CONFIRM]`** per `docs/SPEC.md` §8–10. **`/mgm`** + minimap button. Runtime verification on a live 3.3.5a client is manual.

Spec links:

- `docs/SPEC.md` (§9 Addon; §10 Chat log / Desktop bridge)

#### Step-by-step prompt (MVP-3)

Acting as **[WoW Addon/Lua Expert]**:

- Read `docs/SPEC.md` and `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`.
- Create the `src/MimironsGoldOMatic.WoWAddon` folder.
- Implement the 3.3.5a addon logic (Interface: 30300):
  - `MimironsGoldOMatic.lua` with global **`NotifyWinnerWhisper(payoutId, characterName)`** (Desktop **`/run`**) to send §9 **`/whisper`** (`docs/SPEC.md` §8–9)
  - Global `ReceiveGold(dataString)` to parse and enqueue payouts
  - UI side panel that hooks into `MAIL_SHOW`
  - Auto-fill logic for `SendMailNameEditBox` and `MoneyInputFrame_SetCopper`
- Implement **`!twgold`** whisper detection; print **`[MGM_ACCEPT:UUID]`** to chat for **`WoWChatLog.txt`** / Desktop (no HTTP from Lua)
- **Roulette `/who`:** run **`/who`**, parse **3.3.5a**, emit **`[MGM_WHO]`** + JSON per **`docs/SPEC.md` §8** ( **`WoWChatLog.txt`** ) for Desktop → **`POST /api/roulette/verify-candidate`**
- Emit **`[MGM_CONFIRM:UUID]`** after mail send (**required** for **`Sent`** via chat log)

### MVP-4: Desktop WPF utility (`MimironsGoldOMatic.Desktop`)

- MVVM (CommunityToolkit.Mvvm)
- Queue workflow (explicit claim) for **winner** payouts:
  - `GET /api/payouts/pending`
  - **`NotifyWinnerWhisper`** inject for each **`Pending`** winner (`docs/SPEC.md` §8–9), **then** **Sync/Inject** mail: `PATCH /api/payouts/{id}/status` -> `InProgress`
- **Roulette coordination:** tail **`[MGM_WHO]`** and **`POST /api/roulette/verify-candidate`** (see `docs/SPEC.md`)
- WinAPI injection:
  - Target **foreground** `WoW.exe` (MVP)
  - Inject **`/run NotifyWinnerWhisper(...)`** and **`/run ReceiveGold("...")`** with <255 char chunking
  - Use `PostMessage` as primary strategy with `SendInput` fallback
- Feedback loop:
  - Receive **`!twgold`** from addon → call **EBS** **acceptance** endpoint (not **`Sent`**)
  - **Required:** tail `Logs\WoWChatLog.txt` for **`[MGM_CONFIRM:UUID]`** → mark **`Sent`**
  - Manual overrides in UI:
    - **Mark as Sent**
    - **Fail**
    - **Cancel**
    - **InProgress → Pending** escape hatch (`docs/SPEC.md` §3)

**Status — implemented (code):** `src/MimironsGoldOMatic.Desktop` — MVVM main window + settings (API URL, DPAPI ApiKey, WoW log path, poll interval, PostMessage vs SendInput preference), `GET /api/payouts/pending` poll, auto **`NotifyWinnerWhisper`** for new **`Pending`** rows when WoW is foreground (persisted ids), **Sync/Inject** → **`PATCH` `InProgress`** + chunked **`/run ReceiveGold`**, single **`WoWChatLog.txt`** tail → **`verify-candidate`** / **`confirm-acceptance`** / **`PATCH` `Sent`**, Polly retries on **`HttpClient`**. Runtime WinAPI behavior on real **3.3.5a** clients is manual.

Spec links:

- `docs/SPEC.md#5-api-contract-mvp`
- `docs/SPEC.md#8-desktop--wow-injection-specification-mvp`
- `docs/SPEC.md` (§9–10)

#### Step-by-step prompt (MVP-4)

Acting as **[WPF/WinAPI Expert]**:

- Read `docs/SPEC.md`, `docs/MimironsGoldOMatic.Desktop/ReadME.md`, and reference `MimironsGoldOMatic.Shared`.
- Create the WPF Application (.NET 10) `MimironsGoldOMatic.Desktop` in `/src`.
- Use CommunityToolkit.Mvvm for MVVM structure.
- Implement explicit-claim queue flow:
  - `GET /api/payouts/pending`
  - For each new **`Pending`** winner: inject **`/run NotifyWinnerWhisper("<id>","<CharacterName>")`** per **`docs/SPEC.md` §8–9** (addon sends §9 whisper **before** mail **`ReceiveGold`** flow)
  - On **Sync/Inject** (mail prep): `PATCH /api/payouts/{id}/status` -> `InProgress`
- Implement Win32 `PostMessage` injection for **`NotifyWinnerWhisper`** and **`ReceiveGold`** in WoW:
  - Target the **foreground** `WoW.exe` process (MVP)
  - Implement <255 char chunking for injected `/run` commands
  - Add `SendInput` fallback strategy for blocked/unreliable primary injection
- Implement confirmation loop:
  - **Single** log tail **`Logs\WoWChatLog.txt`**: **`[MGM_WHO]`** → **`POST /api/roulette/verify-candidate`**; **`[MGM_ACCEPT:UUID]`** → **EBS** **acceptance**; **`[MGM_CONFIRM:UUID]`** → **EBS** **`Sent`**; configurable log path (**§10**)
  - Allow **`PATCH` `InProgress` → `Pending`** per **`docs/SPEC.md` §3**
  - Provide manual overrides: **Mark as Sent**, **Fail**, **Cancel**
- Use the pre-shared Desktop `ApiKey` when calling **EBS** endpoints.

### MVP-5: Twitch Extension (`MimironsGoldOMatic.TwitchExtension`)

- Dev Rig-focused integration
- **Enrollment** is **not** form-primary: viewers use **`!twgold <CharacterName>`** in **stream chat** (see `docs/SPEC.md`). Extension shows **instructions + status** (poll the **EBS**).
  - Optional Dev Rig: **`POST /api/payouts/claim`** with `EnrollmentRequestId` for testing
- **Visual roulette** UI:
  - Spin every **5 minutes**; **minimum 1** participant
  - Copy: **subscribe** + **`!twgold <CharacterName>`** in chat; **unique** name; **removed from pool** after **`Sent`**; re-enter via chat
  - **“You won”** + **WoW whisper reply `!twgold`** (after notification whisper, `docs/SPEC.md` §9)
  - Optional UI for **`/who`** / verification state if API exposes it
- Status UX (pull model):
  - `GET /api/payouts/my-last` and/or pool/spin endpoints as implemented

**Status — implemented (MVP):** Viewer panel lives in **`src/MimironsGoldOMatic.TwitchExtension`**. UI inventory and copy: [`docs/MimironsGoldOMatic.TwitchExtension/UI_SPEC.md`](MimironsGoldOMatic.TwitchExtension/UI_SPEC.md); hub tokens and cross-client rules: [`docs/UI_SPEC.md`](UI_SPEC.md).

Spec links:

- `docs/SPEC.md#5-api-contract-mvp`
- `docs/SPEC.md` (§11 Twitch Extension)

#### Step-by-step prompt (MVP-5)

Acting as **[Frontend/Twitch Expert]**:

- Read `docs/SPEC.md` and `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`.
- Scaffold `src/MimironsGoldOMatic.TwitchExtension` using Vite + React + TypeScript.
- Build **instructional** UI (chat commands) and status polling — **not** the sole enrollment path.
- Integrate with Twitch Extension helper (`window.Twitch.ext`); optional **`POST /api/payouts/claim`** for Dev Rig with `EnrollmentRequestId`
- Implement **visual roulette** + **5-minute** countdown (aligned with the **EBS**; **no** early spin).
- Implement pull status UX:
  - Call `GET /api/payouts/my-last` (and any pool APIs)
- **MVP-5 scope (locked):** **viewer panel only** ([`docs/MimironsGoldOMatic.TwitchExtension/UI_SPEC.md`](MimironsGoldOMatic.TwitchExtension/UI_SPEC.md) **UI-101–106**). **Do not** implement broadcaster dashboard panels **UI-201–204** in MVP-5 (post-MVP / when the **EBS** adds broadcaster JWT routes).
- Ensure alignment with Twitch Dev Rig for MVP debugging (**real** Extension JWTs per `docs/SPEC.md`).

### MVP-6: End-to-end demo & verification

- Demo scenario: **`!twgold <CharacterName>`** in **chat** → **roulette spin** (random candidate) → **`/who`** online OK → Desktop **`NotifyWinnerWhisper`** → **winner notification whisper** (§9) → winner **`!twgold`** in **WoW** → **confirm-acceptance** → desktop **`ReceiveGold`** inject → streamer sends mail → **`[MGM_CONFIRM:UUID]`** in log → **EBS** **`Sent`** → **winner removed from pool**
- Add minimal **EBS** / API tests for:
  - idempotency (`EnrollmentRequestId`)
  - one-active-per-user
  - lifetime cap (10k)
  - expiration behavior
  - roulette / pool rules (at least one spin with **1** participant)

**Status — MVP-6 (verification split):**

- **Automated (in place):** `src/MimironsGoldOMatic.Backend.Tests` — xUnit, **PostgreSQL via Testcontainers** (Docker required for **Integration** category), plus **Unit** tests (no Docker) for time/spin-phase and **`!twgold`** line parsing. Integration coverage includes MediatR/HTTP paths aligned with the bullets above (claim rules, **`verify-candidate`**, expiration sweep, **`PATCH` → `Sent`** pool removal). See `docs/MimironsGoldOMatic.Backend/ReadME.md` §Automated tests.
- **Manual (required today):** The **full E2E** path **Twitch chat → EventSub → … → WoW client → `WoWChatLog.txt` → Desktop → Helix §11 announcement** is **not** automated in **CI/CD**. Operators validate it using **`docs/INTERACTION_SCENARIOS.md`** (e.g. SC-001, SC-005) and live/Dev Rig setup. Step-by-step mapping of manual vs target automation: **[Automated E2E Scenarios (MVP-6)](INTERACTION_SCENARIOS.md#automated-e2e-scenarios-mvp-6)**.

**Next steps (MVP-6):**

- **Automate full E2E demo in CI/CD** — extend automation beyond Backend integration tests (e.g. workflow jobs, harnesses, or mocks) only when an approach is chosen; `.github/workflows/` remains a placeholder today.
- **Validate complete operator workflow** — run the full manual scenario end-to-end and record results against **TC-** rows in **`docs/INTERACTION_SCENARIOS.md`**.

For details on the automation approach, see [E2E Automation Plan](E2E_AUTOMATION_PLAN.md). Actionable work items: [E2E Automation Tasks](E2E_AUTOMATION_TASKS.md).

### E2E Automation Progress

- **Plan:** [E2E Automation Plan](E2E_AUTOMATION_PLAN.md) (Tier A **CI** vs Tier B self-hosted; mocks and **SyntheticDesktop**).
- **Task list:** [E2E Automation Tasks](E2E_AUTOMATION_TASKS.md) (ownership, estimates).
- **Status:** documentation linked; implementation pending (see task file).

**Solution layout:** `MimironsGoldOMatic.slnx` includes **Backend.Tests**; **Twitch Extension** and **WoW addon** stay non-MSBuild trees (same as MVP-0).

Spec links:

- `docs/SPEC.md`

#### Step-by-step prompt (MVP-6)

Acting as **[Senior Architect]**:

- Review `docs/SPEC.md` and `docs/ROADMAP.md` for end-to-end consistency.
- Ensure all projects are included in `src/MimironsGoldOMatic.slnx`.
- Synchronize component behavior and verify the full data flow:
  - **Chat** enrollment + Extension status/roulette UX
  - **`/who`** validates winner online -> notify winner -> **EBS** **winner payout** `Pending`
  - Desktop explicit claim + inject -> Addon queue
  - **WoW whisper `!twgold`** → acceptance; **`[MGM_CONFIRM:UUID]`** → `Sent` → pool removal
- Finalize setup notes in root docs as needed.

## Beta (Reliability & streamer UX)

- WoW process picker (not just foreground process)
- Better “stuck InProgress” reconciliation and UX guidance
- Batch actions and filtering in Desktop
- ApiKey rotation / reset story (lightweight)

## Production milestone (Security hardening)

- Harden Twitch JWT validation (**issuer** validation, secret rotation runbooks; current MVP uses **symmetric** Extension secret / HS256, not OIDC JWKS)
- Secrets/config hardening across environments
- Security review (abuse cases, logging hygiene)
- CI pipelines for .NET and frontend builds/tests (as of 2026-04-05, `.github/workflows/` contains only a placeholder — add workflows when ready)


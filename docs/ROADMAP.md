# Project Roadmap: Mimiron's Gold-o-Matic

This roadmap reflects the **finalized MVP specification** agreed during design clarification.

Canonical implementation contracts live in:

- `docs/SPEC.md`

**Interaction scenarios & test cases (for implementation / verification):** When executing MVP steps below, agents should use [`docs/INTERACTION_SCENARIOS.md`](INTERACTION_SCENARIOS.md) for scenario IDs (SC-001, …), paired test cases (TC-001, …), and the **Component Contracts** section at each boundary.

**UI/UX (screens, element inventory, flows):** Use [`docs/UI_SPEC.md`](UI_SPEC.md) for **UI-1xx–UI-4xx** panel definitions, Twitch panel constraints (~318px), WPF window layouts, and WoW 3.3.5a frame notes while implementing MVP-3 / MVP-4 / MVP-5.

**Payout model note:** **Subscribers** join by **`!twgold <CharacterName>`** in **broadcast Twitch chat** (monitored by Backend); **character names** are **unique** in the pool. Gold is **not** paid instantly. A **visual roulette** (**every 5 minutes**, minimum **1** participant) picks **one winner**; **non-winners stay**; **winners are removed from the pool when `Sent`**, and may **re-enter** via chat. **Consent** is **WoW whisper `!twgold`** after the **winner notification whisper** (`docs/SPEC.md` §9). **`Sent`** only after **`[MGM_CONFIRM:UUID]`** in **`WoWChatLog.txt`**.

## MVP (End-to-end happy path)

### MVP-0: Repo skeleton

- Create `src/MimironsGoldOMatic.sln`
- Add projects under `src/`:
  - `MimironsGoldOMatic.Shared`
  - `MimironsGoldOMatic.Backend`
  - `MimironsGoldOMatic.Desktop`
  - `MimironsGoldOMatic.TwitchExtension`
  - `MimironsGoldOMatic.WoWAddon`

Spec links:

- `docs/SPEC.md#4-identity-idempotency-and-dtos`

### MVP-1: Shared contracts (`MimironsGoldOMatic.Shared`)

- Define `PayoutStatus`: `Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired`
- Define `CreatePayoutRequest`: `CharacterName`, `EnrollmentRequestId`
- Define `PayoutDto` fields (MVP):
  - `Id`, `TwitchUserId`, `TwitchDisplayName`, `CharacterName`, `GoldAmount` (fixed 1,000g), `EnrollmentRequestId`, `Status`, `CreatedAt`
- Add shared validation for `CharacterName`

Spec links:

- `docs/SPEC.md#3-statuses--lifecycle-transitions`
- `docs/SPEC.md#4-identity-idempotency-and-dtos`

#### Step-by-step prompt (MVP-1)

Acting as **[Backend/API Expert]**:

- Read `docs/SPEC.md` and `docs/MimironsGoldOMatic.Shared/ReadME.md`.
- Initialize the .NET 10 Class Library project `MimironsGoldOMatic.Shared` inside `/src`.
- Implement the shared contracts **as documented**:
  - `PayoutStatus` enum including: `Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired`
  - `PayoutDto` record including: `TwitchUserId`, `TwitchDisplayName`, `CharacterName`, `GoldAmount`, `EnrollmentRequestId`, `Status`, `CreatedAt`
  - `CreatePayoutRequest` record including: `CharacterName`, `EnrollmentRequestId`
- Ensure the namespace is `MimironsGoldOMatic.Shared`.

### MVP-2: Backend API + persistence (`MimironsGoldOMatic.Backend`)

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
  - `POST /api/roulette/verify-candidate` — Desktop submits **`/who`** result (parsed from **`[MGM_WHO]`** in **`WoWChatLog.txt`**); Backend creates **`Pending`** or **no winner** (`docs/SPEC.md` §5, §8)
- Background job:
  - Hourly: mark `Pending`/`InProgress` older than 24h as `Expired` (terminal, no reactivation)
- Auth/security (MVP):
  - Dev Rig-first for Twitch Extension auth (production JWT validation deferred)
  - Desktop uses a pre-shared `ApiKey` (global static key in backend config)

Spec links:

- `docs/SPEC.md#2-mvp-economics--anti-abuse-rules`
- `docs/SPEC.md#3-statuses--lifecycle-transitions`
- `docs/SPEC.md#5-api-contract-mvp` (includes §5.1 pool/roulette GETs)
- `docs/SPEC.md#6-persistence-model-mvp-es-first`
- `docs/SPEC.md#7-expiration-job-mvp`

#### Step-by-step prompt (MVP-2)

Acting as **[Backend/API Expert]**:

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
- Implement expiration job:
  - Hourly background process transitions `Pending/InProgress` older than 24h to `Expired`
- Implement MVP auth posture:
  - Dev Rig-first for Twitch Extension auth
  - Desktop requests must include a pre-shared `ApiKey`

### MVP-3: WoW Addon (`MimironsGoldOMatic.WoWAddon`)

- Implement `ReceiveGold(dataString)` to enqueue **winner** payouts
- Hook `MAIL_SHOW` and provide a side panel UI
- Implement “Prepare Mail” auto-fill:
  - `SendMailNameEditBox`, subject, gold-to-copper via `MoneyInputFrame_SetCopper`
- Support **roulette `/who`** flow if implemented in-client: execute or surface **`/who <Winner_InGame_Nickname>`** results for Desktop/Backend (normative behavior in `docs/SPEC.md`)
- Intercept **whispers** to the streamer where the message is exactly **`!twgold`** and notify the Desktop utility (Backend **acceptance**); winner **must** have been **notified** first per product flow
- **Required:** after mail is sent, print **`[MGM_CONFIRM:UUID]`** to chat (UUID is payout id) for **`WoWChatLog.txt`** / **`Sent`**

Spec links:

- `docs/SPEC.md` (§9 Addon; §10 Chat log / Desktop bridge)

#### Step-by-step prompt (MVP-3)

Acting as **[WoW Addon/Lua Expert]**:

- Read `docs/SPEC.md` and `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`.
- Create the `src/MimironsGoldOMatic.WoWAddon` folder.
- Implement the 3.3.5a addon logic (Interface: 30300):
  - `MimironsGoldOMatic.lua` with global `ReceiveGold(dataString)` to parse and enqueue payouts
  - UI side panel that hooks into `MAIL_SHOW`
  - Auto-fill logic for `SendMailNameEditBox` and `MoneyInputFrame_SetCopper`
- Implement **`!twgold`** whisper detection; print **`[MGM_ACCEPT:UUID]`** to chat for **`WoWChatLog.txt`** / Desktop (no HTTP from Lua)
- **Roulette `/who`:** run **`/who`**, parse **3.3.5a**, emit **`[MGM_WHO]`** + JSON per **`docs/SPEC.md` §8** ( **`WoWChatLog.txt`** ) for Desktop → **`POST /api/roulette/verify-candidate`**
- Emit **`[MGM_CONFIRM:UUID]`** after mail send (**required** for **`Sent`** via chat log)

### MVP-4: Desktop WPF utility (`MimironsGoldOMatic.Desktop`)

- MVVM (CommunityToolkit.Mvvm)
- Queue workflow (explicit claim) for **winner** payouts:
  - `GET /api/payouts/pending`
  - On **Sync/Inject**: `PATCH /api/payouts/{id}/status` -> `InProgress`
- **Roulette coordination:** inject or assist **`/who <Winner_InGame_Nickname>`** and report online status before Backend finalizes winners (see `docs/SPEC.md`)
- WinAPI injection:
  - Target **foreground** `WoW.exe` (MVP)
  - Inject `/run ReceiveGold("...")` with <255 char chunking
  - Use `PostMessage` as primary strategy with `SendInput` fallback
- Feedback loop:
  - Receive **`!twgold`** from addon → call Backend **acceptance** endpoint (not **`Sent`**)
  - **Required:** tail `Logs\WoWChatLog.txt` for **`[MGM_CONFIRM:UUID]`** → mark **`Sent`**
  - Manual overrides in UI:
    - **Mark as Sent**
    - **Fail**
    - **Cancel**

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
  - On **Sync/Inject**: `PATCH /api/payouts/{id}/status` -> `InProgress`
- Implement Win32 `PostMessage` injection to call `ReceiveGold` in WoW:
  - Target the **foreground** `WoW.exe` process (MVP)
  - Implement <255 char chunking for injected `/run` commands
  - Add `SendInput` fallback strategy for blocked/unreliable primary injection
- Implement confirmation loop:
  - **Single** log tail **`Logs\WoWChatLog.txt`**: **`[MGM_WHO]`** → **`POST /api/roulette/verify-candidate`**; **`[MGM_ACCEPT:UUID]`** → Backend **acceptance**; **`[MGM_CONFIRM:UUID]`** → Backend **`Sent`**; configurable log path (**§10**)
  - Allow **`PATCH` `InProgress` → `Pending`** per **`docs/SPEC.md` §3**
  - Provide manual overrides: **Mark as Sent**, **Fail**, **Cancel**
- Use the pre-shared Desktop `ApiKey` when calling Backend endpoints.

### MVP-5: Twitch Extension (`MimironsGoldOMatic.TwitchExtension`)

- Dev Rig-focused integration
- **Enrollment** is **not** form-primary: viewers use **`!twgold <CharacterName>`** in **stream chat** (see `docs/SPEC.md`). Extension shows **instructions + status** (poll Backend).
  - Optional Dev Rig: **`POST /api/payouts/claim`** with `EnrollmentRequestId` for testing
- **Visual roulette** UI:
  - Spin every **5 minutes**; **minimum 1** participant
  - Copy: **subscribe** + **`!twgold <CharacterName>`** in chat; **unique** name; **removed from pool** after **`Sent`**; re-enter via chat
  - **“You won”** + **WoW whisper reply `!twgold`** (after notification whisper, `docs/SPEC.md` §9)
  - Optional UI for **`/who`** / verification state if API exposes it
- Status UX (pull model):
  - `GET /api/payouts/my-last` and/or pool/spin endpoints as implemented

Spec links:

- `docs/SPEC.md#5-api-contract-mvp`
- `docs/SPEC.md` (§11 Twitch Extension)

#### Step-by-step prompt (MVP-5)

Acting as **[Frontend/Twitch Expert]**:

- Read `docs/SPEC.md` and `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`.
- Scaffold `src/MimironsGoldOMatic.TwitchExtension` using Vite + React + TypeScript.
- Build **instructional** UI (chat commands) and status polling — **not** the sole enrollment path.
- Integrate with Twitch Extension helper (`window.Twitch.ext`); optional **`POST /api/payouts/claim`** for Dev Rig with `EnrollmentRequestId`
- Implement **visual roulette** + **5-minute** countdown (aligned with Backend; **no** early spin).
- Implement pull status UX:
  - Call `GET /api/payouts/my-last` (and any pool APIs)
- Ensure alignment with Twitch Dev Rig for MVP debugging.

### MVP-6: End-to-end demo & verification

- Demo scenario: **`!twgold <CharacterName>`** in **chat** → **roulette spin** → **`/who`** online OK → **winner notification whisper** (§9) → winner **`!twgold`** in **WoW** → **confirm-acceptance** → desktop inject → streamer sends mail → **`[MGM_CONFIRM:UUID]`** in log → backend **`Sent`** → **winner removed from pool**
- Add minimal backend tests for:
  - idempotency (`EnrollmentRequestId`)
  - one-active-per-user
  - lifetime cap (10k)
  - expiration behavior
  - roulette / pool rules (at least one spin with **1** participant)

Spec links:

- `docs/SPEC.md`

#### Step-by-step prompt (MVP-6)

Acting as **[Senior Architect]**:

- Review `docs/SPEC.md` and `docs/ROADMAP.md` for end-to-end consistency.
- Ensure all projects are included in `src/MimironsGoldOMatic.sln`.
- Synchronize component behavior and verify the full data flow:
  - **Chat** enrollment + Extension status/roulette UX
  - **`/who`** validates winner online -> notify winner -> Backend **winner payout** `Pending`
  - Desktop explicit claim + inject -> Addon queue
  - **WoW whisper `!twgold`** → acceptance; **`[MGM_CONFIRM:UUID]`** → `Sent` → pool removal
- Finalize setup notes in root docs as needed.

## Beta (Reliability & streamer UX)

- WoW process picker (not just foreground process)
- Better “stuck InProgress” reconciliation and UX guidance
- Batch actions and filtering in Desktop
- ApiKey rotation / reset story (lightweight)

## Production milestone (Security hardening)

- Full Twitch JWT validation (issuer/audience + public key rotation)
- Secrets/config hardening across environments
- Security review (abuse cases, logging hygiene)
- CI pipelines for .NET and frontend builds/tests


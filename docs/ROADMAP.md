# Project Roadmap: Mimiron's Gold-o-Matic

This roadmap reflects the **finalized MVP specification** agreed during design clarification.

Canonical implementation contracts live in:

- `docs/SPEC.md`

**Interaction scenarios & test cases (for implementation / verification):** When executing MVP steps below, agents should use [`docs/INTERACTION_SCENARIOS.md`](INTERACTION_SCENARIOS.md) for scenario IDs (SC-001, …), paired test cases (TC-001, …), and the **Component Contracts** section at each boundary.

**Payout model note:** Redeeming adds viewers to a **participant pool** and **does not** pay gold instantly. A **visual roulette** (default **every 5 minutes**, minimum **1** participant) picks **one winner** per spin; **non-winners stay in the pool**. Before a win is final, **`/who <Winner_InGame_Nickname>`** **must** confirm the player is **online**; the **Extension must notify** the winner and tell them to **whisper `!twgold`** **to receive the gold mail**. Channel Points reward **“Switch to instant spin”** triggers the next spin early. **`Sent`** is set only after **`[MGM_CONFIRM:UUID]`** appears in **`WoWChatLog.txt`** (required mail-send confirmation), after **`!twgold`** acceptance.

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
- Define `CreatePayoutRequest`: `CharacterName`, `TwitchTransactionId`
- Define `PayoutDto` fields (MVP):
  - `Id`, `TwitchUserId`, `TwitchDisplayName`, `CharacterName`, `GoldAmount` (fixed 1,000g), `TwitchTransactionId`, `Status`, `CreatedAt`
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
  - `PayoutDto` record including: `TwitchUserId`, `TwitchDisplayName`, `CharacterName`, `GoldAmount`, `TwitchTransactionId`, `Status`, `CreatedAt`
  - `CreatePayoutRequest` record including: `CharacterName`, `TwitchTransactionId`
- Ensure the namespace is `MimironsGoldOMatic.Shared`.

### MVP-2: Backend API + persistence (`MimironsGoldOMatic.Backend`)

- PostgreSQL + Marten (Event Store) + CQRS read projections
- Persistence rules:
  - Write-side source of truth: Event Store (Marten)
  - Read-side query models/projections (EF Core optional for query mapping)
  - Defensive uniqueness on read model for `TwitchTransactionId` (idempotency)
- Business rules (MVP):
  - Fixed 1,000g per **winning** payout; redemptions **join the pool** first
  - **Roulette** on a **5-minute** cadence (min **1** participant); **“Switch to instant spin”** Channel Points reward
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
  - Additional **pool / spin** endpoints per `docs/SPEC.md` (to be finalized in implementation)
- Background job:
  - Hourly: mark `Pending`/`InProgress` older than 24h as `Expired` (terminal, no reactivation)
- Auth/security (MVP):
  - Dev Rig-first for Twitch Extension auth (production JWT validation deferred)
  - Desktop uses a pre-shared `ApiKey` (global static key in backend config)

Spec links:

- `docs/SPEC.md#2-mvp-economics--anti-abuse-rules`
- `docs/SPEC.md#3-statuses--lifecycle-transitions`
- `docs/SPEC.md#5-api-contract-mvp`
- `docs/SPEC.md#6-persistence-model-mvp-es-first`
- `docs/SPEC.md#7-expiration-job-mvp`

#### Step-by-step prompt (MVP-2)

Acting as **[Backend/API Expert]**:

- Read `docs/SPEC.md`, `docs/MimironsGoldOMatic.Backend/ReadME.md`, and reference `MimironsGoldOMatic.Shared`.
- Create the ASP.NET Core (.NET 10) Web API project `MimironsGoldOMatic.Backend` in `/src`.
- Configure Marten Event Store with PostgreSQL and implement CQRS persistence:
  - Write-side events are the canonical source of truth
  - Read models include `TwitchUserId` and `TwitchDisplayName`
  - Keep `TwitchTransactionId` idempotency guarantees in write/read flow
- Implement endpoints:
  - `POST /api/payouts/claim` (pool **enrollment**; enforce caps + idempotency; rate limit; return `201` for new and `200` for idempotent replay)
  - **Pool / roulette** services (scheduled **5-minute** spin, **instant spin** reward, **min 1** participant; **non-winners stay**; **`/who`** gate; **winner notification** payload for Extension)
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
- Implement **`!twgold`** whisper detection and a **Desktop notification** path (no direct HTTP from Lua)
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
  - Addon-originated **`!twgold`** → Backend **acceptance**
  - **Required** log tail for **`[MGM_CONFIRM:UUID]`** → Backend **`Sent`**
  - Provide manual overrides: **Mark as Sent**, **Fail**, **Cancel**
- Use the pre-shared Desktop `ApiKey` when calling Backend endpoints.

### MVP-5: Twitch Extension (`MimironsGoldOMatic.TwitchExtension`)

- Dev Rig-focused integration
- Enrollment flow (join pool — **not instant payout**):
  - Collect `CharacterName`
  - Submit `TwitchTransactionId`
  - Call `POST /api/payouts/claim` (or future enroll endpoint per `docs/SPEC.md`)
- **Visual roulette** UI:
  - Default spin every **5 minutes**; **minimum 1** participant
  - **“Switch to instant spin”** Channel Points reward skips wait until next spin
  - **Non-winners stay in the pool**
  - **“You won”** notification + **whisper `!twgold`** instructions (**required** to receive gold mail)
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
- Build the Character Name submission form.
- Integrate with Twitch Extension helper (`window.Twitch.ext`) and send claims to Backend:
  - Include `TwitchTransactionId` for idempotency
  - Call `POST /api/payouts/claim` (pool enrollment)
- Implement **visual roulette** + countdown / instant spin behavior (aligned with Backend).
- Implement pull status UX:
  - Call `GET /api/payouts/my-last` (and any pool APIs)
- Ensure alignment with Twitch Dev Rig for MVP debugging.

### MVP-6: End-to-end demo & verification

- Demo scenario: enroll → **roulette spin** → **`/who`** online OK → **notify winner** (“whisper **`!twgold`**) → winner **pending** → desktop inject → winner **replies** **`!twgold`** → streamer sends mail → **`[MGM_CONFIRM:UUID]`** in log → backend **`Sent`**
- Add minimal backend tests for:
  - idempotency (`TwitchTransactionId`)
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
  - Twitch Extension enrollment -> participant pool + roulette UX
  - **`/who`** validates winner online -> notify winner -> Backend **winner payout** `Pending`
  - Desktop explicit claim + inject -> Addon queue
  - **`!twgold`** → acceptance; **`[MGM_CONFIRM:UUID]`** → `Sent`
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


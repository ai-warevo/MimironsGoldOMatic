# Project Roadmap: Mimiron's Gold-o-Matic

This roadmap reflects the **finalized MVP specification** agreed during design clarification.

Canonical implementation contracts live in:

- `docs/SPEC.md`

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
  - Fixed 1,000g per redemption
  - 10,000g lifetime cap per Twitch user
  - One active payout per Twitch user
  - Rate limiting (e.g. ~5 req/min per IP/user)
- Endpoints (MVP):
  - `POST /api/payouts/claim` (`201` new, `200` idempotent replay)
  - `GET /api/payouts/pending`
  - `PATCH /api/payouts/{id}/status`
  - `GET /api/payouts/my-last` (`404` when none exists)
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
  - `POST /api/payouts/claim` (enforce one-active-per-user, 10k lifetime cap, fixed 1000g; rate limit; return `201` for new and `200` for idempotent replay)
  - `GET /api/payouts/pending`
  - `PATCH /api/payouts/{id}/status`
  - `GET /api/payouts/my-last` (return `404` when none exists)
- Implement expiration job:
  - Hourly background process transitions `Pending/InProgress` older than 24h to `Expired`
- Implement MVP auth posture:
  - Dev Rig-first for Twitch Extension auth
  - Desktop requests must include a pre-shared `ApiKey`

### MVP-3: WoW Addon (`MimironsGoldOMatic.WoWAddon`)

- Implement `ReceiveGold(dataString)` to enqueue payouts
- Hook `MAIL_SHOW` and provide a side panel UI
- Implement “Prepare Mail” auto-fill:
  - `SendMailNameEditBox`, subject, gold-to-copper via `MoneyInputFrame_SetCopper`
- Print confirmation tag to chat:
  - `[MGM_CONFIRM:UUID]` (UUID is payout id)

Spec links:

- `docs/SPEC.md#9-addon-payload-format-mvp`
- `docs/SPEC.md#10-chat-log-parsing-mvp`

#### Step-by-step prompt (MVP-3)

Acting as **[WoW Addon/Lua Expert]**:

- Read `docs/SPEC.md` and `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`.
- Create the `src/MimironsGoldOMatic.WoWAddon` folder.
- Implement the 3.3.5a addon logic (Interface: 30300):
  - `MimironsGoldOMatic.lua` with global `ReceiveGold(dataString)` to parse and enqueue payouts
  - UI side panel that hooks into `MAIL_SHOW`
  - Auto-fill logic for `SendMailNameEditBox` and `MoneyInputFrame_SetCopper`
- Emit confirmation to chat in the exact format:
  - `[MGM_CONFIRM:UUID]`

### MVP-4: Desktop WPF utility (`MimironsGoldOMatic.Desktop`)

- MVVM (CommunityToolkit.Mvvm)
- Queue workflow (explicit claim):
  - `GET /api/payouts/pending`
  - On **Sync/Inject**: `PATCH /api/payouts/{id}/status` -> `InProgress`
- WinAPI injection:
  - Target **foreground** `WoW.exe` (MVP)
  - Inject `/run ReceiveGold("...")` with <255 char chunking
  - Use `PostMessage` as primary strategy with `SendInput` fallback
- Feedback loop:
  - Tail `Logs\WoWChatLog.txt` and detect `[MGM_CONFIRM:UUID]` from actual send confirmation -> mark `Sent`
  - Manual overrides in UI:
    - **Mark as Sent**
    - **Fail**
    - **Cancel**

Spec links:

- `docs/SPEC.md#5-api-contract-mvp`
- `docs/SPEC.md#8-desktop--wow-injection-specification-mvp`
- `docs/SPEC.md#10-chat-log-parsing-mvp`

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
  - Tail `Logs\WoWChatLog.txt` for `[MGM_CONFIRM:UUID]` -> `Sent`
  - Provide manual overrides: **Mark as Sent**, **Fail**, **Cancel**
- Use the pre-shared Desktop `ApiKey` when calling Backend endpoints.

### MVP-5: Twitch Extension (`MimironsGoldOMatic.TwitchExtension`)

- Dev Rig-focused integration
- Claim flow:
  - Collect `CharacterName`
  - Submit `TwitchTransactionId`
  - Call `POST /api/payouts/claim`
- Status UX (pull model):
  - `GET /api/payouts/my-last`

Spec links:

- `docs/SPEC.md#5-api-contract-mvp`

#### Step-by-step prompt (MVP-5)

Acting as **[Frontend/Twitch Expert]**:

- Read `docs/SPEC.md` and `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`.
- Scaffold `src/MimironsGoldOMatic.TwitchExtension` using Vite + React + TypeScript.
- Build the Character Name submission form.
- Integrate with Twitch Extension helper (`window.Twitch.ext`) and send claims to Backend:
  - Include `TwitchTransactionId` for idempotency
  - Call `POST /api/payouts/claim`
- Implement pull status UX:
  - Call `GET /api/payouts/my-last`
- Ensure alignment with Twitch Dev Rig for MVP debugging.

### MVP-6: End-to-end demo & verification

- Demo scenario: claim -> pending -> desktop inject -> addon send-confirm -> backend sent
- Add minimal backend tests for:
  - idempotency (`TwitchTransactionId`)
  - one-active-per-user
  - lifetime cap (10k)
  - expiration behavior

Spec links:

- `docs/SPEC.md`

#### Step-by-step prompt (MVP-6)

Acting as **[Senior Architect]**:

- Review `docs/SPEC.md` and `docs/ROADMAP.md` for end-to-end consistency.
- Ensure all projects are included in `src/MimironsGoldOMatic.sln`.
- Synchronize component behavior and verify the full data flow:
  - Twitch Extension claim -> Backend pending
  - Desktop explicit claim + inject -> Addon queue
  - Addon confirmation tag -> Desktop detects -> Backend `Sent`
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


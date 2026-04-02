# Project Roadmap: Mimiron's Gold-o-Matic

This roadmap reflects the **finalized MVP specification** agreed during design clarification.

## MVP (End-to-end happy path)

### MVP-0: Repo skeleton

- Create `src/MimironsGoldOMatic.sln`
- Add projects under `src/`:
  - `MimironsGoldOMatic.Shared`
  - `MimironsGoldOMatic.Backend`
  - `MimironsGoldOMatic.Desktop`
  - `MimironsGoldOMatic.TwitchExtension`
  - `MimironsGoldOMatic.WoWAddon`

### MVP-1: Shared contracts (`MimironsGoldOMatic.Shared`)

- Define `PayoutStatus`: `Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired`
- Define `CreatePayoutRequest`: `CharacterName`, `TwitchTransactionId`
- Define `PayoutDto` fields (MVP):
  - `Id`, `TwitchUserId`, `TwitchDisplayName`, `CharacterName`, `GoldAmount` (fixed 1,000g), `TwitchTransactionId`, `Status`, `CreatedAt`
- Add shared validation for `CharacterName`

#### Step-by-step prompt (MVP-1)

Acting as **[Backend/API Expert]**:

- Read `ReadME.md`, `docs/ReadME.md` and `docs/MimironsGoldOMatic.Shared/ReadME.md`.
- Initialize the .NET 10 Class Library project `MimironsGoldOMatic.Shared` inside `/src`.
- Implement the shared contracts **as documented**:
  - `PayoutStatus` enum including: `Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired`
  - `PayoutDto` record including: `TwitchUserId`, `TwitchDisplayName`, `CharacterName`, `GoldAmount`, `TwitchTransactionId`, `Status`, `CreatedAt`
  - `CreatePayoutRequest` record including: `CharacterName`, `TwitchTransactionId`
- Ensure the namespace is `MimironsGoldOMatic.Shared`.

### MVP-2: Backend API + persistence (`MimironsGoldOMatic.Backend`)

- PostgreSQL + EF Core
- DB rules:
  - Unique constraint on `TwitchTransactionId` (idempotency)
- Business rules (MVP):
  - Fixed 1,000g per redemption
  - 10,000g lifetime cap per Twitch user
  - One active payout per Twitch user
  - Rate limiting (e.g. ~5 req/min per IP/user)
- Endpoints (MVP):
  - `POST /api/payouts/claim`
  - `GET /api/payouts/pending`
  - `PATCH /api/payouts/{id}/status`
  - `GET /api/payouts/my-last`
- Background job:
  - Hourly: mark `Pending`/`InProgress` older than 24h as `Expired` (terminal, no reactivation)
- Auth/security (MVP):
  - Dev Rig-first for Twitch Extension auth (production JWT validation deferred)
  - Desktop uses a pre-shared `ApiKey` (global static key in backend config)

#### Step-by-step prompt (MVP-2)

Acting as **[Backend/API Expert]**:

- Read `ReadME.md`, `docs/ReadME.md` and `docs/MimironsGoldOMatic.Backend/ReadME.md` and reference `MimironsGoldOMatic.Shared`.
- Create the ASP.NET Core (.NET 10) Web API project `MimironsGoldOMatic.Backend` in `/src`.
- Configure EF Core with PostgreSQL and implement persistence:
  - Unique constraint on `TwitchTransactionId` (idempotency)
  - Fields include `TwitchUserId` and `TwitchDisplayName`
- Implement endpoints:
  - `POST /api/payouts/claim` (enforce one-active-per-user, 10k lifetime cap, fixed 1000g; rate limit)
  - `GET /api/payouts/pending`
  - `PATCH /api/payouts/{id}/status`
  - `GET /api/payouts/my-last`
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

#### Step-by-step prompt (MVP-3)

Acting as **[WoW Addon/Lua Expert]**:

- Read `ReadME.md`, `docs/ReadME.md` and `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`.
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
- Feedback loop:
  - Tail `Logs\WoWChatLog.txt` and detect `[MGM_CONFIRM:UUID]` -> mark `Sent`
  - Manual overrides in UI:
    - **Mark as Sent**
    - **Fail**
    - **Cancel**

#### Step-by-step prompt (MVP-4)

Acting as **[WPF/WinAPI Expert]**:

- Read `ReadME.md`, `docs/ReadME.md` and `docs/MimironsGoldOMatic.Desktop/ReadME.md` and reference `MimironsGoldOMatic.Shared`.
- Create the WPF Application (.NET 10) `MimironsGoldOMatic.Desktop` in `/src`.
- Use CommunityToolkit.Mvvm for MVVM structure.
- Implement explicit-claim queue flow:
  - `GET /api/payouts/pending`
  - On **Sync/Inject**: `PATCH /api/payouts/{id}/status` -> `InProgress`
- Implement Win32 `PostMessage` injection to call `ReceiveGold` in WoW:
  - Target the **foreground** `WoW.exe` process (MVP)
  - Implement <255 char chunking for injected `/run` commands
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

#### Step-by-step prompt (MVP-5)

Acting as **[Frontend/Twitch Expert]**:

- Read `ReadME.md`, `docs/ReadME.md` and `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`.
- Scaffold `src/MimironsGoldOMatic.TwitchExtension` using Vite + React + TypeScript.
- Build the Character Name submission form.
- Integrate with Twitch Extension helper (`window.Twitch.ext`) and send claims to Backend:
  - Include `TwitchTransactionId` for idempotency
  - Call `POST /api/payouts/claim`
- Implement pull status UX:
  - Call `GET /api/payouts/my-last`
- Ensure alignment with Twitch Dev Rig for MVP debugging.

### MVP-6: End-to-end demo & verification

- Demo scenario: claim -> pending -> desktop inject -> addon prepare mail -> confirm -> backend sent
- Add minimal backend tests for:
  - idempotency (`TwitchTransactionId`)
  - one-active-per-user
  - lifetime cap (10k)
  - expiration behavior

#### Step-by-step prompt (MVP-6)

Acting as **[Senior Architect]**:

- Review `ReadME.md`, `docs/ReadME.md` and `docs/ROADMAP.md` for end-to-end consistency.
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


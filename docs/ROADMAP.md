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

### MVP-3: WoW Addon (`MimironsGoldOMatic.WoWAddon`)

- Implement `ReceiveGold(dataString)` to enqueue payouts
- Hook `MAIL_SHOW` and provide a side panel UI
- Implement “Prepare Mail” auto-fill:
  - `SendMailNameEditBox`, subject, gold-to-copper via `MoneyInputFrame_SetCopper`
- Print confirmation tag to chat:
  - `[MGM_CONFIRM:UUID]` (UUID is payout id)

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

### MVP-5: Twitch Extension (`MimironsGoldOMatic.TwitchExtension`)

- Dev Rig-focused integration
- Claim flow:
  - Collect `CharacterName`
  - Submit `TwitchTransactionId`
  - Call `POST /api/payouts/claim`
- Status UX (pull model):
  - `GET /api/payouts/my-last`

### MVP-6: End-to-end demo & verification

- Demo scenario: claim -> pending -> desktop inject -> addon prepare mail -> confirm -> backend sent
- Add minimal backend tests for:
  - idempotency (`TwitchTransactionId`)
  - one-active-per-user
  - lifetime cap (10k)
  - expiration behavior

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


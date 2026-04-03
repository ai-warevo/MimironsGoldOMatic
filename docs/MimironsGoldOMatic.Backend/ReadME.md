## MimironsGoldOMatic.Backend (ASP.NET Core | Bridge between Twitch & WPF Desktop app)

- **Role:** Orchestrates the **participant pool**, **roulette spins**, **payout queue**, and persistent storage.
- **Stack:** ASP.NET Core, Marten (Event Store), PostgreSQL, EF Core (read models only).

## Key Functions

- **Authentication (phased):**
  - **MVP:** optimize for Twitch Dev Rig debugging; production-ready Twitch JWT validation (issuer/audience + key rotation) is a roadmap milestone.
  - **Desktop security (MVP):** Desktop-to-Backend uses a pre-shared `ApiKey` (locally trusted Desktop app).
- **Persistence model (MVP):**
  - Write-side source of truth: Marten Event Store in PostgreSQL.
  - Read-side query model: projections/read tables (EF Core optional for mapping/querying projections).
- **Idempotency:** `TwitchTransactionId` is stored and enforced unique for **redemptions / pool enrollment**.
- **Abuse prevention (MVP):**
  - Fixed 1,000g per **winning** payout (after a spin selects a winner).
  - Max 10,000g lifetime total per Twitch user.
  - One active payout per Twitch user at a time.
  - Rate limiting (e.g. ~5 req/min per IP/user).
- **Roulette (MVP):**
  - **Visual roulette** cadence: default **every 5 minutes**; **minimum 1** participant.
  - **Non-winners remain in the pool** after each spin.
  - **Online gate:** spin resolution **must** use **`/who <Winner_InGame_Nickname>`** before **`Pending` payout**; offline candidates invalid (re-draw policy per `docs/SPEC.md`).
  - **Winner notification:** API/state so the **Twitch Extension** can show **“You won”** and instruct **whisper `!twgold`** to receive gold mail.
  - Channel Points reward **“Switch to instant spin”** triggers the **next** spin early (skips the wait for the current window).
- **Acceptance vs sent:**
  - Record **willingness to accept** gold when Desktop reports the winner’s **`!twgold`** whisper.
  - Set **`Sent`** only when Desktop reports **`[MGM_CONFIRM:UUID]`** observed in **`WoWChatLog.txt`** (mail actually sent).
- **Expiration:** Hourly background job marks `Pending`/`InProgress` older than 24 hours as `Expired` (no reactivation).

## API Endpoints

- **POST** `/api/payouts/claim`: Receives redemptions from Twitch. Validates inputs and **adds the viewer to the participant pool** (does **not** create an instant payable payout by itself; see `docs/SPEC.md`).
- **GET** `/api/payouts/pending`: Fetched by the Desktop App. Returns **winner** payouts available for sync/injection (primarily `Pending`).
- **PATCH** `/api/payouts/{id}/status`: Updates payout status where allowed (Desktop), including **`Sent`** after mail-send confirmation.
- **POST** `/api/payouts/{id}/confirm-acceptance` (recommended): Desktop reports **`!twgold`** matched the winner → record **acceptance** (not **`Sent`**).
- **GET** `/api/payouts/my-last`: Used by the Twitch Extension (pull model) to show the viewer their latest payout status.
  - Returns `404 Not Found` when no payout exists for caller.
- **Pool / spin endpoints:** Additional routes for pool state, spin scheduling, and instant spin (see `docs/SPEC.md`; finalized during implementation).

## Additional Libraries

- `Marten`
- `Npgsql`
- `Npgsql.EntityFrameworkCore.PostgreSQL` (query-side mapping only)
- `Microsoft.AspNetCore.Authentication.JwtBearer`

## Architecture & Patterns
- **Idempotency Pattern:**
  Use `TwitchTransactionId` as the idempotency key. If a network lag causes the extension to send the same request twice, the backend must return the existing record instead of creating a duplicate or consuming limits.
  
- **Outbox Pattern:**
  For any external notifications (Discord, logging), save them to an `Outbox` table within the same transaction as the payout. Use a background worker to process them. This ensures data consistency even if external services are down.

- **Specification Pattern (Business Rules):**
  Encapsulate business logic in Specification classes:
  - `LifetimeLimitSpecification`: Checks the 10k gold cap.
  - `ActiveRequestSpecification`: Ensures only one active request per Twitch user.
  This makes business rules readable, testable, and reusable.

## Event Sourcing
- **Marten Integration:** Persist every state change as a sequence of events (`ClaimCreated`, `InjectedByDesktop`, `WinnerAcceptedGold`, `MailSendConfirmed`, etc.). This provides a full audit trail for both the streamer and developers.

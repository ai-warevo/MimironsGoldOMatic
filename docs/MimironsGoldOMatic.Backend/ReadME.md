## MimironsGoldOMatic.Backend (ASP.NET Core | Bridge between Twitch & WPF Desktop app)

- **UI spec (consumer-facing):** Extension/Desktop/Addon behaviors that the API supports are summarized in `docs/UI_SPEC.md`; API shapes remain canonical in `docs/SPEC.md`.
- **Role:** Orchestrates the **participant pool**, **roulette spins**, **payout queue**, and persistent storage.
- **Stack:** ASP.NET Core, Marten (Event Store), PostgreSQL, EF Core (read models only).

## Key Functions

- **Authentication (phased):**
  - **MVP:** optimize for Twitch Dev Rig debugging; production-ready Twitch JWT validation (issuer/audience + key rotation) is a roadmap milestone.
  - **Desktop security (MVP):** Desktop-to-Backend uses a pre-shared `ApiKey` (locally trusted Desktop app).
- **Persistence model (MVP):**
  - Write-side source of truth: Marten Event Store in PostgreSQL.
  - Read-side query model: projections/read tables (EF Core optional for mapping/querying projections).
- **Chat ingestion (MVP):** **EventSub** `channel.chat.message` — ingest **`!twgold <CharacterName>`** only (enroll / **replace** name for same user per `docs/SPEC.md` §5). **Subscriber** + **unique** name among others; non-subscribers: **log only** (no chat reply). Dedupe by Twitch **`message_id`**. **Acceptance** is **`POST .../confirm-acceptance`** after **`[MGM_ACCEPT:UUID]`** in **`WoWChatLog.txt`** (see `docs/SPEC.md` §9–10).
- **Roulette `/who`:** Desktop parses **`[MGM_WHO]`** from **`WoWChatLog.txt`** and forwards JSON → **`POST /api/roulette/verify-candidate`**; Backend **authoritatively** creates **`Pending`** or **no winner** (**no** second candidate in the same **5-minute** cycle — see `docs/SPEC.md` §1, §5, §8).
- **Idempotency / pool:** **Unique `CharacterName`** in active pool; optional **`EnrollmentRequestId`** for Extension **`POST /api/payouts/claim`**.
- **Abuse prevention (MVP):**
  - Fixed 1,000g per **winning** payout (after a spin selects a winner).
  - Max 10,000g lifetime total per Twitch user.
  - One active payout per Twitch user at a time.
  - Rate limiting (e.g. ~5 req/min per IP/user).
- **Roulette (MVP):**
  - **Visual roulette** cadence: default **every 5 minutes**; **minimum 1** participant.
  - **Candidate selection:** **uniform random** among active pool rows (`docs/SPEC.md` glossary, §5).
  - **Non-winners remain in the pool** after each spin; **winners are removed when `Sent`** (may re-enroll via chat).
  - **Online gate:** spin resolution **must** use **`/who <Winner_InGame_Nickname>`** before **`Pending` payout**; offline candidates invalid (**no** second pick same cycle — `docs/SPEC.md`).
  - **Winner notification:** API/state so the **Twitch Extension** can show **“You won”**; **in-game** whisper flow per **`docs/SPEC.md` §9** (Russian text + reply **`!twgold`**).
- **Acceptance vs sent:**
  - Record **willingness to accept** when Desktop calls **`confirm-acceptance`** after observing **`[MGM_ACCEPT:UUID]`** in **`WoWChatLog.txt`** (addon printed after Lua whisper **`!twgold`** match; `docs/SPEC.md` §9–10).
  - Set **`Sent`** only when Desktop reports **`[MGM_CONFIRM:UUID]`** observed in **`WoWChatLog.txt`** (mail actually sent); then **remove winner from pool**.
- **Expiration:** Hourly background job marks `Pending`/`InProgress` older than 24 hours as `Expired` (no reactivation).

## API Endpoints

- **POST** `/api/payouts/claim` (optional): Extension/Dev Rig enrollment; same rules as **`!twgold <CharacterName>`** (**subscriber**, **unique** name). Primary enrollment is **Twitch chat** (see `docs/SPEC.md`).
- **GET** `/api/payouts/pending`: Fetched by the Desktop App. Returns **winner** payouts available for sync/injection (primarily `Pending`).
- **PATCH** `/api/payouts/{id}/status`: Updates payout status where allowed (Desktop), including **`Sent`** after mail-send confirmation.
- **POST** `/api/payouts/{id}/confirm-acceptance` (recommended): Desktop reports **`!twgold`** matched the winner → record **acceptance** (not **`Sent`**).
- **GET** `/api/payouts/my-last`: Used by the Twitch Extension (pull model) to show the viewer their latest payout status.
  - Returns `404 Not Found` when no payout exists for caller.
- **Pool / spin endpoints (MVP):** **`GET /api/roulette/state`**, **`GET /api/pool/me`** (Extension **JWT-only**); **`POST /api/roulette/verify-candidate`** (Desktop **ApiKey**) — see `docs/SPEC.md` §5–5.1.

## Additional Libraries

- `Marten`
- `Npgsql`
- `Npgsql.EntityFrameworkCore.PostgreSQL` (query-side mapping only)
- `Microsoft.AspNetCore.Authentication.JwtBearer`

## Architecture & Patterns
- **Idempotency Pattern:**
  Use `EnrollmentRequestId` as the idempotency key. If a network lag causes the extension to send the same request twice, the backend must return the existing record instead of creating a duplicate or consuming limits.
  
- **Outbox Pattern:** **Do not** add an Outbox table in MVP **until** the first external notification integration ships; then use the pattern in `docs/SPEC.md` §6 (same transaction as domain events + dispatcher).

- **Specification Pattern (Business Rules):**
  Encapsulate business logic in Specification classes:
  - `LifetimeLimitSpecification`: Checks the 10k gold cap.
  - `ActiveRequestSpecification`: Ensures only one active request per Twitch user.
  This makes business rules readable, testable, and reusable.

## Event Sourcing
- **Marten Integration:** Persist every state change as a sequence of events (`ClaimCreated`, `InjectedByDesktop`, `WinnerAcceptedGold`, `MailSendConfirmed`, etc.). This provides a full audit trail for both the streamer and developers.

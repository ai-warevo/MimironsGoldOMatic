## MimironsGoldOMatic.Backend (ASP.NET Core | Bridge between Twitch & WPF Desktop app)

- **Role:** Orchestrates the payout queue and manages persistent storage.
- **Stack:** ASP.NET Core, EF Core, Marten (Event Store), PostgreSQL.

## Key Functions

- **Authentication (phased):**
  - **MVP:** optimize for Twitch Dev Rig debugging; production-ready Twitch JWT validation (issuer/audience + key rotation) is a roadmap milestone.
  - **Desktop security (MVP):** Desktop-to-Backend uses a pre-shared `ApiKey` (locally trusted Desktop app).
- **Database:** Persists payout payloads via `PayoutEntity` (mapped to PostgreSQL via EF Core).
- **Idempotency:** `TwitchTransactionId` is stored and enforced unique (one redemption = one payout).
- **Abuse prevention (MVP):**
  - Fixed 1,000g per redemption.
  - Max 10,000g lifetime total per Twitch user.
  - One active payout per Twitch user at a time.
  - Rate limiting (e.g. ~5 req/min per IP/user).
- **Expiration:** Hourly background job marks `Pending`/`InProgress` older than 24 hours as `Expired` (no reactivation).

## API Endpoints

- **POST** `/api/payouts/claim`: Receives redemptions from Twitch. Validates inputs, saves as `Pending`.
- **GET** `/api/payouts/pending`: Fetched by the Desktop App. Returns the queue available for sync/injection (primarily `Pending`).
- **PATCH** `/api/payouts/{id}/status`: Updates status. The Desktop App calls this to mark a payout as `Sent` after the WoW action completes.
- **GET** `/api/payouts/my-last`: Used by the Twitch Extension (pull model) to show the viewer their latest payout status.

## Additional Libraries

- `Npgsql.EntityFrameworkCore.PostgreSQL`
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
- **Marten Integration:** Persist every state change as a sequence of events (`ClaimCreated`, `InjectedByDesktop`, `ConfirmedInGame`). This provides a full audit trail for both the streamer and developers.

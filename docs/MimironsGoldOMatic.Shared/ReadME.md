## MimironsGoldOMatic.Shared (.NET 10)

- **Role:** Single source of truth for data structures used by both the Backend and the Desktop App.
- **Stack:** .NET 10 Class Library.

## High-Level Patterns
- **DDD (Domain-Driven Design):** The core logic, limits (10k gold), and state transitions must be encapsulated within the Domain layer (Aggregates/Value Objects).
- **CQRS (Command Query Responsibility Segregation):** Clear separation between write operations (Commands) and read operations (Queries). Use **MediatR** for dispatching.
- **Event Sourcing (ES):** The system of record should be an Event Store (using **Marten** with PostgreSQL). Every change to a payout must be a persisted event for 100% auditability.
- **Audit & Scalability:** Design for high availability and full transparency. A streamer should be able to see exactly when and why a payout failed or was delayed.

## Low-level Patterns
- **FluentValidation:** Implement shared validation rules for `PayoutDto` and `CreatePayoutRequest`. Character name patterns and gold limits must be validated consistently across Backend and Desktop.
- **Primary Constructors:** Use C# 14 / .NET 10 primary constructors for all DTOs and Records.
- **Result Pattern:** Use `FluentResults` for domain and service layer responses instead of throwing exceptions.

## Core Entities

- **PayoutStatus (Enum):**
  - `Pending` (Initial)
  - `InProgress` (Explicitly claimed by Desktop when streamer clicks Sync/Inject)
  - `Sent` (Confirmed in-game or manually marked)
  - `Failed` (Error occurred)
  - `Cancelled` (Streamer cancelled)
  - `Expired` (Older than 24h; closed permanently)
- **PayoutDto (Record):**
  - `Guid Id`
  - `string TwitchUserId` (logic: limits, concurrency)
  - `string TwitchDisplayName` (UX: shown in Desktop)
  - `string CharacterName`
  - `long GoldAmount` (MVP fixed at 1,000g)
  - `string TwitchTransactionId` (idempotency: unique per Twitch redemption)
  - `PayoutStatus Status`
  - `DateTime CreatedAt`
- **CreatePayoutRequest (Record):** Used by Twitch Extension to initiate a claim:
  - `string CharacterName`
  - `string TwitchTransactionId`

## Validation / Logic

Contains shared validation (e.g., CharacterName regex). MVP business rules like fixed gold amount, lifetime caps,
and concurrency limits are enforced by the Backend.

Status and API semantics are normative in `docs/SPEC.md`:

- `POST /api/payouts/claim`: `201` for new creation, `200` for idempotent duplicate replay.
- `GET /api/payouts/my-last`: `404` when no payout exists for caller.

## MimironsGoldOMatic.Shared (.NET 10)

- **Role:** Single source of truth for data structures used by both the Backend and the Desktop App.
- **Stack:** .NET 10 Class Library.

## High-Level Patterns
- **DDD (Domain-Driven Design):** The core logic, limits (10k gold), and state transitions must be encapsulated within the Domain layer (Aggregates/Value Objects).
- **CQRS (Command Query Responsibility Segregation):** Clear separation between write operations (Commands) and read operations (Queries). Use **MediatR** for dispatching **in `MimironsGoldOMatic.Backend` only** — **not** in Shared. This project (`Shared`) holds **DTOs, validation, shared types**; **handlers/pipelines** live in the Backend.
- **Event Sourcing (ES):** The system of record should be an Event Store (using **Marten** with PostgreSQL). Every change to a payout must be a persisted event for 100% auditability.
- **Audit & Scalability:** Design for high availability and full transparency. A streamer should be able to see exactly when and why a payout failed or was delayed.

## Low-level Patterns
- **FluentValidation:** Implement shared validation rules for `PayoutDto` and `CreatePayoutRequest`. Character name patterns and gold limits must be validated consistently across Backend and Desktop.
- **Primary Constructors:** Use C# 14 / .NET 10 primary constructors for all DTOs and Records.
- **Result Pattern:** Use `FluentResults` for domain and service layer responses instead of throwing exceptions.

## Core Entities

- **PayoutStatus (Enum):** (applies to **winner** payouts after a roulette spin)
  - `Pending` (Initial)
  - `InProgress` (Explicitly claimed by Desktop when streamer clicks Sync/Inject)
  - `Sent` (Mail-send confirmed via **`[MGM_CONFIRM:UUID]`** in **`WoWChatLog.txt`** → Desktop/Backend, or manually marked)
  - `Failed` (Error occurred)
  - `Cancelled` (Streamer cancelled)
  - `Expired` (Older than 24h; closed permanently)
- **PayoutDto (Record):**
  - `Guid Id`
  - `string TwitchUserId` (logic: limits, concurrency)
  - `string TwitchDisplayName` (UX: shown in Desktop)
  - `string CharacterName`
  - `long GoldAmount` (MVP fixed at 1,000g)
  - `string EnrollmentRequestId` (links payout to pool enrollment / audit trail where stored)
  - `PayoutStatus Status`
  - `DateTime CreatedAt`
- **CreatePayoutRequest (Record):** Used by Twitch Extension to **join the participant pool**:
  - `string CharacterName`
  - `string EnrollmentRequestId`

## Validation / Logic

Contains shared validation for **`CharacterName`** per **`docs/SPEC.md` §4** (length **2–12**; Latin/Cyrillic letters only; no spaces). MVP business rules like fixed gold amount, lifetime caps,
and concurrency limits are enforced by the Backend.

Status and API semantics are normative in `docs/SPEC.md`:

- `POST /api/payouts/claim`: `201` for new creation, `200` for idempotent duplicate replay.
- `GET /api/payouts/my-last`: `404` when no payout exists for caller.

Field labels and validation-driven UX are aligned with **`docs/UI_SPEC.md`**. **Enrollment** is primarily via Twitch chat **`!twgold <CharacterName>`**; optional **`CreatePayoutRequest`** / **`EnrollmentRequestId`** applies to Extension/Dev Rig paths only (see `docs/SPEC.md`).

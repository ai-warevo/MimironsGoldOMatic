<!-- Updated: 2026-04-05 -->

## MimironsGoldOMatic.Shared (.NET 10)

- **Role:** Single source of truth for data structures used by both the **EBS** (`MimironsGoldOMatic.Backend`) and the Desktop app.
- **Stack:** .NET 10 Class Library.

## High-Level Patterns
- **DDD (Domain-Driven Design):** The core logic, limits (10k gold), and state transitions must be encapsulated within the Domain layer (Aggregates/Value Objects).
- **CQRS (Command Query Responsibility Segregation):** Clear separation between write operations (Commands) and read operations (Queries). Use **MediatR** for dispatching **in `MimironsGoldOMatic.Backend` (EBS) only** — **not** in Shared. This project (`Shared`) holds **DTOs, validation, shared types**; **handlers/pipelines** live in the **EBS**.
- **Event Sourcing (ES):** The system of record should be an Event Store (using **Marten** with PostgreSQL). Every change to a payout must be a persisted event for 100% auditability.
- **Audit & Scalability:** Design for high availability and full transparency. A streamer should be able to see exactly when and why a payout failed or was delayed.

## Low-level Patterns
- **FluentValidation:** Shared validators for `PayoutDto` and `CreatePayoutRequest`; **`CharacterNameRules`** for format checks.
- **Primary constructors / records:** DTOs such as **`PayoutDto`**, **`CreatePayoutRequest`** are **`record`** types in this assembly.
- **Application results:** The **EBS** uses **`HandlerResult<T>`** + **`ApiErrorDto`** in MediatR handlers — **not** part of this library.

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
  - `bool IsRewardSentAnnouncedToChat` (read/API shape; Helix §11 at-most-once flag per `docs/SPEC.md` §6 — defaults to `false` in Shared)
- **CreatePayoutRequest (Record):** Used by Twitch Extension to **join the participant pool**:
  - `string CharacterName`
  - `string EnrollmentRequestId`

## Validation / Logic

Contains shared validation for **`CharacterName`**: **length 2–12** (after trim) and **Unicode letters in Latin or Cyrillic script blocks only** (no digits, punctuation, or spaces), implemented in **`CharacterNameRules`** and FluentValidation (`docs/SPEC.md` §4).

**`PayoutEconomics.MvpWinningPayoutGold`:** fixed **1,000g** per winning payout on `PayoutDto` (SPEC §2); validated in **`PayoutDtoValidator`**.

Other MVP business rules (lifetime caps, concurrency limits) are enforced by the Backend.

Status and API semantics are normative in `docs/SPEC.md`:

- `POST /api/payouts/claim`: `201` for new creation, `200` for idempotent duplicate replay.
- `GET /api/payouts/my-last`: `404` when no payout exists for caller.

Field labels and validation-driven UX are aligned with **`docs/UI_SPEC.md`**. **Enrollment** is primarily via Twitch chat **`!twgold <CharacterName>`**; optional **`CreatePayoutRequest`** / **`EnrollmentRequestId`** applies to Extension/Dev Rig paths only (see `docs/SPEC.md`).

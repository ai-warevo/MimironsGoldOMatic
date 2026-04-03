# General Architectural Requirements (All Components) & Repo Layout

## High-Level Patterns
- **DDD (Domain-Driven Design):** The core logic, limits (10k gold), and state transitions must be encapsulated within the Domain layer (Aggregates/Value Objects).
- **CQRS (Command Query Responsibility Segregation):** Clear separation between write operations (Commands) and read operations (Queries). Use **MediatR** for dispatching.
- **Event Sourcing (ES):** The system of record should be an Event Store (using **Marten** with PostgreSQL). Every change to a payout must be a persisted event for 100% auditability.
- **Audit & Scalability:** Design for high availability and full transparency. A streamer should be able to see exactly when and why a payout failed or was delayed.

### MVP persistence stance (fixed)
- **ES-first in MVP:** Marten/Event Store is the write-side source of truth.
- **EF Core scope in MVP:** read-model projections only (query side), not the canonical write store.

## MimironsGoldOMatic.Shared (.NET 10)
- **FluentValidation:** Implement shared validation rules for `PayoutDto` and `CreatePayoutRequest`. Character name patterns and gold limits must be validated consistently across Backend and Desktop.
- **Primary Constructors:** Use C# 14 / .NET 10 primary constructors for all DTOs and Records.
- **Result Pattern:** Use `FluentResults` for domain and service layer responses instead of throwing exceptions.


## High-level Workflow
1. **Redemption:** A viewer enters their Character Name in the Twitch Extension and spends Channel Points; they are **added to the participant pool** (not paid instantly).
2. **Roulette:** A **visual roulette** runs on a **5-minute** cadence by default (minimum **1** participant). **Non-winners remain in the pool.** Viewers may redeem **“Switch to instant spin”** to trigger the next spin early. **Online check:** **`/who <Winner_InGame_Nickname>`** before finalizing the winner.
3. **Winner payout:** When a spin yields an **online-verified** winner, the Backend creates **payout** state; the **Extension notifies** the winner and instructs **whisper `!twgold`** to receive gold.
4. **Synchronization:** The streamer opens the Desktop WPF App, which fetches **pending winner payouts** via REST API.
5. **Injection:** The Desktop App uses Win32 API (`PostMessage`) to send specialized Lua commands into the WoW client.
6. **Execution:** The WoW Addon receives the commands, populates an internal queue, and provides mail UI helpers for sending gold.
7. **Acceptance:** After **winner notification**, the winner **replies** with whisper **`!twgold`** (required **to receive the gold mail**); the addon notifies Desktop → **server records acceptance**.
8. **Mail sent:** The addon prints **`[MGM_CONFIRM:UUID]`**; Desktop tails **`WoWChatLog.txt`** → **server** marks **`Sent`**.

## MVP Specification (final)

- **GoldAmount:** fixed **1,000g** per **winning** payout (MVP).
- **Limits:** max **10,000g lifetime** per Twitch user.
- **Concurrency:** **one active payout** per Twitch user at a time (when a payout exists).
- **Idempotency:** `TwitchTransactionId` is persisted and enforced unique for redemptions/enrollment.
- **Statuses** (winner payout): `Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired` (24h).
- **Expiration:** Backend hourly job marks `Pending`/`InProgress` older than 24h as `Expired` (no reactivation).
- **Confirmation:** **`/who`** online gate; **winner notification**; **`!twgold`** reply → acceptance on Backend; **`[MGM_CONFIRM:UUID]`** in **`WoWChatLog.txt`** → **`Sent`** (required for automation); manual **Mark as Sent** per `docs/SPEC.md`.
- **Auth (MVP):**
  - Twitch Dev Rig first; production JWT validation is roadmap.
  - Desktop uses a pre-shared `ApiKey` to call the Backend.

## Technical specification (canonical)

The canonical, implementation-guiding contracts live in:

- `docs/SPEC.md` (API/DTOs, status transitions, idempotency, persistence rules, payload format, chunking, and log parsing)
- `docs/UI_SPEC.md` (user-facing UI inventory, states, ASCII layouts, navigation flow, design tokens for Extension / Desktop / WoW addon)

## Core Components
- **[Twitch Extension (Frontend)](MimironsGoldOMatic.TwitchExtension/ReadME.md):** Viewer-facing interface for claims.
- **[Backend (API)](MimironsGoldOMatic.Backend/ReadME.md):** Source of truth for payout lifecycle + persistence + authentication.
- **[Shared Library (Contracts)](MimironsGoldOMatic.Shared/ReadME.md):** DTOs/Enums shared between Backend and Desktop.
- **[Desktop Utility (WPF)](MimironsGoldOMatic.Desktop/ReadME.md):** Bridge between the UI and the running WoW client (Win32 automation).
- **[WoW Addon (Lua)](MimironsGoldOMatic.WoWAddon/ReadME.md):** Final in-game executor (mail hooks + UI helpers).

## Repo Layout (expected)
This repository is organized as a monorepo so the contract (`Shared`) and implementations (`Backend`, `Desktop`) evolve together.

```text
MimironsGoldOMatic/
├── .cursor/
│   ├── commands/
│   └── rules/
│       ├── agent-protocol-compat.mdc
│       └── project-rules.mdc
├── .github/
│   └── workflows/
├── docs/
│   ├── SPEC.md, ROADMAP.md, UI_SPEC.md, INTERACTION_SCENARIOS.md, …
│   ├── MimironsGoldOMatic.*/ReadME.md
│   └── prompts/ (Cursor prompt templates + task history)
├── src/ (solution/projects root)
├── README.md (public overview)
├── CONTEXT.md (architecture summary)
└── AGENTS.md (agent roles + mandatory workflow)
```

## Naming Convention
All C# namespaces must start with `MimironsGoldOMatic`.

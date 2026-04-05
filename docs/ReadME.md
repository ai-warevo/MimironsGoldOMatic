<!-- Updated: 2026-04-05 -->

# General Architectural Requirements (All Components) & Repo Layout

## Documentation vs code

Normative architecture and API behavior live in **`docs/SPEC.md`** and **`docs/ROADMAP.md`**. The **`docs/IMPLEMENTATION_READINESS.md`** file tracks (1) doc/spec consistency and (2) **what is implemented under `src/`** today versus MVP steps (**MVP-1…5** shipped in tree; **MVP-6** tests pending).

## High-Level Patterns
- **DDD (Domain-Driven Design):** The core logic, limits (10k gold), and state transitions must be encapsulated within the Domain layer (Aggregates/Value Objects).
- **CQRS (Command Query Responsibility Segregation):** Clear separation between write operations (Commands) and read operations (Queries). Use **MediatR** for dispatching **in the EBS** (`MimironsGoldOMatic.Backend`; `docs/MimironsGoldOMatic.Shared/ReadME.md`).
- **Event Sourcing (ES):** The system of record should be an Event Store (using **Marten** with PostgreSQL). Every change to a payout must be a persisted event for 100% auditability.
- **Audit & Scalability:** Design for high availability and full transparency. A streamer should be able to see exactly when and why a payout failed or was delayed.

### MVP persistence stance (fixed)
- **ES-first in MVP:** Marten/Event Store is the write-side source of truth.
- **EF Core scope in MVP:** read-model projections only (query side), not the canonical write store.

## MimironsGoldOMatic.Shared (.NET 10)
- **FluentValidation:** Shared validation rules for `PayoutDto` and `CreatePayoutRequest` (`CharacterNameRules`, validators). Character name patterns must be validated consistently across **EBS** and Desktop.
- **Primary Constructors:** Use C# 14 / .NET 10 primary constructors for DTOs and records where applicable.
- **EBS application layer:** Handlers return **`HandlerResult<T>`** + **`ApiErrorDto`** (MediatR in **`MimironsGoldOMatic.Backend`**) — not a shared “result” package in **Shared**.


## High-level Workflow
1. **Subscribe + chat enroll:** A **subscriber** types **`!twgold <CharacterName>`** in **broadcast Twitch chat** (**`!twgold`** prefix **case-insensitive**); the **EBS** ingests via **EventSub** and **adds** them to the **participant pool** if the name is **unique** in the pool. Channel Points are **not** used.
2. **Roulette:** A **visual roulette** runs on a **5-minute** cadence (minimum **1** participant); **next spin** time is **server-authoritative** (`GET /api/roulette/state`, `docs/SPEC.md` §5.1); Extension **countdown** uses that API. **Non-winners remain in the pool.** **Winners leave the pool when gold is `Sent`** and may **re-enter** with **`!twgold <CharacterName>`** again. **Online check:** **`/who <Winner_InGame_Nickname>`** before finalizing the winner.
3. **Winner payout:** When a spin yields an **online-verified** winner, the **EBS** creates **payout** state; the **Extension** shows **“You won”**; **Desktop** injects **`NotifyWinnerWhisper`** and the **addon** sends the **winner notification whisper** (`docs/SPEC.md` §8–9); the winner **replies in WoW** with **`!twgold`** before mail.
4. **Synchronization:** The streamer opens the Desktop WPF App, which fetches **pending winner payouts** via REST API.
5. **Injection:** The Desktop App uses Win32 API (`PostMessage`) to send specialized Lua commands into the WoW client.
6. **Execution:** The WoW Addon receives the commands, populates an internal queue, and provides mail UI helpers for sending gold.
7. **Acceptance:** After the **in-game notification whisper**, the winner sends **`!twgold`** in **WoW** (private message); addon prints **`[MGM_ACCEPT:UUID]`** → **`WoWChatLog.txt`** → Desktop → **server records acceptance**.
8. **Mail sent:** The addon prints **`[MGM_CONFIRM:UUID]`**; Desktop tails **`WoWChatLog.txt`** → **server** marks **`Sent`**.

## MVP Specification (final)

- **GoldAmount:** fixed **1,000g** per **winning** payout (MVP).
- **Limits:** max **10,000g lifetime** per Twitch user.
- **Concurrency:** **one active payout** per Twitch user at a time (when a payout exists).
- **Pool rules:** **Unique `CharacterName`** among active pool entries; chat dedupe by message id; optional **`EnrollmentRequestId`** for Extension enroll API.
- **Statuses** (winner payout): `Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired` (24h).
- **Expiration:** **EBS** hourly job marks `Pending`/`InProgress` older than 24h as `Expired` (no reactivation).
- **Confirmation:** **`/who`** online gate; **winner notification**; **`!twgold`** reply → acceptance on **EBS**; **`[MGM_CONFIRM:UUID]`** in **`WoWChatLog.txt`** → **`Sent`** (required for automation); §11 **Helix** line best-effort after **`Sent`**; manual **Mark as Sent** per `docs/SPEC.md`.
- **Auth (MVP):**
  - Extension **JWT** (**HS256** + optional **`aud`**) per **`Program.cs`** / **`TwitchOptions`**; full issuer hardening is roadmap.
  - Desktop uses pre-shared **`X-MGM-ApiKey`** to call the **EBS**.

## Technical specification (canonical)

The canonical, implementation-guiding contracts live in:

- `docs/SPEC.md` (API/DTOs, status transitions, idempotency, persistence rules, payload format, chunking, and log parsing)
- `docs/UI_SPEC.md` (user-facing UI inventory, states, ASCII layouts, navigation flow, design tokens for Extension / Desktop / WoW addon)

## Core Components

In MVP there is **no direct** link between the **Twitch Extension** and the **WPF Desktop** (no peer connection). Viewers interact via Extension + **broadcast chat**; the streamer uses Desktop + **WoW**; both sides use the **EBS** over HTTP, and gold **`Sent`** confirmation from the game follows **`docs/SPEC.md` §8–10** via **`WoWChatLog.txt`**.

- **[Twitch Extension (Frontend)](MimironsGoldOMatic.TwitchExtension/ReadME.md):** Viewer-facing interface for claims.
- **[EBS / Backend (API)](MimironsGoldOMatic.Backend/ReadME.md):** **`MimironsGoldOMatic.Backend`** — source of truth for payout lifecycle + persistence + Twitch integrations.
- **[Shared Library (Contracts)](MimironsGoldOMatic.Shared/ReadME.md):** DTOs/Enums shared between **EBS** and Desktop.
- **[Desktop Utility (WPF)](MimironsGoldOMatic.Desktop/ReadME.md):** Bridge between the UI and the running WoW client (Win32 automation).
- **[WoW Addon (Lua)](MimironsGoldOMatic.WoWAddon/ReadME.md):** Final in-game executor (mail hooks + UI helpers).

## Repo Layout (expected)
This repository is organized as a monorepo so the contract (`Shared`) and implementations (**EBS** / `MimironsGoldOMatic.Backend`, `Desktop`) evolve together.

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

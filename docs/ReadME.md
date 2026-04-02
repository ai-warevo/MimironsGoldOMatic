# Architecture & Repo Layout

## High-level Workflow
1. **Redemption:** A viewer enters their Character Name in the Twitch Extension and spends Channel Points.
2. **Queueing:** The Backend API receives the request, validates it, and stores it in a PostgreSQL database with `Pending` status.
3. **Synchronization:** The streamer opens the Desktop WPF App, which fetches pending payouts via REST API.
4. **Injection:** The Desktop App uses Win32 API (`PostMessage`) to send specialized Lua commands into the WoW client.
5. **Execution:** The WoW Addon receives the commands, populates an internal queue, and provides one-click gold sending via the Mailbox UI.

## MVP Specification (final)

- **GoldAmount:** fixed **1,000g** per redemption (MVP).
- **Limits:** max **10,000g lifetime** per Twitch user.
- **Concurrency:** **one active payout** per Twitch user at a time.
- **Idempotency:** `TwitchTransactionId` is persisted and enforced unique (one redemption = one payout).
- **Statuses:** `Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired` (24h).
- **Expiration:** Backend hourly job marks `Pending`/`InProgress` older than 24h as `Expired` (no reactivation).
- **Confirmation:** Desktop watches `Logs\WoWChatLog.txt` for `[MGM_CONFIRM:UUID]` and also supports a manual **Mark as Sent**.
- **Auth (MVP):**
  - Twitch Dev Rig first; production JWT validation is roadmap.
  - Desktop uses a pre-shared `ApiKey` to call the Backend.

## MVP API Contract (draft)

This section is the **MVP-level** API contract. Names may evolve, but the semantics should remain stable.

### Common concepts

- **Idempotency**: `TwitchTransactionId` is unique per Twitch redemption. The backend must enforce a unique constraint.
- **Statuses**: `Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired`.
- **Expiration**: hourly job transitions `Pending`/`InProgress` older than 24h to `Expired` (terminal).
- **Auth (MVP)**:
  - Twitch Extension requests: Dev Rig-first (production JWT validation is a later milestone).
  - Desktop requests: include a pre-shared `ApiKey` (header name to be finalized in implementation).

### Endpoints

- **POST** `/api/payouts/claim`
  - **Purpose**: create a payout for a Twitch redemption.
  - **Request**: `CharacterName`, `TwitchTransactionId`.
  - **Behavior**:
    - Enforce “one active payout per Twitch user”.
    - Enforce 10,000g lifetime cap per Twitch user.
    - Rate limit (e.g. ~5 req/min per IP/user).
    - Create as `Pending` on success.

- **GET** `/api/payouts/my-last`
  - **Purpose**: viewer pull model; return the caller's latest payout (or none).
  - **Returns**: a `PayoutDto` (or 404/empty contract; to be finalized).

- **GET** `/api/payouts/pending`
  - **Purpose**: Desktop fetches available queue for syncing/injection.
  - **Returns**: list of payouts (primarily `Pending` for MVP).

- **PATCH** `/api/payouts/{id}/status`
  - **Purpose**: Desktop updates lifecycle state.
  - **Allowed MVP transitions** (guideline):
    - `Pending` -> `InProgress` (on **Sync/Inject**)
    - `Pending/InProgress` -> `Cancelled` (streamer)
    - `Pending/InProgress` -> `Failed` (streamer or Desktop failure)
    - `InProgress` -> `Sent` (chat confirm or manual Mark Sent)

### Common error cases (MVP semantics)

- **Duplicate redemption**: same `TwitchTransactionId` already exists (idempotency).
- **Active payout exists**: user already has a non-terminal payout.
- **Lifetime cap reached**: user would exceed 10,000g total.
- **Expired**: payout is older than 24h and is terminal.

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
│   ├── MimironsGoldOMatic.*/ReadME.md
│   └── prompts/ (Cursor prompt templates + task history)
├── src/ (solution/projects root)
├── README.md (public overview)
├── CONTEXT.md (architecture summary)
└── AGENTS.md (agent roles + mandatory workflow)
```

## Naming Convention
All C# namespaces must start with `MimironsGoldOMatic`.

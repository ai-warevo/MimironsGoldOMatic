# Mimiron's Gold-o-Matic — Technical Specification (MVP)

This document is the **canonical implementation contract** for the MVP.  
`docs/ROADMAP.md` contains step-by-step prompts and links into this spec.  
**User-facing UI** (Twitch Extension, WPF, WoW addon screens, states, ASCII layouts): `docs/UI_SPEC.md`.

## 1) Glossary

- **Redemption**: a single Twitch Channel Points redemption event (e.g. claim or “join pool” reward).
- **Participant pool**: the set of Twitch viewers who have redeemed and are eligible for the roulette. **Non-winners of a spin remain in the pool**; they are not removed when someone else is selected.
- **Spin / roulette**: a scheduled or instant selection that picks **one** winner from the current participant pool using a **visual roulette** (viewer-facing, e.g. in the Twitch Extension overlay).
- **Online verification (`/who`)**: the roulette **must** ensure the **winner** (and, when resolving the pool, participants being validated for that spin) are **actually in-game and online** by running the WoW slash command **`/who <Winner_InGame_Nickname>`**, where **`Winner_InGame_Nickname`** is the enrolled **in-game character name** for that participant. Parsing the **`/who`** result (chat frames / Who UI / client-specific behavior for **3.3.5a**) is part of the implementation; injecting **`/who …`** may be done via the **Desktop** utility and/or **addon** automation.
- **Winner notification**: after a winner is determined and **before** or **while** awaiting acceptance, the winner **must** be **notified of their victory** (see §11). They are instructed that **to receive the in-game mail with gold**, they **must** **reply** to the streamer with a **private in-game message** whose text is exactly **`!twgold`**.
- **Spin interval (default)**: **5 minutes** between automatic spins when no instant spin is triggered.
- **Minimum participants**: **1** — a spin may run when exactly one person is in the pool.
- **Instant spin reward**: a Channel Points reward **“Switch to instant spin”** that **accelerates** the next spin (does not wait for the current 5-minute window).
- **Payout**: a backend record representing the intention to mail gold to an in-game character, **created for the current spin winner** (not at the moment a viewer first redeems).
- **Active payout**: a payout in `Pending` or `InProgress`.
- **Terminal payout**: a payout in `Sent`, `Failed`, `Cancelled`, or `Expired`.
- **Acceptance to receive gold (`!twgold`)**: after **winner notification**, the winner **must** **reply** to the streamer via **private message** with exactly **`!twgold`** to confirm they **will accept** the gold and may receive the mail. This **reply** is **not** the same as confirming that mail was sent (see **`[MGM_CONFIRM:UUID]`**).
- **Mail-send confirmation (`[MGM_CONFIRM:UUID]`)**: after the streamer actually sends the in-game mail, the addon **must** print **`[MGM_CONFIRM:UUID]`** to chat so it appears in **`Logs\WoWChatLog.txt`**. Desktop **must** parse this (required); **`Sent`** on the server is driven by this signal (see §10).

## 2) MVP economics & anti-abuse rules

- **GoldAmount**: fixed at **1,000g** per winning payout (per redemption that results in a paid spin outcome — normative detail: enforce per `TwitchUserId` / transaction id as in §4).
- **Lifetime cap**: max **10,000g total** per `TwitchUserId`.
- **Concurrency**: only **one active payout** per `TwitchUserId` at a time (same as before; applies once a viewer becomes a spin winner and a payout record exists).
- **Rate limiting**: ASP.NET Core rate limiting, target ~**5 requests/min** per IP/user (implementation detail).

## 3) Statuses & lifecycle transitions

### Status enum (MVP)

- `Pending`: created for the **selected winner** after a spin, not yet synced/injected by Desktop.
- `InProgress`: explicitly claimed by Desktop when streamer clicks **Sync/Inject** (prepares mail / queue).
- `Sent`: confirmed on the **server** when the Desktop utility observes **`[MGM_CONFIRM:UUID]`** for that payout id in **`Logs\WoWChatLog.txt`** (required mail-send confirmation). The **`!twgold`** whisper records **willingness to accept** earlier in the flow and **does not** replace **`[MGM_CONFIRM:UUID]`**.
- `Failed`: streamer/Desktop marked failure (e.g., faction restriction, injection failure, etc.).
- `Cancelled`: streamer cancelled in Desktop.
- `Expired`: auto-closed by backend when older than 24 hours (terminal).

### Allowed transitions (normative)

| From | To | Who/when |
|---|---|---|
| `Pending` | `InProgress` | Desktop on **Sync/Inject** |
| `Pending` | `Cancelled` | Desktop (streamer) |
| `Pending` | `Failed` | Desktop (streamer) |
| `InProgress` | `Sent` | **Desktop** observes **`[MGM_CONFIRM:UUID]`** in **`Logs\WoWChatLog.txt`** and calls Backend (`PATCH` status or dedicated endpoint); or **manual Mark as Sent** if policy allows |
| `InProgress` | `Cancelled` | Desktop (streamer) |
| `InProgress` | `Failed` | Desktop (streamer) |
| `Pending`/`InProgress` | `Expired` | Backend hourly expiration job |

## 4) Identity, idempotency, and DTOs

### Identity

- **Enforcement key**: `TwitchUserId` (from Twitch identity; numeric string is acceptable for storage).
- **Display-only**: `TwitchDisplayName` (for Desktop / overlay UX).
- **Recipient**: `CharacterName` (single realm assumption; faction failures handled manually by streamer).

### Idempotency

- `TwitchTransactionId` is a unique identifier for a single redemption.
- Backend MUST enforce uniqueness (DB unique constraint / unique index).

**MVP behavior on duplicate `TwitchTransactionId`:**

- Return the existing **enrollment / redemption** record as an idempotent success (no duplicate pool entry or payout), unless the product later splits “enrollment” from “payout” with separate ids (document any change in SPEC).

## 5) API Contract (MVP)

This section defines the MVP endpoints and semantics. Exact JSON shapes below are **guidance** until implementation locks schemas; behavior is normative.

### Common headers

- **Desktop ApiKey**: `X-MGM-ApiKey: <value>`
  - Required for Desktop endpoints (pool sync, status updates, spin triggers if server-authoritative, whisper-forward).
  - The backend stores the key in configuration (global static key for MVP).

### Participant pool & roulette (normative behavior)

- Redeeming via the Extension **adds** the viewer to the **participant pool** (subject to caps and validation). **No payout is created at this step.**
- On each **spin** (scheduled every **5 minutes** or **instant** via **“Switch to instant spin”**), the system selects **one winner** from the pool. **Viewers not selected stay in the pool.**
- **Minimum pool size**: **1** (spin still runs).
- **Online check (required):** Before a spin outcome is **finalized** (e.g. before creating a **`Pending` payout** and before **winner notification**), the system **must** verify the selected player is **online** by running **`/who <Winner_InGame_Nickname>`** using their pool **character name**. If **`/who`** shows they are **not** online, the implementation **must not** treat them as the winner for that cycle (e.g. **re-draw** from the pool, or retry per documented policy—implementation detail, but **offline picks are invalid**).
- After an **online-verified** winner is selected, the Backend **creates** a payout record in `Pending` for that winner’s linked character (or follows the same character capture flow as today — implementation detail).
- **Winner notification (required):** The winner **must** be notified that they won (see §11). The notification **must** state that **to get the gold in the mailbox**, they **must** **whisper** the streamer **`!twgold`** as a **private message** (exact text). The streamer **waits for that whisper** before sending mail (product workflow; enforcement via UX and optional Backend gating).

**Implementation note:** Concrete routes for `GET/POST` pool state, spin scheduling, and Channel Points webhook handling are **to be added** alongside the Twitch Extension and EventSub configuration; until then, treat this section as the required **product behavior** the API must expose. **`/who`** execution/parsing may live in the **addon**, **Desktop** (command injection), or split between them; the **Backend** may record “online verified” timestamps if useful for audit.

### Error model (MVP)

Recommended JSON error shape (server should be consistent):

```json
{
  "code": "active_payout_exists",
  "message": "User already has an active payout.",
  "details": {}
}
```

Recommended `code` values (MVP):

- `duplicate_redemption`
- `active_payout_exists`
- `lifetime_cap_reached`
- `invalid_character_name`
- `unauthorized`
- `forbidden_apikey`
- `terminal_status_change_not_allowed`
- `not_found`
- `pool_empty` (if a spin is requested with zero participants — should not occur if minimum is 1 and spin is only scheduled when valid)

### POST `/api/payouts/claim` (or renamed enroll endpoint)

**Purpose**: accept a Twitch redemption and **add the user to the participant pool** (not an instant payout).

**Request** (illustrative):

```json
{
  "characterName": "Somecharacter",
  "twitchTransactionId": "abc123-redemption-id"
}
```

**Behavior**:

- Validate `characterName`.
- Enforce lifetime cap and idempotency for `twitchTransactionId`.
- **Do not** create a `Pending` payout solely from this call unless the product later distinguishes “claim” vs “win” with separate resources; **MVP**: only **spin winner** yields a payout row.

**Response**: implementation-specific enrollment DTO; `201`/`200` idempotent patterns still apply to the **redemption**.

### GET `/api/payouts/my-last`

**Purpose**: pull model for Twitch Extension; return caller’s latest **payout** (winner flow) or latest **enrollment** — document the chosen model in implementation (SPEC: viewer cares about **pool + last spin result** as well; Extension UI may require additional endpoints).

### GET `/api/payouts/pending` (Desktop)

**Purpose**: Desktop fetches payouts available for syncing/injection (**winner** payouts primarily `Pending`).

### PATCH `/api/payouts/{id}/status` (Desktop)

**Purpose**: Desktop updates lifecycle state where allowed (see §3).

### POST `/api/payouts/{id}/confirm-acceptance` (Desktop) — **recommended**

**Purpose**: Desktop notifies Backend that the addon observed the winner’s whisper **`!twgold`**: the player **confirms willingness to accept** gold (not that mail was sent).

**Request** (illustrative):

```json
{
  "characterName": "Somecharacter"
}
```

**Rules**:

- Guarded by `X-MGM-ApiKey`.
- Backend records acceptance (e.g. `WinnerAcceptedWillingToReceiveAt`); **does not** set **`Sent`**.
- **Product rule:** the streamer should send in-game mail **only after** this acceptance is recorded (enforcement may be UX + optional API guards).

### Mail-send confirmation → `Sent` (Desktop)

**Purpose**: Desktop **must** tail **`Logs\WoWChatLog.txt`** and detect **`[MGM_CONFIRM:UUID]`** (see §10). On match, call **`PATCH /api/payouts/{id}/status`** with **`Sent`** (or a dedicated confirm-mail-sent endpoint).

**Rules**:

- Guarded by `X-MGM-ApiKey`.
- **`[MGM_CONFIRM:UUID]`** is **required** for automated **`Sent`**; it proves the addon reported **mail was sent**.

## 6) Persistence model (MVP, ES-first)

For MVP, the source of truth is Event Sourcing:

- Event Store is implemented with **Marten** on PostgreSQL.
- Payout lifecycle and **roulette / pool** events are written as append-only domain events.
- Read queries are served from read-model projections.
- EF Core MAY be used for read-model tables only, not as the write-side source of truth.

Minimum recommended fields for payout read model (`PayoutsReadModel`):

- `Id` (UUID, PK)
- `TwitchUserId` (string/varchar; indexed)
- `TwitchDisplayName` (string/varchar)
- `CharacterName` (string/varchar)
- `GoldAmount` (bigint/int64; always 1000 in MVP)
- `TwitchTransactionId` (string/varchar; **UNIQUE** where applicable)
- `Status` (enum/string; indexed)
- `CreatedAt` (timestamp; indexed with status for expiration sweep)
- (Optional but recommended) `UpdatedAt` (timestamp)
- (Recommended) `WinnerAcceptedWillingToReceiveAt` (timestamp nullable): set when Desktop reports **`!twgold`** for this payout (acceptance to receive gold).

Additional read models (pool membership, spin schedule, last spin id) are **required** by the roulette feature; define in implementation.

## 7) Expiration job (MVP)

- Runs **hourly**.
- Transitions payouts in `Pending` or `InProgress` to `Expired` when `CreatedAt < now - 24h`.
- `Expired` is terminal and MUST NOT be reactivated.

## 8) Desktop → WoW injection specification (MVP)

### Target process

- Desktop targets the **foreground** `WoW.exe` process in MVP.

### Command format

Desktop injects `/run` commands that invoke the addon entrypoint:

- `/run ReceiveGold("<payload>")`

### Injection strategy (MVP)

- Primary strategy: `PostMessage`.
- Fallback strategy: `SendInput` (operator-switchable in Desktop settings) when primary injection is blocked/unreliable.

### Payload chunking rule (<255 chars)

WoW chat command input has a practical limit (commonly ~255 chars). For MVP:

- Desktop MUST split injections so that **each injected command line** is **< 255 characters**.
- Chunk by **whole payout entries** (never split mid-entry).
- Each chunk results in **one** `ReceiveGold(...)` call.

### Recommended approach (MVP)

- Build a list of payout entries for injection.
- Pack as many complete entries into one payload chunk as possible while keeping the full command line under 255 chars.
- Send multiple `/run ReceiveGold("...")` lines if needed.

### Roulette `/who` verification (MVP)

- For **roulette resolution**, the Desktop utility and/or addon **must** cause the game client to execute **`/who <Winner_InGame_Nickname>`** and interpret the **3.3.5a** result so the Backend (or local orchestration) only **finalizes** a winner who is **online**.
- Command length for **`/who`** is within normal chat limits; reuse the same **focus + injection** reliability notes as for `/run` where applicable.

## 9) Addon: mail queue, whisper `!twgold`, and mail-send tag (MVP)

`ReceiveGold(dataString)` accepts a semicolon-delimited list of payout entries:

- Entry format: `UUID:CharacterName:GoldCopper;`
  - `UUID`: payout id
  - `CharacterName`: WoW character name (MVP validation should prevent `:` and `;`)
  - `GoldCopper`: integer copper amount (MVP: 1000g = 10000000 copper)

Example:

```
2d2b7b2a-1111-2222-3333-444444444444:Somecharacter:10000000;
```

### Whisper interception (normative)

- Register for **whisper** / private-message events (exact WoW 3.3.5a event/API as appropriate for the client).
- When the **sender** is the **expected winner character** (or matches linked identity per implementation) and the **message text** is exactly **`!twgold`**, the addon **notifies the Desktop utility** (out-of-band channel TBD: saved variable ping file, local socket, or other — **not** the public internet from Lua).
- The Desktop utility calls the Backend (e.g. `POST .../confirm-acceptance`) so the server records that the player **is willing to accept** the gold.

**Product rules:**
- Gold should only be mailed after the winner has been **notified** and has **replied** with a **private message** **`!twgold`** (willingness to accept / consent to receive the mail).
- The **`!twgold`** reply **does not** mean mail has been sent; **`Sent`** still requires **`[MGM_CONFIRM:UUID]`** in the log.

### Mail-send tag (normative; required for automated `Sent`)

When the addon confirms the **actual in-game mail send** for a payout, it **must** print to chat (so it is captured in the chat log):

- `[MGM_CONFIRM:UUID]`

where `UUID` is the payout id. Desktop **must** monitor **`Logs\WoWChatLog.txt`** for this pattern and only then transition the payout to **`Sent`** on the server (see §3, §5).

## 10) Chat log parsing & Desktop bridge (MVP)

### Whisper path (acceptance — not `Sent`)

- **`!twgold`** is delivered to the addon via whisper events; the addon forwards acceptance to Desktop → Backend.
- **Do not** rely on **`WoWChatLog.txt`** for **`!twgold`** unless a specific client log pattern is validated; whisper events in Lua are preferred.

### `[MGM_CONFIRM:UUID]` path (required for `Sent`)

Desktop **must** monitor:

- `Logs\WoWChatLog.txt`

Regex (normative): `\\[MGM_CONFIRM:([0-9a-fA-F-]{36})\\]`

Behavior notes:

- Desktop should tolerate log rotation / truncation.
- On match, Desktop updates Backend to **`Sent`** for that payout id.
- Desktop should allow a manual override (**Mark as Sent**) only as an operator escape hatch if automation misses (policy decision).

## 11) Twitch Extension: visual roulette (MVP)

- Display the **participant pool** (or count) and a **visual roulette** animation on each spin.
- Show **countdown** to the next **5-minute** spin; reflect **instant spin** when the **“Switch to instant spin”** reward is redeemed.
- **Online verification** for the winning entry is enforced in the overall product flow using **`/who <Winner_InGame_Nickname>`** before the win is final (see §5); the Extension may reflect “checking…” / “verified” state if the Backend exposes it.
- Present the **winner** to the streamer and **all viewers**. For the **winning viewer**, show an unmistakable **“You won”** notification as soon as the Backend reports their win (after online verification and **`Pending` payout** if applicable).
- That **winner-facing** notification **must** explain: **to receive the in-game letter with the gold**, they **must** open WoW and send the streamer a **private message** with exactly **`!twgold`** (reply whisper). No gold mail is guaranteed until they do.
- After the streamer sends mail in-game, **`[MGM_CONFIRM:UUID]`** in **`WoWChatLog.txt`** confirms **mail was sent** server-side as **`Sent`**.

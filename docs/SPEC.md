# Mimiron's Gold-o-Matic — Technical Specification (MVP)

This document is the **canonical implementation contract** for the MVP.  
`docs/ROADMAP.md` contains step-by-step prompts and links into this spec.

## 1) Glossary

- **Redemption**: a single Twitch Channel Points redemption event.
- **Payout**: a backend record representing the intention to mail gold to an in-game character.
- **Active payout**: a payout in `Pending` or `InProgress`.
- **Terminal payout**: a payout in `Sent`, `Failed`, `Cancelled`, or `Expired`.

## 2) MVP economics & anti-abuse rules

- **GoldAmount**: fixed at **1,000g** per redemption.
- **Lifetime cap**: max **10,000g total** per `TwitchUserId`.
- **Concurrency**: only **one active payout** per `TwitchUserId` at a time.
- **Rate limiting**: ASP.NET Core rate limiting, target ~**5 requests/min** per IP/user (implementation detail).

## 3) Statuses & lifecycle transitions

### Status enum (MVP)

- `Pending`: created, not yet synced/injected by Desktop.
- `InProgress`: explicitly claimed by Desktop when streamer clicks **Sync/Inject**.
- `Sent`: confirmed (chat log confirm) or manually marked in Desktop.
- `Failed`: streamer/Desktop marked failure (e.g., faction restriction, injection failure, etc.).
- `Cancelled`: streamer cancelled in Desktop.
- `Expired`: auto-closed by backend when older than 24 hours (terminal).

### Allowed transitions (normative)

| From | To | Who/when |
|---|---|---|
| `Pending` | `InProgress` | Desktop on **Sync/Inject** |
| `Pending` | `Cancelled` | Desktop (streamer) |
| `Pending` | `Failed` | Desktop (streamer) |
| `InProgress` | `Sent` | Desktop (chat confirmation of actual send detected or manual Mark Sent) |
| `InProgress` | `Cancelled` | Desktop (streamer) |
| `InProgress` | `Failed` | Desktop (streamer) |
| `Pending`/`InProgress` | `Expired` | Backend hourly expiration job |

## 4) Identity, idempotency, and DTOs

### Identity

- **Enforcement key**: `TwitchUserId` (from Twitch identity; numeric string is acceptable for storage).
- **Display-only**: `TwitchDisplayName` (for Desktop UI convenience).
- **Recipient**: `CharacterName` (single realm assumption; faction failures handled manually by streamer).

### Idempotency

- `TwitchTransactionId` is a unique identifier for a single redemption.
- Backend MUST enforce uniqueness (DB unique constraint / unique index).

**MVP behavior on duplicate `TwitchTransactionId`:**

- Return the existing payout as an idempotent success (no new row).

## 5) API Contract (MVP)

This section defines the MVP endpoints and semantics. Exact JSON shapes below are normative for the MVP.

### Common headers

- **Desktop ApiKey**: `X-MGM-ApiKey: <value>`
  - Required for Desktop endpoints (at minimum `GET /pending` and `PATCH /status`).
  - The backend stores the key in configuration (global static key for MVP).

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

### POST `/api/payouts/claim`

**Purpose**: create a payout for a Twitch redemption.

**Request**:

```json
{
  "characterName": "Somecharacter",
  "twitchTransactionId": "abc123-redemption-id"
}
```

**Behavior**:

- Validate `characterName`.
- Enforce one-active-per-user (based on `TwitchUserId`).
- Enforce lifetime cap (10k total, based on `TwitchUserId`).
- If `twitchTransactionId` already exists: return the existing payout (idempotent).
- On success: create payout in `Pending` with `GoldAmount=1000`.

**Response**:

- `201 Created` when a new payout is created.
- `200 OK` when `twitchTransactionId` already exists and the existing payout is returned (idempotent replay).

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "twitchUserId": "123456",
  "twitchDisplayName": "ViewerName",
  "characterName": "Somecharacter",
  "goldAmount": 1000,
  "twitchTransactionId": "abc123-redemption-id",
  "status": "Pending",
  "createdAt": "2026-04-02T12:34:56Z"
}
```

### GET `/api/payouts/my-last`

**Purpose**: pull model for Twitch Extension; return caller’s latest payout.

- Returns latest payout for `TwitchUserId` ordered by `CreatedAt` (descending).
- If none exists: return `404 Not Found`.

### GET `/api/payouts/pending` (Desktop)

**Purpose**: Desktop fetches the queue available for syncing/injection.

- Returns list of payouts (MVP: primarily `Pending`; showing `InProgress` is optional but must be documented in implementation).

### PATCH `/api/payouts/{id}/status` (Desktop)

**Purpose**: Desktop updates lifecycle state.

**Request**:

```json
{
  "status": "InProgress"
}
```

**Rules**:

- Only Desktop can call this in MVP (guarded by `X-MGM-ApiKey`).
- Must enforce transition rules (see section 3).

## 6) Persistence model (MVP, ES-first)

For MVP, the source of truth is Event Sourcing:

- Event Store is implemented with **Marten** on PostgreSQL.
- Payout lifecycle is written as append-only domain events.
- Read queries are served from read-model projections.
- EF Core MAY be used for read-model tables only, not as the write-side source of truth.

Minimum recommended fields for payout read model (`PayoutsReadModel`):

- `Id` (UUID, PK)
- `TwitchUserId` (string/varchar; indexed)
- `TwitchDisplayName` (string/varchar)
- `CharacterName` (string/varchar)
- `GoldAmount` (bigint/int64; always 1000 in MVP)
- `TwitchTransactionId` (string/varchar; **UNIQUE**)
- `Status` (enum/string; indexed)
- `CreatedAt` (timestamp; indexed with status for expiration sweep)
- (Optional but recommended) `UpdatedAt` (timestamp)

**Uniqueness / constraints**:

- `UNIQUE(TwitchTransactionId)` on read model is recommended for defensive consistency checks.
- “One active payout per user” is enforced by domain rules on the write side; DB-level partial unique index on read model is optional.

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

## 9) Addon payload format (MVP)

`ReceiveGold(dataString)` accepts a semicolon-delimited list of payout entries:

- Entry format: `UUID:CharacterName:GoldCopper;`
  - `UUID`: payout id
  - `CharacterName`: WoW character name (MVP validation should prevent `:` and `;`)
  - `GoldCopper`: integer copper amount (MVP: 1000g = 10000000 copper)

Example:

```
2d2b7b2a-1111-2222-3333-444444444444:Somecharacter:10000000;
```

When the addon confirms the relevant action, it prints:

- `[MGM_CONFIRM:UUID]`

MVP confirmation semantic is **send_confirm**:

- `Sent` means confirmation corresponds to the actual send action, not only a prepare step.

## 10) Chat log parsing (MVP)

Desktop monitors:

- `Logs\WoWChatLog.txt`

It detects confirmations using a strict pattern:

- Regex (recommended): `\\[MGM_CONFIRM:([0-9a-fA-F-]{36})\\]`

Behavior notes:

- Desktop should tolerate log rotation / truncation.
- Desktop should allow a manual override (**Mark as Sent**) if the confirm is missed.


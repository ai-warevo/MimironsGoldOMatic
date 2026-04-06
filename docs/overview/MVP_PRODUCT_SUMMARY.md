<!-- Updated: 2026-04-05 (Deduplication pass) -->

# MVP product summary (at a glance)

**Normative source:** [`docs/overview/SPEC.md`](SPEC.md) (APIs, transitions, persistence, log formats). This page is a compact digest for quick orientation and does not override the spec.

## Pool and chat

- Viewers subscribe and enroll using **`!twgold <CharacterName>`** in broadcast Twitch chat (case-insensitive command prefix). The EBS ingests this through EventSub and uses subscriber information from the chat payload only (see SPEC).
- `CharacterName` stays unique within the active pool; the same viewer can replace their current name with a new enrollment message.
- Chat deduplication uses Twitch **`message_id`**. Optional Extension **`POST /api/payouts/claim`** remains a dev/rig path with **`EnrollmentRequestId`** idempotency and **`Mgm:DevSkipSubscriberCheck`**.

## Roulette

- Visual roulette runs on a fixed **5-minute cadence** with no early spin. Countdown UI must use authoritative `nextSpinAt` / `serverNow` from **`GET /api/roulette/state`**.
- Minimum participants: **1**. Non-winners stay in the pool; winners are removed only after payout reaches **`Sent`**, and can re-enroll later.
- Before creating **`Pending`**, the flow requires **`/who <Winner_InGame_Nickname>`** verification: addon emits **`[MGM_WHO]`** JSON into **`WoWChatLog.txt`**, Desktop forwards to **`verify-candidate`**, and offline candidates resolve to **no winner** for that cycle (no same-cycle redraw).

## Gold and limits

- **1,000g** per **winning** payout (MVP). **10,000g** lifetime cap per **`TwitchUserId`**. **One active payout** (`Pending` / `InProgress`) per **`TwitchUserId`**.

## Winner path

- **Extension:** shows “You won” and instructs the winner to reply **`!twgold`** in WoW after receiving the streamer whisper (SPEC §9).
- **Addon:** sends the §9 whisper text and emits **`[MGM_ACCEPT:UUID]`** when it receives a matching **`!twgold`** whisper response.
- **Desktop:** tails the chat log and calls **`confirm-acceptance`** on `[MGM_ACCEPT]` (this records consent, not `Sent`).
- **Mail confirmation:** for MGM-armed sends, addon emits **`[MGM_CONFIRM:UUID]`**; Desktop then updates payout to **`Sent`**. Helix §11 chat message is attempted after `Sent` on a best-effort basis. Manual **Mark as Sent** remains an operator override.

## Payout statuses (winner row)

`Pending` → `InProgress` → `Sent` | `Failed` | `Cancelled`, with `Expired` applied by hourly job after 24h. **`InProgress` → `Pending`** escape hatch is allowed (SPEC §3).

## Identity fields

- **`TwitchUserId`** — enforcement (limits, concurrency).
- **`TwitchDisplayName`** — UX (Desktop / overlays).
- **`CharacterName`** — in-game recipient; validated in **Shared** (SPEC §4).

## Security (MVP)

- **Extension JWT:** **HS256** with **`Twitch:ExtensionSecret`**; optional **`aud`** = **`ExtensionClientId`**. Dev fallback when secret empty (SPEC / `Program.cs`).
- **Desktop:** **`X-MGM-ApiKey`** = **`Mgm:ApiKey`**.

## Architecture stance

**DDD + CQRS + event sourcing**; **Marten / PostgreSQL** write model; **EF Core** optional read-side only — see [`docs/overview/ARCHITECTURE.md`](ARCHITECTURE.md).

## WoW targeting (MVP)

Desktop targets **foreground** **`WoW.exe`**; process picker is roadmap.

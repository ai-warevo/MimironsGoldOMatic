<!-- Updated: 2026-04-05 (Deduplication pass) -->

# MVP product summary (at a glance)

**Normative source:** [`docs/overview/SPEC.md`](SPEC.md) (APIs, transitions, persistence, log formats). This page is a **consolidated digest** for README-style overviews; it does **not** override the spec.

## Pool and chat

- **Subscribe** then enroll with **`!twgold <CharacterName>`** in **broadcast** Twitch chat (**prefix case-insensitive**). **EBS** ingests via **EventSub**; subscriber eligibility from the **chat payload** only (see SPEC). **`CharacterName`** **unique** in the active pool (same viewer may **replace** their name). Channel Points **not** used.
- Chat dedupe: Twitch **`message_id`**. Optional Extension **`POST /api/payouts/claim`**: **`EnrollmentRequestId`** idempotency; **`Mgm:DevSkipSubscriberCheck`** for local Dev Rig — see SPEC §5.

## Roulette

- **Visual roulette**; **fixed 5-minute** cadence; **no** early spin. **`nextSpinAt` / `serverNow`** from **`GET /api/roulette/state`** are **authoritative** for countdown UI.
- **Minimum participants:** **1**. **Non-winners** stay in the pool. **Winners** removed when payout is **`Sent`**; may re-enroll via chat.
- **Online check:** **`/who <Winner_InGame_Nickname>`** before **`Pending`** payout; addon **`[MGM_WHO]`** + JSON in **`WoWChatLog.txt`** → Desktop **`verify-candidate`**. Offline candidate → **no winner** this cycle (**no** re-draw same cycle).

## Gold and limits

- **1,000g** per **winning** payout (MVP). **10,000g** lifetime cap per **`TwitchUserId`**. **One active payout** (`Pending` / `InProgress`) per **`TwitchUserId`**.

## Winner path

- **Extension:** “You won” + instruct **WoW whisper reply `!twgold`** after the **notification whisper** (SPEC §9).
- **Addon:** sends **`/whisper …`** (§9 Russian text); on **`!twgold`** match → **`[MGM_ACCEPT:UUID]`**.
- **Desktop:** tail log → **`confirm-acceptance`** (not **`Sent`**).
- **Mail:** MGM-armed send → **`[MGM_CONFIRM:UUID]`** + completion whisper → Desktop → **`PATCH` `Sent`**. **Helix** §11 broadcast line after **`Sent`** (best-effort). Manual **Mark as Sent** = operator override.

## Payout statuses (winner row)

`Pending` → `InProgress` → `Sent` | `Failed` | `Cancelled`; `Expired` after 24h (hourly job). **`InProgress` → `Pending`** escape hatch allowed (SPEC §3).

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

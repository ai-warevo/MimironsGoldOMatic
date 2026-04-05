<!-- Updated: 2026-04-05 -->

# Context

## High-Level Purpose
Mimiron's Gold-o-Matic is an end-to-end system for distributing gold in WoW 3.3.5a.

## Implementation status (short)

**MVP-1 … MVP-5** are implemented in **`src/`** (Shared, Backend, Desktop, Twitch Extension, WoW addon). **MVP-6** (test suite / E2E automation) and production packaging are still open. See **`docs/IMPLEMENTATION_READINESS.md`** for the matrix and residual risks.

## System Architecture
`Twitch Extension -> EBS (MimironsGoldOMatic.Backend / ASP.NET Core) -> WPF App (WinAPI/PostMessage) -> WoW 3.3.5a Addon (Lua)`

**EBS:** **`MimironsGoldOMatic.Backend`** is the **Extension Backend Service** — Extension JWT, **EventSub** chat enrollment, **Helix** (e.g. §11 reward-sent line), and REST for Desktop. See **`docs/SPEC.md`** (EBS section).

## MVP Specification (final)

- **Participant pool**: the viewer **must subscribe**, then enroll by **`!twgold <CharacterName>`** in **broadcast Twitch chat** (ingested by the **EBS** via **Twitch EventSub**; **`!twgold`** prefix **case-insensitive**; subscriber flag from EventSub payload only per **`docs/SPEC.md`**). **`CharacterName`** must be **unique** among active pool entries. Channel Points are **not** part of MVP.
- **Roulette**: **visual roulette**; **every 5 minutes** selects **one winner** (no early/off-schedule spins). **Spin schedule** is **server-authoritative**; Extension **countdown** uses **`GET /api/roulette/state`** (`docs/SPEC.md` §5.1). **Non-winners stay in the pool.** **Winners are removed when payout is `Sent`**; they may **re-enter** via **`!twgold <CharacterName>`** in chat. Minimum pool size **1**. Each finalized winner **must** be **online-verified** via **`/who <Winner_InGame_Nickname>`** before **`Pending` payout** / notification; the addon emits **`[MGM_WHO]`** + JSON into **`WoWChatLog.txt`** (no file-bridge; `docs/SPEC.md` §8).
- **Winner notification**: Extension **“You won”** plus **in-game** flow: addon sends **`/whisper <Winner_Name> …`** (Russian text, `docs/SPEC.md` §9); winner replies with **`!twgold`** in WoW (**case-insensitive**). Character **existence/online** at win time is verified with **in-game `/who`** (no external realm API in MVP).
- **Gold per winning payout**: fixed **1,000g** (when a spin produces a payable winner).
- **Anti-abuse**:
  - **Lifetime cap**: max **10,000g total** per Twitch user.
  - **Concurrency**: **one active payout per Twitch user** at a time (when a payout row exists for that user).
  - **Rate limiting**: standard ASP.NET Core rate limiting (e.g. ~5 req/min per IP/user).
- **Idempotency / uniqueness**: chat message dedupe + **unique character name** in pool; optional **`EnrollmentRequestId`** for Extension **`POST /api/payouts/claim`**.
- **Identity fields**:
  - `TwitchUserId` (logic, limits, concurrency)
  - `TwitchDisplayName` (WPF UX)
- **Payout lifecycle statuses** (for the **current winner’s** payout): `Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired`.
- **Expiration**: **EBS** hourly job expires `Pending`/`InProgress` older than 24h; no reactivation.
- **Security (MVP)**:
  - **Twitch Extension JWT:** **HS256** validation using **`Twitch:ExtensionSecret`** (base64); JWT **`aud`** checked when **`Twitch:ExtensionClientId`** is set. **Development** may run with empty secret + dev-derived key (`Program.cs`). **Issuer** / JWKS rotation: roadmap (`docs/ROADMAP.md`).
  - Desktop-to-**EBS** uses pre-shared **`Mgm:ApiKey`** (**`X-MGM-ApiKey`** header).
- **WoW targeting (MVP)**: Desktop targets the **foreground** `WoW.exe` process; process picker is roadmap.
- **Confirmation**:
  - **Acceptance (willing to receive gold)**: After the **winner notification whisper**, the winner whispers **`!twgold`** in WoW → addon prints **`[MGM_ACCEPT:UUID]`** → **`WoWChatLog.txt`** → Desktop → **EBS** **`confirm-acceptance`** (not **`Sent`**).
  - **Mail sent (required for `Sent`)**: **`MAIL_SEND_SUCCESS`** on **MGM-armed** send → addon prints **`[MGM_CONFIRM:UUID]`** in **`Logs\WoWChatLog.txt`** and whispers the winner **`Награда отправлена тебе на почту, проверяй ящик!`**; Desktop **must** parse **`[MGM_CONFIRM:UUID]`** → **`Sent`**. **Twitch broadcast chat:** the **EBS** **must** **attempt** the §11 reward-sent line via **Helix** after **`Sent`** (inline retry; best-effort if Helix fails — `docs/SPEC.md` §9–11).
  - **Fallback**: Desktop manual **Mark as Sent** if policy allows.

## Primary Data Flow (conceptual)
1. **Subscribers** type **`!twgold <CharacterName>`** in **broadcast chat**; the **EBS** ingests via **EventSub** and **adds** the viewer to the **participant pool** (unique name). Extension shows **roulette / pool** UX (polls the **EBS**).
2. On each spin (scheduled **5 minutes** only), the system picks a candidate and **verifies online** with **`/who <Winner_InGame_Nickname>`**; when valid, the **EBS** creates **payout state**; **addon** sends **winner notification whisper** (§9); winner replies **`!twgold`** in WoW before mail.
3. The WPF app syncs **winner** payouts into WoW 3.3.5a Lua instructions, then focuses/communicates with the running game process using WinAPI/PostMessage.
4. The WoW addon receives payload data via hooked mail UI events and fills mail recipient/subject/money fields from a queued instruction string.
5. The **winner** confirms with **WoW whisper `!twgold`** → **EBS** records **acceptance**; the streamer sends mail; the addon emits **`[MGM_CONFIRM:UUID]`**; Desktop reads **`WoWChatLog.txt`** → **EBS** marks **`Sent`** → **Helix** §11 line (best-effort) → **winner removed from pool** (may re-enroll via Twitch chat).

## Data & Artifacts
- Shared contracts (DTOs/enums) live in `MimironsGoldOMatic.Shared` so all modules agree on the payout payload.
- WoW addon payload format must remain compatible with the WPF chunking strategy and 3.3.5a Lua/FrameXML constraints.
- **UI/UX artifact:** `docs/UI_SPEC.md` describes every MVP screen (**UI-1xx–4xx**), element IDs, and navigation flow for Twitch Extension, WPF Desktop, and WoW addon.
- Repo engineering workflow artifacts live under `docs/prompts/` (templates + logged task history).

## Test Topology
- **.NET:** `dotnet test src/MimironsGoldOMatic.slnx` (add test projects under MVP-6; today the solution builds application projects).
- Frontend/backend integration checks should validate:
  - API contract compatibility with shared DTOs.
  - Payload chunking boundaries and WoW-injection command sizing.

## Key Relationships (GPS Map)
- Twitch Extension -> **EBS** -> Desktop client contract:
  - **EBS** returns a payload structure understood by the Desktop app.
- Desktop client -> WoW addon:
  - Desktop converts payouts to an instruction string that the addon’s mail hook consumes.
- **EBS** -> Persistence + Status:
  - **EBS** owns payout lifecycle state, while Desktop requests “sync to WoW” based on pending payouts.

## Compatibility Focus (3.3.5a)
- Addon UI hooks must target the correct mail frame and event names for 3.3.5a.
- WPF WinAPI logic must be documented with timing and focus behavior so it remains reliable on legacy clients.


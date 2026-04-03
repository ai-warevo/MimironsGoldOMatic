# Context

## High-Level Purpose
Mimiron's Gold-o-Matic is an end-to-end system for distributing gold in WoW 3.3.5a.

## System Architecture
`Twitch Extension -> ASP.NET Core API -> WPF App (WinAPI/PostMessage) -> WoW 3.3.5a Addon (Lua)`

## MVP Specification (final)

- **Participant pool**: redeeming adds the viewer to a **pool**; **no instant payout**.
- **Roulette**: **visual roulette**; default **every 5 minutes** selects **one winner**. **Non-winners stay in the pool.** Minimum pool size **1**. Each finalized winner **must** be **online-verified** via **`/who <Winner_InGame_Nickname>`** before **`Pending` payout** / notification.
- **Winner notification**: winners **must** be notified (e.g. Extension **“You won”**) and told to **whisper `!twgold`** privately **to get the gold mail**.
- **Instant spin**: Channel Points reward **“Switch to instant spin”** skips the wait until the next scheduled spin.
- **Gold per winning payout**: fixed **1,000g** (when a spin produces a payable winner).
- **Anti-abuse**:
  - **Lifetime cap**: max **10,000g total** per Twitch user.
  - **Concurrency**: **one active payout per Twitch user** at a time (when a payout row exists for that user).
  - **Rate limiting**: standard ASP.NET Core rate limiting (e.g. ~5 req/min per IP/user).
- **Idempotency**: `TwitchTransactionId` (Twitch redemption unique id) is persisted and enforced unique in the database for redemptions/enrollment.
- **Identity fields**:
  - `TwitchUserId` (logic, limits, concurrency)
  - `TwitchDisplayName` (WPF UX)
- **Payout lifecycle statuses** (for the **current winner’s** payout): `Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired`.
- **Expiration**: Backend hourly job expires `Pending`/`InProgress` older than 24h; no reactivation.
- **Security (MVP)**:
  - Twitch Dev Rig focus; production-grade Twitch JWT validation is a roadmap milestone.
  - Desktop-to-Backend uses a pre-shared `ApiKey` (locally trusted Desktop app).
- **WoW targeting (MVP)**: Desktop targets the **foreground** `WoW.exe` process; process picker is roadmap.
- **Confirmation**:
  - **Acceptance (willing to receive gold)**: After **winner notification**, the winner **replies** with a **private message** exactly **`!twgold`** (**required** to receive the gold mail); the **addon intercepts** it and notifies the **Desktop utility**, which calls the **Backend** to record acceptance (not **`Sent`**).
  - **Mail sent (required for `Sent`)**: **`[MGM_CONFIRM:UUID]`** in **`Logs\WoWChatLog.txt`**; Desktop **must** parse it and then set **`Sent`** on the **Backend** (see `docs/SPEC.md`).
  - **Fallback**: Desktop manual **Mark as Sent** if policy allows.

## Primary Data Flow (conceptual)
1. Twitch Extension collects a player/character input and submits a redemption to the ASP.NET Core API; the viewer is **added to the participant pool** (and Extension shows **roulette / pool** UX).
2. On each spin (scheduled **5 minutes** or **instant spin** reward), the system picks a candidate and **verifies online** with **`/who <Winner_InGame_Nickname>`**; when valid, the Backend creates **payout state** and the winner is **notified** to whisper **`!twgold`** for the mail.
3. The WPF app syncs **winner** payouts into WoW 3.3.5a Lua instructions, then focuses/communicates with the running game process using WinAPI/PostMessage.
4. The WoW addon receives payload data via hooked mail UI events and fills mail recipient/subject/money fields from a queued instruction string.
5. The **winner** (after notification) **replies** with whisper **`!twgold`** → **server** records **acceptance**; the streamer sends mail; the addon emits **`[MGM_CONFIRM:UUID]`**; Desktop reads **`WoWChatLog.txt`** → **server marks `Sent`**.

## Data & Artifacts
- Shared contracts (DTOs/enums) live in `MimironsGoldOMatic.Shared` so all modules agree on the payout payload.
- WoW addon payload format must remain compatible with the WPF chunking strategy and 3.3.5a Lua/FrameXML constraints.
- **UI/UX artifact:** `docs/UI_SPEC.md` describes every MVP screen (**UI-1xx–4xx**), element IDs, and navigation flow for Twitch Extension, WPF Desktop, and WoW addon.
- Repo engineering workflow artifacts live under `docs/prompts/` (templates + logged task history).

## Test Topology (when solution exists)
- Primary verification: `dotnet test src/MimironsGoldOMatic.sln`
- Frontend/backend integration checks should validate:
  - API contract compatibility with shared DTOs.
  - Payload chunking boundaries and WoW-injection command sizing.

## Key Relationships (GPS Map)
- Twitch Extension -> Backend API -> Desktop client contract:
  - Backend returns a payload structure understood by the Desktop app.
- Desktop client -> WoW addon:
  - Desktop converts payouts to an instruction string that the addon’s mail hook consumes.
- Backend -> Persistence + Status:
  - Backend owns payout lifecycle state, while Desktop requests “sync to WoW” based on pending payouts.

## Compatibility Focus (3.3.5a)
- Addon UI hooks must target the correct mail frame and event names for 3.3.5a.
- WPF WinAPI logic must be documented with timing and focus behavior so it remains reliable on legacy clients.


# Context

## High-Level Purpose
Mimiron's Gold-o-Matic is an end-to-end system for distributing gold in WoW 3.3.5a.

## System Architecture
`Twitch Extension -> ASP.NET Core API -> WPF App (WinAPI/PostMessage) -> WoW 3.3.5a Addon (Lua)`

## MVP Specification (final)

- **Gold per redemption**: fixed **1,000g**.
- **Anti-abuse**:
  - **Lifetime cap**: max **10,000g total** per Twitch user.
  - **Concurrency**: **one active payout per Twitch user** at a time.
  - **Rate limiting**: standard ASP.NET Core rate limiting (e.g. ~5 req/min per IP/user).
- **Idempotency**: `TwitchTransactionId` (Twitch redemption unique id) is persisted and enforced unique in the database.
- **Identity fields**:
  - `TwitchUserId` (logic, limits, concurrency)
  - `TwitchDisplayName` (WPF UX)
- **Payout lifecycle statuses**: `Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired`.
- **Expiration**: Backend hourly job expires `Pending`/`InProgress` older than 24h; no reactivation.
- **Security (MVP)**:
  - Twitch Dev Rig focus; production-grade Twitch JWT validation is a roadmap milestone.
  - Desktop-to-Backend uses a pre-shared `ApiKey` (locally trusted Desktop app).
- **WoW targeting (MVP)**: Desktop targets the **foreground** `WoW.exe` process; process picker is roadmap.
- **Confirmation**:
  - Primary: Desktop monitors `Logs\WoWChatLog.txt` for `[MGM_CONFIRM:UUID]`.
  - Fallback: Desktop provides a manual **Mark as Sent** button.

## Primary Data Flow (conceptual)
1. Twitch Extension collects a player/character input and submits a claim to the ASP.NET Core API.
2. The API validates the claim, persists payout state, and returns a payout payload compatible with the WPF app.
3. The WPF app converts payouts into WoW 3.3.5a Lua instructions, then focuses/communicates with the running game process using WinAPI/PostMessage.
4. The WoW addon receives payload data via hooked mail UI events and fills mail recipient/subject/money fields from a queued instruction string.

## Data & Artifacts
- Shared contracts (DTOs/enums) live in `MimironsGoldOMatic.Shared` so all modules agree on the payout payload.
- WoW addon payload format must remain compatible with the WPF chunking strategy and 3.3.5a Lua/FrameXML constraints.
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


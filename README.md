<!-- Updated: 2026-04-05 -->

# Mimiron's Gold-o-Matic

Mimiron's Gold-o-Matic is a Twitch-to-World-of-Warcraft (WoW) ecosystem built for the 3.3.5a gold distribution workflow.
It connects a Twitch Extension UI to an ASP.NET Core API, a local WPF desktop app, and finally a WoW 3.3.5a addon (Lua) that delivers the gold via the in-game mail interface.

## High-level Architecture

`Twitch Extension -> ASP.NET Core API -> WPF App (WinAPI/PostMessage) -> WoW 3.3.5a Addon (Lua)`

**MVP deployment:** **one** broadcaster channel per Backend instance (see `docs/SPEC.md` deployment scope).

## Implementation status

Normative contracts live in **`docs/SPEC.md`**, **`docs/ROADMAP.md`**, **`docs/UI_SPEC.md`**, and **`docs/INTERACTION_SCENARIOS.md`**. **MVP-1 … MVP-5** code exists under **`src/`** (Shared, Backend with Marten + EventSub + Helix hooks, WPF Desktop, Vite/React Extension, WoW addon). **MVP-6** (automated API/integration tests, packaged release story) is not done. Details: **`docs/IMPLEMENTATION_READINESS.md`**.

## MVP Specification (final)

- **Gold per winning payout**: fixed **1,000g** (MVP). **Subscribers** join the giveaway by typing **`!twgold <CharacterName>`** in **broadcast Twitch chat** (**`!twgold`** prefix **case-insensitive**; server nickname for the roulette); the Backend **monitors chat** (e.g. EventSub). **Character names** in the pool must be **unique**. Channel Points are **not** used.
- **Roulette**: a **visual roulette** runs on a **5-minute** cadence and selects **one winner** per spin. The **next spin time** is **server-authoritative** (`GET /api/roulette/state`); the Extension **shows a countdown** from that schedule. **Non-winners stay in the pool.** **Winners are removed from the pool when their payout is `Sent`** (after mail is confirmed); they may **re-enter** with **`!twgold <CharacterName>`** in chat again. There is **no** early or off-schedule spin. Before a win counts, the system **must** verify the chosen player is **online** using WoW **`/who <Winner_InGame_Nickname>`**; if the candidate is **offline**, **no `Pending` payout** is created for that cycle (**no** second draw in the same 5-minute window — `docs/SPEC.md` §1, §5).
- **Winner notification**: the winning viewer is told they won (e.g. Twitch Extension). **Normative consent:** the **WoW addon** sends an in-game **`/whisper <Winner_Name> …`** to the winner with the exact Russian text in **`docs/SPEC.md` §9**; the winner **replies** in-game with **`!twgold`** (private message; **case-insensitive** match) to **consent**. The streamer sends gold mail after acceptance; **`Sent`** requires **`[MGM_CONFIRM:UUID]`** in **`WoWChatLog.txt`**.
- **Minimum participants**: **1** (a spin can run with a single entrant).
- **Anti-abuse**:
  - **Lifetime cap**: max **10,000g total** per Twitch user.
  - **Concurrency**: **one active payout per Twitch user** at a time (must be terminal before a new claim), applied when a payout exists for a spin winner.
  - **Rate limiting**: standard ASP.NET Core rate limiting (e.g. ~5 req/min per IP/user).
- **Idempotency / dedupe:** chat enrollments dedupe by Twitch **`message_id`**; optional Extension **`POST /api/payouts/claim`** may use **`EnrollmentRequestId`**; **unique `CharacterName`** in the active pool is enforced.
- **Statuses** (payout for the **selected winner**): `Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired` (24h).
- **Expiration**: Backend runs an hourly job to mark `Pending`/`InProgress` older than 24h as `Expired` (no reactivation).
- **Acceptance**: After the **in-game winner notification whisper** (`docs/SPEC.md` §9), the **winner** sends a **private in-game message** matching **`!twgold`** (**case-insensitive**); the **addon** prints **`[MGM_ACCEPT:UUID]`** to chat → **`WoWChatLog.txt`** → Desktop → **`confirm-acceptance`** on the **server** (not **`Sent`** yet). **They must confirm before the streamer sends gold mail.**
- **Mail sent → `Sent`**: After an **MGM-armed** in-game mail succeeds (**`MAIL_SEND_SUCCESS`**), the addon **must** print **`[MGM_CONFIRM:UUID]`** to **`Logs\WoWChatLog.txt`** and whisper the winner **`Награда отправлена тебе на почту, проверяй ящик!`**. **Manual** mail sends **without** that arm **must not** emit the tag. The Desktop utility **must** parse **`[MGM_CONFIRM:UUID]`** and set **`Sent`**. Viewers may see **`Награда отправлена персонажу <WINNER_NAME> на почту, проверяй ящик!`** in **Twitch chat** when **`Sent`** applies (`docs/SPEC.md` §11). Manual **Mark as Sent** remains an operator override if needed.
- **Confirmation semantics**: **`Sent`** means **mail-send confirmation** via **`[MGM_CONFIRM:UUID]`** in **`WoWChatLog.txt`** (required for automation). WoW whisper **`!twgold`** (consent reply) is **willingness to accept**, not proof that mail was sent.
- **Security (MVP)**:
  - Extension **Bearer** JWT: validated with **`Twitch:ExtensionSecret`** (HS256) and optional **`aud`** = **`Twitch:ExtensionClientId`**; **Development** can use a dev-derived key if the secret is empty (`Program.cs`).
  - Desktop-to-Backend uses pre-shared **`Mgm:ApiKey`** (header **`X-MGM-ApiKey`**). **Issuer** validation and full production hardening: see **`docs/ROADMAP.md`** (Production milestone).
- **Architecture (MVP)**:
  - DDD + CQRS + Event Sourcing are mandatory.
  - Marten/PostgreSQL Event Store is write-side source of truth.
  - EF Core (if used) is restricted to read-model projections.

## What you get

- A shared contract library for payout requests/responses (so all modules agree on the same data model).
- A backend API that validates claims and persists payout state.
- A WPF desktop client that prepares and injects WoW-compatible mail instructions.
- A WoW 3.3.5a addon that hooks mail UI events and fills mail fields from a queued payout payload.

## Repo layout (expanded stack)

- `/src` contains:
  - `.NET 10` solution/projects (Shared DTOs, ASP.NET Core API, WPF desktop client)
  - `Vite + React + TypeScript` Twitch Extension scaffold
  - `WoW 3.3.5a` addon scaffold (`.toc` + Lua)
- `/.github/workflows` — placeholder only until CI workflows are committed.
- `/.cursor` contains project-specific AI rules to keep naming and compatibility consistent.

## Setup

- **[SETUP.md](SETUP.md)** — index to the setup guides below
- **[SETUP-for-developer.md](SETUP-for-developer.md)** — prerequisites, PostgreSQL, Backend `appsettings` (including Twitch keys), running projects from source
- **[SETUP-for-streamer.md](SETUP-for-streamer.md)** — installing the WoW addon, Twitch Extension, and Desktop app; operator notes

## Documentation

- **Architecture & repo layout (docs entrypoint):** [`docs/ReadME.md`](docs/ReadME.md)
- **Technical specification (canonical contracts):** [`docs/SPEC.md`](docs/SPEC.md)
- **Roadmap:** [`docs/ROADMAP.md`](docs/ROADMAP.md)
- **Interaction scenarios & test cases:** [`docs/INTERACTION_SCENARIOS.md`](docs/INTERACTION_SCENARIOS.md)
- **UI/UX specification (screens, states, ASCII mocks):** [`docs/UI_SPEC.md`](docs/UI_SPEC.md)
- **Component READMEs:**
  - [`docs/MimironsGoldOMatic.Shared/ReadME.md`](docs/MimironsGoldOMatic.Shared/ReadME.md)
  - [`docs/MimironsGoldOMatic.Backend/ReadME.md`](docs/MimironsGoldOMatic.Backend/ReadME.md)
  - [`docs/MimironsGoldOMatic.Desktop/ReadME.md`](docs/MimironsGoldOMatic.Desktop/ReadME.md)
  - [`docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`](docs/MimironsGoldOMatic.TwitchExtension/ReadME.md)
  - [`docs/MimironsGoldOMatic.WoWAddon/ReadME.md`](docs/MimironsGoldOMatic.WoWAddon/ReadME.md)
- **Cursor / agent workflow docs:**
  - **Prompt history:** [`docs/prompts/history/`](docs/prompts/history/)
  - **Prompt templates:** [`docs/prompts/templates/`](docs/prompts/templates/)
  - **Bootstrap prompts:** now embedded in [`docs/ROADMAP.md`](docs/ROADMAP.md)

## Notes

WoW 3.3.5a compatibility relies on:

- specific addon frame names / mail hooks
- careful WinAPI focus and message timing from the WPF app
- chunking constraints for command injection into `/run`


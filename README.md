# Mimiron's Gold-o-Matic

Mimiron's Gold-o-Matic is a Twitch-to-World-of-Warcraft (WoW) ecosystem built for the 3.3.5a gold distribution workflow.
It connects a Twitch Extension UI to an ASP.NET Core API, a local WPF desktop app, and finally a WoW 3.3.5a addon (Lua) that delivers the gold via the in-game mail interface.

## High-level Architecture

`Twitch Extension -> ASP.NET Core API -> WPF App (WinAPI/PostMessage) -> WoW 3.3.5a Addon (Lua)`

## MVP Specification (final)

- **Gold per redemption**: fixed **1,000g** (MVP).
- **Anti-abuse**:
  - **Lifetime cap**: max **10,000g total** per Twitch user.
  - **Concurrency**: **one active payout per Twitch user** at a time (must be terminal before a new claim).
  - **Rate limiting**: standard ASP.NET Core rate limiting (e.g. ~5 req/min per IP/user).
- **Idempotency**: each Twitch redemption includes `TwitchTransactionId`; Backend enforces a unique constraint so **one redemption = one payout**.
- **Statuses**: `Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired` (24h).
- **Expiration**: Backend runs an hourly job to mark `Pending`/`InProgress` older than 24h as `Expired` (no reactivation).
- **Confirmation**: Desktop watches `Logs\WoWChatLog.txt` for `[MGM_CONFIRM:UUID]` and also provides a manual **Mark as Sent** override.
- **Confirmation semantics**: `Sent` is tied to actual mail send confirmation (not prepare-only).
- **Security (MVP)**:
  - Focus on Twitch Dev Rig debugging first; production-grade Twitch JWT verification is a roadmap milestone.
  - Desktop-to-Backend uses a pre-shared `ApiKey` (locally trusted Desktop app).
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
- `/.github/workflows` will contain CI build/test pipelines for both .NET and the React frontend.
- `/.cursor` contains project-specific AI rules to keep naming and compatibility consistent.

## Documentation

- **Architecture & repo layout (docs entrypoint):** [`docs/ReadME.md`](docs/ReadME.md)
- **Technical specification (canonical contracts):** [`docs/SPEC.md`](docs/SPEC.md)
- **Roadmap:** [`docs/ROADMAP.md`](docs/ROADMAP.md)
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


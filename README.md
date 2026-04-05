<!-- Updated: 2026-04-05 (Deduplication pass) -->

# Mimiron's Gold-o-Matic

Mimiron's Gold-o-Matic is a Twitch-to-World-of-Warcraft (WoW) ecosystem built for the 3.3.5a gold distribution workflow.
It connects a Twitch Extension UI to an ASP.NET Core API, a local WPF desktop app, and finally a WoW 3.3.5a addon (Lua) that delivers the gold via the in-game mail interface.

<!-- Content moved to ARCHITECTURE.md. See: docs/ARCHITECTURE.md -->

## Implementation status

Normative contracts live in **`docs/SPEC.md`**, **`docs/ROADMAP.md`**, **`docs/UI_SPEC.md`** (UI hub; per-surface screens under **`docs/MimironsGoldOMatic.*/UI_SPEC.md`**), and **`docs/INTERACTION_SCENARIOS.md`**. **MVP-1 … MVP-5** code exists under **`src/`** (Shared, Backend with Marten + EventSub + Helix hooks, WPF Desktop, Vite/React Extension, WoW addon). **MVP-6** (automated API/integration tests, packaged release story) is not done. Details: **`docs/IMPLEMENTATION_READINESS.md`**.

<!-- Content moved to MVP_PRODUCT_SUMMARY.md. See: docs/MVP_PRODUCT_SUMMARY.md -->

## What you get

- A shared contract library for payout requests/responses (so all modules agree on the same data model).
- A backend API that validates claims and persists payout state.
- A WPF desktop client that prepares and injects WoW-compatible mail instructions.
- A WoW 3.3.5a addon that hooks mail UI events and fills mail fields from a queued payout payload.

<!-- Content moved to PROJECT_STRUCTURE.md. See: docs/PROJECT_STRUCTURE.md -->

## Setup

- **[docs/SETUP.md](docs/SETUP.md)** — overview, shared prerequisites, and first **dotnet** / **npm** build
- **[docs/SETUP-for-developer.md](docs/SETUP-for-developer.md)** — PostgreSQL, Backend `appsettings` (including Twitch keys), running projects from source
- **[docs/SETUP-for-streamer.md](docs/SETUP-for-streamer.md)** — WoW addon, Twitch Extension, Desktop; operator notes

## Documentation

**Hub (deduplicated overviews):**

- **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)** — pipeline, EBS, patterns, component relationships
- **[docs/PROJECT_STRUCTURE.md](docs/PROJECT_STRUCTURE.md)** — repository layout, naming
- **[docs/WORKFLOWS.md](docs/WORKFLOWS.md)** — end-to-end MVP flow
- **[docs/MVP_PRODUCT_SUMMARY.md](docs/MVP_PRODUCT_SUMMARY.md)** — product rules at a glance
- **[docs/GLOSSARY.md](docs/GLOSSARY.md)** — terms → spec sections
- **[docs/SETUP.md](docs/SETUP.md)** — setup overview, prerequisites, first build (role-specific guides linked from there)

**Specifications and tracking:**

- **Architecture & engineering index:** [`docs/ReadME.md`](docs/ReadME.md)
- **Technical specification (canonical contracts):** [`docs/SPEC.md`](docs/SPEC.md)
- **Roadmap:** [`docs/ROADMAP.md`](docs/ROADMAP.md)
- **Interaction scenarios & test cases:** [`docs/INTERACTION_SCENARIOS.md`](docs/INTERACTION_SCENARIOS.md)
- **UI/UX specification:** [`docs/UI_SPEC.md`](docs/UI_SPEC.md) (hub) · [Twitch Extension screens](docs/MimironsGoldOMatic.TwitchExtension/UI_SPEC.md) · [Desktop screens](docs/MimironsGoldOMatic.Desktop/UI_SPEC.md) · [WoW addon screens](docs/MimironsGoldOMatic.WoWAddon/UI_SPEC.md)
- **Context (short pointer doc):** [`CONTEXT.md`](CONTEXT.md)

**Component READMEs:**

- [`docs/MimironsGoldOMatic.Shared/ReadME.md`](docs/MimironsGoldOMatic.Shared/ReadME.md)
- [`docs/MimironsGoldOMatic.Backend/ReadME.md`](docs/MimironsGoldOMatic.Backend/ReadME.md)
- [`docs/MimironsGoldOMatic.Desktop/ReadME.md`](docs/MimironsGoldOMatic.Desktop/ReadME.md)
- [`docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`](docs/MimironsGoldOMatic.TwitchExtension/ReadME.md)
- [`docs/MimironsGoldOMatic.WoWAddon/ReadME.md`](docs/MimironsGoldOMatic.WoWAddon/ReadME.md)

**Cursor / agent workflow:**

- **[AGENTS.md](AGENTS.md)** — roles, workflow, testing guidance
- **Prompt history:** [`docs/prompts/history/`](docs/prompts/history/)
- **Prompt templates:** [`docs/prompts/templates/`](docs/prompts/templates/)
- **Bootstrap prompts:** embedded in [`docs/ROADMAP.md`](docs/ROADMAP.md)

## Notes

<!-- Content moved to ARCHITECTURE.md (Compatibility). See: docs/ARCHITECTURE.md -->

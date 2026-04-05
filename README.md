<!-- Updated: 2026-04-05 (Deduplication pass) -->

# Mimiron's Gold-o-Matic

Mimiron's Gold-o-Matic is a Twitch-to-World-of-Warcraft (WoW) ecosystem built for the 3.3.5a gold distribution workflow.
It connects a Twitch Extension UI to an ASP.NET Core API, a local WPF desktop app, and finally a WoW 3.3.5a addon (Lua) that delivers the gold via the in-game mail interface.

<!-- Content moved to ARCHITECTURE.md. See: docs/overview/ARCHITECTURE.md -->

## Implementation status

Normative contracts live in **`docs/overview/SPEC.md`**, **`docs/overview/ROADMAP.md`**, **`docs/reference/UI_SPEC.md`** (UI hub; per-surface screens under **`docs/components/*/UI_SPEC.md`**), and **`docs/overview/INTERACTION_SCENARIOS.md`**. **MVP-1 … MVP-5** code exists under **`src/`** (Shared, Backend with Marten + EventSub + Helix hooks, WPF Desktop, Vite/React Extension, WoW addon). **MVP-6** (automated API/integration tests, packaged release story) is not done. Details: **`docs/reference/IMPLEMENTATION_READINESS.md`**.

<!-- Content moved to MVP_PRODUCT_SUMMARY.md. See: docs/overview/MVP_PRODUCT_SUMMARY.md -->

## What you get

- A shared contract library for payout requests/responses (so all modules agree on the same data model).
- A backend API that validates claims and persists payout state.
- A WPF desktop client that prepares and injects WoW-compatible mail instructions.
- A WoW 3.3.5a addon that hooks mail UI events and fills mail fields from a queued payout payload.

<!-- Content moved to PROJECT_STRUCTURE.md. See: docs/reference/PROJECT_STRUCTURE.md -->

## Setup

- **[docs/setup/SETUP.md](docs/setup/SETUP.md)** — overview, shared prerequisites, and first **dotnet** / **npm** build
- **[docs/setup/SETUP-for-developer.md](docs/setup/SETUP-for-developer.md)** — PostgreSQL, Backend `appsettings` (including Twitch keys), running projects from source
- **[docs/setup/SETUP-for-streamer.md](docs/setup/SETUP-for-streamer.md)** — WoW addon, Twitch Extension, Desktop; operator notes

## Documentation

**Hub (deduplicated overviews):**

- **[docs/overview/ARCHITECTURE.md](docs/overview/ARCHITECTURE.md)** — pipeline, EBS, patterns, component relationships
- **[docs/reference/PROJECT_STRUCTURE.md](docs/reference/PROJECT_STRUCTURE.md)** — repository layout, naming
- **[docs/reference/WORKFLOWS.md](docs/reference/WORKFLOWS.md)** — end-to-end MVP flow
- **[docs/overview/MVP_PRODUCT_SUMMARY.md](docs/overview/MVP_PRODUCT_SUMMARY.md)** — product rules at a glance
- **[docs/reference/GLOSSARY.md](docs/reference/GLOSSARY.md)** — terms → spec sections
- **[docs/setup/SETUP.md](docs/setup/SETUP.md)** — setup overview, prerequisites, first build (role-specific guides linked from there)

**Specifications and tracking:**

- **Architecture & engineering index:** [`docs/ReadME.md`](docs/ReadME.md)
- **Technical specification (canonical contracts):** [`docs/overview/SPEC.md`](docs/overview/SPEC.md)
- **Roadmap:** [`docs/overview/ROADMAP.md`](docs/overview/ROADMAP.md)
- **Interaction scenarios & test cases:** [`docs/overview/INTERACTION_SCENARIOS.md`](docs/overview/INTERACTION_SCENARIOS.md)
- **UI/UX specification:** [`docs/reference/UI_SPEC.md`](docs/reference/UI_SPEC.md) (hub) · [Twitch Extension screens](docs/components/twitch-extension/UI_SPEC.md) · [Desktop screens](docs/components/desktop/UI_SPEC.md) · [WoW addon screens](docs/components/wow-addon/UI_SPEC.md)
- **Context (short pointer doc):** [`CONTEXT.md`](CONTEXT.md)

**Component READMEs:**

- [`docs/components/shared/ReadME.md`](docs/components/shared/ReadME.md)
- [`docs/components/backend/ReadME.md`](docs/components/backend/ReadME.md)
- [`docs/components/desktop/ReadME.md`](docs/components/desktop/ReadME.md)
- [`docs/components/twitch-extension/ReadME.md`](docs/components/twitch-extension/ReadME.md)
- [`docs/components/wow-addon/ReadME.md`](docs/components/wow-addon/ReadME.md)

**Cursor / agent workflow:**

- **[AGENTS.md](AGENTS.md)** — roles, workflow, testing guidance
- **Prompt history:** [`docs/prompts/history/`](docs/prompts/history/)
- **Prompt templates:** [`docs/prompts/templates/`](docs/prompts/templates/)
- **Bootstrap prompts:** embedded in [`docs/overview/ROADMAP.md`](docs/overview/ROADMAP.md)

## Notes

<!-- Content moved to ARCHITECTURE.md (Compatibility). See: docs/overview/ARCHITECTURE.md -->

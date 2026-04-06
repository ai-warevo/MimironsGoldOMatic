<!-- Updated: 2026-04-06 (Post‑Tier B execution) -->

# MimironsGoldOMatic Documentation

## Navigation

- **Project overview**: [Architecture](overview/ARCHITECTURE.md), [Roadmap](overview/ROADMAP.md), [MVP summary](overview/MVP_PRODUCT_SUMMARY.md), [Interaction scenarios](overview/INTERACTION_SCENARIOS.md), [Specification](overview/SPEC.md)
- **E2E automation**: [Plan](e2e/E2E_AUTOMATION_PLAN.md), [Tasks](e2e/E2E_AUTOMATION_TASKS.md), [Tier B implementation](e2e/TIER_B_IMPLEMENTATION_TASKS.md), [Tier B closure report](e2e/TIER_B_CLOSURE_REPORT.md), [Tier B handover](e2e/TIER_B_HANDOVER.md), [Tier B maintenance checklist](e2e/TIER_B_MAINTENANCE_CHECKLIST.md), [Knowledge transfer](e2e/TIER_B_KNOWLEDGE_TRANSFER.md), [Pre-launch checklist](e2e/TIER_B_PRELAUNCH_CHECKLIST.md), [Post-launch verification](e2e/TIER_B_POSTLAUNCH_VERIFICATION.md), [Tier C requirements (draft)](e2e/TIER_C_REQUIREMENTS.md), [Tier C implementation tasks](e2e/TIER_C_IMPLEMENTATION_TASKS.md), [Tier C kick-off plan](e2e/TIER_C_KICKOFF_PLAN.md), [Team announcement template](e2e/TIER_B_TEAM_ANNOUNCEMENT.md)
- **Components**:
  - [Backend](components/backend/ReadME.md)
  - [Desktop app](components/desktop/ReadME.md), [Desktop UI spec](components/desktop/UI_SPEC.md)
  - [Twitch Extension](components/twitch-extension/ReadME.md), [Extension UI spec](components/twitch-extension/UI_SPEC.md)
  - [WoW addon](components/wow-addon/ReadME.md), [Addon UI spec](components/wow-addon/UI_SPEC.md)
  - [Shared](components/shared/ReadME.md)
- **Setup**: [Main setup](setup/SETUP.md), [For developers](setup/SETUP-for-developer.md), [For streamers](setup/SETUP-for-streamer.md)
- **Reference**: [Glossary](reference/GLOSSARY.md), [Project structure](reference/PROJECT_STRUCTURE.md), [Workflows](reference/WORKFLOWS.md), [Implementation readiness](reference/IMPLEMENTATION_READINESS.md), [UI spec (hub)](reference/UI_SPEC.md)

---

# General architectural requirements (all components) and repo layout

## Documentation vs code

Normative architecture and API behavior live in **`docs/overview/SPEC.md`** and **`docs/overview/ROADMAP.md`**. **`docs/reference/IMPLEMENTATION_READINESS.md`** tracks doc/spec consistency and **`src/`** parity (**MVP-1…5** in tree; **MVP-6** pending).

**Deduplicated overviews:** [`ARCHITECTURE.md`](overview/ARCHITECTURE.md) · [`PROJECT_STRUCTURE.md`](reference/PROJECT_STRUCTURE.md) · [`WORKFLOWS.md`](reference/WORKFLOWS.md) · [`MVP_PRODUCT_SUMMARY.md`](overview/MVP_PRODUCT_SUMMARY.md) · [`GLOSSARY.md`](reference/GLOSSARY.md) · [`SETUP.md`](setup/SETUP.md) (see also [`SETUP-for-developer.md`](setup/SETUP-for-developer.md), [`SETUP-for-streamer.md`](setup/SETUP-for-streamer.md))

<!-- Content moved to ARCHITECTURE.md (Architectural patterns + persistence). See: docs/overview/ARCHITECTURE.md -->

## MimironsGoldOMatic.Shared (.NET 10)

- **Role:** DTOs, enums, and validation consumed by **EBS** and **Desktop** (not MediatR handlers).
- **FluentValidation:** `PayoutDto` / `CreatePayoutRequest` validators; **`CharacterNameRules`** aligned with [`SPEC.md`](overview/SPEC.md) §4.
- **Records / primary constructors:** DTOs such as **`PayoutDto`**, **`CreatePayoutRequest`** live in this assembly.
- **EBS application layer:** **`HandlerResult<T>`** + **`ApiErrorDto`** in **`MimironsGoldOMatic.Backend`** only.

<!-- Content moved to WORKFLOWS.md. See: docs/reference/WORKFLOWS.md -->

<!-- Content moved to MVP_PRODUCT_SUMMARY.md. See: docs/overview/MVP_PRODUCT_SUMMARY.md -->

## Technical specification (canonical)

- [`SPEC.md`](overview/SPEC.md) — APIs, DTOs, transitions, persistence, payloads, log parsing
- [`UI_SPEC.md`](reference/UI_SPEC.md) — UI hub: design tokens, navigation, accessibility; links to [`components/twitch-extension/UI_SPEC.md`](components/twitch-extension/UI_SPEC.md), [`components/desktop/UI_SPEC.md`](components/desktop/UI_SPEC.md), [`components/wow-addon/UI_SPEC.md`](components/wow-addon/UI_SPEC.md)

## Core components (pointers)

<!-- Content moved to ARCHITECTURE.md (Runtime components + relationships). See: docs/overview/ARCHITECTURE.md -->

- **[Twitch Extension](components/twitch-extension/ReadME.md)** · **[EBS / Backend](components/backend/ReadME.md)** · **[Shared](components/shared/ReadME.md)** · **[Desktop](components/desktop/ReadME.md)** · **[WoW addon](components/wow-addon/ReadME.md)**

<!-- Content moved to PROJECT_STRUCTURE.md (tree + C# naming). See: docs/reference/PROJECT_STRUCTURE.md -->

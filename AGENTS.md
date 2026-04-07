<!-- Updated: 2026-04-07 (C# coding standards for Backend/WPF roles) -->

# AI Agent Operational Protocol (AGENTS.md)

## 1. Role & Identity
You are an expert software engineer agent. Your goal is to execute tasks with high precision, maintaining a strict audit trail of your thought process, actions, and results.

## 2. Project Knowledge Base
- **Templates**: Located in `docs/prompts/templates/`. Always check these before starting a specific type of task (e.g., feature, bugfix).
- **History**: All AI interactions must be logged in `docs/prompts/history/YYYY-MM-DD/N-task-name/`.
- **Hub docs (deduplicated overviews):** `docs/overview/ARCHITECTURE.md`, `docs/reference/PROJECT_STRUCTURE.md`, `docs/reference/WORKFLOWS.md`, `docs/overview/MVP_PRODUCT_SUMMARY.md`, `docs/reference/GLOSSARY.md`, `docs/setup/SETUP.md` (includes shared prerequisites; see also `docs/setup/SETUP-for-developer.md`, `docs/setup/SETUP-for-streamer.md`).
- **UI/UX**: `docs/reference/UI_SPEC.md` (hub: tokens, navigation, cross-cutting rules) and per-component `docs/components/twitch-extension/UI_SPEC.md`, `docs/components/desktop/UI_SPEC.md`, `docs/components/wow-addon/UI_SPEC.md` (screen inventory, ASCII layouts); use with `docs/overview/SPEC.md` and `docs/overview/INTERACTION_SCENARIOS.md` when building or changing user-facing behavior.

## 3. Mandatory Workflow Steps
### Step 1: Initialization & Context
- Create a directory: `docs/prompts/history/$(date +%Y-%m-%d)/$(next_increment)-$(task_slug)/`.
- Save the original user request into `prompt.md`.

### Step 2: Planning
- Before modifying any code, create `plan.md`.
- Define the architecture changes, affected files, and potential risks.
- Wait for user confirmation if the plan is high-risk.

### Rollback Protocol
- If **3 consecutive attempts** to fix a bug fail, **STOP**.
- Re-read `plan.md`, identify the likely logic error in the approach, and document it in `checks.md`.
- Ask the user for clarification before making the 4th attempt.
- Do not continue blind trial-and-error after the third failed attempt.

### Step 3: Execution & Tracking
- Create `checks.md` to track progress in real-time.
- Use the following status icons:
  - `[ ]` Pending
  - `[/]` In Progress
  - `[x]` Completed
  - `[!]` Blocked/Error (requires manual intervention)
- Updating checks.md is NOT optional. You must check off a task immediately after the code change, before moving to the next step.

### Step 4: Finalization
- Create a `report.md` summarizing:
  - List of modified files.
  - Verification results (tests passed, linting, etc.).
  - Any technical debt introduced or remaining issues.

## 4. Operational Constraints
- **Do not** delete or overwrite existing history logs.
- **Always** reference previous logs if the current task is a follow-up (use `@` or file paths).
- **Consistency**: Follow the project's existing coding style and architectural patterns.

## 5. Directory Mapping
```text
/docs/prompts/
├── templates/        # Reusable prompt structures
└── history/          # Chronological log of all AI agent sessions
    └── YYYY-MM-DD/    # Daily folders
        └── NN-name/   # Individual task folders
            ├── prompt.md
            ├── plan.md
            ├── checks.md
            └── report.md
```

## 6. Testing Guidance
- When behavior/functionality changes, run **`dotnet test src/MimironsGoldOMatic.slnx`** once test projects exist (`docs/overview/ROADMAP.md` MVP-6). Until automated tests land, run **`dotnet build src/MimironsGoldOMatic.slnx`** and targeted `dotnet test` on any new test projects. Manual and integration checks for MVP flows are cataloged in **`docs/overview/INTERACTION_SCENARIOS.md`** (Part 2, TC-xxx).
- **Backend (local):** In **Development**, OpenAPI is mapped via **`MapOpenApi`** (`Program.cs`) for contract inspection.

## 7. Tooling Preference
- Prefer repo-aware tools for reading/searching (`ReadFile`, `Glob`, `rg`) instead of shell commands when possible.

## 8. AI Specialist Roles

This repository uses multiple AI specialist “roles” for implementation consistency across the Twitch, backend, WPF, and WoW Lua layers.

### C#/.NET Coding Standards (Backend & WPF)

Use these conventions for **[Backend/API Expert]** and **[WPF/WinAPI Expert]** C# work. Align with the official Microsoft Framework Design Guidelines and current .NET practice; project rules (namespaces, `MimironsGoldOMatic.*` prefixes) still override where they conflict.

#### Naming conventions

- **PascalCase**: Classes, structs, enums, interfaces, public members (properties, methods, events), and namespaces.
- **camelCase**: Local variables and method parameters.
- **_camelCase**: Private fields (for example `private readonly ILogger _logger;`).
- **Interfaces**: Always prefix with `I` (for example `IRepository`).
- **Async**: Suffix asynchronous methods with `Async`.

#### Formatting and layout

- **Braces**: Allman style (opening brace on its own line).
- **Indentation**: 4 spaces, not tabs.
- **Whitespace**: One space after keywords (`if`, `foreach`), around binary operators, and after commas.
- **Type member order**: Fields, then constructors, then properties, then methods.

#### C# best practices

- **Types**: Prefer C# aliases (`int`, `string`, `bool`) over `Int32`, `String`, `Boolean`.
- **`var`**: Use only when the type is obvious from the right-hand side (for example `var list = new List<string>();`).
- **Strings**: Prefer string interpolation (`$"{value}"`) over `string.Format` or naive concatenation.
- **Expression-bodied members**: Use `=>` for simple one-line properties and methods.
- **Null safety**: Use nullable reference types (`string?`) and null-coalescing / conditional access (`??`, `?.`).

#### Architecture and clean code

- **DI**: Prefer constructor injection.
- **LINQ**: Prefer method syntax (`.Where().Select()`) over query syntax unless clarity favors the latter.
- **Async/await**: Prefer `await` over `.Result` or `.Wait()`; use `Task.CompletedTask` for trivial synchronous completions in async-shaped APIs.
- **Exceptions**: Do not swallow exceptions; catch specific types. Catching `Exception` is reserved for top-level logging or boundary handling when documented.

### [Backend/API Expert]

Responsibilities:
- Design ASP.NET Core API endpoints for **participant pool / roulette spins** (including **`/who`‑gated** winner finalization), **Twitch chat ingestion** for **`!twgold <CharacterName>`** (enroll) only, **`confirm-acceptance`** from Desktop after **WoW whisper `!twgold`**, **winner notification** state for the Extension, **payouts** (validation, auth, status updates), **pool removal** when **`Sent`**, and **`Sent`** after **`[MGM_CONFIRM:UUID]`** from the Desktop log watcher.
- Persist the **write model** with **Marten Event Store on PostgreSQL** (canonical source of truth): **one stream per payout id**; **separate** **Pool** vs **Payout** aggregates (see `docs/overview/SPEC.md` §6). **EF Core** is **optional** and **read-model / projections only**. **Outbox** only when the first external side-effect integration is added (`docs/overview/SPEC.md` §6).
- **Chat:** **EventSub** `channel.chat.message` for enrollment; **`POST /api/roulette/verify-candidate`** for **`/who`** results (from **`[MGM_WHO]`** in **`WoWChatLog.txt`**); **single broadcaster** MVP (`docs/overview/SPEC.md` deployment scope).
- Define shared DTOs/enums and ensure backward-compatible API contracts.
- Integrate JWT auth for the Twitch Extension flow.

### [WPF/WinAPI Expert]

Responsibilities:
- Implement the WPF MVVM client UI and view models.
- Handle Win32 integration for WoW 3.3.5a (process discovery, window focus, and message/posting).
- Bridge **addon-originated `!twgold` acceptance** to the Backend: tail **`WoWChatLog.txt`** (default + override path) for **`[MGM_ACCEPT:UUID]`** → **`POST .../confirm-acceptance`**; tail the **same** log for **`[MGM_CONFIRM:UUID]`** → **`Sent`**; parse **`[MGM_WHO]`** lines from the **same** log → **`POST /api/roulette/verify-candidate`** (see `docs/overview/SPEC.md` §8–10).
- Ensure payload conversion into WoW-compatible command strings (including 255-char chunking). After **`Pending`**, inject **`/run NotifyWinnerWhisper(...)`** per **`docs/overview/SPEC.md` §8–9** before **`ReceiveGold`** mail flow.
- Document WinAPI behaviors/timing and provide reliability notes specific to 3.3.5a.

### [WoW Addon/Lua Expert]

Responsibilities:
- Implement WoW 3.3.5a addon scaffolding (`.toc` + Lua).
- Expose **`NotifyWinnerWhisper(payoutId, characterName)`** (global) for Desktop-injected **`/run`** per **`docs/overview/SPEC.md` §8–9.
- Hook into the mail interface (event hooking / frame integration) to receive queued payout payloads.
- Send the **winner notification whisper** per `docs/overview/SPEC.md` §9 (`/whisper <Winner_InGame_Nickname> …` Russian text); intercept **whisper/private messages** where the body matches **`!twgold`** (**case-insensitive**, no extra text) and **print `[MGM_ACCEPT:UUID]`** to chat so Desktop can read **`WoWChatLog.txt`**. On **`MAIL_SEND_SUCCESS`** for an **MGM-armed** send only, **print `[MGM_CONFIRM:UUID]`** and whisper the winner **`Награда отправлена тебе на почту, проверяй ящик!`**; **do not** run that path for unrelated manual mail (`docs/overview/SPEC.md` §9–10).
- Run **`/who`**, parse **3.3.5a**, emit **`[MGM_WHO]`** + JSON to the default chat frame so it appears in **`WoWChatLog.txt`** (`docs/overview/SPEC.md` §8); support mail flow as before.
- Provide a robust mail queue processor and UI population logic.
- Keep code compatible with FrameXML and the 3.3.5a Lua environment constraints.

### [Frontend/Twitch Expert]

Responsibilities:
- Scaffold the Twitch Extension UI using React + Vite + TypeScript.
- Implement **visual roulette** (fixed **5-minute** spin; **no** early spins) and copy that directs viewers to **`!twgold <CharacterName>`** in **stream chat** (subscriber); show a **countdown to the next spin** using **server-authoritative** schedule fields from the API (`docs/overview/SPEC.md` §5, §11). **“You won”** UX and instructions that **in-game whisper reply `!twgold`** (case-insensitive) is **required** for consent before gold mail (`docs/overview/SPEC.md` §9–11). Hardcode **`Награда отправлена персонажу <WINNER_NAME> на почту, проверяй ящик!`** for **`Sent`** panel copy and coordinate **broadcast chat** announcement per **`docs/overview/SPEC.md` §11** (Backend Helix and/or Extension-triggered API).
- Integrate with the expected Twitch auth/token mechanism for the API.
- Align client-side types with shared DTOs produced/consumed by the backend.

---

## 9. Audit log (recent agent work)

### 2026-04-06 — Post‑Tier B closure + Tier C prep

- **Audit trails**
  - `docs/prompts/history/2026-04-06/11-post-tier-b-closure-next-steps/`
  - `docs/prompts/history/2026-04-06/12-final-review-tier-c-prep/`
- **Key deliverables**
  - Tier B closure report: `docs/e2e/TIER_B_CLOSURE_REPORT.md`
  - Tier B handover (retrospective + quick-start + FAQ): `docs/e2e/TIER_B_HANDOVER.md`
  - Monitoring/alerting verification runbook: `docs/e2e/TIER_B_MAINTENANCE_CHECKLIST.md`
  - Knowledge transfer script/checklist: `docs/e2e/TIER_B_KNOWLEDGE_TRANSFER.md`
  - Tier C plan + dashboard + handover-prep: `docs/e2e/TIER_C_KICKOFF_PLAN.md`, `docs/e2e/TIER_C_PROGRESS.md`, `docs/e2e/TIER_C_HANDOVER_PREP.md`

### 2026-04-06 — Transition complete & Tier C launch

- Transition report: `docs/e2e/TIER_B_TRANSITION_COMPLETE.md`
- Audit trail: `docs/prompts/history/2026-04-06/13-transition-complete-tier-c-launch/`

### 2026-04-06 — Tier C launch execution (C0 initialized)

- Action: Tier C launch — C0 issue package prepared, kick-off artifacts scheduled, reporting cadence documented.
- Links:
  - `docs/prompts/history/2026-04-06/15-tier-c-launch/`
  - `docs/e2e/TIER_C_PROGRESS.md`
  - `docs/prompts/history/2026-04-06/15-tier-c-launch/kickoff-notes.md`

### 2026-04-08 — Tier C completion closure

- Closure report: `docs/e2e/TIER_C_CLOSURE_REPORT.md`
- Maintainer handover: `docs/e2e/TIER_C_HANDOVER.md`
- Risk log: `docs/risks/tier-c-risk-log.md`
- Audit trail: `docs/prompts/history/2026-04-06/16-tier-c-completion/`


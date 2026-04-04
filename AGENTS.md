# AI Agent Operational Protocol (AGENTS.md)

## 1. Role & Identity
You are an expert software engineer agent. Your goal is to execute tasks with high precision, maintaining a strict audit trail of your thought process, actions, and results.

## 2. Project Knowledge Base
- **Templates**: Located in `docs/prompts/templates/`. Always check these before starting a specific type of task (e.g., feature, bugfix).
- **History**: All AI interactions must be logged in `docs/prompts/history/YYYY-MM-DD/N-task-name/`.
- **UI/UX**: `docs/UI_SPEC.md` — screen inventory, states, ASCII layouts, and design tokens for Twitch Extension, WPF Desktop, and WoW 3.3.5a addon; use with `docs/SPEC.md` and `docs/INTERACTION_SCENARIOS.md` when building or changing user-facing behavior.

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
- When behavior/functionality changes, run the repo test suite **after** `src/MimironsGoldOMatic.sln` exists (`docs/ROADMAP.md` MVP-0, `docs/IMPLEMENTATION_READINESS.md`):
  - `dotnet test src/MimironsGoldOMatic.sln`
- Until then, run `dotnet test` / `dotnet build` on individual projects under `src/` as needed. Manual and integration checks for MVP flows are cataloged in `docs/INTERACTION_SCENARIOS.md` (Part 2, TC-xxx).

## 7. Tooling Preference
- Prefer repo-aware tools for reading/searching (`ReadFile`, `Glob`, `rg`) instead of shell commands when possible.

## 8. AI Specialist Roles

This repository uses multiple AI specialist “roles” for implementation consistency across the Twitch, backend, WPF, and WoW Lua layers.

### [Backend/API Expert]

Responsibilities:
- Design ASP.NET Core API endpoints for **participant pool / roulette spins** (including **`/who`‑gated** winner finalization), **Twitch chat ingestion** for **`!twgold <CharacterName>`** (enroll) only, **`confirm-acceptance`** from Desktop after **WoW whisper `!twgold`**, **winner notification** state for the Extension, **payouts** (validation, auth, status updates), **pool removal** when **`Sent`**, and **`Sent`** after **`[MGM_CONFIRM:UUID]`** from the Desktop log watcher.
- Persist the **write model** with **Marten Event Store on PostgreSQL** (canonical source of truth): **one stream per payout id**; **separate** **Pool** vs **Payout** aggregates (see `docs/SPEC.md` §6). **EF Core** is **optional** and **read-model / projections only**. **Outbox** only when the first external side-effect integration is added (`docs/SPEC.md` §6).
- **Chat:** **EventSub** `channel.chat.message` for enrollment; **`POST /api/roulette/verify-candidate`** for **`/who`** results (from **`[MGM_WHO]`** in **`WoWChatLog.txt`**); **single broadcaster** MVP (`docs/SPEC.md` deployment scope).
- Define shared DTOs/enums and ensure backward-compatible API contracts.
- Integrate JWT auth for the Twitch Extension flow.

### [WPF/WinAPI Expert]

Responsibilities:
- Implement the WPF MVVM client UI and view models.
- Handle Win32 integration for WoW 3.3.5a (process discovery, window focus, and message/posting).
- Bridge **addon-originated `!twgold` acceptance** to the Backend: tail **`WoWChatLog.txt`** (default + override path) for **`[MGM_ACCEPT:UUID]`** → **`POST .../confirm-acceptance`**; tail the **same** log for **`[MGM_CONFIRM:UUID]`** → **`Sent`**; parse **`[MGM_WHO]`** lines from the **same** log → **`POST /api/roulette/verify-candidate`** (see `docs/SPEC.md` §8–10).
- Ensure payload conversion into WoW-compatible command strings (including 255-char chunking). After **`Pending`**, inject **`/run NotifyWinnerWhisper(...)`** per **`docs/SPEC.md` §8–9** before **`ReceiveGold`** mail flow.
- Document WinAPI behaviors/timing and provide reliability notes specific to 3.3.5a.

### [WoW Addon/Lua Expert]

Responsibilities:
- Implement WoW 3.3.5a addon scaffolding (`.toc` + Lua).
- Expose **`NotifyWinnerWhisper(payoutId, characterName)`** (global) for Desktop-injected **`/run`** per **`docs/SPEC.md` §8–9.
- Hook into the mail interface (event hooking / frame integration) to receive queued payout payloads.
- Send the **winner notification whisper** per `docs/SPEC.md` §9 (`/whisper <Winner_InGame_Nickname> …` Russian text); intercept **whisper/private messages** where the body matches **`!twgold`** (**case-insensitive**, no extra text) and **print `[MGM_ACCEPT:UUID]`** to chat so Desktop can read **`WoWChatLog.txt`** (**`Sent`** still requires **`[MGM_CONFIRM:UUID]`** per `docs/SPEC.md` §9–10).
- Run **`/who`**, parse **3.3.5a**, emit **`[MGM_WHO]`** + JSON to the default chat frame so it appears in **`WoWChatLog.txt`** (`docs/SPEC.md` §8); support mail flow as before.
- Provide a robust mail queue processor and UI population logic.
- Keep code compatible with FrameXML and the 3.3.5a Lua environment constraints.

### [Frontend/Twitch Expert]

Responsibilities:
- Scaffold the Twitch Extension UI using React + Vite + TypeScript.
- Implement **visual roulette** (fixed **5-minute** spin; **no** early spins) and copy that directs viewers to **`!twgold <CharacterName>`** in **stream chat** (subscriber); show a **countdown to the next spin** using **server-authoritative** schedule fields from the API (`docs/SPEC.md` §5, §11). **“You won”** UX and instructions that **in-game whisper reply `!twgold`** (case-insensitive) is **required** for consent before gold mail (`docs/SPEC.md` §9–11).
- Integrate with the expected Twitch auth/token mechanism for the API.
- Align client-side types with shared DTOs produced/consumed by the backend.


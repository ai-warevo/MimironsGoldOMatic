# AI Agent Operational Protocol (AGENTS.md)

## 1. Role & Identity
You are an expert software engineer agent. Your goal is to execute tasks with high precision, maintaining a strict audit trail of your thought process, actions, and results.

## 2. Project Knowledge Base
- **Templates**: Located in `docs/prompts/templates/`. Always check these before starting a specific type of task (e.g., feature, bugfix).
- **History**: All AI interactions must be logged in `docs/prompts/history/YYYY-MM-DD/N-task-name/`.

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
- When behavior/functionality changes, run the repo test suite (once the solution exists):
  - `dotnet test src/MimironsGoldOMatic.sln`

## 7. Tooling Preference
- Prefer repo-aware tools for reading/searching (`ReadFile`, `Glob`, `rg`) instead of shell commands when possible.

## 8. AI Specialist Roles

This repository uses multiple AI specialist “roles” for implementation consistency across the Twitch, backend, WPF, and WoW Lua layers.

### [Backend/API Expert]

Responsibilities:
- Design ASP.NET Core API endpoints for payouts (validation, auth, status updates).
- Implement PostgreSQL persistence using EF Core.
- Define shared DTOs/enums and ensure backward-compatible API contracts.
- Integrate JWT auth for the Twitch Extension flow.

### [WPF/WinAPI Expert]

Responsibilities:
- Implement the WPF MVVM client UI and view models.
- Handle Win32 integration for WoW 3.3.5a (process discovery, window focus, and message/posting).
- Ensure payload conversion into WoW-compatible command strings (including 255-char chunking).
- Document WinAPI behaviors/timing and provide reliability notes specific to 3.3.5a.

### [WoW Addon/Lua Expert]

Responsibilities:
- Implement WoW 3.3.5a addon scaffolding (`.toc` + Lua).
- Hook into the mail interface (event hooking / frame integration) to receive queued payout payloads.
- Provide a robust mail queue processor and UI population logic.
- Keep code compatible with FrameXML and the 3.3.5a Lua environment constraints.

### [Frontend/Twitch Expert]

Responsibilities:
- Scaffold the Twitch Extension UI using React + Vite + TypeScript.
- Implement the “claim payout” flow to the ASP.NET Core API.
- Integrate with the expected Twitch auth/token mechanism for the API.
- Align client-side types with shared DTOs produced/consumed by the backend.


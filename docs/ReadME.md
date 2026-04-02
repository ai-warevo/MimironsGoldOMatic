# Architecture & Repo Layout

## High-level Workflow
1. **Redemption:** A viewer enters their Character Name in the Twitch Extension and spends Channel Points.
2. **Queueing:** The Backend API receives the request, validates it, and stores it in a PostgreSQL database with `Pending` status.
3. **Synchronization:** The streamer opens the Desktop WPF App, which fetches pending payouts via REST API.
4. **Injection:** The Desktop App uses Win32 API (`PostMessage`) to send specialized Lua commands into the WoW client.
5. **Execution:** The WoW Addon receives the commands, populates an internal queue, and provides one-click gold sending via the Mailbox UI.

## Core Components
- **Twitch Extension (Frontend):** Viewer-facing interface for claims.
- **Backend (API):** Source of truth for payout lifecycle + persistence + authentication.
- **Shared Library (Contracts):** DTOs/Enums shared between Backend and Desktop.
- **Desktop Utility (WPF):** Bridge between the UI and the running WoW client (Win32 automation).
- **WoW Addon (Lua):** Final in-game executor (mail hooks + UI helpers).

## Repo Layout (expected)
This repository is organized as a monorepo so the contract (`Shared`) and implementations (`Backend`, `Desktop`) evolve together.

```text
MimironsGoldOMatic/
├── .cursor/
│   ├── commands/
│   └── rules/
│       ├── agent-protocol-compat.mdc
│       └── project-rules.mdc
├── .github/
│   └── workflows/
├── docs/
│   ├── MimironsGoldOMatic.*/ReadME.md
│   └── prompts/ (Cursor prompt templates + task history)
├── src/ (solution/projects root)
├── README.md (public overview)
├── CONTEXT.md (architecture summary)
└── AGENTS.md (agent roles + mandatory workflow)
```

## Naming Convention
All C# namespaces must start with `MimironsGoldOMatic`.
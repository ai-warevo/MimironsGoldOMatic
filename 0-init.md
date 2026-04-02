# Project: Mimiron's Gold-o-Matic (WoW 3.3.5a Gold Distribution System)
## Repository bootstrap notes (reference)

Use `1-bootstrap.md` as the canonical setup guide for bootstrapping this workspace.

This file is intentionally kept short and points to the current repo structure:

- `/.cursor/` contains Cursor project rules:
  - `.cursor/rules/agent-protocol-compat.mdc` (workflow priority shim)
  - `.cursor/rules/project-rules.mdc` (project-specific guidance)
- `/.github/workflows/` holds CI pipelines.
- `/docs/` contains architecture/spec documentation.
- `/src/` is the source root for .NET + Twitch (React/Vite) + WoW addon.
- `README.md`, `CONTEXT.md`, and `AGENTS.md` are the main entry points for architecture + agent workflow.


# Plan: migrate `.cursor/rules.md` into `.cursor/rules/*.mdc`

## Motivation
There is duplication between `AGENTS.md`, `.cursor/rules/agent-protocol-compat.mdc`, and the top section of `.cursor/rules.md` (both state that `AGENTS.md` is the single source of truth). Moving the rules into the modern `.cursor/rules/*.mdc` structure allows us to:
- reduce duplication,
- keep `AGENTS.md` as the explicit source of truth,
- preserve the remaining project-specific guidance (namespaces/layout/WinAPI-WoW notes).

## Current state (facts)
- `AGENTS.md` defines the agent workflow source of truth.
- `.cursor/rules/agent-protocol-compat.mdc` already tells the agent to follow `AGENTS.md` in case of conflict.
- `.cursor/rules.md` contains a section duplicating that “source of truth” idea, plus additional project guidance.
- `CONTEXT.md` contains architecture context and does not reference `.cursor/rules.md` directly.

## Target state
- Add `.cursor/rules/project-rules.mdc` containing the non-duplicating parts of `.cursor/rules.md`.
- Update `.cursor/rules.md` to a minimal stub (no duplication) that points to the new `.mdc` file OR remove it if safe.
- Keep `.cursor/rules/agent-protocol-compat.mdc` as-is (it will remain the only workflow-priority shim).

## Refactor strategy
1. Create `.cursor/rules/project-rules.mdc`:
   - Copy content from `.cursor/rules.md`.
   - Remove the section duplicating the “Agent Workflow Source of Truth” from the moved file.
   - Add `.mdc` frontmatter (`alwaysApply: true`) to match the existing shim style.
2. Update `.cursor/rules.md`:
   - Replace the duplicated content with a short note (or minimal remaining unique content if any).
   - Ensure it no longer instructs the agent in a conflicting way with `agent-protocol-compat.mdc` + `AGENTS.md`.
3. Verification:
   - Confirm the duplication string/section is no longer present in `.cursor/rules.md`.
   - Confirm `project-rules.mdc` includes the naming/layout/WinAPI guidance.
   - Run `dotnet test src/MimironsGoldOMatic.sln` (verification gate per DoD, even though this is config-only).

## Testing plan
- `dotnet test src/MimironsGoldOMatic.sln`
- Manual verification (if needed):
  - ensure Cursor still loads project rules after migration (checked by observing generated behavior once).

## Risk assessment
Low-to-medium risk:
- Cursor might still prefer legacy `.cursor/rules.md` if it doesn't load `.mdc` equivalents as expected.
Mitigation:
- Keep `.cursor/rules.md` as a stub without duplication rather than deleting it outright.

## Definition of Done
- [ ] `project-rules.mdc` exists under `.cursor/rules/`.
- [ ] `.cursor/rules.md` no longer duplicates the workflow “source of truth” section.
- [ ] `agent-protocol-compat.mdc` remains the only workflow-priority instruction.
- [ ] Tests pass: `dotnet test src/MimironsGoldOMatic.sln`.
- [ ] `report.md` includes `Potential technical debt`.

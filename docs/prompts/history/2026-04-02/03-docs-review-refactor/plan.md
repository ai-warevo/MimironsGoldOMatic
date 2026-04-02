# Plan: Refactor project documentation (non-code)

## Goal
Improve consistency, remove outdated/duplicated documentation, and align docs with the current repo layout (notably `.cursor/rules/*.mdc` and the deduplicated Cursor rule workflow).

## Scope (files to review/modify)
- `README.md` (only if needed for consistency)
- `docs/ReadME.md` (normalize formatting + fix monorepo structure snippet)
- `0-init.md` (remove escaped markdown artifacts, update cursor rules references)
- `1-bootstrap.md` (update outdated `rules.md` reference)
- `docs/MimironsGoldOMatic.*/*ReadME.md` (light formatting consistency only, no content changes unless incorrect)

## Proposed documentation changes
1. Remove outdated references to `rules.md` under `/.cursor`:
   - Replace with `.cursor/rules/project-rules.mdc` and mention `agent-protocol-compat.mdc` where relevant.
2. Fix `0-init.md` markdown:
   - Remove backslash-escaped markdown markers (e.g., `\#`, `\-`, `\##`) so it renders correctly.
3. Deduplicate public vs internal docs intent:
   - Keep `README.md` as the concise public entrypoint.
   - Update `docs/ReadME.md` to be an “architecture + repo layout” page without contradicting `README.md`.
4. Fix the monorepo structure snippet in `docs/ReadME.md`:
   - Use actual paths/names from this repo (including `.cursor/rules/*.mdc`).
5. Consistency sweep:
   - Verify links/paths and headings don’t refer to removed files.

## Risks
- Low risk: docs-only changes.
- Medium risk: changing filenames/paths in docs can break references; this plan avoids renames unless clearly beneficial.

## Testing / verification
- No automated tests expected for docs-only changes.
- Manual verification:
  - `rg` for leftover references to `.cursor/rules.md` (should be none).
  - Ensure markdown is valid-looking (no `\#` artifacts).

## Definition of Done
- [ ] Outdated references to `rules.md` under `/.cursor` are removed/updated.
- [ ] `0-init.md` renders cleanly (no `\#`, `\##`, `\-` artifacts).
- [ ] `docs/ReadME.md` monorepo snippet matches current repo structure.
- [ ] `report.md` includes verification + `Potential technical debt`.

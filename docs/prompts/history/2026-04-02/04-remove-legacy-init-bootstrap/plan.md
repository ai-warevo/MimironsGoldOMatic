# Plan: remove legacy `0-init.md` and `1-bootstrap.md`

## Motivation
The repo currently has multiple bootstrap/init sources:
- legacy root files (`0-init.md`, `1-bootstrap.md`)
- the canonical init prompt (`docs/prompts/history/2026-04-02/00-init/prompt.md`)
- phased bootstrap prompts (`docs/prompts/todo/`)

To reduce confusion, remove the legacy root files and point entry-point docs to the canonical locations.

## Steps
1. Delete `0-init.md` and `1-bootstrap.md`.
2. Update `README.md` and `docs/ReadME.md` to reference:
   - `docs/prompts/history/2026-04-02/00-init/prompt.md` (initialization)
   - `docs/prompts/todo/` (bootstrap phases)
3. Verify no remaining references to removed files.
4. Commit changes with a concise message.

## Risks
Low: documentation-only change. Mitigated by adding explicit pointers in entry-point docs.


# Report: remove legacy init/bootstrap root docs

## Summary
- Removed legacy root bootstrap docs (`0-init.md`, `1-bootstrap.md`) to avoid duplication.
- Made `docs/prompts/history/2026-04-02/00-init/prompt.md` the canonical init prompt and updated it to reference `.cursor/rules/*.mdc`.
- Kept bootstrap phases in `docs/prompts/todo/` as the canonical phased bootstrap prompts.
- Updated `README.md` and `docs/ReadME.md` to link to the new canonical prompt locations.

## Modified files
- Deleted: `0-init.md`
- Deleted: `1-bootstrap.md`
- Updated: `README.md`
- Updated: `docs/ReadME.md`
- Updated: `docs/prompts/history/2026-04-02/00-init/prompt.md`
- Added: `docs/prompts/todo/*`
- Added: `docs/prompts/history/2026-04-02/04-remove-legacy-init-bootstrap/*` (prompt/plan/checks/report)
- Deleted (superseded): `docs/prompts/history/2026-04-02/01-init/prompt.md`

## Verification
- Searched repo for references to `0-init.md` / `1-bootstrap.md`: only found in history logs.
- Verified `00-init/prompt.md` has no references to `rules.md`.

## Potential technical debt
- Older history logs still mention `0-init.md` / `1-bootstrap.md` (historical context). Keeping them unchanged avoids rewriting history, but means repo-wide searches will still find those strings under `docs/prompts/history/...`.

## Summary

- Integrated the phased bootstrap prompts (previously in `docs/prompts/todo/`) directly into `docs/ROADMAP.md` under `MVP-1..MVP-6`, making the roadmap the single canonical source.
- Updated the root `README.md` to remove the link to `docs/prompts/todo/` and point to `docs/ROADMAP.md` instead.
- Deleted the `docs/prompts/todo/*.md` files to eliminate duplication.

## Modified files

- `docs/ROADMAP.md`
- `README.md`
- Deleted:
  - `docs/prompts/todo/1-Shared.md`
  - `docs/prompts/todo/2-WoWAddon.md`
  - `docs/prompts/todo/3-Backend.md`
  - `docs/prompts/todo/4-Desktop.md`
  - `docs/prompts/todo/5-TwitchExtension.md`
  - `docs/prompts/todo/6-ReadME.md`
- Added workflow artifacts:
  - `docs/prompts/history/2026-04-02/08-integrate-todo-prompts-into-roadmap/*`

## Verification

- Grep for `prompts/todo` now only matches historical logs under `docs/prompts/history/...` (expected).
- No “live docs” links remain to the deleted `docs/prompts/todo/` directory.

## Potential technical debt

- The exact header name for the Desktop `ApiKey` remains implementation-defined; roadmap prompts keep it semantic.


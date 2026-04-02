# Report: migrate Cursor rules into `.cursor/rules/*.mdc`

## Modified files
- Added: `.cursor/rules/project-rules.mdc`
- Deleted: `.cursor/rules.md`
- Updated/Added workflow artifacts:
  - `docs/prompts/history/2026-04-02/01-cursor-rules-migration/prompt.md`
  - `docs/prompts/history/2026-04-02/01-cursor-rules-migration/plan.md`
  - `docs/prompts/history/2026-04-02/01-cursor-rules-migration/checks.md`

## Verification results
- `dotnet test src/MimironsGoldOMatic.sln`: not executed (blocked)
  - Reason: no `.sln`/`.csproj` found in the repository; `src/` contains only `.gitkeep`.

## Potential technical debt
- Cursor rule-loading behavior might still rely on `.cursor/rules.md` in some setups; to reduce risk, if any issue is noticed, we can either:
  - restore a minimal `.cursor/rules.md` stub, or
  - add additional `.mdc` files and metadata (`globs`) if required by Cursor.

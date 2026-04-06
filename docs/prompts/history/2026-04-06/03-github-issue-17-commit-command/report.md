# Report

## Modified files

- `.cursor/commands/commit.md` — full rewrite: CC v1.0.0 workflow, types (`build`, `ci`, `perf`, `revert`, `security`), breaking changes (`!` + `BREAKING CHANGE`), free scope + path-based suggestions, standard footers before custom metadata, header ≤50 / body ≤72, validation checklist, Unix heredoc + PowerShell UTF-8 examples, split-commit guidance.
- `.cursor/rules/git-commit-footer.mdc` — standard footers may appear before `Made-with` / `Co-authored-by`.

## Verification

- Documentation-only change; no `dotnet build` required.
- Rules remain consistent with repo namespace/conventions.

## Technical debt

- None.

# Plan (consolidated)

## A. Amend-commit skill and commit

1. Inspect `git diff HEAD` and `git status` to choose conventional commit type/scope and description.
2. Add `.cursor/skills/amend-commit/` — `SKILL.md`, `reference.md`, `scripts/amend_commit_meta.mjs` (Node `git filter-branch` helpers: `run`, `msg-filter-env`, `env-filter-env`, etc.).
3. Create `docs/prompts/history/2026-04-04/06-amend-commit-skill/` with `prompt.md`, `plan.md`, `checks.md`, `report.md`.
4. Commit with required footer per `.cursor/commands/commit.md`.

## B. Launcher ergonomics

1. **First attempt:** `amend_commit_meta.cmd` next to the `.mjs`, forwarding `%*` via `node "%~dp0amend_commit_meta.mjs"`.
2. **Final:** Remove `.cmd`; add `amend_commit_meta_wrap.mjs` that `spawnSync`s `process.execPath` with `[amend_commit_meta.mjs, ...process.argv.slice(2)]`, `stdio: "inherit"`, preserve exit code; document examples in the wrapper header (no cmd/PowerShell line continuations).

## C. History merge

Combine follow-up sessions into this directory (`06-amend-commit-skill`); remove duplicate history folders `07-amend-commit-wrapper-script` and `08-amend-commit-node-wrapper` after merging their substance into these files.

## Risks

- Skill documents history rewrite; users must understand local-branch-only scope and backup/`refs/original/` cleanup.
- cmd `%*` forwarding was a minor quoting risk; the Node wrapper avoids that.

# User requests (consolidated)

## 1. Initial session

`/commit` (conventional commit with Made-with / Co-authored-by footer) and save prompt history.

Context: add and iterate **amend-commit** Cursor Agent Skill — Node.js (`amend_commit_meta.mjs`) `git filter-branch` automation, no Python/PowerShell; enforce local-only / current-branch-only / no push.

## 2. Wrapper (superseded)

Create a script to call `.cursor/skills/amend-commit/scripts/amend_commit_meta.mjs` with all args and place it next to it.

## 3. Node wrapper (final)

Replace cmd/PowerShell launcher preference: add a JavaScript (Node.js) wrapper instead, forward all argv, and include usage examples in the wrapper file.

## 4. Consolidate history and amend

Merge `docs/prompts/history/2026-04-04/06-amend-commit-skill/`, `07-amend-commit-wrapper-script/`, and `08-amend-commit-node-wrapper/` into a single `06-amend-commit-skill/` (rewrite all four `.md` files there), remove the `07` and `08` directories, and `git commit --amend` with an updated message reflecting the full change set.

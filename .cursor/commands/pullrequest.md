---
description: Analyzes diff vs origin/main and drafts a Pull Request title and description
---

# Pull Request Title + Description (vs `origin/main`)

1. **Sync refs**: Run `git fetch origin main` (or `git fetch origin`) so `origin/main` is current. If the default branch is not `main`, use the repo’s actual default remote branch instead.
2. **Collect context** (run and read output):
   - **Changed files & hunks**: `git diff origin/main...HEAD` (three-dot: changes this branch introduces relative to the merge base with `main`; matches typical PR “Files changed” semantics).
   - **Commit narrative** (optional but useful): `git log origin/main..HEAD --oneline --no-decorate` (two-dot: commits reachable from `HEAD` but not from `origin/main`).
   - If the diff is huge, also use `git diff --stat origin/main...HEAD` for a high-level map.
3. **If `origin/main` is missing or unreachable**: Say so clearly; fall back to `git merge-base HEAD main` / local `main` only if the user confirms, and note the assumption in the PR text.
4. **TITLE** (one line):
   - **Language**: English.
   - **Style**: Concise; imperative or outcome-focused (e.g. “Add …”, “Fix …”, “Refactor …”). Optionally mirror Conventional Commits (`feat`, `fix`, etc.) if it fits.
   - **Length**: Aim for ≤ ~72 characters; no trailing period.
5. **DESCRIPTION** (Markdown body):
   - **Summary**: 1–3 sentences: what changed and why (user-visible or architectural intent).
   - **Changes**: Bullet list grouped by area when helpful (align with this repo: `shared`, `backend`, `desktop`, `addon`, `extension`, `ci`, `docs`, etc.—same idea as `.cursor/commands/commit.md` scopes).
   - **How to test**: Concrete steps or `dotnet test` / build commands if applicable.
   - **Risks / follow-ups**: Only if non-obvious (migrations, breaking API, WinAPI timing, 3.3.5a behavior).
   - **Out of scope**: Only if the diff clearly mixes concerns; otherwise omit.
6. **Output format** (exact sections for copy-paste):

```text
## PR TITLE
<title here>

## PR DESCRIPTION
<markdown body here>
```

7. **Do not** invent changes: only describe what appears in the diff and commit list. If something is unclear from the diff, say what is ambiguous instead of guessing.

### Shell reference (Windows / PowerShell)

```powershell
git fetch origin main
git diff --stat origin/main...HEAD
git diff origin/main...HEAD
git log origin/main..HEAD --oneline --no-decorate
```

On Git Bash, the same `git` arguments apply.

---
name: amend-commit
description: >-
  Rewrites local Git history on the checked-out branch only: normalizes Made-with and
  Co-Authored-By trailers and optionally author/committer via git filter-branch. Does
  not push, fetch, or modify remotes or other local branches. Use from the repo work
  tree when the user wants to fix trailers or metadata without touching remote refs.
  Requires Node.js and Git.
---

# Git history and metadata refactor (amend-commit)

## When to use

Apply when the user asks to:

- Normalize or replace **`Made-with:`** / **`Co-Authored-By:`** trailers on **the current local branch**
- Set **author** / **committer** on that rewrite range only
- Fix one commit on **this branch’s history** and replay descendants (new SHAs)

Do **not** use on **shared** branches without explicit user consent (history rewrite).

## What this skill never does

- **No `git push` or `git fetch`** (or any remote-updating command) — the script does not run them.
- **No updates to `refs/remotes/*`** — remotes may be **read** only for `--base` (e.g. `origin/main`) when computing `merge-base`.
- **No rewriting other local branches** — only the **currently checked-out** branch ref is passed to `filter-branch`.
- **No cross-repo use** — `--repo` must resolve to the **same** Git work tree as the current working directory.

Checkout the branch you intend to rewrite before running.

## `--message-only` (what it does)

`--message-only` is a **boolean flag** (it takes **no** value). It controls whether `git filter-branch` also runs an **`--env-filter`** that rewrites **author and committer** identity on replayed commits.

### With `--message-only` (trailers only)

- The script runs **`--msg-filter` only** (no `--env-filter`).
- For each commit in the revision range, the **commit message** is processed:
  - Any existing trailing block starting at the last **`Made-with:`** (per [reference.md](reference.md)) is **removed**.
  - The message is then ended with the normalized trailers: **`Made-with:`** and **`Co-Authored-By:`** using your **`--made-with`** and **`--co-authored-by`** values.
- **`Author` and `Committer` fields** on each new commit object (name, email, dates as Git applies them during replay) are **not overridden by this tool** — they stay aligned with what Git copies from the original commits when rewriting.
- You **do not** pass **`--author-name`**, **`--author-email`**, **`--committer-name`**, or **`--committer-email`**; those are required only when **`--message-only` is omitted**.

Use this when you only want to **standardize or fix footers** (e.g. Cursor **Made-with** / **Co-authored-by** lines) and **keep** whoever is already recorded as author/committer in history.

### Without `--message-only` (messages + identity)

- The script runs **`--msg-filter`** **and** **`--env-filter`**.
- **Messages** get the same trailer normalization as above.
- **Additionally**, for each replayed commit (or only the targeted commit in `single` mode, per the script’s filters), Git’s **`GIT_AUTHOR_*`** and **`GIT_COMMITTER_*`** are set to the values you pass on the command line, so **author and committer metadata are rewritten** to match those four fields.

Use this when you want **one consistent identity** stamped on the rewritten range (e.g. after a bad import or wrong local `user.name` / `user.email` on past commits).

### Common misconceptions

- **`--message-only` does not mean “dry run.”** History is still rewritten (new commit hashes, `refs/heads/<branch>` moves). You still need backup / `refs/original/` cleanup awareness.
- **Trailers are not “metadata” in Git’s sense** — they live **inside the commit message string**. **`Co-authored-by`** in the message is **not** the same as the commit’s **`committer`** field; this tool’s **`--co-authored-by`** argument only affects **message text**, not Git’s internal committer unless you also omit **`--message-only`** and supply committer fields (which control **`GIT_COMMITTER_*`**, not the trailer line).

## Optional parameters: `--repo`, `--head`, `--base`

The workflow is **always**: one **local** repository (the same work tree as your shell’s cwd) and **one** **checked-out** branch. There is **no** flag to pick “another repo” or “another branch to rewrite.”

These options remain for **narrow technical reasons**, not to bypass those rules:

| Flag | Still exists because | What it does **not** do |
|------|----------------------|-------------------------|
| **`--repo`** | Tells Git **which path** to pass to **`git -C`** (default **`.`**). Handy from monorepo subfolders or scripts where the working directory is not the repo root. | **Cannot** target a different clone than cwd: it **must** resolve to the **same** **`git rev-parse --show-toplevel`** as **`process.cwd()`**. |
| **`--head`** | Optional **validation** only: may be omitted, or set to **`HEAD`** or the **exact current branch short name**. | **Cannot** select another branch to rewrite; any other value is **rejected**. |
| **`--base`** | **`mode=all` only:** the **read-only** ref used with **`merge-base`** to decide **which commits** are considered “on top of” the integration point (default tries **`origin/main`**, **`main`**, etc.). | **Does not** name the branch being rewritten (that is always the **checked-out** branch). **Does not** modify the ref you pass; it is only resolved to a commit SHA. |

**Typical usage:** omit **`--head`**; use default **`--repo .`**; omit **`--base`** unless your upstream for range calculation is not discoverable by the defaults.

## Safety (mandatory)

1. **Warn** that **local** history is rewritten; publishing still requires an explicit user-run **`git push --force-with-lease`** (the script does not push).
2. Prefer a **backup branch** first: `git branch backup/pre-amend HEAD`.
3. After verification, remove **`refs/original/`** (see [reference.md](reference.md)).
4. **Do not** change `git config --global user.*` as part of automation unless the user explicitly asks; the shipped script applies identity **only** inside `filter-branch` via environment variables.

## Implementation (execute)

Run from **inside** the repository (any subdirectory of that work tree). Use **Node.js 18+** (built-in modules only; no `npm install`).

Omit **`--head`** unless you need to pass **`HEAD`** explicitly; it must match the checked-out branch.

```bash
node .cursor/skills/amend-commit/scripts/amend_commit_meta.mjs run \
  --mode all \
  --made-with "Cursor" \
  --co-authored-by "Cursor Agent <cursoragent@cursor.com>" \
  --author-name "Legal Name" \
  --author-email "email@example.com" \
  --committer-name "Legal Name" \
  --committer-email "email@example.com"
```

**Trailers only** (keep per-commit author/committer as stored in Git):

```bash
node .cursor/skills/amend-commit/scripts/amend_commit_meta.mjs run \
  --mode all \
  --made-with "Cursor" \
  --co-authored-by "Cursor Agent <cursoragent@cursor.com>" \
  --message-only
```

**Single commit** (`--commit` must be an ancestor of the **current branch tip**):

```bash
node .cursor/skills/amend-commit/scripts/amend_commit_meta.mjs run \
  --mode single --commit <hash> \
  --made-with "..." --co-authored-by "..." \
  ...identity or --message-only...
```

### Scope rules

| `--mode` | Revision range | Branch ref updated |
|----------|----------------|--------------------|
| `all` | `merge-base(base, current)..current` | **Checked-out** branch only (`--base` is read-only) |
| `single` | `commit^..current` | **Checked-out** branch only; **`commit` on this branch** |

**Symbolic ref:** `filter-branch` still needs a branch **name**; the script always uses the **current branch** short name (not a peeled SHA) for the positive ref (Git for Windows).

### After success

- Show **`git log --oneline -5`** on the **current** branch and confirm trailers.
- Remind that **pushing** is **manual** if they want the remote updated; mention **`refs/original/`** cleanup.
- Summarize **old → new** tip SHA if useful.

## Windows

Use **Git for Windows** (filters run under `sh`). Invoke `node … amend_commit_meta.mjs run …` from **cmd**, **PowerShell**, or **Git Bash** from within the repo.

## Details and edge cases

See [reference.md](reference.md).

## Script maintenance

Filters use **`MGM_*` environment variables** inside `git filter-branch` so `--msg-filter` does not embed fragile quoted payloads. Internal subcommands: **`msg-filter-env`**, **`env-filter-env`**.

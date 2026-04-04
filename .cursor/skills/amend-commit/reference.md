# amend-commit — reference

## Message shape (normative)

After rewrite, each affected commit message ends with exactly one blank line before trailers, then:

```text
Made-with: <value>

Co-Authored-By: <value>
```

Existing trailing `Made-with` blocks (from the last `\n\nMade-with:` onward) are stripped before re-appending.

## Local scope (enforced)

| Rule | Behavior |
|------|----------|
| Repository | **`--repo`** must resolve to the **same** Git work tree as **`git rev-parse --show-toplevel`** from **process.cwd()**. |
| Branch | **Detached HEAD** is rejected. Only the **checked-out local branch** is rewritten; **`--head`** may be omitted, or must be **`HEAD`** or that branch name. |
| `single` / `--commit` | **`--commit`** must be an **ancestor** of the current branch tip (on this branch’s history). |
| Remotes | The script **never** runs **`git push`**, **`git fetch`**, or similar. It does **not** update **`refs/remotes/*`**. Reading **`origin/main`** (or similar) for **`merge-base`** only resolves a commit; it does not mutate remote-tracking refs. |
| Other branches | Other **local** branches are **not** passed as the positive ref to **`filter-branch`**; they are not targets of this tool. |

## Scopes

| Mode | Revision range | Trailers (always via `--msg-filter` when in range) | Author / committer |
|------|----------------|---------------------------------------------------|---------------------|
| `all` | Default: **`merge-base(base, current)..current`**; if that range is empty, **all commits reachable from the branch**. With **`--full-branch`**: always **full ancestry** of the checked-out branch (same objects as `git rev-list <branch>`). | Every commit in range | If **`--message-only`**: unchanged from originals. If **not**: set via **`--env-filter`** to the four CLI identity fields for each commit in range. |
| `single` | `commit^..current` | Only the targeted original `GIT_COMMIT`; others pass through unchanged | If **`--message-only`**: unchanged. If **not**: only the targeted commit gets **`GIT_AUTHOR_*` / `GIT_COMMITTER_*`** overrides; others keep originals. |

`single` still **replays** descendants (new hashes) on **current** only.

## `--message-only` vs full rewrite

- **`--message-only`**: **`--msg-filter`** runs; **`--env-filter`** is **omitted**. Only the **message body** (including trailers) is normalized per commit; **author/committer Git fields** follow Git’s normal replay from the old commits.
- **Without `--message-only`**: both filters run; **messages** and **author/committer** are overwritten per the rules above.

## Why `--repo`, `--head`, and `--base` still exist

They are **optional** knobs, not “pick any repo/branch”:

- **`--repo`**: **`git -C`** path; **must** be the **same** repository root as cwd’s Git work tree (enforced).
- **`--head`**: Optional; must be **`HEAD`** or the **current** branch name if set — used for validation / symmetry with scripts, **not** to choose a different branch.
- **`--base`**: **`mode=all` only**; **read-only** input to **`merge-base`** to define the exclusive range; **ignored** when **`--full-branch`** is set. **Not** the branch being rewritten.

## History rewrite safety

- **Clean index/worktree (tracked files):** **`git filter-branch`** aborts with *You have unstaged changes* if tracked files differ from **`HEAD`** (staged or unstaged). Commit, **`git stash`**, or discard, then rerun. Untracked files alone are usually fine.
- **Publishing:** Updating a **remote** is **outside** this script; the user runs **`git push --force-with-lease`** (or equivalent) manually if they choose.
- **Backup refs:** `git filter-branch` leaves `refs/original/`. After verifying the result, remove them (Bash):

  `git for-each-ref --format="%(refname)" refs/original | xargs -n1 git update-ref -d`

- **Global git config:** The automation script does **not** change `user.name` / `user.email` globally. Identity is applied only inside `filter-branch` via `GIT_AUTHOR_*` / `GIT_COMMITTER_*`.

## Platform notes

- **Windows:** Use Git for Windows; `filter-branch` runs filters under `sh`. The revision argument after `--` uses **`merge-base..<current-branch-name>`** so Git gets a symbolic positive ref (avoids “You must specify a ref to rewrite” with peeled SHAs).
- **Payload quoting:** Trailers and identity are passed through **`MGM_*` environment variables** so characters like `<` in `Co-Authored-By` do not break the shell.
- **`git filter-repo`:** Prefer `git filter-repo` for large repos when installed; this skill ships `filter-branch` because it ships with Git.

## Edge cases

- **Root commit** (`single` on the first commit): `commit^` is invalid — use `all` from an empty tree or edit manually.
- **Empty `merge-base..HEAD`:** If the branch tip is exactly the merge-base with `--base` (e.g. same commit as `main`), the exclusive range is empty; the script **falls back** to rewriting the **full ancestry** of the checked-out branch and prints a note to stderr. If that count is still zero, the script exits with code **1** (nothing to rewrite).

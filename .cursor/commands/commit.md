---
description: >-
  Runs a Conventional Commits v1.0.0–compliant git commit: analyzes diff,
  stages, commits via -F temp file, confirms success—no manual shell paste.
---

# Conventional Commit Generator (v1.0.0 + repo footers)

You **must** produce a message that follows **[Conventional Commits 1.0.0](https://www.conventionalcommits.org/en/v1.0.0/)**, with **standard footers before** **`Made-with: Cursor`** and **`Co-authored-by: Cursor Agent <cursoragent@cursor.com>`** (see `.cursor/rules/git-commit-footer.mdc`), then **run `git commit` yourself**. **Do not** end with copy-paste shell for the user.

**User-facing reply (only):** on success, a single line **`Commit successful: <header>`** (the commit’s subject line, exact first line of the message). On failure, **`Commit failed: <concise reason>`** (include `git` stderr if short). If you **abort** without committing (e.g. unrelated changes warrant splitting), **`Commit aborted: <short reason>`** and optional one short line listing groupings — still **no** pasted commands.

**Do not** use multiple `git commit -m` flags; use **`git commit -F <file>`** once.

---

## Workflow

1. Run **`git status`** and **`git diff HEAD`** to inspect changes (staged + unstaged).
2. If changes are **unrelated** or **too large** for one logical commit, **do not commit**; reply with **`Commit aborted: …`** and brief split suggestions — stop.
3. Infer **`type`**, **`scope`**, **breaking** (and **`!`** + **`BREAKING CHANGE:`** when needed), and **issue references** (`#…`, `Closes` / `Fixes`, branch context).
4. Build the full message: **header**, **body** (motivation, context, **migration** when breaking or API-facing), **standard footers**, then **custom metadata** (exact spelling and blank lines per `.cursor/rules/git-commit-footer.mdc`).
5. **Validate** with the checklist below.
6. **Stage:** run **`git add .`** by default. Use **`git add <paths>`** only when the user explicitly scoped files, or when **`git add .`** would clearly include unrelated work; if uncertain, **`git add .`**.
7. Write the **entire message** to a **temporary UTF‑8 file** (unique name under the system temp dir or `mktemp`-style). **Do not** print the file path unless `Commit failed` needs it.
8. Run **`git commit -F <that-file>`**. **Do not** chain `-m` flags.
9. **Delete** the temp file (best effort, including after failure).
10. Reply **`Commit successful: <header>`** or **`Commit failed: …`**.

---

## Commit types (use one)

| Type | When to use |
|------|-------------|
| **feat** | New capability for the user or consumer of the API/library. |
| **fix** | A bug fix. |
| **docs** | Documentation only. |
| **style** | Whitespace, formatting; **no** semantic change. |
| **refactor** | Neither fixes a bug nor adds a feature (e.g. restructure, rename internals). |
| **perf** | Performance improvement. |
| **test** | Adding or fixing tests. |
| **build** | Build system or external **dependencies** (MSBuild, packages, bundler, etc.). |
| **ci** | CI config/scripts (e.g. GitHub Actions). |
| **chore** | Maintenance that does not fit **build**/**ci** (deps bump sometimes **chore** vs **build** — pick the closer fit). |
| **revert** | Reverts a prior commit. Prefer the Conventional **`revert:`** header form when appropriate (see **Revert** below). |
| **security** | Fixes for security vulnerabilities (**not** in the minimal CC type list, but valid here as an explicit extension). |

**`revert` header (Conventional Commits):** When the change is purely a git revert, the subject line may be:

```text
revert: <short description of what is reverted>

This reverts commit <full-or-abbreviated-hash>.
```

Use normal **`type(scope):`** form instead if you are describing a manual rollback in the same style as other commits.

---

## Breaking changes

- **Detection (heuristic):** Treat as breaking when diffs show removed public API, renamed/changed signatures, behavior incompatible with previous releases, removed config keys, semver-major dependency moves, or explicit `BREAKING`/`obsolete` notes in code/docs.
- **Header:** Append **`!`** immediately before **`:`** — e.g. **`feat(api)!: migrate to REST v2`** (with or without scope: **`feat!:`** is allowed).
- **Footer:** Add at least one line in the **standard footer** (before custom metadata):

  ```text
  BREAKING CHANGE: <what broke>. <migration / what callers must do>.
  ```

  Use **multiple** `BREAKING CHANGE:` lines if several independent breakages need separate migration notes. Wrap footer lines at **≤ 72** characters where reasonable.

---

## Scope

- **Any** lowercase scope is allowed: **`(api)`**, **`(desktop)`**, **`(auth)`**, etc. Omit **`()`** entirely if no scope fits: **`fix: patch buffer overflow`**.
- **Suggest** scope from paths (first match wins; combine only if one clear theme):

  | Path pattern | Suggested scope |
  |--------------|-----------------|
  | `docs/`, `*.md` | `docs` |
  | `**/*test*`, `**/tests/**`, `**/*.Tests/**` | `test` |
  | `.github/workflows/`, `.github/**` | `ci` |
  | `**/MimironsGoldOMatic.Shared/**` | `shared` |
  | Backend/API, `**/*Backend*/**`, `**/api/**` | `backend` or `api` |
  | WPF/Desktop | `desktop` |
  | WoW addon / Lua | `addon` |
  | Extension / React / Vite | `extension` |
  | Win32 integration | `winapi` |
  | Domain/aggregates | `domain` |
  | DB/migrations/DI infra | `infra` |

- Prefer **one** primary scope per commit; if multiple areas change equally, use the **dominant** path or **omit** scope.

---

## Description and body rules

- **Language:** English.
- **Header — imperative, specific:** e.g. *add OAuth2 authentication to desktop login* — not *add feature* or *update stuff*.
- **Header length:** **≤ 50 characters** (count the full first line: `type(scope)!: description`).
- **Body:** Explain **why** and **what** (and **how to migrate** when breaking or when config/API steps are required). Each body line **≤ 72 characters** (wrap prose; URLs may exceed if unavoidable).
- **Blank lines:** One blank line after the subject; one blank line between body and the **standard** footer block; one blank line before **`Made-with:`**.

---

## Footer order (mandatory)

1. **Standard footers** (optional): `BREAKING CHANGE:`, `Closes #123`, `Fixes #456`, `Refs #…`, `See abc1234…`, etc. — each as Conventional / Git trailer style, **before** custom metadata.
2. **Custom metadata (always last, fixed order):**

   ```text
   Made-with: Cursor

   Co-authored-by: Cursor Agent <cursoragent@cursor.com>
   ```

3. **No** extra lines after `Co-authored-by`.
4. Compose the **entire** message once and pass it to **`git commit -F`** (single file).

---

## Full message template

```text
<type>(<scope>)<!>: <description>

<body lines wrapped at ≤72 chars; motivation, context,
migration steps when needed>

<BREAKING CHANGE: ...>   (if breaking; before custom metadata)
<Closes #...>            (optional)
<Refs #...>              (optional)

Made-with: Cursor

Co-authored-by: Cursor Agent <cursoragent@cursor.com>
```

---

## Validation checklist (run before `git commit`)

- [ ] **Type** is one of the listed types (or valid **`revert:`** subject form).
- [ ] **Header** matches `<type>(<scope>):` or `<type>(<scope>)!:` or `type!:` / `type!` variants per spec; description is **imperative** and **≤ 50** chars for the **first line**.
- [ ] If **`!`** is present, **`BREAKING CHANGE:`** appears in the **standard** footer (before **`Made-with`**).
- [ ] **Body** adds context or motivation; **breaking** commits include **migration**.
- [ ] **Issue refs** (if any) sit in the standard footer, **before** custom metadata.
- [ ] **`Made-with`** then **`Co-authored-by`** at the **very end**, with correct blank lines.
- [ ] **Body/footer lines** wrapped at **≤ 72** where practical.
- [ ] Temp file is UTF‑8; **`git commit -F`** used once; temp file removed after attempt.

---

## Examples

**Breaking change + issue ref**

```text
feat(api)!: migrate to REST v2

Remove deprecated /v1/users and /v1/orders. Clients must call
/v2/users and /v2/orders. See upgrade guide in docs.

BREAKING CHANGE: /v1/* endpoints removed. Upgrade client libs to
v2.0.0 before deploying.
Closes #456

Made-with: Cursor

Co-authored-by: Cursor Agent <cursoragent@cursor.com>
```

**Fix with refs**

```text
fix(auth): resolve JWT expiration check

Compare expiry using strict UTC equality so tokens are not
dropped early on boundary times.

Fixes #789
See abc1234 for related audit logging

Made-with: Cursor

Co-authored-by: Cursor Agent <cursoragent@cursor.com>
```

---

## Implementation notes (agent; not for user copy-paste)

- **Unix / Git Bash:** e.g. create with `mktemp` (or `$TMPDIR`), write message with UTF‑8, `git commit -F "$file"`, `rm -f "$file"`.
- **Windows PowerShell:** e.g. `$f = Join-Path ([IO.Path]::GetTempPath()) ("mgm-commit-" + [Guid]::NewGuid().ToString() + ".txt")`; write body with **UTF‑8** (`Set-Content -Encoding utf8` or `.NET`/`Utf8Encoding(false)`); `git commit -F $f`; `Remove-Item -Force $f`.
- Prefer **no BOM** for the message file if your shell/API allows; UTF‑8 is required.
- If **`git commit`** fails (hook, empty commit, conflicts), report **`Commit failed:`** with the reason; still remove the temp file if possible.

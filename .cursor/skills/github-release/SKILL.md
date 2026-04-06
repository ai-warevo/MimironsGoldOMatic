---
name: github-release
description: >-
  Analyzes Conventional Commits to suggest a SemVer tag (vX.Y.Z), generates a
  Markdown changelog, saves a local release draft, and creates/pushes Git tags
  to trigger GitHub release CI. Use when you want an end-to-end, automated
  release cut.
---

# GitHub release (local tag + CI)

## Script

Run from the repository root:

```bash
node .cursor/skills/github-release/scripts/release-skill.js
```

Non-interactive (explicit version + message):

```bash
node .cursor/skills/github-release/scripts/release-skill.js \
  --version=1.2.3 \
  --message="Release v1.2.3" \
  --open-browser
```

Interactive auto-versioning and changelog:

```bash
node .cursor/skills/github-release/scripts/release-skill.js
```

Dry run with changelog preview (no tags created):

```bash
node .cursor/skills/github-release/scripts/release-skill.js --dry-run
```

Overwrite an existing tag locally:

```bash
node .cursor/skills/github-release/scripts/release-skill.js --force
```

Flags (backwards compatible):

- `--version=`: explicit SemVer `MAJOR.MINOR.PATCH` (overrides auto-suggestion).
- `--message=`: tag annotation message (default: `Release vX.Y.Z`).
- `--no-push`: create the tag locally but do not push to `origin`.
- `--open-browser`: open the GitHub Releases page for the new tag.
- `--dry-run`: show analysis, suggested version, changelog, and planned commands
  without creating or pushing any tags.
- `--force`: overwrite an existing tag (`git tag -d` then re-create); in dry-run
  mode, logs that the tag would be overwritten.

## Behavior

- **Conventional Commits analysis**:
  - Scans commits since the last tag (`vX.Y.Z`) or from the beginning of
    history if no tags exist.
  - Classifies commits by type: `feat`, `fix`, `perf`, `refactor`, `docs`,
    `chore`, `build`, `ci`, `revert`, `security`, `other`.
  - Detects breaking changes by:
    - `!` in the header (e.g. `feat(api)!: ...`).
    - `BREAKING CHANGE:` footer lines in the commit body.

- **Automatic SemVer determination**:
  - Uses the last SemVer tag as a base (or `0.0.0` if none).
  - If breaking changes are present → increments **MAJOR**.
  - Else if there are `feat` commits → increments **MINOR**.
  - Otherwise (fixes, docs, chore, etc.) → increments **PATCH**.
  - If `--version=` is provided, it is validated and used verbatim instead of
    the suggestion.

- **Changelog generation**:
  - Builds a Markdown changelog with sections:
    - `### Breaking Changes`
    - `### Features`
    - `### Bug Fixes`
    - `### Other Changes`
  - Each bullet includes the description, short commit hash, and any linked
    issues (`Closes #123`, `Fixes #456`, `Refs #789`).
  - Adds a `[Full Changelog]: https://github.com/.../compare/vX.Y.Z...vX.Y.Z`
    section when a previous tag exists.
  - Wraps lines to stay within ≈72 characters where practical.

- **Local release history**:
  - Writes the full changelog to
    `.cursor/skills/github-release/history/YYYY-MM-DD-vX.Y.Z.md`.
  - The file includes the version, date, all sections, and the full-changelog
    comparison link (when available).

- **Confirmation before tag creation**:
  - Shows:
    - Proposed version.
    - Full changelog preview.
    - Planned `git tag` and `git push` commands.
  - Asks: `Create this release? [y/N]` when running in an interactive TTY.
  - In non-interactive environments, proceeds without a prompt (useful for CI).

- **Error handling hints**:
  - **Tag already exists**:
    - Message explains that `--force` can be used to overwrite or that a new
      version should be chosen.
  - **Not a Git repository**:
    - Message explains that the script must run from the root of a cloned Git
      repository and surfaces `git rev-parse` errors.
  - **Push rejected**:
    - Message suggests verifying remote permissions and that `origin` exists,
      and includes `git`’s stderr/stdout for debugging.
  - **No commits since last tag**:
    - Message explains that there are no new commits to release and advises
      committing changes first.

## Agent workflow

1. Ensure the working tree is clean enough for the user’s policy (optional
   `git status`).
2. Run the script. For interactive releases, review the suggested version,
   changelog preview, and planned commands, then confirm or abort.
3. On failure, surface stderr/stdout plus the script’s hint text (tag exists,
   not a repo, push rejected, no commits to release).
4. After a successful push, CI runs `.github/workflows/release.yml` on the
   `v*` tag and publishes the GitHub Release with artifacts.

## Output & Artifacts

- Git tag: `vX.Y.Z` (annotated).
- Optional: pushed tag on `origin` (`git push origin vX.Y.Z`).
- Optional: opened GitHub Releases page for `vX.Y.Z`.
- Local changelog draft:
  - Path: `.cursor/skills/github-release/history/YYYY-MM-DD-vX.Y.Z.md`.
  - Content:
    - `## vX.Y.Z - YYYY-MM-DD`
    - `### Breaking Changes` / `### Features` / `### Bug Fixes` /
      `### Other Changes`
    - `[Full Changelog]: https://github.com/.../compare/vPrev...vX.Y.Z`

## Prerequisites

- Node.js (for the script).
- Git CLI available on `PATH`.
- `origin` remote pointing at GitHub (for compare links and tag push).

---
name: github-release
description: >-
  Creates SemVer Git tags (vX.Y.Z), pushes to GitHub to trigger release CI, and
  documents the local Node script. Use when the user wants to cut a release,
  tag a version, or automate GitHub Releases from tags.
---

# GitHub release (local tag + CI)

## Script

Run from the repository root:

```bash
node .cursor/skills/github-release/scripts/release-skill.js
```

Non-interactive:

```bash
node .cursor/skills/github-release/scripts/release-skill.js --version=1.2.3 --open-browser
```

Flags: `--version=`, `--message=`, `--no-push`, `--open-browser`, `--dry-run`.

## Agent workflow

1. Ensure the working tree is clean enough for the user’s policy (optional `git status`).
2. Run the script; on failure, surface stderr and suggest fixes (tag exists, not a repo, push rejected).
3. After push, CI runs `.github/workflows/release.yml` on the `v*` tag and publishes the GitHub Release with artifacts.

## Prerequisites

- Node.js (for the script), Git CLI, `origin` remote pointing at GitHub.

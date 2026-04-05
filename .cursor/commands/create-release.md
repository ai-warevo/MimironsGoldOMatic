---
description: Create and push a new Git tag to trigger a GitHub release
---

# Create Release

Use the project’s release script so the version is validated and the annotated tag is pushed to GitHub (triggers `.github/workflows/release.yml` on `v*` tags).

## Steps

1. **Version**: Ask the user for **MAJOR.MINOR.PATCH** (SemVer), e.g. `1.2.3` (optional leading `v` is stripped by the script).
2. **Run** from the repo root:

   ```bash
   node .cursor/skills/github-release/scripts/release-skill.js --open-browser
   ```

   Or pass the version non-interactively:

   ```bash
   node .cursor/skills/github-release/scripts/release-skill.js --version=1.2.3 --open-browser
   ```

3. **Optional**: `--message="Release notes one-liner"` for the annotated tag message; `--no-push` to only create the local tag; `--dry-run` to print commands without running.

## Example terminal output

```text
[release] Repository: G:/devnull/ai-warevo/MimironsGoldOMatic
[release] Creating annotated tag v1.2.3…
[release] Pushing tag to origin…
[release] Pushed v1.2.3 to origin.
[release] Opening https://github.com/OWNER/REPO/releases/tag/v1.2.3
```

## Errors

- **Invalid SemVer**: User must supply `x.y.z` digits only (after optional `v`).
- **Tag already exists**: Suggest `git tag -d vX.Y.Z` or a higher version.
- **Push failed**: Check auth (`gh auth login` or SSH), network, and branch protection rules for tags.

## See also

- Skill: `.cursor/skills/github-release/SKILL.md`
- Workflow: `.github/workflows/release.yml`

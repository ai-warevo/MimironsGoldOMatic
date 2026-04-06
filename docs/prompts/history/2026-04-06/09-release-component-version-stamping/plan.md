## Goal
Ensure every release artifact is stamped with the same release version derived from tag/dispatch input.

## Approach
1. Update `.github/workflows/release.yml` version resolution to export:
   - `RELEASE_VERSION` (e.g., `v1.2.3`)
   - `RELEASE_VERSION_STRIPPED` (e.g., `1.2.3`)
2. Apply `RELEASE_VERSION_STRIPPED` to each component build:
   - Desktop (.NET): pass `/p:Version`, `/p:AssemblyVersion`, `/p:FileVersion`, `/p:InformationalVersion`.
   - Twitch Extension: set `VITE_APP_VERSION` during build.
   - WoW Addon: inject `## Version: ...` into staged `.toc` before zipping.
3. Add lightweight verification output in build jobs to show effective stamped versions.

## Risks
- Workflow syntax regressions from repeated step edits.
- TOC version insertion must avoid modifying source tree; stage-only edit mitigates this.

## Verification
- Validate workflow YAML parses (via CI run).
- Manually inspect artifact README/build logs for version evidence.


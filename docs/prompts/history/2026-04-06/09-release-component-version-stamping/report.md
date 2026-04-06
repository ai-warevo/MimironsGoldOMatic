## Task outcome

Implemented component version stamping in release automation by updating `.github/workflows/release.yml` to derive and propagate both tag-form and stripped semantic versions to build steps.

## Modified files

- `.github/workflows/release.yml`
- `docs/prompts/history/2026-04-06/09-release-component-version-stamping/prompt.md`
- `docs/prompts/history/2026-04-06/09-release-component-version-stamping/plan.md`
- `docs/prompts/history/2026-04-06/09-release-component-version-stamping/checks.md`

## Verification results

- Reviewed workflow diff to confirm all release jobs now export:
  - `RELEASE_VERSION` (e.g., `v1.2.3`)
  - `RELEASE_VERSION_STRIPPED` (e.g., `1.2.3`)
- Confirmed Desktop publish now includes:
  - `/p:Version`
  - `/p:AssemblyVersion`
  - `/p:FileVersion`
  - `/p:InformationalVersion`
- Confirmed Twitch Extension build now receives:
  - `VITE_APP_VERSION=${RELEASE_VERSION_STRIPPED}`
- Confirmed WoW addon pack step now injects or updates:
  - `## Version: ${RELEASE_VERSION_STRIPPED}` in staged `.toc`
- Added per-job verification log steps for Desktop, Extension, and Addon.

## Definition of Done check

- [x] Scope implemented for release workflow version propagation.
- [ ] Behavior covered by tests.
  Workflow changes require CI execution in GitHub Actions for runtime validation.
- [ ] Existing tests pass (`dotnet test src/MimironsGoldOMatic.sln`) after changes.
  Not run; this change is workflow-only.
- [x] No linter/format issues introduced in changed files.
- [x] Error handling considered in addon TOC write path (tmp file + move).
- [x] Risks and rollback impact documented in `plan.md`.
- [x] `Potential technical debt` section included below.

## Potential technical debt

- `Resolve RELEASE_VERSION` logic is still duplicated across jobs; extracting to a reusable workflow or composite action would reduce drift risk.
- `release-skill.js` is not yet emitting a machine-readable release manifest for downstream automation; this remains optional future enhancement.
- Full confidence requires a real tagged/workflow_dispatch release run in GitHub Actions to observe end-to-end artifact metadata.


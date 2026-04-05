# Plan

## Architecture

- **PR (`e2e-test.yml`):** Scoped `dotnet build` for Shared + Backend + Tier A mocks; same Postgres service, same background processes, same Python + curl verification steps.
- **Release (`release.yml`):** Four parallel jobs (Desktop on `windows-latest`, addon + Extension + Docker on `ubuntu-latest`), then `create-release` with `needs:` all four. Version: `workflow_dispatch` optional semver input, else `v0.0.<run_number>` on `main` pushes.
- **Backend container:** Add `src/MimironsGoldOMatic.Backend/Dockerfile` with build context `src/`.

## Docs

- Insert **CI/CD Pipeline Architecture** into `docs/e2e/E2E_AUTOMATION_PLAN.md` (triggers, jobs, artifacts, diagram, benefits, discussion hooks).
- Extend `docs/e2e/E2E_AUTOMATION_TASKS.md` checklist + validation tasks V3–V7.

## Risks

- GHCR permissions and lowercase owner name for image repository.
- `softprops/action-gh-release` tag collisions on rerun with same `run_number` (expected: rare / use dispatch version).
- GitHub heading anchor for internal links — align with GitHub slug for `CI/CD`.

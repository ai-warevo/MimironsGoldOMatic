# Plan: Project structure docs + Tier B finalization

## Architecture / scope

- Refresh `docs/reference/PROJECT_STRUCTURE.md` with current `src/` layout (`Mocks/`, `Tests/` with IntegrationTesting + per-component tests), `.github/workflows`, and an **old path → new path** mapping table.
- Extend `docs/e2e/E2E_AUTOMATION_PLAN.md` with Tier B success report, Tier C section, troubleshooting rows, and CI optimization notes aligned with workflow changes.
- Optimize `.github/workflows/e2e-test.yml`: NuGet cache key including `packages.lock.json`, pip cache, service log capture, artifacts on `always()`, concurrency; document why multi-job parallel E2E is not used (shared Postgres/ports).
- Add `docs/e2e/TIER_C_REQUIREMENTS.md`, `docs/e2e/TIER_B_TEAM_ANNOUNCEMENT.md`; update post-launch verification checkboxes; align backend/desktop/wow READMEs; update `docs/ReadME.md` navigation.
- Update `docs/reference/IMPLEMENTATION_READINESS.md` MVP-6 row for Tier B CI slice.

## Risks

- No `packages.lock.json` in repo yet: cache key falls back to csproj hash (documented).
- Large E2E doc edits: preserve anchors and internal links.

## Files

- `docs/reference/PROJECT_STRUCTURE.md`, `docs/e2e/E2E_AUTOMATION_PLAN.md`, `docs/e2e/TIER_B_POSTLAUNCH_VERIFICATION.md`, `docs/e2e/TIER_C_REQUIREMENTS.md`, `docs/e2e/TIER_B_TEAM_ANNOUNCEMENT.md`
- `.github/workflows/e2e-test.yml`
- `docs/components/backend/ReadME.md`, `docs/components/desktop/ReadME.md`, `docs/components/wow-addon/ReadME.md`
- `docs/ReadME.md`, `docs/reference/IMPLEMENTATION_READINESS.md`

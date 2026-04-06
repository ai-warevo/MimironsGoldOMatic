# Task 05 - CI Workflow Hardening for Production

Acting as **[DevOps Engineer]** and **[Backend/API Expert]**.

## Goal
Harden CI workflows and failure diagnostics for production confidence.

## Read first
- `docs/overview/ROADMAP.md` (Production milestone)
- `.github/workflows/unit-integration-tests.yml`
- `.github/workflows/e2e-test.yml`
- release/monitoring workflows
- `docs/prod/PROD_GO_NO_GO.md`

## Implement
1. Harden flaky or under-instrumented workflow steps.
2. Add deterministic health checks/artifact capture for failure triage.
3. Validate protected-branch quality gates and release workflow dependencies.
4. Update runbooks for recurring failure classes.

## Acceptance criteria
- Representative recent runs are stable and diagnosable.
- Failure output identifies component/step ownership clearly.
- Runbooks include clear recovery/escalation actions.

## Output
- Workflow updates and supporting scripts/docs.
- Create `docs/prod/PROD_CI_HARDENING_REPORT.md`.
- Update `docs/prod/PROD_GO_NO_GO.md` evidence.

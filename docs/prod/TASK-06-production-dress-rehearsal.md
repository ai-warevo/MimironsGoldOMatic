# Task 06 - Production Dress Rehearsal

Acting as **[Senior Architect]** and **[QA Lead]**.

## Goal
Run a production-like end-to-end rehearsal and capture operational evidence.

## Read first
- `docs/overview/INTERACTION_SCENARIOS.md` (SC-001, SC-005)
- `docs/prod/PROD_GO_NO_GO.md`
- `docs/setup/SETUP.md`

## Execute
1. Run full scenario flow in production-like environment/settings.
2. Capture timestamps, system states, and pass/fail checkpoints.
3. Validate alerting, diagnostics, and operator recovery paths.
4. Record defects and residual risk with owners.

## Scope boundary
- Consume CI hardening outputs from `docs/prod/TASK-05-ci-workflow-hardening.md`; do not re-open CI workflow design in this task.
- Focus on runtime/operator validation and evidence quality under production-like conditions.

## Acceptance criteria
- Full rehearsal has traceable evidence and clear outcomes.
- Incident/recovery behavior is exercised at least once.
- Residual risks are documented with mitigation plan.

## Output
- Create `docs/prod/PROD_DRY_RUN_REPORT.md`.
- Update gate evidence in `docs/prod/PROD_GO_NO_GO.md`.

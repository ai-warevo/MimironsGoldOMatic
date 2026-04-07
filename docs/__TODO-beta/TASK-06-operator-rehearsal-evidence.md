# Task 06 - Operator Rehearsal Preflight and Evidence Template

Acting as **[Senior Architect]**.

## Goal
Prepare rehearsal inputs, checkpoints, and evidence template so the formal dry run can execute cleanly.

## Read first
- `docs/overview/INTERACTION_SCENARIOS.md` (SC-001, SC-005 + TC rows)
- `docs/overview/SPEC.md`
- `docs/beta/BETA_GO_NO_GO.md`
- `docs/setup/SETUP.md`

## Execute
1. Build the rehearsal execution checklist from SC/TC rows for SC-001 and SC-005.
2. Predefine evidence capture locations (logs, screenshots, workflow links, timestamps).
3. Validate preconditions (env, secrets, operator roles, observability paths).
4. Document known risks and expected fallback paths before live execution.

## Acceptance criteria
- Dry-run checklist and evidence template are ready for execution handoff.
- Preconditions and expected failure-handling paths are documented.

## Output
- Create `docs/beta/BETA_OPERATOR_REHEARSAL_PLAN.md`.
- Update evidence checklist in `docs/beta/BETA_GO_NO_GO.md`.
- Note: formal execution report is produced by `docs/beta/TASK-15-beta-dress-rehearsal.md`.

# Task 11 - Windows Real-Stack E2E Harness

Acting as **[DevOps Engineer]** and **[WPF/WinAPI Expert]**.

## Goal
Implement reproducible Windows self-hosted E2E path for Desktop + WoW log-driven validation.

## Read first
- `docs/e2e/E2E_AUTOMATION_PLAN.md`
- `docs/e2e/E2E_AUTOMATION_TASKS.md`
- `docs/overview/INTERACTION_SCENARIOS.md`
- `.github/workflows/e2e-test.yml`

## Implement
1. Define runner prerequisites (Windows host, WoW client/log path, secrets, backend URL).
2. Add scripts/workflow steps to execute real-stack flow checks on Windows self-hosted runner.
3. Capture artifacts/log bundles for Desktop, backend, and scenario evidence.
4. Document deterministic execution and rerun procedure.

## Acceptance criteria
- One-command or one-workflow execution path is documented and repeatable.
- Evidence artifacts are attached for pass/fail runs.
- Failures are diagnosable from collected logs without manual guesswork.

## Output
- Add/update scripts under repository automation paths.
- Update E2E docs with Windows self-hosted runner instructions.
- Link evidence expectations from `docs/beta/BETA_GO_NO_GO.md`.

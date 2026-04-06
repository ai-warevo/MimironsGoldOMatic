# Task 05 - CI E2E Stability Hardening

Acting as **[Backend/API Expert]** and **[DevOps Engineer]**.

## Goal
Stabilize Beta confidence in Tier A+B CI (`e2e-test.yml`) and reduce flake impact.

## Read first
- `docs/e2e/E2E_AUTOMATION_PLAN.md`
- `docs/e2e/E2E_AUTOMATION_TASKS.md`
- `docs/e2e/TIER_B_MAINTENANCE_CHECKLIST.md`
- `docs/overview/INTERACTION_SCENARIOS.md`

## Implement
1. Analyze latest failures and top flaky steps.
2. Improve deterministic waits/health checks/log capture where needed.
3. Add or tighten failure diagnostics for faster triage.
4. Update maintenance checklist with concrete runbook for recurring failures.

## Acceptance criteria
- Last 10 runs achieve >= 90% success.
- Last 5 runs contain no unexplained failures.
- Failure output points directly to failing component path.

## Output
- Workflow/script updates.
- Updated maintenance docs and runbook steps.
- Evidence links for success-rate claim.

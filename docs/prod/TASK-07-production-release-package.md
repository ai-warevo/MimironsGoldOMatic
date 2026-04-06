# Task 07 - Production Release Package

Acting as **[Senior Architect]** and **[DevOps Engineer]**.

## Goal
Prepare production release checklist and operator runbook with rollback/incident playbooks.

## Read first
- `docs/prod/PROD_GO_NO_GO.md`
- `docs/setup/SETUP.md`
- CI/release workflows under `.github/workflows/`

## Execute
1. Define release checklist for backend, desktop, extension, and addon artifacts.
2. Define production operator runbook (start, health checks, rollback, incident response).
3. Include communication/escalation protocol and ownership matrix.
4. Validate checklist/runbook against rehearsal evidence.

## Acceptance criteria
- Checklist and runbook are executable and unambiguous.
- Rollback and incident playbooks include clear triggers and commands.
- Artifacts are linked from production gate evidence.

## Output
- Create `docs/prod/PROD_RELEASE_CHECKLIST.md`.
- Create `docs/prod/PROD_OPERATOR_RUNBOOK.md`.
- Update `docs/prod/PROD_GO_NO_GO.md`.

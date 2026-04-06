# Task 02 - Extension Secret Rotation Runbooks

Acting as **[Backend/API Expert]** and **[DevOps Engineer]**.

## Goal
Create and validate Twitch Extension secret rotation runbooks suitable for Production operations.

## Read first
- `docs/overview/ROADMAP.md` (Production milestone)
- `docs/setup/SETUP.md`
- `docs/prod/PROD_GO_NO_GO.md`

## Execute
1. Define standard rotation procedure (planned rotation).
2. Define emergency rotation/reset procedure (incident response).
3. Include rollback strategy and validation checkpoints.
4. Rehearse each path and capture timing/evidence.

## Acceptance criteria
- Runbooks are executable by operator without tribal knowledge.
- Both planned and emergency rotation paths are tested.
- Rollback and post-rotation verification steps are explicit.

## Output
- Create `docs/prod/PROD_EXTENSION_SECRET_ROTATION_RUNBOOK.md`.
- Update `docs/setup/SETUP.md` with references.
- Add evidence links in `docs/prod/PROD_GO_NO_GO.md`.

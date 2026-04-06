# Task 03 - Secrets and Configuration Hardening

Acting as **[Backend/API Expert]** and **[DevOps Engineer]**.

## Goal
Harden secrets and environment configuration boundaries across development, beta, and production.

## Read first
- `docs/overview/ROADMAP.md` (Production milestone)
- `docs/setup/SETUP.md`
- backend/desktop config docs
- `docs/prod/PROD_GO_NO_GO.md`

## Implement
1. Audit config sources for secret sprawl and unsafe defaults.
2. Enforce environment-specific configuration boundaries and precedence.
3. Add validation/startup checks for missing critical Production secrets.
4. Document operator-safe secret handling and rotation cadence.

## Scope boundary
- Do not duplicate Twitch Extension secret rotation procedure work from `docs/prod/TASK-02-extension-secret-rotation-runbooks.md`.
- Focus this task on non-Extension secrets, config boundaries, and fail-fast validation posture.

## Acceptance criteria
- No critical secret exposure paths remain in repo/workflows/logging.
- Production startup fails fast on missing required security settings.
- Config ownership and lifecycle are documented.

## Output
- Code/config/workflow updates as needed.
- Create `docs/prod/PROD_SECRETS_CONFIG_HARDENING.md`.
- Update evidence checklist in `docs/prod/PROD_GO_NO_GO.md`.

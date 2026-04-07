# Task 04 - API Key Rotation and Reset

Acting as **[Backend/API Expert]** and **[WPF/WinAPI Expert]**.

## Goal
Deliver a lightweight Beta story for rotating/resetting Desktop API key (`Mgm:ApiKey`) safely.

## Read first
- `docs/overview/ROADMAP.md` (Beta section)
- `docs/components/backend/ReadME.md`
- `docs/components/desktop/ReadME.md`
- `docs/setup/SETUP.md`

## Implement
1. Define key rotation procedure (old/new overlap or coordinated cutover).
2. Implement minimal backend and desktop support needed for no-downtime or low-risk switch.
3. Add operator-facing instructions and troubleshooting.
4. Ensure key is never logged in plaintext.

## Acceptance criteria
- Rotation/reset can be executed by one operator end-to-end.
- Failed auth after rotation has clear recovery path.
- Docs provide exact steps with verification commands.

## Output
- Config/code updates (if needed) and full runbook section in setup docs.

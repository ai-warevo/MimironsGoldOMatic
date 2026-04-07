# Task 01 - Twitch JWT Issuer Hardening

Acting as **[Backend/API Expert]**.

## Goal
Harden Production JWT validation for Twitch Extension traffic, including issuer checks.

## Read first
- `docs/overview/ROADMAP.md` (Production milestone)
- `docs/overview/SPEC.md`
- `docs/components/backend/ReadME.md`
- `docs/prod/PROD_GO_NO_GO.md`

## Implement
1. Add issuer validation rules for Production JWT verification path.
2. Keep Development behavior explicit and isolated from Production policy.
3. Add/update automated tests for positive and negative issuer cases.
4. Add diagnostics for JWT failures without exposing token secrets.

## Acceptance criteria
- Production JWT path rejects tokens with invalid/missing issuer.
- Automated tests cover issuer validation behavior.
- Documentation clearly separates dev/test/prod auth posture.

## Output
- Backend code + tests.
- Updated backend docs with issuer validation notes.
- Evidence links in `docs/prod/PROD_GO_NO_GO.md`.

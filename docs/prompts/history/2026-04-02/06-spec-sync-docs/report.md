## Summary

Updated “live” project documentation to reflect the finalized MVP specification agreed in the latest conversation:

- Fixed 1,000g redemptions, 10,000g lifetime cap, one-active-per-user concurrency, and rate limiting expectations.
- Added idempotency via `TwitchTransactionId` (unique constraint) and clarified identity fields (`TwitchUserId` + `TwitchDisplayName`).
- Expanded payout lifecycle statuses to include `Cancelled` and `Expired`, plus hourly expiration job behavior.
- Clarified Desktop explicit-claim flow (`Pending` -> `InProgress` on Sync/Inject), confirmation via `[MGM_CONFIRM:UUID]`, and manual “Mark as Sent”.
- Documented MVP security posture (Dev Rig first; Desktop uses pre-shared `ApiKey`) and MVP WoW targeting (foreground `WoW.exe`).
- Added Twitch Extension pull endpoint `/api/payouts/my-last`.

## Modified files

- `README.md`
- `CONTEXT.md`
- `docs/ReadME.md`
- `docs/components/shared/ReadME.md`
- `docs/components/backend/ReadME.md`
- `docs/components/desktop/ReadME.md`
- `docs/components/wow-addon/ReadME.md`
- `docs/components/twitch-extension/ReadME.md`

## Verification

- Documentation consistency pass: statuses, confirmation tag format, and endpoint names are aligned across docs.
- No code changes; test execution not applicable.

## Potential technical debt

- Endpoint semantics for `/api/payouts/pending` (whether it includes `InProgress` for visibility) are documented at a high level; implementation should lock this down in the API contract once the backend is built.
- “Authentication (phased)” is documented; production Twitch JWT verification details remain intentionally deferred to a roadmap milestone.


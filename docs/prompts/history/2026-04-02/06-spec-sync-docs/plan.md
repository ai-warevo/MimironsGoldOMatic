## Goal

Synchronize “live” documentation (outside `docs/prompts/`) to reflect the finalized MVP spec for Mimiron's Gold-o-Matic.

## Scope / files to update

- `README.md`
- `CONTEXT.md`
- `docs/ReadME.md`
- `docs/components/shared/ReadME.md`
- `docs/components/backend/ReadME.md`
- `docs/components/desktop/ReadME.md`
- `docs/components/wow-addon/ReadME.md`
- `docs/components/twitch-extension/ReadME.md`

## Key spec deltas to document

- MVP gold economics: 1,000g fixed per redemption; 10,000g lifetime cap per Twitch user.
- Concurrency: one active payout per Twitch user (must be terminal before new claim).
- Idempotency: `TwitchTransactionId` (Twitch redemption unique id) stored and enforced unique in DB.
- Identity storage: store `TwitchUserId` (logic) + `TwitchDisplayName` (UX) + `CharacterName`.
- Status enum: `Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired`.
- Expiration: background job hourly expires `Pending/InProgress` older than 24h; no reactivation.
- Auth phases:
  - Phase 1: Dev Rig debugging focus; production JWT validation deferred.
  - Desktop-to-Backend: pre-shared `ApiKey` (global static key in backend config).
- Desktop workflow: explicit claim (GET pending list; PATCH to `InProgress` only when streamer clicks Sync/Inject).
- Confirmation: parse `Logs\WoWChatLog.txt` for `[MGM_CONFIRM:UUID]`; desktop includes manual “Mark as Sent”.
- WoW targeting: MVP uses foreground `WoW.exe`; multi-process picker deferred.

## Risks

- Docs drifting from future implementation details; keep MVP scope explicit and call out “Roadmap” items.
- Ensure endpoints listed match across docs.

## Verification

- Consistency pass: same status list, same endpoints, same confirm tag pattern across all docs.
- No code changes; tests not applicable.


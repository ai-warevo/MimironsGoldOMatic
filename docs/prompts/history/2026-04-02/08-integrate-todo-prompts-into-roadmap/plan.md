## Goal

Make `docs/ROADMAP.md` the single canonical location for phased MVP implementation prompts.

## Plan

- Read `docs/prompts/todo/*.md` to capture the existing phased prompts (Shared, Addon, Backend, Desktop, TwitchExtension, Finalization).
- Update `docs/ROADMAP.md` by adding a **Step-by-step prompt** block under each corresponding MVP section:
  - Phase 1 -> MVP-1 (Shared)
  - Phase 2 -> MVP-3 (WoW Addon)
  - Phase 3 -> MVP-2 (Backend)
  - Phase 4 -> MVP-4 (Desktop)
  - Phase 5 -> MVP-5 (Twitch Extension)
  - Phase 6 -> MVP-6 (Finalization & solution sync)
- While integrating, **align prompt text** with the finalized MVP spec:
  - statuses include `Cancelled` + `Expired`
  - idempotency via `TwitchTransactionId`
  - `GET /api/payouts/my-last`
  - Desktop `ApiKey`
  - explicit claim flow (`Pending` -> `InProgress` on Sync/Inject)
- Remove references to `docs/prompts/todo/` from “live docs” (e.g. root `README.md`).
- Delete `docs/prompts/todo/*.md` so roadmap is the sole source.

## Verification

- Ensure no “live docs” link points to deleted `docs/prompts/todo/`.
- Quick grep for `prompts/todo` should only find historical artifacts under `docs/prompts/history/...`.


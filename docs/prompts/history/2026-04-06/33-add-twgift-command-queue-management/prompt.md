## Original user request

Add `!twgift` command with server-side queue management across Backend (EBS), WoW Addon, Desktop app, and Twitch Extension.

Key constraints included:
- Subscriber-only access with one-time usage per viewer per streamer.
- Streamer-scoped persisted queue in PostgreSQL/Marten with single active processing slot.
- State machine: `Pending -> SelectingItem -> ItemSelected -> WaitingConfirmation -> Completed` (+ `Failed` from any state).
- New EBS endpoints for queue/status/update/select/confirm.
- Timeouts: 60 seconds (item selection) and 5 minutes (confirmation).
- Desktop-driven roulette over WoW inventory items returned by addon `GetAllItems()`.
- Recipient in-game confirmation via `!twgift` before mail send.
- Error handling/logging for empty inventory, timeout, and failures.
- Documentation updates in `docs/overview/SPEC.md` and `docs/reference/UI_SPEC.md`.

## Request

Update project documentation to reflect the finalized MVP specification from the latest conversation:

- Economics / anti-abuse rules
- Recipient identification constraints
- Auth/security phases (Dev Rig first, Desktop ApiKey)
- Payout lifecycle statuses (including Cancelled + Expired)
- Feedback loop (chat log confirm + manual override)
- Process targeting behavior (foreground WoW.exe for MVP)
- Idempotency via TwitchTransactionId
- Backend background job for Expired
- Explicit claim flow (Pending -> InProgress on Sync/Inject)
- Twitch Extension pull endpoint for status (`/api/payouts/my-last`)


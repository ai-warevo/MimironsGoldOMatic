## MimironsGoldOMatic.Shared (.NET 10)

- **Role:** Single source of truth for data structures used by both the Backend and the Desktop App.
- **Stack:** .NET 10 Class Library.

## Core Entities

- **PayoutStatus (Enum):**
  - `Pending` (Initial)
  - `InProgress` (Explicitly claimed by Desktop when streamer clicks Sync/Inject)
  - `Sent` (Confirmed in-game or manually marked)
  - `Failed` (Error occurred)
  - `Cancelled` (Streamer cancelled)
  - `Expired` (Older than 24h; closed permanently)
- **PayoutDto (Record):**
  - `Guid Id`
  - `string TwitchUserId` (logic: limits, concurrency)
  - `string TwitchDisplayName` (UX: shown in Desktop)
  - `string CharacterName`
  - `long GoldAmount` (MVP fixed at 1,000g)
  - `string TwitchTransactionId` (idempotency: unique per Twitch redemption)
  - `PayoutStatus Status`
  - `DateTime CreatedAt`
- **CreatePayoutRequest (Record):** Used by Twitch Extension to initiate a claim:
  - `string CharacterName`
  - `string TwitchTransactionId`

## Validation / Logic

Contains shared validation (e.g., CharacterName regex). MVP business rules like fixed gold amount, lifetime caps,
and concurrency limits are enforced by the Backend.

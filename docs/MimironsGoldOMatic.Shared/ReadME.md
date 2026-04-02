## MimironsGoldOMatic.Shared (.NET 10)

- **Role:** Single source of truth for data structures used by both the Backend and the Desktop App.
- **Stack:** .NET 10 Class Library.

## Core Entities

- **PayoutStatus (Enum):** `Pending` (Initial), `InProgress` (Fetched by Desktop), `Sent` (Confirmed in-game), `Failed` (Error occurred).
- **PayoutDto (Record):** `Guid Id`, `string TwitchUser`, `string CharacterName`, `long GoldAmount`, `PayoutStatus Status`, `DateTime CreatedAt`.
- **CreatePayoutRequest (Record):** Used by Twitch Extension to initiate a claim.

## Validation / Logic

Contains shared validation (e.g., CharacterName regex, minimum/maximum gold limits).

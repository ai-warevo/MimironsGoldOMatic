## MimironsGoldOMatic.TwitchExtension (React)

- **Role:** Viewer-facing interface for redeeming points.
- **Stack:** React 18+, Vite, TypeScript, Tailwind CSS.

## Key Functions

- **Form:** Input for "In-game Character Name".
- **Validation:** Prevents submission if the name is empty or invalid.
- **Twitch Integration:** Uses `window.Twitch.ext` to access the viewer's ID and context.
- **API Interaction:**
  - **POST** `/api/payouts/claim` with `CharacterName` and `TwitchTransactionId` (Twitch redemption id) for idempotency.
  - **GET** `/api/payouts/my-last` (pull model) to show the viewer the latest payout status.

## Libraries

- `axios` (for API calls)
- `@twitch-ext/developer-rig` (for testing)

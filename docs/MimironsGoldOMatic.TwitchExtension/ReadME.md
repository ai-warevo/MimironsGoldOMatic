## MimironsGoldOMatic.TwitchExtension (React | Bridge between twitch & backend)

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
- `Zustand` (for State Management)

## Architecture & Patterns
- **Repository Pattern:**
  Implement `IPayoutRepository` (e.g., `TwitchPayoutRepository`). UI components must be agnostic of the communication layer (Fetch/Axios) and JWT injection. Use this abstraction to easily swap between Real API and Mock Data (for Dev Rig testing).
  
- **Store Pattern (State Management):**
  Use **Zustand** to maintain a global state. Track the status of the user's latest claim. If the viewer navigates away or refreshes the extension, the "Request in Progress" state must persist.

- **Error Boundary:**
  Wrap the main form in a React Error Boundary. If the Twitch API or Backend fails, display a themed "Gnomish machinery has jammed!" error message instead of a white screen.

- **Debounce / Throttle:**
  Apply a debounce to the "Redeem" button. Ignore multiple clicks within a 2-3 second window to prevent accidental duplicate requests.

## Key Features
- **Pull-based Status:** Poll the backend `/my-last` endpoint to show the current progress of a payout (Pending -> InProgress -> Sent).

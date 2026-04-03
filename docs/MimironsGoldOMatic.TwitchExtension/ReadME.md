## MimironsGoldOMatic.TwitchExtension (React | Bridge between twitch & backend)

- **Role:** Viewer-facing interface for **joining the participant pool** and **watching the roulette**.
- **Stack:** React 18+, Vite, TypeScript, Tailwind CSS.

## Key Functions

- **Form:** Input for "In-game Character Name".
- **Validation:** Prevents submission if the name is empty or invalid.
- **Twitch Integration:** Uses `window.Twitch.ext` to access the viewer's ID and context.
- **API Interaction:**
  - **POST** `/api/payouts/claim` with `CharacterName` and `TwitchTransactionId` (Twitch redemption id) for idempotency — **adds the viewer to the pool** (not an instant payout).
    - Expects `201 Created` for new enrollment and `200 OK` for idempotent duplicate replay.
  - **GET** `/api/payouts/my-last` (pull model) to show the viewer their latest **payout** status after they win a spin (`404` when none).
- **Visual roulette:** Animated selection on each spin; default **5-minute** cadence; **minimum 1** participant; **non-winners stay in the pool**; reflect **`/who`** / verification if Backend exposes it.
- **Winner UX:** When the logged-in viewer wins, show **“You won”** and explicit instructions: **whisper the streamer** exactly **`!twgold`** in a **private message** **to receive the in-game gold mail**.
- **Instant spin:** Surface the Channel Points reward **“Switch to instant spin”** so the next spin runs without waiting for the current 5-minute window.

## Libraries

- `axios` (for API calls)
- `@twitch-ext/developer-rig` (for testing)
- `Zustand` (for State Management)

## Architecture & Patterns
- **Repository Pattern:**
  Implement `IPayoutRepository` (e.g., `TwitchPayoutRepository`). UI components must be agnostic of the communication layer (Fetch/Axios) and JWT injection. Use this abstraction to easily swap between Real API and Mock Data (for Dev Rig testing).
  
- **Store Pattern (State Management):**
  Use **Zustand** to maintain a global state. Track pool membership, spin countdown, last winner, and the status of the user's latest **winner** payout if applicable.

- **Error Boundary:**
  Wrap the main form in a React Error Boundary. If the Twitch API or Backend fails, display a themed "Gnomish machinery has jammed!" error message instead of a white screen.

- **Debounce / Throttle:**
  Apply a debounce to the "Redeem" button. Ignore multiple clicks within a 2-3 second window to prevent accidental duplicate requests.

## Key Features
- **Pull-based Status:** Poll the backend for pool/spin state and `/my-last` to show **winner** payout progress (`Pending` -> `InProgress` -> `Sent`). Messaging can note that **`Sent`** means mail was confirmed via **`[MGM_CONFIRM:UUID]`**, after the winner whispers **`!twgold`** to show **willingness to accept** gold (see `docs/SPEC.md`).

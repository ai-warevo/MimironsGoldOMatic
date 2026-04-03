## MimironsGoldOMatic.TwitchExtension (React | Bridge between twitch & backend)

- **UI spec:** `docs/UI_SPEC.md` §1 (viewer panel **UI-101–106**; optional broadcaster **UI-201–204**).
- **Role:** Viewer-facing **roulette**, pool display, and **winner / payout status**. **Pool enrollment is driven by Twitch chat** (`!twgold <CharacterName>` per `docs/SPEC.md`), not by a form-only flow.
- **Stack:** React 18+, Vite, TypeScript, Tailwind CSS.

## Key Functions

- **On-panel copy:** Viewers **subscribe** and type **`!twgold <CharacterName>`** in **stream chat** to join the pool; after a win, **watch WoW** for the streamer’s **whisper** (Russian text, `docs/SPEC.md` §9) and **reply `!twgold`** in-game before gold mail.
- **Twitch Integration:** Uses `window.Twitch.ext` for viewer identity and **polls** Backend for pool size, spin state, and winner/payout status.
- **API Interaction (typical):**
  - **GET** `/api/roulette/state`, **`GET /api/pool/me`**, **`GET /api/payouts/my-last`** — all use **Twitch Extension JWT (Bearer)** only in MVP (`docs/SPEC.md` §5, §5.1). **Dev Rig** uses **real Twitch-issued** tokens; Backend validates per Twitch (`docs/SPEC.md` deployment scope).
  - **`GET /api/roulette/state`** + **`GET /api/pool/me`** — server-authoritative **`nextSpinAt`** (UTC **:00/:05/…**), **`spinPhase`** enum, optional **`currentSpinCycleId`**. **Must** drive the **countdown** from `nextSpinAt` / `serverNow`.
  - **`GET /api/payouts/my-last`** — **`PayoutDto`** or **`404`** when the viewer has **no** winner payout yet.
  - **Optional:** **POST** `/api/payouts/claim` for Dev Rig / testing only (same semantics as chat enroll; see `docs/SPEC.md`).
- **Visual roulette:** Animated selection on each spin; **5-minute** cadence; **minimum 1** participant; reflect **`/who`** / verification if Backend exposes it.
- **Winner UX:** **“You won”** + instructions: **in WoW**, reply to the streamer’s whisper with **`!twgold`** (case-insensitive; see `docs/SPEC.md` §9); **`Sent`** after **`[MGM_CONFIRM:UUID]`** in WoW log per spec.

## Libraries

- `axios` (for API calls)
- `@twitch-ext/developer-rig` (for testing)
- `Zustand` (for State Management)

## Architecture & Patterns
- **Repository Pattern:**
  Implement `IPayoutRepository` (e.g. `TwitchPayoutRepository`). UI components must be agnostic of the communication layer (Fetch/Axios) and JWT injection. Use this abstraction to easily swap between Real API and Mock Data (for Dev Rig testing).
  
- **Store Pattern (State Management):**
  Use **Zustand** to maintain a global state. Track spin countdown, last winner, pool hints from API, and the status of the user's latest **winner** payout if applicable.

- **Error Boundary:**
  Wrap the main panel in a React Error Boundary. If the Twitch API or Backend fails, display a themed "Gnomish machinery has jammed!" error message instead of a white screen.

## Key Features
- **Pull-based Status:** Poll the backend for pool/spin state and `/my-last` to show **winner** payout progress (`Pending` -> `InProgress` -> `Sent`). Messaging: **`Sent`** means mail was confirmed via **`[MGM_CONFIRM:UUID]`** after **WoW whisper `!twgold`** consent (see `docs/SPEC.md`).
- **Resilience:** On **`429`**, **`503`**, or network errors, use **exponential backoff** (cap interval, e.g. ≤ 60s) and a **Retry** action (`docs/SPEC.md` §5.1).

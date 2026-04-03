## MimironsGoldOMatic.Desktop (WPF | Bridge between backend & lua addon)

- **UI spec:** `docs/UI_SPEC.md` §3 (WPF **UI-301–308**: API setup, main window, queue, settings, modals, log).
- **Role:** Monitors the API and injects **winner** payout data into the WoW client; bridges **addon → Backend** for **`!twgold`** (acceptance) and **`WoWChatLog.txt`** tailing for **`[MGM_CONFIRM:UUID]`** (required **mail-sent → `Sent`**).
- **Stack:** .NET 10, WPF, MVVM (CommunityToolkit.Mvvm).

## Key Functions

- **API Polling:** Periodically fetches the pending **winner** payout queue from the Backend.
- **Roulette `/who`:** Coordinates injection of **`/who <Winner_InGame_Nickname>`** and parsing/reporting so winners are **online-verified** before notification and **`Pending`** payout (see `docs/SPEC.md`).
- **Process Targeting (MVP):** Targets the **foreground** `WoW.exe` (3.3.5a) process. Process selection from a list is a roadmap feature.

## Command Injection (WPF to Addon)

- Converts the list of payouts into a Lua-compatible string with canonical entry format:
  - `UUID:CharacterName:GoldCopper;`
  - Example: `/run ReceiveGold("2d2b7b2a-1111-2222-3333-444444444444:Somecharacter:10000000;")`
- Splits strings into chunks of < 255 characters (WoW chat limit).
- Uses `PostMessage` as primary strategy.
- Uses `SendInput` fallback strategy (operator-switchable) when primary injection is blocked/unreliable.

## Explicit Claim Flow (Desktop to Backend)

The Desktop app uses an explicit claim model to avoid accidentally locking payouts:

1. Desktop fetches the queue via **GET** `/api/payouts/pending`.
2. When the streamer clicks **Sync/Inject**, Desktop marks selected payouts as `InProgress` via **PATCH** `/api/payouts/{id}/status`.
3. Desktop injects the payload into WoW via `/run ReceiveGold("...")`.

### Feedback Loop (Addon to WPF to Backend)

1. The WoW Addon detects a whisper to the streamer with body exactly **`!twgold`** from the expected winner and **notifies this utility** (IPC mechanism TBD).
2. The Desktop utility calls the Backend (e.g. **POST** `/api/payouts/{id}/confirm-acceptance`) to record **willingness to accept** gold (**not** **`Sent`**).
3. **Required:** The WPF Utility **must** monitor `Logs\WoWChatLog.txt` in real time for **`[MGM_CONFIRM:UUID]`**. On match, it calls the Backend (**PATCH** `/api/payouts/{id}/status` → **`Sent`**) because that tag **confirms mail was sent**.
4. If the log entry is missed, the Desktop UI provides a manual **Mark as Sent** override (policy decision).

## Libraries

- `Refit` or `HttpClient` (for API)
- `CommunityToolkit.Mvvm`

## Architecture & Patterns
- **Strategy Pattern (Injection):**
  Implement `IWoWInputStrategy`. Create `PostMessageStrategy` (primary) and `SendInputStrategy` (fallback). Allow the streamer to switch strategies in settings if one is blocked by a specific private server's anti-cheat.
  
- **Observer Pattern (Addon Bridge & ChatLogWatcher):**
  The addon IPC listener and **`ChatLogWatcher`** must be observable. When **`!twgold`** is confirmed **or** **`[MGM_CONFIRM:UUID]`** appears, notify subscribers (ViewModel + API Service) with the correct follow-up API call (acceptance vs **`Sent`**).

- **State Pattern (UI Behavior):**
  The "Sync" button must act as a state machine. Its appearance and logic should change based on the app state: `Searching for WoW` -> `Process Found` -> `Waiting for Mailbox` -> `Ready to Inject`.

## Resilience
- **Polly Integration:** Use Polly for all outgoing HTTP calls to the Backend to handle transient network issues with retries and exponential backoff.

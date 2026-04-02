## MimironsGoldOMatic.Desktop (WPF | Bridge between backend & lua addon)

- **Role:** Monitors the API and injects payout data into the WoW client.
- **Stack:** .NET 10, WPF, MVVM (CommunityToolkit.Mvvm).

## Key Functions

- **API Polling:** Periodically fetches the pending queue from the Backend.
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

### Feedback Loop (Addon to WPF)
1. The WoW Addon prints `[MGM_CONFIRM:UUID]` to chat upon actual mail send confirmation.
2. The WPF Utility will monitor `Logs\WoWChatLog.txt` in real-time for the pattern `[MGM_CONFIRM:UUID]`.
3. Upon detecting the confirmation tag, the WPF Utility will call the Backend API `PATCH /api/payouts/{id}/status` to mark the status as `Sent`.
4. If the log entry is missed, the Desktop UI provides a manual **Mark as Sent** override.

## Libraries

- `Refit` or `HttpClient` (for API)
- `CommunityToolkit.Mvvm`

## Architecture & Patterns
- **Strategy Pattern (Injection):**
  Implement `IWoWInputStrategy`. Create `PostMessageStrategy` (primary) and `SendInputStrategy` (fallback). Allow the streamer to switch strategies in settings if one is blocked by a specific private server's anti-cheat.
  
- **Observer Pattern (Log Watcher):**
  The `ChatLogWatcher` must be observable. When the specific string `[MGM_CONFIRM:UUID]` appears in `WoWChatLog.txt`, it must notify subscribers (the ViewModel to update UI and the API Service to send a status PATCH).

- **State Pattern (UI Behavior):**
  The "Sync" button must act as a state machine. Its appearance and logic should change based on the app state: `Searching for WoW` -> `Process Found` -> `Waiting for Mailbox` -> `Ready to Inject`.

## Resilience
- **Polly Integration:** Use Polly for all outgoing HTTP calls to the Backend to handle transient network issues with retries and exponential backoff.

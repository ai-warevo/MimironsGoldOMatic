## MimironsGoldOMatic.Desktop (WPF)

- **Role:** Monitors the API and injects payout data into the WoW client.
- **Stack:** .NET 10, WPF, MVVM (CommunityToolkit.Mvvm).

## Key Functions

- **API Polling:** Periodically fetches the pending queue from the Backend.
- **Process Targeting (MVP):** Targets the **foreground** `WoW.exe` (3.3.5a) process. Process selection from a list is a roadmap feature.

## Command Injection (WPF to Addon)

- Converts the list of payouts into a Lua-compatible string: `/run ReceiveGold("PlayerA:100;PlayerB:500;")`.
- Splits strings into chunks of < 255 characters (WoW chat limit).
- Uses `PostMessage` to trigger `WM_KEYDOWN` (Enter), simulates pasting the string, then triggers Enter again.

## Explicit Claim Flow (Desktop to Backend)

The Desktop app uses an explicit claim model to avoid accidentally locking payouts:

1. Desktop fetches the queue via **GET** `/api/payouts/pending`.
2. When the streamer clicks **Sync/Inject**, Desktop marks selected payouts as `InProgress` via **PATCH** `/api/payouts/{id}/status`.
3. Desktop injects the payload into WoW via `/run ReceiveGold("...")`.

### Feedback Loop (Addon to WPF)
1. The WoW Addon will print a specific tag to the chat log upon successful mail sending (e.g., "MGM_CONFIRM:[PayoutId]").
2. The WPF Utility will monitor `Logs\WoWChatLog.txt` in real-time for the pattern `[MGM_CONFIRM:UUID]`.
3. Upon detecting the confirmation tag, the WPF Utility will call the Backend API `PATCH /api/payouts/{id}/status` to mark the status as `Sent`.
4. If the log entry is missed, the Desktop UI provides a manual **Mark as Sent** override.

## Libraries

- `Refit` or `HttpClient` (for API)
- `CommunityToolkit.Mvvm`

## MimironsGoldOMatic.Desktop (WPF)

- **Role:** Monitors the API and injects payout data into the WoW client.
- **Stack:** .NET 10, WPF, MVVM (CommunityToolkit.Mvvm).

## Key Functions

- **API Polling:** Periodically fetches the pending queue from the Backend.
- **Process Detection:** Uses Win32 API (`FindWindow`, `GetWindowThreadProcessId`) to locate the `WoW.exe` (3.3.5a) process.

## Command Injection (WPF to Addon)

- Converts the list of payouts into a Lua-compatible string: `/run ReceiveGold("PlayerA:100;PlayerB:500;")`.
- Splits strings into chunks of < 255 characters (WoW chat limit).
- Uses `PostMessage` to trigger `WM_KEYDOWN` (Enter), simulates pasting the string, then triggers Enter again.

### Feedback Loop (Addon to WPF)
1. The WoW Addon will print a specific tag to the chat log upon successful mail sending (e.g., "MGM_CONFIRM:[PayoutId]").
2. The WPF Utility will monitor `Logs/WoWChatLog.txt` in real-time.
3. Upon detecting the confirmation tag, the WPF Utility will call the Backend API `PATCH /api/payouts/{id}` to mark the status as 'Sent'.

## Libraries

- `Refit` or `HttpClient` (for API)
- `CommunityToolkit.Mvvm`

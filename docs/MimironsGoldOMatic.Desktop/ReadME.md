## MimironsGoldOMatic.Desktop (WPF)

- **Role:** Monitors the API and injects payout data into the WoW client.
- **Stack:** .NET 10, WPF, MVVM (CommunityToolkit.Mvvm).

## Key Functions

- **API Polling:** Periodically fetches the pending queue from the Backend.
- **Process Detection:** Uses Win32 API (`FindWindow`, `GetWindowThreadProcessId`) to locate the `WoW.exe` (3.3.5a) process.

## Command Injection ("The Magic")

- Converts the list of payouts into a Lua-compatible string: `/run ReceiveGold("PlayerA:100;PlayerB:500;")`.
- Splits strings into chunks of < 255 characters (WoW chat limit).
- Uses `PostMessage` to trigger `WM_KEYDOWN` (Enter), simulates pasting the string, then triggers Enter again.

## Libraries

- `Refit` or `HttpClient` (for API)
- `CommunityToolkit.Mvvm`

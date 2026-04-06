## Request

Create a new .NET project at `src\Mocks\WoWMock\WoWMock.csproj` to serve as a mock WoW client for integration testing of the `src\MimironsGoldOMatic.Desktop\MimironsGoldOMatic.Desktop.csproj` component.

### Project Specifications

- **Project name:** `WoWMock`
- **Location:** `src\Mocks\WoWMock\`
- **Type:** Console Application (target .NET 6.0 or later)
- **Purpose:** simulate a WoW 3.3.5a client to test the Desktop app’s interaction without requiring the actual game client.
- **Key simulation targets:**
  - `Logs\WoWChatLog.txt` file creation and real-time updates
  - processing `/run` commands sent via WinAPI (`PostMessage`/`SendInput`)
  - generating mock chat log entries with MGM tags: `[MGM_WHO]`, `[MGM_ACCEPT:UUID]`, `[MGM_CONFIRM:UUID]`
  - responding to in-game mail UI actions (mocked)

### Functional Requirements

1. **Chat log simulation**
   - create and maintain a `WoWChatLog.txt` file at a configurable path
   - append timestamped messages to the log in real time
   - support injection of custom MGM-tagged messages via API

2. **`/run` command handling**
   - log all received `/run` commands (for verification in tests)
   - optionally echo a response back into the chat log (configurable per test case)

3. **REST API for test orchestration**
   - use ASP.NET Core with Kestrel (self-hosted)
   - endpoints:
     - `POST /api/mock/reset`
     - `POST /api/mock/add-message`
     - `GET /api/mock/commands`
     - `POST /api/mock/set-response`

4. **Configuration**
   - `LogFilePath` (default: `Logs\WoWChatLog.txt`)
   - `ApiPort` (default: 5001)
   - `CommandProcessingDelayMs` (default: 100ms)
   - `AutoConfirmAccepts` (default: false)

5. **Logging**
   - diagnostic logs to console
   - optionally write to `WoWMock.log`

### Expected Structure

```
src\Mocks\WoWMock\
├── WoWMock.csproj
├── appsettings.json
├── Program.cs
├── ChatLogSimulator.cs
├── CommandProcessor.cs
├── Api\
│   ├── MockController.cs
│   └── Startup.cs
├── Models\
│   ├── MockMessage.cs
│   └── MockCommand.cs
└── Configuration\
    └── MockSettings.cs
```


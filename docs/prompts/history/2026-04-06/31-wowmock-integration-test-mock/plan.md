## Goal

Add a standalone `WoWMock` process that simulates key World of Warcraft 3.3.5a behaviors needed by `MimironsGoldOMatic.Desktop` integration tests: a real-time `WoWChatLog.txt`, `/run` command capture, and MGM-tagged message emission, all orchestrated via a local REST API.

## Scope / deliverables

- New project at `src/Mocks/WoWMock/WoWMock.csproj` (net6.0 or later), runnable via `dotnet run`.
- Self-hosted ASP.NET Core (Kestrel) REST API:
  - `POST /api/mock/reset`
  - `POST /api/mock/add-message`
  - `GET /api/mock/commands`
  - `POST /api/mock/set-response`
- Chat log simulator:
  - Creates and appends timestamped lines to `LogFilePath` (default `Logs/WoWChatLog.txt`)
  - Safe concurrent writes; file readable while being appended
- `/run` command handling:
  - Accept `/run ...` commands (from injected messages) and store them for retrieval
  - Optional echo into chat log (controlled by response settings)
  - Optional auto-confirm: after seeing `[MGM_ACCEPT:UUID]` emit `[MGM_CONFIRM:UUID]` after delay
- Configuration via `appsettings.json` with the required keys.
- Console diagnostics; optional file diagnostics `WoWMock.log`.

## Architecture

- **`MockSettings`**: strongly typed settings bound from configuration.
- **`ChatLogSimulator`**: single writer with `SemaphoreSlim`; handles timestamp formatting; ensures directories exist.
- **`CommandProcessor`**: thread-safe store of received commands; contains response policy; can emit log messages via `ChatLogSimulator`.
- **`MockState` (internal)**: shared state container used by API controller for reset and coordination.
- **ASP.NET Core MVC**: `Startup` registers services, controller; `Program` hosts Kestrel on `ApiPort`.

## Key behavior decisions (documented in code)

- Message injection is the primary “input” channel; if injected content begins with `/run `, it is treated as a run command (to keep the required API surface minimal while still enabling command verification in tests).
- Auto-confirm uses a conservative UUID extraction and respects `CommandProcessingDelayMs`.
- Chat log uses a format that is stable for parsing by Desktop tailing (timestamp + message), and flushes on each write.

## Files to add

- `src/Mocks/WoWMock/WoWMock.csproj`
- `src/Mocks/WoWMock/appsettings.json`
- `src/Mocks/WoWMock/Program.cs`
- `src/Mocks/WoWMock/ChatLogSimulator.cs`
- `src/Mocks/WoWMock/CommandProcessor.cs`
- `src/Mocks/WoWMock/Api/MockController.cs`
- `src/Mocks/WoWMock/Api/Startup.cs`
- `src/Mocks/WoWMock/Models/MockMessage.cs`
- `src/Mocks/WoWMock/Models/MockCommand.cs`
- `src/Mocks/WoWMock/Configuration/MockSettings.cs`

## Verification

- `dotnet build src/Mocks/WoWMock/WoWMock.csproj`
- `dotnet run --project src/Mocks/WoWMock/WoWMock.csproj`
  - Confirm API listens on configured port.
  - `POST /api/mock/reset` creates/clears log.
  - `POST /api/mock/add-message` appends to `WoWChatLog.txt`.
  - Inject `/run ...` via add-message and confirm it appears in `GET /api/mock/commands`.
  - Enable auto-confirm via `set-response` and confirm `[MGM_CONFIRM:...]` appended after an accept.

## Risks & mitigations

- **Desktop expects WoW timestamp format**: keep timestamps deterministic and easy to parse; adjust if Desktop parser is strict.
- **Concurrent writes/readers**: open streams with `FileShare.ReadWrite` and serialize writes with a semaphore.


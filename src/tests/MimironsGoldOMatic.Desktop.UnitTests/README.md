# MimironsGoldOMatic.Desktop.UnitTests

Fast, isolated unit tests for **Desktop business logic** (chunking, chat-log markers, settings persistence, EBS HTTP client, connection/poll rules). These tests do **not** start the WPF UI or hit a real backend.

## Requirements

- Windows (project targets `net10.0-windows` because it references the WPF Desktop app).
- .NET 10 SDK.

## Run tests

From the repository root.

**Release:** the Desktop app’s Release profile uses `RuntimeIdentifier=win-x64`. Restore that graph once, then test:

```powershell
dotnet restore src/MimironsGoldOMatic.Desktop/MimironsGoldOMatic.Desktop.csproj -r win-x64
dotnet test src/tests/MimironsGoldOMatic.Desktop.UnitTests/MimironsGoldOMatic.Desktop.UnitTests.csproj --configuration Release --no-restore
```

**Debug** (no RID restore needed):

```powershell
dotnet test src/tests/MimironsGoldOMatic.Desktop.UnitTests/MimironsGoldOMatic.Desktop.UnitTests.csproj --configuration Debug
```

With normal verbosity and TRX (for CI), after the **Release** restore above:

```powershell
dotnet test src/tests/MimironsGoldOMatic.Desktop.UnitTests/MimironsGoldOMatic.Desktop.UnitTests.csproj `
  --configuration Release `
  --no-restore `
  --verbosity normal `
  --logger "trx;LogFileName=desktop-unit-tests.trx"
```

Run a single class:

```powershell
dotnet test src/tests/MimironsGoldOMatic.Desktop.UnitTests/MimironsGoldOMatic.Desktop.UnitTests.csproj `
  --filter "FullyQualifiedName~ReceiveGoldCommandChunkerTests"
```

## What is covered

| Area | Tests |
|------|--------|
| ReceiveGold chunking / copper | `ReceiveGoldCommandChunkerTests` |
| `/run NotifyWinnerWhisper` escaping | `WoWRunCommandsTests` |
| WoW chat log path resolution | `WoWChatLogPathResolverTests` |
| Pending payout map for MGM_ACCEPT | `PayoutSnapshotCacheTests` |
| Poll interval clamp + connection tuple | `DesktopConnectionContextTests` |
| Settings JSON + corrupt file handling | `DesktopSettingsStoreTests` |
| Notified-whisper id persistence | `NotifiedWhisperStoreTests` |
| EBS HTTP client (mock handler) | `EbsDesktopClientTests` |
| Chat log markers → API calls | `WoWChatLogTailServiceTests` |

`MainViewModel` is intentionally not constructed here: it ties into `DispatcherTimer` and foreground Win32 checks. Logic that drives the queue UI is covered via the services above and `IEbsDesktopClient` mocks.

## Mocks

- **HTTP:** `DelegatingHttpHandler` + `TestHttpClientFactory` (no network).
- **Backend API in tail tests:** `Moq` over `IEbsDesktopClient`.
- **File system:** temporary directories passed into `DesktopSettingsStore` / `NotifiedWhisperStore` constructors.

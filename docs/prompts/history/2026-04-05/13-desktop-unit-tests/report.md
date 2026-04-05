# Report

## Modified / added files

### Product (testability)

- `src/MimironsGoldOMatic.Desktop/Services/IEbsDesktopClient.cs` — API abstraction for mocks.
- `src/MimironsGoldOMatic.Desktop/Services/EbsDesktopClient.cs` — implements `IEbsDesktopClient`.
- `src/MimironsGoldOMatic.Desktop/Services/WoWChatLogTailService.cs` — `IEbsDesktopClient`; `ProcessLineAsync` is `internal` for tests.
- `src/MimironsGoldOMatic.Desktop/ViewModels/MainViewModel.cs` — `IEbsDesktopClient`.
- `src/MimironsGoldOMatic.Desktop/App.xaml.cs` — registers `IEbsDesktopClient`.
- `src/MimironsGoldOMatic.Desktop/Services/DesktopConnectionContext.cs` — `GetClampedPollIntervalSeconds()` (poll clamp used by `MainViewModel`).
- `src/MimironsGoldOMatic.Desktop/Services/DesktopSettingsStore.cs` — optional `dataDirectory` for temp-dir tests.
- `src/MimironsGoldOMatic.Desktop/Services/NotifiedWhisperStore.cs` — optional `dataDirectory`.
- `src/MimironsGoldOMatic.Desktop/AssemblyInfo.cs` — `InternalsVisibleTo` for unit test assembly.

### Tests & docs

- `src/tests/MimironsGoldOMatic.Desktop.UnitTests/**` — xUnit + Moq; 48 tests.
- `src/tests/MimironsGoldOMatic.Desktop.UnitTests/README.md` — run instructions (Debug / Release + `win-x64` restore).
- `src/MimironsGoldOMatic.slnx` — includes unit test project.
- `.github/workflows/unit-integration-tests.yml` — restores `win-x64` Desktop graph; runs unit tests then smoke tests; separate TRX files.

## Verification

- `dotnet test src/tests/MimironsGoldOMatic.Desktop.UnitTests/...` — **48 passed** (Debug and Release with `win-x64` restore).
- `dotnet test src/MimironsGoldOMatic.Desktop.Tests/...` — smoke test still passes after DI change.

## Technical debt / follow-ups

- `MainViewModel` is not instantiated in unit tests (WPF `DispatcherTimer` + Win32 foreground). Further extraction (e.g. timers behind interfaces) would be needed for direct VM coverage without UI test harnesses.

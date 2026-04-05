# Plan

## Architecture

- Introduce `IEbsDesktopClient` so chat-log tail processing and HTTP client can be mocked in tests without WPF.
- Optional `dataDirectory` constructor parameter on `DesktopSettingsStore` and `NotifiedWhisperStore` for isolated file I/O (temp folders).
- Mark `WoWChatLogTailService.ProcessLineAsync` as `internal` and add `InternalsVisibleTo` for the unit test assembly.

## Affected files

- `src/MimironsGoldOMatic.Desktop/Services/IEbsDesktopClient.cs` (new)
- `src/MimironsGoldOMatic.Desktop/Services/EbsDesktopClient.cs` — implement interface
- `src/MimironsGoldOMatic.Desktop/Services/WoWChatLogTailService.cs` — interface + internal method
- `src/MimironsGoldOMatic.Desktop/ViewModels/MainViewModel.cs` — `IEbsDesktopClient`
- `src/MimironsGoldOMatic.Desktop/App.xaml.cs` — register `IEbsDesktopClient`
- `src/MimironsGoldOMatic.Desktop/Services/DesktopSettingsStore.cs`, `NotifiedWhisperStore.cs` — optional data root
- `src/MimironsGoldOMatic.Desktop/AssemblyInfo.cs` — InternalsVisibleTo
- `src/tests/MimironsGoldOMatic.Desktop.UnitTests/**` — new project + README
- `src/MimironsGoldOMatic.slnx` — include unit test project
- `.github/workflows/unit-integration-tests.yml` — run new tests on Windows

## Risks

- Low: DI registration must resolve both concrete client and interface for existing behavior.

# Plan — MVP-4 Desktop

## Architecture

- **MVVM:** `CommunityToolkit.Mvvm` — `MainViewModel`, optional small VMs for settings / event log.
- **DI:** `Microsoft.Extensions.DependencyInjection` in `App.xaml.cs` (single composition root).
- **HTTP:** Typed `EbsDesktopClient` wrapping `HttpClient` + Polly retry on transient/`429`/`503` (aligned with `docs/components/desktop/ReadME.md`).
- **Settings:** JSON file + **DPAPI**-protected API key file under `%LocalAppData%\MimironsGoldOMatic\`.
- **Win32:** `IWoWInputStrategy` — `PostMessageWoWInputStrategy` (WM_CHAR to focused WoW main HWND), `SendInputWoWInputStrategy` (Unicode `SendInput`). Foreground `WoW.exe` via `GetForegroundWindow` + process name check (`docs/overview/SPEC.md` §8).
- **Log tail:** One `FileStream` tail of `WoWChatLog.txt` (default: `{WoWInstall}\Logs\WoWChatLog.txt`, optional override path from settings). Parse tags per `docs/overview/SPEC.md` §10; idempotent handling with in-memory keys.
- **NotifyWinnerWhisper:** After each successful pending fetch, inject for each `Pending` payout not in persisted `notified-whisper.json` (avoid duplicate whispers across restarts).
- **`[MGM_ACCEPT]`:** Resolve `characterName` from last pending snapshot (`GET /api/payouts/pending`); call `POST .../confirm-acceptance`.
- **ReceiveGold:** Build `UUID:CharacterName:copper;` entries; copper = `GoldAmount * 10000` (WoW copper); chunk so each full `/run ReceiveGold("…")` line is `<255` chars.

## Files (primary)

- `src/MimironsGoldOMatic.Desktop/*.csproj` — packages + `ProjectReference` Shared.
- `Services/EbsDesktopClient.cs`, `Services/WoWChatLogTailService.cs`, `Services/DesktopPaths.cs`, `Services/ReceiveGoldCommandChunker.cs`, `Services/NotifiedWhisperStore.cs`, `Services/DesktopSettingsStore.cs`
- `Api/VerifyCandidateRequestDto.cs` (JSON mirror of Backend contract)
- `Win32/*` — P/Invoke, strategies, foreground locator
- `ViewModels/MainViewModel.cs`, `Views/SettingsWindow.xaml`, `Views/EventLogWindow.xaml`, `MainWindow.xaml`
- `docs/reference/IMPLEMENTATION_READINESS.md`, `docs/overview/ROADMAP.md` (MVP-4 status), `docs/components/desktop/ReadME.md`

## Risks

- Real **3.3.5a** clients vary: document delays and HWND strategy in `Win32` XML doc or class summary; operator can switch to SendInput in settings.
- `PostMessage` may be blocked on some servers — fallback is required.

## Verification

- `dotnet build src/MimironsGoldOMatic.slnx`

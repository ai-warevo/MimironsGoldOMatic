# Report — MVP-4 Desktop

## Summary

Implemented **MVP-4** WPF desktop bridge: EBS HTTP client with **`X-MGM-ApiKey`** and Polly retries, **`GET /api/payouts/pending`** polling, persisted **`NotifyWinnerWhisper`** for new **`Pending`** payouts when foreground **`WoW.exe`** is detected, **Sync/Inject** (**`PATCH` `InProgress`** + chunked **`/run ReceiveGold`**), single **`WoWChatLog.txt`** tail for **`[MGM_WHO]`** / **`[MGM_ACCEPT]`** / **`[MGM_CONFIRM]`**, manual **`PATCH`** actions including **`InProgress`→`Pending`**, settings (DPAPI ApiKey, log path, injection preference), and WinAPI notes in `docs/components/desktop/ReadME.md`.

## Modified / added files

- `src/MimironsGoldOMatic.Desktop/` — project, `App`, `MainWindow`, `SettingsWindow`, `EventLogWindow`, `ViewModels/MainViewModel.cs`, `Services/*`, `Api/*`, `Win32/*`
- `docs/overview/ROADMAP.md` — MVP-4 status + escape hatch bullet
- `docs/reference/IMPLEMENTATION_READINESS.md` — MVP-4 row
- `docs/components/desktop/ReadME.md` — status + WinAPI section
- `docs/prompts/history/2026-04-04/11-mvp-4-desktop/*`

## Verification

- `dotnet build src/MimironsGoldOMatic.slnx` — **passed**
- In-game / E2E not run in this session (requires local EBS + WoW 3.3.5a).

## Technical debt / follow-ups

- Optional: richer UI-305 mailbox/API pills, row filter (UI-304-03), DPAPI migration to credential manager if required later.
- `SendInput` **INPUT** struct sizing is standard x64 layout; if a client reports failed injection, validate **`Marshal.SizeOf<INPUT>()`** against **`SendInput`** expectations on target OS.

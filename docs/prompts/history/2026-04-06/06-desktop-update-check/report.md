## Modified files

- `src/MimironsGoldOMatic.Desktop/App.xaml.cs`
- `src/MimironsGoldOMatic.Desktop/MainWindow.xaml`
- `src/MimironsGoldOMatic.Desktop/Services/EbsDesktopClient.cs`
- `src/MimironsGoldOMatic.Desktop/Services/IEbsDesktopClient.cs`
- `src/MimironsGoldOMatic.Desktop/Services/WoWChatLogTailService.cs`
- `src/MimironsGoldOMatic.Desktop/Services/WoWRunCommands.cs`
- `src/MimironsGoldOMatic.Desktop/ViewModels/MainViewModel.cs`
- `src/MimironsGoldOMatic.Desktop/Services/Updates/IAppVersionProvider.cs`
- `src/MimironsGoldOMatic.Desktop/Services/Updates/AssemblyAppVersionProvider.cs`
- `src/MimironsGoldOMatic.Desktop/Services/Updates/IUpdateService.cs`
- `src/MimironsGoldOMatic.Desktop/Services/Updates/UpdateService.cs`
- `src/MimironsGoldOMatic.Desktop/Services/Updates/VersionCheckResult.cs`
- `src/Tests/MimironsGoldOMatic.Desktop.UnitTests/EbsDesktopClientTests.cs`
- `src/Tests/MimironsGoldOMatic.Desktop.UnitTests/WoWChatLogTailServiceTests.cs`
- `src/Tests/MimironsGoldOMatic.Desktop.UnitTests/UpdateServiceTests.cs`

## Verification

- `dotnet test src/Tests/MimironsGoldOMatic.Desktop.UnitTests/MimironsGoldOMatic.Desktop.UnitTests.csproj` ✅ (55 passed)
- `dotnet build src/MimironsGoldOMatic.slnx` ✅
- `ReadLints` on Desktop paths: no diagnostics.

## Remaining risks / debt

- Update check fallback URL from Desktop config is not introduced yet; button opens only endpoint-provided `releaseNotesUrl`.
- WoW-side update reply currently uses `DEFAULT_CHAT_FRAME:AddMessage` and not targeted whisper routing, because `[MGM_UPDATE_CHECK]` payload has no player identity in Desktop log flow.

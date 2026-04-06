## Plan

### Architecture changes
- Add a new gift-request bounded flow in Backend with Marten documents + stream events and MediatR handlers.
- Keep existing payout flow untouched; implement `!twgift` as a parallel domain flow.
- Extend EventSub ingestion to parse `!twgift <CharacterName>` and enqueue requests server-side.
- Add Desktop polling for active gift selection and confirmation forwarding.
- Add WoW Addon inventory scan + confirmation hooks for the gift flow.
- Add Extension polling/state rendering for gift queue position and status.

### Affected files (expected)
- Backend:
  - `Application/EbsMediator.cs`
  - `Persistence/MartenDocuments.cs`, `Persistence/PayoutStreamEvents.cs`, `Persistence/MgmMartenDocumentConfiguration.cs`
  - `Controllers/TwitchEventSubController.cs`, new gift controllers
  - `Services/*` (chat parser/ingest, timeout processor, Helix subscriber check)
  - `Api/ApiContracts.cs`
- Desktop:
  - `Services/IEbsDesktopClient.cs`, `Services/EbsDesktopClient.cs`
  - `Services/WoWRunCommands.cs`, `Services/WoWChatLogTailService.cs`
  - `ViewModels/MainViewModel.cs`, `MainWindow.xaml`
- WoW Addon:
  - `MimironsGoldOMatic.lua`
- Twitch Extension:
  - `src/api/models.ts`, `src/api/client.ts`, `src/hooks/useMgmEbsPolling.ts`, `src/state/mgmPanelStore.ts`, `src/components/ViewerPanel.tsx`
- Docs:
  - `docs/overview/SPEC.md`
  - `docs/reference/UI_SPEC.md`

### Risks
- EventSub->Desktop->Addon orchestration races (multiple messages/tail dedup).
- Queue promotion race when two requests arrive near-simultaneously.
- API generator overwrites TypeScript API files; edits must stay aligned with generator inputs.
- Helix subscriber verification may fail without correct broadcaster scopes/token.

### Verification plan
- Build all components: `dotnet build src/MimironsGoldOMatic.slnx`.
- Run selected tests around backend/desktop parsing if impacted.
- Run lints on edited files and fix introduced issues.

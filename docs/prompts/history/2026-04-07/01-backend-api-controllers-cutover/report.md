## Summary

Sub-prompt B completed: controllers were ported into `Backend.Api` and now send MediatR requests defined in `Backend.Domain`, with handlers implemented in `Backend.Services` and persistence types aligned to `Backend.Abstract`.

## Key changes

- Controllers moved/added under `src/MimironsGoldOMatic.Backend.Api/Controllers/`:
  - `RouletteController`
  - `GiftRequestsController`
  - `DesktopRouletteController`
  - `DesktopPayoutsController`
  - `DesktopGiftRequestsController`
  - `TwitchEventSubController`
  - `E2eHarnessController`
- `Backend.Api/Program.cs` updated to use `builder.Services.AddMgmBackend(...)`, run Marten schema apply on startup, and enable auth middleware.
- `Backend.Domain/EbsMediator.Contracts.cs` updated to match the existing API request/response shapes (notably `VerifyCandidateRequest` including `capturedAt`) and to host the request DTOs used by controllers.
- MediatR handlers moved into `Backend.Services` (`src/MimironsGoldOMatic.Backend.Services/Mediatr/*`) and supporting services from the legacy backend were ported into `Backend.Services` (gift queue, chat enrollment, hosted services, roulette helpers).
- Persistence types in `Backend.DataAccess` were updated to use `Backend.Abstract.PayoutStatus` to avoid cross-assembly enum drift.
- `Backend.Infrastructure/AddMgmBackend` updated to register the newly-ported services + hosted services and to register MediatR from the handlers assembly.

## Modified / added files (high level)

- `src/MimironsGoldOMatic.Backend.Api/Program.cs`
- `src/MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.csproj`
- `src/MimironsGoldOMatic.Backend.Api/Controllers/*.cs` (new controllers)
- `src/MimironsGoldOMatic.Backend.Domain/EbsMediator.Contracts.cs`
- `src/MimironsGoldOMatic.Backend.Services/MimironsGoldOMatic.Backend.Services.csproj`
- `src/MimironsGoldOMatic.Backend.Services/Mediatr/*.cs` (new handlers)
- `src/MimironsGoldOMatic.Backend.Services/*.cs` (ported services + hosted services)
- `src/MimironsGoldOMatic.Backend.DataAccess/MimironsGoldOMatic.Backend.DataAccess.csproj`
- `src/MimironsGoldOMatic.Backend.DataAccess/Persistence/MartenDocuments.cs`
- `src/MimironsGoldOMatic.Backend.DataAccess/Persistence/PayoutStreamEvents.cs`
- `src/MimironsGoldOMatic.Backend.Infrastructure/DependencyInjection/BackendCompositionExtensions.cs`

## Verification

- `dotnet build src/MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.csproj` ✅ (0 warnings, 0 errors)

## Notes / follow-ups

- Full-solution builds can still be impacted by the legacy `MimironsGoldOMatic.Backend` project’s ApiTsGen MSBuild step if `ApiTsGen.dll` is locked by another process; the new `Backend.Api` host builds cleanly and is the intended cutover target.


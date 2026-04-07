## Summary (Sub-prompt D)

Repointed integration test infrastructure from legacy `MimironsGoldOMatic.Backend` to **`MimironsGoldOMatic.Backend.Api`**.

## Changes

- **IntegrationTesting**: `WebApplicationFactory<Program>` now resolves `Program` from **Backend.Api**; removed legacy project reference; **GiftQueueTimeoutHostedService** excluded in test host alongside roulette/expiration workers.
- **Backend.IntegrationTests**: project references updated (Api + Domain + Services + DataAccess + Shared); **`BackendTestHost`** registers MediatR from **`PostClaimHandler`** (`Backend.Services.Mediatr`); test usings updated from `Backend.Application` / `Backend.Api` DTOs to **`Backend.Domain`**, **`Backend.Abstract`**, and **`Backend.Shared`** (`PayoutEconomics`).
- **Desktop.IntegrationTests**: references **Backend.Api**; **`PayoutDocumentSeed`** uses **`Backend.Abstract.PayoutStatus`** and **`Backend.Shared.PayoutEconomics`**.
- **README** (Backend.IntegrationTests): documents Api host as factory target.

## Verification

- `dotnet build src/Tests/MimironsGoldOMatic.Backend.IntegrationTests/MimironsGoldOMatic.Backend.IntegrationTests.csproj` — success.
- `dotnet build src/Tests/MimironsGoldOMatic.Desktop.IntegrationTests/MimironsGoldOMatic.Desktop.IntegrationTests.csproj` — success.
- `dotnet test src/Tests/MimironsGoldOMatic.Backend.IntegrationTests/MimironsGoldOMatic.Backend.IntegrationTests.csproj -c Debug --no-build` — passed (`44/44`).
- `dotnet test src/Tests/MimironsGoldOMatic.Desktop.IntegrationTests/MimironsGoldOMatic.Desktop.IntegrationTests.csproj -c Debug` — passed (`9/9`).

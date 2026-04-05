# MimironsGoldOMatic.Backend.UnitTests

## Strategy

- **Unit** (`[Trait("Category","Unit")]`, no Docker): spin-phase and roulette time math, FluentValidation, `TwGoldChatEnrollmentParser`, ASP.NET controllers with **Moq** `IMediator`, **ApiKey** authentication via `IAuthenticationService`, **Helix** outbound with mocked `HttpMessageHandler`, **TwitchEventSubController** with **Moq** `IChatEnrollmentIngest`.

PostgreSQL, Marten, and full-host HTTP flows live in **`MimironsGoldOMatic.Backend.IntegrationTests`** (Testcontainers). See `src/Tests/MimironsGoldOMatic.Backend.IntegrationTests/README.md`.

## How to run

From the repository root:

```bash
dotnet test src/Tests/MimironsGoldOMatic.Backend.UnitTests/MimironsGoldOMatic.Backend.UnitTests.csproj
```

Unit-only:

```bash
dotnet test src/Tests/MimironsGoldOMatic.Backend.UnitTests/MimironsGoldOMatic.Backend.UnitTests.csproj --filter "Category=Unit"
```

Full Backend (unit + integration):

```bash
dotnet test src/MimironsGoldOMatic.slnx
```

## Coverage goals

- **Target:** high line coverage on **MimironsGoldOMatic.Backend** core logic exercised by unit tests.
- **Measured scope:** use `coverlet.runsettings` in this folder to exclude **`Program.cs`** (host bootstrap) and **OpenAPI source-generated** `OpenApiXmlCommentSupport.generated.cs`.

```bash
dotnet test src/Tests/MimironsGoldOMatic.Backend.UnitTests/MimironsGoldOMatic.Backend.UnitTests.csproj \
  --configuration Release \
  --settings src/Tests/MimironsGoldOMatic.Backend.UnitTests/coverlet.runsettings \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults/coverage
```

## Layout

- `Unit/` — fast, deterministic tests (AAA, descriptive names).

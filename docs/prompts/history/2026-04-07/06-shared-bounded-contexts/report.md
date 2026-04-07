# Report

## Shared layout

- **`Payouts/`** — `MimironsGoldOMatic.Shared.Payouts`
- **`Gifts/`** — `MimironsGoldOMatic.Shared.Gifts`
- **`Versioning/`** — `MimironsGoldOMatic.Shared.Versioning`

## Other repo changes

- **`GlobalUsings.cs`** added to Backend.Domain, Backend.Shared, Backend.DataAccess, Backend.Services, Backend.Api, Desktop, SyntheticDesktop, Backend.UnitTests, Desktop.UnitTests, Desktop.IntegrationTests, Backend.IntegrationTests (three `global using` lines each).
- **`MimironsGoldOMatic.Backend.IntegrationTests.csproj`** — direct `ProjectReference` to `MimironsGoldOMatic.Shared`.
- **`MimironsGoldOMatic.Desktop.UnitTests.csproj`** — direct `ProjectReference` to `MimironsGoldOMatic.Shared` (for global usings).
- **`TwGoldChatEnrollmentParser.cs`** — XML `cref` corrected to `MimironsGoldOMatic.Backend.Common.CharacterNameRules`.
- **`src/MimironsGoldOMatic.Shared/README.md`** — table documenting bounded contexts.

## Verification

- `dotnet build src/MimironsGoldOMatic.slnx` — succeeded (0 warnings).

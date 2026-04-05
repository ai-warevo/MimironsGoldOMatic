# MimironsGoldOMatic.Backend.UnitTests

## Strategy

- **Unit** (`[Trait("Category","Unit")]`, no Docker): pure time/spin-phase logic, FluentValidation, `TwGoldChatEnrollmentParser`, ASP.NET controllers with **Moq** `IMediator`, **ApiKey** authentication via `IAuthenticationService`, **Helix** outbound with mocked `HttpMessageHandler`, **TwitchEventSubController** with **Moq** `IChatEnrollmentIngest`.
- **Integration** (`[Trait("Category","Integration")]`, Docker): PostgreSQL via **Testcontainers**; Marten schema; MediatR handlers, `ChatEnrollmentService`, `RouletteCycleTick`, `PayoutExpirationProcessor`, and payout/roulette rules aligned with `docs/SPEC.md` and `docs/INTERACTION_SCENARIOS.md`.

Coverage is strongest on **services**, **application handlers**, **controllers**, and **auth**; `Program.cs` startup and Marten configuration are exercised indirectly via integration tests, not duplicated with a full `WebApplicationFactory` (optional follow-up).

## How to run

From the repository root:

```bash
dotnet test src/tests/MimironsGoldOMatic.Backend.UnitTests/MimironsGoldOMatic.Backend.UnitTests.csproj
```

Unit-only (no Docker):

```bash
dotnet test src/tests/MimironsGoldOMatic.Backend.UnitTests/MimironsGoldOMatic.Backend.UnitTests.csproj --filter "Category=Unit"
```

Integration (requires Docker):

```bash
dotnet test src/tests/MimironsGoldOMatic.Backend.UnitTests/MimironsGoldOMatic.Backend.UnitTests.csproj --filter "Category=Integration"
```

Solution entry (from `src/`):

```bash
dotnet test MimironsGoldOMatic.slnx
```

## Coverage goals

- **Target:** ≥ **80%** line coverage on **MimironsGoldOMatic.Backend** core logic (handlers, services, controllers, auth).
- **Measured scope:** use `coverlet.runsettings` in this folder to exclude **`Program.cs`** (host bootstrap) and **OpenAPI source-generated** `OpenApiXmlCommentSupport.generated.cs`, which are not exercised by this suite. With that filter, **Backend** line coverage is **~87%** (run locally; exact figure varies slightly with compiler/tooling).
- Collect locally:

```bash
dotnet test src/tests/MimironsGoldOMatic.Backend.UnitTests/MimironsGoldOMatic.Backend.UnitTests.csproj \
  --configuration Release \
  --settings src/tests/MimironsGoldOMatic.Backend.UnitTests/coverlet.runsettings \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults/coverage
```

Then open the generated `coverage.cobertura.xml` in your IDE or a Cobertura viewer.

## Layout

- `Unit/` — fast, deterministic tests (AAA, descriptive names).
- `Support/` — shared Testcontainers fixture and Marten test host factory.
- `*IntegrationTests.cs` — collection `[PostgresCollection]`, truncate `mgm` between setups.

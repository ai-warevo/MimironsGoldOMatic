# Report

## Modified / added (high level)

- `Program.cs`: `UseRateLimiter` moved after `UseAuthentication` / `UseAuthorization` so authenticated Extension traffic is keyed by `user_id` instead of client IP.
- `src/tests/MimironsGoldOMatic.Backend.IntegrationTests/`: `HttpApiFixtureBase` truncates before host start and after boot; `BackendWebApplicationFactory` sets `Twitch:EventSubSecret` empty and uses `Microsoft.AspNetCore.TestHost` for `ConfigureTestServices`; `CriticalPathPerformanceIntegrationTests` (4 calls under 5s); `xunit.runner.json` + copy to output; `README.md`.
- `EventSubHttpEnrollmentIntegrationTests`, `Tc001BackendHttpPipelineIntegrationTests`: unique synthetic IDs to avoid cross-test collisions.
- `RouletteVerifyCandidateIntegrationTests`: truncate at start of the fact.
- `MimironsGoldOMatic.slnx` / `.sln`: IntegrationTests project entry.
- `Backend.UnitTests.csproj`: removed Testcontainers/Npgsql.
- `.github/workflows/unit-integration-tests.yml`: `dotnet test` on `slnx`.
- `docs/overview/INTERACTION_SCENARIOS.md`: automation bullets reference IntegrationTests project.
- `Backend.UnitTests/README.md`: points to IntegrationTests for Docker-backed tests.

## Verification

- `dotnet test src/MimironsGoldOMatic.slnx --configuration Release`

## Notes

- Integration JWTs are a **development harness** (same HS256 key path as empty `Twitch:ExtensionSecret` in Development), not production Twitch-issued tokens.

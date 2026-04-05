# Report — MVP-6 (Senior Architect)

## Summary

Delivered the **automated verification slice** of MVP-6: a PostgreSQL-backed integration test project for the EBS, small refactors so roulette and expiration rules are shared between hosted services and tests, documentation and readiness updates, and a **Npgsql/Marten-safe** cutoff for the expiration query.

## Modified / added files

| Area | Files |
|------|--------|
| Backend | `Persistence/MgmMartenDocumentConfiguration.cs` (new), `Services/RouletteCycleTick.cs` (new), `Services/PayoutExpirationProcessor.cs` (new), `Program.cs`, `Services/RouletteSynchronizerHostedService.cs`, `Services/PayoutExpirationHostedService.cs` |
| Tests | `MimironsGoldOMatic.Backend.Tests/*` (new project: claim rules, expiration, roulette+verify, patch Sent pool removal, Testcontainers + truncate helper) |
| Solution | `src/MimironsGoldOMatic.slnx` |
| Docs | `docs/reference/IMPLEMENTATION_READINESS.md`, `docs/setup/SETUP.md`, `docs/overview/ROADMAP.md`, `docs/components/backend/ReadME.md` |

## Verification

- **`dotnet test src/MimironsGoldOMatic.slnx`**: **7** passed (Docker required for Testcontainers).
- **`dotnet build src/MimironsGoldOMatic.slnx`**: succeeded.

## Technical notes

- **`.slnx` scope:** Includes **Shared**, **Backend**, **Backend.Tests**, **Desktop**. Twitch Extension and WoW addon remain outside the MSBuild solution per MVP-0.
- **Expiration LINQ:** `PayoutExpirationProcessor` now uses `DateTime.SpecifyKind(..., Unspecified)` for the **24h cutoff** parameter so Npgsql accepts it against `timestamp without time zone` (failure reproduced before fix under Testcontainers).
- **Parallelism:** Postgres test collection uses **`DisableParallelization`** to avoid cross-test DB races on one container.

## Not done here (explicit)

- Full **live** E2E (Twitch chat, Dev Rig, WoW client, Helix) — still manual per `docs/overview/INTERACTION_SCENARIOS.md`.
- CI workflow wiring (roadmap **Production** milestone still mentions placeholder workflows).

## Technical debt

- NU1608 warning: **JasperFx.RuntimeCompiler** vs **Microsoft.Extensions.Logging.Abstractions 10.x** (transitive via Marten); monitor upstream.

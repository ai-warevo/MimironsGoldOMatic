# Plan: MVP-6 (Senior Architect)

## Goals (from ROADMAP)

- Align E2E data-flow narrative with `docs/SPEC.md` (verification note in readiness matrix).
- Add minimal Backend/API integration tests: idempotency, one-active-per-user, lifetime cap, expiration, roulette with one participant.
- Keep `MimironsGoldOMatic.slnx` as the .NET solution; non-MSBuild trees (Twitch Extension, WoW addon) remain documented per MVP-0.
- Update `docs/IMPLEMENTATION_READINESS.md` MVP-6 row; add `dotnet test` + Docker note to `docs/SETUP.md`.

## Architecture

- Extract **one-shot** roulette tick and payout-expiration sweep from hosted services so production code and tests share the same rules (no duplicated business logic in tests).
- Centralize Marten document/event registration in `MgmMartenDocumentConfiguration` used by `Program.cs` and tests.
- Tests: **xUnit** + **Testcontainers** PostgreSQL + real `IDocumentStore` + **MediatR** handlers (same assembly as production).

## Risks

- CI/local runs require **Docker** for Testcontainers (documented).
- First `dotnet test` may be slow (image pull).

## Files to touch

- `src/MimironsGoldOMatic.Backend`: new configuration helper, `RouletteCycleTick`, `PayoutExpirationProcessor`; refactor `Program.cs`, hosted services.
- `src/MimironsGoldOMatic.Backend.Tests`: new project.
- `src/MimironsGoldOMatic.slnx`, `docs/IMPLEMENTATION_READINESS.md`, `docs/SETUP.md`.

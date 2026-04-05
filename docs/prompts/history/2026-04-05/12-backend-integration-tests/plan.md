# Plan

1. Fix compile/runtime issues: `IAsyncLifetime` using, `ConfigureTestServices` namespace, EventSub HMAC bypass via config override.
2. Rate limiting: run `UseRateLimiter` after auth so Extension JWT partitions limits; cap perf test iterations under 5 req/min per user.
3. Isolation: truncate PostgreSQL `mgm` before host boot and after; unique IDs in HTTP scenario tests; optional per-test truncate in MediatR verify test; xunit single-threaded runner for the integration assembly.
4. Solution + CI: ensure `slnx`/`sln` include IntegrationTests; GitHub workflow tests `slnx`.
5. Documentation: IntegrationTests README, UnitTests README split, INTERACTION_SCENARIOS automation line.

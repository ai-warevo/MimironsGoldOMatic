## Goal

Complete Sub-prompt C: align `Backend.Api` host with legacy middleware (rate limiting after auth) and verify build; smoke-run when Postgres is available.

## Changes

- [src/MimironsGoldOMatic.Backend.Api/Program.cs](src/MimironsGoldOMatic.Backend.Api/Program.cs): register `AddRateLimiter` (same partitioning as legacy) and `UseRateLimiter` after `UseAuthorization`.
- [src/MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.http](src/MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.http): point sample request at `GET /api/version` (remove template weather route).

## Verify

- `dotnet build src/MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.csproj`
- Optional: `dotnet run --project ...` and `GET /api/version` when `ConnectionStrings:PostgreSQL` is set.

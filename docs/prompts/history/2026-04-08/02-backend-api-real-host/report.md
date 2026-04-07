## Summary

Sub-prompt C (host parity): added **global rate limiting** in `Backend.Api` to match legacy `Backend/Program.cs` (EventSub bypass, fixed-window keyed by user/IP), with **`UseRateLimiter`** after auth. Updated `Backend.Api.http` to smoke **`GET /api/version`**.

## Modified files

- `src/MimironsGoldOMatic.Backend.Api/Program.cs`
- `src/MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.http`
- `docs/prompts/history/2026-04-08/02-backend-api-real-host/*` (audit)

## Verification

- `dotnet build src/MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.csproj` — success (0 warnings, 0 errors).
- `dotnet run` — fails immediately in this environment without **`ConnectionStrings:PostgreSQL`** (expected from `AddMgmBackend`). Set that (e.g. user secrets or env) and re-run; then hit `GET /api/version` and OpenAPI in Development.

## Next

- Sub-prompt D: repoint integration tests to `Backend.Api`.
- Optional: document or add Development sample connection string **only** if team agrees (avoid committing real secrets).

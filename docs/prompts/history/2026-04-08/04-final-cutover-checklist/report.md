## Cutover Checklist (Sub-prompt E)

### Build gates (already verified)

- `dotnet build src/MimironsGoldOMatic.sln -c Debug` ✅
- `dotnet build src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.csproj -c Debug` ✅

### Consumers to repoint (no deletions yet)

1. Integration tests
   - `src/Tests/MimironsGoldOMatic.Backend.IntegrationTests` now boots `MimironsGoldOMatic.Backend.Api`.
   - `src/Tests/MimironsGoldOMatic.Desktop.IntegrationTests` now references `MimironsGoldOMatic.Backend.Api`.
   - Note: integration tests require Docker for Testcontainers; without Docker, `dotnet test` will fail before hitting code.

2. Runtime entrypoints (when you’re ready to cut over fully)
   - Any external callers (Desktop, WoW addon, Twitch extension) that are currently pointed at `MimironsGoldOMatic.Backend` should be repointed to `MimironsGoldOMatic.Backend.Api`.

### Config keys that must match between legacy and new host

Both hosts ultimately rely on the same option sections / Marten connection string:

- Marten / PostgreSQL:
  - `ConnectionStrings:PostgreSQL` (required by `AddMgmBackend` in the new host)
- Desktop API key auth:
  - `Mgm:ApiKey` (header `X-MGM-ApiKey`, used by the `ApiKey` scheme)
- Twitch extension JWT auth:
  - `Twitch:ExtensionClientId` (audience validation)
  - `Twitch:ExtensionSecret` (base64 signing secret; required outside Development)
  - `Twitch:EventSubSecret` (EventSub HMAC signature verification; can be empty when bypassing in tests/dev)
- Subscriber gate and external integrations:
  - `Mgm:DevSkipSubscriberCheck` (default behavior should be consistent with legacy; tests use `true`)
  - `Twitch:HelixClientId` / `Twitch:HelixClientSecret` / `Twitch:BroadcasterAccessToken` / `Twitch:BroadcasterUserId`

Rate limiting (host-level parity):
- Rate limiting is now wired in `Backend.Api` host (global, after auth) with the same partitioning logic as legacy:
  - EventSub requests under `/api/twitch/eventsub` are not rate-limited.
  - Otherwise: fixed window, keyed by `user_id`/`NameIdentifier`/IP fallback.

### Smoke tests (minimum)

Run the new host in Development with required config and then validate:

1. `GET /api/version` returns `200` with the expected payload shape.
2. OpenAPI is available in Development (via `app.MapOpenApi()`).
3. Optional equivalence checks:
   - Ensure `429` behavior matches legacy by quickly issuing multiple requests under the cap to an authenticated route.

### Notes / operational guardrails

- The new host (`Backend.Api`) currently hard-requires `ConnectionStrings:PostgreSQL`; set it before running `dotnet run`.
- Legacy host remains intact; cutover is safe to do incrementally by switching only the consumers you choose (start with tests).


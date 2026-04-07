## Plan

1. Reconfirm compile gates:
   - `dotnet build src/MimironsGoldOMatic.sln -c Debug`
   - `dotnet build src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.csproj -c Debug`
2. Produce cutover checklist sections:
   - Repoint consumers (tests + any runtime entrypoints)
   - Required matching config keys (auth, rate limiting, Marten connection)
   - Smoke steps to validate equivalence (`GET /api/version` + OpenAPI in Development)
3. Write the result into `docs/prompts/history/2026-04-08/05-final-cutover-checklist/report.md`.


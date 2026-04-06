### User request

Implement build-time C# to TypeScript code generation for DTO models and an Axios-based API client:

- Trigger from `src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.csproj`.
- Generate:
  - `src/MimironsGoldOMatic.TwitchExtension/src/api/models.ts`
  - `src/MimironsGoldOMatic.TwitchExtension/src/api/client.ts`
- Discover DTOs and endpoints from backend C# sources.
- Apply required type mapping and nullable behavior.
- Keep generation idempotent.
- Ensure Axios dependency is present in Twitch Extension.
- Add usage/build documentation and validate with builds.

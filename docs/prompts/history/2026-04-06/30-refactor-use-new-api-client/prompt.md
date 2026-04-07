### User request

Refactor `src/MimironsGoldOMatic.TwitchExtension` to replace all usages of the old API client (`mgmEbsRepository`) with the newly generated `src/api/client.ts` (`MimironsGoldOMaticApiClient`) and `src/api/models.ts`.

Constraints:
- Preserve behavior (UI + polling + claim flow).
- Keep changes minimal and type-safe.
- Ensure `npm run build` succeeds with no TypeScript errors.
- Remove all imports/usages of `mgmEbsRepository` when done.


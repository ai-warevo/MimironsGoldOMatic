## Report

### Root cause

`MimironsGoldOMatic.ApiTsGen` only indexed/considered DTO types whose names matched a suffix whitelist (Request/Response/Dto/Status). The PATCH payload DTO `PatchGiftRequestState` ends with `State`, so it was excluded from `models.ts` even though endpoint parsing still caused `client.ts` to import and use it.

### Fix summary

Extended the generator’s DTO emission predicate to include types starting with `Patch`, ensuring patch payload DTOs are added to the DTO map and therefore can be emitted into `models.ts` when referenced by endpoints.

### Modified files

- `src/tools/MimironsGoldOMatic.ApiTsGen/Program.cs`
- `docs/prompts/history/2026-04-06/34-fix-api-tsgen-patchgiftrequeststate/*`

### Verification results

- `dotnet build src/MimironsGoldOMatic.slnx` ✅ (regenerated `models.ts`/`client.ts`)
- `npm run build` in `src/MimironsGoldOMatic.TwitchExtension` ✅
- Confirmed `models.ts` now exports `PatchGiftRequestState` ✅

### Definition of Done

- [x] Scope implemented
- [x] No new lints introduced (no lints added by this change)
- [x] Verification complete
- [x] `docs/prompts/templates/definition_of_done.md` checked

### Potential technical debt

- The generator still lacks a guard/test ensuring every model token imported by generated `client.ts` is exported by generated `models.ts`. Adding a generator self-check (fail generation with a clear error) would prevent similar issues.


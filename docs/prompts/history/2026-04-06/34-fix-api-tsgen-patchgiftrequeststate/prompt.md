# Bugfix: TS API generator missing `PatchGiftRequestState`

## Symptoms

- **User-visible impact**: `npm run build` fails in the Twitch Extension because generated `client.ts` imports `PatchGiftRequestState` from `models.ts`, but `models.ts` does not export it.
- **Frequency**: always (after generator runs).
- **First observed**: 2026-04-06 (Tier C / `!twgift` work).
- **Regression?**: yes (new DTO introduced that does not match generator type-emission heuristics).

## Reproduction

- **Steps to reproduce**:
  - Run `dotnet build src/MimironsGoldOMatic.slnx` (regenerates `src/MimironsGoldOMatic.TwitchExtension/src/api/models.ts` and `client.ts`).
  - Run `npm run build` in `src/MimironsGoldOMatic.TwitchExtension`.
- **Expected result**: Extension builds successfully.
- **Actual result**: TypeScript compile error `TS2305: Module '"./models"' has no exported member 'PatchGiftRequestState'.`
- **Environment**: Windows 10, PowerShell, Node/npm, Vite, TypeScript.

## Root cause analysis

- **Root cause**: `MimironsGoldOMatic.ApiTsGen` only emits DTO types whose names match a suffix whitelist (Request/Response/Dto/Status). `PatchGiftRequestState` ends with `State`, so it is not emitted into `models.ts`, but endpoints still cause `client.ts` to reference it.
- **Why it happened**: DTO naming for PATCH payload didn’t follow the generator’s `ShouldEmitType` convention.
- **Why it wasn’t caught**: No generator-level test validating “every imported model token in client.ts is exported by models.ts”.

## Fix approach

- **Proposed fix**: Update the generator’s model emission predicate to include patch DTO naming (e.g. types starting with `Patch`), so referenced patch payload types appear in `models.ts`.
- **Alternative considered**: Rename the DTO to end with `Request`. Rejected (API contract churn + not ideal naming for patch payloads).
- **Compatibility / migration**: Generator-only change; regenerated TS API becomes consistent without manual edits.
- **Risk assessment**: Low. May emit a small number of additional types; client correctness improves.

## Tests / verification

- **Manual verification steps**:
  - Run `dotnet build src/MimironsGoldOMatic.slnx` and confirm regenerated `models.ts` exports `PatchGiftRequestState`.
  - Run `npm run build` in the Twitch Extension.

## Prevention

- **Follow-up tasks**: Add a generator self-check (or unit test) to assert `client.ts` imports are a subset of `models.ts` exports.

## Definition of Done

- [ ] Bug no longer reproduces
- [ ] No new lints introduced
- [ ] `report.md` includes root cause, verification, and `Potential technical debt`
- [ ] `docs/prompts/templates/definition_of_done.md` checked


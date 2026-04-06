## Task outcome

Implemented Twitch Extension "Check for Updates" end-to-end using `GET /api/version` with a silent background lifecycle and unobtrusive viewer banner.

## Modified files

- `src/MimironsGoldOMatic.TwitchExtension/vite.config.ts`
- `src/MimironsGoldOMatic.TwitchExtension/src/vite-env.d.ts`
- `src/MimironsGoldOMatic.TwitchExtension/src/config/mgmClientVersion.ts`
- `src/MimironsGoldOMatic.TwitchExtension/src/mgmTypes.ts`
- `src/MimironsGoldOMatic.TwitchExtension/src/api/mgmVersionApi.ts`
- `src/MimironsGoldOMatic.TwitchExtension/src/utils/mgmVersion.ts`
- `src/MimironsGoldOMatic.TwitchExtension/src/utils/mgmVersion.test.ts`
- `src/MimironsGoldOMatic.TwitchExtension/src/components/MgmUpdateBanner.tsx`
- `src/MimironsGoldOMatic.TwitchExtension/src/components/MgmUpdateBanner.test.tsx`
- `src/MimironsGoldOMatic.TwitchExtension/src/components/ViewerPanel.tsx`
- `src/MimironsGoldOMatic.TwitchExtension/src/index.css`
- `docs/components/twitch-extension/ReadME.md`
- `docs/prompts/history/2026-04-06/08-twitch-extension-update-check/prompt.md`
- `docs/prompts/history/2026-04-06/08-twitch-extension-update-check/plan.md`
- `docs/prompts/history/2026-04-06/08-twitch-extension-update-check/checks.md`

## Verification results

- `npm run lint` (Twitch Extension): passed.
- `npm run test:unit` (Twitch Extension): passed (9 suites, 32 tests).
- `npm run build` (Twitch Extension): passed (`tsc -b` + `vite build`).
- IDE lint diagnostics (`ReadLints`) for edited TS/TSX files: no issues.

## Definition of Done check

- [x] Scope implemented according to request.
- [x] Behavior covered by tests (`mgmVersion` + `MgmUpdateBanner` tests added).
- [ ] Existing tests pass (`dotnet test src/MimironsGoldOMatic.sln`) after changes.  
  Not run because this task is scoped to the Extension package; Extension test/build gates were run.
- [x] No new linter/format issues introduced.
- [x] User-facing docs updated (Twitch Extension component README endpoint notes).
- [x] Code comments in English where added.
- [x] User-facing strings aligned (Russian update banner copy as requested).
- [x] Error handling explicit for new failure paths (silent console logging, no blocking UI).
- [x] Risks/rollback impact documented (`plan.md` + notes below).
- [x] `Potential technical debt` section included.

## Potential technical debt

- Version comparison currently treats prerelease suffixes as base `MAJOR.MINOR.PATCH`; if strict prerelease precedence is needed, use a dedicated semver parser library.
- Update check is panel-local state in `ViewerPanel`; if multiple panels/routes are added later, centralizing update state in store/context may reduce duplication.
- `GET /api/version` is called through the same JWT-enabled axios client; if endpoint auth behavior diverges by environment, a public client path may be clearer.


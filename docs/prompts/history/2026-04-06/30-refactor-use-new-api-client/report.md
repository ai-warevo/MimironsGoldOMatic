## Summary

Refactored the Twitch Extension to use the generated Axios client (`src/api/client.ts`) and generated DTOs (`src/api/models.ts`) instead of the legacy `mgmEbsRepository`.

## Modified files

- Added: `src/MimironsGoldOMatic.TwitchExtension/src/api/mgmApiClient.ts`
- Updated: `src/MimironsGoldOMatic.TwitchExtension/src/hooks/useMgmEbsPolling.ts`
- Updated: `src/MimironsGoldOMatic.TwitchExtension/src/components/ViewerPanel.tsx`
- Updated: `src/MimironsGoldOMatic.TwitchExtension/src/state/mgmPanelStore.ts`
- Updated: `src/MimironsGoldOMatic.TwitchExtension/src/hooks/useMgmSpinCountdown.ts`
- Updated: `src/MimironsGoldOMatic.TwitchExtension/src/components/RouletteVisual.tsx`
- Updated tests:
  - `src/MimironsGoldOMatic.TwitchExtension/src/hooks/useMgmEbsPolling.test.tsx`
  - `src/MimironsGoldOMatic.TwitchExtension/src/hooks/useMgmSpinCountdown.test.tsx`
  - `src/MimironsGoldOMatic.TwitchExtension/src/state/mgmPanelStore.test.ts`
- Deleted:
  - `src/MimironsGoldOMatic.TwitchExtension/src/api/mgmEbsRepository.ts`
  - `src/MimironsGoldOMatic.TwitchExtension/src/api/mgmEbsRepository.test.ts`

## Verification

- `npm run build` in `src/MimironsGoldOMatic.TwitchExtension` (tsc + vite) succeeded.

## Potential technical debt

- Generated `RouletteStateResponse.spinPhase` is typed as `string`, while the UI treats it as a known finite set. Consider generating a union type (or exporting a `SpinPhase` enum/union) to improve type-safety.
- `MimironsGoldOMaticApiErrorBody` remains hand-maintained (not generated). If the backend error schema is stable, consider generating it as well.


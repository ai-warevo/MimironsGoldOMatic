## Context
- **Problem**: Viewers may have a stale Extension build cached; there is no in-panel hint that a newer version exists.
- **Why now**: Backend exposes `GET /api/version`, and Desktop + Addon already track update-check work; Extension should match.
- **Constraints**: Must not interfere with roulette/polling; update checks must be silent on failure.

## Proposed solution
- Add a small Version API wrapper to call `GET /api/version` using the existing Axios EBS client.
- Bake the Extension version into the build via `import.meta.env.VITE_APP_VERSION` (defaulting to package.json version via Vite define).
- Add a safe semver compare helper (no-throw; numeric compare with graceful fallback).
- In `MimironsGoldOMaticViewerPanel`, kick off a background version check when `VITE_MGM_EBS_BASE_URL` is present, and optionally poll at low frequency (60 min).
- Show an unobtrusive banner only when server version is newer than local, with “Перезагрузить” (reload) and optional “Подробнее” (release notes URL).

## Affected files (expected)
- `src/MimironsGoldOMatic.TwitchExtension/vite.config.ts`
- `src/MimironsGoldOMatic.TwitchExtension/src/vite-env.d.ts` (env typing)
- `src/MimironsGoldOMatic.TwitchExtension/src/api/mgmVersionApi.ts` (new)
- `src/MimironsGoldOMatic.TwitchExtension/src/config/mgmClientVersion.ts` (new)
- `src/MimironsGoldOMatic.TwitchExtension/src/utils/mgmVersion.ts` (new + tests)
- `src/MimironsGoldOMatic.TwitchExtension/src/components/MgmUpdateBanner.tsx` (new)
- `src/MimironsGoldOMatic.TwitchExtension/src/components/ViewerPanel.tsx` (wire-in)
- `src/MimironsGoldOMatic.TwitchExtension/src/index.css` (banner styles)

## Risks & mitigations
- **EBS requires auth**: use the existing JWT injector; if token missing, request simply fails silently.
- **Malformed version strings**: helper falls back and never throws; on parse failure treat as “no update”.
- **Twitch sandbox**: use plain link with `rel="noopener noreferrer"`; do not open popups/modals.

## Testing plan
- Unit tests for version compare helper (edge cases).
- Jest/RTL smoke test for banner rendering (optional if setup exists; otherwise helper tests only).
- Local manual: set EBS version higher/lower and verify banner appears/hidden; click reload triggers `window.location.reload()`.


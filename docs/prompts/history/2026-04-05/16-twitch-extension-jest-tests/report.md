# Report: Twitch Extension Jest tests

## Modified / added

- `package.json` — scripts: `test`, `test:jest`, `test:unit`, `test:integration`; devDependencies: Jest, ts-jest, axios-mock-adapter, eslint-plugin-jest, etc.; removed Vitest.
- `jest.config.cjs`, `tsconfig.jest.json` — Jest + TypeScript.
- `vite.config.ts` — Vitest block removed (Vite build only).
- `src/test/jest.setup.ts` — `@testing-library/jest-dom/jest-globals` (replaces Vitest-only `setup.ts`).
- `eslint.config.js` — Vitest plugin replaced with `eslint-plugin-jest`.
- `README.md` (TwitchExtension) — Jest commands; integration via axios-mock-adapter.
- `.github/workflows/unit-integration-tests.yml` — step text updated (Jest).
- Tests: `mgmPanelStore.test.ts`, `mgmEbsRepository.test.ts`, `mgmEbsClient.integration.test.ts` (rewritten), `useMgmSpinCountdown.test.tsx`, `useMgmEbsPolling.test.tsx`, `rewardSentAnnouncement.test.ts`, plus migrated `RouletteVisual`, `mapMgmApiErrorToUi`.
- Removed: `src/test/setup.ts` (Vitest), `src/test/msw/server.ts` (MSW not used with Jest in this package).

## Verification

- `npm test` in `src/MimironsGoldOMatic.TwitchExtension`: ESLint, Jest (25 tests), `tsc -b` + Vite build — all passed.

## Note

HTTP integration tests use **axios-mock-adapter** on the real `AxiosInstance` from `createMimironsGoldOMaticEbsClient`, avoiding MSW + Jest resolution issues with ESM-only transitive deps under jsdom.

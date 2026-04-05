# Plan: Twitch Extension Jest tests

1. Replace Vitest with Jest (`ts-jest`, `jsdom`, MSW setup unchanged).
2. Add `jest.config.cjs`, `tsconfig.jest.json`, `src/test/jest.setup.ts`; remove Vitest-only `setup.ts`.
3. Migrate existing tests to Jest APIs; add store, repository, countdown hook, polling hook, reward announcement tests.
4. Update `package.json` scripts, `vite.config.ts`, `eslint.config.js`, CI workflow copy, README test section.

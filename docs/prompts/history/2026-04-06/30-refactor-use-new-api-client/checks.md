## Checks

- [x] Discovery: locate all `mgmEbsRepository` usages and DTO imports.
- [x] Add shared `MimironsGoldOMaticApiClient` initializer for the extension.
- [x] Refactor `useMgmEbsPolling` to call generated client directly.
- [x] Refactor `ViewerPanel` (dev claim path) to call generated client directly.
- [x] Update tests to mock the new client/initializer (remove repo mocks).
- [x] Remove remaining `mgmEbsRepository` usage and validate no imports remain.
- [x] Run `npm run build` and fix any TypeScript errors.
- [x] Write `report.md` (include verification + potential technical debt).


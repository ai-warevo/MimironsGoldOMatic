## Refactor plan

1. Discover all imports/usages of `mgmEbsRepository` and list affected files.
2. Introduce a single shared API client initializer for the extension that:
   - Uses `MimironsGoldOMaticApiClient` from `src/api/client.ts`.
   - Reads the EBS base URL already used by the extension.
   - Injects the Extension JWT via the existing token provider.
3. Refactor hook/component call sites:
   - `useMgmEbsPolling`: replace repository calls with `client.getRouletteState()`, `client.getPoolMe()`, `client.getPayoutsMyLast()` and preserve the 404→null behavior.
   - `ViewerPanel` dev-claim: replace `repo.postClaim()` with `client.postPayoutsClaim()`, preserving existing error UX (including structured API errors).
4. Update type imports to use `src/api/models.ts` where applicable, without changing runtime behavior.
5. Update tests to mock the new API client initializer / `MimironsGoldOMaticApiClient` instead of `mgmEbsRepository`.
6. Remove any remaining `mgmEbsRepository` imports/usages; keep the file only if still needed elsewhere (otherwise delete in a final cleanup).
7. Validate:
   - Run `npm run build` in `src/MimironsGoldOMatic.TwitchExtension`.
   - Spot-check extension runtime flows (polling + dev claim).

## Risks / mitigations

- **Axios error shape differences**: keep existing error mapping logic and only adapt the boundary where calls changed.
- **DTO name differences**: update type imports to the generated model names and map values only at the boundary.


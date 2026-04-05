# Task prompt (archived)

Optimize the E2E pipeline and create a comprehensive release workflow with fully separated component builds and artifact publishing (sequential release job).

Goals:

- Fast Tier A E2E on PRs (without Desktop/addons).
- Full multi-component build and release after merge to `main`.
- Build/publish Desktop (Windows), WoWAddon (Linux), TwitchExtension (Linux), Backend Docker (GHCR).
- `create-release` only after all build jobs succeed.
- Update `docs/E2E_AUTOMATION_PLAN.md` and `docs/E2E_AUTOMATION_TASKS.md`.

Constraints: do not run pipelines locally; preserve E2E env/secrets pattern; keep Python/curl and Postgres service block unchanged in E2E; README.txt inside all ZIPs.

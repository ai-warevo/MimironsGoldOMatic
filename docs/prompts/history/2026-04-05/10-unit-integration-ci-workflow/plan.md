# Plan: unit-integration-tests workflow

## Changes

1. Add `.github/workflows/unit-integration-tests.yml` with `pull_request` → `main`, four parallel `test-*` jobs, `aggregate-results` with `actions/github-script` (upsert PR comment, `continue-on-error` for fork/token limits).
2. Repo gaps: no `Desktop.Tests` and no `npm test` — add minimal `MimironsGoldOMatic.Desktop.Tests` (WPF smoke), register in `MimironsGoldOMatic.slnx`; add `"test": "npm run lint && npm run build"` to Twitch Extension `package.json`.
3. Extend `docs/e2e/E2E_AUTOMATION_PLAN.md` with **Unit and Integration Testing Strategy** + doc control row + CI bullet update.

## Risks

- Fork PRs may not get the summary comment (token permissions); step is best-effort.
- Desktop Release build pulls `win-x64` RID from Desktop csproj; acceptable for `windows-latest` CI.

## Confirmation

Low-risk; no changes to existing workflows.

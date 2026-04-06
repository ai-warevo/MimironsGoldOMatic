# Report: Project structure alignment + Tier B finalization (2026-04-06)

## Modified / added files

| Area | Files |
|------|--------|
| Structure | `docs/reference/PROJECT_STRUCTURE.md` |
| E2E | `docs/e2e/E2E_AUTOMATION_PLAN.md`, `docs/e2e/TIER_B_POSTLAUNCH_VERIFICATION.md`, `docs/e2e/TIER_C_REQUIREMENTS.md` (new), `docs/e2e/TIER_B_TEAM_ANNOUNCEMENT.md` (new) |
| CI | `.github/workflows/e2e-test.yml` |
| Components | `docs/components/backend/ReadME.md`, `docs/components/desktop/ReadME.md`, `docs/components/wow-addon/ReadME.md` |
| Index / readiness | `docs/ReadME.md`, `docs/reference/IMPLEMENTATION_READINESS.md` |
| Agent history | `docs/prompts/history/2026-04-06/01-project-structure-tier-b-docs/*` |

## Verification

- **`dotnet build src/MimironsGoldOMatic.slnx -c Release`**: attempted on the workspace host; **failed** because `MimironsGoldOMatic.Mocks.MockHelixApi.exe` was locked by a running local process (environment contention). No code changes were required for that failure. Re-run after stopping any local mock processes.

## Technical debt / follow-ups

- Add **`packages.lock.json`** (optional central package management) to tighten NuGet cache keys further.
- Pin a **specific** green Actions run URL in release notes when a milestone is tagged (docs use the workflow list + instructions by design).

## Team discussion hooks

- **Docker** for mocks vs current `dotnet run` in CI (maintenance vs startup time).
- **CI cost**: `concurrency` reduces duplicate runs; optional **path filters** for `docs-only` PRs remain a policy choice.
- **Tier C** priority: self-hosted Windows E2E vs staging Twitch vs addon tests — see `TIER_C_REQUIREMENTS.md`.

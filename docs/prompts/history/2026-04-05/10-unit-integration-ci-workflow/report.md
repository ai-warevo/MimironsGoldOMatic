# Report: unit-integration CI workflow

## Modified / added files

- `.github/workflows/unit-integration-tests.yml` (new)
- `src/MimironsGoldOMatic.Desktop.Tests/MimironsGoldOMatic.Desktop.Tests.csproj` (new)
- `src/MimironsGoldOMatic.Desktop.Tests/DesktopSmokeTests.cs` (new)
- `src/MimironsGoldOMatic.slnx` (Desktop.Tests project path)
- `src/MimironsGoldOMatic.TwitchExtension/package.json` (`test` script)
- `docs/E2E_AUTOMATION_PLAN.md` (new section + doc control + CI overview line)

## Verification

- `dotnet test src/MimironsGoldOMatic.Backend.Tests/...` Release: **29 passed**
- `dotnet test src/MimironsGoldOMatic.Desktop.Tests/...` Release: **1 passed**
- `npm ci` + `npm test` in Twitch Extension: **success**

## Technical debt / follow-ups

- WoW addon: replace syntax-only job with a real Lua test runner when available.
- Twitch Extension: add Vitest + coverage artifacts.
- Backend: optional job split if runtime grows.

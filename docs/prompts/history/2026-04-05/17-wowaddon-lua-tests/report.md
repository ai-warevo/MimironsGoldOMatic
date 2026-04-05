# Report: WoW addon Lua tests

## Modified / added files
- `src/MimironsGoldOMatic.WoWAddon/MimironsGoldOMatic.Core.lua` (new)
- `src/MimironsGoldOMatic.WoWAddon/MimironsGoldOMatic.lua` (uses Core)
- `src/MimironsGoldOMatic.WoWAddon/MimironsGoldOMatic.toc` (loads Core then main)
- `src/Tests/MimironsGoldOMatic.WoWAddon.Tests/` — LuaUnit, mock, unit/integration tests, runners
- `.github/workflows/unit-integration-tests.yml` — WoW job runs `luac` + `lua run_tests.lua`
- `.github/workflows/release.yml` — packs `MimironsGoldOMatic.Core.lua` in addon zip

## Verification
- Local `lua` was not available on the dev machine; CI runs `lua run_tests.lua` after `apt install lua5.4`.
- Logic reviewed: integration tests use `WOW_MOCK_CREATED_FRAMES[1]` as event frame after mock no longer tracks UIParent/Minimap.

## Technical notes
- `lib/luaunit.lua` is vendored (BSD) from LuaUnit 3.4.

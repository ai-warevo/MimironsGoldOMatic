# Plan: WoW addon Lua tests

## Approach
- Extract pure helpers into `MimironsGoldOMatic.Core.lua` loaded before main addon (`.toc` order) so parsing/WHO JSON/name logic is unit-testable headlessly.
- Vendor LuaUnit in `WoWAddon.Tests/lib/`.
- Provide `wow_api_mock.lua` stubs for CreateFrame, chat, /who, hooksecurefunc, etc.
- Unit tests: `UnitTests/core_tests.lua` against Core only.
- Integration tests: `IntegrationTests/addon_integration_tests.lua` loads mock + Core + main addon; drives `OnEvent` and public globals.

## Risks
- WoW client uses Lua 5.1; CI uses Lua 5.4 — keep syntax compatible (no goto, avoid 5.4-only features in addon code).

## Files touched
- Addon: new Core.lua, main lua refactor, .toc
- Tests: new tree under `src/Tests/MimironsGoldOMatic.WoWAddon.Tests/`
- CI: `unit-integration-tests.yml` WoW job; `release.yml` addon zip copies Core.lua

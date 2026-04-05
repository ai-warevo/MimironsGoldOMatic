--[[
  LuaUnit runner for WoW addon tests (requires Lua 5.1+ / LuaJIT / Lua 5.4).
  Usage: lua run_tests.lua
]]

local TEST_ROOT = "."
if arg and arg[0] and arg[0] ~= "" then
  local p = arg[0]
  local d = p:match("^(.*[/\\])")
  if d then
    TEST_ROOT = d
  end
end

package.path = TEST_ROOT .. "lib/?.lua;" .. package.path

CORE_LUA = TEST_ROOT .. "../MimironsGoldOMatic.WoWAddon/MimironsGoldOMatic.Core.lua"
ADDON_LUA = TEST_ROOT .. "../MimironsGoldOMatic.WoWAddon/MimironsGoldOMatic.lua"
MOCK_LUA = TEST_ROOT .. "wow_api_mock.lua"

local lu = require("luaunit")

dofile(TEST_ROOT .. "UnitTests/core_tests.lua")
dofile(TEST_ROOT .. "IntegrationTests/addon_integration_tests.lua")

os.exit(lu.LuaUnit.run())

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

-- When arg[0] is bare "run_tests.lua", TEST_ROOT is "." — concatenating "." .. "lib/?.lua"
-- yields ".lib/?.lua" (wrong). Use a prefix that is either "" or a directory ending in / or \.
local ROOT_PREFIX = (TEST_ROOT == "." and "" or TEST_ROOT)

package.path = ROOT_PREFIX .. "lib/?.lua;" .. package.path

CORE_LUA = ROOT_PREFIX .. "../MimironsGoldOMatic.WoWAddon/MimironsGoldOMatic.Core.lua"
ADDON_LUA = ROOT_PREFIX .. "../MimironsGoldOMatic.WoWAddon/MimironsGoldOMatic.lua"
MOCK_LUA = ROOT_PREFIX .. "wow_api_mock.lua"

local lu = require("luaunit")

dofile(ROOT_PREFIX .. "UnitTests/core_tests.lua")
dofile(ROOT_PREFIX .. "IntegrationTests/addon_integration_tests.lua")

os.exit(lu.LuaUnit.run())

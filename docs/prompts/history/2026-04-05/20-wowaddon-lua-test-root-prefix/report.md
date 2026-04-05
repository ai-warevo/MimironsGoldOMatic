# Report

## Modified files

- `src/Tests/MimironsGoldOMatic.WoWAddon.Tests/run_tests.lua` — correct path prefix when script is invoked as `run_tests.lua` from cwd (CI / `python3 run_tests.py`).

## Verification

- Error text `no file '.lib/luaunit.lua'` matches broken concat `"." .. "lib/?.lua"` → `".lib/?.lua"`. No Lua in local PATH; CI will re-run tests.

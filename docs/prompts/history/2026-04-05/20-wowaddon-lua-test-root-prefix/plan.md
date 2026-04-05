# Plan

1. Diagnose: `run_tests.lua` sets `TEST_ROOT` to `"."` when `arg[0]` is bare `run_tests.lua`; then `TEST_ROOT .. "lib/?.lua"` becomes `".lib/?.lua"` instead of `"lib/?.lua"`.
2. Fix: introduce `ROOT_PREFIX = (TEST_ROOT == "." and "" or TEST_ROOT)` and use it for `package.path`, `CORE_LUA` / `ADDON_LUA` / `MOCK_LUA`, and `dofile` paths.

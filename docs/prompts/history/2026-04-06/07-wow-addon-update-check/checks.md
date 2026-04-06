## Checks

- [x] Locate existing `/mgm` handler and protocol tags in addon Lua code.
- [x] Add update-check helper to emit `[MGM_UPDATE_CHECK]` and user-facing status text.
- [x] Extend slash command handling for `update` and `checkupdate` while preserving fallback behavior.
- [x] Add integration test for update-check slash command path.
- [!] Run test suite for WoW addon and capture results. (`lua` executable is missing on PATH in current environment.)

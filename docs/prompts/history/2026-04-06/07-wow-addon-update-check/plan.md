## Plan

1. Inspect the existing addon command registration and communication tag emit points in `src/MimironsGoldOMatic.WoWAddon/MimironsGoldOMatic.lua`.
2. Add a focused helper for update-check request flow that:
   - prints local status messages;
   - emits `[MGM_UPDATE_CHECK]` verbatim to chat.
3. Extend `/mgm` slash handler with additive subcommands (`update`, `checkupdate`) while preserving prior default behavior.
4. Add an integration test in `src/Tests/MimironsGoldOMatic.WoWAddon.Tests/IntegrationTests/addon_integration_tests.lua` to verify emitted tag and messages.
5. Run WoW addon tests and record outcomes.

## Risks

- Changing slash command parsing could break existing `/mgm` panel toggle if not kept as fallback.
- Message text assertions in tests can be brittle if phrasing changes.

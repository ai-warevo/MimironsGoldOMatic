Implement the WoW addon "check for updates" flow from `tmp/cursor_prompt_wow_addon_updates.md`:

- Add `/mgm update` and `/mgm checkupdate` command handling.
- Print immediate user-facing status/guidance messages in chat.
- Emit `[MGM_UPDATE_CHECK]` into chat so Desktop can detect it in `WoWChatLog.txt`.
- Keep existing tags and flows unchanged and backward-compatible.
- Add/update tests for the slash-command behavior.

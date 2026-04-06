Summary for github issue #19 – Check for Updates feature planning

Modified/created files:
- `docs/prompts/history/2026-04-06/05-github-issue-19-check-for-updates/prompt.md`
- `docs/prompts/history/2026-04-06/05-github-issue-19-check-for-updates/plan.md`
- `docs/prompts/history/2026-04-06/05-github-issue-19-check-for-updates/checks.md`
- `docs/prompts/history/2026-04-06/05-github-issue-19-check-for-updates/report.md` (this file)
- `tmp/updates_implementation_plan.md`
- `tmp/cursor_prompt_desktop_updates.md`
- `tmp/cursor_prompt_twitch_extension_updates.md`
- `tmp/cursor_prompt_wow_addon_updates.md`
- `.gitignore` (updated to ignore `tmp/`)

Verification:
- No source code under `src/` was modified; this task is documentation and planning only.
- The four `tmp/*.md` files contain the requested implementation plan and component-specific Cursor prompts for the Check for Updates feature.
- `.gitignore` ensures the `tmp/` directory contents remain untracked by Git.

Outstanding items / technical debt:
- The actual implementation of the Check for Updates feature in Backend, Desktop, Twitch Extension, and WoW Addon still needs to be executed, using the new planning docs as guidance.

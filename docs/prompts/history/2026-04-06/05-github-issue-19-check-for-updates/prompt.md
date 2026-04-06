github issue #19 – Check for Updates feature planning

User request (abridged):

- Generate a comprehensive implementation plan for the «Check for Updates» feature across all four components (Twitch Extension, EBS, Desktop WPF, WoW Addon).
- Produce four markdown files:
  - `updates_implementation_plan.md` – high-level cross-component plan.
  - `cursor_prompt_desktop_updates.md` – Cursor implementation prompt for Desktop (WPF).
  - `cursor_prompt_twitch_extension_updates.md` – Cursor implementation prompt for Twitch Extension.
  - `cursor_prompt_wow_addon_updates.md` – Cursor implementation prompt for WoW Addon (Lua, 3.3.5a).
- Place the new doc files under a `tmp` directory at the project root and update `.gitignore` to ignore that directory.

Key constraints and context:

- Monorepo structure under `src/` with `MimironsGoldOMatic.*` projects for Backend (EBS), Desktop, Twitch Extension, and WoW Addon.
- Follow `AGENTS.md` workflow (history, plan, checks, report).
- EBS will expose a version endpoint that Desktop and Twitch Extension call directly; WoW Addon goes via Desktop as an intermediary.

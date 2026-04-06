High-level plan for github issue #19 – Check for Updates feature

1. Analyze project structure and existing docs to align with architecture:
   - Confirm locations of Backend (EBS), Desktop (WPF), Twitch Extension, and WoW Addon projects under `src/`.
   - Skim `docs/overview/ARCHITECTURE.md` and any component-specific specs if necessary to ensure the plan respects current flows and naming conventions.
2. Design the cross-component update architecture:
   - Define the EBS version endpoint (route, auth, payload shape) as a single source of truth for public version metadata.
   - Specify how Desktop and the Twitch Extension call this endpoint (timing, caching, failure behavior).
   - Define the WoW Addon ↔ Desktop protocol for update checks, including new chat markers or message formats if needed.
3. Draft `updates_implementation_plan.md`:
   - Describe the overall feature purpose and UX goals.
   - Document the request flows:
     - Desktop → EBS.
     - Twitch Extension → EBS.
     - WoW Addon → Desktop → EBS → Desktop → WoW Addon.
   - Enumerate data contracts (DTOs, JSON schema, chat tags) and error-handling strategies.
   - Outline testing strategies (unit, integration, manual) and deployment/versioning notes.
4. Draft `cursor_prompt_desktop_updates.md`:
   - Provide a self-contained Cursor prompt targeting the WPF/Desktop project.
   - Include role, goals, requirements (update polling, UI indicators, user prompts, WoW Addon protocol compatibility), constraints, and output format expectations.
   - Add minimal example snippets (e.g., HttpClient-based version check, simple WPF binding example).
5. Draft `cursor_prompt_twitch_extension_updates.md`:
   - Provide a self-contained Cursor prompt for the Vite/React Twitch Extension.
   - Define when/how the extension checks the EBS endpoint, how to present update information non-intrusively, and any API doc adjustments.
   - Include example TypeScript fetch code and a small UI example for the banner/notification.
6. Draft `cursor_prompt_wow_addon_updates.md`:
   - Provide a self-contained Cursor prompt for the WoW 3.3.5a Lua addon.
   - Define a new `/mgm` subcommand for update checks and the message format for communicating with Desktop.
   - Describe expected behavior when Desktop is offline and the feedback to show in chat.
   - Provide example Lua snippets for the slash command handler and chat output.
7. Wire artifacts into the repo:
   - Create a `tmp/` directory at the repo root (if not present) and add:
     - `tmp/updates_implementation_plan.md`
     - `tmp/cursor_prompt_desktop_updates.md`
     - `tmp/cursor_prompt_twitch_extension_updates.md`
     - `tmp/cursor_prompt_wow_addon_updates.md`
   - Update `.gitignore` to ignore the `tmp/` directory so these planning artifacts stay untracked.
8. Create and update `checks.md` during execution:
   - Track completion of analysis, plan file, and the three Cursor prompt files.
9. After drafting all four files, create `report.md`:
   - Summarize the created documents, their intent, and any assumptions.
   - Note that no code behavior in `src/` was changed (docs-only / planning changes).

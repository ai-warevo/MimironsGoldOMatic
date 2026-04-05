# Plan

1. Create target directories under `docs/`.
2. `git mv` each file per user mapping table; remove empty legacy `docs/MimironsGoldOMatic.*` folders.
3. Update links:
   - Repo root (`README.md`, `CONTEXT.md`, `AGENTS.md`): `docs/<section>/...` paths.
   - `docs/ReadME.md`: paths relative to `docs/` (`overview/...`, etc.).
   - Nested docs: relative paths (`../overview/`, `../reference/`, etc.).
   - `src/**` and Lua/toc: adjust `../../docs/...` depth where needed.
   - `docs/prompts/history/**`: update path strings for navigability (no deletion of logs).
4. Replace `docs/ReadME.md` intro with navigation section; preserve useful existing body or merge.
5. Verify: search for stale `docs/MimironsGoldOMatic.` and old flat `docs/overview/SPEC.md` link targets where invalid.

## Risks

- Missed relative links in a few markdown files; mitigate with repo-wide grep for old path segments.

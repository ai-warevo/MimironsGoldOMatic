# Report: docs directory restructure

## Summary

Reorganized product documentation under `docs/overview/`, `docs/e2e/`, `docs/components/{backend,desktop,twitch-extension,wow-addon,shared}/`, `docs/setup/`, and `docs/reference/`. Updated Markdown links (including relative targets from nested paths) and root hub files. Added a **Navigation** section to `docs/ReadME.md`.

## Modified / moved (high level)

- **Moved:** All files per user mapping (git rename).
- **Updated:** `README.md`, `CONTEXT.md`, `AGENTS.md`, `docs/ReadME.md`, and the majority of cross-linking docs under `docs/` and `src/MimironsGoldOMatic.TwitchExtension/README.md`; WoW Lua header comments; bulk path updates across `docs/prompts/history/**` backtick paths where the bulk replace touched them.

## Verification

- `dotnet build src/MimironsGoldOMatic.slnx` — succeeded (0 warnings, 0 errors).

## Notes

- Historical agent logs under `docs/prompts/history/` may still mention legacy `docs/MimironsGoldOMatic.*` paths in narrative text; functional product docs use the new layout.
- Normative **SPEC** lives only under `docs/overview/SPEC.md` (no duplicate under `reference/`).

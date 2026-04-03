# Plan

## Decision

- **MVP normative bridge:** Addon **prints machine-readable tags to WoW chat** so they appear in **`Logs\WoWChatLog.txt`**, matching the existing **`[MGM_CONFIRM:UUID]`** approach.
- Add **`[MGM_ACCEPT:UUID]`** when the addon has validated whisper **`!twgold`** (consent). Desktop **tails the same file** with a second regex and calls **`POST /api/payouts/{id}/confirm-acceptance`**.
- **Rationale:** 3.3.5a FrameXML Lua has no reliable **named-pipe/TCP/file IPC** to an external Desktop process; **chat print → log** is implementable and auditable. This is **not** “parsing the user’s whisper from the log” — the addon **emits** the tag after a **Lua whisper event**.

## Files

- `docs/SPEC.md` (§1 glossary, §5, §9, §10)
- `AGENTS.md`, `docs/MimironsGoldOMatic.Desktop/ReadME.md`, `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`
- `docs/INTERACTION_SCENARIOS.md`, `docs/IMPLEMENTATION_READINESS.md` as needed

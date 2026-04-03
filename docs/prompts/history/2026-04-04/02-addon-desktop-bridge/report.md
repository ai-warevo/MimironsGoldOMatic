# Report: Addon → Desktop bridge clarification

## Decision (normative MVP)

- **WoW Addon → Desktop** for whisper **`!twgold`** acceptance uses the **same channel** as mail confirmation: **addon prints a tag to WoW chat** → **`Logs\WoWChatLog.txt`** → Desktop tail → HTTP to Backend.
- New tag: **`[MGM_ACCEPT:UUID]`** (payout id), emitted **after** Lua validates the whisper. Existing tag **`[MGM_CONFIRM:UUID]`** unchanged for **`Sent`**.
- **Rationale:** WoW **3.3.5a** FrameXML Lua cannot rely on named pipes, TCP, or arbitrary file IPC to an external process; **chat print → log** is the implementable, testable pattern (and is **not** “parsing the user’s whisper from the log” — the addon **emits** the tag).

## Modified files

- `AGENTS.md`
- `README.md`
- `CONTEXT.md`
- `docs/SPEC.md`
- `docs/ReadME.md`
- `docs/ROADMAP.md`
- `docs/UI_SPEC.md`
- `docs/INTERACTION_SCENARIOS.md`
- `docs/IMPLEMENTATION_READINESS.md`
- `docs/MimironsGoldOMatic.Backend/ReadME.md`
- `docs/MimironsGoldOMatic.Desktop/ReadME.md`
- `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`

## Verification

- Documentation only; no automated tests.

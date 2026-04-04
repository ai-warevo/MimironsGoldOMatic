# Plan

## Architecture

- Single file `MimironsGoldOMatic.lua` (Interface 30300): event dispatcher table, queue with row state, WHO pending state, pending acceptance after winner whisper.
- No external JSON library: compact JSON for `[MGM_WHO]` built with `string.format` and minimal escaping.
- Globals: `NotifyWinnerWhisper`, `ReceiveGold`, `MGM_RunWhoForSpin(spinCycleId, characterName)` for Desktop `/run` (spin cycle id from EBS `currentSpinCycleId`).

## Risks

- Taint if touching protected mail UI from wrong context — Prepare Mail only when mailbox open, not in combat for minimap toggle.
- `/who` timing: single pending WHO; `WHO_LIST_UPDATE` emits one line.

## Files

- `src/MimironsGoldOMatic.WoWAddon/MimironsGoldOMatic.lua` (replace scaffold)
- `MimironsGoldOMatic.toc` (notes)
- `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`, `docs/IMPLEMENTATION_READINESS.md`, `docs/ROADMAP.md`

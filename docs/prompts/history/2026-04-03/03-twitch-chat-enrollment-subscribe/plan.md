# Plan: Subscribe + Twitch chat `!twgold`; pool removal after Sent

## Normative changes

1. **Eligibility:** **Subscriber** to the channel (not “follow-only”).
2. **Enrollment / re-entry:** **Broadcast Twitch chat** is monitored for **`!twgold <CharacterName>`** (`CharacterName` = server nickname for roulette). Rejects duplicate **character names** already in the active pool.
3. **Post-win acceptance:** **`!twgold`** (no argument) in **broadcast chat** after winner notification records **willingness to receive** gold (aligns with “confirm via chat command”); keep WoW **`[MGM_CONFIRM:UUID]`** for **`Sent`**.
4. **Pool rule:** **Non-winners** stay in the pool; **winners are removed** when the payout is **`Sent`**; they may **re-enter** with another **`!twgold <CharacterName>`** in chat.
5. **Extension:** Primarily **roulette + status**; enrollment is **chat-driven** (update TwitchExtension ReadME + UI_SPEC).

## Files

- Root: `README.md`, `CONTEXT.md`, `AGENTS.md`
- `docs/overview/SPEC.md` (glossary, §5, §9–11), `docs/ReadME.md`, `docs/overview/ROADMAP.md`, `docs/overview/INTERACTION_SCENARIOS.md`, `docs/reference/UI_SPEC.md`, `docs/reference/IMPLEMENTATION_READINESS.md`
- `docs/MimironsGoldOMatic.*.ReadME.md` as needed

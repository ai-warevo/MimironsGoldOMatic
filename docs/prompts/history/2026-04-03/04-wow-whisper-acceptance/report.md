# Report: WoW whisper winner notification + `!twgold` reply (consent)

## Summary

Aligned all product docs with the user request: the **addon** sends **`/whisper <Winner_InGame_Nickname> …`** (Russian text per `docs/SPEC.md` §9); **consent** is an **in-game private message** reply with exactly **`!twgold`**. Removed **Twitch chat `!twgold` (no args)** as the primary acceptance path.

## Files updated (this completion pass)

- `docs/SPEC.md` (glossary monitoring text)
- `docs/ReadME.md`, `docs/ROADMAP.md`, `docs/INTERACTION_SCENARIOS.md`, `docs/UI_SPEC.md`, `docs/IMPLEMENTATION_READINESS.md`
- `docs/MimironsGoldOMatic.{Backend,TwitchExtension,WoWAddon,Desktop}/ReadME.md`

## Prior session note

`docs/SPEC.md` §9–11 and glossary were already partially updated before the user reported a missing response; this pass finished cross-file consistency.

# Report: WoW whisper winner notification + `!twgold` reply (consent)

## Summary

Aligned all product docs with the user request: the **addon** sends **`/whisper <Winner_InGame_Nickname> …`** (Russian text per `docs/overview/SPEC.md` §9); **consent** is an **in-game private message** reply with exactly **`!twgold`**. Removed **Twitch chat `!twgold` (no args)** as the primary acceptance path.

## Files updated (this completion pass)

- `docs/overview/SPEC.md` (glossary monitoring text)
- `docs/ReadME.md`, `docs/overview/ROADMAP.md`, `docs/overview/INTERACTION_SCENARIOS.md`, `docs/reference/UI_SPEC.md`, `docs/reference/IMPLEMENTATION_READINESS.md`
- `docs/MimironsGoldOMatic.{Backend,TwitchExtension,WoWAddon,Desktop}/ReadME.md`

## Prior session note

`docs/overview/SPEC.md` §9–11 and glossary were already partially updated before the user reported a missing response; this pass finished cross-file consistency.

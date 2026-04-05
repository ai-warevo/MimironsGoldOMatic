# Plan: WoW whisper winner notification + `!twgold` reply for acceptance

## Changes

- **Normative acceptance:** in-game **private message** from winner with body exactly **`!twgold`** after the addon causes the **outgoing whisper** to `<Winner_InGame_Nickname>` with the exact Russian text (and `/whisper` command form) provided by the user.
- **De-emphasize / remove:** Twitch broadcast chat **`!twgold`** (no args) as primary acceptance; Backend chat ingestion for that case optional or omitted in MVP docs.
- **Update:** `docs/overview/SPEC.md` glossary, §3, §5, §9–11; root README/CONTEXT/AGENTS; component ReadMEs; INTERACTION_SCENARIOS SC-001; UI_SPEC; ROADMAP; IMPLEMENTATION_READINESS.

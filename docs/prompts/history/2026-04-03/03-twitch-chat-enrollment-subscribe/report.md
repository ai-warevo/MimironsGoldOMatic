# Report: Subscribe + Twitch chat `!twgold`; remove winner from pool on `Sent`

## Summary

Documentation now matches the clarified MVP: **subscribers** enroll and re-enter via **broadcast Twitch chat** **`!twgold <CharacterName>`** (unique **CharacterName** in the pool); **acceptance** after a win is **`!twgold`** in **chat** with **no** argument (WoW whisper **`!twgold`** remains an **optional** addon path). The Backend **monitors chat** (EventSub/bot — implementation detail). **Winners are removed from the participant pool when the payout is `Sent`**; they may join again with **`!twgold <CharacterName>`**.

## Files modified

- Root: `README.md`, `CONTEXT.md`, `AGENTS.md`
- `docs/SPEC.md`, `docs/ReadME.md`, `docs/ROADMAP.md`, `docs/INTERACTION_SCENARIOS.md`, `docs/UI_SPEC.md`, `docs/IMPLEMENTATION_READINESS.md`
- `docs/MimironsGoldOMatic.{Shared,Backend,TwitchExtension,Desktop,WoWAddon}/ReadME.md`

## Verification

- `docs/SPEC.md` glossary and §5 define chat commands, uniqueness, pool removal on **`Sent`**, and optional **`POST /api/payouts/claim`**.
- `docs/INTERACTION_SCENARIOS.md` SC-001 updated for chat ingest flow.

## Follow-ups (implementation)

- Implement EventSub (or IRC) chat ingestion, subscriber checks, and pool row deletion on **`Sent`** in code when projects exist.

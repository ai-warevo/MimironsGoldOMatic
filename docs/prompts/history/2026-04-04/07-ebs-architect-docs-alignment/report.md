# Report

## Summary

Architect decisions (inline Helix §11, **no** Outbox in MVP, **EventSub-only** enrollment subscriber signal, **`IsRewardSentAnnouncedToChat`**, roadmap mandatory checklist, SC-022 non-speculative APIs) were written into **`docs/SPEC.md`** and cross-cutting docs. **EBS** replaces generic “Backend” in prose while **`MimironsGoldOMatic.Backend`** remains the project name.

## Git

- **Commit:** `2b1eec2` — `docs(spec): align MVP docs with EBS, Helix §11, and roadmap checklist`

## Modified files (in commit)

- `AGENTS.md`
- `CONTEXT.md`
- `docs/IMPLEMENTATION_READINESS.md`
- `docs/INTERACTION_SCENARIOS.md`
- `docs/MimironsGoldOMatic.Backend/ReadME.md`
- `docs/MimironsGoldOMatic.Desktop/ReadME.md`
- `docs/MimironsGoldOMatic.Shared/ReadME.md`
- `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`
- `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`
- `docs/ROADMAP.md`
- `docs/ReadME.md`
- `docs/SPEC.md`

## Verification

- `dotnet build src/MimironsGoldOMatic.slnx` — **succeeded** (0 warnings / 0 errors) at time of session.

## Technical debt / follow-ups

- **`PayoutDto`** may omit **`IsRewardSentAnnouncedToChat`** until API exposes it; spec places it on read model first.
- **`CharacterName`** FluentValidation still uses simplified `char.IsLetter` vs full Latin/Cyrillic script rules (**§4**); noted in `IMPLEMENTATION_READINESS.md`.

## History

- This folder is a **retroactive** prompt log for the above commit (AGENTS.md workflow was not applied before commit).

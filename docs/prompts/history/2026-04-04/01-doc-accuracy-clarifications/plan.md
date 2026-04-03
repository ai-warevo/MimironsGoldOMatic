# Plan (doc accuracy — awaiting user answers)

## Goals

- Resolve open product/contract ambiguities identified in review (case rules, spin authority, IPC, API shapes, edge cases).
- Align `AGENTS.md` and component docs with canonical `docs/SPEC.md` (ES-first vs EF Core).
- Update `docs/INTERACTION_SCENARIOS.md` to close or re-label OPEN QUESTIONS per decisions.
- Optionally add a short normative subsection to `docs/SPEC.md` (or appendix) for decisions.

## Affected files (tentative)

- `AGENTS.md`
- `docs/SPEC.md`
- `docs/INTERACTION_SCENARIOS.md`
- `docs/IMPLEMENTATION_READINESS.md` (if residual risks change)
- `docs/MimironsGoldOMatic.Backend/ReadME.md`, `docs/MimironsGoldOMatic.Desktop/ReadME.md`, `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`
- `docs/ROADMAP.md` only if we add minimum API contract references

## Risks

- Over-specifying JSON before code exists — keep minimum fields only if user wants contracts locked now.

## Status

- [x] User answers questionnaire
- [x] Apply doc edits
- [x] `checks.md` + `report.md`

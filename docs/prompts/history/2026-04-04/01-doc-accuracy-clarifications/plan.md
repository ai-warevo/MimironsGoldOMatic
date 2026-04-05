# Plan (doc accuracy — awaiting user answers)

## Goals

- Resolve open product/contract ambiguities identified in review (case rules, spin authority, IPC, API shapes, edge cases).
- Align `AGENTS.md` and component docs with canonical `docs/overview/SPEC.md` (ES-first vs EF Core).
- Update `docs/overview/INTERACTION_SCENARIOS.md` to close or re-label OPEN QUESTIONS per decisions.
- Optionally add a short normative subsection to `docs/overview/SPEC.md` (or appendix) for decisions.

## Affected files (tentative)

- `AGENTS.md`
- `docs/overview/SPEC.md`
- `docs/overview/INTERACTION_SCENARIOS.md`
- `docs/reference/IMPLEMENTATION_READINESS.md` (if residual risks change)
- `docs/components/backend/ReadME.md`, `docs/components/desktop/ReadME.md`, `docs/components/wow-addon/ReadME.md`
- `docs/overview/ROADMAP.md` only if we add minimum API contract references

## Risks

- Over-specifying JSON before code exists — keep minimum fields only if user wants contracts locked now.

## Status

- [x] User answers questionnaire
- [x] Apply doc edits
- [x] `checks.md` + `report.md`

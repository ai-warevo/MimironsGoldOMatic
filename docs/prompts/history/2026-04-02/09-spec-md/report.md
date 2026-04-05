## Summary

Implemented documentation structure option A:

- Added `docs/overview/SPEC.md` as the canonical MVP technical specification (API/DTOs, idempotency, statuses/transitions, persistence notes, expiration job, payload format, chunking guidance, and chat log parsing).
- Updated `docs/overview/ROADMAP.md` to link each MVP phase to the relevant SPEC sections and to treat SPEC as the canonical contract.
- Updated `README.md` and `docs/ReadME.md` entrypoints to link to `docs/overview/SPEC.md`.
- Reduced duplication by removing the inline “MVP API Contract” block from `docs/ReadME.md` in favor of `docs/overview/SPEC.md`.

## Modified files

- `docs/overview/SPEC.md` (new)
- `docs/overview/ROADMAP.md`
- `docs/ReadME.md`
- `README.md`
- Added workflow artifacts:
  - `docs/prompts/history/2026-04-02/09-spec-md/*`

## Verification

- Manual link and consistency check: roadmap “Spec links” refer to headings present in `docs/overview/SPEC.md`.
- No code changes; test execution not applicable.

## Potential technical debt

- Some implementation details are still “by choice” (e.g., `GET /api/payouts/my-last` empty behavior; `POST /claim` exact status code). These are explicitly documented as MVP decisions to finalize during implementation.


## Summary

- Added an MVP-level API contract section to `docs/ReadME.md` (endpoints, auth expectations, statuses, transitions, error semantics).
- Saved the finalized project roadmap to `docs/overview/ROADMAP.md` and linked it from the root `README.md`.

## Modified files

- `docs/ReadME.md`
- `docs/overview/ROADMAP.md`
- `README.md`

## Verification

- Manual consistency check: endpoints and status names match the finalized MVP spec and the component READMEs.
- No code changes; test execution not applicable.

## Potential technical debt

- Header name / exact auth mechanism for Desktop `ApiKey` is intentionally left for implementation (doc notes it is “to be finalized”).
- Error response shapes (status codes + JSON body) are documented semantically; implementation should finalize a shared error DTO if needed.


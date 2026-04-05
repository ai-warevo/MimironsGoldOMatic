# Plan

1. Inventory Backend routes, DTOs, auth, and notable config (`Mgm:DevSkipSubscriberCheck`, EventSub, rate limits) from `src/`.
2. Align `docs/overview/SPEC.md` §5 with implemented APIs (including `POST /api/twitch/eventsub`, `PATCH` response body, `POST /api/payouts/claim` response + subscriber gate).
3. Refresh overview docs (`README`, `CONTEXT`, `docs/ReadME`, `IMPLEMENTATION_READINESS`, `ROADMAP`, `UI_SPEC`, `INTERACTION_SCENARIOS`, `SETUP*`, `AGENTS`, component ReadMEs) for implementation status and remove stale “scaffold-only” claims where MVP code exists.
4. Add `<!-- Updated: 2026-04-05 -->` to each touched file; use `<!-- MANUAL UPDATE REQUIRED -->` only where code cannot answer.

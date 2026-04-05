# Plan

1. Add `src/Mocks/MockHelixApi` and `src/Mocks/SyntheticDesktop` with documented health JSON, Helix POST + last-request, Desktop run-sequence + last-run.
2. Add `scripts/tier_b_verification/` Python scripts (`requests`, logging).
3. Extend `docs/e2e/E2E_AUTOMATION_PLAN.md` with readiness verification, first run guide, troubleshooting rows.
4. Update Backend ReadME Tier B setup; add `docs/e2e/TIER_B_PRELAUNCH_CHECKLIST.md`; refresh `TIER_B_IMPLEMENTATION_TASKS.md` statuses.
5. Register projects in `MimironsGoldOMatic.slnx`; align PATCH JSON with Backend camelCase string enums (Shared `PayoutStatus`).

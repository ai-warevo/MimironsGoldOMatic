# Report: Follow-based pool; remove Channel Points and instant spin (documentation)

## Summary

Updated project documentation so MVP **does not** depend on Twitch Channel Points or Partner status. **Pool eligibility** is **follow the broadcaster** + enroll (character name). **Roulette** runs on a **fixed 5-minute** cadence with **no** early/off-schedule spin. Enrollment idempotency is documented as **`EnrollmentRequestId`** (replacing **`TwitchTransactionId`** / redemption-centric wording).

## Modified files

- Root: `README.md`, `CONTEXT.md`, `AGENTS.md`
- `docs/ReadME.md`, `docs/SPEC.md`, `docs/ROADMAP.md`, `docs/INTERACTION_SCENARIOS.md`, `docs/UI_SPEC.md`, `docs/IMPLEMENTATION_READINESS.md`
- `docs/MimironsGoldOMatic.Shared/ReadME.md`, `docs/MimironsGoldOMatic.Backend/ReadME.md`, `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`

## Prompt history

- `docs/prompts/history/2026-04-03/02-follow-pool-no-channel-points/` (`prompt.md`, `plan.md`, `checks.md`, `report.md`)

## Verification

- Grep on `docs/*.md` (excluding `docs/prompts/` history archives): no remaining normative **`TwitchTransactionId`**, **“Switch to instant spin”**, or Channel Points **reward** flows in product docs.
- No code or `src/` README changes in this task (docs-only); shared DTO names in code will need a follow-up implementation pass to match **`EnrollmentRequestId`**.

## Technical debt / follow-ups

- Implement follow verification + `EnrollmentRequestId` in Backend, Shared, and Twitch Extension when coding starts.
- Prior `docs/prompts/history/**` entries still mention old Channel Points wording by design (historical log; not rewritten).

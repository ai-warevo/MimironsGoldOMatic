# Plan: Follow-based pool; remove Channel Points and instant spin (docs)

## Goal

Align root `*.md` and `docs/**/*.md` (excluding `docs/prompts/` except new history) with:

- **Eligibility:** follow the broadcaster to join the giveaway participant pool (no Channel Points / partner requirement).
- **Payout:** fixed **1,000g** per winning spin outcome (unchanged).
- **Roulette:** visual roulette on a fixed **5-minute** cadence only; remove **early / instant spin** and all Channel Points reward references.
- **Idempotency:** replace **Twitch redemption**-centric `TwitchTransactionId` with **`EnrollmentRequestId`** (client-generated UUID for enroll retries) in normative docs.

## Affected files (expected)

- Root: `README.md`, `CONTEXT.md`, `AGENTS.md`
- `docs/ReadME.md`, `docs/overview/SPEC.md`, `docs/overview/ROADMAP.md`, `docs/overview/INTERACTION_SCENARIOS.md`, `docs/reference/UI_SPEC.md`, `docs/reference/IMPLEMENTATION_READINESS.md`
- `docs/MimironsGoldOMatic.*/ReadME.md` (Shared, Backend, TwitchExtension; Desktop/WoW if references appear)

## Risks

- Implementation code and `src/` READMEs are **out of scope** for this task; contracts in docs will drift until code matches.

## Verification

- Grep for removed concepts: `Channel Points`, `Switch to instant`, `instant spin`, `TwitchTransactionId`, `redemption` (where inappropriate).
- Read-through of `docs/overview/SPEC.md` glossary and §5 for internal consistency.

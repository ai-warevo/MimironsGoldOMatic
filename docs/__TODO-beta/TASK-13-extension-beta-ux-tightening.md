# Task 13 - Extension Beta UX Tightening

Acting as **[Frontend/Twitch Expert]**.

## Goal
Polish Beta viewer UX states/copy for countdown, winner instructions, and transient API recovery.

## Read first
- `docs/components/twitch-extension/UI_SPEC.md`
- `docs/reference/UI_SPEC.md`
- `docs/overview/SPEC.md`
- `docs/beta/BETA_GO_NO_GO.md`

## Implement
1. Refine countdown state copy and fallback behavior for late/partial API responses.
2. Clarify winner instruction wording for in-game whisper `!twgold` consent step.
3. Add resilient transient failure states with recovery messaging and retry hints.
4. Keep copy and state transitions aligned with backend contracts.

## Acceptance criteria
- Viewer-facing states are unambiguous and action-oriented.
- Transient API failures show recoverable UX (no dead-end or blank states).
- UI copy matches approved Beta behavior and terminology.

## Output
- Extension UI/code updates.
- Updated UX notes in component docs if behavior/copy matrix changed.
- Evidence links/screenshots added to `docs/beta/BETA_GO_NO_GO.md`.

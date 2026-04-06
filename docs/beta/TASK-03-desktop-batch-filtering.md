# Task 03 - Desktop Batch Actions and Filtering

Acting as **[WPF/WinAPI Expert]**.

## Goal
Add Beta productivity features for queue management: filtering, sorting, and batch actions.

## Read first
- `docs/overview/ROADMAP.md` (Beta section)
- `docs/components/desktop/UI_SPEC.md`
- `docs/components/desktop/ReadME.md`

## Implement
1. Add filters for status and text search (character/payout id).
2. Add sort by created time and status.
3. Add batch actions for selected rows where safe:
   - mark sent
   - fail
   - cancel
   - move `InProgress` to `Pending` where allowed
4. Include confirmation prompts and result summary.

## Acceptance criteria
- Operator can manage multiple payouts faster.
- Batch actions respect lifecycle rules and partial failure handling.
- UI remains responsive for larger lists.

## Output
- Desktop view + viewmodel changes.
- Minimal docs update with usage notes.
- Verification notes with example scenarios.

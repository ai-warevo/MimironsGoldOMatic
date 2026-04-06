# Task 02 - InProgress Reconciliation

Acting as **[WPF/WinAPI Expert]** and **[Backend/API Expert]**.

## Goal
Improve Beta reliability for payouts stuck in `InProgress`.

## Read first
- `docs/overview/SPEC.md` (§status lifecycle)
- `docs/overview/ROADMAP.md` (Beta section)
- `docs/components/desktop/ReadME.md`
- `docs/components/backend/ReadME.md`

## Implement
1. Add Desktop reconciliation flow for old `InProgress` payouts:
   - detect stale age threshold
   - propose safe actions (`Pending`, `Failed`, `Cancelled`, or retry injection)
2. Add clear UI guidance and one-click action per stale row.
3. Ensure backend transitions remain valid and audited.
4. Make actions idempotent and safe under retries/restarts.

## Acceptance criteria
- Stale `InProgress` payouts are surfaced clearly.
- Operator can recover without manual DB edits.
- No invalid status transitions are introduced.

## Output
- Desktop UX + logic updates.
- Backend adjustments (if needed) for safe reconciliation transitions.
- Updated docs and test/verification notes.

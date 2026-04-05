## Goal

Document a concrete MVP API contract and persist the finalized roadmap under `docs/`.

## Scope / files

- Update: `docs/ReadME.md` (add "MVP API Contract")
- Add: `docs/overview/ROADMAP.md`
- Optional: Update `README.md` to link the roadmap (only if it improves discoverability)

## Notes

- Keep the contract consistent with the finalized MVP spec: `TwitchTransactionId` idempotency, `ApiKey` for Desktop, `GET /api/payouts/my-last` pull model, statuses including `Cancelled`/`Expired`, and explicit claim flow.


## Report

### Modified files

- `docs/overview/SPEC.md`
- `docs/ReadME.md`
- `README.md`
- `docs/components/backend/ReadME.md`
- `docs/components/desktop/ReadME.md`
- `docs/components/shared/ReadME.md`
- `docs/components/twitch-extension/ReadME.md`
- `docs/components/wow-addon/ReadME.md`
- `docs/overview/ROADMAP.md`
- `docs/reference/IMPLEMENTATION_READINESS.md`

### What was aligned

- MVP persistence model fixed to **ES-first**:
  - Marten/Event Store is canonical write-side source of truth.
  - EF Core is constrained to query/read-model usage.
- MVP architecture emphasis kept strict: **DDD + CQRS + ES**.
- API semantics fixed:
  - `POST /api/payouts/claim`: `201` for new creation, `200` for idempotent replay.
  - `GET /api/payouts/my-last`: `404` when no payout exists.
- Confirmation semantics fixed:
  - `Sent` corresponds to actual send confirmation (`send_confirm`).
- Desktop injection strategy fixed:
  - `PostMessage` primary + `SendInput` fallback.
- Payload/confirm examples normalized to canonical forms:
  - payload entry `UUID:CharacterName:GoldCopper;`
  - confirm tag `[MGM_CONFIRM:UUID]`

### Verification

- Performed documentation consistency pass against selected decisions.
- Added a readiness matrix mapping each approved decision to explicit documentation locations.
- No code compilation/tests were run (docs-only change set).

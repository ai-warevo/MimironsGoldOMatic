## Report

### Modified files

- `docs/SPEC.md`
- `docs/ReadME.md`
- `README.md`
- `docs/MimironsGoldOMatic.Backend/ReadME.md`
- `docs/MimironsGoldOMatic.Desktop/ReadME.md`
- `docs/MimironsGoldOMatic.Shared/ReadME.md`
- `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`
- `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`
- `docs/ROADMAP.md`
- `docs/IMPLEMENTATION_READINESS.md`

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

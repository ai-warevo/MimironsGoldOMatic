## Goal

Add a single canonical technical specification file (`docs/overview/SPEC.md`) to eliminate ambiguity for agents implementing MVP tasks, while keeping the roadmap concise.

## Scope

- Add: `docs/overview/SPEC.md`
- Update: `docs/overview/ROADMAP.md` (add links to SPEC sections)
- Update: `docs/ReadME.md` (link to SPEC)
- Update: `README.md` (link to SPEC)

## Decisions to encode (MVP defaults)

- Desktop `ApiKey` header name: `X-MGM-ApiKey`
- Claim idempotency: duplicate `TwitchTransactionId` returns the existing payout (idempotent success), not a second record.
- Addon payload format: chunked list of *complete* payout entries; avoid splitting mid-entry.

## Verification

- Ensure links resolve within repo.
- Ensure roadmap references the SPEC sections relevant to each MVP step.


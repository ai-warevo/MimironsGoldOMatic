# MVP Implementation Readiness Matrix

This matrix is a final consistency sweep for the approved MVP decisions.
Canonical normative source remains `docs/SPEC.md`.

| Decision | Required state | Fixed in docs | Status |
|---|---|---|---|
| Architecture baseline | DDD + CQRS + ES are mandatory in MVP | `README.md`, `docs/ReadME.md`, `docs/SPEC.md`, `docs/ROADMAP.md` | Ready |
| MVP write-side source of truth | ES-first with Marten/PostgreSQL | `docs/SPEC.md`, `docs/ReadME.md`, `docs/MimironsGoldOMatic.Backend/ReadME.md`, `docs/ROADMAP.md` | Ready |
| EF Core role | Read-model/query side only | `docs/SPEC.md`, `docs/ReadME.md`, `docs/MimironsGoldOMatic.Backend/ReadME.md`, `README.md`, `docs/ROADMAP.md` | Ready |
| Claim endpoint success semantics | `POST /api/payouts/claim`: `201` new, `200` idempotent replay | `docs/SPEC.md`, `docs/MimironsGoldOMatic.Backend/ReadME.md`, `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`, `docs/MimironsGoldOMatic.Shared/ReadME.md`, `docs/ROADMAP.md` | Ready |
| Empty viewer history semantics | `GET /api/payouts/my-last`: `404 Not Found` when no payout exists | `docs/SPEC.md`, `docs/MimironsGoldOMatic.Backend/ReadME.md`, `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`, `docs/MimironsGoldOMatic.Shared/ReadME.md`, `docs/ROADMAP.md` | Ready |
| Confirmation semantics | `Sent` means actual send confirmation (`send_confirm`) | `docs/SPEC.md`, `README.md`, `docs/MimironsGoldOMatic.Desktop/ReadME.md`, `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`, `docs/ROADMAP.md` | Ready |
| Desktop injection strategy | Primary `PostMessage`, fallback `SendInput` | `docs/SPEC.md`, `docs/MimironsGoldOMatic.Desktop/ReadME.md`, `docs/ROADMAP.md` | Ready |
| Addon payload format | `UUID:CharacterName:GoldCopper;` | `docs/SPEC.md`, `docs/MimironsGoldOMatic.Desktop/ReadME.md` | Ready |
| Confirmation tag format | `[MGM_CONFIRM:UUID]` | `docs/SPEC.md`, `README.md`, `docs/MimironsGoldOMatic.Desktop/ReadME.md`, `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`, `docs/ROADMAP.md` | Ready |
| Frontend state stack | Zustand is required in MVP | `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md` | Ready |

## Residual implementation risks (not contradictions)

- Spec is now consistent for behavior and contracts, but implementation still needs concrete code-level decisions:
  - exact Marten stream design (stream-per-payout vs stream-per-user aggregate);
  - projection update strategy and replay/rebuild procedure;
  - concurrency control details in command handlers;
  - operational details for WinAPI timing/retry behavior in real WoW 3.3.5a sessions.

## Go/No-Go

- **Go**: documentation is consistent enough to start implementation on the approved MVP track.

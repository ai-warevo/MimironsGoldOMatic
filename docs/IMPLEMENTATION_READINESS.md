# MVP Implementation Readiness Matrix

This matrix is a final consistency sweep for the approved MVP decisions.
Canonical normative source remains `docs/SPEC.md`.
For **user-visible** behavior and layout, cross-check `docs/UI_SPEC.md` (UI-1xx–4xx) alongside component READMEs.

| Decision | Required state | Fixed in docs | Status |
|---|---|---|---|
| Architecture baseline | DDD + CQRS + ES are mandatory in MVP | `README.md`, `docs/ReadME.md`, `docs/SPEC.md`, `docs/ROADMAP.md` | Ready |
| MVP write-side source of truth | ES-first with Marten/PostgreSQL | `docs/SPEC.md`, `docs/ReadME.md`, `docs/MimironsGoldOMatic.Backend/ReadME.md`, `docs/ROADMAP.md` | Ready |
| EF Core role | Read-model/query side only | `docs/SPEC.md`, `docs/ReadME.md`, `docs/MimironsGoldOMatic.Backend/ReadME.md`, `README.md`, `docs/ROADMAP.md` | Ready |
| Claim endpoint success semantics | `POST /api/payouts/claim`: `201` new, `200` idempotent replay (pool **enrollment**) | `docs/SPEC.md`, `docs/MimironsGoldOMatic.Backend/ReadME.md`, `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`, `docs/MimironsGoldOMatic.Shared/ReadME.md`, `docs/ROADMAP.md` | Ready |
| Empty viewer history semantics | `GET /api/payouts/my-last`: `404 Not Found` when no payout exists | `docs/SPEC.md`, `docs/MimironsGoldOMatic.Backend/ReadME.md`, `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`, `docs/MimironsGoldOMatic.Shared/ReadME.md`, `docs/ROADMAP.md` | Ready |
| Acceptance semantics | **`[MGM_ACCEPT:UUID]`** in **`WoWChatLog.txt`** after Lua whisper **`!twgold`** → Desktop **`confirm-acceptance`**; **not** **`Sent`** | `docs/SPEC.md` §9–10, `README.md`, `docs/MimironsGoldOMatic.Desktop/ReadME.md`, `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`, `docs/ROADMAP.md` | Ready |
| Confirmation semantics (`Sent`) | **`Sent`** requires **`[MGM_CONFIRM:UUID]`** in **`WoWChatLog.txt`** (Desktop log watcher → Backend) | `docs/SPEC.md`, `README.md`, `docs/MimironsGoldOMatic.Desktop/ReadME.md`, `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`, `docs/ROADMAP.md` | Ready |
| Desktop injection strategy | Primary `PostMessage`, fallback `SendInput` | `docs/SPEC.md`, `docs/MimironsGoldOMatic.Desktop/ReadME.md`, `docs/ROADMAP.md` | Ready |
| Addon payload format | `UUID:CharacterName:GoldCopper;` | `docs/SPEC.md`, `docs/MimironsGoldOMatic.Desktop/ReadME.md` | Ready |
| Mail-send tag format | **`[MGM_CONFIRM:UUID]`** — **required** in **`WoWChatLog.txt`** for automated **`Sent`** | `docs/SPEC.md`, `README.md`, `docs/MimironsGoldOMatic.Desktop/ReadME.md`, `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`, `docs/ROADMAP.md` | Ready |
| Acceptance tag format | **`[MGM_ACCEPT:UUID]`** — **required** in **`WoWChatLog.txt`** for automated **`confirm-acceptance`** | `docs/SPEC.md` §9–10, `docs/MimironsGoldOMatic.Desktop/ReadME.md`, `docs/MimironsGoldOMatic.WoWAddon/ReadME.md` | Ready |
| Roulette behavior | Visual roulette; **5-minute** spin only (**no** early spin); **min 1** participant; **non-winners stay**; **winners removed on `Sent`**; **`/who`** before finalize; **Twitch chat** enroll **`!twgold <CharacterName>`**; **WoW** winner whisper + reply **`!twgold`**; **subscriber**-gated | `docs/SPEC.md`, `README.md`, `docs/ROADMAP.md`, `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`, `docs/MimironsGoldOMatic.Desktop/ReadME.md`, `docs/MimironsGoldOMatic.WoWAddon/ReadME.md` | Ready |
| Chat prefix & whisper consent | Enrollment **`!twgold`** prefix **case-insensitive**; WoW whisper **`!twgold`** consent **case-insensitive** (after trim) | `docs/SPEC.md` §1, §5, §9–11; `README.md`; `AGENTS.md` | Ready |
| Spin schedule & Extension timer | **`nextSpinAt` / `serverNow`** server-authoritative; Extension **must** show countdown from API | `docs/SPEC.md` §5.1, §11; `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md` | Ready |
| Minimum pool/roulette HTTP | **`GET /api/roulette/state`**, **`GET /api/pool/me`** (normative fields in `docs/SPEC.md` §5.1) | `docs/SPEC.md`, `docs/MimironsGoldOMatic.Backend/ReadME.md` | Ready |
| Desktop `InProgress` gate | **`Pending` → `InProgress`** only after WoW target detected | `docs/SPEC.md` §3; `docs/MimironsGoldOMatic.Desktop/ReadME.md`; `docs/INTERACTION_SCENARIOS.md` SC-011 | Ready |
| Outbox pattern | **Required** when external side effects exist (same tx as domain events) | `docs/SPEC.md` §6; `docs/MimironsGoldOMatic.Backend/ReadME.md` | Ready |
| Frontend state stack | Zustand is required in MVP | `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md` | Ready |

## Residual implementation risks (not contradictions)

- Locked in **`docs/SPEC.md`**: **EventSub** chat; **single broadcaster** MVP; **JWT-only** Extension reads for **`/api/roulette/state`** + **`/api/pool/me`**; **`GET /api/payouts/my-last`** **`404`** when no payout; **`spinPhase`** enum; **UTC :00/:05** spin grid; **no re-draw** offline same cycle; pool **replace** on re-enroll; **`POST /api/roulette/verify-candidate`** + **file-bridge**; **Marten** stream **per payout id** + **separate Pool/Payout** aggregates; **Outbox** deferred until first external integration; **`InProgress`→`Pending`** allowed; **`CharacterName`** **§4**; WoW log **default + override**; unknown **`MGM_ACCEPT`** → log/ignore.
- Remaining engineering detail (expected):
  - projection update strategy and replay/rebuild procedure;
  - concurrency control in command handlers;
  - WinAPI timing/retry on real **3.3.5a** clients;
  - validate addon **file-bridge** write path + **`WoWChatLog.txt`** print path on target clients;
  - **idempotent** **`confirm-acceptance`** / **`verify-candidate`** under log replay.

## Go/No-Go

- **Go**: documentation is consistent enough to start implementation on the approved MVP track.

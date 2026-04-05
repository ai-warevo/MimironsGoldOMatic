# MVP Implementation Readiness Matrix

## What this file measures

1. **Documentation / spec alignment** — The table below checks that MVP decisions are **reflected consistently** across normative docs (`docs/SPEC.md`, READMEs, roadmap). A **Ready** row means the **written contracts** agree; it does **not** mean the feature is **implemented in code**.
2. **Source tree parity** — See [Source code parity (MVP track)](#source-code-parity-mvp-track) for **what exists today** under `src/` versus MVP-0…MVP-6 in `docs/ROADMAP.md`.

Canonical normative source remains `docs/SPEC.md`. For **user-visible** behavior and layout, cross-check `docs/UI_SPEC.md` (UI-1xx–4xx) alongside component READMEs.

| Decision | Required state | Fixed in docs | Doc / spec alignment |
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
| Mail-send tag format | **`[MGM_CONFIRM:UUID]`** on **`MAIL_SEND_SUCCESS`** (MGM-armed send only) + winner whisper **`Награда отправлена тебе на почту, проверяй ящик!`** | `docs/SPEC.md`, `README.md`, `docs/MimironsGoldOMatic.Desktop/ReadME.md`, `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`, `docs/ROADMAP.md` | Ready |
| Twitch chat reward-sent line | **`Награда отправлена персонажу <WINNER_NAME> на почту, проверяй ящик!`** (Extension hardcodes; Helix/EBS per §11) | `docs/SPEC.md` §11, `docs/UI_SPEC.md` UI-104, `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md` | Ready |
| Acceptance tag format | **`[MGM_ACCEPT:UUID]`** — **required** in **`WoWChatLog.txt`** for automated **`confirm-acceptance`** | `docs/SPEC.md` §9–10, `docs/MimironsGoldOMatic.Desktop/ReadME.md`, `docs/MimironsGoldOMatic.WoWAddon/ReadME.md` | Ready |
| Roulette behavior | Visual roulette; **5-minute** spin only (**no** early spin); **min 1** participant; **non-winners stay**; **winners removed on `Sent`**; **`/who`** before finalize; **Twitch chat** enroll **`!twgold <CharacterName>`**; **WoW** winner whisper + reply **`!twgold`**; **subscriber**-gated | `docs/SPEC.md`, `README.md`, `docs/ROADMAP.md`, `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`, `docs/MimironsGoldOMatic.Desktop/ReadME.md`, `docs/MimironsGoldOMatic.WoWAddon/ReadME.md` | Ready |
| Chat prefix & whisper consent | Enrollment **`!twgold`** prefix **case-insensitive**; WoW whisper **`!twgold`** consent **case-insensitive** (after trim) | `docs/SPEC.md` §1, §5, §9–11; `README.md`; `AGENTS.md` | Ready |
| Spin schedule & Extension timer | **`nextSpinAt` / `serverNow`** server-authoritative; Extension **must** show countdown from API | `docs/SPEC.md` §5.1, §11; `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md` | Ready |
| Minimum pool/roulette HTTP | **`GET /api/roulette/state`**, **`GET /api/pool/me`** (normative fields in `docs/SPEC.md` §5.1) | `docs/SPEC.md`, `docs/MimironsGoldOMatic.Backend/ReadME.md` | Ready |
| Desktop `InProgress` gate | **`Pending` → `InProgress`** only after WoW target detected | `docs/SPEC.md` §3; `docs/MimironsGoldOMatic.Desktop/ReadME.md`; `docs/INTERACTION_SCENARIOS.md` SC-011 | Ready |
| Outbox pattern | **Required** when external side effects exist (same tx as domain events) | `docs/SPEC.md` §6; `docs/MimironsGoldOMatic.Backend/ReadME.md` | Ready |
| Frontend state stack | Zustand is required in MVP | `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md` | Ready |
| `/who` → Backend path | Addon **`[MGM_WHO]`** + JSON in **`WoWChatLog.txt`** → Desktop **`verify-candidate`** (no disk file-bridge) | `docs/SPEC.md` §5, §8, §10 | Ready |
| Extension poll resilience | Backoff + Retry on **`429`/`503`/network** (`docs/SPEC.md` §5.1) | `docs/SPEC.md`, `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md` | Ready |
| Spin candidate pick | **Uniform random** among active pool rows | `docs/SPEC.md` glossary, §5 | Ready |
| `verify-candidate` grace | **30s** after **UTC** spin boundary that **closes** the cycle’s verification window | `docs/SPEC.md` §5 | Ready |
| Winner whisper trigger | Desktop **`/run NotifyWinnerWhisper(id,name)`** → addon sends §9 **`/whisper`** | `docs/SPEC.md` §8–9 | Ready |
| MediatR placement | Handlers in **Backend** only; **Shared** = contracts/validation | `docs/MimironsGoldOMatic.Shared/ReadME.md` | Ready |
| MVP-5 Extension scope | **Viewer** UI only; **no** UI-201–204 | `docs/ROADMAP.md`, `docs/UI_SPEC.md` | Ready |
| Shared DTO field names | `PayoutDto` / `CreatePayoutRequest` use **`EnrollmentRequestId`** (idempotency for Extension claim path) per `docs/SPEC.md` §4 | `docs/SPEC.md`, `docs/MimironsGoldOMatic.Shared/ReadME.md`, `src/MimironsGoldOMatic.Shared` | Ready |
| Shared `CharacterName` validation | **2–12** characters (after trim), **Latin/Cyrillic script letters only** in Shared (`CharacterNameRules` + FluentValidation) | `docs/SPEC.md` §4, `docs/MimironsGoldOMatic.Shared/ReadME.md` | Ready |

## Source code parity (MVP track)

Snapshot of `src/` versus `docs/ROADMAP.md` (MVP-0 … MVP-6). Update this section when major scaffolding lands.

| MVP step | Roadmap intent | Current `src/` state |
|---|---|---|
| MVP-0 | `src/MimironsGoldOMatic.slnx` + project skeletons | **Complete (skeleton):** `slnx` lists the three .NET projects; `src/MimironsGoldOMatic.WoWAddon` and `src/MimironsGoldOMatic.TwitchExtension` exist as non-MSBuild trees (per `docs/ROADMAP.md` MVP-0 — not part of the .NET solution file). |
| MVP-1 | Shared contracts + validation | **Complete:** `PayoutStatus`, `PayoutDto` (incl. `IsRewardSentAnnouncedToChat`), `CreatePayoutRequest`, `CharacterNameRules`, FluentValidation validators (see `docs/MimironsGoldOMatic.Shared/ReadME.md`). |
| MVP-2 | Backend API + Marten/PostgreSQL | **Implemented:** Marten event store + read docs, MVP routes (`claim`, pool/roulette, payouts pending/status/confirm, verify-candidate), EventSub webhook, JWT + ApiKey auth, global rate limit (5/min per user/IP; EventSub exempt), Helix announcement + expiration job. Requires local Postgres + `appsettings` (`Mgm`, `Twitch`, connection string). |
| MVP-3 | WoW addon | **Implemented (MVP):** globals **`NotifyWinnerWhisper`**, **`ReceiveGold`**, **`MGM_RunWhoForSpin`**; **`[MGM_WHO]`** JSON, **`[MGM_ACCEPT]`** / **`[MGM_CONFIRM]`**; **`MAIL_SHOW`** queue panel + **Prepare Mail** (`SendMail*` + **`MoneyInputFrame_SetCopper`**); minimap + **`/mgm`**. **Optional / later:** UI-405 debug frame, scroll list polish, `MimironsGoldOMatic` singleton table refactor. |
| MVP-4 | WPF Desktop + WinAPI | **Implemented (MVP):** pending poll, **`NotifyWinnerWhisper`** + **Sync/Inject** (`ReceiveGold` chunking), **`WoWChatLog.txt`** tail (`[MGM_WHO]` / `[MGM_ACCEPT]` / `[MGM_CONFIRM]`), EBS **`PATCH`** overrides + **`InProgress`→`Pending`**, PostMessage→SendInput fallback, settings + DPAPI ApiKey. Manual verification on **WoW 3.3.5a** recommended. |
| MVP-5 | Twitch Extension UI | **Implemented (MVP):** viewer panel (Twitch `onAuthorized`, EBS polling for roulette/pool/`my-last`), server-skew countdown, `spinPhase` UX per `docs/UI_SPEC.md` UI-101–106, Zustand + backoff on 429/503/network. Configure **`VITE_MGM_EBS_BASE_URL`** (see `src/MimironsGoldOMatic.TwitchExtension/.env.example`); use Dev Rig for real JWTs. |
| MVP-6 | E2E demo + tests | **Not started** (depends on MVP-2…5 + tests). |

## Residual implementation risks (not contradictions)

- Locked in **`docs/SPEC.md`**: **EventSub** chat (subscriber flag from payload only for enroll); **single broadcaster** MVP; **real Twitch JWT** validation (Dev Rig + deploy); **JWT-only** Extension reads for **`/api/roulette/state`** + **`/api/pool/me`**; **`GET /api/payouts/my-last`** **`404`** when no payout; **`spinPhase`** enum (transitions **EBS**-defined); **UTC :00/:05** spin grid; **no re-draw** offline same cycle; spin **candidate** = **uniform random** among pool rows; **`verify-candidate`** **30s** grace after **UTC boundary closing** that cycle’s verification window; pool **replace** on re-enroll; **`POST /api/roulette/verify-candidate`** + **`[MGM_WHO]`** in **`WoWChatLog.txt`** (no file-bridge); **Desktop** **`/run NotifyWinnerWhisper`** → addon §9 whisper; **503** + Extension backoff (**§5**); **MediatR** handlers **EBS** only; **MVP-5** viewer Extension only; **Marten** stream **per payout id** + **separate Pool/Payout** aggregates; **§11 Helix** inline **3×** retry (**no** Outbox in MVP); **`InProgress`→`Pending`** allowed; **`CharacterName`** **§4**; WoW log **default + override**; unknown **`MGM_ACCEPT`** → log/ignore; Extension **429/503** backoff + Retry (**§5.1**); **`active_payout_exists`** — no auto-expire to force a new win.
- Remaining engineering detail (expected):
  - projection update strategy and replay/rebuild procedure;
  - concurrency control in command handlers;
  - WinAPI timing/retry on real **3.3.5a** clients;
  - validate addon **`[MGM_WHO]`** / **`[MGM_ACCEPT]`** / **`[MGM_CONFIRM]`** visibility in **`WoWChatLog.txt`** on target clients;
  - **idempotent** **`confirm-acceptance`** / **`verify-candidate`** under log replay;
  - optional: extend **`CharacterName`** script ranges if product adds non–Latin/Cyrillic realm naming rules.

## Go/No-Go

- **Go (documentation):** MVP contracts are consistent across normative docs; agents can implement against `docs/SPEC.md` + `docs/ROADMAP.md`.
- **Go (product demo):** **No** — an end-to-end demo still needs Postgres/Twitch configuration and **running** Backend, Desktop, Extension (Dev Rig or hosted), and WoW addon against the same live API.

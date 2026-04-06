<!-- Updated: 2026-04-06 (Project structure alignment + Tier B finalization) -->

# MVP Implementation Readiness Matrix

## What this file measures

1. **Documentation / spec alignment** — The table below checks that MVP decisions are **reflected consistently** across normative docs (`docs/overview/SPEC.md`, READMEs, roadmap). A **Ready** row means the **written contracts** agree; it does **not** mean the feature is **implemented in code**.
2. **Source tree parity** — See [Source code parity (MVP track)](#source-code-parity-mvp-track) for **what exists today** under `src/` versus MVP-0…MVP-6 in `docs/overview/ROADMAP.md`.
3. **MVP-6 verification** — See [MVP-6 verification status](#mvp-6-verification-status) for **Automated** vs **Manual** coverage; full **E2E** (chat → WoW → Helix) detail lives in **[Automated E2E Scenarios (MVP-6)](../overview/INTERACTION_SCENARIOS.md#automated-e2e-scenarios-mvp-6)** (`docs/overview/INTERACTION_SCENARIOS.md`).

Canonical normative source remains **`docs/overview/SPEC.md`**. For **user-visible** behavior and layout, cross-check **`docs/reference/UI_SPEC.md`** (hub: tokens, navigation) and **`docs/components/*/UI_SPEC.md`** (UI-1xx–4xx screens) alongside component READMEs. **Conceptual overviews** (architecture, MVP digest, workflows): [`docs/overview/ARCHITECTURE.md`](../overview/ARCHITECTURE.md), [`docs/overview/MVP_PRODUCT_SUMMARY.md`](../overview/MVP_PRODUCT_SUMMARY.md), [`docs/reference/WORKFLOWS.md`](WORKFLOWS.md).

| Decision | Required state | Fixed in docs | Doc / spec alignment |
|---|---|---|---|
| Architecture baseline | DDD + CQRS + ES are mandatory in MVP | `docs/overview/ARCHITECTURE.md`, `docs/overview/SPEC.md`, `docs/overview/ROADMAP.md`, `README.md`, `docs/ReadME.md` | Ready |
| MVP write-side source of truth | ES-first with Marten/PostgreSQL | `docs/overview/SPEC.md`, `docs/ReadME.md`, `docs/components/backend/ReadME.md`, `docs/overview/ROADMAP.md` | Ready |
| EF Core role | Read-model/query side only | `docs/overview/SPEC.md`, `docs/ReadME.md`, `docs/components/backend/ReadME.md`, `README.md`, `docs/overview/ROADMAP.md` | Ready |
| Claim endpoint success semantics | `POST /api/payouts/claim`: `201` new, `200` idempotent replay (pool **enrollment**) | `docs/overview/SPEC.md`, `docs/components/backend/ReadME.md`, `docs/components/twitch-extension/ReadME.md`, `docs/components/shared/ReadME.md`, `docs/overview/ROADMAP.md` | Ready |
| Empty viewer history semantics | `GET /api/payouts/my-last`: `404 Not Found` when no payout exists | `docs/overview/SPEC.md`, `docs/components/backend/ReadME.md`, `docs/components/twitch-extension/ReadME.md`, `docs/components/shared/ReadME.md`, `docs/overview/ROADMAP.md` | Ready |
| Acceptance semantics | **`[MGM_ACCEPT:UUID]`** in **`WoWChatLog.txt`** after Lua whisper **`!twgold`** → Desktop **`confirm-acceptance`**; **not** **`Sent`** | `docs/overview/SPEC.md` §9–10, `README.md`, `docs/components/desktop/ReadME.md`, `docs/components/wow-addon/ReadME.md`, `docs/overview/ROADMAP.md` | Ready |
| Confirmation semantics (`Sent`) | **`Sent`** requires **`[MGM_CONFIRM:UUID]`** in **`WoWChatLog.txt`** (Desktop log watcher → Backend) | `docs/overview/SPEC.md`, `README.md`, `docs/components/desktop/ReadME.md`, `docs/components/wow-addon/ReadME.md`, `docs/overview/ROADMAP.md` | Ready |
| Desktop injection strategy | Primary `PostMessage`, fallback `SendInput` | `docs/overview/SPEC.md`, `docs/components/desktop/ReadME.md`, `docs/overview/ROADMAP.md` | Ready |
| Addon payload format | `UUID:CharacterName:GoldCopper;` | `docs/overview/SPEC.md`, `docs/components/desktop/ReadME.md` | Ready |
| Mail-send tag format | **`[MGM_CONFIRM:UUID]`** on **`MAIL_SEND_SUCCESS`** (MGM-armed send only) + winner whisper **`Награда отправлена тебе на почту, проверяй ящик!`** | `docs/overview/SPEC.md`, `README.md`, `docs/components/desktop/ReadME.md`, `docs/components/wow-addon/ReadME.md`, `docs/overview/ROADMAP.md` | Ready |
| Twitch chat reward-sent line | **`Награда отправлена персонажу <WINNER_NAME> на почту, проверяй ящик!`** (Extension hardcodes; Helix/EBS per §11) | `docs/overview/SPEC.md` §11, `docs/components/twitch-extension/UI_SPEC.md` UI-104, `docs/components/twitch-extension/ReadME.md` | Ready |
| Acceptance tag format | **`[MGM_ACCEPT:UUID]`** — **required** in **`WoWChatLog.txt`** for automated **`confirm-acceptance`** | `docs/overview/SPEC.md` §9–10, `docs/components/desktop/ReadME.md`, `docs/components/wow-addon/ReadME.md` | Ready |
| Roulette behavior | Visual roulette; **5-minute** spin only (**no** early spin); **min 1** participant; **non-winners stay**; **winners removed on `Sent`**; **`/who`** before finalize; **Twitch chat** enroll **`!twgold <CharacterName>`**; **WoW** winner whisper + reply **`!twgold`**; **subscriber**-gated | `docs/overview/SPEC.md`, `README.md`, `docs/overview/ROADMAP.md`, `docs/components/twitch-extension/ReadME.md`, `docs/components/desktop/ReadME.md`, `docs/components/wow-addon/ReadME.md` | Ready |
| Chat prefix & whisper consent | Enrollment **`!twgold`** prefix **case-insensitive**; WoW whisper **`!twgold`** consent **case-insensitive** (after trim) | `docs/overview/SPEC.md` §1, §5, §9–11; `README.md`; `AGENTS.md` | Ready |
| Spin schedule & Extension timer | **`nextSpinAt` / `serverNow`** server-authoritative; Extension **must** show countdown from API | `docs/overview/SPEC.md` §5.1, §11; `docs/components/twitch-extension/ReadME.md` | Ready |
| Minimum pool/roulette HTTP | **`GET /api/roulette/state`**, **`GET /api/pool/me`** (normative fields in `docs/overview/SPEC.md` §5.1) | `docs/overview/SPEC.md`, `docs/components/backend/ReadME.md` | Ready |
| Desktop `InProgress` gate | **`Pending` → `InProgress`** only after WoW target detected | `docs/overview/SPEC.md` §3; `docs/components/desktop/ReadME.md`; `docs/overview/INTERACTION_SCENARIOS.md` SC-011 | Ready |
| Outbox pattern | **Required** when external side effects exist (same tx as domain events) | `docs/overview/SPEC.md` §6; `docs/components/backend/ReadME.md` | Ready |
| Frontend state stack | Zustand is required in MVP | `docs/components/twitch-extension/ReadME.md` | Ready |
| `/who` → Backend path | Addon **`[MGM_WHO]`** + JSON in **`WoWChatLog.txt`** → Desktop **`verify-candidate`** (no disk file-bridge) | `docs/overview/SPEC.md` §5, §8, §10 | Ready |
| Extension poll resilience | Backoff + Retry on **`429`/`503`/network** (`docs/overview/SPEC.md` §5.1) | `docs/overview/SPEC.md`, `docs/components/twitch-extension/ReadME.md` | Ready |
| Spin candidate pick | **Uniform random** among active pool rows | `docs/overview/SPEC.md` glossary, §5 | Ready |
| `verify-candidate` grace | **30s** after **UTC** spin boundary that **closes** the cycle’s verification window | `docs/overview/SPEC.md` §5 | Ready |
| Winner whisper trigger | Desktop **`/run NotifyWinnerWhisper(id,name)`** → addon sends §9 **`/whisper`** | `docs/overview/SPEC.md` §8–9 | Ready |
| MediatR placement | Handlers in **Backend** only; **Shared** = contracts/validation | `docs/components/shared/ReadME.md` | Ready |
| MVP-5 Extension scope | **Viewer** UI only; **no** UI-201–204 | `docs/overview/ROADMAP.md`, `docs/components/twitch-extension/UI_SPEC.md` | Ready |
| Shared DTO field names | `PayoutDto` / `CreatePayoutRequest` use **`EnrollmentRequestId`** (idempotency for Extension claim path) per `docs/overview/SPEC.md` §4 | `docs/overview/SPEC.md`, `docs/components/shared/ReadME.md`, `src/MimironsGoldOMatic.Shared` | Ready |
| Shared `CharacterName` validation | **2–12** characters (after trim), **Latin/Cyrillic script letters only** in Shared (`CharacterNameRules` + FluentValidation) | `docs/overview/SPEC.md` §4, `docs/components/shared/ReadME.md` | Ready |

## Source code parity (MVP track)

Snapshot of `src/` versus `docs/overview/ROADMAP.md` (MVP-0 … MVP-6). Update this section when major scaffolding lands.

| MVP step | Roadmap intent | Current `src/` state |
|---|---|---|
| MVP-0 | `src/MimironsGoldOMatic.slnx` + project skeletons | **Complete (skeleton):** `slnx` lists the three .NET projects; `src/MimironsGoldOMatic.WoWAddon` and `src/MimironsGoldOMatic.TwitchExtension` exist as non-MSBuild trees (per `docs/overview/ROADMAP.md` MVP-0 — not part of the .NET solution file). |
| MVP-1 | Shared contracts + validation | **Complete:** `PayoutStatus`, `PayoutDto` (incl. `IsRewardSentAnnouncedToChat`), `CreatePayoutRequest`, `CharacterNameRules`, FluentValidation validators (see `docs/components/shared/ReadME.md`). |
| MVP-2 | Backend API + Marten/PostgreSQL | **Implemented:** Marten event store + read docs, MVP routes (`claim`, pool/roulette, payouts pending/status/confirm, verify-candidate), EventSub webhook, JWT + ApiKey auth, global rate limit (5/min per user/IP; EventSub exempt), Helix announcement + expiration job. Requires local Postgres + `appsettings` (`Mgm`, `Twitch`, connection string). |
| MVP-3 | WoW addon | **Implemented (MVP):** globals **`NotifyWinnerWhisper`**, **`ReceiveGold`**, **`MGM_RunWhoForSpin`**; **`[MGM_WHO]`** JSON, **`[MGM_ACCEPT]`** / **`[MGM_CONFIRM]`**; **`MAIL_SHOW`** queue panel + **Prepare Mail** (`SendMail*` + **`MoneyInputFrame_SetCopper`**); minimap + **`/mgm`**. **Optional / later:** UI-405 debug frame, scroll list polish, `MimironsGoldOMatic` singleton table refactor. |
| MVP-4 | WPF Desktop + WinAPI | **Implemented (MVP):** pending poll, **`NotifyWinnerWhisper`** + **Sync/Inject** (`ReceiveGold` chunking), **`WoWChatLog.txt`** tail (`[MGM_WHO]` / `[MGM_ACCEPT]` / `[MGM_CONFIRM]`), EBS **`PATCH`** overrides + **`InProgress`→`Pending`**, PostMessage→SendInput fallback, settings + DPAPI ApiKey. Manual verification on **WoW 3.3.5a** recommended. |
| MVP-5 | Twitch Extension UI | **Implemented (MVP):** Code in **`src/MimironsGoldOMatic.TwitchExtension`** (Vite/React/TS). Viewer panel: Twitch `onAuthorized`, EBS polling for roulette/pool/`my-last`, server-skew countdown, `spinPhase` UX per [`docs/components/twitch-extension/UI_SPEC.md`](../components/twitch-extension/UI_SPEC.md) **UI-101–106**; hub UI rules in [`docs/reference/UI_SPEC.md`](UI_SPEC.md). Zustand + backoff on 429/503/network. Configure **`VITE_MGM_EBS_BASE_URL`** (see `src/MimironsGoldOMatic.TwitchExtension/.env.example`); use Dev Rig for real JWTs. |
| MVP-6 | E2E demo + tests | **Completed (automated slices):** (1) **`dotnet test`** — `src/Tests/MimironsGoldOMatic.Backend.UnitTests` / **IntegrationTests** as above. (2) **CI Tier A + B** — [`.github/workflows/e2e-test.yml`](../../.github/workflows/e2e-test.yml): **EventSub mock → pool enrollment**, **E2E harness → SyntheticDesktop → MockHelix → `Sent` + pool removal** — [`docs/e2e/E2E_AUTOMATION_PLAN.md`](../e2e/E2E_AUTOMATION_PLAN.md). **Still manual / Tier C:** real **WoW client + WPF Desktop + live Twitch** operator validation per [`INTERACTION_SCENARIOS`](../overview/INTERACTION_SCENARIOS.md) ([Automated E2E Scenarios (MVP-6)](../overview/INTERACTION_SCENARIOS.md#automated-e2e-scenarios-mvp-6)); draft scope in [`docs/e2e/TIER_C_REQUIREMENTS.md`](../e2e/TIER_C_REQUIREMENTS.md). |

## MVP-6 verification status

| Area | Verification status | Notes |
|---|---|---|
| Backend business rules (pool, roulette, payouts, expiration, `Sent` pool removal) | **Automated** | `dotnet test src/MimironsGoldOMatic.slnx --filter Category=Integration` — requires **Docker** (**Testcontainers**). |
| Pure logic (spin schedule boundaries, `spinPhase`, **`!twgold`** line parsing) | **Automated** | `dotnet test src/MimironsGoldOMatic.slnx --filter Category=Unit` — no Docker. |
| Live Twitch EventSub → EBS enrollment | **Manual** | Real Twitch/Dev Rig; not covered as live traffic in **CI/CD**. |
| WoW 3.3.5a addon + `WoWChatLog.txt` tags + Desktop WinAPI inject | **Manual** | SC-001, SC-003, SC-004, etc.; no headless WoW harness in repo. |
| Helix **Send Chat Message** after **`Sent`** | **Automated (mocked in CI)** + **Manual (live Twitch)** | **MockHelixApi** + **`Twitch:HelixApiBaseUrl`** in **Tier B**; live broadcaster token not used in default PR workflow. |

## E2E Automation Progress

For details on the automation approach, see [E2E Automation Plan](../e2e/E2E_AUTOMATION_PLAN.md). Developer checklist and ownership: [E2E Automation Tasks](../e2e/E2E_AUTOMATION_TASKS.md).

- **Status:** **Tier A + Tier B** implemented in **CI** ([`e2e-test.yml`](../../.github/workflows/e2e-test.yml)); **Tier B** final validation: [Tier B Final Validation & Success Report](../e2e/E2E_AUTOMATION_PLAN.md#tier-b-final-validation--success-report).
- **Next:** [Tier C requirements (draft)](../e2e/TIER_C_REQUIREMENTS.md) (real Desktop/WoW, optional staging Twitch).
- **Baseline:** [`docs/overview/INTERACTION_SCENARIOS.md`](../overview/INTERACTION_SCENARIOS.md#automated-e2e-scenarios-mvp-6) (manual vs **Automated** targets).

## Residual implementation risks (not contradictions)

- Locked in **`docs/overview/SPEC.md`**: **EventSub** chat (subscriber flag from payload only for enroll); **single broadcaster** MVP; **real Twitch JWT** validation (Dev Rig + deploy); **JWT-only** Extension reads for **`/api/roulette/state`** + **`/api/pool/me`**; **`GET /api/payouts/my-last`** **`404`** when no payout; **`spinPhase`** enum (transitions **EBS**-defined); **UTC :00/:05** spin grid; **no re-draw** offline same cycle; spin **candidate** = **uniform random** among pool rows; **`verify-candidate`** **30s** grace after **UTC boundary closing** that cycle’s verification window; pool **replace** on re-enroll; **`POST /api/roulette/verify-candidate`** + **`[MGM_WHO]`** in **`WoWChatLog.txt`** (no file-bridge); **Desktop** **`/run NotifyWinnerWhisper`** → addon §9 whisper; **503** + Extension backoff (**§5**); **MediatR** handlers **EBS** only; **MVP-5** viewer Extension only; **Marten** stream **per payout id** + **separate Pool/Payout** aggregates; **§11 Helix** inline **3×** retry (**no** Outbox in MVP); **`InProgress`→`Pending`** allowed; **`CharacterName`** **§4**; WoW log **default + override**; unknown **`MGM_ACCEPT`** → log/ignore; Extension **429/503** backoff + Retry (**§5.1**); **`active_payout_exists`** — no auto-expire to force a new win.
- Remaining engineering detail (expected):
  - projection update strategy and replay/rebuild procedure;
  - concurrency control in command handlers;
  - **Helix subscriber verification** for **`POST /api/payouts/claim`** when **`Mgm:DevSkipSubscriberCheck`** is **`false`** (currently returns **`403 not_subscriber`** unless dev flag is set);
  - WinAPI timing/retry on real **3.3.5a** clients;
  - validate addon **`[MGM_WHO]`** / **`[MGM_ACCEPT]`** / **`[MGM_CONFIRM]`** visibility in **`WoWChatLog.txt`** on target clients;
  - **idempotent** **`confirm-acceptance`** / **`verify-candidate`** under log replay;
  - optional: extend **`CharacterName`** script ranges if product adds non–Latin/Cyrillic realm naming rules.

## Go/No-Go

- **Go (documentation):** MVP contracts are consistent across normative docs; agents can implement against `docs/overview/SPEC.md` + `docs/overview/ROADMAP.md`.
- **Go (product demo):** **No** — an end-to-end demo still needs Postgres/Twitch configuration and **running** Backend, Desktop, Extension (Dev Rig or hosted), and WoW addon against the same live API.

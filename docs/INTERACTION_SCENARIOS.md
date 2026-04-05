<!-- Updated: 2026-04-05 (MVP-6 status sync; E2E cross-refs) -->

# Mimiron's Gold-o-Matic — Interaction Scenarios & Test Cases

This document translates **`docs/SPEC.md`** into **interaction scenarios (SC-)** and **test cases (TC-)**. It does **not** invent behavior beyond those sources.

**References:** [`README.md`](../README.md), [`CONTEXT.md`](../CONTEXT.md), [`AGENTS.md`](../AGENTS.md), [`docs/SPEC.md`](SPEC.md), [`docs/ROADMAP.md`](ROADMAP.md), [`docs/UI_SPEC.md`](UI_SPEC.md) (hub) and [`docs/MimironsGoldOMatic.TwitchExtension/UI_SPEC.md`](MimironsGoldOMatic.TwitchExtension/UI_SPEC.md) / [`docs/MimironsGoldOMatic.Desktop/UI_SPEC.md`](MimironsGoldOMatic.Desktop/UI_SPEC.md) / [`docs/MimironsGoldOMatic.WoWAddon/UI_SPEC.md`](MimironsGoldOMatic.WoWAddon/UI_SPEC.md), [`docs/WORKFLOWS.md`](WORKFLOWS.md), [`docs/MVP_PRODUCT_SUMMARY.md`](MVP_PRODUCT_SUMMARY.md), component `ReadME.md` files under `docs/MimironsGoldOMatic.*/`.

End-to-end narrative: **[WORKFLOWS.md](WORKFLOWS.md)** · product digest: **[MVP_PRODUCT_SUMMARY.md](MVP_PRODUCT_SUMMARY.md)**.

<!-- Former long opening paragraph moved there. See: docs/WORKFLOWS.md · docs/MVP_PRODUCT_SUMMARY.md -->

**Implementation scope (roadmap phases):** Treat **`docs/SPEC.md`** as the only normative contract for **what to build**. Scenarios in this file marked **future / not MVP**, **placeholder**, or **open** (e.g. SC-020 pause/resume, SC-022 retry-token speculation) **must not** be implemented — **no** speculative endpoints (retry tokens, pause APIs, etc.) unless/until **`docs/SPEC.md`** adds them. Each roadmap step should follow the **mandatory checklist** the owner supplies (SPEC § references, TC ids, **no endpoints outside SPEC**).

### How to use Part 2 (test cases)

- **TC-xxx** rows are **verification targets** derived from `docs/SPEC.md` and related docs. They are **not** bound to existing automated tests until those suites exist.
- **When to run:** after the relevant MVP slice ships (e.g. **EBS** routes for TC-003+; Desktop WinAPI for TC-005+; addon mail path for TC-007+).
- **Automation:** **`dotnet test src/MimironsGoldOMatic.slnx`** — **Unit** project: **`MimironsGoldOMatic.Backend.UnitTests`**, **`--filter Category=Unit`** (no Docker). **Integration** project: **`src/Tests/MimironsGoldOMatic.Backend.IntegrationTests`**, **`--filter Category=Integration`** (**Testcontainers** / Docker). Full solution run executes both. Extension UI and WoW/Desktop flows remain **Manual** unless a harness is added; see [Automated E2E Scenarios (MVP-6)](#automated-e2e-scenarios-mvp-6).
- **Auth notes:** `docs/SPEC.md` requires **real Twitch-issued Extension JWTs** (Dev Rig and production). Tests must not rely on a long-term “mock JWT” bypass unless explicitly labeled as **temporary harness** and called out in test code.

---

## Automated E2E Scenarios (MVP-6)

**Overall status:** **Manual** (**target: Automated** in **CI/CD**).

This section maps the **intended** full product pipe — **Twitch chat message → Backend processing → WoW addon / Desktop behavior → Helix API outcome** — to how it is verified **today** vs what **automation** would add. It does **not** change **`docs/SPEC.md`** behavior. Roadmap alignment: **`docs/ROADMAP.md` MVP-6**; matrix: **`docs/IMPLEMENTATION_READINESS.md`** ([MVP-6 verification status](IMPLEMENTATION_READINESS.md#mvp-6-verification-status)).

| Step | Flow segment | Manual verification (today) | Target automated check (CI/CD) | Prerequisites / notes |
|---|---|---|---|---|
| 1 | **Twitch chat** enrollment (`!twgold`) → **Backend** pool | Execute **SC-005** (live **EventSub**) or Dev Rig / operator sends chat; confirm pool via **`GET /api/pool/me`** or DB. Optional: **`POST /api/payouts/claim`** for Extension-shaped enrollment in dev. | **Integration** tests already exercise **Backend** HTTP + persistence (not live Twitch). A future **CI** job could add signed **EventSub** fixture posts or keep relying on HTTP enrollment tests. | Running Backend + Postgres; Twitch credentials for **Manual** path. |
| 2 | **Backend** spin / **`verify-candidate`** / payout lifecycle | Operator aligns clock with spin grid; **Desktop** submits **`[MGM_WHO]`** payload via **`POST /api/roulette/verify-candidate`**; observe **`Pending`** and Extension state. | **`dotnet test`** **`Category=Integration`** (`PostClaimRulesIntegrationTests`, `RouletteVerifyCandidateIntegrationTests`, etc.). | **Docker** for **Testcontainers**. |
| 3 | **WoW addon** UI / mail queue + **`WoWChatLog.txt`** tags + **Desktop** WinAPI | **SC-001**, **SC-003**, **SC-004**: real **WoW 3.3.5a**, **`[MGM_WHO]`**, **`[MGM_ACCEPT]`**, **`[MGM_CONFIRM]`**, inject **`/run`**, mail send. | No automated test in repo (would require client harness or simulator). | Stable WoW + log path; streamer Desktop **ApiKey**. |
| 4 | **Helix** chat announcement (**§11**) after **`Sent`** | After **`PATCH` → `Sent`**, confirm chat line (**Russian** copy per **SPEC**) or inspect logs. | **CI** could mock **Helix** or use a test double; **not** implemented today. | **Twitch** app scopes + tokens for live check; **Backend** `Twitch:*` config. |

For details on the automation approach, see [E2E Automation Plan](E2E_AUTOMATION_PLAN.md).

### E2E Automation Progress

- **Plan:** [E2E Automation Plan](E2E_AUTOMATION_PLAN.md).
- **Tasks / ownership:** [E2E Automation Tasks](E2E_AUTOMATION_TASKS.md).
- **Roadmap:** [`docs/ROADMAP.md`](ROADMAP.md) MVP-6 **Next steps** and **E2E Automation Progress**.

**Related narrative:** **SC-001** (full end-to-end). **Next steps** for automation are listed under **`docs/ROADMAP.md` MVP-6 — Next steps**.

---

## Part 1 — Component Interaction Scenarios

### SC-001: Viewer obtains gold successfully (full end-to-end)

**Trigger:** Viewer is a **subscriber**, sends **`!twgold Norinn`** in **broadcast chat**, and later roulette selects them as **online-verified** winner; they **`!twgold`** in **WoW** (whisper reply) to consent and receive mail.

**Actor:** Viewer (Twitch + WoW), Streamer (WoW + Desktop), System (Backend chat + spin/schedule)

**Preconditions:** Backend reachable; chat ingestion active; viewer **subscribed**; **`CharacterName`** **unique** in pool; pool has ≥1 participant; Extension authorized for status UI (MVP: Dev Rig posture); Desktop eventually online with correct `ApiKey`; WoW 3.3.5a running (foreground for MVP).

**Flow:**

1. [Viewer] → [Twitch Chat]: `!twgold Norinn` | [ChatIngest] → [Backend]: enroll subscriber with **unique** name
2. [Backend] → [Backend]: persist **pool enrollment**; no `Pending` payout yet (`docs/SPEC.md` §5)
3. [System] → [Backend]: spin fires on **5-minute** schedule (UTC **:00/:05/…**); candidate drawn; **`currentSpinCycleId`** issued (`docs/SPEC.md` §5.1)
4. [WoWAddon] → [WoW Client]: run **`/who Norinn`**; parse **3.3.5a** result → **`DEFAULT_CHAT_FRAME:AddMessage`** **`[MGM_WHO]{...json}`** → **`Logs\WoWChatLog.txt`** (`docs/SPEC.md` §8) | [Desktop] tails log → **`POST /api/roulette/verify-candidate`** (`X-MGM-ApiKey`)
5. [Backend] → [Backend]: if **`online: true`**, create **payout** `Pending`; else **no winner** this cycle (**no** re-draw); expose state for Extension when **`Pending`** exists (**winner notification**)
6. [TwitchExtension] → [Viewer]: **“You won”** + instruct **WoW whisper reply `!twgold`** (`docs/SPEC.md` §11)
7. [Desktop] → [WoW Client]: inject **`/run NotifyWinnerWhisper("<payoutId>","Norinn")`** (`docs/SPEC.md` §8–9) → [WoWAddon] sends **winner notification whisper** (`/whisper Norinn …` Russian text per §9; **addon-only** typing)
8. [Viewer/WoW] → [Streamer/WoW]: whisper reply matching **`!twgold`** (**case-insensitive**)
9. [WoWAddon] → [WoW Chat Log]: print `[MGM_ACCEPT:<uuid>]` → [Desktop] tails `WoWChatLog.txt` | [Desktop] → [Backend]: `POST /api/payouts/{id}/confirm-acceptance`
10. [Desktop] → [Backend]: `GET /api/payouts/pending` → streamer **Sync/Inject**
11. [Desktop] → [Backend]: `PATCH /api/payouts/{id}/status` `{ "status": "InProgress" }`
12. [Desktop] → [WoW Client]: WinAPI **PostMessage** / **SendInput** fallback: `/run ReceiveGold("<chunked payload>")` \<255 chars per line (`docs/SPEC.md` §8)
13. [WoW Client] → [WoWAddon]: Lua `ReceiveGold(dataString)`; queue `MAIL_SHOW` UX; streamer sends mail on **MGM-armed** path (1000g = 10000000 copper)
14. [WoW Client] → [WoWAddon]: **`MAIL_SEND_SUCCESS`** → print `[MGM_CONFIRM:<uuid>]` → `Logs\WoWChatLog.txt`; whisper winner **`Награда отправлена тебе на почту, проверяй ящик!`** (`docs/SPEC.md` §9). **Manual** send without arm → no tag / no completion whisper.
15. [Desktop] → [Backend]: tail log → `PATCH /api/payouts/{id}/status` `{ "status": "Sent" }` → **remove winner from pool**
16. [Backend] and/or [TwitchExtension] → [Twitch Chat]: optional **`Награда отправлена персонажу <Winner> на почту, проверяй ящик!`** (`docs/SPEC.md` §11)

**Postconditions:** Payout `Sent`; winner **removed** from participant pool; pool row may be re-added later via **`!twgold <CharacterName>`**; viewer sees status in Extension; winner received **in-game** completion whisper; stream chat may show **§11** announcement.

**Failure exits:** enrollment rejected (not subscribed, duplicate **character name** in pool, cap, invalid name); **offline at `/who`** (**no winner** this cycle — **no** re-draw); missing **WoW whisper `!twgold`** consent; injection or mail failure; log never shows `MGM_CONFIRM`; API down at any HTTP step.

---

### SC-002: Streamer authenticates the WPF Desktop App against the API

**Trigger:** Streamer starts Desktop and connects to Backend.

**Actor:** Streamer

**Preconditions:** Backend configured with expected `X-MGM-ApiKey`; Desktop stores same secret.

**Flow:**

1. [Desktop] → [Backend]: first privileged call e.g. `GET /api/payouts/pending` | headers: `X-MGM-ApiKey: <secret>`
2. [Backend] → [Desktop]: `200 OK` + JSON list (may be empty) | or error if key invalid

**Postconditions:** Desktop can poll/patch payout APIs.

**Failure exits:** wrong/missing key → `403` `forbidden_apikey` (per `docs/SPEC.md` §5 error model); network failure.

---

### SC-003: WPF App connects to a running WoW 3.3.5a client via WinAPI

**Trigger:** Streamer launches Desktop with WoW already running (MVP: **foreground** `WoW.exe`).

**Actor:** Streamer

**Preconditions:** `WoW.exe` process present; Desktop has permission to use Win32 APIs.

**Flow:**

1. [Desktop] → [OS]: enumerate / find foreground WoW window (implementation-specific; e.g. `FindWindow`, `GetForegroundWindow`, process name `WoW.exe`)
2. [Desktop] → [WoW Client]: **PostMessage** (primary) to game input with `/run ...` or `/who ...` text path; **SendInput** if fallback selected

**Postconditions:** Desktop can inject chat commands reliably enough for MVP (`docs/SPEC.md` §8).

**Failure exits:** WoW not running; wrong window focused; anti-cheat blocking PostMessage; injection timing/focus failure.

---

### SC-004: WoW Addon confirms gold delivery via in-game mail and reports back

**Trigger:** Streamer completes **Send Mail** in UI after `ReceiveGold` populated fields (**MGM-armed** send path).

**Actor:** Streamer (in-game)

**Preconditions:** Mailbox open; addon queued payout; recipient accepted per product flow (`!twgold` already processed); gold available; send is **armed** for MGM (not a unrelated manual compose).

**Flow:**

1. [WoWAddon] → [Mail UI]: `SendMailNameEditBox`, subject, `MoneyInputFrame_SetCopper` (3.3.5a)
2. [WoW Client]: client submits mail → **`MAIL_SEND_SUCCESS`** (or **`MAIL_FAILED`**)
3. On **`MAIL_SEND_SUCCESS`** (MGM-armed only): [WoWAddon] → [Chat / WoWChatLog]: **`[MGM_CONFIRM:<payoutGuid>]`** (required for automated `Sent`); [WoWAddon] → [Winner]: whisper **`Награда отправлена тебе на почту, проверяй ящик!`** (`docs/SPEC.md` §9)
4. [Desktop] → [Backend]: observes log line → `PATCH` → `Sent`

**Postconditions:** Backend `Sent`; audit log shows mail-send confirmation path; winner got completion whisper.

**Failure exits:** mailbox closed; insufficient gold; invalid recipient; **`MAIL_FAILED`** (no tag / no completion whisper); manual send without MGM arm (no tag); Desktop misses log rotation.

---

### SC-005: Twitch EventSub delivers chat enrollment to the EBS

**Trigger:** A **subscriber** (badge on **`channel.chat.message`**) types **`!twgold Norinn`** in **broadcast** chat; Twitch POSTs an EventSub notification to the EBS.

**Actor:** Viewer, System (Twitch → EBS)

**Preconditions:** EventSub subscription **`channel.chat.message`** is **enabled**; callback URL reaches **`POST /api/twitch/eventsub`**; **`Twitch:EventSubSecret`** matches (or empty for local dev only); **`ConnectionStrings:PostgreSQL`** available.

**Flow:**

1. [Twitch] → [Backend]: `POST /api/twitch/eventsub` with signed headers and JSON body (`subscription.type` = `channel.chat.message`, `event` contains `message_id`, `chatter_user_id`, `message.text`, `badges`).
2. [Backend] → [Backend]: verify HMAC when secret configured; parse **`!twgold <CharacterName>`**; if **not** subscriber per badges → log / ignore; else dedupe by **`message_id`**, validate name, update **pool** (replace same **`TwitchUserId`** row per `docs/SPEC.md` §5).

**Postconditions:** Pool row exists for viewer; **no** `Pending` payout until a spin + **`verify-candidate`** path succeeds.

**Failure exits:** wrong signature → **`401`**; malformed payload → ignored or minimal response; duplicate **`message_id`** → no-op; name taken by another user → silent reject (no pool change); active payout / lifetime cap → silent reject.

---

### SC-010: API receives enrollment / spin updates but WPF Desktop App is offline

**Trigger:** Viewers enroll; Backend creates `Pending` winner payout; Desktop not running.

**Actor:** System / Viewer

**Preconditions:** Backend up; Desktop down or not polling.

**Flow:**

1. [Viewer] → [Twitch Chat] / [System]: enrollment + scheduled spin complete → **`Pending`** payout exists on **EBS** (Extension may only poll for status — not required to create the payout).
2. [Backend] → [Desktop]: **no** poll — queue grows in DB only

**Postconditions:** Payouts stay `Pending` until Desktop polls or **hourly job** may later `Expired` if >24h (`docs/SPEC.md` §7).

**Failure exits:** streamer cannot inject until Desktop online; viewer stuck waiting past UX expectations (product issue, not separate error code in spec).

---

### SC-011: WPF App is running but WoW client process is not found

**Trigger:** Streamer clicks **Sync/Inject** or `/who` automation runs without WoW.

**Actor:** Streamer

**Preconditions:** Desktop authenticated; no `WoW.exe` / no target window.

**Flow:**

1. [Desktop] → [OS]: locate WoW → **fails**
2. [Desktop] → [Desktop]: UI state “Searching for WoW” / error; **no** successful `PostMessage`

**Postconditions:** No injection; payout remains **`Pending`** (Desktop **must not** `PATCH` to **`InProgress`** until WoW is detected — `docs/SPEC.md` §3).

**Failure exits:** user never launches WoW; wrong client build.

> **Resolution (product):** Desktop **must not** transition **`Pending` → `InProgress`** until the WoW client target is found (`docs/SPEC.md` §3).

---

### SC-012: WoW character name in the request does not exist on the realm

**Trigger:** Viewer enrolls with a name that is not a real character on the streamer’s realm/faction context (format-valid).

**Actor:** Viewer

**Preconditions:** Backend validates **format** only (shared **`CharacterNameRules`**). MVP does **not** call external realm/Armory APIs.

**Flow:**

1. [Viewer] → [Twitch Chat]: **`!twgold <Name>`** (primary path) **or** [TwitchExtension] → [Backend]: **`POST /api/payouts/claim`** (optional; requires **`Mgm:DevSkipSubscriberCheck`** for local Dev Rig while Helix subscriber check on claim is unimplemented — see `docs/SPEC.md` §5).
2. [Backend] → [Backend]: if rules pass, **pool** row may be stored (`docs/SPEC.md` §4, §5).
3. At **spin / win** time, **`/who <Name>`** in-game is the **online / presence** check (`docs/SPEC.md` glossary). A non-existent or offline name yields **no `Pending` payout** that cycle (**no** re-draw). If a payout still reaches mail UX incorrectly, streamer may use **manual** **`Failed`**.

**Postconditions:** Possible **enrollment** stored; payout delivery may hit **Failed** in Desktop or streamer correction.

**Failure exits:** mail cannot be delivered to non-existent toon; addon/mail API errors.

> **Resolution (product):** No separate “realm database” lookup in MVP. **In-game `/who`** is the normative check when a candidate winner is evaluated; enrollment remains **format + pool rules** only.

---

### SC-013: Duplicate gold request for the same viewer within cooldown window

**Trigger:** Same viewer repeats enroll submit or rapid duplicate HTTP calls before rate limit window elapses.

**Actor:** Viewer / network retry

**Preconditions:** Existing enrollment for same `EnrollmentRequestId` or active payout per user.

**Flow (idempotent enroll):** [TwitchExtension] → [Backend]: duplicate `enrollmentRequestId` → `200 OK` same enrollment (`docs/SPEC.md` §4).

**Flow (rate limit):** burst of requests → `429` or server rate-limit response (ASP.NET Core rate limiter ~5/min per SPEC).

**Flow (active payout):** second **winner** payout while one **active** → `409`-style error body e.g. `active_payout_exists` (`docs/SPEC.md` §5).

**Postconditions:** No double-spend via same enrollment request id; abuse bounded by rate limit.

**Failure exits:** client mishandles idempotent `200` vs `201`.

---

### SC-014: Twitch token validation fails on the API side

**Trigger:** Extension calls Backend with missing/invalid/expired Twitch JWT.

**Actor:** Viewer

**Preconditions:** Backend validates **real Twitch-issued** Extension JWTs (**Dev Rig** and deploy — `docs/SPEC.md` deployment scope).

**Flow:**

1. [TwitchExtension] → [Backend]: `POST /api/payouts/claim` with bad `Authorization: Bearer …`
2. [Backend] → [TwitchExtension]: `401 unauthorized` | body: `{ "code": "unauthorized", … }` (recommended shape §5)

**Postconditions:** No pool write.

**Failure exits:** clock skew; wrong extension secret; Dev Rig misconfiguration.

---

### SC-015: WoW Addon fails to send mail (mailbox not open / target unreachable)

**Trigger:** Stream triggers mail send while mailbox closed, bad recipient, or insufficient gold.

**Actor:** Streamer

**Preconditions:** Payout `InProgress`; addon queue has entry.

**Flow:**

1. [WoWAddon] wrapper rejects send → **no** `MGM_CONFIRM` line
2. [Desktop]: no `Sent` from automation
3. [Streamer] → [Desktop]: **Mark as Failed** or retry after fixing (`docs/SPEC.md` transitions allow `Failed`)

**Postconditions:** Payout `Failed` or remains `InProgress` until operator acts.

**Failure exits:** silent addon bug; duplicate send attempts (state machine should prevent per WoWAddon ReadME).

---

### SC-016: API request queue overflow (too many concurrent requests)

**Trigger:** Many Extensions or bots hit API above configured limits.

**Actor:** External clients

**Preconditions:** Rate limiter / server max concurrency configured.

**Flow:**

1. [Clients] → [Backend]: burst HTTP
2. [Backend] → [Clients]: `429 Too Many Requests` or server-defined throttling (ASP.NET rate limiting per SPEC ~5 req/min per IP/user)

**Postconditions:** No unbounded queue assumed in docs—**reject or delay** per implementation.

**Failure exits:** DDoS beyond app layer; DB overload not detailed in MVP docs.

**Resolution:** Global saturation may yield **`503`**; Extension backoff + Retry per **`docs/SPEC.md` §5** error model and **§5.1**. Per-user **`429`** remains ~5 req/min target.

---

### SC-017: WPF App loses connection to API mid-session

**Trigger:** Network drop during poll, PATCH, or confirm calls.

**Actor:** System

**Preconditions:** Desktop mid-flow (e.g. after addon signaled `!twgold` locally).

**Flow:**

1. [Desktop] → [Backend]: `GET /api/payouts/pending` → **IOException / timeout**
2. [Desktop]: Polly retry/backoff (per Desktop ReadME); surfaced in UI

**Postconditions:** Eventually consistent if retries succeed; operator may use overrides.

**Failure exits:** prolonged outage → stale `InProgress`; missed **`MGM_CONFIRM`** sync until back online.

---

### SC-020: Streamer pauses/resumes gold distribution *(future / not MVP)*

**Status:** **Placeholder scenario** — documents a possible future control; **do not** implement or test as current product behavior. **`docs/SPEC.md`** — **pause/resume** is **not** in MVP.

**Trigger:** Streamer wants to temporarily stop processing payouts.

**Actor:** Streamer

**Preconditions:** None defined in SPEC for a **pause** flag.

**Flow:**

> **Resolved for MVP:** no pause flag; streamer stops processing only by **operational** means (e.g. not running Desktop, not confirming mail, or cancelling payouts per other scenarios).

**Postconditions (conceptual):** A later spec may add a pause flag; until then, operator-only workarounds.


**Failure exits:** N/A until specified.

---

### SC-021: Streamer manually cancels a pending request

**Trigger:** Streamer chooses cancel in Desktop for a `Pending` (or allowed state) payout.

**Actor:** Streamer

**Preconditions:** Payout in `Pending` (or as allowed by transition table §3).

**Flow:**

1. [Desktop] → [Backend]: `PATCH /api/payouts/{id}/status` `{ "status": "Cancelled" }` + `X-MGM-ApiKey`

**Postconditions:** Payout `Cancelled` (terminal for operator purpose per lifecycle).

**Failure exits:** illegal transition → `terminal_status_change_not_allowed`; wrong id → `404`.

---

### SC-022: System retries a failed delivery attempt *(operational / not a separate API in MVP)*

**Trigger:** First injection or mail attempt failed; streamer retries.

**Actor:** Streamer / Desktop

**Preconditions:** Payout `InProgress` or returned to operable state per policy.

**Flow:**

1. [Desktop] → [WoW Client]: re-issue `/run ReceiveGold("...")` after fixing root cause **or** streamer completes mail manually with addon still emitting `MGM_CONFIRM`

**Postconditions:** `MGM_CONFIRM` observed → `Sent`.

**Failure exits:** double payout if state machine buggy (mitigated by single active payout rule).

> **MVP (locked):** **Do not** implement **retry tokens**, **retry endpoints**, or other speculative Backend APIs for this scenario. Recovery is **operator-driven** re-inject / same payout id (**idempotent** client behavior per **`docs/SPEC.md`**). A future spec may add machinery; until then, **ignore** this scenario for new API surface.

---

## Part 2 — Test Cases

### TC-001: E2E — Pool to Sent (happy path)

**Covers:** SC-001

**Component under test:** E2E (TwitchExtension + Backend + Desktop + WoWAddon)

**Type:** E2E

**Preconditions:**

- Test Backend URL; valid `ApiKey`; Dev Rig JWT or mocks; WoW test client with addon; Desktop test build; clearing known payout rows for test Twitch user.

**Input:**

| Field | Value |
|-------|-------|
| characterName | Norinn |
| enrollmentRequestId | 550e8400-e29b-41d4-a716-446655440000 |
| twitchUserId | 90001337 |
| goldAmount (winner payout) | 1000g (10000000 copper) |

**Steps:**

1. Simulate chat **`!twgold Norinn`** → pool enroll **or** Extension `POST /api/payouts/claim` → `201`
2. Simulate/trigger spin + `/who` success (test hook or scripted Desktop)
3. Assert Backend has `Pending` payout for Norinn
4. Extension shows winner UX; simulate **WoW whisper `!twgold`** → Desktop `confirm-acceptance` → success
6. Desktop `PATCH InProgress`, inject `ReceiveGold`, complete mail in-game
7. Assert `WoWChatLog.txt` contains `[MGM_CONFIRM:<id>]`
8. Desktop `PATCH Sent`

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| Final GET payout status | `Sent` |
| HTTP PATCH last call | `200` |
| confirm-acceptance | `200`; `WinnerAcceptedWillingToReceiveAt` set in read model (if exposed) |

**Expected Side Effects:**

- Marten events appended; read model updated; optional Polly retries on transient failures (no unbounded loops).

**Notes:** 3.3.5a **`MAIL_SHOW`** / whisper event names must match addon; E2E may be split into **stubs** for `/who` parsing.

---

### TC-002: E2E — Offline winner at `/who` (failure)

**Covers:** SC-001

**Component under test:** Backend + Desktop (roulette orchestration)

**Type:** Integration

**Preconditions:** Pool with one character known offline in test harness.

**Input:**

| Field | Value |
|-------|-------|
| characterName | Offlinebob |
| Simulated `/who` result | not found / offline |

**Steps:**

1. Enroll Offlinebob
2. Force spin selecting Offlinebob
3. Run `/who Offlinebob`; mock parser returns offline

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| `Pending` payout for this spin | **not** created (or spin re-issues per policy) |
| Extension | no “You won” for loser of invalid draw |

**Expected Side Effects:**

- No **`Pending`** payout created for this spin cycle; pool unchanged until next scheduled spin.

**Notes:** **`docs/SPEC.md`** — **no re-draw** in the same **5-minute** cycle; no second candidate pick.

---

### TC-003: Desktop API key accepted

**Covers:** SC-002

**Component under test:** Backend

**Type:** Integration

**Preconditions:** Known good API key in config.

**Input:**

| Field | Value |
|-------|-------|
| Header X-MGM-ApiKey | `<configured-secret>` |

**Steps:**

1. `GET /api/payouts/pending` with header

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| HTTP status | `200` |
| Body | JSON array (possibly empty) |

**Expected Side Effects:** None beyond auth logging.

**Notes:** None.

---

### TC-004: Desktop API key rejected

**Covers:** SC-002

**Component under test:** Backend

**Type:** Integration

**Preconditions:** Same as TC-003 but wrong key.

**Input:**

| Field | Value |
|-------|-------|
| Header X-MGM-ApiKey | `wrong-key` |

**Steps:**

1. `GET /api/payouts/pending`

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| HTTP status | `403` |
| body.code | `forbidden_apikey` |

**Expected Side Effects:** None.

**Notes:** Align with `docs/SPEC.md` §5 error model.

---

### TC-005: WinAPI inject happy path

**Covers:** SC-003

**Component under test:** Desktop

**Type:** Integration

**Preconditions:** WoW running; test window focus.

**Input:**

| Field | Value |
|-------|-------|
| Command line (example) | `/run ReceiveGold("…")` \<255 chars |

**Steps:**

1. Desktop resolves WoW window
2. Send via **PostMessage** strategy
3. Verify addon received (test stub or in-game echo)

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| WoW chat box / addon | command executed |

**Expected Side Effects:** WinAPI `PostMessage` to game window message queue.

**Notes:** Timing/focus documented for 3.3.5a compatibility per project rules.

---

### TC-006: WoW process not found

**Covers:** SC-011

**Preconditions:** No `WoW.exe`; Desktop running.

**Input:** N/A

**Steps:**

1. Desktop `FindWindow` / process scan

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| UI / error | “WoW not found”; no inject |

**Expected Side Effects:** Optional telemetry.

**Notes:** MVP targets **foreground** `WoW.exe` only (`docs/SPEC.md` §8).

---

### TC-030: WinAPI inject fails (focus / PostMessage rejected)

**Covers:** SC-003

**Component under test:** Desktop

**Type:** Integration

**Preconditions:** WoW running but HWND wrong or injection test double returns failure.

**Input:**

| Field | Value |
|-------|-------|
| Strategy | PostMessage (primary) |

**Steps:**

1. Attempt inject without correct focus
2. Optionally switch to SendInput fallback per settings

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| First attempt | no addon `ReceiveGold` observed OR error surfaced |
| Fallback (if enabled) | may succeed per strategy pattern |

**Expected Side Effects:** Logged Win32 error codes.

**Notes:** Align with `IWoWInputStrategy` (`docs/MimironsGoldOMatic.Desktop/ReadME.md`).

---

### TC-031: WoW launched after initial “not found” state

**Covers:** SC-011

**Component under test:** Desktop

**Type:** Integration

**Preconditions:** Start with no WoW; then user starts `WoW.exe`.

**Steps:**

1. Desktop scan → not found (same as TC-006)
2. Launch WoW; Desktop periodic re-scan

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| After launch | process found; UI **Ready to Inject** (or equivalent) |

**Expected Side Effects:** Subsequent PostMessage allowed.

**Notes:** Recovery path for streamer workflow.

---

### TC-007: Addon emits MGM_CONFIRM and completion whisper after MGM-armed MAIL_SEND_SUCCESS

**Covers:** SC-004

**Component under test:** WoWAddon

**Type:** Integration (in-client or harness)

**Preconditions:** Mail compose filled for payout `a1b2c3d-1111-2222-3333-444444444444`; mailbox open; send **MGM-armed**.

**Input:**

| Field | Value |
|-------|-------|
| payoutId | a1b2c3d-1111-2222-3333-444444444444 |

**Steps:**

1. Trigger confirmed send through addon/MGM path → **`MAIL_SEND_SUCCESS`**
2. Read `WoWChatLog.txt` and verify winner whisper (in-client)
3. Send mail manually without MGM arm → **`MAIL_SEND_SUCCESS`** must **not** emit `[MGM_CONFIRM:…]`

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| MGM path log line | `[MGM_CONFIRM:a1b2c3d-1111-2222-3333-444444444444]` |
| Winner whisper body | `Награда отправлена тебе на почту, проверяй ящик!` |
| Manual send | no `[MGM_CONFIRM]` |

**Expected Side Effects:** Lua prints tag; completion whisper via **`SendChatMessage`**; no HTTP from Lua.

**Notes:** `docs/SPEC.md` §9 (**`MAIL_SEND_SUCCESS`** / **`MAIL_FAILED`**).

---

### TC-008: Mail send blocked — no MGM_CONFIRM

**Covers:** SC-004

**Component under test:** WoWAddon

**Type:** Unit / Integration

**Preconditions:** Mailbox **closed** or validation fails in wrapper.

**Steps:**

1. Attempt send via wrapper

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| MGM_CONFIRM | **absent** |
| Queue state | remains PROCESSING or READY per state machine |

**Expected Side Effects:** None on Backend until operator fails payout.

**Notes:** `SendMail` frame names must match 3.3.5a.

---

### TC-009: Pending payouts accumulate when Desktop offline

**Covers:** SC-010

**Component under test:** Backend

**Type:** Integration

**Preconditions:** Create `Pending` payout via test API; no Desktop polls.

**Steps:**

1. Seed `Pending` row
2. Wait (no `PATCH` from Desktop)

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| Status | remains `Pending` until Desktop or expiration job |

**Expected Side Effects:** Hourly job may set `Expired` after 24h.

**Notes:** None.

---

### TC-010: Desktop polls and drains queue

**Covers:** SC-010

**Component under test:** Desktop + Backend

**Type:** Integration

**Preconditions:** `Pending` exists; Desktop starts.

**Steps:**

1. `GET /api/payouts/pending`

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| HTTP status | `200` |
| Body | includes `Pending` payout id |

**Expected Side Effects:** None.

**Notes:** Verifies recovery from SC-010 scenario.

---

### TC-011: Invalid character name format rejected

**Covers:** SC-012

**Component under test:** Backend

**Type:** Unit / Integration

**Input:**

| Field | Value |
|-------|-------|
| characterName | `Bad:Name` |

**Steps:**

1. `POST /api/payouts/claim`

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| HTTP status | `400` (or domain equivalent) |
| body.code | `invalid_character_name` |

**Expected Side Effects:** No pool row.

**Notes:** Realm existence check not in MVP spec—see SC-012 open question.

---

### TC-012: Format-valid but nonexistent character — streamer marks Failed

**Covers:** SC-012

**Component under test:** Desktop + Backend

**Type:** E2E (manual)

**Preconditions:** Enrolled name `Ghostcow` never created on realm.

**Steps:**

1. Complete flow until mail fails
2. `PATCH` `{ "status": "Failed" }`

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| Final status | `Failed` |

**Expected Side Effects:** Event `Failed` in store.

**Notes:** Documents operator escape hatch; not automatic API validation.

---

### TC-013: Idempotent duplicate enrollmentRequestId

**Covers:** SC-013

**Component under test:** Backend

**Type:** Integration

**Input:**

| Field | Value |
|-------|-------|
| enrollmentRequestId | 550e8400-e29b-41d4-a716-446655440001 |

**Steps:**

1. `POST /api/payouts/claim` → `201`
2. Repeat same body + id → `200`

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| Second response | `200 OK`; same logical enrollment |
| DB unique constraint | no duplicate row for id |

**Expected Side Effects:** None.

**Notes:** Per `docs/SPEC.md` §4.

---

### TC-014: Rate limit exceeded

**Covers:** SC-013 / SC-016

**Component under test:** Backend

**Type:** Integration

**Preconditions:** Configure low rate limit for test.

**Steps:**

1. Issue >5 requests/min from same user/IP

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| HTTP status | `429` (or 403 per host policy) |

**Expected Side Effects:** No unbounded queue assumed.

**Notes:** `docs/SPEC.md` §2 / §5.

---

### TC-015: active_payout_exists on second winner flow

**Covers:** SC-013

**Component under test:** Backend

**Type:** Integration

**Preconditions:** Test user already has `Pending` or `InProgress` payout.

**Steps:**

1. Attempt operation that violates one-active rule (e.g. second concurrent winning payout—exact endpoint depends on spin implementation)

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| body.code | `active_payout_exists` |

**Expected Side Effects:** None.

**Notes:** Exact trigger route may be spin finalize API when added.

---

### TC-016: Unauthorized Extension claim

**Covers:** SC-014

**Component under test:** Backend

**Type:** Integration

**Preconditions:** JWT validation **strict** mode ON.

**Steps:**

1. `POST /api/payouts/claim` without valid Bearer

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| HTTP status | `401` |
| body.code | `unauthorized` |

**Expected Side Effects:** None.

**Notes:** Dev Rig may bypass—label test environment.

---

### TC-017: Valid Extension JWT (Dev Rig or deployed)

**Covers:** SC-014

**Component under test:** Backend + TwitchExtension

**Type:** Integration

**Preconditions:** Dev Rig (or deployed Extension) session with a **real Twitch-issued** Extension JWT, as required by `docs/SPEC.md` (MVP deployment scope).

**Steps:**

1. Call a JWT-protected enroll or read endpoint (e.g. `POST /api/payouts/claim` when implemented) with valid `Authorization: Bearer <token>`.

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| HTTP status | `201` or `200` (or other success per endpoint contract) |

**Expected Side Effects:** Enrollment or read succeeds per `docs/SPEC.md`.

**Notes:** Stricter production JWT rotation and issuer checks are a roadmap hardening item; MVP still uses **validated real tokens**, not a permanent mock bypass.

---

### TC-018: Mailbox closed — no Sent

**Covers:** SC-015

**Component under test:** WoWAddon

**Type:** Integration

**Steps:**

1. Attempt send path without `MAIL_SHOW`

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| No MGM_CONFIRM | true |

**Expected Side Effects:** Streamer uses Fail in Desktop.

**Notes:** 3.3.5a FrameXML.

---

### TC-019: Streamer PATCH Failed after mail error

**Covers:** SC-015

**Component under test:** Backend

**Type:** Integration

**Steps:**

1. `PATCH /api/payouts/{id}/status` `{ "status": "Failed" }` from `InProgress`

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| HTTP status | `200` |
| Read model status | `Failed` |

**Expected Side Effects:** Terminal for manual resolution.

**Notes:** Transition table `docs/SPEC.md` §3.

---

### TC-020: Rate limit smoke (concurrent clients)

**Covers:** SC-016

**Component under test:** Backend

**Type:** Load / Integration

**Steps:**

1. Parallel `POST` from N virtual users

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| p99 latency / errors | bounded; `429` under overload |

**Expected Side Effects:** None.

**Notes:** **`503`** / global saturation — `docs/SPEC.md` §5 error model + §5.1 Extension backoff.

---

### TC-021: Single client under limit succeeds

**Covers:** SC-016

**Component under test:** Backend

**Type:** Integration

**Steps:**

1. ≤5 req/min per doc target from one identity

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| Responses | `2xx` for valid bodies |

**Expected Side Effects:** None.

**Notes:** Tune limiter constants to SPEC.

---

### TC-022: API outage — Polly retries

**Covers:** SC-017

**Component under test:** Desktop

**Type:** Integration (chaos)

**Preconditions:** Wiremock drops first N requests.

**Steps:**

1. `GET /api/payouts/pending`

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| Outcome after retries | success or surfaced failure |

**Expected Side Effects:** Polly backoff (Desktop ReadME).

**Notes:** Avoid infinite retry loops.

---

### TC-023: API outage — exhausted retries

**Covers:** SC-017

**Component under test:** Desktop

**Type:** Integration

**Preconditions:** Backend down prolonged.

**Steps:**

1. Poll until Polly gives up

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| UI | error state; queue not updated |

**Expected Side Effects:** None on server.

**Notes:** Operator restarts when network returns.

---

### TC-024: Pause/resume placeholder (not in SPEC)

**Covers:** SC-020

**Component under test:** Backend / Desktop

**Type:** N/A

**Preconditions:** None.

**Steps:** Document only until spec adds pause.

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| Test | skipped / pending requirements |

**Expected Side Effects:** None.

**Notes:** Align with SC-020 *(future / not MVP)* — no pause in spec until added.

---

### TC-025: Unauthorized pause if implemented later

**Covers:** SC-020

**Component under test:** Backend

**Type:** Unit (future)

**Steps:** Placeholder.

**Expected Result:** N/A

**Expected Side Effects:** N/A

**Notes:** Reserved ID; implement when feature exists.

---

### TC-026: Cancel Pending payout

**Covers:** SC-021

**Component under test:** Backend

**Type:** Integration

**Preconditions:** Payout `Pending`.

**Steps:**

1. `PATCH /api/payouts/{id}/status` `{ "status": "Cancelled" }` + ApiKey

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| HTTP status | `200` |
| status | `Cancelled` |

**Expected Side Effects:** Event stored.

**Notes:** None.

---

### TC-027: Illegal transition Cancel from Sent

**Covers:** SC-021

**Component under test:** Backend

**Type:** Integration

**Preconditions:** Payout already `Sent`.

**Steps:**

1. `PATCH` `{ "status": "Cancelled" }`

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| HTTP status | `400` / `409` |
| body.code | `terminal_status_change_not_allowed` |

**Expected Side Effects:** None.

**Notes:** §3 transitions.

---

### TC-028: Retry inject after first PostMessage failure

**Covers:** SC-022

**Component under test:** Desktop

**Type:** Integration

**Steps:**

1. First PostMessage returns failure path in test double
2. Second PostMessage succeeds

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| Addon receives | payload once (no duplicate queue entries if idempotent) |

**Expected Side Effects:** Second WinAPI PostMessage.

**Notes:** Backend **should** reject illegal transitions; duplicate **`MGM_CONFIRM`** → idempotent **`PATCH`** or stable error (`docs/SPEC.md` §3).

---

### TC-029: Second MGM_CONFIRM ignored for Sent payout

**Covers:** SC-022

**Component under test:** Desktop + Backend

**Type:** Integration

**Preconditions:** Payout already `Sent`.

**Steps:**

1. Simulate duplicate `[MGM_CONFIRM:uuid]` line in log

**Expected Result:**

| Assertion | Expected Value |
|-----------|----------------|
| Backend | no invalid double transition (idempotent PATCH or reject) |

**Expected Side Effects:** Logged warning optional.

**Notes:** Align with terminal state rules.

---

## Component Contracts

Summary of **documented** boundaries. Pool/spin polling routes are defined in **`docs/SPEC.md` §5.1**.

**MVP boundary:** **Twitch Extension** and **WPF Desktop** have **no direct** integration (no shared socket, no peer channel). Viewers use the Extension plus **broadcast chat**; the streamer uses Desktop plus **WoW**; both sides coordinate through **HTTP** to the **EBS** / Desktop paths (**Twitch Extension JWT** vs `X-MGM-ApiKey`, **`docs/SPEC.md`**), and mail completion uses the **WoW chat log** bridge per **`docs/SPEC.md` §8–10**.

### Twitch Extension → ASP.NET Core API

| Direction | Message/Endpoint | Payload shape | Success response | Failure response |
|-----------|------------------|---------------|------------------|------------------|
| Extension → API | `POST /api/payouts/claim` (optional) | `{ "characterName": string, "enrollmentRequestId": string }` + Twitch JWT (subscriber verified server-side) | `201 Created` (new enroll) or `200 OK` (idempotent) | `400` `invalid_character_name`; `401` `unauthorized`; `403`/`400` if not subscribed; `409`/`400` `character_name_taken_in_pool`; `429` rate limit; cap errors e.g. `lifetime_cap_reached` |
| Chat → EBS (Backend) | **Twitch EventSub** transport → `channel.chat.message` | Enrollment text **`!twgold <CharacterName>`** parsed from event payload (`docs/SPEC.md` §5; **EBS** / EventSub in SPEC) | pool enroll / replace per `docs/SPEC.md` §5 | domain errors per `docs/SPEC.md` §5 |
| Extension → API | `GET /api/payouts/my-last` | Twitch Extension **JWT** (Bearer) | `200` + **`PayoutDto`** | **`404`** when no winner payout |
| Extension → API | `GET /api/roulette/state` | Twitch Extension **JWT** | `200` + schedule + **`spinPhase`** enum + optional **`currentSpinCycleId`** (`docs/SPEC.md` §5.1) | `401`, `429`, domain errors |
| Extension → API | `GET /api/pool/me` | Twitch Extension **JWT** | `200` + enrollment hint | `401`, `429` |
| API → Extension | (pull only in MVP) | — | Extension polls for **winner notification** / status | Error boundary UI per TwitchExtension ReadME |

### EBS (ASP.NET Core API) → Twitch broadcast chat

Normative copy and **Helix** delivery: **`docs/SPEC.md` §11** (reward-sent announcement when a payout becomes **`Sent`**; **EBS**-owned credentials).

| Direction | Message/Endpoint | Payload shape | Success response | Failure response |
|-----------|------------------|---------------|------------------|------------------|
| EBS → Chat | **Twitch Helix** `Send Chat Message` **immediately** on transition to **`Sent`** (MVP locked) | One line: `Награда отправлена персонажу <WINNER_NAME> на почту, проверяй ящик!` (`WINNER_NAME` = enrolled **`CharacterName`**) | Message visible in **broadcast** chat | Helix/auth errors (`401`/`403`), rate limits; must **not** depend on WoW addon |

> **Note:** The chat line is **not** a return path into Desktop or WoW; it is **broadcast-only** viewer notice aligned with Extension hardcoded copy.

### ASP.NET Core API → WPF Desktop App

| Direction | Message/Endpoint | Payload shape | Success response | Failure response |
|-----------|------------------|---------------|------------------|------------------|
| API ← Desktop | `GET /api/payouts/pending` | header `X-MGM-ApiKey` | `200` JSON list of **winner** payouts (`Pending` primary) | `403` `forbidden_apikey` |
| API ← Desktop | `POST /api/roulette/verify-candidate` | JSON from **`[MGM_WHO]`** line + `X-MGM-ApiKey` (`docs/SPEC.md` §5, §8) | `200` (may create `Pending`) | `400`; `403` |
| API ← Desktop | `PATCH /api/payouts/{id}/status` | `{ "status": "…" }` — allowed: `Pending`/`InProgress`/`Sent`/`Failed`/`Cancelled` per **`docs/SPEC.md` §3** (includes **`InProgress` → `Pending`** escape hatch) | `200` | `400` `terminal_status_change_not_allowed`; `403`; `404` `not_found` |
| API ← Desktop | `POST /api/payouts/{id}/confirm-acceptance` | `{ "characterName": string }` (**required**) | `200` (acceptance recorded; not `Sent`) | `403`; `404` |
| API ← Desktop | Mail-send path | Desktop derives from log → `PATCH` → `Sent` | `200` | same as PATCH |

> Desktop **does not** consume a Backend push channel in MVP docs; **polling** is implied.

### WPF Desktop App → WoW Client (WinAPI / PostMessage)

| Direction | Message/Endpoint | Payload shape | Success response | Failure response |
|-----------|------------------|---------------|------------------|------------------|
| Desktop → WoW | Win32 focus + **PostMessage** (primary) | Window handle + chat/input messages carrying **`/run NotifyWinnerWhisper("uuid","Name")`**, **`/run ReceiveGold("…")`**, or **`/who Name`** text | Game executes; addon/Lua runs | Wrong HWND; focus timing; anti-cheat block |
| Desktop → WoW | **SendInput** (fallback) | OS input synthesize | Same | User-configured fallback failures |
| WoW → Desktop | (no direct callback) | — | — | — |

### WoW Client → WoW Addon (Lua)

| Direction | Message/Endpoint | Payload shape | Success response | Failure response |
|-----------|------------------|---------------|------------------|------------------|
| Client → Addon | Global `NotifyWinnerWhisper(payoutId, characterName)` | via **`/run`** from Desktop (`docs/SPEC.md` §8–9) | §9 **`/whisper`** sent by addon | Bad args; WoW limits |
| Client → Addon | Global `ReceiveGold(dataString)` | semicolon entries: `UUID:CharacterName:GoldCopper;` | Queued mail prep | Parse error; invalid delimiters |
| Client → Addon | **MAIL_SHOW** (event) | (FrameXML) | Side panel + queue UX | Events not fired if wrong hook |
| Client → Addon | Whisper events | sender + text matching **`!twgold`** (case-insensitive) | Print **`[MGM_ACCEPT:UUID]`** to chat | Wrong event registration on 3.3.5a |
| Addon → Client | Chat print **`[MGM_WHO]`** + JSON / `[MGM_ACCEPT:UUID]` / `[MGM_CONFIRM:UUID]` | string | lines in `WoWChatLog.txt` | Wrong tag; mail not sent → no **CONFIRM** |
| Addon → **Chat log** | **`[MGM_WHO]`** + JSON (`docs/SPEC.md` §8) | `/who` parse result | Desktop tail **`WoWChatLog.txt`** → **`POST /api/roulette/verify-candidate`** | Line missing; parse failure |

### WoW Addon → WPF Desktop App → ASP.NET Core API

| Direction | Message/Endpoint | Payload shape | Success response | Failure response |
|-----------|------------------|---------------|------------------|------------------|
| Addon → Desktop | **`[MGM_WHO]`** in **`Logs\WoWChatLog.txt`** (`docs/SPEC.md` §8) | `/who` result for **`verify-candidate`** | Desktop **`POST /api/roulette/verify-candidate`** | Log path wrong; stale **`spinCycleId`** |
| Addon → Desktop | **`[MGM_ACCEPT:UUID]`** in `Logs\WoWChatLog.txt` | addon prints after Lua whisper match | Desktop **confirm-acceptance** | Log path wrong; tag missing |
| Desktop → API | `POST /api/payouts/{id}/confirm-acceptance` | JSON body + ApiKey | `200` | `403`, `404`, validation |
| Desktop → API | `PATCH /api/payouts/{id}/status` `Sent` | after **`[MGM_CONFIRM:UUID]`** in `Logs\WoWChatLog.txt` | `200` | transition errors |
| Desktop | Manual **Mark as Sent** | operator override | same PATCH path | audit note |

---

**Document maintenance:** If **`[MGM_ACCEPT]`** / **`[MGM_CONFIRM]`** log formats change, update regexes in Desktop and **`docs/SPEC.md` §10**. **Pool/spin** routes: `docs/SPEC.md` §5.1 (`GET /api/roulette/state`, `GET /api/pool/me`).

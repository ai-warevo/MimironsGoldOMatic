<!-- Updated: 2026-04-05 (Deduplication pass) -->

# Mimiron's Gold-o-Matic — Technical Specification (MVP)

This document is the **canonical implementation contract** for the MVP.  
`docs/overview/ROADMAP.md` contains step-by-step prompts and links into this spec.  
**User-facing UI:** hub [`docs/reference/UI_SPEC.md`](../reference/UI_SPEC.md) (tokens, navigation); per-surface screens in [`docs/components/twitch-extension/UI_SPEC.md`](../components/twitch-extension/UI_SPEC.md), [`docs/components/desktop/UI_SPEC.md`](../components/desktop/UI_SPEC.md), [`docs/components/wow-addon/UI_SPEC.md`](../components/wow-addon/UI_SPEC.md).

**Code alignment:** MVP slices **MVP-1 … MVP-5** are implemented under `src/` (Shared, Backend, Desktop, Twitch Extension, WoW addon). Remaining gaps (automated tests, packaging, production hardening) are summarized in `docs/reference/IMPLEMENTATION_READINESS.md`.

**Non-normative digests (do not override this file):** [`docs/overview/MVP_PRODUCT_SUMMARY.md`](MVP_PRODUCT_SUMMARY.md), [`docs/reference/GLOSSARY.md`](../reference/GLOSSARY.md), [`docs/reference/WORKFLOWS.md`](../reference/WORKFLOWS.md), [`docs/overview/ARCHITECTURE.md`](ARCHITECTURE.md).

### EBS — Extension Backend Service (normative, MVP)

- **Definition:** **`MimironsGoldOMatic.Backend`** is the **EBS** — the server that implements Extension-facing APIs, **Twitch Extension JWT** validation (**§5.1**, Bearer), and integrations that require broadcaster/Twitch credentials.
- **Helix:** The EBS **owns** Twitch **Helix** API credentials (client id/secret, broadcaster user access where required) used for **`Send Chat Message`** (§11 reward-sent line) and other Helix calls tied to this product.
- **EventSub:** The EBS **hosts** the **Twitch EventSub** subscription lifecycle and **consumes** **`channel.chat.message`** events for **`!twgold <CharacterName>`** enrollment (**§1** glossary, **§5**). Chat messages do not POST directly to the EBS HTTP surface; **EventSub** is the **transport** from Twitch to the EBS.
- **Enrollment — subscriber flag (MVP, locked):** For **`!twgold`** pool enrollment, the EBS **must** trust the **subscriber / badges** data supplied on the **`channel.chat.message`** EventSub notification only. **Do not** perform secondary **Helix** lookups to verify subscription during enrollment (saves API quota; Twitch is authoritative for the event payload).

### Helix §11 reward-sent delivery (MVP, locked)

- **No Outbox table** and **no** dedicated background worker for this path in MVP.
- **Inline try + retry:** Immediately **after** the database transaction that commits payout status **`Sent`**, the EBS **must** call **Helix** **`Send Chat Message`** with the §11 template (**3** attempts, simple retry policy — e.g. Polly or equivalent).
- **Failure handling:** If Helix still fails after **3** retries, **do not** rollback **`Sent`** (in-game gold is already committed; chat is **best-effort**). **Log** the failure at error level for operations.
- **Strictly once per payout:** The Helix announcement **must** run **at most once** per **`PayoutId`** — e.g. boolean **`IsRewardSentAnnouncedToChat`** on the payout read model / entity, or trigger Helix only on transition **into** **`Sent`** from a non-terminal state (idempotent **`PATCH`** must not re-post).

### MVP deployment scope (normative)

- **Single broadcaster** for MVP: one configured Twitch **channel / broadcaster** per EBS deployment. Extension JWT, EventSub, and Helix calls target that channel only. Multi-channel support is **out of scope** for MVP unless added later.
- **Auth (MVP, locked):** **Twitch Dev Rig** and **production** both use **real Twitch-issued Extension JWTs** (Bearer); the **EBS** **must** validate them per Twitch (no long-term “mock JWT” bypass for the first deploy). Pool/roulette/payout-read routes that require JWT (**§5.1**) remain **JWT-only** as specified.
- **Pause / resume distribution:** **not** in MVP — no API or Desktop “pause mode”; streamers stop by not running Desktop or by operator workflow only.

## 1) Glossary

- **Subscription requirement**: the viewer **must be a subscriber** to the broadcaster’s channel (Twitch subs) to use **`!twgold`** chat commands for this product. **MVP (locked):** at enrollment, use the subscriber indication from the **`channel.chat.message`** EventSub payload only — **no** secondary Helix subscriber lookups for enrollment (see **EBS** section above, **§5**).
- **Twitch chat monitoring (normative)**: the system **must** ingest **broadcast** (channel) chat messages on the live stream to detect **enrollment** commands (`!twgold <CharacterName>`) only. **MVP:** use **EventSub** `channel.chat.message` (required). IRC/bot-only paths are **not** part of the MVP contract unless explicitly added later.
- **Enrollment command**: **`!twgold <CharacterName>`** typed in **broadcast Twitch chat**, where **`CharacterName`** is the viewer’s **server / in-game nickname** used for the roulette pool. The **`!twgold`** prefix is **case-insensitive** (e.g. `!TWGold Name` counts); whitespace-separated; **`CharacterName`** must satisfy the same validation rules as **`CharacterName`** elsewhere in this spec.
- **Character uniqueness (pool)**: at any time, each **`CharacterName`** may appear **at most once** among **active** pool entries (no two different viewers holding the same name in the pool simultaneously). A second viewer (or the same viewer) attempting to enroll the same name while it is already taken must receive a clear error (e.g. `character_name_taken_in_pool`).
- **Pool re-enroll / name change (same viewer):** If a **`TwitchUserId`** already has a pool row and sends a new **`!twgold <OtherName>`**, **replace** that row’s **`CharacterName`** with the new name (release the old name for other viewers), subject to validation and uniqueness of **OtherName**.
- **Participant pool**: the set of **subscribers** who have successfully enrolled via **`!twgold <CharacterName>`** and not been removed per pool rules. **Non-winners of a spin remain in the pool.** **Winners are removed from the pool when their payout becomes `Sent`** (gold mail confirmed — see §3, §5); they **may re-enter** the pool by sending **`!twgold <CharacterName>`** again in broadcast chat.
- **Spin / roulette**: a **scheduled** selection every **5 minutes** that picks **one** winner from the current participant pool using a **visual roulette** (viewer-facing, e.g. in the Twitch Extension overlay). There is **no** early or off-schedule spin. The **next spin time** is **server-authoritative** (see **`GET /api/roulette/state`**); the Extension **must** display the **roulette countdown** using API-provided timestamps (not a free-running client-only clock as the source of truth).
- **Spin candidate selection (MVP, locked):** When a spin runs, the Backend picks **one** **candidate** from **active** pool rows using **uniform random** selection (each eligible row **equal** probability). Implementation may use **`System.Random`** or another suitable RNG; document the choice in Backend code/readme.
- **Online verification (`/who`)**: the roulette **must** ensure the **candidate winner** is **actually in-game and online** by running **`/who <Winner_InGame_Nickname>`** where the name matches the pool **`CharacterName`**. **MVP normative split:** the **addon** runs **`/who`**, **parses** the **3.3.5a** result, and emits a **`[MGM_WHO]`** log line (§8, §10) via **`DEFAULT_CHAT_FRAME:AddMessage`** (or equivalent) so it appears in **`Logs\WoWChatLog.txt`** — **WoW 3.3.5a addons cannot write arbitrary files**; there is **no** JSON file-bridge. **Desktop** **tails** the same log, parses **`[MGM_WHO]`**, and **POST**s the payload to **`POST /api/roulette/verify-candidate`**. The **Backend** is **authoritative**: it **only** creates a **`Pending`** payout when it accepts an **`online: true`** report for the current spin cycle (subject to **§5** grace window). **No re-draw** in the same **5-minute** cycle: if the candidate is **offline** (or a single-person pool is offline), **no winner** is produced until the **next** scheduled spin.
- **Winner notification (Twitch Extension)**: the Extension **may** show **“You won”** (see §11); **normative** winner contact is **in-game** (§9).
- **Winner notification whisper (WoW, normative)**: After a **`Pending`** payout exists for the winner, the **addon** **must** cause the client to send an in-game **whisper** to the winner’s character (**`<Winner_InGame_Nickname>`** = enrolled **`CharacterName`**) using this **exact** chat command text (single line; if the line exceeds client limits, use an addon-defined strategy that preserves the **exact** Russian body text):

  `/whisper <Winner_InGame_Nickname> Поздравляю, ты победил в розыгрыше! Дай мне своё согласие на получение награды - ответь на это сообщение одной фразой: !twgold`

  **Implementation note:** If the client uses `/w` instead of `/whisper`, document equivalent behavior; **do not** change the Russian body text without a product decision.
- **Spin interval**: **5 minutes** between automatic spins (fixed cadence; no skip-ahead). **`nextSpinAt`** is aligned to **UTC** wall-clock boundaries **:00, :05, :10, …** (multiples of 5 minutes).
- **Minimum participants**: **1** — a spin may run when exactly one person is in the pool.
- **Payout**: a backend record representing the intention to mail gold to an in-game character, **created for the current spin winner** (not at chat enrollment).
- **Active payout**: a payout in `Pending` or `InProgress`.
- **Terminal payout**: a payout in `Sent`, `Failed`, `Cancelled`, or `Expired`.
- **Acceptance to receive gold (WoW, normative)**: After receiving the **winner notification whisper** (above), the winner **must** send a **private in-game message** (whisper reply) whose text matches **`!twgold`** with **case-insensitive** comparison after trim (**no** other words or punctuation). The **addon** **must** detect this in **Lua** whisper events, then **print `[MGM_ACCEPT:UUID]`** to WoW chat so it appears in **`Logs\WoWChatLog.txt`**; the **Desktop** utility **must** tail that file and call **`POST .../confirm-acceptance`** (see §9–10). This is **not** proof that mail was sent (see **`[MGM_CONFIRM:UUID]`**). *Non-normative:* Twitch broadcast chat **`!twgold`** (no args) as an alternate acceptance path is **not** part of the MVP contract unless explicitly added later.
- **Acceptance tag (`[MGM_ACCEPT:UUID]`)**: **Addon-emitted** line printed to WoW chat after a valid whisper **`!twgold`** is observed; **`UUID`** is the payout id. Used so **Desktop** can automate **`confirm-acceptance`** via the **same** `WoWChatLog.txt` watcher as **`[MGM_CONFIRM:UUID]`** (different regex). **Not** the same as parsing the user’s whisper text from the log — the addon **owns** the tag content.
- **Mail-send confirmation (`[MGM_CONFIRM:UUID]`)**: after an **MGM-armed** in-game mail send succeeds (**`MAIL_SEND_SUCCESS`**, §9), the addon **must** print **`[MGM_CONFIRM:UUID]`** to WoW chat so it appears in **`Logs\WoWChatLog.txt`**, then whisper the winner the **mail-completion** Russian line (§9). **Manual** non-MGM mail sends **must not** emit this tag. Desktop **must** parse **`[MGM_CONFIRM:UUID]`** (required); **`Sent`** on the server is driven by this signal (see §10). **`Sent`** also **removes** the winner from the **participant pool** (see §5).

## 2) MVP economics & anti-abuse rules

- **GoldAmount**: fixed at **1,000g** per winning payout (enforce per `TwitchUserId` / enrollment idempotency as in §4).
- **Lifetime cap**: max **10,000g total** per `TwitchUserId`.
- **Concurrency**: only **one active payout** per `TwitchUserId` at a time (same as before; applies once a viewer becomes a spin winner and a payout record exists). A second win/spin finalize while a non-terminal payout exists **must** be rejected with **`active_payout_exists`** (or equivalent); **do not** auto-expire or replace an existing active payout to make room.
- **Rate limiting**: ASP.NET Core **partitioned** fixed-window limiter — **5 requests / minute** per authenticated Extension user id (JWT `user_id` claim) or client IP; **`POST /api/twitch/eventsub`** is **not** rate-limited (separate partition with no limiter).

## 3) Statuses & lifecycle transitions

### Status enum (MVP)

- `Pending`: created for the **selected winner** after a spin, not yet synced/injected by Desktop.
- `InProgress`: explicitly claimed by Desktop when streamer clicks **Sync/Inject** (prepares mail / queue). Desktop **must** only perform this transition when the **WoW client target is detected** (MVP: foreground `WoW.exe` per **Desktop → WoW injection**, §8); **do not** move to **`InProgress`** if WoW is not found.
- `Sent`: confirmed on the **server** when the Desktop utility observes **`[MGM_CONFIRM:UUID]`** for that payout id in **`Logs\WoWChatLog.txt`** (required mail-send confirmation). **WoW whisper reply** **`!twgold`** (case-insensitive) records **willingness to accept** earlier in the flow and **does not** replace **`[MGM_CONFIRM:UUID]`**. Transitioning to **`Sent`** **removes** that winner from the **participant pool** (they may re-enroll via **`!twgold <CharacterName>`** in Twitch chat).
- `Failed`: streamer/Desktop marked failure (e.g., faction restriction, injection failure, etc.).
- `Cancelled`: streamer cancelled in Desktop.
- `Expired`: auto-closed by backend when older than 24 hours (terminal).

### Allowed transitions (normative)

| From | To | Who/when |
|---|---|---|
| `Pending` | `InProgress` | Desktop on **Sync/Inject**, **only after** WoW target is detected (§8 Desktop → WoW injection) |
| `Pending` | `Cancelled` | Desktop (streamer) |
| `Pending` | `Failed` | Desktop (streamer) |
| `InProgress` | `Sent` | **Desktop** observes **`[MGM_CONFIRM:UUID]`** in **`Logs\WoWChatLog.txt`** and calls Backend (`PATCH` status or dedicated endpoint); or **manual Mark as Sent** if policy allows |
| `InProgress` | `Cancelled` | Desktop (streamer) |
| `InProgress` | `Failed` | Desktop (streamer) |
| `InProgress` | `Pending` | Desktop (**escape hatch** — e.g. unlock queue after failed inject; streamer policy) |
| `Pending`/`InProgress` | `Expired` | Backend hourly expiration job |

## 4) Identity, idempotency, and DTOs

### Identity

- **Enforcement key**: `TwitchUserId` (from Twitch identity; numeric string is acceptable for storage).
- **Display-only**: `TwitchDisplayName` (for Desktop / overlay UX).
- **Recipient**: `CharacterName` (single realm assumption; faction failures handled manually by streamer).

### `CharacterName` validation (MVP, normative)

Shared rules for chat enrollment, **`POST /api/payouts/claim`**, and server-side checks:

- **Length:** **2–12** characters (inclusive).
- **Allowed characters:** **Unicode letters** in **Latin** or **Cyrillic** scripts only (no digits, no punctuation, **no spaces**).
- **Normalization:** trim surrounding whitespace before validation; reject if empty after trim.

Implement with **FluentValidation** in **`MimironsGoldOMatic.Shared`** (same rules Backend + Desktop).

### Idempotency

- **Chat-enrolled pool entries:** dedupe **retries** using the Twitch **`message_id`** (or equivalent) for the same **`!twgold <CharacterName>`** message; ignore duplicate chat deliveries.
- **Optional Extension/API path:** `EnrollmentRequestId` is a unique identifier for a single **`POST /api/payouts/claim`** (client-generated UUID recommended) when that path is used.
- The **EBS** MUST enforce **pool uniqueness** on **`CharacterName`** among active pool rows (see glossary).

**MVP behavior on duplicate `EnrollmentRequestId` (Extension only):**

- Return the existing **enrollment** record as an idempotent success (no duplicate pool entry or payout).

## 5) API Contract (MVP)

This section defines the MVP endpoints and semantics. **`GET /api/roulette/state`** and **`GET /api/pool/me`** field lists in **§5.1** are **normative** for MVP. Other illustrative JSON bodies remain **guidance** until OpenAPI/schemas are locked; behavior is normative.

### Common headers

- **Desktop ApiKey**: `X-MGM-ApiKey: <value>`
  - Required for Desktop endpoints (pool sync, status updates, spin triggers if server-authoritative, whisper-forward).
  - The **EBS** stores the key in configuration (global static key for MVP).

### Twitch EventSub webhook — chat enrollment (MVP, implemented)

- **Route:** `POST /api/twitch/eventsub`
- **Auth:** **AllowAnonymous** at the ASP.NET layer. **Signature verification** uses `Twitch-Eventsub-Message-Id`, `Twitch-Eventsub-Message-Timestamp`, and `Twitch-Eventsub-Message-Signature` with HMAC-SHA256 when **`Twitch:EventSubSecret`** is configured; if the secret is **empty**, verification is **skipped** (convenient for local tunnel testing — **do not** use an empty secret in production).
- **Behavior:** Responds to EventSub **`challenge`** with plain-text body for subscription verification. For **`channel.chat.message`** notifications, the EBS parses the payload and calls the same enrollment rules as chat (**`!twgold <CharacterName>`**, subscriber badges from payload: `subscriber`, `founder`, `premium`), with **`message_id`** deduplication.
- **Rate limiting:** This path is **exempt** from the global fixed-window limiter in `Program.cs` so Twitch deliveries are not throttled as viewer traffic.

### Development configuration — Extension `POST /api/payouts/claim` vs chat enrollment

- **Chat (`EventSub`):** Subscriber gating uses **only** the EventSub payload (**§1**, **EBS** section). There is **no** `DevSkipSubscriberCheck` branch on this path.
- **Extension claim (`POST /api/payouts/claim`):** The handler enforces **`Mgm:DevSkipSubscriberCheck`**. When **`false`** (default), the API returns **`403`** with **`not_subscriber`** — Helix-based subscriber verification for this path is **not** implemented yet; set **`DevSkipSubscriberCheck`** to **`true`** in **Development** only to exercise the claim API (e.g. Dev Rig). **Product intent:** eventual Helix verification for claim should match chat enrollment rules.

### Participant pool & roulette (normative behavior)

- **Enrollment (primary):** When EventSub indicates a **subscriber** and they send **`!twgold <CharacterName>`** in **broadcast Twitch chat** (prefix **case-insensitive**), and **`CharacterName`** is valid and **not already held** by another active pool entry, **add** or **update** that viewer’s pool row (**§1** glossary: same **`TwitchUserId`** may **replace** their previous **`CharacterName`**). **No payout** is created at this step. **MVP:** subscriber eligibility comes from the **`channel.chat.message`** EventSub payload only (no Helix lookup). If they are **not** a subscriber per that payload: **do not** add to pool; **log server-side only** (no chat bot reply required). If the name is taken by **another** viewer, reject (`character_name_taken_in_pool` or equivalent).
- **Winner contact & acceptance (after win, WoW):** After **`Pending`** payout and **winner notification whisper** (§9), the winner **must** reply in-game with **`!twgold`** (case-insensitive; §9). The **addon** forwards to Desktop → **`POST .../confirm-acceptance`**. The streamer **should** send in-game mail only after acceptance is recorded (§9).
- **Optional Extension enrollment:** A **`POST /api/payouts/claim`** (§5) may still add a subscriber to the pool for Dev Rig / Extension-only flows; behavior must match chat enrollment rules (subscription + unique **`CharacterName`**).
- On each **spin** (scheduled every **5 minutes** only), the system selects **one candidate** from the pool (**uniform random** among active pool rows — see glossary). **Losers of that spin (non-winners) stay in the pool.**
- **Winner removal:** When a winner’s payout transitions to **`Sent`**, **remove** that participant (**`TwitchUserId`** + **`CharacterName`**) from the pool. They **may** join again with a new **`!twgold <CharacterName>`** message in chat.
- **Minimum pool size**: **1** (spin still runs).
- **Online check (required):** For each spin cycle, the Backend selects a **candidate** from the pool, then requires an **`online: true`** **`/who`** report delivered via **`[MGM_WHO]`** in **`Logs\WoWChatLog.txt`** → Desktop **tail** → **`POST /api/roulette/verify-candidate`** (**§8**, **§10**). If the report is **`online: false`** (or missing before the cycle boundary + grace window), **no `Pending` payout** is created for that cycle — **no re-draw** in the same **5-minute** window. If the pool had **exactly one** participant and they are offline, **no winner** this cycle (same rule).

- After the Backend **accepts** an **`online: true`** report for the candidate, it **creates** a payout record in **`Pending`** for that winner’s **`CharacterName`**.

**Implementation note:** **Chat ingestion** for **enrollment** uses **EventSub** `channel.chat.message` (see glossary); document reconnect/backfill policy. **Acceptance** is **WoW whisper `!twgold`** (§9). **Pool + spin schedule** for the Extension are normative in **§5.1**. **`/who`** run/parse is **addon**; **report transport** is **chat log line `DEFAULT_CHAT_FRAME:AddMessage`** → **`WoWChatLog.txt`** → Desktop (**§8**, **§10**).

### Error model (MVP)

Recommended JSON error shape (server should be consistent):

```json
{
  "code": "active_payout_exists",
  "message": "User already has an active payout.",
  "details": {}
}
```

**HTTP overload:** Under load or upstream saturation, the API **may** return **`503 Service Unavailable`** (hosting-dependent). Twitch Extension clients **should** treat **`503`** like **`429`** and apply **§5.1** (backoff + Retry). **Global** queue depth is **not** normative beyond per-identity rate limits.

Recommended `code` values (MVP):

- `duplicate_enrollment`
- `active_payout_exists`
- `lifetime_cap_reached`
- `invalid_character_name`
- `unauthorized`
- `forbidden_apikey`
- `terminal_status_change_not_allowed`
- `not_found`
- `pool_empty` (if a spin is requested with zero participants — should not occur if minimum is 1 and spin is only scheduled when valid)
- `character_name_taken_in_pool` (enrollment command uses a name already active in the pool)
- `not_subscriber` (chat or API enrollment from non-subscriber)

### Twitch chat commands (normative product contract)

| Chat message (broadcast) | When | Effect |
|--------------------------|------|--------|
| `!twgold <CharacterName>` (prefix **case-insensitive**) | Subscriber; name available | Add/update pool entry for this viewer with **`CharacterName`** |
| `!twgold` with **no** `<CharacterName>` (wrong arity) | — | **Ignore** (no pool change; no error required) |
| Other `!twgold ...` variants | — | **Ignore** unless explicitly supported later |

**Acceptance** is **not** via Twitch chat in MVP; see **WoW** table in §9.

**Parsing:** treat the **`!twgold`** enrollment prefix as **case-insensitive**; compare normalized message start after trim.

### POST `/api/payouts/claim` (optional; Extension / Dev Rig)

**Purpose**: optional path to **add a subscriber to the participant pool** when not using chat-only enrollment (same rules as **`!twgold <CharacterName>`**).

**Auth:** Twitch Extension JWT (**Bearer**), same scheme as **`GET /api/roulette/state`**.

**Request** (normative shape; camelCase JSON):

```json
{
  "characterName": "Somecharacter",
  "enrollmentRequestId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Behavior** (as implemented):

- **Subscriber check:** When **`Mgm:DevSkipSubscriberCheck`** is **`false`**, the EBS returns **`403`** with **`not_subscriber`** (Helix verification for this path not wired yet). When **`true`** (**local dev only**), the check is skipped so Dev Rig / tests can call the endpoint.
- Validate `characterName` (**FluentValidation** / **`CharacterNameRules`**) and **pool uniqueness** of the name; enforce **one active payout** per user, **lifetime cap**, and **`EnrollmentRequestId`** idempotency (same as **§4**).
- **Do not** create a `Pending` payout from this call; only **spin winner** yields a payout row.

**Response** (`201` new enrollment, `200` idempotent replay for same user + same **`enrollmentRequestId`**):

```json
{
  "characterName": "Somecharacter",
  "enrollmentRequestId": "550e8400-e29b-41d4-a716-446655440000"
}
```

(Error responses use **`ApiErrorDto`**: `code`, `message`, `details` — see **§5** error model.)

### GET `/api/payouts/my-last`

**Purpose**: pull model for Twitch Extension; return the caller’s latest **winner payout** as **`PayoutDto`**.

**Auth:** Twitch Extension JWT (Bearer) — same as **`GET /api/roulette/state`** / **`GET /api/pool/me`**.

**Response:**

- **`200 OK`** with a **`PayoutDto`**-shaped JSON body when a winner payout exists for **`TwitchUserId`**.
- **`404 Not Found`** when the viewer has **no** winner payout row yet (strict “payout-only” contract; pool-only membership is **not** returned here — use **`GET /api/pool/me`**).

### GET `/api/payouts/pending` (Desktop)

**Purpose**: Desktop fetches payouts available for syncing/injection (**winner** payouts primarily `Pending`).

### PATCH `/api/payouts/{id}/status` (Desktop)

**Purpose**: Desktop updates lifecycle state where allowed (see §3), including the **`InProgress` → `Pending`** escape hatch (streamer unlock after failed inject).

**Request body** (camelCase): `{ "status": "Pending" | "InProgress" | "Sent" | "Failed" | "Cancelled" | "Expired" }` (string enum; must be an allowed transition).

**Response:** **`200 OK`** with **`PayoutDto`** JSON (same fields as **Shared** / **`GET /api/payouts/my-last`**: `id`, `twitchUserId`, `twitchDisplayName`, `characterName`, `goldAmount`, `enrollmentRequestId`, `status`, `createdAt`, `isRewardSentAnnouncedToChat`).

### POST `/api/payouts/{id}/confirm-acceptance` (Desktop) — **recommended**

**Purpose**: Record that the winner **confirmed willingness to accept** gold when the **addon** observed an in-game **private message** to the streamer with body matching **`!twgold`** (**case-insensitive** after trim; reply to the winner notification whisper; see §9) — not that mail was sent. **MVP:** only **Desktop** calls this endpoint, after **`[MGM_ACCEPT:UUID]`** in the log (**§10**); Twitch chat ingestion **does not** drive acceptance.

**Trigger (MVP, normative):** Desktop **must** call this after observing **`[MGM_ACCEPT:UUID]`** for that **`{id}`** in **`Logs\WoWChatLog.txt`** (see §10). The **`{id}`** in the URL **must** match the UUID in the tag.

**Request** (normative fields):

```json
{
  "characterName": "Somecharacter"
}
```

- **`characterName`** is **required** (must match the payout’s winner character for validation).

**Rules**:

- Guarded by `X-MGM-ApiKey`.
- Backend records acceptance (e.g. `WinnerAcceptedWillingToReceiveAt`); **does not** set **`Sent`**.
- **Idempotency:** repeating the same **`[MGM_ACCEPT:UUID]`** line (log replay) **must** not create inconsistent state; return success if already accepted.
- **Product rule:** the streamer should send in-game mail **only after** this acceptance is recorded (enforcement may be UX + optional API guards).

### Mail-send confirmation → `Sent` (Desktop)

**Purpose**: Desktop **must** tail **`Logs\WoWChatLog.txt`** and detect **`[MGM_CONFIRM:UUID]`** (see §10). On match, call **`PATCH /api/payouts/{id}/status`** with **`Sent`** (or a dedicated confirm-mail-sent endpoint).

**Rules**:

- Guarded by `X-MGM-ApiKey`.
- **`[MGM_CONFIRM:UUID]`** is **required** for automated **`Sent`**; it proves the addon reported **mail was sent**.

### POST `/api/roulette/verify-candidate` (Desktop)

**Purpose:** Submit the **parsed `/who` result** for the current spin’s **candidate** so the **Backend** can **authoritatively** create (or not create) a **`Pending`** payout.

**Auth:** `X-MGM-ApiKey` (Desktop).

**Request** (normative): same JSON object as emitted by the addon after the **`[MGM_WHO]`** prefix on the log line (**§8**; may include **`schemaVersion`**; Backend **must** accept **`schemaVersion`: 1**).

```json
{
  "schemaVersion": 1,
  "spinCycleId": "550e8400-e29b-41d4-a716-446655440000",
  "characterName": "Norinn",
  "online": true,
  "capturedAt": "2026-04-04T12:00:01.000Z"
}
```

**Behavior (normative):**

- If **`online`** is **`true`** and **`spinCycleId`** matches the active spin cycle and **`characterName`** matches the server-selected candidate, the Backend **creates** the **`Pending`** payout for that viewer.
- If **`online`** is **`false`**, the Backend **does not** create a payout; **no re-draw** occurs in the same cycle (**§1** glossary).
- If the payload is **invalid** or **out of sequence**, return **`400`** with a stable error `code`.
- **Late arrival (locked):** The grace window is anchored to the **UTC** `:00/:05/:10/…` **spin boundary** that **closes** eligibility for **`verify-candidate`** for the active **`spinCycleId`** (the instant the cycle stops waiting for `/who` / Desktop). If the **`POST`** arrives **within 30 seconds after** that boundary instant, the Backend **may** still accept a valid report (tolerance for client/log delay). Beyond that window, treat as **out of sequence** (**`400`**) unless the Backend defines a broader policy in a later revision.

**Log alignment:** Desktop **must** build this JSON **from** the **`[MGM_WHO]`** line in **`WoWChatLog.txt`** (**§8**, **§10**) and POST it; the body **must** match what the addon printed (same fields).

### Minimum pool & roulette HTTP contract (MVP, normative)

These routes supply **server-authoritative** spin scheduling and pool hints for the Twitch Extension. The Extension **must** drive the **visual roulette countdown** from **`nextSpinAt`** / **`serverNow`** (poll or SSE/WebSocket later — polling is fine for MVP).

#### GET `/api/roulette/state`

**Purpose:** Return **canonical** schedule and pool size for the **configured broadcaster channel** (MVP: **single** channel per deployment).

**Auth:** **Twitch Extension JWT (Bearer) only** — no public unauthenticated access in MVP.

**Response** (normative fields):

```json
{
  "nextSpinAt": "2026-04-04T12:05:00.000Z",
  "serverNow": "2026-04-04T12:01:23.456Z",
  "spinIntervalSeconds": 300,
  "poolParticipantCount": 12,
  "spinPhase": "idle",
  "currentSpinCycleId": "550e8400-e29b-41d4-a716-446655440000"
}
```

- **`nextSpinAt`**: ISO-8601 UTC instant of the **next** scheduled spin boundary (**`:00`, `:05`, `:10`, … UTC** — multiples of 5 minutes; single source of truth for countdown UI).
- **`serverNow`**: ISO-8601 UTC “now” on the server (helps correct client drift when computing remaining time).
- **`spinIntervalSeconds`**: **300** in MVP.
- **`poolParticipantCount`**: non-negative integer; number of **active** pool entries for the current channel.
- **`spinPhase`**: **closed enum** for MVP — exactly one of: **`idle`**, **`collecting`**, **`spinning`**, **`verification`**, **`completed`**. **Transitions** between phases for a cycle are **Backend-defined** (implementation detail), as long as responses remain consistent with this contract and **`docs/components/twitch-extension/UI_SPEC.md`** UX.
- **`currentSpinCycleId`**: UUID string for the **active** spin cycle (omit or `null` when **`spinPhase`** is **`idle`**); used to correlate **`POST /api/roulette/verify-candidate`** and **`[MGM_WHO]`** log payloads.

#### GET `/api/pool/me`

**Purpose:** Tell the **authenticated viewer** whether they are in the pool and under which **`CharacterName`**.

**Auth:** **Twitch Extension JWT (Bearer) only**; identifies **`TwitchUserId`**.

**Response**:

```json
{
  "isEnrolled": true,
  "characterName": "Norinn"
}
```

- When not enrolled: **`isEnrolled`** = `false` and **`characterName`** may be `null` or omitted.

**Rules:**

- **`nextSpinAt` / `serverNow`** are the **only** authoritative schedule for “time until next spin”; the Extension **must still display** the roulette timer/countdown in the UI (see §11).
- Early/off-schedule spins remain **forbidden** (glossary).

#### Extension resilience (overload / errors)

- On **`429`**, **`503`**, or network failure when polling **`GET /api/roulette/state`**, **`GET /api/pool/me`**, or **`GET /api/payouts/my-last`**, the Extension **should** show a **friendly error** (see **`docs/components/twitch-extension/UI_SPEC.md`**) and **exponential backoff** between retries (cap the maximum interval, e.g. **≤ 60s**), plus a **Retry** control so the viewer is not stuck in a tight loop. **`503`** indicates temporary overload or dependency failure; behavior is **host-defined** beyond this client guidance (**§5** error model).

## 6) Persistence model (MVP, ES-first)

For MVP, the source of truth is Event Sourcing:

- Event Store is implemented with **Marten** on PostgreSQL.
- **Stream identity (MVP):** use **one Marten event stream per payout id** (payout id = stream id) for payout lifecycle events.
- **Aggregates (MVP):** model **Pool** and **Payout** as **separate** domain aggregates; orchestration (spin selects candidate → verify → create payout) lives in the **application** layer — **do not** merge pool and payout into a single god-aggregate.
- Roulette / pool events and payout events are written as append-only domain events on their respective streams.
- Read queries are served from read-model projections.
- EF Core MAY be used for read-model tables only, not as the write-side source of truth.

**Outbox:** **Do not** create an **Outbox** table in MVP. **Helix** **`Send Chat Message`** for §11 is **inline try + retry** after the **`Sent`** commit (see **EBS** — Helix §11 reward-sent delivery); **not** Outbox-driven. A future **Outbox** (e.g. Discord or other channels) may be added **post-MVP** when a second external side-effect class is integrated; follow **Outbox** as in `docs/components/backend/ReadME.md` (same transaction as domain events + background dispatcher) **only** when explicitly added to the roadmap.

Minimum recommended fields for payout read model (`PayoutsReadModel`):

- `Id` (UUID, PK)
- `TwitchUserId` (string/varchar; indexed)
- `TwitchDisplayName` (string/varchar)
- `CharacterName` (string/varchar)
- `GoldAmount` (bigint/int64; always 1000 in MVP)
- `EnrollmentRequestId` (string/varchar; **UNIQUE** where applicable for pool enrollment correlation)
- `Status` (enum/string; indexed)
- `CreatedAt` (timestamp; indexed with status for expiration sweep)
- (Optional but recommended) `UpdatedAt` (timestamp)
- (Recommended) `WinnerAcceptedWillingToReceiveAt` (timestamp nullable): set when Desktop reports **`!twgold`** for this payout (acceptance to receive gold).
- **`IsRewardSentAnnouncedToChat`** (boolean): set **`true`** only after **Helix** **`Send Chat Message`** **succeeds** for §11; while **`false`**, transitioning to **`Sent`** may trigger the inline retry loop; once **`true`**, **do not** call Helix again for that **`PayoutId`** (idempotent **`PATCH`**). If all **3** attempts fail, leave **`false`**, **log**, and **do not** rollback **`Sent`** (see **EBS** — Helix §11 reward-sent delivery).

Additional read models (pool membership, spin schedule, last spin id) are **required** by the roulette feature; define in implementation.

## 7) Expiration job (MVP)

- Runs **hourly**.
- Transitions payouts in `Pending` or `InProgress` to `Expired` when `CreatedAt < now - 24h`.
- `Expired` is terminal and MUST NOT be reactivated.

## 8) Desktop → WoW injection specification (MVP)

### Target process

- Desktop targets the **foreground** `WoW.exe` process in MVP.

### Command format

Desktop injects `/run` commands that invoke addon entrypoints:

- `/run ReceiveGold("<payload>")` — mail-queue payload (**winner** payouts for **`InProgress`** mail flow).
- `/run NotifyWinnerWhisper("<payoutId>","<characterName>")` — **after** the Backend creates **`Pending`** for that winner, Desktop **must** inject this line so the **addon** sends the §9 **winner notification whisper** (addon issues **`/whisper`**; Desktop does **not** paste the Russian text). **`payoutId`** is the payout UUID; **`characterName`** matches pool **`CharacterName`**. Respect **§8** `<255` chars per injected line (this call is short).

### Injection strategy (MVP)

- Primary strategy: `PostMessage`.
- Fallback strategy: `SendInput` (operator-switchable in Desktop settings) when primary injection is blocked/unreliable.

### Payload chunking rule (<255 chars)

WoW chat command input has a practical limit (commonly ~255 chars). For MVP:

- Desktop MUST split injections so that **each injected command line** is **< 255 characters**.
- Chunk by **whole payout entries** (never split mid-entry).
- Each chunk results in **one** `ReceiveGold(...)` call.

### Recommended approach (MVP)

- Build a list of payout entries for injection.
- Pack as many complete entries into one payload chunk as possible while keeping the full command line under 255 chars.
- Send multiple `/run ReceiveGold("...")` lines if needed.

### Roulette `/who` verification & unified log bridge (MVP, normative)

WoW **3.3.5a** addon Lua **cannot** write arbitrary files to disk for Desktop consumption. **All** addon → Desktop signaling for **`/who`** results **and** payout acceptance/confirmation uses **one** channel: lines that appear in **`Logs\WoWChatLog.txt`** (default path below).

- The **addon** **runs** **`/who <CharacterName>`** in the client, **parses** online/offline for **3.3.5a**, then emits **one** line to the **default chat frame** via **`DEFAULT_CHAT_FRAME:AddMessage`** (or equivalent) so the client records it in **`WoWChatLog.txt`**.
- **Line format (normative):** the literal prefix **`[MGM_WHO]`** immediately followed by a **single JSON object** (UTF-8) on the **same** line — no newlines inside the object. The JSON fields **must** match **`POST /api/roulette/verify-candidate`** (**§5**). **Plain language:** “JSON” here means **structured text** in braces `{ … }` with **named fields** (`"schemaVersion"`, `"spinCycleId"`, etc.) and values; it must be **one continuous line** in **`WoWChatLog.txt`**. Extra spaces are optional; **compact** output (see example below) is recommended.

```json
{
  "schemaVersion": 1,
  "spinCycleId": "550e8400-e29b-41d4-a716-446655440000",
  "characterName": "Norinn",
  "online": true,
  "capturedAt": "2026-04-04T12:00:01.000Z"
}
```

Example log line (single line):

`[MGM_WHO]{"schemaVersion":1,"spinCycleId":"550e8400-e29b-41d4-a716-446655440000","characterName":"Norinn","online":true,"capturedAt":"2026-04-04T12:00:01.000Z"}`

- **`schemaVersion`:** **1** for MVP.
- **`spinCycleId`:** must match **`currentSpinCycleId`** from **`GET /api/roulette/state`** for the active cycle.
- **`capturedAt`:** ISO-8601 UTC when the addon determined the result.
- **Desktop** **tails** **`Logs\WoWChatLog.txt`**, parses **`[MGM_WHO]`** lines, and **POST**s the JSON to **`POST /api/roulette/verify-candidate`** with **`X-MGM-ApiKey`**. The **Backend** is **authoritative** for creating **`Pending`** (see **§5**).

**WoW `Logs\WoWChatLog.txt` path (Desktop):** **Default** `Logs\WoWChatLog.txt` relative to the configured **WoW install directory**; **full path override** in Desktop settings (**§10**). **No separate file-bridge path** — only this log file for addon-originated signals in MVP.

## 9) Addon: winner whisper, consent reply `!twgold`, mail queue, and mail-send tag (MVP)

`ReceiveGold(dataString)` accepts a semicolon-delimited list of payout entries:

- Entry format: `UUID:CharacterName:GoldCopper;`
  - `UUID`: payout id
  - `CharacterName`: WoW character name (MVP validation should prevent `:` and `;`)
  - `GoldCopper`: integer copper amount (MVP: 1000g = 10000000 copper)

Example:

```
2d2b7b2a-1111-2222-3333-444444444444:Somecharacter:10000000;
```

### Winner notification whisper (normative)

**Trigger (MVP, locked):** The **addon** is the only component that types the §9 **`/whisper …`** into WoW. The **Desktop** app **does not** inject the Russian whisper text directly. After the Backend creates **`Pending`** for the winner, the **Desktop** **must** call **`/run NotifyWinnerWhisper("<payoutId>","<characterName>")`** (see **§8**) so the **addon** runs its Lua handler and **then** executes the whisper line below. Order relative to **`ReceiveGold`**: **notify whisper first** (this section), **then** mail prep/inject when the streamer syncs (**`ReceiveGold`**).

When invoked for a **`Pending`** payout, the **addon** **must** send the following **exact** command as a single client chat line. If the line exceeds client limits, use an addon-defined strategy that preserves the **exact** Russian body text:

```
/whisper <Winner_InGame_Nickname> Поздравляю, ты победил в розыгрыше! Дай мне своё согласие на получение награды - ответь на это сообщение одной фразой: !twgold
```

- **`<Winner_InGame_Nickname>`** is the enrolled **`CharacterName`** for that payout.
- The streamer’s character sends this whisper **to** the winner; the winner sees the Russian instructions and must reply with **`!twgold`** only (comparison **case-insensitive** after trim).

### Whisper reply interception — acceptance (normative)

- Register for **whisper** / private-message events (exact WoW 3.3.5a event/API as appropriate for the client).
- When the **sender** is the **expected winner character** (matches **`CharacterName`** for the active **`Pending`** payout) and the **message text** matches **`!twgold`** (**case-insensitive** after trim), the addon **must** print **`[MGM_ACCEPT:UUID]`** to **WoW chat** (same channel/window behavior you use for **`[MGM_CONFIRM:UUID]`** so it is captured in **`Logs\WoWChatLog.txt`**). **`UUID`** is the payout id.
- **Why not named pipes / sockets / files:** WoW **3.3.5a** addon Lua (FrameXML) does **not** provide a reliable, supported way to talk to an external Desktop process **except** via in-game outputs the client records (here: **chat print → `WoWChatLog.txt`**). **HTTP from Lua** is **not** used.

**Product rules:**
- Gold should only be mailed after the winner has **received** the notification whisper and **replied** with **`!twgold`** (consent).
- The **`!twgold`** reply **does not** mean mail was sent; **`Sent`** still requires **`[MGM_CONFIRM:UUID]`** in the WoW log.
- **Desktop** tails **`Logs\WoWChatLog.txt`** and calls **`POST .../confirm-acceptance`** when **`[MGM_ACCEPT:UUID]`** matches (§5, §10).

### Mail-send detection (normative; MGM-tracked sends only)

**Event:** WoW **3.3.5a** fires **`MAIL_SEND_SUCCESS`** when the client successfully submits an outgoing mail. **`MAIL_FAILED`** fires when the send does not complete.

**MGM vs manual mail:** The addon **must** treat **`MAIL_SEND_SUCCESS`** as the trigger for the steps below **only** when the send was **armed** by the **MGM mail queue** flow (e.g. after **`ReceiveGold`** / **Prepare Mail** from this addon). If the streamer composes and sends mail **manually** in the default mailbox UI **without** going through that armed path, the addon **must not** emit **`[MGM_CONFIRM:UUID]`**, **must not** send the **mail-completion whisper** (below), and **must not** clear or advance MGM payout state for that send.

**Arming (implementation guidance):** Set an internal **pending-send** context (payout id + winner **`CharacterName`**) immediately before the **`SendMail(...)`** call that corresponds to an MGM-prepared send (e.g. **secure hook** on **`SendMail`** / **`SendMailFrame_SendMail`** when the recipient matches the armed payout). Clear the armed state on **`MAIL_FAILED`**, **mailbox close**, or successful handling of **`MAIL_SEND_SUCCESS`**.

### Mail-send tag (normative; required for automated `Sent`)

When the addon handles **`MAIL_SEND_SUCCESS`** for an **MGM-armed** send, it **must**:

1. Print to the **default chat frame** (so it appears in **`Logs\WoWChatLog.txt`**):

   - `[MGM_CONFIRM:UUID]`

   where `UUID` is the payout id.

2. **Then** whisper the **winner** (same **`CharacterName`** as the mailed recipient) the **exact** in-game text (single whisper; hardcoded in addon):

   `Награда отправлена тебе на почту, проверяй ящик!`

   Use **`SendChatMessage`** with **`"WHISPER"`** (or equivalent 3.3.5a API) so only the winner sees this line. This is **separate** from **`[MGM_CONFIRM:UUID]`** (which Desktop uses for **`Sent`**).

Desktop **must** monitor **`Logs\WoWChatLog.txt`** for **`[MGM_CONFIRM:UUID]`** and only then transition the payout to **`Sent`** on the server (see §3, §5).

## 10) Chat log parsing & Desktop bridge (MVP)

### Desktop `WoWChatLog.txt` responsibilities (normative summary)

**Single integration surface for MVP:** Desktop **must** implement **one** real-time tail of **`Logs\WoWChatLog.txt`** and apply **three** normative patterns:

| Tag / prefix | When emitted by addon | Desktop action |
|-----|------------------------|----------------|
| **`[MGM_WHO]{...json}`** | After **`/who`** parse for the spin candidate (§8) | **`POST /api/roulette/verify-candidate`** with parsed JSON + **`X-MGM-ApiKey`** |
| **`[MGM_ACCEPT:UUID]`** | After Lua detects valid whisper **`!twgold`** from the expected winner (§9) | **`POST /api/payouts/{id}/confirm-acceptance`** with **`{id}`** = UUID |
| **`[MGM_CONFIRM:UUID]`** | After **MGM-armed** mail succeeds (**`MAIL_SEND_SUCCESS`**, §9) | **`PATCH`** payout → **`Sent`** (or equivalent) |

- **Important:** **`[MGM_ACCEPT:UUID]`** is **addon-emitted** after whisper events — Desktop is **not** parsing the user’s whisper body from the log. (Parsing raw whisper lines from **`WoWChatLog.txt`** remains **out of scope** for MVP.)

### `[MGM_WHO]` path (required for automated `verify-candidate`)

Desktop **must** monitor the **same** tail as **`[MGM_ACCEPT]`** / **`[MGM_CONFIRM]`**.

- **Prefix (normative):** `[MGM_WHO]` immediately followed by JSON (**§8**).
- **Parse:** extract the JSON object after the prefix; validate **`schemaVersion`**; **POST** to **`POST /api/roulette/verify-candidate`**.
- Duplicate or stale lines: follow **§5** idempotency / **`400`** for out-of-sequence reports.

### WoW whisper path (acceptance — not `Sent`, normative)

- **`!twgold`** as a **private in-game message** from the winner is delivered to the addon via **Lua**; the addon then **prints `[MGM_ACCEPT:UUID]`** so Desktop can complete the HTTP step (see §9).

### `[MGM_ACCEPT:UUID]` path (required for automated `confirm-acceptance`)

Desktop **must** monitor:

- `Logs\WoWChatLog.txt`

Regex (normative): `\\[MGM_ACCEPT:([0-9a-fA-F-]{36})\\]`

Behavior notes:

- On first match for a payout id, Desktop calls **`POST /api/payouts/{id}/confirm-acceptance`** (see §5).
- If the UUID is **unknown**, the payout is in a **wrong state**, or **`{id}`** does not exist: **log** and **ignore** (do **not** spam the API) — **§5** error handling may still return **`404`** if called.
- Duplicate lines should be **idempotent** on the Backend side.

### `[MGM_CONFIRM:UUID]` path (required for `Sent`)

Desktop **must** monitor:

- `Logs\WoWChatLog.txt` (same tail as above)

Regex (normative): `\\[MGM_CONFIRM:([0-9a-fA-F-]{36})\\]`

Behavior notes:

- Desktop should tolerate log rotation / truncation.
- On match, Desktop updates Backend to **`Sent`** for that payout id.
- Desktop should allow a manual override (**Mark as Sent**) only as an operator escape hatch if automation misses (policy decision).

**Note:** The winner also receives an in-game **whisper** on **`MAIL_SEND_SUCCESS`** (MGM path only); that whisper **does not** replace **`[MGM_CONFIRM:UUID]`** for Desktop parsing.

## 11) Twitch Extension: visual roulette (MVP)

- Display the **participant pool** (or count) and a **visual roulette** animation on each spin.
- Show a **countdown / timer** to the next spin using **`GET /api/roulette/state`** (`nextSpinAt`, `serverNow`); do **not** use a client-only clock as the authority for spin timing. Fixed **5-minute** cadence; **no** early spin UX.
- **Enrollment copy:** tell viewers they **must be subscribers** and must type **`!twgold <CharacterName>`** in **stream chat** to join the pool (not only a form inside the Extension). **Character names** in the pool must be **unique** (explain collision if needed).
- **Online verification** for the winning entry uses **`/who <Winner_InGame_Nickname>`** before the win is final (see §5); the Extension may reflect “checking…” / “verified” state if the Backend exposes it via **`spinPhase`** / API fields.
- Present the **winner** to the streamer and **all viewers**. For the **winning viewer**, show **“You won”** as soon as the Backend reports their win (after online verification and **`Pending` payout** if applicable).
- **Winner-facing** instructions **must** say: you will receive an **in-game whisper** (Russian text per §9) from the streamer’s character; **reply** to that whisper with **`!twgold`** (case-insensitive) to **consent**; then the streamer sends gold mail; **`Sent`** follows **`[MGM_CONFIRM:UUID]`** in **`WoWChatLog.txt`**.
- After **`Sent`**, the winner is **removed** from the pool; they can **re-enter** with **`!twgold <CharacterName>`** in chat again.
- **Twitch chat — reward sent announcement (normative copy):** When a winning payout becomes **`Sent`** (gold mail confirmed), the **EBS** **MUST** **attempt** to post the confirmation line to **broadcast stream chat** if **Helix** is available — **best-effort** after **3** inline retries (see **EBS** — Helix §11 reward-sent delivery; failure to post **does not** undo **`Sent`**). The **exact** template is ( **`WINNER_NAME`** = enrolled **`CharacterName`** for that payout):

  `Награда отправлена персонажу <WINNER_NAME> на почту, проверяй ящик!`

  **Extension:** keep this string **hardcoded** in the Twitch Extension source (e.g. a small template helper) so in-panel copy stays aligned with the **same** template the **EBS** posts to broadcast chat.

  **Delivery (MVP, locked):** The **EBS** **must** invoke **Twitch Helix** `Send Chat Message` **immediately** after the authoritative **`Sent`** commit, with **3** retry attempts. Use the template above with **`CharacterName`** from the payout. **At-most-once** per **`PayoutId`** (see **`IsRewardSentAnnouncedToChat`**, **§6**). The Extension **does not** trigger chat delivery; it may only reflect **`Sent`** in UI via existing read APIs. The chat line **must not** rely on the WoW addon (Twitch chat is outside the game client).

  **Winner panel:** the winning viewer’s Extension UI **must** show equivalent confirmation (can reuse the same Russian template with **`WINNER_NAME`** = self).

- On **overload** (**`429`**, **`503`**, network errors), follow **§5.1 Extension resilience** (backoff + Retry).

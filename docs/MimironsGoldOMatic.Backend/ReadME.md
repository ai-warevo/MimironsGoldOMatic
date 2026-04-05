<!-- Updated: 2026-04-05 (Tier A validation + Tier B plan) -->

## MimironsGoldOMatic.Backend — EBS (Extension Backend Service) (ASP.NET Core | Bridge between Twitch Extension & WPF Desktop)

<!-- System pipeline and EBS role: docs/ARCHITECTURE.md · Product digest: docs/MVP_PRODUCT_SUMMARY.md -->

- **EBS:** This project **`MimironsGoldOMatic.Backend`** is the **EBS** — same as **`docs/SPEC.md`**. It owns **Twitch Extension JWT** validation (**HS256** via **`Twitch:ExtensionSecret`**; optional **`aud`** = **`ExtensionClientId`**), **EventSub** (`channel.chat.message` → **`POST /api/twitch/eventsub`**), **Helix** (§11 **`Send Chat Message`**, inline **3×** retry in **`HelixChatService`**, **no** Outbox in MVP), and Desktop **`X-MGM-ApiKey`** routes.
- **Repository status:** `src/MimironsGoldOMatic.Backend` implements **MVP-2** per `docs/ROADMAP.md`: Marten on PostgreSQL, MediatR command/query handlers, Extension JWT + Desktop `X-MGM-ApiKey`, EventSub chat enrollment, roulette/pool/payout HTTP surface, Helix reward-sent announcement (inline retries), and hourly payout expiration. Set `ConnectionStrings:PostgreSQL`, `Mgm:ApiKey`, and `Twitch:*` in configuration (see `appsettings.Development.json` for a local Postgres example). **EF Core** is not used in this project yet (read models are Marten documents).
- **UI spec (consumer-facing):** [`docs/UI_SPEC.md`](../UI_SPEC.md) (hub) and per-client [`docs/MimironsGoldOMatic.TwitchExtension/UI_SPEC.md`](../MimironsGoldOMatic.TwitchExtension/UI_SPEC.md), [`docs/MimironsGoldOMatic.Desktop/UI_SPEC.md`](../MimironsGoldOMatic.Desktop/UI_SPEC.md), [`docs/MimironsGoldOMatic.WoWAddon/UI_SPEC.md`](../MimironsGoldOMatic.WoWAddon/UI_SPEC.md); API shapes remain canonical in `docs/SPEC.md`.
- **Stack:** ASP.NET Core, Marten (Event Store), PostgreSQL. EF Core remains optional for future read-side tooling (`docs/SPEC.md` §6).

## Key Functions

<!-- Pool / roulette / abuse rules summary: docs/MVP_PRODUCT_SUMMARY.md · End-to-end flow: docs/WORKFLOWS.md -->

- **Authentication (phased):**
  - **MVP:** Extension **Bearer** JWT validated with symmetric key from **`Twitch:ExtensionSecret`**; **Development** fallback when secret empty. **Issuer** / JWKS-style rotation: roadmap.
  - **Desktop security (MVP):** **`X-MGM-ApiKey`** matching **`Mgm:ApiKey`** (`ApiKeyAuthenticationHandler`).
- **Persistence model (MVP):**
  - Write-side source of truth: Marten Event Store in PostgreSQL.
  - Read-side query model: projections/read tables (EF Core optional for mapping/querying projections).
- **Chat ingestion (MVP):** **EventSub** `channel.chat.message` — ingest **`!twgold <CharacterName>`** only (enroll / **replace** name for same user per `docs/SPEC.md` §5). **Subscriber** eligibility from the **EventSub payload only** (no Helix lookup on enroll); **unique** name among others; non-subscribers: **log only** (no chat reply). Dedupe by Twitch **`message_id`**. **Acceptance** is **`POST .../confirm-acceptance`** after **`[MGM_ACCEPT:UUID]`** in **`WoWChatLog.txt`** (see `docs/SPEC.md` §9–10).
- **Roulette `/who`:** Desktop parses **`[MGM_WHO]`** from **`WoWChatLog.txt`** and forwards JSON → **`POST /api/roulette/verify-candidate`**; **EBS** **authoritatively** creates **`Pending`** or **no winner** (**no** second candidate in the same **5-minute** cycle — see `docs/SPEC.md` §1, §5, §8).
- **Idempotency / pool:** **Unique `CharacterName`** in active pool; optional **`EnrollmentRequestId`** for Extension **`POST /api/payouts/claim`**.
- **Abuse prevention (MVP):**
  - Fixed 1,000g per **winning** payout (after a spin selects a winner).
  - Max 10,000g lifetime total per Twitch user.
  - One active payout per Twitch user at a time.
  - Rate limiting (e.g. ~5 req/min per IP/user).
- **Roulette (MVP):**
  - **Visual roulette** cadence: default **every 5 minutes**; **minimum 1** participant.
  - **Candidate selection:** **uniform random** among active pool rows (`docs/SPEC.md` glossary, §5).
  - **Non-winners remain in the pool** after each spin; **winners are removed when `Sent`** (may re-enroll via chat).
  - **Online gate:** spin resolution **must** use **`/who <Winner_InGame_Nickname>`** before **`Pending` payout**; offline candidates invalid (**no** second pick same cycle — `docs/SPEC.md`).
  - **Winner notification:** API/state so the **Twitch Extension** can show **“You won”**; **in-game** whisper flow per **`docs/SPEC.md` §9** (Russian text + reply **`!twgold`**).
- **Acceptance vs sent:**
  - Record **willingness to accept** when Desktop calls **`confirm-acceptance`** after observing **`[MGM_ACCEPT:UUID]`** in **`WoWChatLog.txt`** (addon printed after Lua whisper **`!twgold`** match; `docs/SPEC.md` §9–10).
  - Set **`Sent`** only when Desktop reports **`[MGM_CONFIRM:UUID]`** observed in **`WoWChatLog.txt`** (mail actually sent); then **remove winner from pool**.
- **Expiration:** Hourly background job marks `Pending`/`InProgress` older than 24 hours as `Expired` (no reactivation).
- **Twitch chat (reward sent, §11):** When a payout becomes **`Sent`**, the **EBS** **must** **attempt** **`Send Chat Message`** via **Helix** (inline **3×** retry; **no** **`Sent`** rollback on failure; **once** per **`PayoutId`** — `IsRewardSentAnnouncedToChat` / transition guard, `docs/SPEC.md` EBS + §6 + §11).

## API Endpoints

- **POST** `/api/twitch/eventsub`: **EventSub** webhook (**AllowAnonymous** + HMAC when secret set). **`channel.chat.message`** → **`ChatEnrollmentService`** (`docs/SPEC.md` §5).
- **POST** `/api/payouts/claim` (optional): Extension/Dev Rig pool enrollment; **`Mgm:DevSkipSubscriberCheck`** gates Helix-less dev use (**`docs/SPEC.md`** §5). Returns **`PoolEnrollmentResponse`** JSON on success.
- **GET** `/api/payouts/pending`: Fetched by the Desktop App. Returns **winner** payouts available for sync/injection (primarily `Pending`).
- **PATCH** `/api/payouts/{id}/status`: Updates payout status where allowed (Desktop); response body **`PayoutDto`**.
- **POST** `/api/payouts/{id}/confirm-acceptance` (recommended): Desktop reports **`!twgold`** matched the winner → record **acceptance** (not **`Sent`**).
- **GET** `/api/payouts/my-last`: Used by the Twitch Extension (pull model) to show the viewer their latest payout status.
  - Returns `404 Not Found` when no payout exists for caller.
- **Pool / spin endpoints (MVP):** **`GET /api/roulette/state`**, **`GET /api/pool/me`** (Extension **JWT-only**); **`POST /api/roulette/verify-candidate`** (Desktop **ApiKey**) — see `docs/SPEC.md` §5–5.1.

## Automated tests (MVP-6)

- Project: **`src/Tests/MimironsGoldOMatic.Backend.UnitTests`** (referenced from **`src/MimironsGoldOMatic.slnx`**). See **`src/Tests/MimironsGoldOMatic.Backend.UnitTests/README.md`** for coverage scope and **`coverlet.runsettings`**.
- **Unit (no Docker):** **`dotnet test src/MimironsGoldOMatic.slnx --filter Category=Unit`** — time/spin phase, `!twgold` parser, controllers (Moq), ApiKey auth, Helix client, EventSub controller, FluentValidation.
- **Integration (Docker):** **`dotnet test src/MimironsGoldOMatic.slnx --filter Category=Integration`** — PostgreSQL via **Testcontainers**, Marten + MediatR (claims, chat enrollment, `verify-candidate`, expiration, payout status, roulette tick).
- **All tests:** **`dotnet test src/MimironsGoldOMatic.slnx`** — runs unit + integration; full suite needs Docker. Not a substitute for Twitch/WoW manual scenarios.
- **CI Tier A (E2E mocks):** GitHub Actions **`.github/workflows/e2e-test.yml`** runs Backend + Postgres + **`src/Mocks/MockEventSubWebhook`** + **`src/Mocks/MockExtensionJwt`**, sends a synthetic **`channel.chat.message`**, asserts **`GET /api/pool/me`**. See [`docs/E2E_AUTOMATION_PLAN.md`](../E2E_AUTOMATION_PLAN.md) ([How to run Tier A E2E tests](../E2E_AUTOMATION_PLAN.md#how-to-run-tier-a-e2e-tests-github-actions)).

### Running Tier A E2E locally (manual)

Mirror the workflow on one machine (Linux/macOS/WSL or separate terminals on Windows):

1. Start **PostgreSQL 16** with database **`mgm`**, user/password matching your connection string (same shape as CI: `Host=localhost;Port=5432;Database=mgm;Username=postgres;Password=postgres`).
2. **Terminal A — Backend:**  
   `ASPNETCORE_ENVIRONMENT=Development`  
   `ASPNETCORE_URLS=http://127.0.0.1:8080`  
   `ConnectionStrings__PostgreSQL=Host=localhost;Port=5432;Database=mgm;Username=postgres;Password=postgres`  
   `Mgm__ApiKey=ci-desktop-api-key`  
   `Mgm__DevSkipSubscriberCheck=true`  
   `Twitch__EventSubSecret=<same secret as mocks>`  
   then `dotnet run --project src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.csproj -c Release` (or Debug).
3. **Terminal B — MockEventSubWebhook:**  
   `ASPNETCORE_URLS=http://127.0.0.1:9051`  
   `Backend__BaseUrl=http://127.0.0.1:8080`  
   `Twitch__EventSubSecret=<same as Backend>`  
   then `dotnet run --project src/Mocks/MockEventSubWebhook/MimironsGoldOMatic.Mocks.MockEventSubWebhook.csproj`.
4. **Terminal C — MockExtensionJwt:**  
   `ASPNETCORE_URLS=http://127.0.0.1:9052`  
   Leave **`Twitch:ExtensionSecret`** empty only if Backend is in **Development** (shared dev key); otherwise set the **same** base64 **`Twitch:ExtensionSecret`** on both.  
   `dotnet run --project src/Mocks/MockExtensionJwt/MimironsGoldOMatic.Mocks.MockExtensionJwt.csproj`.
5. Send the synthetic EventSub notification:  
   `python3 .github/scripts/send_e2e_eventsub.py --url http://127.0.0.1:9051 --secret "<secret>" --user-id e2e-viewer-1 --login e2eviewer1 --text "!twgold Etoehero"`
6. Fetch a token and call the API:  
   `curl -s "http://127.0.0.1:9052/token?userId=e2e-viewer-1&displayName=E2EViewer"` → use **`access_token`** as **`Authorization: Bearer …`** on **`GET http://127.0.0.1:8080/api/pool/me`**.

Full checklist: [`docs/E2E_AUTOMATION_TASKS.md`](../E2E_AUTOMATION_TASKS.md) (**Tier A Validation Checklist**).

### Debugging mock services

- **`MockEventSubWebhook` returns 401:** HMAC failed — align **`Twitch__EventSubSecret`** with **`--secret`** in **`send_e2e_eventsub.py`**; ensure the script’s JSON body is unchanged between signing and POST (compact JSON as in the script).
- **Forward fails / 502 from mock:** Backend not listening on **`Backend:BaseUrl`** or wrong path; check mock logs for EBS **`StatusCode`**.
- **`GET /api/pool/me` 401:** JWT signing key mismatch — in **Development**, both Backend and **MockExtensionJwt** must use empty **`ExtensionSecret`** (dev SHA256 fallback) **or** the same base64 secret; if **`Twitch:ExtensionClientId`** is set on Backend, set the same **`Twitch:ExtensionClientId`** on the mock so **`aud`** validates.
- **Health checks:** **`GET /health`** on **9051** / **9052** should return JSON with **`status`** **`ok`**.

### Environment variables (local E2E)

| Variable | Component | Purpose |
|----------|-----------|---------|
| `ASPNETCORE_ENVIRONMENT` | Backend | **`Development`** for dev Extension JWT key when secret empty. |
| `ASPNETCORE_URLS` | All ASP.NET processes | Bind addresses (**8080**, **9051**, **9052**). |
| `ConnectionStrings__PostgreSQL` | Backend | Marten / PostgreSQL. |
| `Mgm__ApiKey` | Backend | Desktop **`X-MGM-ApiKey`** routes (set for parity with Desktop tests). |
| `Mgm__DevSkipSubscriberCheck` | Backend | **`true`** so synthetic subscriber payload enrolls without Helix. |
| `Twitch__EventSubSecret` | Backend + MockEventSubWebhook | HMAC verification (empty = skip verification). |
| `Backend__BaseUrl` | MockEventSubWebhook | EBS root URL for forward. |
| `Twitch__ExtensionSecret` | Backend + MockExtensionJwt | Base64 symmetric key; empty + **Development** uses shared dev fallback string. |
| `Twitch__ExtensionClientId` | Backend + MockExtensionJwt | Optional JWT **`aud`**; must match if set. |

## Additional Libraries

- `Marten`
- `MediatR`
- `FluentValidation.DependencyInjectionExtensions`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Polly.Extensions.Http` (referenced; **Helix** retries are implemented as a simple **3-attempt** loop in **`HelixChatService`** today)

## Architecture & Patterns
- **Idempotency Pattern:**
  Use `EnrollmentRequestId` as the idempotency key. If a network lag causes the extension to send the same request twice, the backend must return the existing record instead of creating a duplicate or consuming limits.
  
- **Outbox Pattern:** **Do not** add an **Outbox** table in MVP. **Helix** §11 uses **inline** post-**`Sent`** calls (**not** Outbox). A future **Outbox** (e.g. Discord) is **post-MVP** — see `docs/SPEC.md` §6.

- **Specification Pattern (Business Rules):**
  Encapsulate business logic in Specification classes:
  - `LifetimeLimitSpecification`: Checks the 10k gold cap.
  - `ActiveRequestSpecification`: Ensures only one active request per Twitch user.
  This makes business rules readable, testable, and reusable.

## Event Sourcing
- **Marten Integration:** Payout streams use registered event types such as **`PayoutCreated`**, **`PayoutStatusChanged`**, **`WinnerAcceptanceRecorded`**, **`HelixRewardSentAnnouncementSucceeded`** (`Program.cs`). This provides an append-only audit trail for payout lifecycle changes.

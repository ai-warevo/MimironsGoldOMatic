<!-- Updated: 2026-04-05 (Deduplication pass) -->

## MimironsGoldOMatic.TwitchExtension (React | Bridge between Twitch & EBS)

**Cross-cutting:** [`docs/overview/ARCHITECTURE.md`](../../overview/ARCHITECTURE.md) · [`docs/overview/MVP_PRODUCT_SUMMARY.md`](../../overview/MVP_PRODUCT_SUMMARY.md) · [`docs/reference/WORKFLOWS.md`](../../reference/WORKFLOWS.md)

- **Repository status:** `src/MimironsGoldOMatic.TwitchExtension` is **MVP-5** — Vite + React + TypeScript viewer panel with **`window.Twitch.ext`**, Zustand (`mgmPanelStore`), axios EBS client, **`mgmEbsRepository`** calling **`/api/roulette/state`**, **`/api/pool/me`**, **`/api/payouts/my-last`**, visual roulette + countdown from **`nextSpinAt`/`serverNow`**, and **UI-101–106**-aligned UX ([`UI_SPEC.md`](UI_SPEC.md)). See `docs/reference/IMPLEMENTATION_READINESS.md`.
- **UI spec:** [`UI_SPEC.md`](UI_SPEC.md) in this folder — **MVP:** viewer **UI-101–106** only. Broadcaster **UI-201–204** are **post-MVP** (see `docs/overview/ROADMAP.md` MVP-5). Cross-cutting tokens: [`docs/reference/UI_SPEC.md`](../../reference/UI_SPEC.md).
- **Role:** Viewer-facing **roulette**, pool display, and **winner / payout status**. **Pool enrollment is driven by Twitch chat** (`!twgold <CharacterName>` per `docs/overview/SPEC.md`), not by a form-only flow.
- **Stack:** React 19, Vite 8, TypeScript 5.9, Zustand, axios (no Tailwind in this package — panel styling is component CSS).

## Key Functions

- **On-panel copy:** Viewers **subscribe** and type **`!twgold <CharacterName>`** in **stream chat** to join the pool; after a win, **watch WoW** for the streamer’s **whisper** (Russian text, `docs/overview/SPEC.md` §9) and **reply `!twgold`** in-game before gold mail.
- **Twitch Integration:** Uses `window.Twitch.ext` for viewer identity and **polls** the **EBS** for pool size, spin state, and winner/payout status.
- **API Interaction (typical):**
  - **GET** `/api/roulette/state`, **`GET /api/pool/me`**, **`GET /api/payouts/my-last`** — all use **Twitch Extension JWT (Bearer)** only in MVP (`docs/overview/SPEC.md` §5, §5.1). **Dev Rig** uses **real Twitch-issued** tokens; **EBS** validates per Twitch (`docs/overview/SPEC.md` deployment scope).
  - **`GET /api/roulette/state`** + **`GET /api/pool/me`** — server-authoritative **`nextSpinAt`** (UTC **:00/:05/…**), **`spinPhase`** enum, optional **`currentSpinCycleId`**. **Must** drive the **countdown** from `nextSpinAt` / `serverNow`.
  - **`GET /api/payouts/my-last`** — **`PayoutDto`** or **`404`** when the viewer has **no** winner payout yet.
  - **Optional:** **POST** `/api/payouts/claim` for Dev Rig / testing only — requires **`Mgm:DevSkipSubscriberCheck`** on the EBS until Helix subscriber verification is implemented for this path (`docs/overview/SPEC.md` §5).
- **Visual roulette:** Animated selection on each spin; **5-minute** cadence; **minimum 1** participant; reflect **`/who`** / verification if the **EBS** exposes it.
- **Winner UX:** **“You won”** + instructions: **in WoW**, reply to the streamer’s whisper with **`!twgold`** (case-insensitive; see `docs/overview/SPEC.md` §9); **`Sent`** after **`[MGM_CONFIRM:UUID]`** in WoW log per spec.
- **Reward-sent copy (normative):** Hardcode the Russian template for **`Sent`** (in-panel) per **`docs/overview/SPEC.md` §11: `Награда отправлена персонажу <WINNER_NAME> на почту, проверяй ящик!`** (`WINNER_NAME` = enrolled **`CharacterName`**). Twitch **broadcast chat** delivery is **EBS Helix** only in MVP (Extension **does not** trigger chat post).

## Libraries

- `axios` — HTTP client; **`createMimironsGoldOMaticEbsClient`** adds **`Authorization: Bearer`** from Twitch helper.
- `zustand` — panel store + polling hooks (`useMgmEbsPolling`, `useMgmSpinCountdown`, `useTwitchExtensionAuth`).
- **Dev Rig** — external Twitch tooling (not an npm dependency in `package.json`).

## Architecture & Patterns
- **Repository:** **`createMimironsGoldOMaticEbsRepository`** wraps typed GET/POST paths (`src/api/mgmEbsRepository.ts`).
- **Store / hooks:** Zustand drives roulette state, API errors, and backoff; see `src/state/mgmPanelStore.ts` and `src/hooks/`.
- **Error boundary:** **`PanelErrorBoundary`** wraps the viewer UI (`src/components/PanelErrorBoundary.tsx`).

## Key Features
- **Pull-based Status:** Poll the **EBS** for pool/spin state and `/my-last` to show **winner** payout progress (`Pending` -> `InProgress` -> `Sent`). Messaging: **`Sent`** means mail was confirmed via **`[MGM_CONFIRM:UUID]`** after **WoW whisper `!twgold`** consent (see `docs/overview/SPEC.md`).
- **Resilience:** On **`429`**, **`503`**, or network errors, use **exponential backoff** (cap interval, e.g. ≤ 60s) and a **Retry** action (`docs/overview/SPEC.md` §5.1).

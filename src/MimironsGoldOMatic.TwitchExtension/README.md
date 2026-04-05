<!-- Updated: 2026-04-05 (Deduplication pass) -->

# Mimiron's Gold-o-Matic — Twitch Extension (source)

MVP **viewer** panel: **Vite + React + TypeScript** (Zustand, axios, EBS polling). **Behavior and APIs:** [`docs/SPEC.md`](../../docs/SPEC.md), [`docs/MimironsGoldOMatic.TwitchExtension/UI_SPEC.md`](../../docs/MimironsGoldOMatic.TwitchExtension/UI_SPEC.md) (**UI-101–106**), hub [`docs/UI_SPEC.md`](../../docs/UI_SPEC.md). **Product digest:** [`docs/MVP_PRODUCT_SUMMARY.md`](../../docs/MVP_PRODUCT_SUMMARY.md). **Architecture:** [`docs/ARCHITECTURE.md`](../../docs/ARCHITECTURE.md). **Full component doc:** [`docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`](../../docs/MimironsGoldOMatic.TwitchExtension/ReadME.md).

**Setup:** [`docs/SETUP.md`](../../docs/SETUP.md) (prerequisites + first build); **Backend / Twitch keys:** [`docs/SETUP-for-developer.md`](../../docs/SETUP-for-developer.md). **Env:** copy `.env.example` → `.env.local`, set **`VITE_MGM_EBS_BASE_URL`**.

**Scripts:** `npm run dev`, `npm run build`, `npm run lint` — see **`package.json`**.

**Automated tests (Vitest, in this package — not under repo `src/Tests/` .NET projects):**

- **`npm test`** — ESLint, **`vitest run`** (unit + integration), then production **`tsc` + `vite build`** (same as CI).
- **`npm run test:unit`** — `*.test.ts` / `*.test.tsx` only (e.g. colocated next to components).
- **`npm run test:integration`** — `*.integration.test.ts` (HTTP via **MSW**; shared server in **`src/test/msw/server.ts`**, setup in **`src/test/setup.ts`**).
- **`npm run test:vitest`** — all Vitest suites without lint/build.

Backend/Desktop (from repo root): **`dotnet test src/MimironsGoldOMatic.slnx`**.

<!-- Generic Vite / React / ESLint template guidance: https://vite.dev/ -->

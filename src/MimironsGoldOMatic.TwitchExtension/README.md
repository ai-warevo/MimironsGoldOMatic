<!-- Updated: 2026-04-05 (Deduplication pass) -->

# Mimiron's Gold-o-Matic — Twitch Extension (source)

MVP **viewer** panel: **Vite + React + TypeScript** (Zustand, axios, EBS polling). **Behavior and APIs:** [`docs/SPEC.md`](../../docs/SPEC.md), [`docs/UI_SPEC.md`](../../docs/UI_SPEC.md) (**UI-101–106**). **Product digest:** [`docs/MVP_PRODUCT_SUMMARY.md`](../../docs/MVP_PRODUCT_SUMMARY.md). **Architecture:** [`docs/ARCHITECTURE.md`](../../docs/ARCHITECTURE.md). **Full component doc:** [`docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`](../../docs/MimironsGoldOMatic.TwitchExtension/ReadME.md).

**Setup:** [`docs/SETUP_COMMON.md`](../../docs/SETUP_COMMON.md) (prerequisites + first build); **Backend / Twitch keys:** [SETUP-for-developer.md](../../SETUP-for-developer.md). **Env:** copy `.env.example` → `.env.local`, set **`VITE_MGM_EBS_BASE_URL`**.

**Scripts:** `npm run dev`, `npm run build`, `npm run lint` — see **`package.json`**.

<!-- Generic Vite / React / ESLint template guidance: https://vite.dev/ -->

<!-- Updated: 2026-04-05 (Deduplication pass) -->

# Mimiron's Gold-o-Matic — Twitch Extension (source)

MVP **viewer** panel: **Vite + React + TypeScript** (Zustand, axios, EBS polling). **Behavior and APIs:** [`docs/overview/SPEC.md`](../../docs/overview/SPEC.md), [`docs/components/twitch-extension/UI_SPEC.md`](../../docs/components/twitch-extension/UI_SPEC.md) (**UI-101–106**), hub [`docs/reference/UI_SPEC.md`](../../docs/reference/UI_SPEC.md). **Product digest:** [`docs/overview/MVP_PRODUCT_SUMMARY.md`](../../docs/overview/MVP_PRODUCT_SUMMARY.md). **Architecture:** [`docs/overview/ARCHITECTURE.md`](../../docs/overview/ARCHITECTURE.md). **Full component doc:** [`docs/components/twitch-extension/ReadME.md`](../../docs/components/twitch-extension/ReadME.md).

**Setup:** [`docs/setup/SETUP.md`](../../docs/setup/SETUP.md) (prerequisites + first build); **Backend / Twitch keys:** [`docs/setup/SETUP-for-developer.md`](../../docs/setup/SETUP-for-developer.md). **Env:** copy `.env.example` → `.env.local`, set **`VITE_MGM_EBS_BASE_URL`**.

**Scripts:** `npm run dev`, `npm run build`, `npm run lint` — see **`package.json`**.

## API code generation (C# -> TypeScript Axios)

TypeScript API artifacts are generated from backend C# contracts during backend build.

- Trigger: `dotnet build src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.csproj`
- Generator project: `src/Tools/MimironsGoldOMatic.ApiTsGen`
- Generated files:
  - `src/MimironsGoldOMatic.TwitchExtension/src/api/models.ts`
  - `src/MimironsGoldOMatic.TwitchExtension/src/api/client.ts`

### Install dependencies

```bash
npm install
```

### Manual generator run (debugging)

```bash
dotnet run --project src/Tools/MimironsGoldOMatic.ApiTsGen/MimironsGoldOMatic.ApiTsGen.csproj -- \
  src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Api \
  src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Application \
  src/MimironsGoldOMatic.Shared \
  src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Common \
  src/MimironsGoldOMatic.TwitchExtension/src/api
```

### Usage example

```typescript
import { MimironsGoldOMaticApiClient } from './api/client'

const api = new MimironsGoldOMaticApiClient(
  import.meta.env.VITE_MGM_EBS_BASE_URL,
  () => window.localStorage.getItem('mgm.jwt'),
)

const state = await api.getRouletteState()
console.log(state.spinPhase, state.nextSpinAt)
```

**Automated tests (Jest, in this package — not under repo `src/Tests/` .NET projects):**

- **`npm test`** — ESLint, **`jest`** (unit + integration), then production **`tsc` + `vite build`** (same as CI).
- **`npm run test:unit`** — unit tests only (excludes `*.integration.test.ts`).
- **`npm run test:integration`** — `*.integration.test.ts` (HTTP via **axios-mock-adapter** on the same `AxiosInstance` the panel uses; setup in **`src/test/jest.setup.ts`**).
- **`npm run test:jest`** — all Jest suites without lint/build.

Backend/Desktop (from repo root): **`dotnet test src/MimironsGoldOMatic.slnx`**.

<!-- Generic Vite / React / ESLint template guidance: https://vite.dev/ -->

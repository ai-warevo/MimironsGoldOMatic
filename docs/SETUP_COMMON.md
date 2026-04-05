<!-- Updated: 2026-04-05 (Deduplication pass) -->

# Shared setup — prerequisites and first build

Used by **[SETUP-for-developer.md](../SETUP-for-developer.md)**. Streamers may skim **prerequisites** when working with a developer. **Product behavior:** [`docs/SPEC.md`](SPEC.md).

## Prerequisites

| Tool | Used for | Notes |
|------|-----------|--------|
| **[.NET SDK 10](https://dotnet.microsoft.com/download)** | Shared, Backend, Desktop | `dotnet --version` → **10.x**. |
| **[Node.js](https://nodejs.org/)** (LTS) | `src/MimironsGoldOMatic.TwitchExtension` | `node --version`, `npm --version`. |
| **PostgreSQL** | EBS / Marten | Local, Docker, or cloud; **16+** reasonable. |
| **Git** | Clone / contribute | — |

**Optional:** [Twitch Developer Rig](https://dev.twitch.tv/docs/extensions/rig/) (Extension + real JWTs); **WoW 3.3.5a** client (addon + Desktop integration).

## Clone and restore

```bash
git clone <repository-url>
cd MimironsGoldOMatic
```

**.NET**

```bash
dotnet restore src/MimironsGoldOMatic.slnx
dotnet build src/MimironsGoldOMatic.slnx
```

**Twitch Extension**

```bash
cd src/MimironsGoldOMatic.TwitchExtension
npm install
npm run build
```

**Local Extension dev server**

```bash
npm run dev
```

Copy **`.env.example`** → **`.env.local`** and set **`VITE_MGM_EBS_BASE_URL`** to the EBS base URL (no trailing slash), e.g. `http://localhost:5088`. Use the **Developer Rig** for real Extension JWTs; plain Vite alone stays unauthenticated until a token exists.

## Next steps

- **Backend config, PostgreSQL, Twitch keys:** [SETUP-for-developer.md](../SETUP-for-developer.md) §3 onward.
- **Streamer install:** [SETUP-for-streamer.md](../SETUP-for-streamer.md).

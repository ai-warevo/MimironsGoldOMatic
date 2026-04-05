<!-- Updated: 2026-04-05 -->

# Setup

Setup is split by role: **this file** (overview, shared prerequisites, and first **dotnet** / **npm** build), **[SETUP-for-developer.md](SETUP-for-developer.md)** (PostgreSQL, Backend `appsettings`, Twitch keys, running from source), and **[SETUP-for-streamer.md](SETUP-for-streamer.md)** (WoW addon, Twitch Extension, Desktop for broadcasters).

Start with the table below; the developer guide explains every `appsettings` field (including `Twitch` and `Mgm`) and where to obtain values from Twitch.

## Guide index

| Guide | Audience |
|--------|----------|
| **This document** — [Shared prerequisites and first build](#shared-prerequisites-and-first-build) | Everyone: prerequisites, clone, first **dotnet** / **npm** build (referenced from the developer guide). |
| **[SETUP-for-developer.md](SETUP-for-developer.md)** | Contributors: PostgreSQL, Backend configuration (including Twitch keys), running projects from source. |
| **[SETUP-for-streamer.md](SETUP-for-streamer.md)** | Streamers/operators: installing the WoW addon, Twitch Extension, and Desktop app, and what still requires a developer build today. |

---

## Shared prerequisites and first build

Used by **[SETUP-for-developer.md](SETUP-for-developer.md)**. Streamers may skim **prerequisites** when working with a developer. **Product behavior:** [`SPEC.md`](../overview/SPEC.md).

### Prerequisites

| Tool | Used for | Notes |
|------|-----------|--------|
| **[.NET SDK 10](https://dotnet.microsoft.com/download)** | Shared, Backend, Desktop | `dotnet --version` → **10.x**. |
| **[Node.js](https://nodejs.org/)** (LTS) | `src/MimironsGoldOMatic.TwitchExtension` | `node --version`, `npm --version`. |
| **PostgreSQL** | EBS / Marten | Local, Docker, or cloud; **16+** reasonable. |
| **Git** | Clone / contribute | — |

**Optional:** [Twitch Developer Rig](https://dev.twitch.tv/docs/extensions/rig/) (Extension + real JWTs); **WoW 3.3.5a** client (addon + Desktop integration).

### Clone and restore

```bash
git clone <repository-url>
cd MimironsGoldOMatic
```

**.NET**

```bash
dotnet restore src/MimironsGoldOMatic.slnx
dotnet build src/MimironsGoldOMatic.slnx
```

**Backend integration tests (MVP-6)**

```bash
dotnet test src/MimironsGoldOMatic.slnx
```

Requires **[Docker](https://docs.docker.com/get-docker/)** running locally for **integration** tests: **Testcontainers** starts **PostgreSQL**. Fast **unit** slice (no Docker): **`dotnet test src/MimironsGoldOMatic.slnx --filter Category=Unit`**. Without Docker, the full `dotnet test` (all categories) fails when starting the container.

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

### Next steps

- **Backend config, PostgreSQL, Twitch keys:** [SETUP-for-developer.md](SETUP-for-developer.md) §3 onward.
- **Streamer install:** [SETUP-for-streamer.md](SETUP-for-streamer.md).

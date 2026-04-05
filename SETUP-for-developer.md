# Setup for developers

This document explains how to configure and run **Mimiron's Gold-o-Matic** from the repository: tooling, local services, and **Backend** configuration. For broadcaster-facing install steps (addon / Extension / Desktop), see **[SETUP-for-streamer.md](SETUP-for-streamer.md)**.

Normative product behavior remains in **`docs/SPEC.md`**. Implementation status per component is in **`docs/IMPLEMENTATION_READINESS.md`**.

---

## 1. Prerequisites

Install the following on your machine (versions should match or exceed what the repo targets).

| Tool | Used for | Notes |
|------|-----------|--------|
| **[.NET SDK 10](https://dotnet.microsoft.com/download)** | `MimironsGoldOMatic.Shared`, `MimironsGoldOMatic.Backend`, `MimironsGoldOMatic.Desktop` | Run `dotnet --version` and confirm a **10.x** SDK. |
| **[Node.js](https://nodejs.org/)** (LTS recommended) | `src/MimironsGoldOMatic.TwitchExtension` (Vite + React) | Run `node --version` and `npm --version`. |
| **PostgreSQL** | Backend persistence (Marten Event Store) | Local install, Docker, or a dev instance. **16+** is a reasonable default. |
| **Git** | Cloning and contributing | — |

Optional:

- **Twitch [Developer Rig](https://dev.twitch.tv/docs/extensions/rig/)** — testing the Extension against real Twitch JWTs and your EBS.
- **WoW 3.3.5a client** — addon and Desktop integration testing.

---

## 2. Clone and restore

```bash
git clone <repository-url>
cd MimironsGoldOMatic
```

**.NET**

```bash
dotnet restore src/MimironsGoldOMatic.slnx
dotnet build src/MimironsGoldOMatic.slnx
```

**Twitch Extension (frontend)**

```bash
cd src/MimironsGoldOMatic.TwitchExtension
npm install
npm run build
```

For local UI development:

```bash
npm run dev
```

---

## 3. PostgreSQL

The Backend **requires** a PostgreSQL database. Marten creates/updates schema on startup (`ApplyAllConfiguredChangesToDatabaseAsync` in `Program.cs`).

1. Create a database (example name: `mgm`).
2. Copy the sample connection string from **`src/MimironsGoldOMatic.Backend/appsettings.Development.json`** and adjust host, port, database name, user, and password.
3. Point **`ConnectionStrings:PostgreSQL`** at that database (see §4.3).

Example (local):

```text
Host=localhost;Port=5432;Database=mgm;Username=postgres;Password=yourpassword
```

---

## 4. Backend configuration (`appsettings`)

All paths below are under **`src/MimironsGoldOMatic.Backend/`**.

### 4.1 Files

| File | Purpose |
|------|---------|
| **`appsettings.json`** | Baseline; safe defaults. **Do not commit secrets** — use User Secrets or environment variables in real deployments. |
| **`appsettings.Development.json`** | Local dev overrides (sample Postgres URL, dev API key). Loaded when `ASPNETCORE_ENVIRONMENT=Development`. |

Override in production with:

- **[.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)** (dev machine), or  
- **Environment variables** (double underscore for nested keys, e.g. `ConnectionStrings__PostgreSQL`), or  
- **Azure Key Vault / secret manager** (hosted).

### 4.2 `appsettings.Development.json`: `Mgm` and `Twitch` (lines 5–17)

The repo ships a **development** template so you can run the API locally. The blocks below match **`src/MimironsGoldOMatic.Backend/appsettings.Development.json`** (approximately lines 5–17). Fill **Twitch** when you test real Extension JWTs, EventSub webhooks, or Helix chat posts; leave strings **empty** only while you rely on Development-only behavior (see each row).

```json
  "Mgm": {
    "ApiKey": "dev-desktop-api-key-change-me",
    "DevSkipSubscriberCheck": true
  },
  "Twitch": {
    "ExtensionClientId": "",
    "ExtensionSecret": "",
    "EventSubSecret": "",
    "HelixClientId": "",
    "HelixClientSecret": "",
    "BroadcasterAccessToken": "",
    "BroadcasterUserId": ""
  },
```

| Key | What to put |
|-----|-------------|
| **`Mgm:ApiKey`** | Any strong secret you invent for local use (or replace the sample). The **Desktop** app must send the **same** value in header **`X-MGM-ApiKey`**. Not from Twitch. |
| **`Mgm:DevSkipSubscriberCheck`** | **`true`** = treat non-subscribers like subscribers for **`!twgold`** enrollment (easier local testing). Use **`false`** when you want production-like subscriber gating. |
| **`Twitch:ExtensionClientId`** | Your Extension’s **Client ID** from the [Developer Console](https://dev.twitch.tv/console) → **Extensions** → your extension. Needed so JWT **`aud`** validation matches. |
| **`Twitch:ExtensionSecret`** | **Base64** “Extension Secret” from the same Extension → **Secret Keys**. Backend decodes it for **HS256** JWT validation. In **Development**, if this is **empty**, a fixed dev key is used instead (see §4.5). |
| **`Twitch:EventSubSecret`** | The **webhook secret** you set when creating the EventSub subscription for **`channel.chat.message`** (must match Twitch’s HMAC). If **empty**, signature checks are skipped (dev only). Callback: **`POST /api/twitch/eventsub`**. |
| **`Twitch:HelixClientId`** | **Client ID** of a [registered application](https://dev.twitch.tv/docs/authentication/register-app/) used for Helix (`Client-Id` header on Send Chat Message). Often the same app you used for OAuth tools. |
| **`Twitch:HelixClientSecret`** | **Client secret** of that application. Reserved for token refresh / app-token flows; **currently unused** by Backend code but kept for configuration parity. |
| **`Twitch:BroadcasterAccessToken`** | Broadcaster **user** OAuth token with scopes required for [Send Chat Message](https://dev.twitch.tv/docs/api/reference#send-chat-message). Used as **`Authorization: Bearer`**. |
| **`Twitch:BroadcasterUserId`** | Broadcaster’s numeric Twitch **user id** (must match the account for the token). Used as **`broadcaster_id`** / **`sender_id`** in the chat API body. |

Longer explanations and Twitch doc links: **`Mgm`** in §4.4, **`Twitch`** in §4.5.

### 4.3 `ConnectionStrings:PostgreSQL`

**What it is:** Npgsql connection string for Marten.

**Where to get it:** You define it when you create the PostgreSQL database (local installer, Docker, cloud console).

**Required:** Yes — the app throws at startup if this is missing or empty (non-Development templates).

---

### 4.4 `Mgm` section

| Key | Meaning |
|-----|---------|
| **`ApiKey`** | Shared secret for **Desktop** → Backend calls. Sent as HTTP header **`X-MGM-ApiKey`**. Choose a long random string; enter the **same** value in the Desktop app (**File → Settings**). |
| **`DevSkipSubscriberCheck`** | **`true`** only in local dev if you want to bypass subscriber-only enrollment rules while testing. **`false`** in production-like configs. |

**Where to get `ApiKey`:** You generate it (password manager, `openssl rand -hex 32`, etc.). It is not issued by Twitch.

---

### 4.5 `Twitch` section (detailed)

These values come from the **[Twitch Developer Console](https://dev.twitch.tv/console)** and Twitch APIs. Official references:

- [Extensions — Creating an extension](https://dev.twitch.tv/docs/extensions/#creating-an-extension)
- [Extension JWT overview](https://dev.twitch.tv/docs/extensions/reference/#jwt-schema)
- [EventSub](https://dev.twitch.tv/docs/eventsub/) (webhook verification, subscriptions)
- [Helix API — Send Chat Message](https://dev.twitch.tv/docs/api/reference#send-chat-message)
- [Register an application](https://dev.twitch.tv/docs/authentication/register-app/) (Helix **Client ID** / **Client Secret**)

#### `ExtensionClientId`

**What it is:** Your **Extension** client ID. Used as JWT **`aud`** (audience) when validating Extension Bearer tokens.

**Where to get it:** Twitch Developer Console → your Extension → **Extension client ID** (sometimes labeled Client ID on the extension overview).

#### `ExtensionSecret`

**What it is:** **Base64-encoded** secret Twitch gives you for the Extension. The Backend decodes it and uses it as the **HS256** key to validate Extension JWTs (`Program.cs`).

**Where to get it:** Developer Console → your Extension → **Extension Secrets** (or “Secret Keys”). Twitch shows a **base64** secret — paste that string into **`ExtensionSecret`** as-is.

**Development:** If **`ExtensionSecret`** is empty and **`ASPNETCORE_ENVIRONMENT`** is **Development**, the API uses a **fixed dev-derived key** (SHA256 of a placeholder string). **Non-Development** environments **must** set a real base64 secret or startup fails.

**Common mistake:** Pasting the raw binary or a hex string. It must be the **base64** value from Twitch.

#### `EventSubSecret`

**What it is:** Shared secret used to verify incoming **EventSub** webhook HTTP calls (`Twitch-Eventsub-Message-Signature` HMAC in `TwitchEventSubController`). Must match the secret you configure when creating the EventSub **webhook** subscription for `channel.chat.message`.

**Where to get it:** You choose it when you register the EventSub subscription (Twitch docs: [EventSub — Webhooks](https://dev.twitch.tv/docs/eventsub/handling-webhook-events/)). Store the same value here and in your Twitch subscription tool/script.

**Development:** If left **empty**, signature verification is **skipped** (see code: verification returns `true`). Convenient for local tunnel testing; **do not** use empty secret in production.

**Callback URL:** Your public HTTPS URL ending with **`POST /api/twitch/eventsub`** (e.g. `https://your-domain/api/twitch/eventsub`). Local development typically uses a tunnel ([ngrok](https://ngrok.com/), [Cloudflare Tunnel](https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/), etc.).

#### `HelixClientId` and `HelixClientSecret`

**What they are:** Credentials for a Twitch **application** registered in the Developer Console. **`HelixClientId`** is sent as **`Client-Id`** on Helix HTTP requests (see `HelixChatService`).

**Where to get them:** [Register Your Application](https://dev.twitch.tv/docs/authentication/register-app/) → create a **Confidential** or **Public** app as appropriate → copy **Client ID** and **Client Secret**.

**Note:** The current Backend code uses **`HelixClientId`** for Send Chat Message; **`HelixClientSecret`** is present in configuration for future flows (e.g. app access token or token refresh) and may be unused until implemented.

#### `BroadcasterAccessToken`

**What it is:** OAuth **user access token** for the **broadcaster** account, authorized to send chat messages via Helix. The Backend sends **`Authorization: Bearer <token>`** to `POST https://api.twitch.tv/helix/chat/messages`.

**Where to get it:** Use the Twitch OAuth flow (e.g. [Authorization Code Grant](https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#authorization-code-grant-flow)) with the scopes required by **Send Chat Message** (see the [Send Chat Message](https://dev.twitch.tv/docs/api/reference#send-chat-message) reference for current **required** scopes). Store the **user** access token string here.

**Operational note:** User tokens **expire**. For production you will eventually want a refresh flow; MVP may use manual token renewal during demos.

#### `BroadcasterUserId`

**What it is:** Twitch **numeric user ID** of the broadcaster (same account as the token). Used as `broadcaster_id` and `sender_id` in the Send Chat Message body.

**Where to get it:** Call Helix **`GET https://api.twitch.tv/helix/users`** with the broadcaster token (or use Twitch CLI / dashboard tools). The response includes **`id`**.

---

## 5. Running the Backend

From the repo root:

```bash
cd src/MimironsGoldOMatic.Backend
dotnet run
```

Or specify environment:

```bash
$env:ASPNETCORE_ENVIRONMENT="Development"   # PowerShell
dotnet run
```

- Ensure **`ConnectionStrings:PostgreSQL`** is set.
- For real Extension JWT validation outside Development, set **`Twitch:ExtensionSecret`** (base64) and **`Twitch:ExtensionClientId`** as appropriate.

OpenAPI (Development): typically exposed when `Development` — see `Program.cs` (`MapOpenApi`).

---

## 6. Other projects (short)

| Project | Path | Command |
|---------|------|--------|
| **Shared** | `src/MimironsGoldOMatic.Shared` | Referenced by Backend; `dotnet build` via solution. |
| **Desktop** | `src/MimironsGoldOMatic.Desktop` | `dotnet run --project src/MimironsGoldOMatic.Desktop` (WPF; MVP-4 queue + log tail + injection per readiness doc). |
| **Twitch Extension** | `src/MimironsGoldOMatic.TwitchExtension` | `npm run dev` / `npm run build` (MVP-5 UI still scaffold per readiness doc). |
| **WoW addon** | `src/MimironsGoldOMatic.WoWAddon` | Copy addon folder into WoW **`Interface\AddOns`** (see streamer guide). |

---

## 7. Quick config checklist

- [ ] PostgreSQL running; database created; **`ConnectionStrings:PostgreSQL`** set.
- [ ] **`Mgm:ApiKey`** set; same value in Desktop **File → Settings** (`X-MGM-ApiKey`).
- [ ] **`Twitch:ExtensionClientId`** and **`Twitch:ExtensionSecret`** (base64) from Extension console.
- [ ] **`Twitch:EventSubSecret`** matches webhook; public URL points to **`/api/twitch/eventsub`**.
- [ ] **`Twitch:HelixClientId`** (+ secret if you use token helper flows).
- [ ] **`Twitch:BroadcasterAccessToken`** and **`Twitch:BroadcasterUserId`** for §11 chat announcements.

For end-to-end behavior, align with **`docs/INTERACTION_SCENARIOS.md`** and **`docs/ROADMAP.md`** (MVP-3…5 client work).

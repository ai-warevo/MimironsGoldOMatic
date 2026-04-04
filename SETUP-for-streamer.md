# Setup for streamers and operators

This guide is for **broadcasters** who run Mimiron's Gold-o-Matic: **WoW addon**, **Twitch Extension**, and **Desktop** helper, and how they connect to the **Backend (EBS)**.

**Important:** As of the current repository state, the **Backend** implements MVP-2; the **Desktop**, **Twitch Extension UI**, and **addon** are still **partial or scaffold** in places. See **`docs/IMPLEMENTATION_READINESS.md`** for what is actually implemented. Until releases exist, “setup” often means **running from source** with a developer (see **[SETUP-for-developer.md](SETUP-for-developer.md)**).

Normative behavior (chat commands, mail flow, logs) is defined in **`docs/SPEC.md`**.

---

## 1. What you need (conceptual)

| Piece | Role |
|--------|------|
| **Backend (EBS)** | Hosted API: pool, roulette, payouts, Twitch chat ingestion (EventSub), Extension JWT, Desktop API key. **Requires PostgreSQL** and Twitch configuration. |
| **Twitch Extension** | Panel viewers see: countdown, pool hints, winner status. Talks to the EBS over HTTPS with **Extension JWT**. |
| **Desktop (WPF)** | Runs on the **streamer's PC**: tails **`WoWChatLog.txt`**, calls the EBS with **`X-MGM-ApiKey`**, injects `/run` commands into WoW. |
| **WoW 3.3.5a addon** | In-game mail UI, whispers, **`[MGM_WHO]`**, **`[MGM_ACCEPT:UUID]`**, **`[MGM_CONFIRM:UUID]`** lines in the chat log. |

**MVP scope:** **One** broadcaster channel per Backend instance (`docs/SPEC.md` deployment scope).

---

## 2. WoW addon

### 2.1 Install

1. Get the addon folder **`MimironsGoldOMatic`** (from a release zip or from **`src/MimironsGoldOMatic.WoWAddon`** in this repo once packaged).
2. Copy it into your WoW client:

   ```text
   <WoW 3.3.5a folder>\Interface\AddOns\MimironsGoldOMatic\
   ```

3. Restart WoW (or `/reload` after first install). Enable the addon on the character select **AddOns** list.

### 2.2 Chat log (required for automation)

The Desktop utility and Backend integration rely on lines appearing in Blizzard's chat log file:

```text
<WoW folder>\Logs\WoWChatLog.txt
```

Enable chat logging in WoW options if needed so **`[MGM_WHO]`**, **`[MGM_ACCEPT:UUID]`**, and **`[MGM_CONFIRM:UUID]`** from the addon are written there. The Desktop watches this file (`docs/SPEC.md` §10).

### 2.3 In-game behavior (summary)

- Viewers enroll via **`!twgold <CharacterName>`** in **Twitch chat** (subscriber-gated; see spec).
- After a win, the flow uses **in-game whispers** and **`!twgold`** consent; the addon prints tags the Desktop forwards to the EBS.
- For roulette **online checks**, the Desktop (or a macro) runs **`/run MGM_RunWhoForSpin("<currentSpinCycleId>","<CharacterName>")`** so the addon prints **`[MGM_WHO]`** + JSON (`docs/SPEC.md` §8). **`currentSpinCycleId`** comes from **`GET /api/roulette/state`**.

Details: **`docs/MimironsGoldOMatic.WoWAddon/ReadME.md`** and **`docs/SPEC.md`** §8–10.

---

## 3. Twitch Extension

### 3.1 Viewer installation (when published)

When the Extension is **live on Twitch**:

1. As the broadcaster, add the Extension in the **[Creator Dashboard](https://dashboard.twitch.tv/)** → **Extensions** → **My Extensions**.
2. Activate it as a **panel** (or the slot you configured).
3. Viewers open the panel on your channel; it loads your frontend bundle and obtains a **Twitch Extension JWT** for API calls to your EBS.

### 3.2 Development / testing

Developers use the **[Extension Developer Rig](https://dev.twitch.tv/docs/extensions/rig/)** and a configured **Base URI** pointing at the EBS or a tunnel. Streamers rarely need the Rig unless you are co-testing a dev build.

**Build from repo (developers):**

```bash
cd src/MimironsGoldOMatic.TwitchExtension
npm install
npm run dev
```

Hosted URL for Twitch must be **HTTPS** in production.

Component doc: **`docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`**.

---

## 4. Desktop application

### 4.1 What it does

- Polls **`GET /api/payouts/pending`** (and related routes).
- Sends **`X-MGM-ApiKey`** matching **`Mgm:ApiKey`** on the Backend.
- Injects WoW commands (e.g. **`NotifyWinnerWhisper`**, **`ReceiveGold`**) per **`docs/SPEC.md`** §8.
- Tails **`WoWChatLog.txt`** for **`[MGM_WHO]`**, **`[MGM_ACCEPT:UUID]`**, **`[MGM_CONFIRM:UUID]`**.

### 4.2 Install

- **Future:** a signed installer or zip from releases.
- **Today:** build from source with the .NET SDK (see **[SETUP-for-developer.md](SETUP-for-developer.md)**):

  ```bash
  dotnet run --project src/MimironsGoldOMatic.Desktop
  ```

Configure the **EBS base URL** and **API key** in the app settings when the settings UI exists (MVP-4 — see readiness doc).

Component doc: **`docs/MimironsGoldOMatic.Desktop/ReadME.md`**.

---

## 5. Backend (EBS) — what the streamer must know

You do **not** run PostgreSQL on the gaming PC if the API is hosted in the cloud. You **do** need:

- A stable **HTTPS** URL for the Extension and EventSub webhook.
- Secrets configured on the server (**Extension** secret, **EventSub** secret, **Helix** app id, **broadcaster** token and user id) as documented in **[SETUP-for-developer.md](SETUP-for-developer.md)** §4.4.

The Desktop and Extension must point at the **same** EBS URL (different auth: JWT vs API key).

---

## 6. Troubleshooting pointers

| Symptom | Check |
|--------|--------|
| Extension API **401** | Extension JWT / **`ExtensionClientId`** / **`ExtensionSecret`** on EBS. |
| Chat enrollments never arrive | EventSub subscription to **`channel.chat.message`**, public **`/api/twitch/eventsub`** URL, **`EventSubSecret`** match, Twitch subscription **enabled**. |
| No “reward sent” line in Twitch chat | **`BroadcasterAccessToken`**, **`BroadcasterUserId`**, **`HelixClientId`**, token scopes for Send Chat Message. |
| Desktop never marks **Sent** | **`WoWChatLog.txt`** path, addon printed **`[MGM_CONFIRM:UUID]`**, Desktop tail running, same **`ApiKey`**. |

---

## 7. Further reading

- **`docs/SPEC.md`** — full MVP contracts.
- **`docs/UI_SPEC.md`** — panel layouts (Extension, Desktop, addon).
- **`docs/INTERACTION_SCENARIOS.md`** — scenarios and test cases.
- **`SETUP-for-developer.md`** — filling `appsettings` and running services locally.

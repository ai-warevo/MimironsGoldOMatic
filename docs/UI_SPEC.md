<!-- Updated: 2026-04-05 -->

# Mimiron's Gold-o-Matic — Detailed UI/UX Specification

This document describes **user-facing** interfaces for the MVP. It is **not** source code.

**Implementation note:** **MVP-5** viewer panel (**UI-101–106**) is implemented in `src/MimironsGoldOMatic.TwitchExtension` (Zustand + EBS polling). **MVP-4** Desktop (**UI-301–308** subset) and **MVP-3** WoW addon (**UI-401–405** subset) are implemented under `src/`; compare `docs/IMPLEMENTATION_READINESS.md` for polish / optional items (e.g. UI-405). Broadcaster dashboard **UI-201–204** remains **post-MVP**.

## Related documentation

Keep this UI spec aligned with the rest of the repo (paths relative to this file in `docs/`):

| Area | Markdown |
|------|----------|
| Project overview | [README.md](../README.md) |
| Architecture summary | [CONTEXT.md](../CONTEXT.md) |
| Agent workflow & roles | [AGENTS.md](../AGENTS.md) |
| Docs index (DDD/CQRS/stack) | [ReadME.md](ReadME.md) |
| **Canonical MVP contracts** | [SPEC.md](SPEC.md) |
| MVP prompts & sequencing | [ROADMAP.md](ROADMAP.md) |
| Interaction scenarios & TCs | [INTERACTION_SCENARIOS.md](INTERACTION_SCENARIOS.md) |
| Shared contracts | [MimironsGoldOMatic.Shared/ReadME.md](MimironsGoldOMatic.Shared/ReadME.md) |
| Backend API | [MimironsGoldOMatic.Backend/ReadME.md](MimironsGoldOMatic.Backend/ReadME.md) |
| WPF Desktop / WinAPI | [MimironsGoldOMatic.Desktop/ReadME.md](MimironsGoldOMatic.Desktop/ReadME.md) |
| Twitch Extension | [MimironsGoldOMatic.TwitchExtension/ReadME.md](MimironsGoldOMatic.TwitchExtension/ReadME.md) |
| WoW addon | [MimironsGoldOMatic.WoWAddon/ReadME.md](MimironsGoldOMatic.WoWAddon/ReadME.md) |
| Implementation readiness | [IMPLEMENTATION_READINESS.md](IMPLEMENTATION_READINESS.md) |

**Global product rules (UI copy must reflect these):**

- Viewers **must subscribe** and type **`!twgold <CharacterName>`** in **broadcast stream chat** to join the pool (**`!twgold`** prefix **case-insensitive**; **CharacterName** = server nickname; **unique** among pool participants). **Gold is not paid instantly** on enroll. Channel Points are **not** used.
- A **visual roulette** runs on a **fixed 5-minute** cadence (**no** early or off-schedule spin). The **next spin** instant is **server-authoritative**; the Extension **must** show a **countdown / timer** using **`GET /api/roulette/state`** (`docs/SPEC.md` §5.1, §11). **Non-winners stay** in the pool; **winners are removed when `Sent`** and may **re-enter** with **`!twgold <CharacterName>`** again.
- **Winners** are **online-verified** via **`/who`** before **`Pending` payout**; Extension shows **“You won”**; **in WoW**, winner gets the **notification whisper** and replies **`!twgold`** (**case-insensitive**; `docs/SPEC.md` §5, §9).
- **Fixed 1,000g** per winning payout (`docs/SPEC.md` §2). **Desktop** holds **`X-MGM-ApiKey`**; Extension uses Twitch EBS/JWT to **Backend** (not the Desktop secret).

> ⚠️ **DECISION (locked):** Streamer **Extension dashboard** screens (**UI-201–204**) are **out of MVP-5** — implement **viewer** panels **UI-101–106** only (`docs/ROADMAP.md` MVP-5). Layouts below remain **reference** for post-MVP when broadcaster JWT routes exist; MVP gold stays fixed per **`docs/SPEC.md`**.

---

## 1. Twitch Extension — Viewer-facing panel

**Constraints:** Twitch **panel** iframe is typically **~318px** max width, variable height. Extension runs in **sandboxed** iframe; only Twitch-provided extension helper + EBS calls.

> ⚠️ **DECISION:** Inner ASCII blocks use **~40 character** content width as a monospace stand-in for 318px CSS (`font-size` ~11–13px).

---

## UI-101: Viewer Panel — Loading / Unauthenticated / Restricted

**Component:** Frontend (Twitch Extension)  
**Actor:** Viewer (or System)  
**Trigger:** Extension mounted; JWT not ready; viewer denied; or status unavailable.  
**MVP Stage:** MVP-5

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-101-01 | Static text | "Mimiron's Gold-o-Matic" | default / dimmed | none |
| EL-101-02 | Static text | Status line (see states) | loading / error | none |
| EL-101-03 | Button | [ Retry ] | default / disabled | re-init `window.Twitch.ext` context, refetch auth |
| EL-101-04 | Static text | Hint: "Subscribe and type !twgold <name> in chat" | shown when auth OK; enrollment is **chat**, not the panel | none |

### States

- **Default (loading):** Twitch helper loading; spinner or gnomish gears metaphor.
- **Unauthenticated:** Viewer has not granted required identity — show EL-101-02 explaining log in to Twitch / open in live channel.
- **Error (Extension / EBS):** EL-101-02 shows friendly failure; EL-101-03 enabled.
- **Authenticated:** EL-101-04 visible; transition to UI-102 (instructions + live status).

### ASCII Visualization

```
/* ~318px panel, content width ~40 "cols"                                    */
╔══════════════════════════════════════════╗
║  Mimiron's Gold-o-Matic                  ║
║  ┌────────────────────────────────────┐  ║
║  │  ● ● ●   Loading gnome gears…      │  ║  ← EL-101-02 (loading)
║  └────────────────────────────────────┘  ║
╚══════════════════════════════════════════╝
```

```
╔══════════════════════════════════════════╗
║  Mimiron's Gold-o-Matic                  ║
║  ┌────────────────────────────────────┐  ║
║  │ Sign in to Twitch to use this      │  ║
║  │ panel on a live channel.           │  ║
║  └────────────────────────────────────┘  ║
║           [ Retry ]                     ║  ← EL-101-03
╚══════════════════════════════════════════╝
```

```
╔══════════════════════════════════════════╗
║  Mimiron's Gold-o-Matic                  ║
║  ┌────────────────────────────────────┐  ║
║  │ Gnomish machinery has jammed!      │  ║  ← Error boundary / API
║  │ Try again in a moment.             │  ║
║  └────────────────────────────────────┘  ║
║           [ Retry ]                     ║
╚══════════════════════════════════════════╝
```

### Transitions

- Default (loading) → Authenticated: JWT + channel context ready → show UI-102.
- Loading → Error: EBS timeout → show error copy + Retry.
- Unauthenticated → Authenticated: viewer action in parent Twitch UI (outside iframe) → extension receives `onAuthorized`.

### Constraints & Notes

- **Twitch:** `window.Twitch.ext` lifecycle; opaque tokens; CORS/EBS only.
- **A11y:** Status text exposed as live region for loading/error.
- **i18n:** All strings in string table (future).

---

## UI-102: Viewer Panel — How to join / Pool status (chat enrollment)

**Component:** Frontend  
**Actor:** Viewer  
**Trigger:** Authenticated; panel shows **instructions** and **polls** Backend for pool/spin state (enrollment happens in **Twitch chat**, not here).  
**MVP Stage:** MVP-5

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-102-01 | Static text | Title | default | none |
| EL-102-02 | Static text | **How to join** block: subscribe + `!twgold <CharacterName>` in **chat** | default | none |
| EL-102-03 | Static text | Realm hint | default | *"Same realm as the streamer’s WoW character"* (`docs/SPEC.md`) |
| EL-102-04 | Static text | **Uniqueness** note: one character name per pool slot | default | none |
| EL-102-05 | Static text | Pool size / "You're in the pool" (from API if viewer is in pool) | default | none |
| EL-102-06 | Link-like button | [ View rules ▼ ] | collapsed / expanded | toggles accordion |

> ⚠️ **DECISION:** Optional **Dev Rig** control: hidden **POST** `/api/payouts/claim` test button — **not** shown in production MVP copy.

### States

- **Default:** Instructions visible; EL-102-05 from **`GET`** pool/me if implemented.
- **In pool:** EL-102-05 shows membership; roulette region active (UI-106 + animation area).
- **API error:** UI-105 variant.

### ASCII Visualization

```
╔══════════════════════════════════════════╗
║  Gold Pool  (viewer)                     ║
║  To join: subscribe, then type in       ║
║  stream chat: !twgold YourName           ║  ← EL-102-02
║  (Name must be unique in the pool.)    ║  ← EL-102-04
║  Same realm as streamer (MVP).         ║  ← EL-102-03
║  Pool: 12 gnomes    EL-102-05           ║
╚══════════════════════════════════════════╝
```

### Transitions

- Authenticated → In pool: Backend reports membership → show EL-102-05 + roulette.
- Pool → Error: API error body → UI-105 variant.

### Constraints & Notes

- **Enrollment** is **`!twgold <CharacterName>`** in **broadcast chat**; Backend ingests (`docs/SPEC.md` §5).
- Debounce only applies to **optional** Dev Rig claim button, not chat.

---

## UI-103: Viewer Panel — Awaiting / Spinning / Verifying

**Component:** Frontend  
**Actor:** System + Viewer  
**Trigger:** Enrolled; spin started; or `/who` verification in progress.  
**MVP Stage:** MVP-5

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-103-01 | Roulette visual | Animated wheel / list highlight | idle / spinning / verifying | decorative |
| EL-103-02 | Static text | "Checking if winner is online…" | `/who` phase | none |
| EL-103-03 | Progress | ████░░░░ (indeterminate) | loading | none |

### States

- **Spinning:** EL-103-01 animates; suspense ~ UX-defined duration synced to Backend.
- **Verifying:** EL-103-02 visible when **`spinPhase`** is **`verification`** (`docs/SPEC.md` §5.1, §11).
- **Idle between spins:** EL-103-01 static; countdown in UI-106.

### ASCII Visualization

```
╔══════════════════════════════════════════╗
║  ★ ★   ROULETTE   ★ ★                    ║
║       .───────────────.                  ║
║      ╱  Norinn  Kael  ▶╲   ← EL-103-01   ║
║     ╱    ░░░ spin ░░░   ╲               ║
║      ╲_________________╱                ║
║  Checking if winner is online…  EL-103-02 ║
║  ████████░░░░  (busy)           EL-103-03 ║
╚══════════════════════════════════════════╝
```

### Transitions

- Spinning → Verifying: **`GET /api/roulette/state`** exposes **`spinPhase`** (`verification`, etc.; `docs/SPEC.md` §5.1).
- Verifying → Winner / No-op: re-draw if offline per spec → return to idle + toast (DECISION: inline message "Redraw—winner offline").

### Constraints & Notes

- Heavy animation: respect **`prefers-reduced-motion`** (DECISION).

---

## UI-104: Viewer Panel — You won / Payout progressing / Sent

**Component:** Frontend  
**Actor:** Viewer  
**Trigger:** Backend marks viewer as winner; `GET /api/payouts/my-last` returns payout row.  
**MVP Stage:** MVP-5

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-104-01 | Banner | "YOU WON!" | pulse / static | none |
| EL-104-02 | Static text | In **WoW**, reply to the streamer’s whisper with **`!twgold`** (exact) to consent | default | selectable copy |
| EL-104-03 | Status chip | Pending / In progress / Sent | colors | poll-driven |
| EL-104-04 | Static text | **Sent** explainer (Russian, normative in `docs/SPEC.md` §11) | default | none |
| EL-104-05 | Static text (optional) | Note that stream chat may show the public announcement line | default | none |

### States

- **Won — Pending:** After **in-game** notification whisper; waiting for viewer **`!twgold`** whisper reply (and streamer mail flow).
- **In progress:** Desktop `InProgress`; mail prep.
- **Sent:** Mail confirmed via **`[MGM_CONFIRM:UUID]`** after **`MAIL_SEND_SUCCESS`** on the **MGM-armed** send (`docs/SPEC.md` §9). Show the **exact** Russian line (hardcoded in Extension), with **`WINNER_NAME`** = viewer’s enrolled **`CharacterName`**:

  `Награда отправлена персонажу <WINNER_NAME> на почту, проверяй ящик!`

  The **same** template (same placeholder) is used for the **broadcast Twitch chat** announcement when **`Sent`** is applied (`docs/SPEC.md` §11). In WoW, the winner also receives a **private** completion whisper from the addon (different Russian text, §9).

### ASCII Visualization

```
╔══════════════════════════════════════════╗
║  ┌────────────────────────────────────┐  ║
║  │   ★  YOU WON!  ★                   │  ║ ← EL-104-01
║  └────────────────────────────────────┘  ║
║  In WoW, whisper back (confirm):       ║
║  ┌────────────────────────────────────┐  ║
║  │  !twgold     ← copy                │  ║ ← EL-104-02
║  └────────────────────────────────────┘  ║
║  Status: [ In progress ●●●○○ ]           ║ ← EL-104-03
║  When Sent: mail with 1,000g on its way.  ║
╚══════════════════════════════════════════╝
```

```
╔══════════════════════════════════════════╗
║  You won — delivery complete!             ║
║  Status: [  Sent ✓  ]        EL-104-03    ║
║  Награда отправлена персонажу Norinn      ║
║  на почту, проверяй ящик!    ← EL-104-04  ║
╚══════════════════════════════════════════╝
```

### Transitions

- Won → In progress: poll `my-last` status from Backend.
- In progress → Sent: poll shows `Sent`.
- Sent → (stay): show success summary; **winner removed from pool** per `docs/SPEC.md` — rejoin with **`!twgold <CharacterName>`** in chat if they want another draw.

### Constraints & Notes

- Do **not** show per-request **gold cooldown** as primary UX — MVP uses **roulette** + **lifetime cap**; optional rate-limit messaging in UI-105.

---

## UI-105: Viewer Panel — Error States

**Component:** Frontend  
**Actor:** Viewer  
**Trigger:** API errors; rate limit; cap; active payout.  
**MVP Stage:** MVP-5

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-105-01 | Icon + text | Error title | by code | none |
| EL-105-02 | Body copy | Mapped from `code` | | none |
| EL-105-03 | Button | [ Dismiss ] / [ Retry ] | | |

### States (error variants)

| API `code` (SPEC §5) | Title | Body (example) |
|----------------------|-------|----------------|
| `invalid_character_name` | Invalid name | Fix your character name and try again. |
| `lifetime_cap_reached` | Gnome vault full | You've reached the 10,000g lifetime cap for this channel product. |
| `active_payout_exists` | Finish current win | You already have an active payout; check status above. |
| `unauthorized` | Session expired | Refresh the panel or re-open the stream. |
| (network) | Can't reach gnomes | EBS unreachable — Retry later. |

### ASCII Visualization

```
╔══════════════════════════════════════════╗
║  ┌────────────────────────────────────┐  ║
║  │ ⚠  Gnome vault full                │  ║
║  │ Lifetime cap reached (10,000g).     │  ║
║  └────────────────────────────────────┘  ║
║            [ OK ]                        ║
╚══════════════════════════════════════════╝
```

### Transitions

- Any screen → UI-105: failed API → show mapped variant.
- UI-105 → UI-102: Dismiss + valid state.

### Constraints & Notes

- Error boundary **"Gnomish machinery has jammed!"** per TwitchExtension ReadME — global catch-all.

---

## UI-106: Viewer Panel — Next spin countdown widget

**Component:** Frontend  
**Actor:** Viewer  
**Trigger:** Enrolled; between spins.  
**MVP Stage:** MVP-5

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-106-01 | Timer | `MM:SS` | running | none |

### States

- **Countdown:** Default 5:00 from Backend sync (`docs/SPEC.md`).

### ASCII Visualization

```
┌──────────────────────────────────────────┐
│  Next spin in   04:32  ← EL-106-01        │
│  ████████████░░░░░                      │
└──────────────────────────────────────────┘
```

### Transitions

- Countdown → 00:00: trigger spin UX (UI-103).

### Constraints & Notes

> ⚠️ **DECISION:** Countdown **drift** corrected on each `GET` poll from Backend to avoid client-only skew.

---

## 2. Twitch Extension — Streamer / broadcaster config

> ⚠️ **DECISION:** MVP **`docs/SPEC.md`** does **not** define Extension-only configuration of **`X-MGM-ApiKey`** (secret is on **Desktop**). UI-201–204 describe **optional** broadcaster views when Backend adds **`role: broadcaster`** JWT routes — **or** thin panels that deep-link to Desktop documentation.

---

## UI-201: Broadcaster — Initial setup / connection overview

**Component:** Frontend (Extension **config** or **live config** view)  
**Actor:** Streamer  
**Trigger:** First open of extension configuration in Creator Dashboard.  
**MVP Stage:** MVP-5+ (requires product decision)

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-201-01 | Static text | Setup checklist | default | none |
| EL-201-02 | Static text | "Install Mimiron's Gold-o-Matic Desktop" | | |
| EL-201-03 | Read-only | Backend EBS URL status | ok / fail | test ping (DECISION) |
| EL-201-04 | Button | [ Open setup docs ] | | opens README / docs URL |

### States

- **Default:** Checklist not complete.
- **Healthy:** EL-201-03 green indicator (DECISION: health endpoint).

### ASCII Visualization (config view often wider than 318px)

```
╔════════════════════════════════════════════════════╗
║  Mimiron's Gold-o-Matic — Broadcaster Setup        ║
║  ┌──────────────────────────────────────────────┐║
║  │ [x] Add Extension to panel                   │║
║  │ [ ] Install Desktop app + enter API key      │║
║  │ [ ] Confirm chat bot / EventSub for !twgold    │║
║  └──────────────────────────────────────────────┘║
║  EBS status:  (●) OK     EL-201-03               ║
║  [ Open setup docs ]                             ║
╚════════════════════════════════════════════════════╝
```

### Transitions

- N/A checklist state machine (documentation-driven).

### Constraints & Notes

- **Twitch:** Config view runs in different context than panel; test in Dev Rig **Live Config**.

---

## UI-202: Broadcaster — Session dashboard (queue / activity)

**Component:** Frontend  
**Actor:** Streamer  
**Trigger:** Broadcaster opens live config or dedicated dashboard route.  
**MVP Stage:** MVP-5+ (needs broadcaster API)

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-202-01 | List | Recent spins / winners | scroll | |
| EL-202-02 | List | Open payouts (Pending) | empty / items | |
| EL-202-03 | Button | [ Refresh ] | | poll Backend |

> ⚠️ **DECISION:** Data source: **GET** pool/spin/pending aggregated for `broadcaster_id` — **not** in `docs/SPEC.md` yet; placeholder contract.

### ASCII Visualization

```
╔════════════════════════════════════════════════════╗
║  Session Dashboard                    [ Refresh ]║
║  ┌─ Recent winners ────────────────────────────┐ ║
║  │ 04:12  Norinn   Pending                    │ ║
║  │ 04:07  Kaeldan  Sent                       │ ║
║  └────────────────────────────────────────────┘ ║
╚════════════════════════════════════════════════════╝
```

### Constraints & Notes

- Read-only unless UI-204 overrides exist.

---

## UI-203: Broadcaster — Settings

**Component:** Frontend  
**Actor:** Streamer  
**Trigger:** Settings tab.  
**MVP Stage:** MVP-5+

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-203-01 | Display | Gold per win: **1,000g** | read-only MVP | |
| EL-203-02 | Display | Spin interval: **5 min** | read-only MVP | |
| EL-203-03 | Checkbox | [ ] Pause all spins | on/off | **DECISION** (no SPEC pause API) |

> ⚠️ **DECISION:** MVP keeps gold & interval **read-only** per `docs/SPEC.md` §2; toggles become active in post-MVP when Backend supports them.

### ASCII Visualization

```
╔════════════════════════════════════════════════════╗
║  Settings                                          ║
║  Gold per winning payout:  1,000g  (MVP fixed)     ║
║  Spin interval:            5:00    (MVP fixed)     ║
║  [ ] Pause distribution  (coming soon)              ║
╚════════════════════════════════════════════════════╝
```

---

## UI-204: Broadcaster — Manual override

**Component:** Frontend  
**Actor:** Streamer  
**Trigger:** Operator needs cancel / nudge.  
**MVP Stage:** MVP-5+

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-204-01 | List row actions | [ Cancel payout ] | Pending only | `PATCH` Cancelled (via EBS proxy + Desktop policy DECISION) |
| EL-204-02 | Button | [ Force mark sent ] | **Not recommended** | DECISION: prefer Desktop for `Sent` integrity |

> ⚠️ **DECISION:** **`PATCH`** with **`X-MGM-ApiKey`** is **Desktop** in MVP; broadcaster Extension would call **EBS** which uses **server-side secret** — **do not** expose raw ApiKey in Extension. Override UI should proxy through Backend with **broadcaster role**.

### ASCII Visualization

```
╔════════════════════════════════════════════════════╗
║  Overrides (advanced)                            ║
║  Row: abc123…  Norinn  [ Cancel ]                 ║
║  ⚠ Use Desktop for Mark Sent (log integrity)      ║
╚════════════════════════════════════════════════════╝
```

---

## 3. WPF Desktop Application

**MVP Stage:** MVP-4

---

## UI-301: Desktop — API configuration / “login”

**Component:** Desktop  
**Actor:** Streamer  
**Trigger:** First launch or settings.  
**MVP Stage:** MVP-4

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-301-01 | Input | [ API base URL________ ] | | save local settings |
| EL-301-02 | Password box | [ X-MGM-ApiKey________ ] | | secure storage DECISION |
| EL-301-03 | Button | [ Test connection ] | loading | `GET /api/payouts/pending` |
| EL-301-04 | Button | [ Save ] | | persist |

### States

- **Default:** Empty or saved values.
- **Success:** Toast "Connected".
- **Error:** `403` forbidden_apikey messaging.

### ASCII Visualization

```
╔════════════════════════════════════════════╗
║  Mimiron's Desktop — API Setup       [✕]  ║
║  ┌──────────────────────────────────────┐  ║
║  │ API Base URL                         │  ║
║  │ [https://api.example.com_____]       │  ║
║  │ X-MGM-ApiKey                         │  ║
║  │ [••••••••••••••••••••]                │  ║
║  └──────────────────────────────────────┘  ║
║   [ Test connection ]   [ Save ]          ║
╚════════════════════════════════════════════╝
```

### Transitions

- Save → close → UI-302/303.

### Constraints & Notes

- Store secrets with **DPAPI** or credential manager (DECISION: implementation).

---

## UI-302: Desktop — Main window idle (WoW not connected)

**Component:** Desktop  
**Actor:** Streamer  
**Trigger:** No `WoW.exe` / foreground target.  
**MVP Stage:** MVP-4

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-302-01 | Status bar | "Searching for WoW…" | idle / error | |
| EL-302-02 | Button | [ Refresh detection ] | | rescan process |
| EL-302-03 | Menu | File / Settings / View log | | opens UI-306, UI-308 |

### ASCII Visualization

```
╔══════════════════════════════════════════════════════╗
║  Mimiron's Gold-o-Matic  v0.1.0    [_] [□] [✕]     ║
╠══════════════════════════════════════════════════════╣
║  Status: Searching for WoW (foreground)…  EL-302-01 ║
║  ┌────────────────────────────────────────────────┐║
║  │   ( illustration: sleeping gnome / no WoW )     │║
║  └────────────────────────────────────────────────┘║
║  [ Refresh detection ]                             ║
╠══════════════════════════════════════════════════════╣
║  File   Settings   View log              UI-302-03 ║
╚══════════════════════════════════════════════════════╝
```

### Transitions

- Idle → Active: process found (`docs/MimironsGoldOMatic.Desktop` state machine).

---

## UI-303: Desktop — Main window active (WoW connected)

**Component:** Desktop  
**Actor:** Streamer  
**Trigger:** Foreground `WoW.exe` found.  
**MVP Stage:** MVP-4

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-303-01 | Status | Process name + PID | | |
| EL-303-02 | Button | [ Sync / Inject ] | disabled until Ready | flow § Desktop ReadME |
| EL-303-03 | Indicator | Mailbox / Ready | per state machine | |

### ASCII Visualization

```
╔══════════════════════════════════════════════════════╗
║  Mimiron's Gold-o-Matic  v0.1.0    [_] [□] [✕]     ║
╠══════════════════════════════════════════════════════╣
║  WoW: HIGH  PID:12345   ✓ Ready to Inject   EL-303 ║
║  ┌─ Queue (summary) ───────────────────────────────┐ ║
║  │ 3 Pending   0 In progress                      │ ║
║  └────────────────────────────────────────────────┘ ║
║  [ Sync / Inject ]  ← EL-303-02                     ║
╠══════════════════════════════════════════════════════╣
║  Last log: ACK Norinn  !twgold  @ 04:14:02         ║
╚══════════════════════════════════════════════════════╝
```

### Transitions

- Ready → Waiting for mailbox: game state change.
- `Sync/Inject` → `PATCH InProgress` + WinAPI inject.

### Constraints & Notes

- **WinAPI:** `PostMessage` primary; `SendInput` fallback — show active strategy in status (DECISION).

---

## UI-304: Desktop — Request queue panel (winner payouts)

**Component:** Desktop  
**Actor:** Streamer  
**Trigger:** Embedded in main or side panel.  
**MVP Stage:** MVP-4

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-304-01 | DataGrid | Columns: Id, Toon, Gold, Status, Accepted? | | sort |
| EL-304-02 | Button row | Mark Failed / Cancel / Mark Sent | per row rules | `PATCH` |
| EL-304-03 | Filter | ▼ All / Pending / In progress | | |

### ASCII Visualization

```
┌─ Payout queue ─────────────────────────────────────┐
│ Filter: [ All                           ▼ ] EL-304-03│
│ ┌─────┬────────┬──────┬─────────┬─────────────────┐ │
│ │ ID  │ Toon   │ Gold │ Status  │ Actions         │ │
│ ├─────┼────────┼──────┼─────────┼─────────────────┤ │
│ │ a1..│ Norinn │1000g │Pending  │[Fail][Cancel]  │ │
│ │ b2..│ Kael   │1000g │InProg   │[Fail][Mk Sent] │ │
│ └─────┴────────┴──────┴─────────┴─────────────────┘ │
└────────────────────────────────────────────────────┘
```

### Transitions

- Row status updates from poll + **`WoWChatLog.txt`** watcher (**`MGM_ACCEPT`** / **`MGM_CONFIRM`**).

---

## UI-305: Desktop — WoW connection status bar

**Component:** Desktop  
**Actor:** System  
**Trigger:** Always visible (footer or top).  
**MVP Stage:** MVP-4

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-305-01 | Pill | Process | found / missing | |
| EL-305-02 | Pill | Mailbox | open / closed | |
| EL-305-03 | Pill | API | online / degraded | |

### ASCII Visualization

```
╠══════════════════════════════════════════════════════╣
║ WoW ●  Mailbox ○  API ●     Last inject: 04:14:01   ║
╚══════════════════════════════════════════════════════╝
```

---

## UI-306: Desktop — Settings window

**Component:** Desktop  
**Actor:** Streamer  
**Trigger:** Menu → Settings.  
**MVP Stage:** MVP-4

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-306-01 | Input | Polling interval (sec) | default 15 DECISION | |
| EL-306-02 | Dropdown | Injection: PostMessage / SendInput | | |
| EL-306-03 | Input | Polly retry count | | |
| EL-306-04 | File picker | WoW `Logs` path override | optional DECISION | |

### ASCII Visualization

```
╔══════════════════════════════╗
║  Settings             [✕]    ║
║  Poll interval: [ 15 ] sec   ║
║  Input strategy: [PostMsg▼] ║
║  Retries: [ 3 ]              ║
║  WoW log dir: [ Browse… ]    ║
║           [ OK ]  [ Cancel ] ║
╚══════════════════════════════╝
```

---

## UI-307: Desktop — Error / alert modal

**Component:** Desktop  
**Actor:** System  
**Trigger:** API unreachable; WoW not found; injection failure.  
**MVP Stage:** MVP-4

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-307-01 | Text | Title / body | | |
| EL-307-02 | Button | [ Retry ] [ Exit ] | | |

### ASCII Visualization

```
        ┌────────────────────────┐
        │ ⚠  API unreachable     │
        │ Polly retries failed.   │
        │  [ Retry ]  [ Exit ]   │
        └────────────────────────┘
```

### Transitions

- Modal blocks interaction modal-DLG (DECISION).

---

## UI-308: Desktop — Delivery / event log

**Component:** Desktop  
**Actor:** Streamer  
**Trigger:** Menu View log.  
**MVP Stage:** MVP-4

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-308-01 | ListBox | Timestamped lines | scroll | copy line |
| EL-308-02 | Button | [ Clear ] | | |
| EL-308-03 | Button | [ Export ] | | |

### ASCII Visualization

```
╔════════════════════════════════════════════════════╗
║  Event log                              [ Clear ]  ║
║  ┌──────────────────────────────────────────────┐ ║
║  │ 04:14:01  MGM_CONFIRM match a1b2c3d…         │ ║
║  │ 04:13:40  MGM_ACCEPT match → confirm-accept │ ║
║  │ 04:13:05  PostMessage inject OK              │ ║
║  └──────────────────────────────────────────────┘ ║
╚════════════════════════════════════════════════════╝
```

---

## 4. WoW 3.3.5a Addon UI

**MVP Stage:** MVP-3  
**Widgets:** `Frame`, `Button`, `EditBox`, `FontString`, `Texture`, `ScrollingMessageFrame` or scroll frame pattern.

> ⚠️ **WOW CONSTRAINT:** No arbitrary HTML; all layout via **`SetPoint`** anchors; **XML** or runtime CreateFrame only; **Interface: 30300**.

---

## UI-401: Addon — Entry point (minimap / slash / launcher)

**Component:** Addon  
**Actor:** Streamer  
**Trigger:** Player logs in.  
**MVP Stage:** MVP-3

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-401-01 | Minimap icon | Texture button | hover | toggle UI-402 |
| EL-401-02 | Slash | `/mgm` (DECISION) | | opens UI-402 |

> ⚠️ **DECISION:** **`docs/MimironsGoldOMatic.WoWAddon`** emphasizes **MAIL_SHOW** side panel; minimap button is **standard** but optional if slash-only.

### Frame props

| Property | Value |
|----------|--------|
| Strata | **MEDIUM** |
| EnableMouse | **true** (button) |
| Anchors | Minimap cluster DECISION or standalone TOPRIGHT |

### ASCII Visualization

```
        [☆]  ← EL-401-01 on minimap rim
```

### States

- Hidden in combat (DECISION) vs always show — **DECISION:** hide in combat to reduce taint risk.

---

## UI-402: Addon — Main side panel (MAIL_SHOW)

**Component:** Addon  
**Actor:** Streamer  
**Trigger:** **`MAIL_SHOW`** or `/mgm`.  
**MVP Stage:** MVP-3

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-402-01 | FontString | Title "MGM Queue" | | |
| EL-402-02 | ScrollFrame + rows | Recipient list | READY/PROC/SENT | click row |
| EL-402-03 | Button | [ Prepare Mail ] | enabled when row READY | fills SendMail* boxes |
| EL-402-04 | FontString | Hint: open mailbox | | |

### Frame props

| Property | Value |
|----------|--------|
| Strata | **HIGH** (above world, below dialogs DECISION) |
| EnableMouse | **true** |
| Anchors | **RIGHT** of `MailFrame` or **LEFT** `MailFrame`, `TOPRIGHT` offset (`SetPoint`) |

### ASCII Visualization (beside mail frame — conceptual)

```
  ┌─ Mail (vanilla) ─────┐   ┌─ MGM Queue ────────┐
  │                      │   │ Norinn  READY  [▶]│
  │  To: _________       │   │ Kael   SENT       │
  │                      │   │ [ Prepare Mail ]  │
  └──────────────────────┘   └───────────────────┘
```

### States

- **Mailbox closed:** EL-402-04 "Open mailbox".
- **Queue empty:** empty state string.

### ASCII — Mailbox closed

```
╔══════════════════════╗
║ MGM Queue            ║
║ (Open mailbox first)║
╚══════════════════════╝
```

> ⚠️ **WOW CONSTRAINT:** Tainted execution paths — do not hook secure buttons without care; **Prepare Mail** should only touch **SendMail** frames when allowed in 3.3.5a.

---

## UI-403: Addon — Success toast (mail sent / tag emitted)

**Component:** Addon  
**Actor:** System  
**Trigger:** After successful send + `[MGM_CONFIRM:…]` printed.  
**MVP Stage:** MVP-3

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-403-01 | Frame + text | "Mail sent — Norinn" | fade | click dismiss |

### Frame props

| Strata | **TOOLTIP** (top visibility) |
| EnableMouse | true |
| Anchors | `CENTER` UIParent, `Y` offset positive |

### ASCII Visualization

```
              ┌─────────────────────┐
              │ ✓ Mail sent (Norinn)│
              └─────────────────────┘
                     ↑ toast
```

---

## UI-404: Addon — Error toast

**Component:** Addon  
**Actor:** System  
**Trigger:** Mailbox closed; validation fail; no gold.  
**MVP Stage:** MVP-3

### ASCII Visualization

```
              ┌─────────────────────┐
              │ ✕ Mailbox closed    │
              └─────────────────────┘
```

### Frame props

Strata **HIGH**; short duration (~3s).

---

## UI-405: Addon — Debug / log frame (streamer-only toggle)

**Component:** Addon  
**Actor:** Streamer  
**Trigger:** `/mgm debug` (DECISION) or checkbox in UI-402.  
**MVP Stage:** MVP-3 (optional)

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-405-01 | ScrollingMessageFrame | Lines | | scroll |
| EL-405-02 | Checkbox | [ x ] Debug | | |

### Frame props

Strata **LOW** (don't block play); **Movable** (DECISION).

### ASCII Visualization

```
╔══════════════════════╗
║ MGM Debug      [✕]   ║
║ ┌──────────────────┐ ║
║ │ who: Norinn OK   │ ║
║ │ whisper hook on  │ ║
║ └──────────────────┘ ║
╚══════════════════════╝
```

> ⚠️ **DECISION:** Strip or noop in **release** build for performance.

---

## Navigation Flow

### Viewer (Twitch Extension panel)

```
UI-101 ──[auth ready]──→ UI-102 ──[enroll OK]──→ UI-106 (countdown)
                              │                        │
                              │                        └──→ UI-103 (spin/verify)
                              │                                 │
                              │              [offline redraw]───┤
                              │                                 ▼
                              │                         UI-104 (won / status)
                              │                                 │
                              │                         [poll Sent]
                              └──[error]──→ UI-105 ──[dismiss]──→ UI-102
```

### Streamer (Extension config / dashboard — optional MVP+)

```
UI-201 ──→ UI-202 ──→ UI-203
              │
              └──→ UI-204 (advanced)
```

### Desktop App

```
UI-301 (API setup) ──→ UI-302 (idle) ──[WoW found]──→ UI-303 (active)
                              │                            │
                              │                            ├── UI-304 queue
                              │                            ├── UI-305 status bar
                              │                            └── UI-307 errors
                              └── settings ──→ UI-306
                              └── log ───────→ UI-308
```

### WoW Addon

```
UI-401 (icon/slash) ──→ UI-402 (side panel)
                              ├── EL-402-03 Prepare Mail ──→ (vanilla send)
                              ├── success ───────────────────→ UI-403
                              └── failure ───────────────────→ UI-404
        optional: /mgm debug ──→ UI-405
```

---

## Design Tokens & Shared Vocabulary

| Token | Value | Used in |
|-------|-------|---------|
| Color: brand-gold | `#FFB300` | Extension headers, EL-104-01, UI-403 border (texture) |
| Color: brand-iron | `#5D4037` | Extension gnome/industrial accents (DECISION) |
| Color: error | `#E53935` | UI-105, UI-404, UI-307 |
| Color: success | `#43A047` | UI-104 Sent chip, UI-403 |
| Panel max width | `318px` | Twitch panel (UI-101–106); config views may be wider |
| Countdown format | `MM:SS` | UI-106 (next spin) |
| Max character name length | `12` | WoW default; align **`CharacterName` validation** in Shared (DECISION if realm allows longer) |
| Gold amount display | `1,000g` | UI-104, UI-203, UI-304 |
| Whisper command | `!twgold` (exact) | UI-104, instructional copy |
| Confirm tag pattern | `[MGM_CONFIRM:<uuid>]` | UI-308 log lines, internal |
| Debounce join | `2–3 s` | UI-102 (`docs/MimironsGoldOMatic.TwitchExtension`) |
| API poll default | `15 s` (DECISION) | UI-306 |
| Font / theme | Twitch vs WPF vs WoW | Use Twitch purple only in Extension; **do not** assume WoW fonts match web |

---

## Document control

| Version | Date | Notes |
|---------|------|--------|
| 1.0 | 2026-04-03 | Initial UI spec from `docs/SPEC.md` + component ReadMEs |
| 1.1 | 2026-04-03 | Follow-gated pool; removed Channel Points + instant spin (aligned to `docs/SPEC.md`) |
| 1.2 | 2026-04-03 | Subscribe + **`!twgold <CharacterName>`** chat enroll; **`!twgold`** acceptance; remove winner on `Sent` (`docs/SPEC.md`) |
| 1.3 | 2026-04-03 | WoW **winner notification whisper** + whisper **`!twgold`** consent (`docs/SPEC.md` §9) |

When **`docs/SPEC.md`** adds concrete **pool/spin** and **broadcaster** routes, update **EL-** bindings and **UI-201–204** without changing IDs where possible.

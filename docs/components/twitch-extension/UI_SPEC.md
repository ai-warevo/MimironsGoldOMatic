<!-- This document describes the UI for the MimironsGoldOMatic Twitch Extension. For general UI guidelines (colors, typography, design tokens, cross-app navigation), see [Main UI spec](../../reference/UI_SPEC.md). -->
<!-- Updated: 2026-04-05 (File split into component-specific specs) -->

# Twitch Extension — UI specification

**Implementation:** `src/MimironsGoldOMatic.TwitchExtension` — MVP-5 **viewer** panel (**UI-101–106**) with Zustand + EBS polling. **Post-MVP reference:** broadcaster **UI-201–204** (no EBS routes in MVP). See [`IMPLEMENTATION_READINESS.md`](../../reference/IMPLEMENTATION_READINESS.md).

**Product copy (normative):** [`MVP_PRODUCT_SUMMARY.md`](../../overview/MVP_PRODUCT_SUMMARY.md), [`SPEC.md`](../../overview/SPEC.md).
**Scope note:** this file defines Twitch-facing UI behavior and copy shape, while backend lifecycle authority remains in `docs/overview/SPEC.md`.

> **DECISION (locked):** Implement **viewer UI-101–106** only in MVP-5; **UI-201–204** stay post-MVP ([`ROADMAP.md`](../../overview/ROADMAP.md)).

## Related documentation

| Area | Link |
|------|------|
| Main UI hub + tokens | [`UI_SPEC.md`](../../reference/UI_SPEC.md) |
| WPF Desktop UI | [`MimironsGoldOMatic.Desktop/UI_SPEC.md`](../desktop/UI_SPEC.md) |
| WoW addon UI | [`MimironsGoldOMatic.WoWAddon/UI_SPEC.md`](../wow-addon/UI_SPEC.md) |
| Component engineering | [`Twitch Extension component guide`](ReadME.md) |

---

## 1. Twitch Extension — Viewer-facing panel

**Constraints:** Twitch panel iframe is typically **~318px** wide (variable height). The extension runs in a sandboxed iframe and may call only Twitch helper APIs plus EBS HTTP endpoints.

> ⚠️ **DECISION:** Inner ASCII blocks use **~40 character** content width as a monospace stand-in for 318px CSS (`font-size` ~11–13px).

---

## UI-101: Viewer Panel — Loading / Unauthenticated / Restricted

**Component:** Frontend (Twitch Extension)
**Actor:** Viewer (or System)
**Trigger:** Extension mounted; JWT/context not ready; viewer unauthorized; or panel state unavailable.
**MVP Stage:** MVP-5

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-101-01 | Static text | "Mimiron's Gold-o-Matic" | default / dimmed | none |
| EL-101-02 | Static text | Status line (see states) | loading / error | none |
| EL-101-03 | Button | [ Retry ] | default / disabled | re-init `window.Twitch.ext` context, refetch auth |
| EL-101-04 | Static text | Hint: "Subscribe and type !twgold <name> in chat" | shown when auth OK; enrollment is **chat**, not the panel | none |

### States

- **Default (loading):** Twitch helper and auth context loading; show spinner/animated placeholder.
- **Unauthenticated:** Viewer identity unavailable; show guidance to sign in or open a live channel context.
- **Error (Extension/EBS):** Friendly failure text with `Retry` action enabled.
- **Authenticated:** Instruction hint appears and panel transitions to UI-102.

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
**Trigger:** Authenticated state; panel shows join instructions and polls backend pool/spin state (enrollment is in Twitch chat, not panel controls).
**MVP Stage:** MVP-5

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-102-01 | Static text | Title | default | none |
| EL-102-02 | Static text | **How to join** block: subscribe + `!twgold <CharacterName>` in **chat** | default | none |
| EL-102-03 | Static text | Realm hint | default | *"Same realm as the streamer’s WoW character"* (`docs/overview/SPEC.md`) |
| EL-102-04 | Static text | **Uniqueness** note: one character name per pool slot | default | none |
| EL-102-05 | Static text | Pool size / "You're in the pool" (from API if viewer is in pool) | default | none |
| EL-102-06 | Link-like button | [ View rules ▼ ] | collapsed / expanded | toggles accordion |

> ⚠️ **DECISION:** Optional **Dev Rig** control: hidden **POST** `/api/payouts/claim` test button — **not** shown in production MVP copy.

### States

- **Default:** Join instructions visible; EL-102-05 reflects `GET /api/pool/me` and roulette summary data when available.
- **In pool:** EL-102-05 shows membership; roulette region active (UI-106 + animation area).
- **API error:** Transition to UI-105 variant with mapped copy.

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

- Authenticated → In pool when backend membership is reported; show EL-102-05 and activate roulette/countdown widgets.
- Pool → Error when backend request fails; present UI-105 copy and recovery action.

### Constraints & Notes

- **Enrollment** is **`!twgold <CharacterName>`** in **broadcast chat**; Backend ingests (`docs/overview/SPEC.md` §5).
- Debounce only applies to **optional** Dev Rig claim button, not chat.

---

## UI-103: Viewer Panel — Awaiting / Spinning / Verifying

**Component:** Frontend
**Actor:** System + Viewer
**Trigger:** Viewer is enrolled and roulette cycle enters spinning or verification phase.
**MVP Stage:** MVP-5

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-103-01 | Roulette visual | Animated wheel / list highlight | idle / spinning / verifying | decorative |
| EL-103-02 | Static text | "Checking if winner is online…" | `/who` phase | none |
| EL-103-03 | Progress | ████░░░░ (indeterminate) | loading | none |

### States

- **Spinning:** EL-103-01 animates with backend-synchronized phase timing.
- **Verifying:** EL-103-02 appears when `spinPhase=verification` (`docs/overview/SPEC.md` §5.1, §11).
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

- Spinning → Verifying when `GET /api/roulette/state` reports `spinPhase=verification` (`docs/overview/SPEC.md` §5.1).
- Verifying → Winner / No-op: re-draw if offline per spec → return to idle + toast (DECISION: inline message "Redraw—winner offline").

### Constraints & Notes

- Heavy animation: respect **`prefers-reduced-motion`** (DECISION).

---

## UI-104: Viewer Panel — You won / Payout progressing / Sent

**Component:** Frontend
**Actor:** Viewer
**Trigger:** Backend identifies current viewer as winner and `GET /api/payouts/my-last` returns a payout row.
**MVP Stage:** MVP-5

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-104-01 | Banner | "YOU WON!" | pulse / static | none |
| EL-104-02 | Static text | In **WoW**, reply to the streamer’s whisper with **`!twgold`** (exact) to consent | default | selectable copy |
| EL-104-03 | Status chip | Pending / In progress / Sent | colors | poll-driven |
| EL-104-04 | Static text | **Sent** explainer (Russian, normative in `docs/overview/SPEC.md` §11) | default | none |
| EL-104-05 | Static text (optional) | Note that stream chat may show the public announcement line | default | none |

### States

- **Won — Pending:** After **in-game** notification whisper; waiting for viewer **`!twgold`** whisper reply (and streamer mail flow).
- **In progress:** Desktop `InProgress`; mail prep.
- **Sent:** Mail confirmed via **`[MGM_CONFIRM:UUID]`** after **`MAIL_SEND_SUCCESS`** on the **MGM-armed** send (`docs/overview/SPEC.md` §9). Show the **exact** Russian line (hardcoded in Extension), with **`WINNER_NAME`** = viewer’s enrolled **`CharacterName`**:

  `Награда отправлена персонажу <WINNER_NAME> на почту, проверяй ящик!`

  The **same** template (same placeholder) is used for the **broadcast Twitch chat** announcement when **`Sent`** is applied (`docs/overview/SPEC.md` §11). In WoW, the winner also receives a **private** completion whisper from the addon (different Russian text, §9).

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

- Won → In progress when polled payout status changes from backend.
- In progress → Sent when backend status is `Sent`.
- Sent → (stay): show success summary; **winner removed from pool** per `docs/overview/SPEC.md` — rejoin with **`!twgold <CharacterName>`** in chat if they want another draw.

### Constraints & Notes

- Do **not** show per-request **gold cooldown** as primary UX — MVP uses **roulette** + **lifetime cap**; optional rate-limit messaging in UI-105.

---

## UI-105: Viewer Panel — Error States

**Component:** Frontend
**Actor:** Viewer
**Trigger:** API errors, throttling, cap enforcement, or active payout conflicts.
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

- Any screen → UI-105 when API call fails and error is mapped.
- UI-105 → UI-102: Dismiss + valid state.

### Constraints & Notes

- Error boundary **"Gnomish machinery has jammed!"** per TwitchExtension ReadME — global catch-all.

---

## UI-106: Viewer Panel — Next spin countdown widget

**Component:** Frontend
**Actor:** Viewer
**Trigger:** Enrolled viewer state between active spins.
**MVP Stage:** MVP-5

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-106-01 | Timer | `MM:SS` | running | none |

### States

- **Countdown:** Default 5:00 from Backend sync (`docs/overview/SPEC.md`).

### ASCII Visualization

```
┌──────────────────────────────────────────┐
│  Next spin in   04:32  ← EL-106-01        │
│  ████████████░░░░░                      │
└──────────────────────────────────────────┘
```

### Transitions

- Countdown → 00:00 transitions to spin UX (UI-103), subject to backend phase updates.

### Constraints & Notes

> ⚠️ **DECISION:** Countdown **drift** corrected on each `GET` poll from Backend to avoid client-only skew.

---

## 2. Twitch Extension — Streamer / broadcaster config

> ⚠️ **DECISION:** MVP **`docs/overview/SPEC.md`** does **not** define Extension-only configuration of **`X-MGM-ApiKey`** (secret is on **Desktop**). UI-201–204 describe **optional** broadcaster views when Backend adds **`role: broadcaster`** JWT routes — **or** thin panels that deep-link to Desktop documentation.

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

> ⚠️ **DECISION:** Data source: **GET** pool/spin/pending aggregated for `broadcaster_id` — **not** in `docs/overview/SPEC.md` yet; placeholder contract.

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

> ⚠️ **DECISION:** MVP keeps gold & interval **read-only** per `docs/overview/SPEC.md` §2; toggles become active in post-MVP when Backend supports them.

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


---

## Navigation (Twitch Extension)

### Viewer (panel)

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

### Streamer (config / dashboard — optional MVP+)

```
UI-201 ──→ UI-202 ──→ UI-203
              │
              └──→ UI-204 (advanced)
```

Full cross-component diagram: [Main UI spec — Navigation Flow](../../reference/UI_SPEC.md#navigation-flow).

<!-- This document describes the WPF Desktop UI for MimironsGoldOMatic. For general UI guidelines (colors, typography, design tokens, cross-app navigation), see [Main UI spec](../../reference/UI_SPEC.md). -->
<!-- Updated: 2026-04-05 (File split into component-specific specs) -->

# WPF Desktop — UI specification

**Implementation:** `src/MimironsGoldOMatic.Desktop` — **UI-301–308** (MVP-4). WinAPI / process discovery: [`ReadME.md`](ReadME.md).

## Related documentation

| Area | Link |
|------|------|
| Main UI hub + tokens | [`UI_SPEC.md`](../../reference/UI_SPEC.md) |
| Twitch Extension UI | [`MimironsGoldOMatic.TwitchExtension/UI_SPEC.md`](../twitch-extension/UI_SPEC.md) |
| WoW addon UI | [`MimironsGoldOMatic.WoWAddon/UI_SPEC.md`](../wow-addon/UI_SPEC.md) |

---

## WPF Desktop application

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

- Idle → Active: process found (`src/MimironsGoldOMatic.Desktop` state machine).

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

## Navigation (WPF Desktop)

```
UI-301 (API setup) ──→ UI-302 (idle) ──[WoW found]──→ UI-303 (active)
                              │                            │
                              │                            ├── UI-304 queue
                              │                            ├── UI-305 status bar
                              │                            └── UI-307 errors
                              └── settings ──→ UI-306
                              └── log ───────→ UI-308
```

Full cross-component diagram: [Main UI spec — Navigation Flow](../../reference/UI_SPEC.md#navigation-flow).

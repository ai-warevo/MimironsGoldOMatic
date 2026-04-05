<!-- This document describes the WoW 3.3.5a addon UI for MimironsGoldOMatic. For general UI guidelines (colors, typography, design tokens, cross-app navigation), see [Main UI spec](../UI_SPEC.md). -->
<!-- Updated: 2026-04-05 (File split into component-specific specs) -->

# WoW 3.3.5a addon — UI specification

**Implementation:** `src/MimironsGoldOMatic.WoWAddon` — **UI-401–405** (MVP-3). Lua / FrameXML: [`ReadME.md`](ReadME.md).

## Related documentation

| Area | Link |
|------|------|
| Main UI hub + tokens | [`UI_SPEC.md`](../UI_SPEC.md) |
| Twitch Extension UI | [`MimironsGoldOMatic.TwitchExtension/UI_SPEC.md`](../MimironsGoldOMatic.TwitchExtension/UI_SPEC.md) |
| WPF Desktop UI | [`MimironsGoldOMatic.Desktop/UI_SPEC.md`](../MimironsGoldOMatic.Desktop/UI_SPEC.md) |

---

## WoW 3.3.5a addon UI

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

## Navigation (WoW addon)

```
UI-401 (icon/slash) ──→ UI-402 (side panel)
                              ├── EL-402-03 Prepare Mail ──→ (vanilla send)
                              ├── success ───────────────────→ UI-403
                              └── failure ───────────────────→ UI-404
        optional: /mgm debug ──→ UI-405
```

Full cross-component diagram: [Main UI spec — Navigation Flow](../UI_SPEC.md#navigation-flow).

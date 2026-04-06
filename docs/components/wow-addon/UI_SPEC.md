<!-- This document describes the WoW 3.3.5a addon UI for MimironsGoldOMatic. For general UI guidelines (colors, typography, design tokens, cross-app navigation), see [Main UI spec](../../reference/UI_SPEC.md). -->
<!-- Updated: 2026-04-05 (File split into component-specific specs) -->

# WoW 3.3.5a addon — UI specification

**Implementation:** `src/MimironsGoldOMatic.WoWAddon` — **UI-401–405** (MVP-3). Lua / FrameXML: [`WoW Addon component guide`](ReadME.md).
**Normative behavior source:** addon lifecycle and chat-log contract semantics are defined in [`docs/overview/SPEC.md`](../../overview/SPEC.md). This file focuses on in-game UI shape and interaction ergonomics.

## Related documentation

| Area | Link |
|------|------|
| Main UI hub + tokens | [`UI_SPEC.md`](../../reference/UI_SPEC.md) |
| Twitch Extension UI | [`MimironsGoldOMatic.TwitchExtension/UI_SPEC.md`](../twitch-extension/UI_SPEC.md) |
| WPF Desktop UI | [`MimironsGoldOMatic.Desktop/UI_SPEC.md`](../desktop/UI_SPEC.md) |

---

## WoW 3.3.5a addon UI

**MVP Stage:** MVP-3
**Widgets:** `Frame`, `Button`, `EditBox`, `FontString`, `Texture`, `ScrollingMessageFrame` or scroll frame pattern.

> ⚠️ **WOW CONSTRAINT:** No arbitrary HTML. Layout is driven by **`SetPoint`** anchors with XML and/or runtime `CreateFrame`, targeting **Interface: 30300**.

---

## UI-401: Addon — Entry point (minimap / slash / launcher)

**Component:** Addon
**Actor:** Streamer
**Trigger:** Player login/session start.
**MVP Stage:** MVP-3

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-401-01 | Minimap icon | Texture button | hover | toggle UI-402 |
| EL-401-02 | Slash | `/mgm` (DECISION) | | opens UI-402 |

> ⚠️ **DECISION:** The primary operator path is the **MAIL_SHOW** side panel; minimap icon remains optional when slash-only control is preferred.

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

- Visibility in combat is a local implementation decision; recommended default is hidden-in-combat to reduce taint risk.

---

## UI-402: Addon — Main side panel (MAIL_SHOW)

**Component:** Addon
**Actor:** Streamer
**Trigger:** **`MAIL_SHOW`** event or `/mgm` command.
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

- **Mailbox closed:** EL-402-04 prompts user to open mailbox first.
- **Queue empty:** explicit empty-state message.

### ASCII — Mailbox closed

```
╔══════════════════════╗
║ MGM Queue            ║
║ (Open mailbox first)║
╚══════════════════════╝
```

> ⚠️ **WOW CONSTRAINT:** Avoid insecure taint paths. `Prepare Mail` should mutate `SendMail*` controls only in allowed 3.3.5a contexts.

---

## UI-403: Addon — Success toast (mail sent / tag emitted)

**Component:** Addon
**Actor:** System
**Trigger:** Successful mail send followed by `[MGM_CONFIRM:…]` emission.
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
**Trigger:** Mailbox unavailable, validation failure, or insufficient gold/input constraints.
**MVP Stage:** MVP-3

### ASCII Visualization

```
              ┌─────────────────────┐
              │ ✕ Mailbox closed    │
              └─────────────────────┘
```

### Frame props

Strata **HIGH**; short-lived notification (~3s).

---

## UI-405: Addon — Debug / log frame (streamer-only toggle)

**Component:** Addon
**Actor:** Streamer
**Trigger:** `/mgm debug` command (optional) or toggle in UI-402.
**MVP Stage:** MVP-3 (optional)

### Element Inventory

| ID     | Element Type | Label/Placeholder | State variants | Action on interact |
|--------|--------------|-------------------|----------------|---------------------|
| EL-405-01 | ScrollingMessageFrame | Lines | | scroll |
| EL-405-02 | Checkbox | [ x ] Debug | | |

### Frame props

Strata **LOW** (non-blocking during gameplay); optional movable behavior.

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

> ⚠️ **DECISION:** Optional debug frame can be stripped or no-op in release profiles for performance and noise control.


---

## Navigation (WoW addon)

```
UI-401 (icon/slash) ──→ UI-402 (side panel)
                              ├── EL-402-03 Prepare Mail ──→ (vanilla send)
                              ├── success ───────────────────→ UI-403
                              └── failure ───────────────────→ UI-404
        optional: /mgm debug ──→ UI-405
```

Full cross-component diagram: [Main UI spec — Navigation Flow](../../reference/UI_SPEC.md#navigation-flow).

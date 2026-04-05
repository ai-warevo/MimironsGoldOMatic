<!-- Updated: 2026-04-05 (File split into component-specific specs) -->

# Mimiron's Gold-o-Matic — UI/UX specification (hub)

This **hub** document holds **cross-cutting** UI guidance, **design tokens**, and **end-to-end navigation** for the MVP. **Per-screen inventories, ASCII layouts, and platform constraints** live in the **component UI specs** linked at the end. This is **not** source code.

**Implementation snapshot:** **MVP-5** Twitch Extension viewer panel (**UI-101–106**) in `src/MimironsGoldOMatic.TwitchExtension`; **MVP-4** Desktop (**UI-301–308**); **MVP-3** WoW addon (**UI-401–405**). Broadcaster Extension **UI-201–204** remain **post-MVP**. Compare [`IMPLEMENTATION_READINESS.md`](IMPLEMENTATION_READINESS.md).

## Related documentation

Paths below are relative to this file (`docs/`):

| Area | Markdown |
|------|----------|
| Project overview | [README.md](../README.md) |
| Architecture summary | [CONTEXT.md](../CONTEXT.md) · [ARCHITECTURE.md](ARCHITECTURE.md) |
| Product rules (digest) | [MVP_PRODUCT_SUMMARY.md](MVP_PRODUCT_SUMMARY.md) |
| Agent workflow & roles | [AGENTS.md](../AGENTS.md) |
| Docs index | [ReadME.md](ReadME.md) |
| **Canonical MVP contracts** | [SPEC.md](SPEC.md) |
| MVP prompts & sequencing | [ROADMAP.md](ROADMAP.md) |
| Interaction scenarios & TCs | [INTERACTION_SCENARIOS.md](INTERACTION_SCENARIOS.md) |
| Shared contracts | [MimironsGoldOMatic.Shared/ReadME.md](MimironsGoldOMatic.Shared/ReadME.md) |
| Backend API | [MimironsGoldOMatic.Backend/ReadME.md](MimironsGoldOMatic.Backend/ReadME.md) |
| **Twitch Extension UI (screens)** | [MimironsGoldOMatic.TwitchExtension/UI_SPEC.md](MimironsGoldOMatic.TwitchExtension/UI_SPEC.md) |
| **WPF Desktop UI (screens)** | [MimironsGoldOMatic.Desktop/UI_SPEC.md](MimironsGoldOMatic.Desktop/UI_SPEC.md) |
| **WoW addon UI (screens)** | [MimironsGoldOMatic.WoWAddon/UI_SPEC.md](MimironsGoldOMatic.WoWAddon/UI_SPEC.md) |
| Component engineering | [MimironsGoldOMatic.Desktop/ReadME.md](MimironsGoldOMatic.Desktop/ReadME.md) · [TwitchExtension/ReadME.md](MimironsGoldOMatic.TwitchExtension/ReadME.md) · [WoWAddon/ReadME.md](MimironsGoldOMatic.WoWAddon/ReadME.md) |
| Implementation readiness | [IMPLEMENTATION_READINESS.md](IMPLEMENTATION_READINESS.md) |

**Global product rules (UI copy):** use **[MVP_PRODUCT_SUMMARY.md](MVP_PRODUCT_SUMMARY.md)** and **[SPEC.md](SPEC.md)** for pool, roulette, consent, and §11 copy constraints.

---

## Design language

- **Tone:** Friendly **gnomish engineering** / steampunk-industrial metaphors (gears, vault, machinery) for loading and errors; keep copy consistent across Extension, Desktop strings, and addon toasts where applicable.
- **Honesty:** Enrollment is **Twitch chat** (`!twgold`); the Extension panel explains and shows status—it does not replace chat enrollment.
- **MVP scope:** Gold amount and spin interval are **fixed** per SPEC; avoid implying streamer-configurable economy in UI.

<!-- MANUAL UPDATE REQUIRED: Brand voice examples (RU/EN), marketing alignment, and banned phrases. -->

---

## Color schemes

Semantic colors used across products (see **Design tokens** for full table):

| Role | Hex | Typical use |
|------|-----|-------------|
| **Primary / brand gold** | `#FFB300` | Extension headers, winner banner accents, addon success border (texture) |
| **Secondary / brand iron** | `#5D4037` | Gnome / industrial accents (Extension) |
| **Error** | `#E53935` | Extension error panel, addon error toast, Desktop alert modal |
| **Success** | `#43A047` | Sent chip, success toast, positive indicators |

<!-- MANUAL UPDATE REQUIRED: Define dark-mode or high-contrast variants if products gain themes. -->

---

## Typography

- **Twitch Extension (panel):** Body text typically **~11–13px** in a **~318px** max-width iframe; ASCII mocks in component spec use **~40 character** line width as a monospace stand-in.
- **WPF Desktop:** System / Segoe UI stack; sizes follow standard window chrome and `DataGrid` defaults unless overridden.

<!-- MANUAL UPDATE REQUIRED: Type scale for Desktop (H1/H2/body/caption) and addon `FontString` font objects. -->

---

## Iconography

- **Extension:** Decorative roulette / status icons; align with Twitch panel density (small footprint).
- **WoW addon:** Blizzard texture paths or bundled textures; minimap button **EL-401-01** per addon UI spec.

<!-- MANUAL UPDATE_REQUIRED: Central icon inventory (paths, sizes, license). -->

---

## Interaction patterns

- **Loading:** Indeterminate progress + status copy; Extension uses **live region** for loading/error (UI-101).
- **Retry / dismiss:** Primary recovery for transient EBS failures; exponential backoff per [`SPEC.md`](SPEC.md) for polling.
- **Motion:** Respect **`prefers-reduced-motion`** for roulette animation (Extension UI-103).
- **Errors:** Map API `code` to titled + body copy (Extension UI-105); Desktop uses modal **UI-307** for blocking alerts.
- **Countdown:** **MM:SS** format; server-synced poll to correct drift (Extension UI-106).

---

## Accessibility standards

- **Extension:** Expose status as **live region** where specified (UI-101); ensure **Retry** / **Dismiss** are keyboard-focusable inside the iframe.
- **Desktop:** WPF **keyboard navigation** and default control patterns for dialogs and menus.

<!-- MANUAL UPDATE REQUIRED: Formal WCAG target level and contrast audit per surface (Extension iframe, WPF, WoW). -->

---

## Design tokens & shared vocabulary

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

## Navigation flow

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

### Desktop app

```
UI-301 (API setup) ──→ UI-302 (idle) ──[WoW found]──→ UI-303 (active)
                              │                            │
                              │                            ├── UI-304 queue
                              │                            ├── UI-305 status bar
                              │                            └── UI-307 errors
                              └── settings ──→ UI-306
                              └── log ───────→ UI-308
```

### WoW addon

```
UI-401 (icon/slash) ──→ UI-402 (side panel)
                              ├── EL-402-03 Prepare Mail ──→ (vanilla send)
                              ├── success ───────────────────→ UI-403
                              └── failure ───────────────────→ UI-404
        optional: /mgm debug ──→ UI-405
```

---

## Document control

| Version | Date | Notes |
|---------|------|--------|
| 1.0 | 2026-04-03 | Initial UI spec from `docs/SPEC.md` + component ReadMEs |
| 1.1 | 2026-04-03 | Follow-gated pool; removed Channel Points + instant spin |
| 1.2 | 2026-04-03 | Subscribe + **`!twgold <CharacterName>`** chat enroll; **`!twgold`** acceptance |
| 1.3 | 2026-04-03 | WoW **winner notification whisper** + whisper **`!twgold`** consent (`docs/SPEC.md` §9) |
| 1.4 | 2026-04-05 | Split: hub (`UI_SPEC.md`) + per-component `UI_SPEC.md` under `docs/MimironsGoldOMatic.*/` |

When **`docs/SPEC.md`** adds concrete **pool/spin** and **broadcaster** routes, update **EL-** bindings and **UI-201–204** in the Twitch Extension UI spec without changing IDs where possible.

---

## Component-specific UI specifications

- **[Twitch Extension UI](MimironsGoldOMatic.TwitchExtension/UI_SPEC.md)** — **UI-101–106** (MVP-5 viewer), **UI-201–204** (post-MVP broadcaster reference)
- **[WPF Desktop UI](MimironsGoldOMatic.Desktop/UI_SPEC.md)** — **UI-301–308** (MVP-4)
- **[WoW Addon UI](MimironsGoldOMatic.WoWAddon/UI_SPEC.md)** — **UI-401–405** (MVP-3)

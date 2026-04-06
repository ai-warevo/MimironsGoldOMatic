<!-- Updated: 2026-04-06 (Project structure alignment + Tier B finalization) -->

## MimironsGoldOMatic.WoWAddon (Lua | Bridge between WPF Desktop App & game)

**Cross-cutting:** [`docs/overview/ARCHITECTURE.md`](../../overview/ARCHITECTURE.md) · [`docs/overview/MVP_PRODUCT_SUMMARY.md`](../../overview/MVP_PRODUCT_SUMMARY.md) · [`docs/reference/WORKFLOWS.md`](../../reference/WORKFLOWS.md)

- **Repository status:** `src/MimironsGoldOMatic.WoWAddon` implements **MVP-3** per `docs/overview/ROADMAP.md`: queue + **`MAIL_SHOW`** side panel, **`NotifyWinnerWhisper`**, **`ReceiveGold`**, **`MGM_RunWhoForSpin`**, chat-log tags, and mail-send confirmation. **UI-405** debug frame and richer scroll UX are optional follow-ups ([`UI_SPEC.md`](UI_SPEC.md)).
- **UI spec:** [`UI_SPEC.md`](UI_SPEC.md) (addon **UI-401–405**: entry point, MAIL_SHOW panel, toasts, debug frame). Hub: [`docs/reference/UI_SPEC.md`](../../reference/UI_SPEC.md).
- **Role:** Receives Desktop-injected commands and executes the in-game operator flow. It detects `!twgold` whisper consent and emits `[MGM_CONFIRM:UUID]` after successful mail send (required for backend transition to `Sent`).
- **Target:** WoW 3.3.5a (WotLK).
- **Files:** `MimironsGoldOMatic.toc` (Interface: 30300), `MimironsGoldOMatic.lua`.

## Globals (Desktop `/run`)

| Global | Purpose |
|--------|---------|
| **`NotifyWinnerWhisper(payoutId, characterName)`** | Sends §9 winner notification whisper (`docs/overview/SPEC.md` §9); arms **`[MGM_ACCEPT]`** matching for that character. |
| **`ReceiveGold(dataString)`** | Parses `UUID:CharacterName:GoldCopper;` entries into the mail queue. |
| **`MGM_RunWhoForSpin(spinCycleId, characterName)`** | Runs **`/who`**, then prints **`[MGM_WHO]{...json}`** with **`spinCycleId`** from **`GET /api/roulette/state`** (`currentSpinCycleId`). Desktop calls when the EBS has a spin candidate. |

Slash **`/mgm`** and the minimap coin button toggle the queue panel.

## Key Logic

- **ReceiveGold(dataString):** Semicolon-separated entries; populates **`mgmQueue`** (`READY` → **Prepare Mail** → `PROCESSING` → success pop).
- **NotifyWinnerWhisper(payoutId, characterName):** **`SendChatMessage(..., "WHISPER", ...)`** with exact §9 Russian body.
- **Event handling:** **`MAIL_SHOW`** opens/anchors the panel. **`MAIL_SEND_SUCCESS`** / **`MAIL_FAILED`** / **`MAIL_CLOSED`** logic is restricted to MGM-armed sends only (**§9**), preventing side effects on unrelated manual mail.
- **`/who` (roulette):** **`MGM_RunWhoForSpin`** → **`RunMacroText("/who …")`** → **`WHO_LIST_UPDATE`** → **`[MGM_WHO]`** + JSON (`docs/overview/SPEC.md` §8).
- **Whisper acceptance:** On **`CHAT_MSG_WHISPER`**, when body is exactly **`!twgold`** (case-insensitive after trim) and sender matches the pending notification target, emit **`[MGM_ACCEPT:UUID]`**.
- **Mail-send tag + completion whisper:** **`MAIL_SEND_SUCCESS`** for MGM-armed send → **`[MGM_CONFIRM:UUID]`** then whisper **`Награда отправлена тебе на почту, проверяй ящик!`**

## UI Features

- Queue list (text rows) + **Prepare Mail** (fills **`SendMailNameEditBox`**, subject, **`MoneyInputFrame_SetCopper`**).
- Success toast after **`[MGM_CONFIRM]`** (UI-403–style).

## Security

Only accepts commands via global functions; no external network access (WoW limitation). **Whisper detection** is local to the client; Desktop reads **`WoWChatLog.txt`**.

## Architecture & Patterns

- Central **`OnEvent`** frame for mail, whispers, WHO (MVP simplicity).
- **`hooksecurefunc("SendMail", …)`** pairs armed payout with **`MAIL_SEND_SUCCESS`**.

## Technical Details

- **Feedback loop:** **`!twgold`** → **`[MGM_ACCEPT:UUID]`** → Desktop → Backend. **`[MGM_CONFIRM:UUID]`** → Desktop → Backend **`Sent`**.

## E2E testing note

- **CI Tier A+B** ([`.github/workflows/e2e-test.yml`](../../../.github/workflows/e2e-test.yml)) does **not** start the WoW client. It validates **EBS** behavior with **synthetic EventSub**, **MockHelixApi**, and **SyntheticDesktop** (HTTP-only). **In-game** tags (**`[MGM_WHO]`**, **`[MGM_ACCEPT]`**, **`[MGM_CONFIRM]`**) are covered by manual scenarios and any addon/Lua test harnesses added under **`src/Tests/`**; full **client + addon + log** automation is **Tier C** — [`docs/e2e/TIER_C_REQUIREMENTS.md`](../../e2e/TIER_C_REQUIREMENTS.md), [`docs/e2e/E2E_AUTOMATION_PLAN.md`](../../e2e/E2E_AUTOMATION_PLAN.md).

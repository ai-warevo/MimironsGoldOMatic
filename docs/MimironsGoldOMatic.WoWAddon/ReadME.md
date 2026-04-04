## MimironsGoldOMatic.WoWAddon (Lua | Bridge between WPF Dekstop App & game)

- **Repository status:** `src/MimironsGoldOMatic.WoWAddon` implements **MVP-3** per `docs/ROADMAP.md`: queue + **`MAIL_SHOW`** side panel, **`NotifyWinnerWhisper`**, **`ReceiveGold`**, **`MGM_RunWhoForSpin`**, chat-log tags, and mail-send confirmation. **UI-405** debug frame and richer scroll UX are optional follow-ups (`docs/UI_SPEC.md`).
- **UI spec:** `docs/UI_SPEC.md` §4 (addon **UI-401–405**: entry point, MAIL_SHOW panel, toasts, debug frame).
- **Role:** Receives injected commands and provides the final UX for the streamer; **detects `!twgold` whispers** (willingness to accept gold) and **prints `[MGM_CONFIRM:UUID]`** after mail is sent (**required** for **`Sent`** on the server).
- **Target:** WoW 3.3.5a (WotLK).
- **Files:** `MimironsGoldOMatic.toc` (Interface: 30300), `MimironsGoldOMatic.lua`.

## Globals (Desktop `/run`)

| Global | Purpose |
|--------|---------|
| **`NotifyWinnerWhisper(payoutId, characterName)`** | Sends §9 winner notification whisper (`docs/SPEC.md` §9); arms **`[MGM_ACCEPT]`** matching for that character. |
| **`ReceiveGold(dataString)`** | Parses `UUID:CharacterName:GoldCopper;` entries into the mail queue. |
| **`MGM_RunWhoForSpin(spinCycleId, characterName)`** | Runs **`/who`**, then prints **`[MGM_WHO]{...json}`** with **`spinCycleId`** from **`GET /api/roulette/state`** (`currentSpinCycleId`). Desktop calls when the EBS has a spin candidate. |

Slash **`/mgm`** and the minimap coin button toggle the queue panel.

## Key Logic

- **ReceiveGold(dataString):** Semicolon-separated entries; populates **`mgmQueue`** (`READY` → **Prepare Mail** → `PROCESSING` → success pop).
- **NotifyWinnerWhisper(payoutId, characterName):** **`SendChatMessage(..., "WHISPER", ...)`** with exact §9 Russian body.
- **Event handling:** **`MAIL_SHOW`** opens/anchors the panel; **`MAIL_SEND_SUCCESS`** / **`MAIL_FAILED`** / **`MAIL_CLOSED`** for MGM-armed sends only (**§9**).
- **`/who` (roulette):** **`MGM_RunWhoForSpin`** → **`RunMacroText("/who …")`** → **`WHO_LIST_UPDATE`** → **`[MGM_WHO]`** + JSON (`docs/SPEC.md` §8).
- **Whisper acceptance:** **`CHAT_MSG_WHISPER`**: body **`!twgold`** (case-insensitive), sender matches pending notification target → **`[MGM_ACCEPT:UUID]`**.
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

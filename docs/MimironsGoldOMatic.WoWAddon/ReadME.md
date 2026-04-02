## MimironsGoldOMatic.WoWAddon (Lua)

- **Role:** Receives injected commands and provides the final UX for the streamer.
- **Target:** WoW 3.3.5a (WotLK).
- **Files:** `MimironsGoldOMatic.toc` (Interface: 30300), `MimironsGoldOMatic.lua`.

## Key Logic

- **ReceiveGold(dataString):** Global function. Parses the semicolon-delimited string and populates a local `MimironsQueue` table.
- **Event Handling:** Registers `MAIL_SHOW`. When the mailbox is opened, it shows a side panel.

## UI Features

- A scrollable list of pending recipients.
- **"Prepare Mail" Button:** Automatically sets `SendMailNameEditBox`, `SendMailSubjectEditBox`, and converts gold to copper via `MoneyInputFrame_SetCopper`.

## Security

Only accepts commands via the global function; no external network access (WoW limitation).

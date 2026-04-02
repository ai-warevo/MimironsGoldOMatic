## MimironsGoldOMatic.WoWAddon (Lua | Bridge between WPF Dekstop App & game)

- **Role:** Receives injected commands and provides the final UX for the streamer.
- **Target:** WoW 3.3.5a (WotLK).
- **Files:** `MimironsGoldOMatic.toc` (Interface: 30300), `MimironsGoldOMatic.lua`.

## Key Logic

- **ReceiveGold(dataString):** Global function. Parses the semicolon-delimited string and populates a local `MimironsQueue` table.
- **Event Handling:** Registers `MAIL_SHOW`. When the mailbox is opened, it shows a side panel.
- **Confirmation Tag:** After actual send confirmation, the addon prints `[MGM_CONFIRM:UUID]` to chat so the Desktop app can detect it via `Logs\WoWChatLog.txt`.

## UI Features

- A scrollable list of pending recipients.
- **"Prepare Mail" Button:** Automatically sets `SendMailNameEditBox`, `SendMailSubjectEditBox`, and converts gold to copper via `MoneyInputFrame_SetCopper`.

## Security

Only accepts commands via the global function; no external network access (WoW limitation).

## Architecture & Patterns
- **Event Dispatcher (Table-based):**
  Avoid a monolithic `OnEvent` function. Create a registry table where events like `MAIL_SHOW`, `CHAT_MSG_ADDON`, and `PLAYER_LOGIN` are mapped to specific handler functions for modularity.
  
- **Proxy / Data Wrapper for WoW API:**
  Create a wrapper for the `SendMail` API. All mail attempts must go through this wrapper to check if the mailbox is open, the recipient is valid, and the streamer has enough gold before calling the game's internal functions.

- **State Machine for Payout Queue:**
  Each payout entry must have a state:
  - `READY`: In the local queue, waiting for processing.
  - `PROCESSING`: Data currently injected into the mail compose frame.
  - `SENT`: Confirmed via chat log or manual override.
  This prevents race conditions and double-spending during rapid user interactions.

- **Singleton Pattern:**
  The addon must expose a single global object `MimironsGoldOMatic` for coordination.

## Technical Details
- **Feedback Loop:** Must print `[MGM_CONFIRM:UUID]` to the system chat upon successful mail initiation to allow the Desktop app to read the `WoWChatLog.txt`.

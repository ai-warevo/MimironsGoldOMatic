## MimironsGoldOMatic.WoWAddon (Lua | Bridge between WPF Dekstop App & game)

- **UI spec:** `docs/UI_SPEC.md` §4 (addon **UI-401–405**: entry point, MAIL_SHOW panel, toasts, debug frame).
- **Role:** Receives injected commands and provides the final UX for the streamer; **detects `!twgold` whispers** (willingness to accept gold) and **prints `[MGM_CONFIRM:UUID]`** after mail is sent (**required** for **`Sent`** on the server).
- **Target:** WoW 3.3.5a (WotLK).
- **Files:** `MimironsGoldOMatic.toc` (Interface: 30300), `MimironsGoldOMatic.lua`.

## Key Logic

- **ReceiveGold(dataString):** Global function. Parses the semicolon-delimited string and populates a local `MimironsQueue` table.
- **Event Handling:** Registers `MAIL_SHOW`. When the mailbox is opened, it shows a side panel.
- **`/who` (roulette):** Participate in **online verification** for the selected **Winner_InGame_Nickname** (run or consume **`/who`** results per `docs/SPEC.md`) so offline winners are not finalized.
- **`!twgold` whisper:** After the winner has been **notified** (Extension), when they **reply** with a **private message** whose text is exactly **`!twgold`**, the addon **notifies the Desktop utility** (IPC bridge) so the Backend records **willingness to accept** gold (**required** to receive the mail). The recipient must be **online**.
- **Mail-send tag (required):** After the streamer **actually sends** the in-game mail for a payout, the addon **must** print **`[MGM_CONFIRM:UUID]`** to chat so it appears in **`Logs\WoWChatLog.txt`**. Desktop uses this to set **`Sent`** on the server.

## UI Features

- A scrollable list of pending recipients.
- **"Prepare Mail" Button:** Automatically sets `SendMailNameEditBox`, `SendMailSubjectEditBox`, and converts gold to copper via `MoneyInputFrame_SetCopper`.

## Security

Only accepts commands via the global function; no external network access (WoW limitation). **Whisper detection** is local to the client and forwards to the trusted Desktop utility only.

## Architecture & Patterns
- **Event Dispatcher (Table-based):**
  Avoid a monolithic `OnEvent` function. Create a registry table where events like `MAIL_SHOW`, whisper events, and `PLAYER_LOGIN` are mapped to specific handler functions for modularity.
  
- **Proxy / Data Wrapper for WoW API:**
  Create a wrapper for the `SendMail` API. All mail attempts must go through this wrapper to check if the mailbox is open, the recipient is valid, and the streamer has enough gold before calling the game's internal functions.

- **State Machine for Payout Queue:**
  Each payout entry must have a state:
  - `READY`: In the local queue, waiting for processing.
  - `PROCESSING`: Data currently injected into the mail compose frame.
  - `SENT`: Confirmed via **`[MGM_CONFIRM:UUID]`** in the chat log path (and manual override if needed).
  This prevents race conditions and double-spending during rapid user interactions.

- **Singleton Pattern:**
  The addon must expose a single global object `MimironsGoldOMatic` for coordination.

## Technical Details
- **Feedback loop:** **`!twgold`** → Desktop → Backend **acceptance**. **`[MGM_CONFIRM:UUID]`** → **`WoWChatLog.txt`** → Desktop → Backend **`Sent`**.

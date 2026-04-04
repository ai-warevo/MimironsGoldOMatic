## MimironsGoldOMatic.WoWAddon (Lua | Bridge between WPF Dekstop App & game)

- **Repository status:** `src/MimironsGoldOMatic.WoWAddon` contains `.toc` and a **minimal** Lua stub. Behaviors below (**`ReceiveGold`**, **`NotifyWinnerWhisper`**, **`[MGM_WHO]`**, whispers, **`[MGM_ACCEPT]`** / **`[MGM_CONFIRM]`**) are **specified** for implementation — see `docs/IMPLEMENTATION_READINESS.md`.
- **UI spec:** `docs/UI_SPEC.md` §4 (addon **UI-401–405**: entry point, MAIL_SHOW panel, toasts, debug frame).
- **Role:** Receives injected commands and provides the final UX for the streamer; **detects `!twgold` whispers** (willingness to accept gold) and **prints `[MGM_CONFIRM:UUID]`** after mail is sent (**required** for **`Sent`** on the server).
- **Target:** WoW 3.3.5a (WotLK).
- **Files:** `MimironsGoldOMatic.toc` (Interface: 30300), `MimironsGoldOMatic.lua`.

## Key Logic

- **ReceiveGold(dataString):** Global function. Parses the semicolon-delimited string and populates a local `MimironsQueue` table.
- **NotifyWinnerWhisper(payoutId, characterName):** Global function (`docs/SPEC.md` §8–9). Invoked when Desktop injects **`/run NotifyWinnerWhisper("…","…")`**. The addon **must** send the §9 **`/whisper <characterName> …`** line (Russian text exact per spec). **Desktop** does not paste the whisper text.
- **Event Handling:** Registers `MAIL_SHOW`. When the mailbox is opened, it shows a side panel.
- **`/who` (roulette):** **Run** **`/who`**, **parse** the **3.3.5a** result, emit **`[MGM_WHO]`** + JSON via **`DEFAULT_CHAT_FRAME:AddMessage`** (`docs/SPEC.md` §8) so it appears in **`Logs\WoWChatLog.txt`** → Desktop **`POST /api/roulette/verify-candidate`**; Backend **authoritatively** creates **`Pending`** or **no winner** this cycle. **No** separate file-bridge — addons cannot write arbitrary files.
- **Winner notification whisper (normative):** The **addon** sends **`/whisper <Winner_InGame_Nickname> …`** with the exact Russian text in **`docs/SPEC.md` §9** (MVP: **addon-only**, not Desktop injection). **Acceptance:** when the winner **whispers** text matching **`!twgold`** (**case-insensitive** after trim), the addon **prints `[MGM_ACCEPT:UUID]`** to WoW chat (captured in **`Logs\WoWChatLog.txt`**) so the Desktop utility can call **`confirm-acceptance`**. The recipient must be **online**. **`[MGM_CONFIRM:UUID]`** is printed after mail send (`docs/SPEC.md` §9–10).
- **Mail-send tag + completion whisper (required):** On **`MAIL_SEND_SUCCESS`**, **only** for an **MGM-armed** send (queue / **Prepare Mail** path — **not** a manual mailbox send), the addon **must** print **`[MGM_CONFIRM:UUID]`** to the default chat frame (**`Logs\WoWChatLog.txt`**) **then** whisper the winner: **`Награда отправлена тебе на почту, проверяй ящик!`** (`docs/SPEC.md` §9). Register **`MAIL_FAILED`** and clear armed state so stray events do not emit tags.

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
- **Feedback loop:** **`!twgold`** (Lua) → **`[MGM_ACCEPT:UUID]`** in chat/log → Desktop → Backend **acceptance**. **`[MGM_CONFIRM:UUID]`** → same log → Desktop → Backend **`Sent`**.

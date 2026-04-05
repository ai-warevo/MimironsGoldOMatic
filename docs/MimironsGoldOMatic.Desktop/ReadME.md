<!-- Updated: 2026-04-05 (Deduplication pass) -->

## MimironsGoldOMatic.Desktop (WPF | Bridge between EBS & Lua addon)

**Cross-cutting:** [`docs/ARCHITECTURE.md`](../ARCHITECTURE.md) · [`docs/MVP_PRODUCT_SUMMARY.md`](../MVP_PRODUCT_SUMMARY.md) · [`docs/WORKFLOWS.md`](../WORKFLOWS.md)

- **Repository status:** `src/MimironsGoldOMatic.Desktop` ships the **MVP-4** WPF utility: queue UI, **`HttpClient`** + Polly to the EBS ( **`X-MGM-ApiKey`** ), single-file tail of **`WoWChatLog.txt`**, and foreground **`WoW.exe`** injection per `docs/SPEC.md` §8–10. See `docs/IMPLEMENTATION_READINESS.md` for parity notes.
- **UI spec:** `docs/UI_SPEC.md` §3 (WPF **UI-301–308**: API setup, main window, queue, settings, modals, log).
- **Role:** Monitors the **EBS** and injects **winner** payout data into the WoW client; bridges **addon → EBS** for **`!twgold`** (acceptance) and **`WoWChatLog.txt`** tailing for **`[MGM_CONFIRM:UUID]`** (required **mail-sent → `Sent`**).
- **Stack:** .NET 10, WPF, MVVM (CommunityToolkit.Mvvm).

## Key Functions

- **API Polling:** Periodically fetches the pending **winner** payout queue from the **EBS**.
- **Roulette `/who`:** Tails **`Logs\WoWChatLog.txt`** for **`[MGM_WHO]`** lines from the addon (`docs/SPEC.md` §8) and **`POST`s** **`/api/roulette/verify-candidate`** with **`X-MGM-ApiKey`**; **EBS** creates **`Pending`** or **no winner** (no re-draw same cycle).
- **Process Targeting (MVP):** Targets the **foreground** `WoW.exe` (3.3.5a) process. Process selection from a list is a roadmap feature.

## Command Injection (WPF to Addon)

- **Winner notification (before mail queue):** After the **EBS** reports **`Pending`** for a winner, inject **`/run NotifyWinnerWhisper("<payoutId>","<CharacterName>")`** so the addon sends the §9 whisper (`docs/SPEC.md` §8–9). **Then** use **`ReceiveGold`** when the streamer syncs mail (**`InProgress`**).
- Converts the list of payouts into a Lua-compatible string with canonical entry format:
  - `UUID:CharacterName:GoldCopper;`
  - Example: `/run ReceiveGold("2d2b7b2a-1111-2222-3333-444444444444:Somecharacter:10000000;")`
- Splits strings into chunks of < 255 characters (WoW chat limit).
- Uses `PostMessage` as primary strategy.
- Uses `SendInput` fallback strategy (operator-switchable) when primary injection is blocked/unreliable.

## Explicit Claim Flow (Desktop to EBS)

The Desktop app uses an explicit claim model to avoid accidentally locking payouts:

1. Desktop fetches the queue via **GET** `/api/payouts/pending`.
2. When the streamer clicks **Sync/Inject**, Desktop **must** have located the **WoW** target (`WoW.exe`, MVP: foreground) **before** marking payouts as `InProgress` via **PATCH** `/api/payouts/{id}/status` (see `docs/SPEC.md` §3).
3. Desktop injects the payload into WoW via `/run ReceiveGold("...")`.

### Feedback Loop (Addon to WPF to EBS)

1. The WoW Addon detects a whisper whose body matches **`!twgold`** (**case-insensitive** after trim) from the expected winner (reply to the **winner notification whisper**, `docs/SPEC.md` §9) and **prints `[MGM_ACCEPT:UUID]`** to WoW chat so it appears in **`Logs\WoWChatLog.txt`**.
2. The WPF Utility **must** use **one** tail of **`Logs\WoWChatLog.txt`**: on **`[MGM_ACCEPT:UUID]`**, call **POST** `/api/payouts/{id}/confirm-acceptance` (**willingness to accept**; **not** **`Sent`**).
3. **Required:** On **`[MGM_CONFIRM:UUID]`** in the **same** log stream, call the **EBS** (**PATCH** `/api/payouts/{id}/status` → **`Sent`**) — mail was sent (`docs/SPEC.md` §10).
4. If the log entry is missed, the Desktop UI provides a manual **Mark as Sent** override (policy decision).

## Libraries

- `Refit` or `HttpClient` (for API)
- `CommunityToolkit.Mvvm`

## Architecture & Patterns
- **Strategy Pattern (Injection):**
  Implement `IWoWInputStrategy`. Create `PostMessageStrategy` (primary) and `SendInputStrategy` (fallback). Allow the streamer to switch strategies in settings if one is blocked by a specific private server's anti-cheat.
  
- **Observer Pattern (ChatLogWatcher):**
  The **`ChatLogWatcher`** (single tail of **`WoWChatLog.txt`**) must be observable. On **`[MGM_WHO]`** vs **`[MGM_ACCEPT:UUID]`** vs **`[MGM_CONFIRM:UUID]`**, route to **`verify-candidate`** vs **`confirm-acceptance`** vs **`PATCH` `Sent`** respectively (`docs/SPEC.md` §10).

- **State Pattern (UI Behavior):**
  The "Sync" button must act as a state machine. Its appearance and logic should change based on the app state: `Searching for WoW` -> `Process Found` -> `Waiting for Mailbox` -> `Ready to Inject`.

## Resilience
- **Polly Integration:** Use Polly for all outgoing HTTP calls to the Backend to handle transient network issues with retries and exponential backoff.

## WinAPI / WoW 3.3.5a compatibility (MVP)

- **Window discovery:** Only the **foreground** window is considered. `GetForegroundWindow` → `GetWindowThreadProcessId` → process name **`WoW`** (module **`WoW.exe`**) must match. If another app is focused, injection is skipped or fails fast — the streamer must click into WoW before **Sync/Inject** or auto **`NotifyWinnerWhisper`**.
- **Focus timing:** `ShowWindow(SW_RESTORE)` + `SetForegroundWindow` run before posting keys. Short sleeps (~60–120 ms) follow focus and between **`WM_CHAR`** posts so the client’s chat box keeps up on slower machines; if commands truncate, increase delays in `PostMessageWoWInputStrategy` / `SendInputWoWInputStrategy` or prefer **SendInput** in Settings.
- **Primary vs fallback:** **`PostMessage(WM_CHAR)`** targets the **main** WoW HWND (same as foreground). If the client ignores posted messages (some anti-cheat or focus modes), the coordinator retries with **`SendInput`** (`KEYEVENTF_UNICODE`). Operator can force **SendInput** first via Settings.
- **What breaks if timing differs:** If WoW is not actually receiving keyboard focus, chat may not open and **`/run`** text can appear in the wrong UI layer or be dropped. If delays are too short, partial commands may be sent — reduce poll/inject frequency or increase inter-character delay.

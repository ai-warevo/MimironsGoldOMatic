# Report: Gold payout + roulette documentation update

## Modified files

- `README.md`
- `CONTEXT.md`
- `AGENTS.md`
- `docs/SPEC.md` (canonical rewrite for pool, roulette, `!twgold`, API notes)
- `docs/ROADMAP.md`
- `docs/ReadME.md`
- `docs/IMPLEMENTATION_READINESS.md`
- `docs/MimironsGoldOMatic.Backend/ReadME.md`
- `docs/MimironsGoldOMatic.Desktop/ReadME.md`
- `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`
- `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`
- `docs/MimironsGoldOMatic.Shared/ReadME.md`
- `src/MimironsGoldOMatic.TwitchExtension/README.md`
- `docs/prompts/history/2026-04-03/01-gold-payout-roulette-docs/` (`prompt.md`, `plan.md`, `checks.md`, `report.md`)

## Verification

- Documentation-only change; no `dotnet test` run required for this task.

## Technical debt / follow-ups

- `docs/SPEC.md` defers concrete **pool/spin** route shapes and addon→Desktop IPC to implementation.
- Align `POST /api/payouts/claim` naming with a dedicated **enroll** resource if the team splits routes.

## Addendum — `!twgold` vs `[MGM_CONFIRM:UUID]` (same session)

- **`!twgold`** (whisper): documents **willingness to accept** gold — Backend records **acceptance**, not **`Sent`**.
- **`[MGM_CONFIRM:UUID]`** in **`Logs\WoWChatLog.txt`**: **required** — Desktop **must** parse it; addon **must** emit after mail send; drives automated **`Sent`**.
- Canonical detail: `docs/SPEC.md` §3, §5, §9–10; endpoint rename to **`POST .../confirm-acceptance`** for the whisper path.

## Addendum — `/who`, winner notification, `!twgold` reply (same session)

- Roulette **must** verify the selected **Winner_InGame_Nickname** is **online** with **`/who <Winner_InGame_Nickname>`** before finalizing; offline picks invalid (re-draw / policy per spec).
- Winners **must** be **notified** (Extension **“You won”** + instructions).
- **To receive the gold mail**, the winner **must** **reply** via **private in-game message** **`!twgold`** after notification; streamer **waits** for that whisper before mailing.

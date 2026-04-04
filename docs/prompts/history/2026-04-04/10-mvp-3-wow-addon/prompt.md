# Prompt

Implement MVP-3 WoW addon (`src/MimironsGoldOMatic.WoWAddon`) per `docs/ROADMAP.md`, `docs/SPEC.md` §8–10, and `docs/UI_SPEC.md` UI-401–405 (practical subset): globals `NotifyWinnerWhisper`, `ReceiveGold`, `MGM_RunWhoForSpin`; MAIL_SHOW side panel; Prepare Mail; whisper `!twgold` → `[MGM_ACCEPT:UUID]`; `/who` → `[MGM_WHO]` JSON; MGM-armed `MAIL_SEND_SUCCESS` → `[MGM_CONFIRM:UUID]` + completion whisper.

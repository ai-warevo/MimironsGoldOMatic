# Action: Phase 2 - Implementation of WoW Addon
Acting as [WoW Addon Expert], please read the documentation in @docs/MimironsGoldOMatic.WoWAddon/ReadME.md.

Create the `src/MimironsGoldOMatic.WoWAddon` folder. 
Implement the 3.3.5a (Interface: 30300) addon logic:
1. `MimironsGoldOMatic.lua` with the global `ReceiveGold(dataString)` function.
2. The UI side-panel (MimironFrame) that hooks into `MAIL_SHOW`.
3. Auto-fill logic for `SendMailNameEditBox` and `MoneyInputFrame_SetCopper` as specified.

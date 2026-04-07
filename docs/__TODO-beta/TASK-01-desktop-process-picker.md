# Task 01 - Desktop WoW Process Picker

Acting as **[WPF/WinAPI Expert]**.

## Goal
Implement Beta feature: select a specific WoW process/window instead of only using foreground targeting.

## Read first
- `docs/overview/SPEC.md`
- `docs/overview/ROADMAP.md` (Beta section)
- `docs/components/desktop/ReadME.md`
- `docs/reference/UI_SPEC.md`

## Implement
1. Add a process picker UI and selected-target persistence in Desktop settings.
2. Support refresh/reselect when process exits/restarts.
3. Use selected process for injection path (`NotifyWinnerWhisper` and `ReceiveGold`), with safe fallback and clear status messages.
4. Keep existing PostMessage primary and SendInput fallback behavior.
5. Document assumptions/timing notes for WoW 3.3.5a.

## Acceptance criteria
- User can explicitly choose target WoW process/window.
- Injection uses selected target reliably.
- If target is invalid, app shows actionable error and recovery steps.
- Existing foreground-only behavior is no longer required for normal operation.

## Output
- Code changes in Desktop project.
- Updated docs in `docs/components/desktop/ReadME.md`.
- Short verification notes in a task report file under `docs/prompts/history/<date>/.../report.md`.

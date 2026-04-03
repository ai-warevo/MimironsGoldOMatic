# Plan

## Goal

Lock MVP implementation risks via user decisions and reflect them in documentation so agents do not improvise on file-bridge, auth, or acceptance paths.

## Approach

1. Update **`docs/SPEC.md`** as single source of truth: remove JSON file-bridge; define **`[MGM_WHO]`** + JSON on one log line; unified **`WoWChatLog.txt`** tail; addon-only winner whisper; Desktop-only **`confirm-acceptance`**; real Twitch JWT for Dev Rig + deploy; 30s grace for **`verify-candidate`**; Extension backoff/Retry; **`active_payout_exists`** without auto-expire; pause N/A MVP; **`spinPhase`** transitions Backend-defined.
2. Align **`README.md`**, **`CONTEXT.md`**, **`AGENTS.md`**, **`docs/ROADMAP.md`**, **`docs/INTERACTION_SCENARIOS.md`**, **`docs/IMPLEMENTATION_READINESS.md`**, component **`ReadME.md`** files under **`docs/MimironsGoldOMatic.*/`**.

## Risks

- Implementers must parse **`[MGM_WHO]`** JSON robustly (single-line, UTF-8).
- Grace window interpretation (30s after cycle boundary) must match Backend clock.

## Out of scope

- Application code / solution build (documentation only).

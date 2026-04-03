# Report: lock implementation decisions (questionnaire answers)

## Source

User answers to the clarification questionnaire (sections A–J), applied to **`docs/SPEC.md`** and aligned docs.

## Summary of normative decisions

| Topic | Decision |
|-------|----------|
| **A** | Single broadcaster per MVP deployment; Extension JWT context only. |
| **B** | **`GET /api/roulette/state`**, **`GET /api/pool/me`**: strict Twitch Extension **JWT** (Bearer). |
| **C** | **`GET /api/payouts/my-last`**: **`404`** if no winner payout; **`200`** + **`PayoutDto`** when exists. |
| **D** | **`spinPhase`**: `idle`, `collecting`, `spinning`, `verification`, `completed`. Offline candidate: **no winner**, **no re-draw** same cycle; single offline → no winner. **`nextSpinAt`**: UTC **:00/:05/…**. **`currentSpinCycleId`** in **`GET /api/roulette/state`**. |
| **E** | Same user new **`!twgold Name`**: **replace** pool row. Non-sub: **log only**. Chat: **EventSub** required for MVP. |
| **F** | Addon runs **`/who`**, parses, **file-bridge** JSON; Desktop **POST** **`/api/roulette/verify-candidate`**. Backend **authoritative** for **`Pending`**. |
| **G** | **`confirm-acceptance`**: **`characterName`** required. **`PATCH`**: **`InProgress` → `Pending`** allowed. **Outbox**: **no** table until first external integration. |
| **H** | Marten: **stream per payout id**; **separate** Pool and Payout aggregates. |
| **I** | **`WoWChatLog.txt`**: default + full-path override. Unknown **`MGM_ACCEPT`**: log + ignore. |
| **J** | **`CharacterName`**: length **2–12**; Latin/Cyrillic letters only; no spaces (**§4**). |

## New / updated API (SPEC §5)

- **`POST /api/roulette/verify-candidate`** (Desktop, ApiKey) — body aligned with **§8** file-bridge JSON.

## Files touched

- `AGENTS.md`
- `README.md`
- `docs/SPEC.md`
- `docs/ROADMAP.md`
- `docs/UI_SPEC.md`
- `docs/INTERACTION_SCENARIOS.md`
- `docs/IMPLEMENTATION_READINESS.md`
- `docs/MimironsGoldOMatic.Backend/ReadME.md`
- `docs/MimironsGoldOMatic.Desktop/ReadME.md`
- `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`
- `docs/MimironsGoldOMatic.Shared/ReadME.md`
- `docs/prompts/history/2026-04-04/03-lock-implementation-decisions/plan.md`, `checks.md`

## Verification

- Documentation only; no `dotnet test`.

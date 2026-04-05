# Report: doc accuracy clarifications (2026-04-04)

## User decisions (questionnaire)

| # | Decision |
|---|----------|
| 1 | **B** — Twitch chat enrollment: **`!twgold`** prefix **case-insensitive**. |
| 2 | **B** — WoW whisper consent: **`!twgold`** **case-insensitive** (after trim). |
| 3 | **A** — Spin schedule **server-authoritative**; Extension **must still show** roulette **countdown/timer** from API (`GET /api/roulette/state`). |
| 4 | **A** — Document **minimum normative** pool/roulette HTTP contract in `docs/overview/SPEC.md`. |
| 5 | **Clarified:** **`WoWChatLog.txt`** is the **required** Desktop path for **`[MGM_CONFIRM:UUID]`** → **`Sent`**. Whisper **`!twgold`** acceptance remains **Lua events → Desktop (IPC)** by default; **do not** rely on log parsing for whispers unless a 3.3.5a pattern is validated (`docs/overview/SPEC.md` §10). |
| 6 | **A** — **`Pending` → `InProgress`** only after **WoW** target is detected. |
| 7 | **User:** In-game **`/who <Name>`** is how presence/online is checked; no external realm API in MVP. |
| 8 | **A** — **`AGENTS.md`**: Backend write model = **Marten/PostgreSQL ES**; EF **read models optional**. |
| 9 | **B** — **Outbox** required for MVP **when external side effects exist** (`docs/overview/SPEC.md` §6). |
| 10 | **A** — Malformed chat (e.g. **`!twgold`** without name): **ignore** (no pool change). |

## Modified files

- `AGENTS.md`
- `README.md`
- `CONTEXT.md`
- `docs/overview/SPEC.md`
- `docs/ReadME.md`
- `docs/overview/ROADMAP.md`
- `docs/reference/UI_SPEC.md`
- `docs/overview/INTERACTION_SCENARIOS.md` (SC-011, SC-012)
- `docs/reference/IMPLEMENTATION_READINESS.md`
- `docs/components/backend/ReadME.md`
- `docs/components/desktop/ReadME.md`
- `docs/components/twitch-extension/ReadME.md`
- `docs/components/wow-addon/ReadME.md`
- `docs/prompts/history/2026-04-04/01-doc-accuracy-clarifications/plan.md` (status via `checks.md`)

## Verification

- Documentation-only change; no `dotnet test` run.

## Technical debt / follow-ups

- Implement **OpenAPI** / locked JSON Schemas when Backend exists; §5.1 field lists remain normative until then.

**Superseded:** Addon→Desktop acceptance is **`[MGM_ACCEPT:UUID]`** via **`WoWChatLog.txt`** — see `docs/prompts/history/2026-04-04/02-addon-desktop-bridge/report.md`.

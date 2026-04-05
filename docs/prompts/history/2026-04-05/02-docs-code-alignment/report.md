# Report — Documentation aligned with `src/` (2026-04-05)

## Modified files

- `docs/overview/SPEC.md` — EventSub route, rate limiter behavior, Extension `claim` vs `DevSkipSubscriberCheck`, `PoolEnrollmentResponse`, `PATCH` returns `PayoutDto`, code-alignment blurb.
- `docs/overview/ROADMAP.md` — Auth/status text, CI placeholder note, implementation snapshot wording.
- `docs/reference/UI_SPEC.md` — Implementation note reflects MVP-5/4/3 code.
- `docs/overview/INTERACTION_SCENARIOS.md` — `dotnet test` note, SC-012 fix, new **SC-005** (EventSub).
- `docs/reference/IMPLEMENTATION_READINESS.md` — Residual risk: Helix + `claim` gap.
- `README.md`, `CONTEXT.md`, `AGENTS.md`, `SETUP.md`, `SETUP-for-developer.md`, `SETUP-for-streamer.md` — status, auth, roulette offline rule, `DevSkipSubscriberCheck` accuracy.
- `docs/ReadME.md` — Removed incorrect `FluentResults` guidance; EBS result pattern.
- `docs/components/backend/ReadME.md` — Endpoints, auth, Helix retries, event types.
- `docs/components/twitch-extension/ReadME.md` — Implemented stack/patterns; `claim` caveat.
- `docs/components/desktop/ReadME.md`, `docs/components/shared/ReadME.md`, `docs/components/wow-addon/ReadME.md` — Banner + accuracy fixes.

## Unchanged (no edit needed or out of scope)

- All files under `docs/prompts/` except this task folder (per user ignore rules).
- No other standalone `.md` files under `docs/` root besides those listed (all touched).

## Verification

- `dotnet build src/MimironsGoldOMatic.slnx` — **succeeded** (0 warnings).
- `dotnet test` — not run (no test projects in solution yet; matches MVP-6 gap).

## Potential technical debt

- **`POST /api/payouts/claim`** still lacks Helix subscriber verification when `Mgm:DevSkipSubscriberCheck` is `false`; docs now state this explicitly — product may want parity with EventSub enrollment.
- **`Polly.Extensions.Http`** is referenced in Backend `.csproj` but **Helix** uses a manual retry loop; package may be removable or Polly could be wired in later.
- **Part 2 (TC-xxx)** tables in `INTERACTION_SCENARIOS.md` were not exhaustively renumbered after inserting **SC-005**; scenario IDs in prose remain consistent.
- **CI:** `.github/workflows/` is still placeholder-only; roadmap notes this.

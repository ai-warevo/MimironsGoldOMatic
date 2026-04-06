<!-- Updated: 2026-04-06 (Project structure alignment + Tier B finalization) -->

# Tier C — requirements (draft)

**Parent plan:** [`E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md) — section **Tier C: Future Scope & Requirements**. **Tier B** (CI mocks, **Backend ↔ MockHelixApi ↔ SyntheticDesktop**) is closed; Tier C extends toward **real clients** and optional **additional mocks**.

---

## 1. Goals (high level)

| Theme | Description |
|--------|-------------|
| **Real Desktop + WoW** | Automated or semi-automated pipeline on **self-hosted Windows** runners (or manual nightly): real **`WoW.exe`**, real addon, Desktop **WinAPI** / log tail → EBS. |
| **Stronger fidelity** | Compare **SyntheticDesktop** timings and payloads against **MimironsGoldOMatic.Desktop** behavior; reduce drift in headers, order, and error handling. |
| **Broader mocks** | Optional **IRC/EventSub live** staging, **real Helix** with broadcaster secrets in a gated environment (not default PR CI). |
| **Observability** | Structured logs/metrics from E2E harnesses for flake diagnosis (align with [`E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md) troubleshooting). |

---

## 2. Features / components to add

- **Self-hosted E2E job** (optional workflow or `workflow_dispatch`): Windows runner prerequisites (WoW 3.3.5a path, log path, API keys).
- **Desktop harness parity tests**: shared test library for HTTP sequence vs **SyntheticDesktop** (`src/Mocks/SyntheticDesktop/`).
- **Addon contract tests**: expand `src/Tests/MimironsGoldOMatic.WoWAddon.Tests/` for **`[MGM_*]`** tag formats (see [`docs/components/wow-addon/ReadME.md`](../components/wow-addon/ReadME.md)).
- **Staging Twitch**: secrets for real **Helix** and **EventSub** (org **Environments**), documented in [`docs/setup/SETUP-for-developer.md`](../setup/SETUP-for-developer.md).

---

## 3. Integration points (current structure)

| Area | Path / doc |
|------|------------|
| EBS APIs | `src/MimironsGoldOMatic.Backend/` — Desktop routes, Helix, harness `POST /api/e2e/prepare-pending-payout` (Development only). |
| Synthetic mock | `src/Mocks/SyntheticDesktop/` |
| Helix mock | `src/Mocks/MockHelixApi/` |
| Real Desktop | `src/MimironsGoldOMatic.Desktop/` — [`docs/components/desktop/ReadME.md`](../components/desktop/ReadME.md) |
| Addon | `src/MimironsGoldOMatic.WoWAddon/` — [`docs/components/wow-addon/ReadME.md`](../components/wow-addon/ReadME.md) |
| CI | `.github/workflows/e2e-test.yml`, `unit-integration-tests.yml` |

---

## 4. Success criteria (initial)

1. **Documented** Windows/self-hosted runbook with pass/fail criteria for at least one **SC-001** segment involving real log tags.
2. **No regression** to **Tier A / Tier B** default PR workflows (Linux-hosted mocks).
3. **Secrets**: no broadcaster tokens in logs; use GitHub **Environments** for optional staging jobs.

---

## 5. Task breakdown (high level)

| Phase | Tasks |
|--------|--------|
| **C0** | Prioritize: full Windows E2E vs staging Twitch vs addon-only tests; cost/concurrency decision. |
| **C1** | Spec runner image(s), WoW install constraints, artifact retention for log uploads. |
| **C2** | Implement smallest vertical slice (e.g. log tag detection integration with mock Backend). |
| **C3** | Harden flakiness (focus timing, `docs/overview/SPEC.md` windows). |

---

## 6. Dependencies on external systems

- **Twitch**: Helix, EventSub, Extension JWT (test channel, dev rig) — see [`docs/overview/SPEC.md`](../overview/SPEC.md).
- **WoW 3.3.5a**: client availability on runner (licensing and ToS are operator responsibility).
- **GitHub**: self-hosted runner registration, **Actions** minutes vs **hosted** cost tradeoffs.

---

## 7. Risks and mitigations

| Risk | Mitigation |
|------|------------|
| Flaky UI/WinAPI | Keep **SyntheticDesktop** path in CI; Tier C as optional/nightly. |
| Secret exposure | Environments, OIDC, no token echo in artifacts. |
| Runner cost | Path filters, schedule-only Tier C, cache **NuGet**/pip as in [`e2e-test.yml`](../../.github/workflows/e2e-test.yml). |

<!-- Updated: 2026-04-06 (Transition complete & Tier C launch) -->

# Tier C — kick-off plan (Weeks 1–4)

**Requirements:** [`TIER_C_REQUIREMENTS.md`](TIER_C_REQUIREMENTS.md) · **Task board:** [`TIER_C_IMPLEMENTATION_TASKS.md`](TIER_C_IMPLEMENTATION_TASKS.md) · **Tier B handover:** [`TIER_B_HANDOVER.md`](TIER_B_HANDOVER.md) · **Project structure:** [`docs/reference/PROJECT_STRUCTURE.md`](../reference/PROJECT_STRUCTURE.md)

This plan turns Tier C requirements into a 4‑week execution cadence while keeping **Tier A + Tier B** PR CI stable.

---

## 1. Scope guardrails (non-negotiable)

- **Tier A + Tier B remain functional** on PRs to `main` (`.github/workflows/e2e-test.yml`).
- **Tier C jobs are optional** until they are proven stable (prefer `workflow_dispatch` and/or `schedule` behind policy).
- **No secrets** in logs/artifacts; staging Twitch jobs must use **GitHub Environments** and never run on fork PRs.

---

## 2. Resource requirements

| Role | Needed for | Named owner |
|------|------------|-------------|
| Backend / CI owner | Tier C workflow design, harness/logging, staging jobs | **Anatoly Ivanov** (`ai.vibeqodez@vk.com`) |
| Desktop / WinAPI owner | Windows runner + Desktop parity + log tail constraints | **Anatoly Ivanov** (`ai.vibeqodez@vk.com`) |
| Addon owner | `[MGM_*]` contract tests + log formats | **Anatoly Ivanov** (`ai.vibeqodez@vk.com`) |

Infrastructure prerequisites (as decisions):
- Self-hosted Windows runner capacity (or manual operator machine)
- Optional staging Twitch app + test channel access (via Environment‑gated secrets)

---

## 3. Timeline (Week 1–4)

**Planned start date:** 2026-04-13 (Mon)
**Planned end date:** 2026-05-08 (Fri)

Owners: see **Resource requirements** (Section 2).

### Week 1 — C0 decisions + runbooks

- **Milestone:** M‑C0 (prioritization locked)
- **Dates:** 2026-04-13 → 2026-04-17
- **Owner:** Anatoly Ivanov
- **Deliverables:**
  - Decision on Tier C primary track (**Windows E2E** vs **staging Twitch** vs **addon tests**) (`C0-01`)
  - CI cost/concurrency policy (`C0-02`)
  - Draft Windows E2E setup plan (`C0-03`)
  - Draft staging Twitch Environment plan (`C0-04`)

### Week 2 — Infrastructure proof + minimal workflow skeleton

- **Milestone:** M‑C1 (runner/env spec approved)
- **Dates:** 2026-04-20 → 2026-04-24
- **Owner:** Anatoly Ivanov
- **Deliverables (depending on C0 decision):**
  - Self-hosted runner smoke workflow (no secrets) proving the runner works (`C1-01` prerequisite slice)
  - GitHub Environment(s) created + documented (names, rules) (`C1-02`)

### Week 3 — First vertical slice

- **Milestone:** M‑C2 (first vertical slice green)
- **Dates:** 2026-04-27 → 2026-05-01
- **Owner:** Anatoly Ivanov
- **Candidate slices (choose one):**
  - **Windows slice:** real Desktop log tail contract or controlled log replay (no WoW required on day 1)
  - **Staging Helix slice:** Environment‑gated `workflow_dispatch` that posts one message to a test channel and stores only non-sensitive evidence (`C2-04`)
  - **Addon slice:** expand contract tests for `[MGM_*]` tags (`C2-03`)

### Week 4 — Hardening + observability

- **Milestone:** M‑C3 (parity + flake budget)
- **Dates:** 2026-05-04 → 2026-05-08
- **Owner:** Anatoly Ivanov
- **Deliverables:**
  - Flake budget + retry policy documented and applied (`C3-01`)
  - Structured harness logs + correlation conventions (`C3-02`)
  - Parity plan for Desktop vs SyntheticDesktop (if Windows track chosen) (`C2-02`)

---

## 4. Risks & mitigations (based on Tier B lessons)

| Risk | Mitigation |
|------|------------|
| WinAPI / UI timing flake | Keep Tier B mocks as PR gate; Tier C as nightly/manual until stable. |
| Token leakage | Environments + masking; avoid printing headers; artifact redaction rules. |
| CI minute overrun | Concurrency, optional Tier C triggers, and explicit policy (C0-02). |
| Debuggability gaps | Always-on artifacts and a troubleshooting matrix, mirroring Tier B practice. |

### Risk mitigation actions (owned + dated)

| Risk | Action | Owner | Due |
|------|--------|-------|-----|
| Token leakage | Define Environment protection rules + “no forks” policy | Anatoly Ivanov | 2026-04-17 |
| WinAPI flake | Keep Tier C jobs optional until 5 consecutive green runs | Anatoly Ivanov | 2026-05-08 |
| CI minutes | Document Tier C trigger policy (nightly/dispatch only) | Anatoly Ivanov | 2026-04-17 |
| Debuggability gaps | Require artifact bundle for Tier C jobs mirroring Tier B | Anatoly Ivanov | 2026-05-01 |

---

## 5. Links (single source of truth)

- Tier C requirements: [`TIER_C_REQUIREMENTS.md`](TIER_C_REQUIREMENTS.md)
- Tier C tasks: [`TIER_C_IMPLEMENTATION_TASKS.md`](TIER_C_IMPLEMENTATION_TASKS.md)
- Tier B operations/handover: [`TIER_B_HANDOVER.md`](TIER_B_HANDOVER.md), [`TIER_B_MAINTENANCE_CHECKLIST.md`](TIER_B_MAINTENANCE_CHECKLIST.md)
- Tier B retrospective: `TIER_B_HANDOVER.md` → **Retrospective Summary & Lessons Learned**
- Repo structure: [`docs/reference/PROJECT_STRUCTURE.md`](../reference/PROJECT_STRUCTURE.md)

<!-- Updated: 2026-04-08 (Tier C complete) -->

# Tier C — implementation tasks

**Requirements:** [`TIER_C_REQUIREMENTS.md`](TIER_C_REQUIREMENTS.md) · **E2E plan (Tier C scope):** [`E2E_AUTOMATION_PLAN.md` — Tier C](E2E_AUTOMATION_PLAN.md#tier-c-future-scope--requirements) · **Issue:** [#16](https://github.com/ai-warevo/MimironsGoldOMatic/issues/16)

**Legend:** **Priority**: `HIGH` | `MED` | `LOW` · **Est** = engineering days (rough) · **Status:** `todo` | `progress` | `done` | `blocked`

---

## Milestones and target dates

| Milestone | Target | Notes |
|-----------|--------|--------|
| **M-C0** — Prioritization locked | 2026-04-08 | Completed with issue-backed decisions and kickoff alignment. |
| **M-C1** — Runner / env spec approved | Deferred | Moved to post-Tier-C follow-up backlog. |
| **M-C2** — First vertical slice green | Deferred | Moved to post-Tier-C follow-up backlog. |
| **M-C3** — Parity + flake budget | Deferred | Moved to post-Tier-C follow-up backlog. |

---

## Task table

| ID | Priority | Task | Owner | Est | Depends on | Status | Acceptance criteria |
|----|----------|------|-------|-----|------------|--------|---------------------|
| **C0-01** | **HIGH** | Lock Tier C track (self-hosted Windows vs staging Twitch vs addon tests) | **Anatoly Ivanov** | 0.5 | — | done | Decision closed via issue #123 and kickoff record. |
| **C0-02** | **HIGH** | Define CI cost/concurrency policy (PR vs nightly Tier C, Actions minutes) | **Anatoly Ivanov** | 0.5 | C0-01 | done | Policy closed via issue #124 and kickoff decisions. |
| **C0-03** | **HIGH** | Windows E2E setup plan (self-hosted runner prerequisites + WoW/Desktop constraints) | **Anatoly Ivanov** | 2 | C0-01 | done | Planning scope closed via issue #125 with tracked follow-up execution. |
| **C0-04** | **HIGH** | Staging Twitch OAuth/config plan (Helix secrets + Environment gating) | **Anatoly Ivanov** | 1.5 | C0-01 | done | Planning scope closed via issue #126 with tracked follow-up execution. |
| **C0-05** | **MED** | Nightly Tier B vs PR Tier B decision (policy) | **Anatoly Ivanov** | 0.5 | C0-02 | done | Policy resolved and documented in kickoff notes. |
| **C0-06** | **MED** | `docs/`-only path filter decision for E2E workflows | **Anatoly Ivanov** | 0.5 | C0-02 | done | Deferred-by-policy decision recorded for follow-up phase. |
| **C1-01** | **HIGH** | Self-hosted Windows runner spec (WoW path, log path, secrets layout) | **Anatoly Ivanov** | 3 | C0-03 | blocked | Deferred to post-Tier-C follow-up backlog. |
| **C1-02** | **HIGH** | GitHub **Environment** definition for staging Twitch (Helix / EventSub secrets) | **Anatoly Ivanov** | 2 | C0-04 | blocked | Deferred to post-Tier-C follow-up backlog. |
| **C2-01** | **MED** | Extract/share HTTP choreography library (**SyntheticDesktop** + tests + optional Desktop) | **Anatoly Ivanov** | 5 | C0-01 | blocked | Deferred to post-Tier-C follow-up backlog. |
| **C2-02** | **MED** | Desktop vs **SyntheticDesktop** parity test plan | **Anatoly Ivanov** | 3 | C2-01 | blocked | Deferred to post-Tier-C follow-up backlog. |
| **C2-03** | **MED** | Addon `[MGM_*]` contract tests expansion | **Anatoly Ivanov** | 3 | — | blocked | Deferred to post-Tier-C follow-up backlog. |
| **C2-04** | **HIGH** | Optional staging job: real Helix send to test channel | **Anatoly Ivanov** | 5 | C1-02 | blocked | Deferred to post-Tier-C follow-up backlog. |
| **C2-05** | **MED** | Optional staging job: EventSub delivery to dev rig | **Anatoly Ivanov** | 5 | C1-02 | blocked | Deferred to post-Tier-C follow-up backlog. |
| **C3-01** | **MED** | Flake budget + retry policy for Tier C jobs | **Anatoly Ivanov** | 2 | C1-01 | blocked | Deferred to post-Tier-C follow-up backlog. |
| **C3-02** | **MED** | Observability: structured harness logs (correlation ids) | **Anatoly Ivanov** | 3 | C2-01 | blocked | Deferred to post-Tier-C follow-up backlog. |

---

## Documentation links (per task theme)

| Theme | Sections |
|-------|----------|
| Real WoW / Desktop | [`E2E_AUTOMATION_PLAN.md` section 1 — Overview](E2E_AUTOMATION_PLAN.md#1-overview), [`desktop/ReadME.md`](../components/desktop/ReadME.md) |
| Helix / Twitch | [`SPEC.md`](../overview/SPEC.md), [`backend/ReadME.md`](../components/backend/ReadME.md) |
| Addon | [`wow-addon/ReadME.md`](../components/wow-addon/ReadME.md) |
| CI patterns | [`E2E_AUTOMATION_PLAN.md` — E2E Pipeline Maintenance Guide](E2E_AUTOMATION_PLAN.md#e2e-pipeline-maintenance-guide) |

---

## Definition of done (Tier C program)

1. At least **one** Tier C governance slice (C0 planning and kickoff) is closed with linked issues and ownership.
2. Tier A/B baseline remains green per `TIER_B_POSTLAUNCH_VERIFICATION.md`.
3. Handover note exists in `TIER_C_HANDOVER.md` and closure report `TIER_C_CLOSURE_REPORT.md`.

---

## Closure note (2026-04-08)

Tier C planning/governance scope is complete. Engineering execution items C1-C3 are retained as post-Tier-C follow-up backlog and remain tracked in this table as `blocked` until re-scoped.

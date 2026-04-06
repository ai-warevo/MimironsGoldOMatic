<!-- Updated: 2026-04-06 (Post‑Tier B execution) -->

# Tier C — implementation tasks

**Requirements:** [`TIER_C_REQUIREMENTS.md`](TIER_C_REQUIREMENTS.md) · **E2E plan (Tier C scope):** [`E2E_AUTOMATION_PLAN.md` — Tier C](E2E_AUTOMATION_PLAN.md#tier-c-future-scope--requirements) · **Issue:** [#16](https://github.com/ai-warevo/MimironsGoldOMatic/issues/16)

**Legend:** **Priority**: `HIGH` | `MED` | `LOW` · **Est** = engineering days (rough) · **Status:** `todo` | `progress` | `done` | `blocked`

---

## Milestones and target dates

| Milestone | Target | Notes |
|-----------|--------|--------|
| **M-C0** — Prioritization locked | *Team sets* | Choose Windows E2E vs staging Twitch vs addon-first (see **C0** tasks). |
| **M-C1** — Runner / env spec approved | *TBD* | Spec in [`SETUP-for-developer.md`](../setup/SETUP-for-developer.md) + Tier C section of E2E plan. |
| **M-C2** — First vertical slice green | *TBD* | One chosen slice end-to-end (manual or CI). |
| **M-C3** — Parity + flake budget | *TBD* | SyntheticDesktop vs Desktop divergence documented; p95 stable. |

---

## Task table

| ID | Priority | Task | Owner | Est | Depends on | Status | Acceptance criteria |
|----|----------|------|-------|-----|------------|--------|---------------------|
| **C0-01** | **HIGH** | Lock Tier C track (self-hosted Windows vs staging Twitch vs addon tests) | **Anatoly Ivanov** | 0.5 | — | todo | A written decision is added to `TIER_C_REQUIREMENTS.md` §1 and linked from `TIER_C_KICKOFF_PLAN.md`. |
| **C0-02** | **HIGH** | Define CI cost/concurrency policy (PR vs nightly Tier C, Actions minutes) | **Anatoly Ivanov** | 0.5 | C0-01 | todo | Policy documented in `docs/reference/WORKFLOWS.md` or `E2E_AUTOMATION_PLAN.md`, including when Tier C runs and who can dispatch it. |
| **C0-03** | **HIGH** | Windows E2E setup plan (self-hosted runner prerequisites + WoW/Desktop constraints) | **Anatoly Ivanov** | 2 | C0-01 | todo | Runbook drafted: runner setup, WoW install path policy, log locations, cleanup, artifact upload plan; explicitly **no secrets** in logs. |
| **C0-04** | **HIGH** | Staging Twitch OAuth/config plan (Helix secrets + Environment gating) | **Anatoly Ivanov** | 1.5 | C0-01 | todo | GitHub **Environment** approach defined (names, required reviewers), plus a secrets checklist and “never on fork PRs” rule. |
| **C0-05** | **MED** | Nightly Tier B vs PR Tier B decision (policy) | **Anatoly Ivanov** | 0.5 | C0-02 | todo | Decision recorded + reflected in workflow triggers; rollback instructions documented. |
| **C0-06** | **MED** | `docs/`-only path filter decision for E2E workflows | **Anatoly Ivanov** | 0.5 | C0-02 | todo | Either: (a) path filters implemented and documented, or (b) explicit decision recorded explaining why not. |
| **C1-01** | **HIGH** | Self-hosted Windows runner spec (WoW path, log path, secrets layout) | **Anatoly Ivanov** | 3 | C0-03 | todo | A runnable spec exists with prerequisites, cleanup, and artifact retention; includes a minimal “smoke” workflow that proves the runner can execute a no-secrets job. |
| **C1-02** | **HIGH** | GitHub **Environment** definition for staging Twitch (Helix / EventSub secrets) | **Anatoly Ivanov** | 2 | C0-04 | todo | Environment(s) documented; fork/PR rules noted; secrets set checklist produced (without values). |
| **C2-01** | **MED** | Extract/share HTTP choreography library (**SyntheticDesktop** + tests + optional Desktop) | **Anatoly Ivanov** | 5 | C0-01 | todo | A shared module is consumed by SyntheticDesktop and at least one test; boundaries documented. |
| **C2-02** | **MED** | Desktop vs **SyntheticDesktop** parity test plan | **Anatoly Ivanov** | 3 | C2-01 | todo | A matrix covers endpoints/headers/bodies/order/error-handling; automated or scripted checklist exists. |
| **C2-03** | **MED** | Addon `[MGM_*]` contract tests expansion | **Anatoly Ivanov** | 3 | — | todo | Tests cover tag formats and gating logic per `docs/overview/SPEC.md`; runs in `unit-integration-tests.yml`. |
| **C2-04** | **HIGH** | Optional staging job: real Helix send to test channel | **Anatoly Ivanov** | 5 | C1-02 | todo | Gated `workflow_dispatch`; never on fork PRs; tokens redacted; verifies a real Helix send and stores only non-sensitive evidence. |
| **C2-05** | **MED** | Optional staging job: EventSub delivery to dev rig | **Anatoly Ivanov** | 5 | C1-02 | todo | Gated job validates real EventSub ingest; failures link to runbook; no secrets leaked in artifacts. |
| **C3-01** | **MED** | Flake budget + retry policy for Tier C jobs | **Anatoly Ivanov** | 2 | C1-01 | todo | Max retries documented; Tier B remains deterministic PR gate; escalation path defined. |
| **C3-02** | **MED** | Observability: structured harness logs (correlation ids) | **Anatoly Ivanov** | 3 | C2-01 | todo | Correlation id conventions documented; logs usable in CI artifacts and local runs. |

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

1. At least **one** Tier C slice meets its acceptance criteria in the table above with **no regression** to **Tier A/B** on default PR CI.
2. Secrets and logs reviewed for **no broadcaster token exposure**.
3. Handover note added to [`TIER_B_HANDOVER.md`](TIER_B_HANDOVER.md) or successor Tier C handover doc.

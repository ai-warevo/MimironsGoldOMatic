<!-- Updated: 2026-04-06 (Tier B closure + Tier C kick-off) -->

# Tier C — implementation tasks

**Requirements:** [`TIER_C_REQUIREMENTS.md`](TIER_C_REQUIREMENTS.md) · **E2E plan (Tier C scope):** [`E2E_AUTOMATION_PLAN.md` — Tier C](E2E_AUTOMATION_PLAN.md#tier-c-future-scope--requirements) · **Issue:** [#16](https://github.com/ai-warevo/MimironsGoldOMatic/issues/16)

**Legend:** **Owner** = role placeholder; **Est** = engineering days (rough); **Status:** `todo` | `progress` | `done` | `blocked`

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

| ID | Task | Owner | Est | Depends on | Status | Acceptance criteria |
|----|------|-------|-----|------------|--------|---------------------|
| **C0-01** | Prioritize Tier C track (self-hosted Windows vs staging Twitch vs addon tests) | EM + Tech lead | 0.5 | — | todo | Decision recorded in [`TIER_C_REQUIREMENTS.md`](TIER_C_REQUIREMENTS.md) section 1 + meeting notes linked. |
| **C0-02** | Cost/concurrency policy (PR vs nightly Tier C, Actions minutes) | EM | 0.5 | C0-01 | todo | Written policy in [`E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md) or `WORKFLOWS.md`. |
| **C1-01** | Self-hosted Windows runner spec (WoW path, log path, secrets layout) | DevOps + Desktop | 3 | C0-01 | todo | Runbook section: prerequisites, cleanup, artifact upload **without** token leakage. |
| **C1-02** | GitHub **Environment** definition for staging Twitch (Helix / EventSub secrets) | Backend + EM | 2 | C0-01 | todo | Environment name(s) documented; fork/PR rules noted. |
| **C2-01** | Extract or share HTTP choreography library (**SyntheticDesktop** + tests + optional Desktop) | Backend + Desktop | 5 | C0-01 | todo | Single module consumed by mock + tests; README explains boundaries. |
| **C2-02** | Desktop vs **SyntheticDesktop** parity test plan | Desktop + QA | 3 | C2-01 | todo | Matrix of endpoints, headers, error handling; automated or scripted checklist. |
| **C2-03** | Addon `[MGM_*]` contract tests expansion | Addon | 3 | — | todo | Tests in `src/Tests/MimironsGoldOMatic.WoWAddon.Tests/` cover tag formats per [`SPEC.md`](../overview/SPEC.md). |
| **C2-04** | Optional staging job: real Helix send to test channel | Backend | 5 | C1-02 | todo | Gated `workflow_dispatch`; never on forked PRs; redacts tokens in logs. |
| **C2-05** | Optional staging job: EventSub delivery to dev rig | Backend + Extension | 5 | C1-02 | todo | Documented failure modes; aligns with MVP broadcaster scope. |
| **C3-01** | Flake budget + retry policy for Tier C jobs | DevOps | 2 | C1-01 | todo | Document max retries; Tier B remains deterministic PR gate. |
| **C3-02** | Observability: structured harness logs (correlation ids) | Backend + CI | 3 | C2-01 | todo | Log fields documented; works locally and on runner. |

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

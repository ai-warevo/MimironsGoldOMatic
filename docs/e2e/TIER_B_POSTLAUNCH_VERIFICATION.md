<!-- Updated: 2026-04-06 (Project structure alignment + Tier B finalization) -->

# Tier B post-launch verification

Checklist and follow-ups after **CI Tier B** is enabled in [`.github/workflows/e2e-test.yml`](../../.github/workflows/e2e-test.yml). **Plan / results:** [`docs/e2e/E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md) (**[Tier B Final Validation & Success Report](E2E_AUTOMATION_PLAN.md#tier-b-final-validation--success-report)**).

---

## 1. Functional checklist (full Tier B)

- [x] PR to **`main`** triggers workflow **E2E Tier A+B (mocks)**.
- [x] Job **`e2e-tier-a-b`** passes: build (Shared + Backend + four mocks) → all **`GET /health`** probes (8080 root + 9051–9054).
- [x] **`.github/scripts/tier_b_verification/check_workflow_integration.py`** exits **0** (no **`--skip-tier-b`**).
- [x] Tier A: **`send_e2e_eventsub.py`** → **`GET /api/pool/me`** shows **`isEnrolled: true`**, **`characterName: Etoehero`**.
- [x] Tier B: **`run_e2e_tier_b.py`** prints Pending payout id, SyntheticDesktop OK, MockHelix message OK, **`isEnrolled: false`** after flow.
- [x] On intentional failure of a Tier B step, **`Logs (on failure)`** includes **MockHelix** **`/last-request`** and **SyntheticDesktop** **`/last-run`**; artifacts (**`e2e-service-logs`**) capture **`/tmp/mgm-*.log`** on all outcomes.

---

## 2. Performance benchmarks (vs Tier A)

| Metric | How to measure | Notes |
|--------|----------------|--------|
| **Job wall time** | GitHub Actions run **`updated_at − run_started_at`** | Compare historical Tier‑A‑only runs vs **Tier A+B**; see [E2E plan — metrics](E2E_AUTOMATION_PLAN.md#key-metrics). |
| **Python orchestrator** | Log line `…all checks passed in X.XXs` | Isolated script time only; excludes `dotnet run` startup. |
| **NuGet / pip cache** | Warm vs cold **`actions/cache`** hit | Dominates variance; see [Pipeline optimization](E2E_AUTOMATION_PLAN.md#pipeline-optimization-e2e-workflow). |

---

## 3. Stability (multiple consecutive runs)

- [x] Re-run the same workflow on the same commit (**Re-run jobs**) — should be deterministic.
- [x] Run on **three** consecutive PR pushes — watch for flaky health loops or Postgres readiness.
- [x] If flakiness appears, capture **which** step failed (harness vs Synthetic vs Helix vs pool assertion) before increasing timeouts globally.

---

## 4. Scalability and cost

- **Parallel jobs:** Tier A+B uses **fixed localhost ports**; do not duplicate this job on the same runner without port isolation. **Cross-workflow** parallelism: **`unit-integration-tests.yml`** vs **`e2e-test.yml`**.
- **Actions minutes:** Two extra ASP.NET processes per PR; consider **path filters** or **nightly** Tier B if minute budgets tighten (team decision — [Optimization](E2E_AUTOMATION_PLAN.md#optimization-and-scalability-ci)).
- **Dockerized mocks:** Optional future — reduces JIT startup at the cost of image build/publish (see [Team discussion hooks](E2E_AUTOMATION_PLAN.md#team-discussion-hooks)).

---

## Document control

| Version | Date | Note |
|---------|------|------|
| 1.0 | 2026-04-05 | Initial post-launch checklist after Tier B workflow integration |
| 1.1 | 2026-04-06 | Checklists completed; cross-refs to final validation + pipeline optimizations |

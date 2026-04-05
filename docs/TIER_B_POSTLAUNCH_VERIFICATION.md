<!-- Updated: 2026-04-05 (Tier B integration & first run) -->

# Tier B post-launch verification

Checklist and follow-ups after **CI Tier B** is enabled in [`.github/workflows/e2e-test.yml`](../.github/workflows/e2e-test.yml). **Plan / results:** [`docs/E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md) (**[Tier B Integration Results](E2E_AUTOMATION_PLAN.md#tier-b-integration-results)**).

---

## 1. Functional checklist (full Tier B)

- [ ] PR to **`main`** triggers workflow **E2E Tier A+B (mocks)**.
- [ ] Job **`e2e-tier-a-b`** passes: build (Shared + Backend + four mocks) → all **`GET /health`** probes (8080 root + 9051–9054).
- [ ] **`scripts/tier_b_verification/check_workflow_integration.py`** exits **0** (no **`--skip-tier-b`**).
- [ ] Tier A: **`send_e2e_eventsub.py`** → **`GET /api/pool/me`** shows **`isEnrolled: true`**, **`characterName: Etoehero`**.
- [ ] Tier B: **`run_e2e_tier_b.py`** prints Pending payout id, SyntheticDesktop OK, MockHelix message OK, **`isEnrolled: false`** after flow.
- [ ] On intentional failure of a Tier B step, **`Logs (on failure)`** includes **MockHelix** **`/last-request`** and **SyntheticDesktop** **`/last-run`**.

---

## 2. Performance benchmarks (vs Tier A)

| Metric | How to measure | Notes |
|--------|----------------|--------|
| **Job wall time** | GitHub Actions run **`updated_at − run_started_at`** | Compare last Tier‑A‑only historical run (if available) vs first Tier A+B run; expect modest increase (see [Integration Results](E2E_AUTOMATION_PLAN.md#tier-b-integration-results)). |
| **Python orchestrator** | Log line `…all checks passed in X.XXs` | Isolated script time only; excludes `dotnet run` startup. |
| **NuGet cache** | Warm vs cold **`actions/cache`** hit | Dominates variance more than Tier B mocks. |

---

## 3. Stability (multiple consecutive runs)

- [ ] Re-run the same workflow on the same commit (**Re-run jobs**) — should be deterministic.
- [ ] Run on **three** consecutive PR pushes — watch for flaky health loops or Postgres readiness.
- [ ] If flakiness appears, capture **which** step failed (harness vs Synthetic vs Helix vs pool assertion) before increasing timeouts globally.

---

## 4. Scalability and cost

- **Parallel jobs:** Tier A+B uses **fixed localhost ports**; do not duplicate this job on the same runner without port isolation.
- **Actions minutes:** Two extra ASP.NET processes per PR; consider **path filters** or **nightly** Tier B if minute budgets tighten (team decision — [Optimization](E2E_AUTOMATION_PLAN.md#optimization-and-scalability-ci)).
- **Dockerized mocks:** Optional future — reduces JIT startup at the cost of image build/publish (see [Team discussion hooks](E2E_AUTOMATION_PLAN.md#team-discussion-hooks)).

---

## Document control

| Version | Date | Note |
|---------|------|------|
| 1.0 | 2026-04-05 | Initial post-launch checklist after Tier B workflow integration |

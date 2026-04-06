<!-- Updated: 2026-04-06 (Post‑Tier B execution) -->

# Tier B / E2E pipeline — maintenance checklist

**Workflow:** [`.github/workflows/e2e-test.yml`](../../.github/workflows/e2e-test.yml) · **Handover context:** [`TIER_B_HANDOVER.md`](TIER_B_HANDOVER.md) · **Full guide:** [`E2E_AUTOMATION_PLAN.md` — E2E Pipeline Maintenance Guide](E2E_AUTOMATION_PLAN.md#e2e-pipeline-maintenance-guide)

---

## Verification of monitoring & alerting (operators)

These checks validate that **monitoring** and **alerting** remain functional after workflow edits.

### A) Weekly health report (manual dispatch)

**Workflow:** [`.github/workflows/e2e-weekly-health-report.yml`](../../.github/workflows/e2e-weekly-health-report.yml)

1. GitHub → **Actions** → workflow **“E2E weekly health report”**.
2. Select **Run workflow** (manual `workflow_dispatch`).
3. Open the run → confirm the **Summary** includes:
   - 30‑day rolling success/failure counts
   - Data sourced from the **GitHub Actions REST API** (see the script’s call to `actions.listWorkflowRuns`)

**What “good” looks like**
- Summary table shows **Completed runs (excl. cancelled)** and non-negative counts for **Success** and **Failure**.
- The workflow link points at `e2e-test.yml` runs.

**Record**
- **Latest manual run URL**: *TBD (paste Actions run URL)*  
- **Observed counts**: *TBD*

### B) Consecutive-failure alert (deduplicated issue)

**Workflow:** [`.github/workflows/e2e-consecutive-failure-alert.yml`](../../.github/workflows/e2e-consecutive-failure-alert.yml)

This workflow should open **exactly one** GitHub issue when the **two most recent** completed runs of `e2e-test.yml` are both **failure** (excluding cancelled/skipped).

**How to simulate two consecutive failures safely**
- Create a temporary PR to `main` that introduces a deliberate failure in `.github/workflows/e2e-test.yml` (e.g., add a one-off step `run: exit 1` near the end of the job).
- Run the workflow twice as **two separate completed failures**:
  - Push commit (run fails)
  - Push another commit (run fails again) or use **Re-run jobs** if it creates a new completed run entry
- Confirm that:
  - **Exactly one** open issue exists with title `E2E Tier A+B: consecutive workflow failures`
  - The issue body contains links to the **latest** and **previous** failed runs
  - Additional failures while the issue is open do **not** create duplicate issues (dedupe works)

**Record**
- **Alert issue URL**: *TBD*
- **Failed run URLs**: *TBD (latest)*, *TBD (previous)*
- **Deduplication check**: *TBD (confirmed one open issue only)*

### C) `e2e-test.yml` job Summary table (timing boundaries)

**Workflow:** [`.github/workflows/e2e-test.yml`](../../.github/workflows/e2e-test.yml)

1. Open any recent run → job **`e2e-tier-a-b`**.
2. Confirm the job **Summary** contains a table titled **“E2E performance (this run)”** with:
   - **Total job wall-clock** seconds
   - **Tier B step boundary** seconds (or explicitly `n/a` if timestamps missing)

**Record**
- **Example run URL**: *TBD*
- **Observed total wall time**: *TBD*
- **Observed Tier B boundary time**: *TBD*

## Weekly verification (≈15 min)

- [ ] Open [E2E workflow runs](https://github.com/ai-warevo/MimironsGoldOMatic/actions/workflows/e2e-test.yml); confirm latest PRs to **`main`** show **green** **`e2e-tier-a-b`** where expected.
- [ ] Skim **Job summary** on the latest run (performance table: total wall time + Tier B slice).
- optional: Open latest **`e2e-weekly-health-report`** workflow run (scheduled Mondays) for rolling success rate.
- [ ] If any failure: download **`e2e-service-logs`** artifact and trace using the [**Troubleshooting matrix** in `TIER_B_HANDOVER.md`](TIER_B_HANDOVER.md#4-troubleshooting-matrix).

---

## Monthly review (≈45 min)

- [ ] **Actions Insights:** compare median job duration for **`e2e-test.yml`** vs prior month; note regressions after dependency or workflow edits.
- [ ] **Caches:** confirm **NuGet** / **pip** keys still reference [`src/**/*.csproj`](../../src/) and [`.github/scripts/tier_b_verification/requirements.txt`](../../.github/scripts/tier_b_verification/requirements.txt).
- [ ] **Docs:** verify [`E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md) document control row matches substantive workflow changes.
- [ ] **Tier C backlog:** skim [`TIER_C_IMPLEMENTATION_TASKS.md`](TIER_C_IMPLEMENTATION_TASKS.md) statuses; reprioritize with team.

---

## Pipeline update checklist (before merging workflow changes)

- [ ] **`dotnet build`** scope still includes any **new** mock project referenced by **`dotnet run`** steps.
- [ ] Port matrix **8080 / 9051–9054** unchanged or documented in [`TIER_B_HANDOVER.md`](TIER_B_HANDOVER.md) + workflow comments.
- [ ] **`Mgm__EnableE2eHarness`** / **`Development`** assumptions preserved for harness routes.
- [ ] Log artifact paths (`/tmp/mgm-*.log`) still match **upload-artifact** step.
- [ ] Smoke locally or on draft PR: Tier A enrollment + Tier B orchestrator.

---

## Emergency response (failed E2E on `main` PR)

1. **Reproduce:** Re-run failed job; confirm not transient runner flake.
2. **Artifacts:** Pull **`e2e-service-logs`**; read **`mgm-backend.log`**, **`mgm-tier-b-orchestrator.log`**, **`mgm-mock-helix.log`**.
3. **Bisect:** Recent PRs touching Backend **`HelixChatService`**, **`TwitchOptions`**, mocks, or **`run_e2e_tier_b.py`**.
4. **Contain:** If main is blocked, consider revert; open issue with log excerpts (redact secrets — workflow uses inline test tokens only today).
5. **Alerting:** If [**e2e-consecutive-failure-alert**](../../.github/workflows/e2e-consecutive-failure-alert.yml) opened an issue, use it as the war-room thread.

---

## Related automation

| Workflow | Purpose |
|----------|---------|
| [`e2e-test.yml`](../../.github/workflows/e2e-test.yml) | PR **Tier A + B** gate |
| [`e2e-weekly-health-report.yml`](../../.github/workflows/e2e-weekly-health-report.yml) | Scheduled / manual rolling stats |
| [`e2e-consecutive-failure-alert.yml`](../../.github/workflows/e2e-consecutive-failure-alert.yml) | Opens issue when **two** consecutive **`e2e-test.yml`** runs **fail** |

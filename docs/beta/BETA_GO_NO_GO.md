<!-- Updated: 2026-04-06 (solo-owner go decision) -->

# Beta Go/No-Go Gate

## 1. Purpose

Define a single decision gate for starting the Beta phase with clear, measurable criteria and named sign-off owners.

Decision outcomes:
- **Go**: all mandatory gates pass.
- **Conditional Go**: mandatory gates pass, but accepted non-blocking risks remain with owners and deadlines.
- **No-Go**: any mandatory gate fails or a hard blocker is active.

---

## 2. Scope

This gate covers:
- Backend (`MimironsGoldOMatic.Backend.Api` + **`Backend.*`** libraries)
- Desktop (`MimironsGoldOMatic.Desktop`)
- WoW addon (`MimironsGoldOMatic.WoWAddon`)
- Twitch Extension (`MimironsGoldOMatic.TwitchExtension`)
- CI/E2E workflows and operator runbooks

Reference documents:
- `docs/overview/SPEC.md`
- `docs/overview/ROADMAP.md`
- `docs/overview/INTERACTION_SCENARIOS.md`
- `docs/e2e/E2E_AUTOMATION_PLAN.md`
- `docs/e2e/TIER_C_CLOSURE_REPORT.md`
- `docs/e2e/TIER_C_HANDOVER.md`

---

## 3. Exit Criteria (Mandatory)

### A) Functional correctness

- [ ] Core MVP flow validated for SC-001 and SC-005 with evidence links.
- [ ] No open **P0** defects.
- [ ] No open **P1** defects in payout lifecycle transitions (`Pending`, `InProgress`, `Sent`, `Failed`, `Cancelled`, `Expired`).
- [ ] Contract parity check complete for key API endpoints (`claim`, `pending`, `status`, `confirm-acceptance`, `my-last`, `roulette/state`, `pool/me`, `verify-candidate`).

Pass threshold:
- 100% pass for required scenario checks in the Beta test run.

### B) Reliability and E2E

- [ ] CI Tier A+B workflow is stable for the latest 5 runs.
- [ ] Success rate for the latest 10 `e2e-test.yml` runs is >= 90%.
- [ ] No unresolved flaky test marked as blocking in Tier A/B paths.
- [ ] Manual live-stack operator rehearsal completed at least once and documented against TC rows.

Pass threshold:
- All four checks complete with evidence links.

### C) Security and configuration

- [ ] Environment secrets are not hardcoded in repository files.
- [ ] Desktop `Mgm:ApiKey` handling and rotation/reset procedure documented and tested.
- [ ] Extension JWT validation configuration verified for intended Beta environment.
- [ ] No open high-severity security finding without mitigation plan and owner.

Pass threshold:
- 0 unmitigated high-severity findings.

### D) Operations and support readiness

- [ ] Operator runbook exists and covers start/recovery/escalation.
- [ ] Monitoring and alerting runbook is verified (weekly health + failure alert flow).
- [ ] Incident response owner rotation is assigned.
- [ ] Known-risk register is current with mitigation dates.

Pass threshold:
- 100% of checklist items completed with named owners.

### E) Release readiness

- [ ] Release packaging process validated for Desktop, WoW addon, Twitch Extension, and Backend artifact/image outputs.
- [ ] Rollback steps documented and tested on staging/local rehearsal.
- [ ] Release notes template prepared for Beta communication.

Pass threshold:
- 1 successful rehearsal run with artifact and rollback evidence.

---

## 4. Hard Blockers (Automatic No-Go)

Any one item below triggers **No-Go**:
- Active P0 defect.
- Broken or failing payout state transition in core flow.
- Inability to complete one full operator rehearsal (chat -> payout -> sent confirmation).
- Missing recovery path for stuck `InProgress` payouts.
- Unmitigated high-severity security issue.

---

## 5. Sign-Off Owners (Solo Mode)

| Area | Owner role | Named owner | Status | Notes |
|---|---|---|---|---|
| Product/Scope | Product owner | Anatoly Ivanov | Approved | Final scope lock |
| Backend/API | Backend lead | Anatoly Ivanov | Approved | Contract + lifecycle |
| Desktop/WinAPI | Desktop lead | Anatoly Ivanov | Approved | Injection/reconciliation |
| WoW addon | Addon lead | Anatoly Ivanov | Approved | Log tags/mail flow |
| Twitch Extension | Frontend lead | Anatoly Ivanov | Approved | Viewer UX/read model |
| QA/E2E | QA lead | Anatoly Ivanov | Approved | SC/TC evidence |
| Ops/Release | DevOps lead | Anatoly Ivanov | Approved | Workflow and rollback |
| Security | Security reviewer | Anatoly Ivanov | Approved | Risk acceptance |

Sign-off rule:
- **Go** requires all owner statuses set to **Approved**.
- **Conditional Go** requires explicit exception list with due dates.
- In solo mode, the single owner accepts both implementation and risk sign-off decisions.

---

## 6. Decision Record

| Field | Value |
|---|---|
| Decision date (UTC) | 2026-04-06 |
| Decision | Go |
| Evidence bundle | `docs/overview/ROADMAP.md`, `docs/reference/IMPLEMENTATION_READINESS.md`, `docs/e2e/TIER_C_CLOSURE_REPORT.md`, `docs/e2e/TIER_C_HANDOVER.md` |
| Exceptions accepted | None |
| Next review date | 2026-04-13 |

---

## 7. Evidence Checklist (fill before decision)

- [ ] Link to latest successful `e2e-test.yml` runs
- [ ] Link to manual SC/TC run record
- [ ] Link to risk log snapshot
- [ ] Link to release rehearsal artifacts
- [ ] Link to rollback rehearsal notes


<!-- Updated: 2026-04-06 (Transition complete & Tier C launch) -->

# Tier C — handover prep (checklist + templates)

Use this file throughout Tier C so that closure/handover is low-friction and consistent with Tier B.

---

## 1. Running checklist (keep up to date)

### Documentation

- [ ] `TIER_C_REQUIREMENTS.md` updated to match current decisions (C0 outcomes)
- [ ] `TIER_C_IMPLEMENTATION_TASKS.md` statuses maintained
- [ ] `TIER_C_PROGRESS.md` updated weekly
- [ ] Any Tier C workflow/runbook changes reflected in `docs/reference/WORKFLOWS.md`

### Verification and safety

- [ ] Tier A + Tier B PR CI remains green (no regressions)
- [ ] Tier C workflows (if added) are gated (`workflow_dispatch` / Environments) and never run on fork PRs
- [ ] Artifacts/logs reviewed for **no secret leakage**
- [ ] “Five consecutive green runs” evidence captured for any new Tier C CI gates

### Knowledge transfer

- [ ] Demo script prepared and rehearsed
- [ ] Demo environment checklist validated
- [ ] Invite sent, attendees confirmed, meeting recorded/notes captured

### Closure artifacts

- [ ] Tier C closure report written and linked from `docs/ReadME.md`
- [ ] Tier C handover doc for maintainers written (mirrors Tier B handover style)

---

## 2. Templates (copy/paste)

### A) Closure report skeleton

```text
<!-- Updated: YYYY-MM-DD (Tier C closure) -->

# Tier C — closure report

## Summary
- What Tier C added
- What remains out of scope

## Monitoring/verification evidence
- Run URLs, issue links, artifact links

## Operational handover
- Where maintainers start
- SME contacts

## Follow-ups
- Risks
- Next-tier backlog
```

### B) Knowledge transfer session skeleton

```text
<!-- Updated: YYYY-MM-DD (Tier C knowledge transfer) -->

# Tier C — knowledge transfer

## Demo script
- Architecture
- Workflow triggers and gating
- Failure triage via artifacts

## Demo environment checklist
- Accounts
- Runners
- Secrets policy
```

---

## 3. Lessons learned to carry forward (Tier B → Tier C)

- Prefer **artifact-first troubleshooting** (make “what to download and read” explicit).
- Add monitoring/alerting early; avoid silent CI degradation.
- Keep optional Tier C jobs gated until stable; preserve Tier B as deterministic PR coverage.

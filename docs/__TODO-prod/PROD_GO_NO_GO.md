<!-- Updated: 2026-04-06 -->

# Production Go/No-Go Gate

## 1. Purpose

Define a single decision gate for Production launch with measurable security, reliability, and operations criteria.

Decision outcomes:
- **Go**: all mandatory criteria pass, or only accepted low-risk exceptions remain.
- **No-Go**: any critical criterion fails or evidence is incomplete.

## 2. Scope

Production hardening scope from roadmap:
- Twitch Extension JWT hardening (issuer validation + secret rotation runbooks)
- Secrets/config hardening across environments
- Security review (abuse cases + logging hygiene)
- CI workflow hardening for .NET, frontend, and E2E

## 3. Exit Criteria

### A) Authentication and cryptography posture

- [ ] Twitch Extension JWT validation includes issuer validation for Production profile.
- [ ] Secret rotation runbooks exist, are tested, and include rollback steps.
- [ ] JWT failure paths are observable and do not leak sensitive token data.

### B) Secrets and configuration hardening

- [ ] No production secrets are committed or exposed in logs/artifacts.
- [ ] Environment-specific config boundaries are documented and enforced.
- [ ] Rotation/reset procedures for critical secrets are validated end-to-end.

### C) Security review completion

- [ ] Abuse-case review completed with findings severity-ranked.
- [ ] Logging hygiene reviewed; sensitive fields are redacted or omitted.
- [ ] High-severity findings are fixed or explicitly accepted with mitigation owner/date.

### D) CI and operational reliability

- [ ] Hardened CI workflows are green for representative runs (`unit-integration-tests.yml`, `e2e-test.yml`, release/monitoring workflows).
- [ ] Alerting/runbooks for CI and production-facing failures are validated.
- [ ] Release/rollback rehearsal evidence is attached.

## 4. Owners and Sign-off

| Area | Owner role | Primary owner | Status | Notes |
| --- | --- | --- | --- | --- |
| Backend/Auth | Backend lead | Anatoly Ivanov | Pending | JWT + key posture |
| Desktop/Operator | Desktop lead | Anatoly Ivanov | Pending | Runbook operability |
| Frontend/Extension | Frontend lead | Anatoly Ivanov | Pending | Extension auth UX impact |
| Security | Security owner | Anatoly Ivanov | Pending | Findings/risk treatment |
| CI/Release | DevOps owner | Anatoly Ivanov | Pending | Workflow and release hardening |

## 5. Decision Record

| Field | Value |
| --- | --- |
| Decision date (UTC) | TBD |
| Decision | TBD (`Go` / `No-Go`) |
| Evidence bundle | TBD |
| Residual risks accepted | TBD |
| Next review date | TBD |

## 6. Evidence Checklist (fill before decision)

- [ ] Link to JWT issuer-hardening change set and tests.
- [ ] Link to secret rotation/reset runbooks and rehearsal evidence.
- [ ] Link to security review report and issue tracker references.
- [ ] Link to hardened CI workflow run history and failure-triage runbook.
- [ ] Link to production release package and rollback rehearsal report.

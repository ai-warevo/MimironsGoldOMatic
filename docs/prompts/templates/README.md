# Prompt Templates Index

This directory contains reusable prompt templates for common engineering workflows.

## Quick start

1. Pick the template that matches your task.
2. Copy its prompt skeleton into your task request.
3. Create task history under `docs/prompts/history/YYYY-MM-DD/NN-task-slug/`.
4. Fill `prompt.md`, `plan.md`, `checks.md`, and `report.md`.
5. Validate completion against `docs/prompts/templates/definition_of_done.md`.

## Which template should I use?

| Task type | Template | Use when |
|---|---|---|
| New capability | `feature.md` | You are adding new user-visible behavior or a new subsystem. |
| Defect/regression | `bugfix.md` | Something behaves incorrectly and needs root-cause-driven repair. |
| Internal cleanup | `refactor.md` | Improve structure/maintainability with preserved behavior. |
| Architecture decision | `tech-design.md` | You need alignment on a major design or risky change. |
| Quality gate/review | `review.md` | You want a structured code review and quality checklist. |
| Docs/onboarding | `docs-gen.md` | You need READMEs, onboarding docs, runbooks, or guides. |
| Time-boxed research | `spike.md` | Feasibility is unclear and you need evidence before building. |
| Performance tuning | `performance.md` | Latency/throughput/memory/cost goals are not met. |
| Security hardening | `security.md` | Threat modeling or sensitive path validation is required. |
| Incident analysis | `postmortem.md` | You need an outage/incident RCA with corrective actions. |
| Changelog/release comms | `release-notes.md` | You are preparing release notes with breaking-change details. |

## Recommended workflow

- Keep prompts concrete: scope, constraints, and success metrics.
- Ask for verification explicitly (tests, lint, manual steps).
- Require risk notes and rollback strategy for medium/high-risk work.
- Close every task with a completed `report.md` including `Potential technical debt`.

## Sample request (copy/paste)

```md
Use template: `docs/prompts/templates/bugfix.md`

Task:
Fix intermittent combat freeze after end-turn.

Requirements:
1) Create history folder under `docs/prompts/history/YYYY-MM-DD/NN-combat-freeze-fix/`.
2) Fill `prompt.md`, `plan.md`, `checks.md`, and `report.md`.
3) Implement fix with regression test.
4) Run tests and summarize verification.
5) Use command /commit and provide PR title/body.
```


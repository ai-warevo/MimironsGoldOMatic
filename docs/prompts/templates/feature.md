# New Feature Template

## When to use

Use this when you are adding a **new capability** (new user flow, new API, new domain concept, or a significant new slice of functionality).

## How to use (required workflow)

- Create a history folder: `docs/prompts/history/YYYY-MM-DD/NN-short-slug/`
- Save the exact request into `prompt.md`
- Write a concrete plan into `plan.md`
- Track progress in `checks.md`
- Finish with `report.md` (files changed + verification + `Potential technical debt`)

## Mandatory quality gate

- Before closing the task, validate against `docs/prompts/templates/definition_of_done.md`.
- In `report.md`, always include a `Potential technical debt` section.

## Prompt (copy/paste)

### Context

- **Problem**:
- **Why now**:
- **Users / stakeholders**:
- **Constraints** (time, tech, compatibility, infra):
- **Success metric(s)**:

### Requirements

- **Must have**:
- **Should have**:
- **Nice to have**:

### Non-goals

- **Explicitly out of scope**:

### Proposed solution

- **High-level approach**:
- **Architecture changes** (modules/classes/services impacted):
- **Data model / contracts** (types, DTOs, DB schema, API):
- **Backwards compatibility**:
- **Performance considerations**:
- **Security/privacy considerations**:

### UX / behavior details (if applicable)

- **Key flows**:
- **Edge cases**:
- **Error states**:
- **Accessibility**:

### Testing plan

- **Unit tests**:
- **Integration tests**:
- **Manual verification steps**:

### Rollout plan

- **Feature flags / gradual rollout**:
- **Migration steps**:
- **Observability** (logs/metrics/traces, dashboards, alerts):
- **Rollback plan**:

### Risks & mitigations

- **Risk 1**:
- **Risk 2**:

### Deliverables (Definition of Done)

- [ ] Implementation merged
- [ ] Tests added/updated and passing
- [ ] Lint/format clean
- [ ] Docs updated (README / ADR / runbook as needed)
- [ ] Telemetry/monitoring updated (if applicable)
- [ ] `docs/prompts/history/.../report.md` completed with `Potential technical debt`
- [ ] `docs/prompts/templates/definition_of_done.md` checked

## Filled example (mini)

### Context

- **Problem**: Players can’t quickly see party status; combat decisions feel random.
- **Why now**: New combat AI requires better player feedback.
- **Users / stakeholders**: Player, game designer.
- **Constraints**: Keep UI text-only for now.
- **Success metric(s)**: Reduce “missed heal” events by 30% in playtests.

### Requirements

- **Must have**: Party HUD showing HP and status effects.
- **Should have**: Highlight critical HP.
- **Nice to have**: Tooltip explanations.


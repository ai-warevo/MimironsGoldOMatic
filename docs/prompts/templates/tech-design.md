# Technical Design / RFC Template

## When to use

Use this for changes that need **alignment**: new subsystem, new persistence, public API, major refactor, cross-team impact, risky performance/security work.

## How to use (required workflow)

- Create a history folder: `docs/prompts/history/YYYY-MM-DD/NN-short-slug/`
- Save the exact request into `prompt.md`
- Write this RFC into `plan.md` or `rfc.md` (if you prefer)
- Track progress in `checks.md`
- Finish with `report.md` (including `Potential technical debt`)

## Mandatory quality gate

- Before closing the task, validate against `docs/prompts/templates/definition_of_done.md`.
- In `report.md`, always include a `Potential technical debt` section.

## Document

### Title

**<Short, specific title>**

### Status

- Draft / Proposed / Accepted / Implemented / Rejected / Superseded

### Summary

1-3 sentences describing what is changing and why.

### Goals

- **Goal 1**
- **Goal 2**

### Non-goals

- **Non-goal 1**

### Background / problem statement

- **What exists today**:
- **What’s broken / limiting**:

### Proposed approach

- **Architecture overview**:
- **Key components**:
- **Data model**:
- **Interfaces / contracts**:
- **Error handling**:
- **Observability** (logs/metrics/traces):
- **Security**:

### Alternatives considered

- **Alternative A**: pros/cons
- **Alternative B**: pros/cons

### Rollout / migration plan

- **Steps**:
- **Backwards compatibility**:
- **Rollback**:

### Performance & capacity

- **Expected load**:
- **Critical paths**:
- **Benchmarks / profiling plan**:

### Risks & mitigations

- **Risk**:
- **Mitigation**:

### Open questions

- **Question 1**:

### Decision

What was decided, by whom, and when.

## Filled example (mini)

### Summary

Introduce a deterministic turn scheduler to eliminate intermittent hangs and make AI simulation reproducible for tests.


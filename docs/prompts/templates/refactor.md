# Refactoring Template

## When to use

Use this when you want to **improve internal code structure** without changing external behavior (or with strictly controlled, intentional behavior changes).

## How to use (required workflow)

- Create a history folder: `docs/prompts/history/YYYY-MM-DD/NN-short-slug/`
- Save the exact request into `prompt.md`
- Write a concrete plan into `plan.md`
- Track progress in `checks.md`
- Finish with `report.md` (what changed + verification + `Potential technical debt`)

## Mandatory quality gate

- Before closing the task, validate against `docs/prompts/templates/definition_of_done.md`.
- In `report.md`, always include a `Potential technical debt` section.

## Prompt (copy/paste)

### Motivation

- **Pain today** (duplication, complexity, brittle tests, unclear boundaries):
- **Goal** (readability, modularity, performance, testability, maintainability):
- **Scope** (which modules/components):

### Current state (facts)

- **Key entry points**:
- **Core responsibilities**:
- **Constraints** (API stability, backward compatibility, deadlines):

### Target state

- **Proposed design**:
- **New boundaries** (modules/classes/functions):
- **Public contracts** (what must remain stable):

### Refactor strategy

- **Small steps** (sequence of safe `.cursors/commands/commit.md`):
- **Safety rails** (tests, feature flags, canaries):
- **Rollback plan**:

### Testing plan

- **Existing tests to rely on**:
- **New tests to add** (especially around contracts):

### Risk assessment

- **High-risk areas**:
- **Mitigation**:

### Definition of Done

- [ ] Behavior preserved (or deltas explicitly documented)
- [ ] Complexity reduced (explain how: fewer branches, smaller modules, etc.)
- [ ] Tests passing
- [ ] `docs/prompts/history/.../report.md` explains refactor impact and `Potential technical debt`
- [ ] `docs/prompts/templates/definition_of_done.md` checked

## Filled example (mini)

### Motivation

- **Pain today**: Combat AI logic is spread across 5 files with circular dependencies.
- **Goal**: Centralize decision policy and make actions composable/testable.
- **Scope**: `CombatAi*` modules only.


# Bug Fix Template

## When to use

Use this when something **works incorrectly** (crash, wrong output, regression, flaky test, broken UX, security bug).

## How to use (required workflow)

- Create a history folder: `docs/prompts/history/YYYY-MM-DD/NN-short-slug/`
- Save the exact request into `prompt.md`
- Write a concrete plan into `plan.md`
- Track progress in `checks.md`
- Finish with `report.md` (root cause + verification + `Potential technical debt`)

## Mandatory quality gate

- Before closing the task, validate against `docs/prompts/templates/definition_of_done.md`.
- In `report.md`, always include a `Potential technical debt` section.

## Prompt (copy/paste)

### Symptoms

- **User-visible impact**:
- **Frequency** (always / intermittent):
- **First observed** (version/commit/date):
- **Regression?** (yes/no/unknown):

### Reproduction

- **Steps to reproduce**:
- **Expected result**:
- **Actual result**:
- **Environment** (OS, runtime, config):

### Evidence

- **Logs / stack traces**:
- **Screenshots / recordings**:
- **Failing tests**:

### Root cause analysis

- **Root cause**:
- **Why it happened**:
- **Why it wasn’t caught** (test gap, missing validation, unclear contract):

### Fix approach

- **Proposed fix**:
- **Alternative considered**:
- **Compatibility / migration**:
- **Risk assessment**:

### Tests / verification

- **New/updated tests**:
- **Manual verification steps**:
- **Negative tests / edge cases**:

### Prevention

- **Follow-up tasks** (monitoring, lint rule, extra test coverage, docs):

### Definition of Done

- [ ] Bug no longer reproduces
- [ ] Regression test added (when feasible)
- [ ] No new lints introduced
- [ ] `docs/prompts/history/.../report.md` includes root cause, verification, and `Potential technical debt`
- [ ] `docs/prompts/templates/definition_of_done.md` checked

## Filled example (mini)

### Symptoms

- **User-visible impact**: Combat occasionally hangs after “End Turn”.
- **Frequency**: ~1/20 combats.

### Root cause analysis

- **Root cause**: Turn-advance loop waits on an event never fired when a stunned unit dies mid-turn.
- **Why it wasn’t caught**: No test covers “stunned + death + end turn” combination.


# Code Review & Quality Template

## When to use

Use this to request a **structured code review** (pre-merge, post-merge audit, quality bar check, or refactor safety review).

## Mandatory quality gate

- Before closing the task, validate against `docs/prompts/templates/definition_of_done.md`.
- In `report.md`, always include a `Potential technical debt` section.

## Prompt (copy/paste)

### Review context

- **Change summary**:
- **Risk level** (low/medium/high):
- **Affected areas**:
- **Out of scope**:

### What to review for (checklist)

- **Correctness**
  - [ ] Meets stated requirements
  - [ ] Handles edge cases and error paths
  - [ ] No unintended behavior changes
- **Design**
  - [ ] Clear ownership/boundaries between modules
  - [ ] APIs are coherent and consistent
  - [ ] No unnecessary coupling
- **Readability**
  - [ ] Names reflect intent
  - [ ] Complexity is reasonable (functions not doing too much)
- **Testing**
  - [ ] Tests cover key logic and boundaries
  - [ ] Tests are deterministic and not flaky
- **Performance**
  - [ ] No obvious hot-path regressions
  - [ ] Avoids unnecessary allocations/IO
- **Security**
  - [ ] Validates inputs
  - [ ] No secrets logged
  - [ ] Authz/authn considered where relevant
- **Operational readiness**
  - [ ] Logging/metrics useful and non-noisy
  - [ ] Rollback plan or safe failure modes
- **Docs**
  - [ ] User/dev docs updated if behavior changes

### Requested output format

- **Top issues** (must-fix)
- **Suggestions** (nice-to-have)
- **Questions** (clarifications)
- **Confidence level** (high/medium/low) with reason

## Filled example (mini)

### Review context

- **Change summary**: Add party HUD component and wire into combat screen.
- **Risk level**: Medium (touches render loop).
- **Affected areas**: UI, combat state store.


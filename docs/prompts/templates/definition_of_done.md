# Definition of Done (DoD)

Use this checklist as a mandatory quality gate for any task executed from templates in this folder.

## Mandatory Criteria (10/10)

- [ ] Scope is fully implemented according to the request and agreed constraints.
- [ ] Behavior changes are covered by tests (or a clear reason is documented if tests are not feasible).
- [ ] Existing tests pass (`dotnet test src/MimironsGoldOMatic.sln`) after changes.
- [ ] No new linter/format issues are introduced in modified files.
- [ ] User-facing docs are updated when behavior, commands, or workflow changed.
- [ ] Code comments are written in English.
- [ ] User-facing strings are consistent and localized as requested (English unless stated otherwise).
- [ ] Error handling is explicit for newly added failure paths.
- [ ] Potential risks and rollback impact are documented.
- [ ] `report.md` includes a **Potential technical debt** section:
  - Workarounds/hacks introduced
  - Trade-offs accepted short-term
  - Follow-up actions and owners (if known)

## Final Gate

Before closing a task, the agent must explicitly confirm which DoD items are satisfied and list any exceptions.


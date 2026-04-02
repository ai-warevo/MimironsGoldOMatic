# Documentation & Onboarding Template

## When to use

Use this to create or improve **developer docs**, onboarding, READMEs, runbooks, or architecture overviews.

## Mandatory quality gate

- Before closing the task, validate against `docs/prompts/templates/definition_of_done.md`.
- In `report.md`, always include a `Potential technical debt` section.

## Prompt (copy/paste)

### Audience

- **Who is this for** (new dev, ops, contributor, player modder):
- **Their goal**:
- **Assumed knowledge**:

### Scope

- **What should be covered**:
- **What should not be covered**:

### Outputs requested

- [ ] README update
- [ ] “Getting started” guide
- [ ] Development workflow (build/test/lint)
- [ ] Project structure overview
- [ ] Common tasks cookbook
- [ ] Troubleshooting section
- [ ] Runbook / operational guide
- [ ] Glossary

### Required details (fill in)

- **Repo entrypoint** (solution/workspace, main app):
- **Build commands**:
- **Test commands**:
- **Lint/format commands**:
- **Config files** (.env, appsettings, etc.):
- **Local dev dependencies** (DB, redis, etc.):

### Style constraints

- **Tone**: concise, actionable
- **Format**: headings + copy/paste commands
- **Include**: concrete examples
- **Avoid**: vague statements without steps

### Definition of Done

- [ ] Docs are accurate against the current repo state
- [ ] All commands were verified (or explicitly labeled “example”)
- [ ] Links work and paths match repo structure
- [ ] `docs/prompts/templates/definition_of_done.md` checked
- [ ] `report.md` includes `Potential technical debt`

## Filled example (mini)

### Audience

- **Who is this for**: New contributor.
- **Their goal**: Run the game + tests locally.
- **Assumed knowledge**: Basic .NET + CLI.


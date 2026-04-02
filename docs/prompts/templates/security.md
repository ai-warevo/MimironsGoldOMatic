# Security Review / Hardening Template

## When to use

Use this for security-sensitive changes, threat modeling, or hardening existing flows.

## Mandatory quality gate

- Before closing the task, validate against `docs/prompts/templates/definition_of_done.md`.
- In `report.md`, always include a `Potential technical debt` section.

## Prompt (copy/paste)

### Context

- **System/component**:
- **Data sensitivity** (PII, credentials, tokens, payment):
- **Trust boundaries**:

### Threat model

- **Assets to protect**:
- **Likely attackers**:
- **Attack vectors**:
- **Potential impact**:

### Security controls

- **Authentication**:
- **Authorization**:
- **Input validation / sanitization**:
- **Secrets handling**:
- **Logging/auditability**:
- **Rate limiting / abuse prevention**:

### Validation plan

- **Security tests** (unit/integration/fuzz/static checks):
- **Manual abuse cases to run**:
- **Known residual risks**:

### Definition of Done

- [ ] High/critical issues addressed
- [ ] No sensitive data leakage in logs/errors
- [ ] Access controls validated
- [ ] Residual risks documented with owner
- [ ] `docs/prompts/templates/definition_of_done.md` checked
- [ ] `report.md` includes `Potential technical debt`


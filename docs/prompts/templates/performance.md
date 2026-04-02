# Performance Optimization Template

## When to use

Use this when a system is correct but too slow, too memory-heavy, or too expensive at expected load.

## Mandatory quality gate

- Before closing the task, validate against `docs/prompts/templates/definition_of_done.md`.
- In `report.md`, always include a `Potential technical debt` section.

## Prompt (copy/paste)

### Performance problem

- **Symptom** (latency, throughput, memory, CPU, cost):
- **SLO/SLA target**:
- **Current baseline**:
- **Workload profile** (peak, steady, burst):

### Measurement first

- **Instrumentation available**:
- **Benchmark/profiling plan**:
- **Success metrics**:

### Candidate optimizations

- **Option A**:
- **Option B**:
- **Option C**:

### Risks and trade-offs

- **Correctness risk**:
- **Complexity/maintainability cost**:
- **Infra/cost implications**:

### Rollout plan

- **Guardrails** (feature flag, canary, threshold alerts):
- **Rollback trigger**:

### Definition of Done

- [ ] Benchmark/profiler evidence attached
- [ ] Target metric met or gap explained
- [ ] No correctness regressions in tests
- [ ] Monitoring updated for the optimized path
- [ ] `docs/prompts/templates/definition_of_done.md` checked
- [ ] `report.md` includes `Potential technical debt`


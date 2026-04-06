Modified files:
- `docs/prod/TASK-03-secrets-config-hardening.md`
- `docs/prod/TASK-06-production-dress-rehearsal.md`
- `docs/prod/PROD_TASKS_INDEX.md`
- `docs/prompts/history/2026-04-06/28-production-task-deduplication/prompt.md`
- `docs/prompts/history/2026-04-06/28-production-task-deduplication/plan.md`
- `docs/prompts/history/2026-04-06/28-production-task-deduplication/checks.md`
- `docs/prompts/history/2026-04-06/28-production-task-deduplication/report.md`

Result:
- Removed practical overlap between production tasks by adding explicit scope boundaries.
- Clarified that extension secret rotation is owned by Task 02, while Task 03 handles broader secrets/config hardening.
- Clarified that Task 05 owns CI workflow hardening and Task 06 consumes those outputs for production-like runtime/operator rehearsal.
- Added index notes so execution handoffs are explicit for future runs.

Verification:
- Lint diagnostics on edited documentation paths reported no issues.

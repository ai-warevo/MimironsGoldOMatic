<!-- Updated: 2026-04-06 -->

# Production Task Prompts (Cursor)

One file = one task = one prompt. Run in this order:

1. `docs/prod/TASK-01-twitch-jwt-issuer-hardening.md`
2. `docs/prod/TASK-02-extension-secret-rotation-runbooks.md`
3. `docs/prod/TASK-03-secrets-config-hardening.md`
4. `docs/prod/TASK-04-security-review-abuse-logging.md`
5. `docs/prod/TASK-05-ci-workflow-hardening.md`
6. `docs/prod/TASK-06-production-dress-rehearsal.md`
7. `docs/prod/TASK-07-production-release-package.md`
8. `docs/prod/TASK-08-production-go-no-go-decision.md`

Roadmap production coverage map:

1. JWT hardening (issuer validation, secret rotation runbooks) ->
   `TASK-01`, `TASK-02`
2. Secrets/config hardening across environments ->
   `TASK-03`
3. Security review (abuse cases, logging hygiene) ->
   `TASK-04`
4. CI hardening for .NET/frontend/E2E/release/monitoring workflows ->
   `TASK-05`

Execution note:
- After each task, update `docs/prod/PROD_GO_NO_GO.md` evidence checklist with links/results.
- Scope note: `TASK-02` owns Twitch Extension secret rotation procedures; `TASK-03` covers broader secrets/config hardening outside that runbook.
- Scope note: `TASK-05` hardens CI mechanics; `TASK-06` validates production-like runtime/operator behavior using those CI outputs.

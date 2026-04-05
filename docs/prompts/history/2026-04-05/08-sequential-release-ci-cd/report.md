# Report

## Modified / added files

- `.github/workflows/e2e-test.yml` — comments, NuGet cache, scoped build.
- `.github/workflows/release.yml` — new release pipeline.
- `src/MimironsGoldOMatic.Backend/Dockerfile` — Backend Linux image (context `src/`).
- `docs/e2e/E2E_AUTOMATION_PLAN.md` — CI/CD Pipeline Architecture section + Tier A timing/build notes.
- `docs/e2e/E2E_AUTOMATION_TASKS.md` — PR checklist + V3–V7 release validation tasks.

## Verification

- Workflows were **not** executed in CI (per request). Local `dotnet build` for scoped projects recommended before merge.

## Technical debt / follow-ups

- Default release version `v0.0.<run_number>` may not match product marketing semver; team may adopt tag-only releases or a `VERSION` file.
- WoW addon ZIP currently packs `.toc`/`.lua` only; add locale folders when they exist in tree.
- Optional: `.dockerignore` under `src/` to shrink Docker build context.

## E2E logic preservation

- **Unchanged:** `on.pull_request.branches: [main]`, `ubuntu-latest`, Postgres **16** service definition, `DOTNET_VERSION`, `E2E_EVENTSUB_SECRET`, Backend + mock env blocks, `dotnet run` invocations, Python `send_e2e_eventsub.py` invocation, JWT + `curl` + Python assert for `GET /api/pool/me`, failure log step.

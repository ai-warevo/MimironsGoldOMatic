<!-- Updated: 2026-04-06 (Project structure alignment + Tier B finalization) -->

# Team message — Tier B E2E complete (template)

**Subject:** Mimiron’s Gold-o-Matic — Tier B E2E (CI) complete + docs refresh

---

Hi team,

**Tier B** of our GitHub Actions E2E pipeline is **done and green**: the PR workflow **[E2E Tier A+B (mocks)](https://github.com/ai-warevo/MimironsGoldOMatic/actions/workflows/e2e-test.yml)** runs **Backend + PostgreSQL + MockEventSubWebhook + MockExtensionJwt + MockHelixApi + SyntheticDesktop**, exercises the **enrollment → harness → payout `Sent` → Helix capture → pool removal** path, and matches the plan in **`docs/e2e/E2E_AUTOMATION_PLAN.md`**.

**Useful links**

- **Workflow runs:** [e2e-test.yml runs](https://github.com/ai-warevo/MimironsGoldOMatic/actions/workflows/e2e-test.yml) — open the latest **success** to see job **`e2e-tier-a-b`** (expand steps for logs).
- **Formal write-up:** **`docs/e2e/E2E_AUTOMATION_PLAN.md`** — sections **Tier B Final Validation & Success Report** and **Pipeline optimization (E2E workflow)**.
- **Structure map:** **`docs/reference/PROJECT_STRUCTURE.md`** — updated tree and **old path → new path** table for `src/Tests/`, `src/Mocks/`, etc.

**What we achieved**

- **Tier A** (synthetic EventSub + JWT pool check) and **Tier B** (Development E2E harness, synthetic Desktop HTTP sequence, MockHelix **`/last-request`**) run in **one** Linux job with fixed localhost ports — same contracts as production APIs, without real WoW in default CI.
- **Docs** now point at the current **`src/`** layout; component READMEs (**Backend / Desktop / WoW addon**) reference E2E and Tier B env vars where relevant.

**What’s next (short)**

- **CI:** NuGet + pip caching, **always()** log artifacts, and concurrency tuning are documented in the E2E plan and implemented in **`.github/workflows/e2e-test.yml`** — reduces flake diagnosis time and duplicate PR churn.
- **Tier C:** requirements draft — **`docs/e2e/TIER_C_REQUIREMENTS.md`** (real Desktop/WoW/self-hosted runner, staging Twitch). We should align on **priority** (Windows runner vs staging API vs addon tests) and **cost** (minutes, secrets).

**Invitation**

Reply in thread or in the next stand-up with **Tier C** priorities (e.g. self-hosted Windows E2E vs real Helix staging vs deeper addon tests). If you touch **`src/Tests/`** or **`src/Mocks/`**, use **`docs/reference/PROJECT_STRUCTURE.md`** as the path reference.

Thanks,

[Your name]

---

*This file is a reusable template; copy into email or chat as needed.*

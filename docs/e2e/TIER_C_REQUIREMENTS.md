<!-- Updated: 2026-04-06 (Tier B closure + Tier C kick-off) -->

# Tier C — requirements (draft)

**Parent plan:** [`E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md) — section **Tier C: Future Scope & Requirements**. **Tier B** (CI mocks, **Backend ↔ MockHelixApi ↔ SyntheticDesktop**) is **implementation-complete**; Tier C extends toward **real clients** and optional **staging Twitch**. **Tasks:** [`TIER_C_IMPLEMENTATION_TASKS.md`](TIER_C_IMPLEMENTATION_TASKS.md). **Handover (Tier B):** [`TIER_B_HANDOVER.md`](TIER_B_HANDOVER.md).

---

## 1. Goals (high level)

| Theme | Description |
|--------|-------------|
| **Real Desktop + WoW** | Automated or semi-automated pipeline on **self-hosted Windows** runners (or manual nightly): real **`WoW.exe`**, real addon, Desktop **WinAPI** / log tail → EBS. |
| **Stronger fidelity** | Compare **SyntheticDesktop** timings and payloads against **MimironsGoldOMatic.Desktop** behavior; reduce drift in headers, order, and error handling. |
| **Broader mocks** | Optional **IRC/EventSub live** staging, **real Helix** with broadcaster secrets in a gated environment (not default PR CI). |
| **Observability** | Structured logs/metrics from E2E harnesses for flake diagnosis (align with [`E2E_AUTOMATION_PLAN.md`](E2E_AUTOMATION_PLAN.md) troubleshooting). |

---

## 2. Features / components to add

- **Self-hosted E2E job** (optional workflow or `workflow_dispatch`): Windows runner prerequisites (WoW 3.3.5a path, log path, API keys).
- **Desktop harness parity tests**: shared test library for HTTP sequence vs **SyntheticDesktop** (`src/Mocks/SyntheticDesktop/`).
- **Addon contract tests**: expand `src/Tests/MimironsGoldOMatic.WoWAddon.Tests/` for **`[MGM_*]`** tag formats (see [`docs/components/wow-addon/ReadME.md`](../components/wow-addon/ReadME.md)).
- **Staging Twitch**: secrets for real **Helix** and **EventSub** (org **Environments**), documented in [`docs/setup/SETUP-for-developer.md`](../setup/SETUP-for-developer.md).

---

## 3. Integration points (current structure)

| Area | Path / doc |
|------|------------|
| EBS APIs | `src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Api/` — Desktop routes, Helix, harness `POST /api/e2e/prepare-pending-payout` (Development only). |
| Synthetic mock | `src/Mocks/SyntheticDesktop/` |
| Helix mock | `src/Mocks/MockHelixApi/` |
| Real Desktop | `src/MimironsGoldOMatic.Desktop/` — [`docs/components/desktop/ReadME.md`](../components/desktop/ReadME.md) |
| Addon | `src/MimironsGoldOMatic.WoWAddon/` — [`docs/components/wow-addon/ReadME.md`](../components/wow-addon/ReadME.md) |
| CI | `.github/workflows/e2e-test.yml`, `unit-integration-tests.yml` |

---

## 4. Success criteria (initial)

1. **Documented** Windows/self-hosted runbook with pass/fail criteria for at least one **SC-001** segment involving real log tags.
2. **No regression** to **Tier A / Tier B** default PR workflows (Linux-hosted mocks).
3. **Secrets**: no broadcaster tokens in logs; use GitHub **Environments** for optional staging jobs.

---

## 5. Task breakdown (high level)

| Phase | Tasks |
|--------|--------|
| **C0** | Prioritize: full Windows E2E vs staging Twitch vs addon-only tests; cost/concurrency decision. |
| **C1** | Spec runner image(s), WoW install constraints, artifact retention for log uploads. |
| **C2** | Implement smallest vertical slice (e.g. log tag detection integration with mock Backend). |
| **C3** | Harden flakiness (focus timing, `docs/overview/SPEC.md` windows). |

---

## 6. Dependencies on external systems

- **Twitch**: Helix, EventSub, Extension JWT (test channel, dev rig) — see [`docs/overview/SPEC.md`](../overview/SPEC.md).
- **WoW 3.3.5a**: client availability on runner (licensing and ToS are operator responsibility).
- **GitHub**: self-hosted runner registration, **Actions** minutes vs **hosted** cost tradeoffs.

---

## 7. Risks and mitigations (summary)

| Risk | Mitigation |
|------|------------|
| Flaky UI/WinAPI | Keep **SyntheticDesktop** path in CI; Tier C as optional/nightly. |
| Secret exposure | Environments, OIDC, no token echo in artifacts. |
| Runner cost | Path filters, schedule-only Tier C, cache **NuGet**/pip as in [`e2e-test.yml`](../../.github/workflows/e2e-test.yml). |

---

## 8. Detailed feature breakdown

### F1 — Real WoW + Desktop (self-hosted or manual)

| Field | Specification |
|--------|----------------|
| **Goal** | Execute **SC-001** segments with **`WoW.exe` 3.3.5a**, production addon, **`WoWChatLog.txt`** tags, and **`MimironsGoldOMatic.Desktop`** WinAPI / HTTP bridge to EBS. |
| **Trigger** | Nightly **`workflow_dispatch`**, label-gated PR, or **manual** runbook only — **not** default Linux PR CI. |
| **Integration points** | Desktop log tail → `confirm-acceptance` / `verify-candidate` per [`SPEC.md`](../overview/SPEC.md); same EBS routes as Tier B. |
| **Artifacts** | Redacted **`WoWChatLog.txt`**, Desktop trace, EBS correlation logs. |
| **Dependencies** | Licensed client; operator-controlled machine; Windows runner registration. |

### F2 — Staging Twitch (Helix + optional EventSub)

| Field | Specification |
|--------|----------------|
| **Goal** | Validate real **Helix** `POST /helix/chat/messages` and optional **EventSub** delivery using **test channel / dev rig** credentials. |
| **Trigger** | **GitHub Environments** with required reviewers; **`workflow_dispatch`**; never auto-run on untrusted PRs. |
| **Integration points** | [`HelixChatService`](../../src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Services/HelixChatService.cs), [`TwitchEventSubController`](../../src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Api/Controllers/TwitchEventSubController.cs), Extension JWT contract. |
| **Dependencies** | Broadcaster OAuth refresh strategy; Twitch app Client-Id; compliance with Twitch developer policies. |

### F3 — Parity tests (SyntheticDesktop vs Desktop)

| Field | Specification |
|--------|----------------|
| **Goal** | Detect drift in **URL paths**, **headers** (`X-MGM-ApiKey`), **JSON bodies**, **ordering**, and **error handling** between [`SyntheticDesktop`](../../src/Mocks/SyntheticDesktop/) and WPF client. |
| **Implementation options** | Shared **.NET** client library; golden-file HTTP recordings; scripted diff of **`GET /last-run`** vs Desktop telemetry. |
| **Dependencies** | C2-01 / C2-02 owners ([`TIER_C_IMPLEMENTATION_TASKS.md`](TIER_C_IMPLEMENTATION_TASKS.md)). |

### F4 — Addon contract expansion

| Field | Specification |
|--------|----------------|
| **Goal** | Automated validation of **`[MGM_WHO]`**, **`[MGM_ACCEPT:UUID]`**, **`[MGM_CONFIRM:UUID]`** formats and mail-send gating per [`SPEC.md`](../overview/SPEC.md). |
| **Integration points** | [`src/MimironsGoldOMatic.WoWAddon/`](../../src/MimironsGoldOMatic.WoWAddon/), `src/Tests/MimironsGoldOMatic.WoWAddon.Tests/`. |

---

## 9. Technical specifications (cross-cutting)

| Topic | Requirement |
|--------|-------------|
| **Secrets** | Store in **GitHub Environments**; mask in **Actions**; no tokens in **E2E** artifacts; **Production** EBS must never enable **`Mgm:EnableE2eHarness`**. |
| **Observability** | Correlation id (payout id) propagated from orchestrator to Backend logs; Desktop logs tag line numbers for **`[MGM_*]`** matches. |
| **Determinism** | Tier B PR workflow remains **single-job**, fixed ports; Tier C may use dynamic ports **only** if documented in [`TIER_B_HANDOVER.md`](TIER_B_HANDOVER.md) successor. |
| **Backward compatibility** | Changes to **`Desktop` HTTP** contract require **SyntheticDesktop** + **`run_e2e_tier_b.py`** updates in the **same** PR unless Tier B intentionally deprecated (major version bump). |

---

## 10. Integration points (expanded)

| Component | Path | Tier C touch |
|-----------|------|--------------|
| **EBS** | [`src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Api/`](../../src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Api/) | Optional **Helix** / **EventSub** env-specific config; harness remains **Development-only**. |
| **SyntheticDesktop** | [`src/Mocks/SyntheticDesktop/`](../../src/Mocks/SyntheticDesktop/) | Source for parity tests; may reference shared library with Desktop. |
| **Desktop** | [`src/MimironsGoldOMatic.Desktop/`](../../src/MimironsGoldOMatic.Desktop/) | Real WinAPI + log paths; CI may compile-test only. |
| **Addon** | [`src/MimironsGoldOMatic.WoWAddon/`](../../src/MimironsGoldOMatic.WoWAddon/) | Contract tests + optional headless log replay fixtures. |
| **Mocks** | [`src/Mocks/`](../../src/Mocks/) | Unchanged for Tier B; optional **Record/Replay** Helix stub for Tier C offline tests. |
| **Workflows** | [`.github/workflows/`](../../.github/workflows/) | New Tier C YAML must not weaken **Tier A+B** `on:` filters without team sign-off. |

---

## 11. Dependencies (roles and external systems)

| Dependency | Type | Owner (placeholder) |
|------------|------|---------------------|
| **WoW 3.3.5a** install media / ToS | External | Operator |
| **Twitch** dev app + broadcaster tokens | External | Streamer / Backend |
| **GitHub** larger runners or self-hosted Windows | Infra | DevOps |
| **Desktop** WinAPI expertise | Team | Desktop engineer |
| **Lua / addon** expertise | Team | Addon engineer |

---

## 12. Risk assessment matrix

| Risk | Impact | Likelihood | Mitigation strategy |
|------|--------|------------|---------------------|
| **Token leak** in runner artifact | Critical (account compromise) | Low (if Environments used) | Secret scanning; Environment protection rules; no `echo` of tokens; short-lived tokens. |
| **WinAPI flake** (focus, timing) | High (false failures) | Medium | Nightly-only; retries with caps; keep Tier B as PR gate. |
| **Helix rate limits / 5xx** | Medium | Low on test channel | Exponential backoff in **`HelixChatService`**; mock fallback tests. |
| **Runner cost overrun** | Medium | Medium | Scheduled Tier C; path filters; org billing alerts. |
| **SPEC / code drift** on **`[MGM_*]`** | High | Medium | Addon contract tests + SPEC review in Tier C PR template. |
| **Fork PR malicious code** with secrets exfil | Critical | Low | No secrets on fork workflows; `pull_request_target` avoided for Tier C. |

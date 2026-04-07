<!-- Updated: 2026-04-07 (Backend projects under src/MimironsGoldOMatic.Backend/) -->

# Repository structure

Monorepo layout: shared **contracts** (`MimironsGoldOMatic.Shared`) and implementations (**EBS**, **Desktop**, **Twitch Extension**, **WoW addon**) evolve together with **CI mocks** and **test projects** under a single `src/` root.

## Top-level layout

```text
MimironsGoldOMatic/
├── .cursor/
│   ├── commands/
│   └── rules/
│       ├── agent-protocol-compat.mdc
│       └── project-rules.mdc
├── .github/
│   ├── workflows/              # e2e-test, unit-integration-tests, release, …
│   └── scripts/                # E2E Python (send_e2e_eventsub, run_e2e_tier_b, tier_b_verification)
├── docs/
│   ├── overview/
│   ├── e2e/                    # E2E plan, Tier B/C docs, tasks
│   ├── components/             # backend, desktop, twitch-extension, wow-addon, shared
│   ├── setup/
│   ├── reference/
│   ├── ReadME.md
│   └── prompts/                # Cursor templates + task history (not product docs)
├── src/
│   ├── MimironsGoldOMatic.slnx
│   ├── MimironsGoldOMatic.Shared/
│   ├── MimironsGoldOMatic.Backend/   # EBS: Backend.Api, Backend.Domain, Backend.Services, …
│   │   ├── MimironsGoldOMatic.Backend.Api/
│   │   └── …
│   ├── MimironsGoldOMatic.Desktop/
│   ├── MimironsGoldOMatic.TwitchExtension/   (Vite + React; not always in every build scope)
│   ├── MimironsGoldOMatic.WoWAddon/
│   ├── Mocks/                  # MockEventSubWebhook, MockExtensionJwt, MockHelixApi, SyntheticDesktop
│   └── Tests/                  # Backend/Desktop unit+integration, IntegrationTesting, WoWAddon.Tests
├── README.md
├── CONTEXT.md
├── AGENTS.md
└── …
```

## `src/` stack

- **.NET 10** — Shared library, ASP.NET Core **EBS**, WPF **Desktop**, **mocks**, and **test** projects listed in **`MimironsGoldOMatic.slnx`** (except non-MSBuild trees such as the Twitch Extension and WoW addon sources).
- **Vite + React + TypeScript** — Twitch Extension (own **`package.json`**).
- **WoW 3.3.5a** — addon (`.toc` + Lua); Lua tests may live under **`src/Tests/MimironsGoldOMatic.WoWAddon.Tests/`** (see CI).

## Naming (C#)

Every C# **namespace** and **project name** is prefixed with **`MimironsGoldOMatic.`** (see [`.cursor/rules/project-rules.mdc`](../../.cursor/rules/project-rules.mdc)).

---

## Path mapping (legacy → current)

Use this table when updating bookmarks, scripts, or old chat logs. **Do not** rewrite archived files under `docs/prompts/history/`; those keep historical paths for audit.

| Old / informal path | Current path | Migration notes |
|---------------------|--------------|-----------------|
| `MimironsGoldOMatic.WEBAPI.Backend` / legacy monolith `MimironsGoldOMatic.Backend` | `src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Api/` (+ **`Backend.Domain`**, **`Backend.Services`**, …) | ASP.NET host is **`MimironsGoldOMatic.Backend.Api`**; see [`E2E_AUTOMATION_PLAN.md`](../e2e/E2E_AUTOMATION_PLAN.md) code roots. |
| `src/MimironsGoldOMatic.Backend.*` (flat under `src/`, pre–2026-04 layout) | `src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.*` | All **`Backend.*`** MSBuild projects live under **`src/MimironsGoldOMatic.Backend/`**; solution folder **MimironsGoldOMatic.Backend** in **`MimironsGoldOMatic.sln(x)`**. |
| `src/Backend/` (short folder name, pre–2026-04-08) | `src/MimironsGoldOMatic.Backend/` | Folder rename only; project IDs unchanged. |
| `src/tests/...` (lowercase) | `src/Tests/...` | Test tree uses **PascalCase** `Tests` on Windows/Linux CI. |
| `src/tests/MimironsGoldOMatic.Backend.UnitTests` | `src/Tests/MimironsGoldOMatic.Backend.UnitTests` | Same project names; path casing only. |
| `src/tests/MimironsGoldOMatic.Backend.IntegrationTests` | `src/Tests/MimironsGoldOMatic.Backend.IntegrationTests` | Docker-backed integration tests. |
| `src/tests/MimironsGoldOMatic.Desktop.UnitTests` | `src/Tests/MimironsGoldOMatic.Desktop.UnitTests` | WPF-linked unit tests (Windows CI). |
| `src/tests/MimironsGoldOMatic.Desktop.IntegrationTests` | `src/Tests/MimironsGoldOMatic.Desktop.IntegrationTests` | Desktop↔EBS integration. |
| `src/tests/MimironsGoldOMatic.IntegrationTesting` | `src/Tests/MimironsGoldOMatic.IntegrationTesting` | Shared test host/fixtures. |
| “Mocks live only in docs / future” | `src/Mocks/<MockName>/` | Tier A+B mocks are **real projects** in the solution. |
| `.github/workflows/` “placeholder until CI” | `.github/workflows/*.yml` | **E2E**, **unit/integration**, **release** (and any additions) are active. |

**Optional future:** Central package management may add **`packages.lock.json`** at the repo or project level; until then, CI cache keys also hash `src/**/*.csproj` (see [`e2e-test.yml`](../../.github/workflows/e2e-test.yml)).

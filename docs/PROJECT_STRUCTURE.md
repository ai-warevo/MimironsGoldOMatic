<!-- Updated: 2026-04-05 (Deduplication pass) -->

# Repository structure

Monorepo layout: shared **contracts** (`MimironsGoldOMatic.Shared`) and implementations (**EBS**, **Desktop**, **Twitch Extension**, **WoW addon**) evolve together.

## Top-level layout

```text
MimironsGoldOMatic/
├── .cursor/
│   ├── commands/
│   └── rules/
│       ├── agent-protocol-compat.mdc
│       └── project-rules.mdc
├── .github/
│   └── workflows/          (placeholder until CI is added)
├── docs/
│   ├── SPEC.md, ROADMAP.md, UI_SPEC.md (hub), INTERACTION_SCENARIOS.md
│   ├── ARCHITECTURE.md, PROJECT_STRUCTURE.md, WORKFLOWS.md
│   ├── MVP_PRODUCT_SUMMARY.md, GLOSSARY.md, SETUP.md, SETUP-for-developer.md, SETUP-for-streamer.md
│   ├── IMPLEMENTATION_READINESS.md, ReadME.md
│   ├── MimironsGoldOMatic.*/ReadME.md, MimironsGoldOMatic.*/UI_SPEC.md (per component)
│   └── prompts/            (Cursor templates + task history — not part of product docs)
├── src/
│   ├── MimironsGoldOMatic.slnx
│   ├── MimironsGoldOMatic.Shared/
│   ├── MimironsGoldOMatic.Backend/
│   ├── MimironsGoldOMatic.Desktop/
│   ├── MimironsGoldOMatic.TwitchExtension/   (Vite + React; not in .slnx)
│   └── MimironsGoldOMatic.WoWAddon/        (Lua; not in .slnx)
├── README.md
├── CONTEXT.md
├── AGENTS.md
└── …
```

## `src/` stack

- **.NET 10** — Shared library, ASP.NET Core **EBS**, WPF **Desktop** (listed in **`MimironsGoldOMatic.slnx`**).
- **Vite + React + TypeScript** — Twitch Extension (own **`package.json`**).
- **WoW 3.3.5a** — addon (`.toc` + Lua).

## Naming (C#)

Every C# **namespace** and **project name** is prefixed with **`MimironsGoldOMatic.`** (see [`.cursor/rules/project-rules.mdc`](../.cursor/rules/project-rules.mdc)).

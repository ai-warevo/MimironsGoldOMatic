# Report — Documentation deduplication (2026-04-05)

## New files

- `docs/overview/ARCHITECTURE.md` — pipeline, EBS, runtime components, DDD/CQRS/ES, relationships, WoW compatibility.
- `docs/reference/PROJECT_STRUCTURE.md` — monorepo tree, `src/` stack, C# naming.
- `docs/reference/WORKFLOWS.md` — end-to-end MVP steps, dev/agent/setup pointers.
- `docs/overview/MVP_PRODUCT_SUMMARY.md` — consolidated MVP product bullets (non-normative digest).
- `docs/reference/GLOSSARY.md` — term index → SPEC sections / hub docs.
- `docs/SETUP_COMMON.md` — prerequisites, clone, dotnet/npm build, Extension env note.

## Files updated

- `README.md`, `CONTEXT.md`, `AGENTS.md`, `SETUP.md`, `SETUP-for-developer.md`, `SETUP-for-streamer.md`
- `docs/ReadME.md`, `docs/overview/SPEC.md` (intro links only), `docs/reference/UI_SPEC.md`, `docs/overview/ROADMAP.md`, `docs/overview/INTERACTION_SCENARIOS.md`, `docs/reference/IMPLEMENTATION_READINESS.md`
- `docs/components/backend/ReadME.md`, `docs/components/shared/ReadME.md`, `docs/components/twitch-extension/ReadME.md`, `docs/components/desktop/ReadME.md`, `docs/components/wow-addon/ReadME.md`
- `src/MimironsGoldOMatic.TwitchExtension/README.md` (replaced stale Vite template duplicate with pointers)

## Files unchanged (no duplicate block removed)

- `docs/overview/SPEC.md` body (§1+) — remains **normative**; only header gained digest links.
- `docs/components/backend/ReadME.md` **Key Functions** — kept EBS-specific detail; cross-refs added to reduce re-reading product prose elsewhere.
- `.cursor/**/*.md`, other `src/**/*.md` without substantive doc overlap in scope.
- All of `docs/prompts/**` (ignored per task).

## Major duplicates consolidated

| Topic | Was repeated in | Now |
|--------|------------------|-----|
| Architecture / pipeline / patterns | README, CONTEXT, docs/ReadME, Shared ReadME | `docs/overview/ARCHITECTURE.md` |
| MVP product bullets | README, CONTEXT, docs/ReadME, UI_SPEC global rules, ROADMAP payout note | `docs/overview/MVP_PRODUCT_SUMMARY.md` |
| End-to-end flow | CONTEXT, docs/ReadME, INTERACTION_SCENARIOS intro | `docs/reference/WORKFLOWS.md` |
| Repo tree + naming | README, docs/ReadME | `docs/reference/PROJECT_STRUCTURE.md` |
| Prerequisites + first build | SETUP-for-developer §1–2 | `docs/SETUP_COMMON.md` |
| Streamer “what you need” table | SETUP-for-streamer §1 | Points to `docs/overview/ARCHITECTURE.md` |
| Vite/ESLint boilerplate + wrong “template only” | `src/.../TwitchExtension/README.md` | Short README + links (info preserved in hub + component ReadME) |

## Manual review

- **`docs/components/backend/ReadME.md`** still has long **Key Functions** text that overlaps **`MVP_PRODUCT_SUMMARY.md`** at a high level; trimming further would trade duplication for weaker EBS onboarding — left as-is with cross-links.
- **IMPLEMENTATION_READINESS** matrix “Fixed in docs” column still lists legacy paths (`README.md`, etc.); rows remain accurate; optional cleanup to prefer hub filenames only.

## Potential technical debt

- Keep **`MVP_PRODUCT_SUMMARY.md`** in sync when **`SPEC.md`** product rules change (digest drift risk).
- Consider one CI or doc lint check for broken relative links to new hub files.

# Report — Documentation deduplication (2026-04-05)

## New files

- `docs/ARCHITECTURE.md` — pipeline, EBS, runtime components, DDD/CQRS/ES, relationships, WoW compatibility.
- `docs/PROJECT_STRUCTURE.md` — monorepo tree, `src/` stack, C# naming.
- `docs/WORKFLOWS.md` — end-to-end MVP steps, dev/agent/setup pointers.
- `docs/MVP_PRODUCT_SUMMARY.md` — consolidated MVP product bullets (non-normative digest).
- `docs/GLOSSARY.md` — term index → SPEC sections / hub docs.
- `docs/SETUP_COMMON.md` — prerequisites, clone, dotnet/npm build, Extension env note.

## Files updated

- `README.md`, `CONTEXT.md`, `AGENTS.md`, `SETUP.md`, `SETUP-for-developer.md`, `SETUP-for-streamer.md`
- `docs/ReadME.md`, `docs/SPEC.md` (intro links only), `docs/UI_SPEC.md`, `docs/ROADMAP.md`, `docs/INTERACTION_SCENARIOS.md`, `docs/IMPLEMENTATION_READINESS.md`
- `docs/MimironsGoldOMatic.Backend/ReadME.md`, `docs/MimironsGoldOMatic.Shared/ReadME.md`, `docs/MimironsGoldOMatic.TwitchExtension/ReadME.md`, `docs/MimironsGoldOMatic.Desktop/ReadME.md`, `docs/MimironsGoldOMatic.WoWAddon/ReadME.md`
- `src/MimironsGoldOMatic.TwitchExtension/README.md` (replaced stale Vite template duplicate with pointers)

## Files unchanged (no duplicate block removed)

- `docs/SPEC.md` body (§1+) — remains **normative**; only header gained digest links.
- `docs/MimironsGoldOMatic.Backend/ReadME.md` **Key Functions** — kept EBS-specific detail; cross-refs added to reduce re-reading product prose elsewhere.
- `.cursor/**/*.md`, other `src/**/*.md` without substantive doc overlap in scope.
- All of `docs/prompts/**` (ignored per task).

## Major duplicates consolidated

| Topic | Was repeated in | Now |
|--------|------------------|-----|
| Architecture / pipeline / patterns | README, CONTEXT, docs/ReadME, Shared ReadME | `docs/ARCHITECTURE.md` |
| MVP product bullets | README, CONTEXT, docs/ReadME, UI_SPEC global rules, ROADMAP payout note | `docs/MVP_PRODUCT_SUMMARY.md` |
| End-to-end flow | CONTEXT, docs/ReadME, INTERACTION_SCENARIOS intro | `docs/WORKFLOWS.md` |
| Repo tree + naming | README, docs/ReadME | `docs/PROJECT_STRUCTURE.md` |
| Prerequisites + first build | SETUP-for-developer §1–2 | `docs/SETUP_COMMON.md` |
| Streamer “what you need” table | SETUP-for-streamer §1 | Points to `docs/ARCHITECTURE.md` |
| Vite/ESLint boilerplate + wrong “template only” | `src/.../TwitchExtension/README.md` | Short README + links (info preserved in hub + component ReadME) |

## Manual review

- **`docs/MimironsGoldOMatic.Backend/ReadME.md`** still has long **Key Functions** text that overlaps **`MVP_PRODUCT_SUMMARY.md`** at a high level; trimming further would trade duplication for weaker EBS onboarding — left as-is with cross-links.
- **IMPLEMENTATION_READINESS** matrix “Fixed in docs” column still lists legacy paths (`README.md`, etc.); rows remain accurate; optional cleanup to prefer hub filenames only.

## Potential technical debt

- Keep **`MVP_PRODUCT_SUMMARY.md`** in sync when **`SPEC.md`** product rules change (digest drift risk).
- Consider one CI or doc lint check for broken relative links to new hub files.

<!-- Updated: 2026-04-05 (Deduplication pass) -->

# Context

Short entry point for **architecture** and **workflow** pointers. **Canonical contracts:** [`docs/SPEC.md`](docs/SPEC.md).

## High-level purpose

Mimiron's Gold-o-Matic is an end-to-end system for distributing gold in WoW 3.3.5a.

## Implementation status

**MVP-1 … MVP-5** are implemented in **`src/`**. **MVP-6** (tests / E2E automation) and production packaging are open. Matrix: [`docs/IMPLEMENTATION_READINESS.md`](docs/IMPLEMENTATION_READINESS.md).

<!-- Content moved to ARCHITECTURE.md. See: docs/ARCHITECTURE.md -->

<!-- Content moved to MVP_PRODUCT_SUMMARY.md. See: docs/MVP_PRODUCT_SUMMARY.md -->

## Primary data flow and relationships

<!-- Content moved to WORKFLOWS.md. See: docs/WORKFLOWS.md -->

## Data and artifacts

- Shared contracts: **`MimironsGoldOMatic.Shared`** (DTOs / enums / validation).
- WoW payload / chunking: [`docs/SPEC.md`](docs/SPEC.md) §8–9.
- **UI/UX:** [`docs/UI_SPEC.md`](docs/UI_SPEC.md) (hub) · per-surface [`docs/MimironsGoldOMatic.TwitchExtension/UI_SPEC.md`](docs/MimironsGoldOMatic.TwitchExtension/UI_SPEC.md), [`docs/MimironsGoldOMatic.Desktop/UI_SPEC.md`](docs/MimironsGoldOMatic.Desktop/UI_SPEC.md), [`docs/MimironsGoldOMatic.WoWAddon/UI_SPEC.md`](docs/MimironsGoldOMatic.WoWAddon/UI_SPEC.md) (**UI-1xx–4xx**).
- Engineering workflow artifacts: **`docs/prompts/`** (templates + history).

## Test topology

- **.NET:** `dotnet test src/MimironsGoldOMatic.slnx` when test projects exist ([`docs/ROADMAP.md`](docs/ROADMAP.md) MVP-6); until then **`dotnet build`** on the solution.
- Integration focus: API ↔ Shared DTOs; injection chunking vs WoW limits.

## Compatibility

<!-- Content moved to ARCHITECTURE.md (Compatibility). See: docs/ARCHITECTURE.md -->

## See also

- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) · [`docs/PROJECT_STRUCTURE.md`](docs/PROJECT_STRUCTURE.md) · [`docs/GLOSSARY.md`](docs/GLOSSARY.md)

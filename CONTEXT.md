<!-- Updated: 2026-04-05 (Deduplication pass) -->

# Context

Short entry point for **architecture** and **workflow** pointers. **Canonical contracts:** [`docs/overview/SPEC.md`](docs/overview/SPEC.md).

## High-level purpose

Mimiron's Gold-o-Matic is an end-to-end system for distributing gold in WoW 3.3.5a.

## Implementation status

**MVP-1 … MVP-5** are implemented in **`src/`**. **MVP-6** (tests / E2E automation) and production packaging are open. Matrix: [`docs/reference/IMPLEMENTATION_READINESS.md`](docs/reference/IMPLEMENTATION_READINESS.md).

<!-- Content moved to ARCHITECTURE.md. See: docs/overview/ARCHITECTURE.md -->

<!-- Content moved to MVP_PRODUCT_SUMMARY.md. See: docs/overview/MVP_PRODUCT_SUMMARY.md -->

## Primary data flow and relationships

<!-- Content moved to WORKFLOWS.md. See: docs/reference/WORKFLOWS.md -->

## Data and artifacts

- Shared contracts: **`MimironsGoldOMatic.Shared`** (DTOs / enums / validation).
- WoW payload / chunking: [`docs/overview/SPEC.md`](docs/overview/SPEC.md) §8–9.
- **UI/UX:** [`docs/reference/UI_SPEC.md`](docs/reference/UI_SPEC.md) (hub) · per-surface [`docs/components/twitch-extension/UI_SPEC.md`](docs/components/twitch-extension/UI_SPEC.md), [`docs/components/desktop/UI_SPEC.md`](docs/components/desktop/UI_SPEC.md), [`docs/components/wow-addon/UI_SPEC.md`](docs/components/wow-addon/UI_SPEC.md) (**UI-1xx–4xx**).
- Engineering workflow artifacts: **`docs/prompts/`** (templates + history).

## Test topology

- **.NET:** `dotnet test src/MimironsGoldOMatic.slnx` when test projects exist ([`docs/overview/ROADMAP.md`](docs/overview/ROADMAP.md) MVP-6); until then **`dotnet build`** on the solution.
- Integration focus: API ↔ Shared DTOs; injection chunking vs WoW limits.

## Compatibility

<!-- Content moved to ARCHITECTURE.md (Compatibility). See: docs/overview/ARCHITECTURE.md -->

## See also

- [`docs/overview/ARCHITECTURE.md`](docs/overview/ARCHITECTURE.md) · [`docs/reference/PROJECT_STRUCTURE.md`](docs/reference/PROJECT_STRUCTURE.md) · [`docs/reference/GLOSSARY.md`](docs/reference/GLOSSARY.md)

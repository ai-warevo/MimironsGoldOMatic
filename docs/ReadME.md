<!-- Updated: 2026-04-05 (Deduplication pass) -->

# General Architectural Requirements (All Components) & Repo Layout

## Documentation vs code

Normative architecture and API behavior live in **`docs/SPEC.md`** and **`docs/ROADMAP.md`**. **`docs/IMPLEMENTATION_READINESS.md`** tracks doc/spec consistency and **`src/`** parity (**MVP-1…5** in tree; **MVP-6** pending).

**Deduplicated overviews:** [`ARCHITECTURE.md`](ARCHITECTURE.md) · [`PROJECT_STRUCTURE.md`](PROJECT_STRUCTURE.md) · [`WORKFLOWS.md`](WORKFLOWS.md) · [`MVP_PRODUCT_SUMMARY.md`](MVP_PRODUCT_SUMMARY.md) · [`GLOSSARY.md`](GLOSSARY.md) · [`SETUP.md`](SETUP.md) (see also [`SETUP-for-developer.md`](SETUP-for-developer.md), [`SETUP-for-streamer.md`](SETUP-for-streamer.md))

<!-- Content moved to ARCHITECTURE.md (Architectural patterns + persistence). See: docs/ARCHITECTURE.md -->

## MimironsGoldOMatic.Shared (.NET 10)

- **Role:** DTOs, enums, and validation consumed by **EBS** and **Desktop** (not MediatR handlers).
- **FluentValidation:** `PayoutDto` / `CreatePayoutRequest` validators; **`CharacterNameRules`** aligned with [`SPEC.md`](SPEC.md) §4.
- **Records / primary constructors:** DTOs such as **`PayoutDto`**, **`CreatePayoutRequest`** live in this assembly.
- **EBS application layer:** **`HandlerResult<T>`** + **`ApiErrorDto`** in **`MimironsGoldOMatic.Backend`** only.

<!-- Content moved to WORKFLOWS.md. See: docs/WORKFLOWS.md -->

<!-- Content moved to MVP_PRODUCT_SUMMARY.md. See: docs/MVP_PRODUCT_SUMMARY.md -->

## Technical specification (canonical)

- [`SPEC.md`](SPEC.md) — APIs, DTOs, transitions, persistence, payloads, log parsing
- [`UI_SPEC.md`](UI_SPEC.md) — screens, states, ASCII layouts, design tokens

## Core components (pointers)

<!-- Content moved to ARCHITECTURE.md (Runtime components + relationships). See: docs/ARCHITECTURE.md -->

- **[Twitch Extension](MimironsGoldOMatic.TwitchExtension/ReadME.md)** · **[EBS / Backend](MimironsGoldOMatic.Backend/ReadME.md)** · **[Shared](MimironsGoldOMatic.Shared/ReadME.md)** · **[Desktop](MimironsGoldOMatic.Desktop/ReadME.md)** · **[WoW addon](MimironsGoldOMatic.WoWAddon/ReadME.md)**

<!-- Content moved to PROJECT_STRUCTURE.md (tree + C# naming). See: docs/PROJECT_STRUCTURE.md -->

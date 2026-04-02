# Implementation Prompt: MVP-0 + MVP-1

Use this prompt to start coding from a clean docs-aligned baseline.

---

Acting as **[Backend/API Expert]** with strict adherence to:
- `docs/SPEC.md` (canonical contract)
- `docs/IMPLEMENTATION_READINESS.md` (readiness matrix)
- `AGENTS.md` (workflow and logging protocol)

## Scope

Implement only:
- `MVP-0` (solution/project skeleton)
- `MVP-1` (`MimironsGoldOMatic.Shared` contracts)

Do **not** implement backend endpoints, desktop injection logic, addon code, or frontend UI in this task.

## Architectural constraints (mandatory)

- Follow **DDD + CQRS + Event Sourcing** project direction.
- MVP write-side source of truth is ES-first (Marten on PostgreSQL), but for this task only Shared contracts are implemented.
- Keep namespaces and project naming consistent with `MimironsGoldOMatic.*`.

## Step A — MVP-0 (Repo/solution skeleton)

Create under `src/`:
- `MimironsGoldOMatic.sln`
- `MimironsGoldOMatic.Shared` (.NET 10 class library)
- `MimironsGoldOMatic.Backend` (.NET 10 web api project; empty scaffold only)
- `MimironsGoldOMatic.Desktop` (.NET 10 WPF project; empty scaffold only)
- `MimironsGoldOMatic.TwitchExtension` (Vite + React + TS scaffold; minimal bootstrapped app)
- `MimironsGoldOMatic.WoWAddon` (folder scaffold with placeholder `.toc` and `.lua`)

Add all .NET projects to `MimironsGoldOMatic.sln`.

## Step B — MVP-1 (Shared contracts only)

In `MimironsGoldOMatic.Shared` implement:

1. `PayoutStatus` enum with exact values:
- `Pending`
- `InProgress`
- `Sent`
- `Failed`
- `Cancelled`
- `Expired`

2. `CreatePayoutRequest` record with:
- `string CharacterName`
- `string TwitchTransactionId`

3. `PayoutDto` record with:
- `Guid Id`
- `string TwitchUserId`
- `string TwitchDisplayName`
- `string CharacterName`
- `long GoldAmount`
- `string TwitchTransactionId`
- `PayoutStatus Status`
- `DateTime CreatedAt`

4. Shared validation for character name (FluentValidation):
- non-empty
- max length (reasonable WoW-safe bound)
- reject `:` and `;` (payload delimiter safety)

## Acceptance criteria (must pass)

- `dotnet --version` indicates .NET 10 SDK available or clear actionable failure report is provided.
- `dotnet sln src/MimironsGoldOMatic.sln list` includes all .NET projects.
- `dotnet build src/MimironsGoldOMatic.sln` succeeds.
- Shared contracts compile without warnings promoted to errors.
- Namespaces start with `MimironsGoldOMatic`.
- No conflicts with `docs/SPEC.md` DTO/status naming.

## Output requirements

When done, provide:
1. Modified/created file list.
2. Build output summary.
3. Any blocked items (if tooling/environment issue).
4. Brief note on readiness to start MVP-2 backend implementation.

## Safety constraints

- Do not rewrite documentation contracts during this task.
- Do not introduce unrelated dependencies.
- Do not skip tests/build checks silently.

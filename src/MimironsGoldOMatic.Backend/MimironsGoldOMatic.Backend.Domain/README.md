## MimironsGoldOMatic.Backend.Domain

Bounded-context layout for MediatR contracts and domain-facing abstractions. Each context folder contains the standard slices (`Abstract`, `Commands`, `Dtos`, `Entities`, `Events`, `Exceptions`, `Handlers`, `Queries`, `Repositories`, `Services`). Empty slices are reserved with `.gitkeep` until types land.

**Contexts (current):**

- **`System`** — shared API result shapes (`ApiErrorDto`, `HandlerResult<T>`), E2E harness DTOs, and **`IUnitOfWork` / `IUnitOfWorkFactory`** (`System/Abstract`).
- **`Roulette`** — spin / pool / payout / verify-candidate commands and queries.
- **`Gifts`** — gift queue commands and queries.

Handler implementations remain in **`MimironsGoldOMatic.Backend.Services`** (this project holds contracts only).

### Dependency direction (strict)

- **Allowed:** `MimironsGoldOMatic.Backend.Domain` → `MimironsGoldOMatic.Shared.*` and `MediatR` only (per this project’s `.csproj` references).
- **Not referenced:** `Backend.Services`, `Backend.DataAccess`, `Backend.Api`, `Backend.Infrastructure`, `Backend.Cli`, integration tests.

### Verification

- **Gate (this layer):** `dotnet build src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Domain/MimironsGoldOMatic.Backend.Domain.csproj`

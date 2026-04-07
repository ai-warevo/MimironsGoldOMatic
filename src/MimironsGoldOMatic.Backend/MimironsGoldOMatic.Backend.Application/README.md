## MimironsGoldOMatic.Backend.Application

**Application layer** for the EBS backend: **CQRS** with **MediatR** (commands, queries, handlers in one assembly), integration-facing services (Helix, chat enrollment, gift queue), and hosted background work (roulette sync, payout expiry, gift timeouts).

Bounded contexts stay visible in folder layout:

- **`System`** — API result shapes (`ApiErrorDto`, `HandlerResult<T>`), E2E harness DTOs, and **`IUnitOfWork` / `IUnitOfWorkFactory`** (`System/Abstract`).
- **`Roulette`** — pool / spin / payout / verify-candidate commands, queries, and DTOs; **handlers** live under **`Mediatr/`** (e.g. `EbsMediatorHandlers.cs`).
- **`Gifts`** — gift queue commands and queries; **handlers** in **`Mediatr/GiftMediatorHandlers.cs`**.

Root-level types (e.g. `ChatEnrollmentService`, `HelixChatService`, `*HostedService`) are application services used by **Infrastructure** DI and API controllers.

### Dependency direction

- **References:** `MimironsGoldOMatic.Backend.Configuration`, `MimironsGoldOMatic.Backend.Common`, `MimironsGoldOMatic.Backend.Infrastructure.Persistence` (Marten documents and IDs), `MimironsGoldOMatic.Shared`, **MediatR**, **Marten**, **FluentValidation** (validators primarily in **Common**).
- **Does not reference:** `Backend.Api` (host), `Backend.Infrastructure` (composition/auth), test projects.

### Verification

- `dotnet build src/MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Application/MimironsGoldOMatic.Backend.Application.csproj`

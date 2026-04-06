## MimironsGoldOMatic.Backend.Domain

MediatR request/query/command contracts for the backend write side (`EbsMediator.Contracts.cs` and related types).

### Dependency direction (strict)

- **Allowed:** `MimironsGoldOMatic.Backend.Domain` → `MimironsGoldOMatic.Backend.Abstract`, `MimironsGoldOMatic.Backend.Shared`, and `MediatR` only.
- **Not referenced:** `Backend.Services`, `Backend.DataAccess`, `Backend.Api`, `Backend.Infrastructure`, `Backend.Cli`, `Backend.IntegrationTests`.

### Temporary compatibility

No temporary compatibility reference is currently required in this project. Mediator contracts use `MimironsGoldOMatic.Backend.Abstract` DTOs.

### Deferred cleanup (out of scope for compile migration)

- Remove or replace placeholder `Class1.cs` when real domain types or split files are introduced.
- Continue consolidating duplicate contract definitions across old/new backend layers as migration proceeds.

### Verification handoff

- **Gate (this layer):** `dotnet build src/MimironsGoldOMatic.Backend.Domain/MimironsGoldOMatic.Backend.Domain.csproj`
- **Next (orchestrator):** `tmp/prompts/backend-layer-compiles/micro/41-verify-layer-gates.md` — re-run Services, DataAccess, and Domain builds before full solution verification.

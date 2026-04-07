Plan — Backend architecture migration

Goal

- Split `src/MimironsGoldOMatic.Backend/` monolith into the target project structure while keeping behavior and external API contracts stable.

Current state (summary)

- `MimironsGoldOMatic.Backend` (ASP.NET Core): controllers, auth, configuration, MediatR “application” handlers, Marten persistence, Helix/Twitch integration services, hosted background services.
- `MimironsGoldOMatic.Shared`: backend-facing DTOs/enums + FluentValidation validators (library reference).

Target state (projects)

- `MimironsGoldOMatic.Backend.Api`: ASP.NET Core host + controllers; minimal orchestration.
- `MimironsGoldOMatic.Backend.Abstract`: POCO-only contracts (no package refs, no project refs).
- `MimironsGoldOMatic.Backend.Common`: general helpers + validation helpers (can reference libraries), but should not depend on other backend projects.
- `MimironsGoldOMatic.Backend.Domain`: business logic, commands/queries/handlers/events; depends on `Abstract` and `Shared`.
- `MimironsGoldOMatic.Backend.DataAccess`: Marten (ORM/event store) and persistence implementations; depends on `Domain`.
- `MimironsGoldOMatic.Backend.Services`: third-party integration implementations (Helix/Twitch, HTTP); depends on `Domain` (interfaces) + `Shared`.
- `MimironsGoldOMatic.Backend.Infrastructure`: DI composition roots, auth plumbing, hosted service registrations; depends on `Domain` + `DataAccess` + `Services`.
- `MimironsGoldOMatic.Backend.IntegrationTests`: integration tests; depends on `Backend.Api` (test server) and test utilities.
- `MimironsGoldOMatic.Backend.Cli`: optional consolidation of existing mocks/tools; not required for compile parity.

Execution steps

1. Scaffold new projects under `src/` with correct target frameworks and references.
2. Introduce a “composition” pattern:
   - `Backend.Infrastructure` exposes `IServiceCollection` extension methods used by `Backend.Api`.
3. Migrate code by concern:
   - Contracts: move POCO DTOs/enums to `Backend.Abstract`; move validators to `Backend.Shared`.
   - Persistence: move Marten document config + stream event definitions to `Backend.DataAccess` (or keep event POCOs in `Domain/.../Events` if treated as domain events).
   - Domain: move MediatR messages/handlers to `Backend.Domain` (keep folder layout stable first; bounded-context reshaping can follow).
   - Services: move Helix/Twitch integration classes to `Backend.Services`, behind `Domain` interfaces.
   - Api: move controllers and minimal `Program.cs` host to `Backend.Api`.
4. Update tests to reference `Backend.Api` and any moved shared test utilities.
5. Build and fix compilation issues iteratively until `dotnet build src/MimironsGoldOMatic.slnx` passes.

Risks / mitigations

- Namespace/type moves affecting generated TypeScript client: keep contracts stable and update ApiTsGen input paths.
- Circular dependencies: enforce directionality early (Api → Infrastructure → {Domain,DataAccess,Services}; Domain → {Abstract,Shared}; DataAccess/Services implement Domain interfaces).
- Mixed responsibilities in handlers: defer deep refactors; focus on project boundaries first.

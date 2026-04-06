## Goal

Cut over the ASP.NET Core host to the new `Backend.Api` project by moving controllers into `src/MimironsGoldOMatic.Backend.Api/Controllers/*` and ensuring each endpoint is wired to `Backend.Domain` (MediatR handlers / domain services) rather than legacy in-host logic.

## Non-goals

- Reintroducing rate limiting in `Backend.Infrastructure` (explicitly deferred to `Backend.Api` host wiring).
- Large refactors of domain logic, persistence, or contracts beyond what is needed for compilation + basic runtime wiring.

## Approach

- Inventory current controllers/endpoints in the repo (legacy + new).
- Ensure `Backend.Api` references the correct layers (`Backend.Infrastructure`, `Backend.Domain`, shared DTOs).
- Move/port controllers into `Backend.Api/Controllers`, keeping route shapes stable.
- Update controller implementations to use MediatR requests/handlers (or the domain service abstractions) from `Backend.Domain`.
- Ensure auth attributes/schemes align with the infrastructure auth registration (API key + any existing JWT).
- Build the solution and fix compilation errors by adjusting DI registrations and project references.

## Files expected to change

- `src/MimironsGoldOMatic.Backend.Api/Controllers/*`
- `src/MimironsGoldOMatic.Backend.Api/Program.cs` (host wiring, if needed)
- `src/MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.csproj` (references)
- Potentially `src/MimironsGoldOMatic.Backend.Domain/*` (only if required to expose handlers/contracts used by controllers)

## Risks

- Endpoint routes might diverge from the legacy host if not carefully matched.
- Missing DI registrations may surface only at runtime; mitigate by building and ensuring controller constructors resolve.

## Verification

- `dotnet build src/MimironsGoldOMatic.sln`
- If test projects exist for backend/API, run targeted `dotnet test` for those projects.


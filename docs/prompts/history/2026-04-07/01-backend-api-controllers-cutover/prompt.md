@tmp/prompts 
@tmp/prompts/backend-api-cutover-ready/micro/01-infra-composition-root.md 

Sub-prompt A is now **done**.

- Implemented `AddMgmBackend(...)` in `src/MimironsGoldOMatic.Backend.Infrastructure/DependencyInjection/BackendCompositionExtensions.cs`
- Added API-key auth handler in `src/MimironsGoldOMatic.Backend.Infrastructure/Auth/ApiKeyAuthenticationHandler.cs`
- **Verified:** `dotnet build src/MimironsGoldOMatic.Backend.Infrastructure/MimironsGoldOMatic.Backend.Infrastructure.csproj` ✅ (`0 warnings, 0 errors`)

Note: I temporarily **removed the rate-limiter registration** from `Backend.Infrastructure` because `services.AddRateLimiter(...)` wasn’t available in that project shape in this environment. Rate limiting can be reintroduced in `Backend.Api` host wiring later (where it’s already known to work).

Proceed to **Sub-prompt B** (move controllers into `src/MimironsGoldOMatic.Backend.Api/Controllers/*` and wire them to `Backend.Domain`)?


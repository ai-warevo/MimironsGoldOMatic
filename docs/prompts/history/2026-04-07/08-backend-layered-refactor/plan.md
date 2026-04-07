# Plan

1. Merge **Backend.Domain** + **Backend.Services** into **MimironsGoldOMatic.Backend.Application** (CQRS contracts + handlers + app services in one assembly; namespaces `MimironsGoldOMatic.Backend.Application.*`).
2. Rename **Backend.DataAccess** → **MimironsGoldOMatic.Backend.Infrastructure.Persistence**; namespace `MimironsGoldOMatic.Backend.Infrastructure.Persistence` for Marten documents/configuration.
3. Update **Backend.Infrastructure** composition root: reference Application + Infrastructure.Persistence; remove duplicate DI registrations; fix MediatR comment.
4. Update **Api**, tests, **slnx**, and **docs/reference/PROJECT_STRUCTURE.md** hub lines.
5. Remove stub **Class1.cs** and empty **.gitkeep** placeholders in Application.
6. Verify: `dotnet build` + `dotnet test` on solution.

## Risks

- Large namespace churn in tests and TS API generation inputs — mitigated by solution-wide replace and rebuild.

## Summary

- Moved **Backend** automated tests to **`src/tests/MimironsGoldOMatic.Backend.UnitTests`** (project **MimironsGoldOMatic.Backend.UnitTests**); updated **`MimironsGoldOMatic.slnx`**, **`MimironsGoldOMatic.sln`**, **`.github/workflows/unit-integration-tests.yml`**, and **`docs/MimironsGoldOMatic.Backend/ReadME.md`**.
- Added **Moq**-based unit tests for controllers, **ApiKey** auth, **HelixChatService**, **TwitchEventSubController**; expanded parser/spin/time tests; **FluentValidation** tests for **`CreatePayoutRequestValidator`**.
- Added integration coverage: **MediatR** edge cases, **ChatEnrollmentService**, **RouletteCycleTick**, **PostClaim** subscriber gate, **Patch** transitions, **PayoutExpiration** for **InProgress**.
- Production: **`IChatEnrollmentIngest`** + DI registration for mockable EventSub ingestion.
- Coverage: **`coverlet.runsettings`** excludes **`Program.cs`** and OpenAPI generated file; Backend package **~87%** line coverage with that filter.

## Verification

- `dotnet test src/MimironsGoldOMatic.slnx --configuration Release` — all tests passed (Backend.UnitTests 93 + Desktop 1 after relocate to `src/tests/MimironsGoldOMatic.Backend.UnitTests`).

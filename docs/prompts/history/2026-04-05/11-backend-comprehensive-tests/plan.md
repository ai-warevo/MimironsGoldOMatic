1. Relocate test project to `tests/MimironsGoldOMatic.Backend.Tests`, wire solution + CI.
2. Add Moq unit tests for HTTP-facing types; keep Testcontainers integration for Marten/MediatR.
3. Introduce `IChatEnrollmentIngest` if needed for EventSub controller tests.
4. Add `coverlet.runsettings` to exclude host/bootstrap noise from the 80% metric; document in README.

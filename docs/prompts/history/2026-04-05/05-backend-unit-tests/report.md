# Report — Backend unit tests

## Done

- **`TwGoldChatEnrollmentParser`** (`Services/TwGoldChatEnrollmentParser.cs`): shared regex for `!twgold <token>`; **`ChatEnrollmentService`** delegates to it (behavior unchanged).
- **Unit tests** in **`Backend.Tests/Unit/`** with **`[Trait("Category","Unit")]`**:
  - **`RouletteTimeTests`** — floor idempotency, epoch 5-minute grid, next boundary strictly after input / on boundary.
  - **`SpinPhaseResolverTests`** — idle, collecting, completed (no candidate past window), spinning, verification, completed past deadline.
  - **`TwGoldChatEnrollmentParserTests`** — theory rows for matches and rejects.
- **Integration** classes tagged **`Category=Integration`** for filtering.
- **Docs:** `docs/MimironsGoldOMatic.Backend/ReadME.md`, `docs/SETUP.md` — `dotnet test --filter Category=Unit` vs full suite.

## Verification

- `dotnet test src/MimironsGoldOMatic.slnx --filter Category=Unit` — pass (22 tests).
- `dotnet test src/MimironsGoldOMatic.slnx` — pass (29 tests, Docker for integration).

## Notes

- MediatR handlers and HTTP surface remain covered by **integration** tests; further **unit** coverage would need interfaces/mocks around `IDocumentStore` or smaller extracted services.

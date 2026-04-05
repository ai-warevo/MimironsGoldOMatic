# Plan — Backend unit tests

- Add **fast** xUnit tests (no Docker) under `Backend.Tests/Unit/` with `[Trait("Category","Unit")]`.
- Cover **`RouletteTime`** (5-minute floor / next boundary invariants).
- Cover **`SpinPhaseResolver`** (collecting / spinning / verification / completed / idle) with fixed UTC instants.
- Extract **`TwGoldChatEnrollmentParser`** from **`ChatEnrollmentService`** so `!twgold <name>` parsing is tested without Marten.
- Document `dotnet test --filter Category=Unit` in Backend ReadME.

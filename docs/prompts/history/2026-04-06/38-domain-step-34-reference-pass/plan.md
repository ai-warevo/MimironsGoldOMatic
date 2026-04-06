## Plan

1. Confirm Domain code only needs MediatR, `MimironsGoldOMatic.Backend.Abstract`, and `MimironsGoldOMatic.Shared`.
2. Drop `MimironsGoldOMatic.Backend.Shared` from Domain csproj (no source references; avoid extra layer).
3. `dotnet build` Domain project; fix only if build fails.

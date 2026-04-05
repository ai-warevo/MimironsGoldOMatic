# Plan

1. Root cause: second `dotnet restore` on the unit test project **without** `-r win-x64` rewrites Desktop’s `obj/project.assets.json` without the RID-specific graph; Release build still applies `RuntimeIdentifier=win-x64` on Desktop → NETSDK1047.
2. Fix: single `dotnet restore` on `MimironsGoldOMatic.Desktop.UnitTests.csproj` with `-r win-x64` (restores transitive Desktop + Shared with correct assets). Remove redundant Desktop-only restore.

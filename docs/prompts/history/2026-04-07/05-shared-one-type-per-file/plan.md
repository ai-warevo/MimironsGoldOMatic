# Plan

1. Split `GiftApiContracts.cs` into one `.cs` per type (enum + six records).
2. Split `DesktopEbsApiContracts.cs` into three request record files; preserve XML summaries.
3. Remove the old multi-type files; keep `namespace MimironsGoldOMatic.Shared` and type names unchanged so consumers need no edits.
4. Verify with `dotnet build src/MimironsGoldOMatic.slnx`.

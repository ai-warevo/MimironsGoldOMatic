# Report: Desktop single EXE publish

## Modified / added files

- `src/MimironsGoldOMatic.Desktop/Properties/PublishProfiles/win-x64-single-file.pubxml` (new)
- `src/MimironsGoldOMatic.Desktop/MimironsGoldOMatic.Desktop.csproj` — `RemovePublishPdbsWhenSingleFile` target
- `.github/workflows/release.yml` — desktop publish uses profile (self-contained single file)

## Verification

- `dotnet publish ... -p:PublishProfile=win-x64-single-file -o ./publish/profile-test` → output contained only `MimironsGoldOMatic.Desktop.exe`.

## Notes

- Self-contained EXE is much larger than framework-dependent multi-file layout; no separate .NET Desktop runtime install on target PCs.
- Release ZIP still adds `README.txt` next to the EXE.

# Plan: Desktop true single-file publish

- **Cause:** Framework-dependent single-file does not bundle WPF/native dependencies; many DLLs + host files remain beside the EXE.
- **Fix:** Self-contained `PublishSingleFile` + `IncludeNativeLibrariesForSelfExtract`; strip `*.pdb` from publish output after publish when single-file (referenced projects still emit PDBs).
- **Artifacts:** Add `Properties/PublishProfiles/win-x64-single-file.pubxml`; align `release.yml` desktop step; optional MSBuild target on Desktop project.

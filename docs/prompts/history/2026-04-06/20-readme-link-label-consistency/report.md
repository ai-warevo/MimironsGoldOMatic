# Report

## Scope
Normalize markdown link label naming consistency for README/ReadME references without renaming files or changing link targets.

## Files updated
- docs/reference/UI_SPEC.md`n- docs/components/desktop/UI_SPEC.md`n- docs/components/twitch-extension/UI_SPEC.md`n- docs/components/wow-addon/UI_SPEC.md`n
## Summary of changes
- Replaced mixed filename-style link labels with human-readable labels (for example, Repository README, Desktop guide, WoW Addon component guide).
- Preserved all link targets and existing file names (ReadME.md vs README.md) as-is.

## Verification
- Manual diff check confirms links resolve to same targets and only label text changed in this pass.


# Plan

1. Add `.github/scripts/collect-release-assets.sh` with the same behavior as the inlined bash (env: `RELEASE_VERSION`, optional `RELEASE_ASSETS_DIR` / `UPLOAD_DIR`, append `NOTES_FILE` to `GITHUB_ENV` when set).
2. Replace the long `run: |` block in `release.yml` with `bash .github/scripts/collect-release-assets.sh`.

## Risks

Low: behavior must match; `RELEASE_VERSION` must remain available from the prior step via `GITHUB_ENV`.

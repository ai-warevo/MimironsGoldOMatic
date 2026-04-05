# Report

## Modified / added files

- `.github/scripts/collect-release-assets.sh` (new)
- `.github/workflows/release.yml` (refactored step)
- `docs/prompts/history/2026-04-05/25-release-collect-assets-script/*` (agent log)

## Verification

- Local `bash -n` was not run (no `bash` on the Windows shell used); script is intended for `ubuntu-latest` where `bash` is available.

## Notes

Optional overrides: `RELEASE_ASSETS_DIR`, `UPLOAD_DIR`. `NOTES_FILE` is only appended to `GITHUB_ENV` when that variable is set (GitHub Actions).

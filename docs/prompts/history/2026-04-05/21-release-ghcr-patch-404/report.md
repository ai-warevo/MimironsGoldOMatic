# Report

## Modified files

- `.github/workflows/release.yml` — GHCR visibility step uses package JSON `url`, JSON content-type, optional `GH_PACKAGES_VISIBILITY_PAT`, clearer 404 guidance.

## Operator note

Add repository or organization secret **`GH_PACKAGES_VISIBILITY_PAT`** (classic: `write:packages`; fine-grained: Packages write for org; SSO authorized if required) so PATCH succeeds for org-owned packages.

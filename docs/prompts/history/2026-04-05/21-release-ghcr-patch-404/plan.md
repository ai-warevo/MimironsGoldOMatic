# Plan

1. Use canonical `url` from GET package JSON for PATCH (not hand-built path) + `Content-Type: application/json`.
2. Document that org GHCR visibility changes often require a PAT with `write:packages` / package admin; support optional secret `GH_PACKAGES_VISIBILITY_PAT` used in place of `GITHUB_TOKEN` for API calls.
3. Improve error text when PATCH is 404 and PAT is unset.

# Plan

1. Enumerate all *.md files in the repository and exclude docs/prompts/**.
2. Apply an aggressive normalization pass for markdown quality:
   - trim trailing whitespace
   - normalize ATX headings spacing
   - collapse excessive empty lines
   - remove accidental consecutive duplicate content lines
3. Refresh stale Tier C status metadata and cross-links in e2e and index docs.
4. Verify modified set via git diff and produce completion report.

## Risks
- Broad markdown normalization may touch many files; avoid semantic drift by limiting automation to structural cleanup.
- Existing user edits are preserved and not reverted.

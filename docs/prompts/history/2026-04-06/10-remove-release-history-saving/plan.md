## Goal
Remove the release helper’s behavior that writes changelog drafts into a local “history” folder.

## Scope
- Update `.cursor/skills/github-release/scripts/release-skill.js` to stop creating/writing `.cursor/skills/github-release/history/*`.
- Keep the rest of the release flow intact (changelog generation, tag creation/push, browser open).

## Approach
- Delete the changelog draft persistence function and its call site.
- Remove now-unused imports (`fs`, `path`).
- Update script header documentation to match behavior.

## Risks / Mitigations
- **Risk**: Another part of the script depends on the history path.
  - **Mitigation**: Run the script with `--dry-run` to verify it still executes the normal path without attempting filesystem writes.


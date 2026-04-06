## Summary
Removed the release helper’s behavior that persisted changelog drafts into a local “history” directory under `.cursor/skills/github-release/history/`.

## Modified files
- `.cursor/skills/github-release/scripts/release-skill.js`

## Verification
- Ran the script with `--dry-run`; it executed normally up to the expected “no commits since last tag” guard and did not attempt any filesystem writes.


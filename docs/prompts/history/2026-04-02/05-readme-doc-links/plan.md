## Audience

- **Who is this for**: Contributors and maintainers reading the repo README(s).
- **Their goal**: Quickly find the right documentation entrypoints (module READMEs + prompt history) without scrolling or hunting paths.
- **Assumed knowledge**: Basic GitHub/Markdown navigation.

## Scope

- **What should be covered**:
  - Update `README.md` to:
    - Collapse/squash "Project prompts (Cursor)" to a single link to `docs/prompts/history/`.
    - Add a concise "Docs" index linking to key documentation files under `docs/` (component READMEs + prompt dirs).
  - Update `docs/ReadME.md` to:
    - Add the same (or slightly more detailed) docs index linking to component READMEs under `docs/`.
    - Collapse "Cursor prompts" to a single link to `docs/prompts/history/` (relative path from `docs/ReadME.md`).
- **What should not be covered**:
  - No behavior/code changes outside Markdown docs.
  - No content rewrite of individual component READMEs beyond linking.

## Files expected to change

- `README.md`
- `docs/ReadME.md`
- `docs/prompts/history/2026-04-02/05-readme-doc-links/{prompt,plan,checks,report}.md`

## Risks / Notes

- Link correctness: paths differ between repo-root `README.md` and `docs/ReadME.md` (relative links must be adjusted).
- Case sensitivity: repo uses `ReadME.md` (nonstandard casing) in multiple places; links must match exact filenames.


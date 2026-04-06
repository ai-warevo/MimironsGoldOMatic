# GitHub issue #17

Improve the `/commit` Cursor command to fully comply with Conventional Commits v1.0.0 while preserving `Made-with: Cursor` and `Co-authored-by: Cursor Agent <cursoragent@cursor.com>` at the end of the message.

Scope: expand types (build, ci, perf, revert, security), breaking changes (`!` + `BREAKING CHANGE` footer), any scope with path-based suggestions, standard footers before custom metadata, stronger body/line-length rules, Unix heredoc + PowerShell UTF-8 `-F`, validation checklist.

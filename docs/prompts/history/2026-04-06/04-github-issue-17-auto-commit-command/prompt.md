# GitHub issue #17 (follow-up)

Rewrite `/commit` so it automatically runs `git add` (default `.` or selective), writes message to a temp file, runs `git commit -F`, cleans up, and replies only `Commit successful: <header>` or an error—not manual shell instructions.

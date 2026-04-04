#!/usr/bin/env node
/**
 * Forwards all CLI arguments to amend_commit_meta.mjs in this directory (same as
 * running node on that file directly, with correct argv pass-through on Windows).
 *
 * Examples (run from repo root; adjust path if your cwd differs):
 *
 *   node .cursor/skills/amend-commit/scripts/amend_commit_meta_wrap.mjs --help
 *
 *   node .cursor/skills/amend-commit/scripts/amend_commit_meta_wrap.mjs run --help
 *
 *   node .cursor/skills/amend-commit/scripts/amend_commit_meta_wrap.mjs run --mode all --message-only --made-with "Cursor" --co-authored-by "Name <name@example.com>"
 *
 *   node .cursor/skills/amend-commit/scripts/amend_commit_meta_wrap.mjs run --mode single --commit <full-or-abbrev-sha> --message-only --made-with "Tool" --co-authored-by "Other <o@example.com>"
 *
 *   node .cursor/skills/amend-commit/scripts/amend_commit_meta_wrap.mjs run --mode all --base origin/main --made-with "X" --co-authored-by "Y <y@example.com>" --author-name "A" --author-email "a@example.com" --committer-name "C" --committer-email "c@example.com"
 *
 *   node .cursor/skills/amend-commit/scripts/amend_commit_meta_wrap.mjs run --mode all --base origin/main --made-with "Cursor" --co-authored-by "Cursor Agent <cursoragent@cursor.com>" --author-name "Anatoly Ivanov" --author-email "ai.vibeqodez@vk.com" --committer-name "Anatoly Ivanov" --committer-email "ai.vibeqodez@vk.com"
 */

import { spawnSync } from "node:child_process";
import path from "node:path";
import process from "node:process";
import { fileURLToPath } from "node:url";

const scriptDir = path.dirname(fileURLToPath(import.meta.url));
const target = path.join(scriptDir, "amend_commit_meta.mjs");
const forwarded = process.argv.slice(2);

const result = spawnSync(process.execPath, [target, ...forwarded], {
  stdio: "inherit",
  windowsHide: true,
});

if (result.error) {
  console.error(result.error.message);
  process.exit(1);
}

const code = result.status;
process.exit(code === null ? 1 : code);

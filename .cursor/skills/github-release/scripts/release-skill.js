#!/usr/bin/env node
/**
 * Local release helper: validate SemVer, create annotated tag vX.Y.Z, push to origin.
 * Run from repo root: node .cursor/skills/github-release/scripts/release-skill.js
 * Flags: --version=1.2.3 --message="..." --no-push --open-browser --dry-run
 */

const { spawnSync } = require("child_process");
const readline = require("readline");

const SEMVER_RE = /^\d+\.\d+\.\d+$/;

function log(msg) {
  console.log(`[release] ${msg}`);
}

function warn(msg) {
  console.warn(`[release] ${msg}`);
}

function die(msg, code = 1) {
  console.error(`[release] ERROR: ${msg}`);
  process.exit(code);
}

function git(args, opts = {}) {
  const r = spawnSync("git", args, {
    encoding: "utf8",
    ...opts,
  });
  return {
    status: r.status ?? 1,
    stdout: (r.stdout || "").trim(),
    stderr: (r.stderr || "").trim(),
  };
}

function parseArgs(argv) {
  const out = {
    version: null,
    message: null,
    push: true,
    openBrowser: false,
    dryRun: false,
  };
  for (const a of argv) {
    if (a === "--no-push") out.push = false;
    else if (a === "--open-browser") out.openBrowser = true;
    else if (a === "--dry-run") out.dryRun = true;
    else if (a.startsWith("--version=")) out.version = a.slice("--version=".length);
    else if (a.startsWith("--message=")) out.message = a.slice("--message=".length);
  }
  return out;
}

function repoRoot() {
  const { status, stdout } = git(["rev-parse", "--show-toplevel"]);
  if (status !== 0) die("Not inside a Git repository (git rev-parse failed).");
  return stdout;
}

function tagExists(tag) {
  const { status } = git(["rev-parse", `refs/tags/${tag}`]);
  return status === 0;
}

function remotePushUrl() {
  const { status, stdout } = git(["remote", "get-url", "--push", "origin"]);
  if (status !== 0) return null;
  return stdout;
}

/** Best-effort GitHub releases URL for owner/repo */
function githubReleaseUrlFromRemote(tag) {
  const url = remotePushUrl();
  if (!url) return null;
  let m = url.match(/github\.com[:/]([^/]+)\/([^/.]+)/i);
  if (!m) return null;
  const owner = m[1];
  const repo = m[2].replace(/\.git$/i, "");
  return `https://github.com/${owner}/${repo}/releases/tag/${encodeURIComponent(tag)}`;
}

function openUrl(url) {
  const { platform } = process;
  if (platform === "win32") {
    spawnSync("cmd", ["/c", "start", "", url], { stdio: "ignore", detached: true });
  } else if (platform === "darwin") {
    spawnSync("open", [url], { stdio: "ignore", detached: true });
  } else {
    spawnSync("xdg-open", [url], { stdio: "ignore", detached: true });
  }
}

function promptLine(question) {
  const rl = readline.createInterface({ input: process.stdin, output: process.stdout });
  return new Promise((resolve) => {
    rl.question(question, (answer) => {
      rl.close();
      resolve(answer.trim());
    });
  });
}

async function main() {
  const args = parseArgs(process.argv.slice(2));
  let ver = args.version;

  if (!ver && process.stdin.isTTY) {
    ver = await promptLine("Enter new version (SemVer MAJOR.MINOR.PATCH, e.g. 1.2.3): ");
  }
  if (!ver) {
    die("No version. Pass --version=1.2.3 or run interactively in a TTY.");
  }
  ver = ver.replace(/^v/i, "");
  if (!SEMVER_RE.test(ver)) {
    die(`Invalid SemVer "${ver}". Expected digits like 1.2.3 (optionally prefixed with v).`);
  }

  const tag = `v${ver}`;
  const cwd = repoRoot();
  process.chdir(cwd);
  log(`Repository: ${cwd}`);

  if (tagExists(tag)) {
    die(
      `Tag "${tag}" already exists locally. Delete it with "git tag -d ${tag}" if you really want to recreate, or pick a new version.`,
      2
    );
  }

  const msg =
    args.message ||
    `Release ${tag}`;

  const runTag = ["tag", "-a", tag, "-m", msg];
  const runPush = ["push", "origin", tag];

  if (args.dryRun) {
    log(`Dry run: would run: git ${runTag.join(" ")}`);
    if (args.push) log(`Dry run: would run: git ${runPush.join(" ")}`);
    return;
  }

  log(`Creating annotated tag ${tag}…`);
  const t = git(runTag);
  if (t.status !== 0) {
    die(`git tag failed:\n${t.stderr || t.stdout}`);
  }

  if (!args.push) {
    log(`Tag created locally. Push when ready: git push origin ${tag}`);
    return;
  }

  log("Pushing tag to origin…");
  const p = git(runPush);
  if (p.status !== 0) {
    warn(`git push failed. Remove local tag if needed: git tag -d ${tag}`);
    die(p.stderr || p.stdout || "git push failed");
  }
  log(`Pushed ${tag} to origin.`);

  if (args.openBrowser) {
    const rel = githubReleaseUrlFromRemote(tag);
    if (rel) {
      log(`Opening ${rel}`);
      openUrl(rel);
    } else {
      warn("Could not parse GitHub URL from origin; open Releases manually.");
    }
  }
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});

#!/usr/bin/env bash
# Collect release ZIPs and backend-image.tar into upload/, write SHA256SUMS.txt and release-notes.md.
# Intended for GitHub Actions create-release job; paths match download-artifact layout.
set -euo pipefail

RELEASE_ASSETS_DIR="${RELEASE_ASSETS_DIR:-release-assets}"
UPLOAD_DIR="${UPLOAD_DIR:-upload}"
WORKSPACE="${GITHUB_WORKSPACE:-${PWD:-.}}"

if [ -z "${RELEASE_VERSION:-}" ]; then
  echo "::error::RELEASE_VERSION is not set" >&2
  exit 1
fi

mkdir -p "${UPLOAD_DIR}"
find "${RELEASE_ASSETS_DIR}" -type f -name '*.zip' -print0 | while IFS= read -r -d '' f; do
  base="$(basename "$f")"
  cp -f "$f" "${UPLOAD_DIR}/${base}"
done
zip_count="$(find "${UPLOAD_DIR}" -maxdepth 1 -name '*.zip' | wc -l)"
zip_count="${zip_count//[[:space:]]/}"
if [ "${zip_count:-0}" -lt 1 ]; then
  echo "No ZIP artifacts found under ${RELEASE_ASSETS_DIR}"
  find "${RELEASE_ASSETS_DIR}" -type f || true
  exit 1
fi
TAR="$(find "${RELEASE_ASSETS_DIR}" -type f -name 'backend-image.tar' | head -n1)"
if [ -z "${TAR}" ]; then
  echo "::error::backend-image.tar not found (expected under ${RELEASE_ASSETS_DIR}/backend-docker-image/ after download)"
  find "${RELEASE_ASSETS_DIR}" -type f || true
  exit 1
fi
cp -f "${TAR}" "${UPLOAD_DIR}/backend-image.tar"
(cd "${UPLOAD_DIR}" && sha256sum *.zip backend-image.tar > SHA256SUMS.txt)

CURRENT_TAG="${RELEASE_VERSION}"
if git rev-parse -q --verify "refs/tags/${CURRENT_TAG}" >/dev/null 2>&1; then
  PREV_TAG="$(git tag -l 'v*' --sort=-version:refname | awk -v cur="${CURRENT_TAG}" '$0 != cur {print; exit}')"
else
  PREV_TAG="$(git describe --tags --abbrev=0 2>/dev/null || true)"
fi
NOTES_FILE="${WORKSPACE}/release-notes.md"
TMP_COMMITS="$(mktemp)"
trap 'rm -f "${TMP_COMMITS}"' EXIT
if [ -n "${PREV_TAG}" ]; then
  git log --pretty=format:'%s (%h)' "${PREV_TAG}..HEAD" > "${TMP_COMMITS}" || true
else
  git log --pretty=format:'%s (%h)' -n 50 > "${TMP_COMMITS}" || true
fi
{
  echo "## Changelog (Conventional Commits)"
  echo ""
  echo "Commits since \`${PREV_TAG:-<repository start>}\` — grouped by prefix where possible."
  echo ""
  echo "### Features"
  FEAT="$(grep -E '^feat(\([^)]*\))?!?:' "${TMP_COMMITS}" || true)"
  if [ -z "${FEAT}" ]; then echo "- _(none)_"; else echo "${FEAT}" | sed 's/^/- /'; fi
  echo ""
  echo "### Fixes"
  FIX="$(grep -E '^fix(\([^)]*\))?!?:' "${TMP_COMMITS}" || true)"
  if [ -z "${FIX}" ]; then echo "- _(none)_"; else echo "${FIX}" | sed 's/^/- /'; fi
  echo ""
  echo "### Other"
  REST="$(grep -Ev '^(feat|fix)(\([^)]*\))?!?:' "${TMP_COMMITS}" | grep -v '^$' || true)"
  if [ -z "${REST}" ]; then echo "- _(none)_"; else echo "${REST}" | sed 's/^/- /'; fi
  echo ""
  echo "## Artifacts"
  echo ""
  echo "ZIP bundles (Desktop, WoW addon, Twitch Extension), Backend container image (\`backend-image.tar\`), and \`SHA256SUMS.txt\` are attached. The same image is pushed to GHCR."
} > "${NOTES_FILE}"

if [ -n "${GITHUB_ENV:-}" ]; then
  echo "NOTES_FILE=${NOTES_FILE}" >> "${GITHUB_ENV}"
fi

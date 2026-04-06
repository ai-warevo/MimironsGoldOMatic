function parseSemverParts(value: string): number[] | null {
  const trimmed = value.trim()
  const match = /^(\d+)\.(\d+)\.(\d+)(?:[-+].*)?$/.exec(trimmed)
  if (!match) return null

  const major = Number.parseInt(match[1], 10)
  const minor = Number.parseInt(match[2], 10)
  const patch = Number.parseInt(match[3], 10)

  if (Number.isNaN(major) || Number.isNaN(minor) || Number.isNaN(patch)) {
    return null
  }

  return [major, minor, patch]
}

/**
 * Returns true if remote is newer than local.
 * Falls back to locale string compare when semver parsing is unavailable.
 */
export function isMimironsGoldOMaticNewerVersion(remote: string, local: string): boolean {
  const remoteParts = parseSemverParts(remote)
  const localParts = parseSemverParts(local)

  if (!remoteParts || !localParts) {
    return remote.localeCompare(local, undefined, { numeric: true, sensitivity: 'base' }) > 0
  }

  for (let i = 0; i < 3; i += 1) {
    if (remoteParts[i] === localParts[i]) continue
    return remoteParts[i] > localParts[i]
  }

  return false
}


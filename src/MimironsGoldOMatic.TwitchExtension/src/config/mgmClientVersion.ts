const rawVersion = import.meta.env.VITE_APP_VERSION?.trim()

/** Build-time extension version. Falls back to 0.0.0 when unavailable. */
export const MIMIRONS_GOLD_O_MATIC_EXTENSION_VERSION =
  rawVersion && rawVersion.length > 0 ? rawVersion : '0.0.0'


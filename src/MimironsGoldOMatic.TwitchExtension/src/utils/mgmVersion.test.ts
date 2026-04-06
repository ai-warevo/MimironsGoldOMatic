import { describe, expect, it } from '@jest/globals'
import { isMimironsGoldOMaticNewerVersion } from './mgmVersion'

describe('isMimironsGoldOMaticNewerVersion', () => {
  it('returns true when remote major version is newer', () => {
    expect(isMimironsGoldOMaticNewerVersion('2.0.0', '1.9.9')).toBe(true)
  })

  it('returns true when remote minor version is newer', () => {
    expect(isMimironsGoldOMaticNewerVersion('1.4.0', '1.3.9')).toBe(true)
  })

  it('returns true when remote patch version is newer', () => {
    expect(isMimironsGoldOMaticNewerVersion('1.4.2', '1.4.1')).toBe(true)
  })

  it('returns false when versions are equal', () => {
    expect(isMimironsGoldOMaticNewerVersion('1.4.2', '1.4.2')).toBe(false)
  })

  it('returns false when remote version is older', () => {
    expect(isMimironsGoldOMaticNewerVersion('1.4.1', '1.4.2')).toBe(false)
  })

  it('accepts semver with prerelease/build suffixes', () => {
    expect(isMimironsGoldOMaticNewerVersion('1.2.4-beta+7', '1.2.3')).toBe(true)
  })

  it('falls back without throwing for malformed versions', () => {
    expect(isMimironsGoldOMaticNewerVersion('v2', 'v1')).toBe(true)
    expect(isMimironsGoldOMaticNewerVersion('invalid', 'zzz')).toBe(false)
  })
})


import { describe, expect, it } from 'vitest'
import { mapMimironsGoldOMaticApiErrorToUi } from './mapMgmApiErrorToUi'

describe('mapMimironsGoldOMaticApiErrorToUi', () => {
  it('returns fallbacks when err is nullish', () => {
    expect(
      mapMimironsGoldOMaticApiErrorToUi(undefined, 'T', 'B'),
    ).toEqual({ title: 'T', body: 'B' })
    expect(
      mapMimironsGoldOMaticApiErrorToUi(null, 'T', 'B'),
    ).toEqual({ title: 'T', body: 'B' })
  })

  it('maps known API codes to viewer copy', () => {
    expect(
      mapMimironsGoldOMaticApiErrorToUi(
        { code: 'not_subscriber', message: 'x', details: null },
        'F',
        'FB',
      ).title,
    ).toBe('Subscriber only')
    expect(
      mapMimironsGoldOMaticApiErrorToUi(
        { code: 'lifetime_cap_reached', message: 'x', details: null },
        'F',
        'FB',
      ).title,
    ).toBe('Gnome vault full')
  })

  it('uses message for unknown codes', () => {
    expect(
      mapMimironsGoldOMaticApiErrorToUi(
        { code: 'unknown', message: 'Custom', details: null },
        'F',
        'FB',
      ),
    ).toEqual({ title: 'F', body: 'Custom' })
  })
})

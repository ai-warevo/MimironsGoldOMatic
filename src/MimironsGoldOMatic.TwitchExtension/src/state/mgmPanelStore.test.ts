import { beforeEach, describe, expect, it } from '@jest/globals'
import type { PoolMeResponse, RouletteStateResponse } from '../api/models'
import { useMimironsGoldOMaticPanelStore } from './mgmPanelStore'

const roulette: RouletteStateResponse = {
  nextSpinAt: '2026-01-01T00:05:00.000Z',
  serverNow: '2026-01-01T00:00:00.000Z',
  spinIntervalSeconds: 300,
  poolParticipantCount: 2,
  spinPhase: 'idle',
  currentSpinCycleId: 'cycle-1',
}

const poolMe: PoolMeResponse = {
  isEnrolled: true,
  characterName: 'Norinn',
}

describe('useMimironsGoldOMaticPanelStore', () => {
  beforeEach(() => {
    useMimironsGoldOMaticPanelStore.setState({
      authView: 'loading',
      roulette: null,
      poolMe: null,
      myLast: null,
      pollBackoffMs: 3000,
      pollError: null,
      lastPollWallClockMs: null,
      uiError: null,
      claimBusy: false,
      pollGeneration: 0,
    })
  })

  it('setPollSuccess stores payloads and resets backoff metadata', () => {
    useMimironsGoldOMaticPanelStore.getState().setPollFailure({
      kind: 'network',
      nextBackoffMs: 6000,
    })
    useMimironsGoldOMaticPanelStore.getState().setPollSuccess({
      roulette,
      poolMe,
      myLast: null,
      giftQueue: [],
      myGift: null,
    })

    const s = useMimironsGoldOMaticPanelStore.getState()
    expect(s.roulette).toEqual(roulette)
    expect(s.poolMe).toEqual(poolMe)
    expect(s.myLast).toBeNull()
    expect(s.pollBackoffMs).toBe(3000)
    expect(s.pollError).toBeNull()
    expect(s.lastPollWallClockMs).not.toBeNull()
  })

  it('setPollFailure records API errors and applies backoff', () => {
    useMimironsGoldOMaticPanelStore.getState().setPollFailure({
      kind: 'api',
      status: 503,
      body: { code: 'x', message: 'y', details: {} },
      nextBackoffMs: 6000,
    })

    const s = useMimironsGoldOMaticPanelStore.getState()
    expect(s.pollError?.kind).toBe('api')
    expect(s.pollError?.status).toBe(503)
    expect(s.pollBackoffMs).toBe(6000)
  })

  it('bumpPoll clears poll error, resets backoff, and increments generation', () => {
    useMimironsGoldOMaticPanelStore.getState().setPollFailure({
      kind: 'api',
      status: 500,
      nextBackoffMs: 12_000,
    })
    const genBefore = useMimironsGoldOMaticPanelStore.getState().pollGeneration
    useMimironsGoldOMaticPanelStore.getState().bumpPoll()
    const s = useMimironsGoldOMaticPanelStore.getState()
    expect(s.pollGeneration).toBe(genBefore + 1)
    expect(s.pollError).toBeNull()
    expect(s.pollBackoffMs).toBe(3000)
  })

  it('setUiError and setClaimBusy update UI flags', () => {
    useMimironsGoldOMaticPanelStore.getState().setUiError({
      code: 'active_payout_exists',
      message: 'm',
      details: {},
    })
    useMimironsGoldOMaticPanelStore.getState().setClaimBusy(true)
    let s = useMimironsGoldOMaticPanelStore.getState()
    expect(s.uiError?.code).toBe('active_payout_exists')
    expect(s.claimBusy).toBe(true)

    useMimironsGoldOMaticPanelStore.getState().setUiError(null)
    useMimironsGoldOMaticPanelStore.getState().setClaimBusy(false)
    s = useMimironsGoldOMaticPanelStore.getState()
    expect(s.uiError).toBeNull()
    expect(s.claimBusy).toBe(false)
  })
})

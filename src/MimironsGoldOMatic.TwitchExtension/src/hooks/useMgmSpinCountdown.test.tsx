import { renderHook, act } from '@testing-library/react'
import { describe, expect, it, beforeEach, afterEach, jest } from '@jest/globals'
import type { RouletteStateResponse } from '../api/models'
import { useMgmSpinCountdown } from './useMgmSpinCountdown'

const roulette: RouletteStateResponse = {
  nextSpinAt: '2026-01-01T00:05:00.000Z',
  serverNow: '2026-01-01T00:00:00.000Z',
  spinIntervalSeconds: 300,
  poolParticipantCount: 0,
  spinPhase: 'idle',
  currentSpinCycleId: null,
}

describe('useMgmSpinCountdown', () => {
  beforeEach(() => {
    jest.useFakeTimers()
    jest.setSystemTime(new Date('2026-01-01T00:00:00.000Z'))
  })

  afterEach(() => {
    jest.useRealTimers()
  })

  it('returns zeroed display when roulette or last poll is missing', () => {
    const { result: noRoulette } = renderHook(() =>
      useMgmSpinCountdown(null, Date.now()),
    )
    expect(noRoulette.current).toEqual({ remainingSeconds: 0, labelMmSs: '00:00' })

    const { result: noPoll } = renderHook(() => useMgmSpinCountdown(roulette, null))
    expect(noPoll.current).toEqual({ remainingSeconds: 0, labelMmSs: '00:00' })
  })

  it('computes remaining seconds from server skew and ticks down each second', () => {
    const lastPollWallClockMs = Date.now()
    const { result } = renderHook(() => useMgmSpinCountdown(roulette, lastPollWallClockMs))

    expect(result.current.remainingSeconds).toBe(300)
    expect(result.current.labelMmSs).toBe('05:00')

    act(() => {
      jest.advanceTimersByTime(1000)
    })
    expect(result.current.remainingSeconds).toBe(299)
    expect(result.current.labelMmSs).toBe('04:59')
  })

  it('treats invalid ISO timestamps as empty countdown', () => {
    const bad: RouletteStateResponse = {
      ...roulette,
      serverNow: 'not-a-date',
      nextSpinAt: 'also-bad',
    }
    const { result } = renderHook(() => useMgmSpinCountdown(bad, Date.now()))
    expect(result.current).toEqual({ remainingSeconds: 0, labelMmSs: '00:00' })
  })
})

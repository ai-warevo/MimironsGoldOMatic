import { renderHook, waitFor, act } from '@testing-library/react'
import { describe, expect, it, beforeEach, jest } from '@jest/globals'
import { createMimironsGoldOMaticApiClient } from '../api/mgmApiClient'
import type {
  PoolMeResponse,
  RouletteStateResponse,
} from '../api/models'
import { useMimironsGoldOMaticPanelStore } from '../state/mgmPanelStore'
import { useMgmEbsPolling } from './useMgmEbsPolling'

jest.mock('../api/mgmApiClient', () => ({
  createMimironsGoldOMaticApiClient: jest.fn(),
}))

const mockCreateClient = jest.mocked(createMimironsGoldOMaticApiClient)

const roulette: RouletteStateResponse = {
  nextSpinAt: '2026-01-01T00:05:00.000Z',
  serverNow: '2026-01-01T00:00:00.000Z',
  spinIntervalSeconds: 300,
  poolParticipantCount: 1,
  spinPhase: 'idle',
  currentSpinCycleId: null,
}

const poolMe: PoolMeResponse = {
  isEnrolled: false,
  characterName: null,
}

function resetStoreForPolling() {
  useMimironsGoldOMaticPanelStore.setState({
    authView: 'ready',
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
}

describe('useMgmEbsPolling', () => {
  beforeEach(() => {
    resetStoreForPolling()
    mockCreateClient.mockClear()
    mockCreateClient.mockReturnValue(
      {
        getRouletteState: async () => roulette,
        getPoolMe: async () => poolMe,
        getPayoutsMyLast: async () => {
          throw Object.assign(new Error('not found'), {
            isAxiosError: true,
            response: { status: 404 },
          })
        },
      } as unknown as ReturnType<typeof createMimironsGoldOMaticApiClient>,
    )
  })

  it('does not poll when auth is not ready', async () => {
    useMimironsGoldOMaticPanelStore.setState({ authView: 'loading' })
    renderHook(() => useMgmEbsPolling('https://ebs.example/mgm'))
    await act(async () => {
      await new Promise((r) => setTimeout(r, 50))
    })
    expect(mockCreateClient).not.toHaveBeenCalled()
  })

  it('does not poll without EBS base URL', async () => {
    renderHook(() => useMgmEbsPolling(undefined))
    await act(async () => {
      await new Promise((r) => setTimeout(r, 50))
    })
    expect(mockCreateClient).not.toHaveBeenCalled()
  })

  it('after initial delay, merges roulette/pool/payout into the panel store', async () => {
    renderHook(() => useMgmEbsPolling('https://ebs.example/mgm'))

    await waitFor(
      () => {
        expect(useMimironsGoldOMaticPanelStore.getState().roulette).toEqual(roulette)
      },
      { timeout: 8000 },
    )
    expect(useMimironsGoldOMaticPanelStore.getState().poolMe).toEqual(poolMe)
    expect(useMimironsGoldOMaticPanelStore.getState().myLast).toBeNull()
  })
})

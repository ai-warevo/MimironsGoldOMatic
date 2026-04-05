import { renderHook, waitFor, act } from '@testing-library/react'
import { describe, expect, it, beforeEach, jest } from '@jest/globals'
import { createMimironsGoldOMaticEbsClient } from '../api/mgmEbsClient'
import {
  createMimironsGoldOMaticEbsRepository,
  type MimironsGoldOMaticEbsRepository,
} from '../api/mgmEbsRepository'
import type {
  MimironsGoldOMaticPoolMe,
  MimironsGoldOMaticRouletteState,
} from '../mgmTypes'
import { useMimironsGoldOMaticPanelStore } from '../state/mgmPanelStore'
import { useMgmEbsPolling } from './useMgmEbsPolling'

jest.mock('../api/mgmEbsClient', () => ({
  createMimironsGoldOMaticEbsClient: jest.fn(() => ({})),
}))

jest.mock('../api/mgmEbsRepository', () => ({
  createMimironsGoldOMaticEbsRepository: jest.fn(),
}))

const mockCreateClient = jest.mocked(createMimironsGoldOMaticEbsClient)
const mockCreateRepo = jest.mocked(createMimironsGoldOMaticEbsRepository)

const roulette: MimironsGoldOMaticRouletteState = {
  nextSpinAt: '2026-01-01T00:05:00.000Z',
  serverNow: '2026-01-01T00:00:00.000Z',
  spinIntervalSeconds: 300,
  poolParticipantCount: 1,
  spinPhase: 'idle',
  currentSpinCycleId: null,
}

const poolMe: MimironsGoldOMaticPoolMe = {
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
    mockCreateRepo.mockClear()
    mockCreateClient.mockReturnValue({} as ReturnType<typeof createMimironsGoldOMaticEbsClient>)
    const repo: MimironsGoldOMaticEbsRepository = {
      getRouletteState: async () => roulette,
      getPoolMe: async () => poolMe,
      getMyLastPayout: async () => null,
      postClaim: async () => undefined,
    }
    mockCreateRepo.mockReturnValue(repo)
  })

  it('does not poll when auth is not ready', async () => {
    useMimironsGoldOMaticPanelStore.setState({ authView: 'loading' })
    renderHook(() => useMgmEbsPolling('https://ebs.example/mgm'))
    await act(async () => {
      await new Promise((r) => setTimeout(r, 50))
    })
    expect(mockCreateRepo).not.toHaveBeenCalled()
  })

  it('does not poll without EBS base URL', async () => {
    renderHook(() => useMgmEbsPolling(undefined))
    await act(async () => {
      await new Promise((r) => setTimeout(r, 50))
    })
    expect(mockCreateRepo).not.toHaveBeenCalled()
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

import { AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from 'axios'
import { describe, expect, it, jest } from '@jest/globals'
import { createMimironsGoldOMaticEbsRepository } from './mgmEbsRepository'
import type {
  MimironsGoldOMaticPoolMe,
  MimironsGoldOMaticPayoutDto,
  MimironsGoldOMaticRouletteState,
} from '../mgmTypes'

function axiosError(status: number, data: unknown): AxiosError {
  const err = new AxiosError('request failed')
  err.response = {
    status,
    data,
    statusText: 'Error',
    headers: {},
    config: {} as InternalAxiosRequestConfig,
  }
  return err
}

function mockClient(get: jest.Mock, post: jest.Mock): AxiosInstance {
  return { get, post } as unknown as AxiosInstance
}

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

describe('createMimironsGoldOMaticEbsRepository', () => {
  it('getRouletteState GETs /api/roulette/state', async () => {
    const get = jest.fn(async () => ({ data: roulette }))
    const repo = createMimironsGoldOMaticEbsRepository(mockClient(get, jest.fn()))
    await expect(repo.getRouletteState()).resolves.toEqual(roulette)
    expect(get).toHaveBeenCalledWith('/api/roulette/state')
  })

  it('getPoolMe GETs /api/pool/me', async () => {
    const get = jest.fn(async () => ({ data: poolMe }))
    const repo = createMimironsGoldOMaticEbsRepository(mockClient(get, jest.fn()))
    await expect(repo.getPoolMe()).resolves.toEqual(poolMe)
    expect(get).toHaveBeenCalledWith('/api/pool/me')
  })

  it('getMyLastPayout returns null on 404', async () => {
    const get = jest.fn(async () => {
      throw axiosError(404, {})
    })
    const repo = createMimironsGoldOMaticEbsRepository(mockClient(get, jest.fn()))
    await expect(repo.getMyLastPayout()).resolves.toBeNull()
  })

  it('getMyLastPayout rethrows non-404 errors', async () => {
    const get = jest.fn(async () => {
      throw axiosError(500, { code: 'x' })
    })
    const repo = createMimironsGoldOMaticEbsRepository(mockClient(get, jest.fn()))
    await expect(repo.getMyLastPayout()).rejects.toMatchObject({ response: { status: 500 } })
  })

  it('getMyLastPayout returns payout body on 200', async () => {
    const payout: MimironsGoldOMaticPayoutDto = {
      id: 'p1',
      twitchUserId: 'u1',
      twitchDisplayName: 'Viewer',
      characterName: 'A',
      goldAmount: 100,
      enrollmentRequestId: 'e1',
      status: 'Pending',
      createdAt: '2026-01-01T00:00:00.000Z',
      isRewardSentAnnouncedToChat: false,
    }
    const get = jest.fn(async () => ({ data: payout }))
    const repo = createMimironsGoldOMaticEbsRepository(mockClient(get, jest.fn()))
    await expect(repo.getMyLastPayout()).resolves.toEqual(payout)
  })

  it('postClaim attaches API error body on structured 4xx', async () => {
    const post = jest.fn(async () => {
      throw axiosError(400, {
        code: 'invalid_character_name',
        message: 'Bad name',
        details: {},
      })
    })
    const repo = createMimironsGoldOMaticEbsRepository(mockClient(jest.fn(), post))

    await expect(repo.postClaim('x', 'y')).rejects.toMatchObject({
      message: 'Bad name',
      api: { code: 'invalid_character_name' },
    })
    expect(post).toHaveBeenCalledWith('/api/payouts/claim', {
      characterName: 'x',
      enrollmentRequestId: 'y',
    })
  })

  it('postClaim rethrows when response is not an API error body', async () => {
    const post = jest.fn(async () => {
      throw axiosError(400, 'plain text')
    })
    const repo = createMimironsGoldOMaticEbsRepository(mockClient(jest.fn(), post))
    await expect(repo.postClaim('a', 'b')).rejects.toMatchObject({
      response: { status: 400, data: 'plain text' },
    })
  })
})

import axios from 'axios'
import { useEffect } from 'react'
import { createMimironsGoldOMaticApiClient } from '../api/mgmApiClient'
import type { PayoutDto } from '../api/models'
import type { MimironsGoldOMaticApiErrorBody, MimironsGoldOMaticGiftQueueEntry } from '../mgmTypes'
import { useMimironsGoldOMaticPanelStore } from '../state/mgmPanelStore'
import {
  getMimironsGoldOMaticChannelId,
  getMimironsGoldOMaticExtensionJwt,
  getMimironsGoldOMaticUserId,
} from './useTwitchExtensionAuth'

const MIN_BACKOFF_MS = 3000
const MAX_BACKOFF_MS = 60_000

function nextBackoffMs(current: number): number {
  return Math.min(MAX_BACKOFF_MS, Math.max(MIN_BACKOFF_MS, current * 2))
}

function isApiErrorBody(x: unknown): x is MimironsGoldOMaticApiErrorBody {
  return (
    typeof x === 'object' &&
    x !== null &&
    'code' in x &&
    typeof (x as { code: unknown }).code === 'string'
  )
}

export function useMgmEbsPolling(ebsBaseUrl: string | undefined): void {
  const authView = useMimironsGoldOMaticPanelStore((s) => s.authView)
  const pollGeneration = useMimironsGoldOMaticPanelStore((s) => s.pollGeneration)
  const setPollSuccess = useMimironsGoldOMaticPanelStore((s) => s.setPollSuccess)
  const setPollFailure = useMimironsGoldOMaticPanelStore((s) => s.setPollFailure)
  const resetBackoff = useMimironsGoldOMaticPanelStore((s) => s.resetBackoff)

  useEffect(() => {
    if (authView !== 'ready' || !ebsBaseUrl) {
      return
    }

    const client = createMimironsGoldOMaticApiClient(ebsBaseUrl, getMimironsGoldOMaticExtensionJwt)

    let cancelled = false
    let timer: ReturnType<typeof setTimeout> | undefined
    let backoffLocal = MIN_BACKOFF_MS

    const schedule = (ms: number) => {
      if (cancelled) return
      timer = setTimeout(() => void tick(), ms)
    }

    const tick = async () => {
      if (cancelled) return

      try {
        const [roulette, poolMe, myLast] = await Promise.all([
          client.getRouletteState(),
          client.getPoolMe(),
          (async (): Promise<PayoutDto | null> => {
            try {
              return await client.getPayoutsMyLast()
            } catch (e) {
              if (axios.isAxiosError(e) && e.response?.status === 404) {
                return null
              }
              throw e
            }
          })(),
        ])
        const channelId = getMimironsGoldOMaticChannelId()
        const viewerId = getMimironsGoldOMaticUserId()
        let giftQueue: MimironsGoldOMaticGiftQueueEntry[] = []
        let myGift: MimironsGoldOMaticGiftQueueEntry | null = null
        if (channelId) {
          const token = getMimironsGoldOMaticExtensionJwt()
          const { data } = await axios.get<MimironsGoldOMaticGiftQueueEntry[]>(
            `${ebsBaseUrl.replace(/\/$/, '')}/api/streamers/${encodeURIComponent(channelId)}/gift-queue`,
            token ? { headers: { Authorization: `Bearer ${token}` } } : undefined,
          )
          giftQueue = data
          myGift = viewerId ? data.find((x) => x.viewerId === viewerId) ?? null : null
        }
        if (cancelled) return
        backoffLocal = MIN_BACKOFF_MS
        resetBackoff()
        setPollSuccess({ roulette, poolMe, myLast, giftQueue, myGift })

        const active =
          roulette.spinPhase === 'spinning' ||
          roulette.spinPhase === 'verification' ||
          roulette.spinPhase === 'collecting'
        schedule(active ? 2000 : 3000)
      } catch (e) {
        if (cancelled) return

        let status: number | undefined
        let body: MimironsGoldOMaticApiErrorBody | undefined
        if (axios.isAxiosError(e)) {
          status = e.response?.status
          const d = e.response?.data
          if (isApiErrorBody(d)) body = d
        }

        const retryable =
          status === 429 ||
          status === 503 ||
          status === undefined ||
          (status !== undefined && status >= 500)

        if (!retryable && status === 401) {
          backoffLocal = MAX_BACKOFF_MS
          setPollFailure({
            kind: 'api',
            status,
            body: body ?? { code: 'unauthorized', message: 'Session expired.', details: {} },
            nextBackoffMs: MAX_BACKOFF_MS,
          })
        } else if (!retryable) {
          backoffLocal = nextBackoffMs(backoffLocal)
          setPollFailure({
            kind: 'api',
            status,
            body,
            nextBackoffMs: backoffLocal,
          })
        } else {
          backoffLocal = nextBackoffMs(backoffLocal)
          setPollFailure({
            kind: status === undefined ? 'network' : 'api',
            status,
            body,
            nextBackoffMs: backoffLocal,
          })
        }

        schedule(backoffLocal)
      }
    }

    schedule(MIN_BACKOFF_MS)

    return () => {
      cancelled = true
      if (timer) clearTimeout(timer)
    }
  }, [authView, ebsBaseUrl, pollGeneration, resetBackoff, setPollFailure, setPollSuccess])
}

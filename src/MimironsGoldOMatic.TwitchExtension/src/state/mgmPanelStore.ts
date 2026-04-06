import { create } from 'zustand'
import type {
  MimironsGoldOMaticApiErrorBody,
} from '../mgmTypes'
import type { PayoutDto, PoolMeResponse, RouletteStateResponse } from '../api/models'
import type { MimironsGoldOMaticGiftQueueEntry } from '../mgmTypes'

export type MimironsGoldOMaticAuthView =
  | 'loading'
  | 'no_twitch_helper'
  | 'unauthenticated'
  | 'missing_ebs_url'
  | 'ready'

type PollErrorKind = 'network' | 'api'

export interface MimironsGoldOMaticPanelStore {
  authView: MimironsGoldOMaticAuthView
  setAuthView: (v: MimironsGoldOMaticAuthView) => void

  roulette: RouletteStateResponse | null
  poolMe: PoolMeResponse | null
  myLast: PayoutDto | null
  giftQueue: MimironsGoldOMaticGiftQueueEntry[]
  myGift: MimironsGoldOMaticGiftQueueEntry | null

  pollBackoffMs: number
  pollError: { kind: PollErrorKind; status?: number; body?: MimironsGoldOMaticApiErrorBody } | null
  lastPollWallClockMs: number | null

  setPollSuccess: (payload: {
    roulette: RouletteStateResponse
    poolMe: PoolMeResponse
    myLast: PayoutDto | null
    giftQueue: MimironsGoldOMaticGiftQueueEntry[]
    myGift: MimironsGoldOMaticGiftQueueEntry | null
  }) => void
  setPollFailure: (e: {
    kind: PollErrorKind
    status?: number
    body?: MimironsGoldOMaticApiErrorBody
    nextBackoffMs: number
  }) => void
  resetBackoff: () => void

  uiError: MimironsGoldOMaticApiErrorBody | null
  setUiError: (e: MimironsGoldOMaticApiErrorBody | null) => void

  claimBusy: boolean
  setClaimBusy: (b: boolean) => void

  /** Increment to restart the EBS polling loop after Retry (UI-101 / UI-105). */
  pollGeneration: number
  bumpPoll: () => void
}

export const useMimironsGoldOMaticPanelStore = create<MimironsGoldOMaticPanelStore>((set) => ({
  authView: 'loading',
  setAuthView: (authView) => set({ authView }),

  roulette: null,
  poolMe: null,
  myLast: null,
  giftQueue: [],
  myGift: null,

  pollBackoffMs: 3000,
  pollError: null,
  lastPollWallClockMs: null,

  setPollSuccess: ({ roulette, poolMe, myLast, giftQueue, myGift }) =>
    set({
      roulette,
      poolMe,
      myLast,
      giftQueue,
      myGift,
      pollBackoffMs: 3000,
      pollError: null,
      lastPollWallClockMs: Date.now(),
    }),

  setPollFailure: ({ kind, status, body, nextBackoffMs }) =>
    set({
      pollError: { kind, status, body },
      pollBackoffMs: nextBackoffMs,
    }),

  resetBackoff: () => set({ pollBackoffMs: 3000, pollError: null }),

  uiError: null,
  setUiError: (uiError) => set({ uiError }),

  claimBusy: false,
  setClaimBusy: (claimBusy) => set({ claimBusy }),

  pollGeneration: 0,
  bumpPoll: () =>
    set((s) => ({
      pollGeneration: s.pollGeneration + 1,
      pollError: null,
      pollBackoffMs: 3000,
    })),
}))

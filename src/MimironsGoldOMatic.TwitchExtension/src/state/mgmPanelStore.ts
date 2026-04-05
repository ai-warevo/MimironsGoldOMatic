import { create } from 'zustand'
import type {
  MimironsGoldOMaticApiErrorBody,
  MimironsGoldOMaticPoolMe,
  MimironsGoldOMaticPayoutDto,
  MimironsGoldOMaticRouletteState,
} from '../mgmTypes'

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

  roulette: MimironsGoldOMaticRouletteState | null
  poolMe: MimironsGoldOMaticPoolMe | null
  myLast: MimironsGoldOMaticPayoutDto | null

  pollBackoffMs: number
  pollError: { kind: PollErrorKind; status?: number; body?: MimironsGoldOMaticApiErrorBody } | null
  lastPollWallClockMs: number | null

  setPollSuccess: (payload: {
    roulette: MimironsGoldOMaticRouletteState
    poolMe: MimironsGoldOMaticPoolMe
    myLast: MimironsGoldOMaticPayoutDto | null
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

  pollBackoffMs: 3000,
  pollError: null,
  lastPollWallClockMs: null,

  setPollSuccess: ({ roulette, poolMe, myLast }) =>
    set({
      roulette,
      poolMe,
      myLast,
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

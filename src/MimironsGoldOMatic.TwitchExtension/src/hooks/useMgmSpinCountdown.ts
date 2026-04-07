import { useEffect, useMemo, useState } from 'react'
import type { RouletteStateResponse } from '../api/models'

export interface MimironsGoldOMaticCountdownTick {
  remainingSeconds: number
  labelMmSs: string
}

function formatMmSs(totalSeconds: number): string {
  const s = Math.max(0, totalSeconds)
  const m = Math.floor(s / 60)
  const r = s % 60
  return `${String(m).padStart(2, '0')}:${String(r).padStart(2, '0')}`
}

/**
 * Countdown to `nextSpinAt` using server skew from the last `roulette` payload (`serverNow`).
 */
export function useMgmSpinCountdown(
  roulette: RouletteStateResponse | null,
  lastPollWallClockMs: number | null,
): MimironsGoldOMaticCountdownTick {
  const [nowMs, setNowMs] = useState(() => Date.now())

  useEffect(() => {
    if (!roulette || lastPollWallClockMs === null) {
      return
    }
    const id = window.setInterval(() => setNowMs(Date.now()), 1000)
    return () => window.clearInterval(id)
  }, [roulette, lastPollWallClockMs])

  return useMemo(() => {
    if (!roulette || lastPollWallClockMs === null) {
      return { remainingSeconds: 0, labelMmSs: '00:00' }
    }

    const serverNowMs = Date.parse(roulette.serverNow)
    const nextMs = Date.parse(roulette.nextSpinAt)
    if (Number.isNaN(serverNowMs) || Number.isNaN(nextMs)) {
      return { remainingSeconds: 0, labelMmSs: '00:00' }
    }

    const skewMs = serverNowMs - lastPollWallClockMs
    const nowServerAligned = nowMs + skewMs
    const remainingMs = nextMs - nowServerAligned
    const remainingSeconds = Math.max(0, Math.ceil(remainingMs / 1000))
    return {
      remainingSeconds,
      labelMmSs: formatMmSs(remainingSeconds),
    }
  }, [roulette, lastPollWallClockMs, nowMs])
}

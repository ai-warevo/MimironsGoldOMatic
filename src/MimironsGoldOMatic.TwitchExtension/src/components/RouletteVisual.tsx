import { useEffect, useMemo, useState } from 'react'

const DECO_NAMES = ['Norinn', 'Kael', 'Mimir', 'Gearspan', 'Cogsworth', 'Tinker']

export interface MimironsGoldOMaticRouletteVisualProps {
  spinPhase: string
  poolParticipantCount: number
}

export function MimironsGoldOMaticRouletteVisual({
  spinPhase,
  poolParticipantCount,
}: MimironsGoldOMaticRouletteVisualProps) {
  const [tick, setTick] = useState(0)
  const reducedMotion =
    typeof window !== 'undefined' &&
    window.matchMedia?.('(prefers-reduced-motion: reduce)').matches

  useEffect(() => {
    if (spinPhase !== 'spinning' || reducedMotion) return
    const id = setInterval(() => setTick((t) => t + 1), 120)
    return () => clearInterval(id)
  }, [spinPhase, reducedMotion])

  const highlight = useMemo(() => {
    if (poolParticipantCount <= 0) return '—'
    const idx = tick % Math.max(1, Math.min(DECO_NAMES.length, poolParticipantCount || DECO_NAMES.length))
    return DECO_NAMES[idx] ?? '…'
  }, [poolParticipantCount, tick])

  const phaseLine = (() => {
    switch (spinPhase) {
      case 'collecting':
        return 'Collecting entries for this spin…'
      case 'spinning':
        return reducedMotion ? 'Spinning…' : 'Spinning…'
      case 'verification':
        return 'Checking if winner is online…'
      case 'completed':
        return 'Round complete.'
      default:
        return poolParticipantCount > 0
          ? 'Waiting for the next spin window.'
          : 'Pool is empty — be the first to enter via chat.'
    }
  })()

  return (
    <section className="mgm-roulette" aria-label="Roulette">
      <div className="mgm-roulette__banner">★ Roulette ★</div>
      <div
        className={
          spinPhase === 'spinning' && !reducedMotion
            ? 'mgm-roulette__wheel mgm-roulette__wheel--spin'
            : 'mgm-roulette__wheel'
        }
      >
        <span className="mgm-roulette__name">{highlight}</span>
      </div>
      <p className="mgm-roulette__phase">{phaseLine}</p>
      {spinPhase === 'verification' ? (
        <div className="mgm-progress mgm-progress--indeterminate" role="progressbar" aria-busy="true" />
      ) : null}
    </section>
  )
}

import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { MimironsGoldOMaticRouletteVisual } from './RouletteVisual'

function mockMatchMedia(matches: boolean) {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    configurable: true,
    value: vi.fn().mockImplementation((query: string) => ({
      matches,
      media: query,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    })),
  })
}

describe('MimironsGoldOMaticRouletteVisual', () => {
  it('shows collecting copy when spinPhase is collecting', () => {
    mockMatchMedia(false)
    render(
      <MimironsGoldOMaticRouletteVisual
        spinPhase="collecting"
        poolParticipantCount={3}
      />,
    )
    expect(
      screen.getByText('Collecting entries for this spin…'),
    ).toBeInTheDocument()
    expect(screen.getByLabelText('Roulette')).toBeInTheDocument()
  })

  it('shows empty-pool hint when idle and count is zero', () => {
    mockMatchMedia(false)
    render(
      <MimironsGoldOMaticRouletteVisual spinPhase="idle" poolParticipantCount={0} />,
    )
    expect(
      screen.getByText(
        'Pool is empty — be the first to enter via chat.',
      ),
    ).toBeInTheDocument()
  })
})

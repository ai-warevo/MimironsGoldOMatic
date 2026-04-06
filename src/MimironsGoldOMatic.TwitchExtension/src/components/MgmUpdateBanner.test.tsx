import { describe, expect, it, jest } from '@jest/globals'
import { fireEvent, render, screen } from '@testing-library/react'
import { MimironsGoldOMaticUpdateBanner } from './MgmUpdateBanner'

describe('MimironsGoldOMaticUpdateBanner', () => {
  it('renders version update copy and release notes link', () => {
    render(
      <MimironsGoldOMaticUpdateBanner
        currentVersion="1.0.0"
        latestVersion="1.1.0"
        releaseNotesUrl="https://example.com/release-notes"
        onReloadRequested={() => {}}
      />,
    )

    expect(
      screen.getByText(/Доступна новая версия расширения Mimiron's Gold-o-Matic/i),
    ).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Подробнее' })).toHaveAttribute(
      'href',
      'https://example.com/release-notes',
    )
  })

  it('calls reload callback when button is clicked', () => {
    const onReloadRequested = jest.fn()
    render(
      <MimironsGoldOMaticUpdateBanner
        currentVersion="1.0.0"
        latestVersion="1.1.0"
        onReloadRequested={onReloadRequested}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Перезагрузить' }))
    expect(onReloadRequested).toHaveBeenCalledTimes(1)
  })
})


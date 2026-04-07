import { useCallback, useEffect, useState } from 'react'
import axios from 'axios'
import { fetchMimironsGoldOMaticVersionInfo } from '../api/mgmVersionApi'
import { MIMIRONS_GOLD_O_MATIC_EXTENSION_VERSION } from '../config/mgmClientVersion'
import { createMimironsGoldOMaticApiClient } from '../api/mgmApiClient'
import { useMgmEbsPolling } from '../hooks/useMgmEbsPolling'
import { useMgmSpinCountdown } from '../hooks/useMgmSpinCountdown'
import { getMimironsGoldOMaticExtensionJwt, useTwitchExtensionAuth } from '../hooks/useTwitchExtensionAuth'
import { rewardSentChatAnnouncement } from '../rewardSentAnnouncement'
import { useMimironsGoldOMaticPanelStore } from '../state/mgmPanelStore'
import type { MimironsGoldOMaticApiErrorBody, MimironsGoldOMaticVersionInfoDto } from '../mgmTypes'
import { isMimironsGoldOMaticNewerVersion } from '../utils/mgmVersion'
import { MimironsGoldOMaticUpdateBanner } from './MgmUpdateBanner'
import { mapMimironsGoldOMaticApiErrorToUi } from './mapMgmApiErrorToUi'
import { MimironsGoldOMaticRouletteVisual } from './RouletteVisual'

const VERSION_CHECK_INTERVAL_MS = 60 * 60 * 1000

function payoutStatusLabel(status: string): string {
  switch (status) {
    case 'InProgress':
      return 'In progress'
    case 'Pending':
      return 'Pending'
    case 'Sent':
      return 'Sent'
    case 'Failed':
      return 'Failed'
    case 'Cancelled':
      return 'Cancelled'
    case 'Expired':
      return 'Expired'
    default:
      return status
  }
}

export function MimironsGoldOMaticViewerPanel() {
  useTwitchExtensionAuth()

  const ebsBaseUrl = import.meta.env.VITE_MGM_EBS_BASE_URL

  const authView = useMimironsGoldOMaticPanelStore((s) => s.authView)
  const roulette = useMimironsGoldOMaticPanelStore((s) => s.roulette)
  const poolMe = useMimironsGoldOMaticPanelStore((s) => s.poolMe)
  const myLast = useMimironsGoldOMaticPanelStore((s) => s.myLast)
  const pollError = useMimironsGoldOMaticPanelStore((s) => s.pollError)
  const myGift = useMimironsGoldOMaticPanelStore((s) => s.myGift)
  const lastPollWallClockMs = useMimironsGoldOMaticPanelStore((s) => s.lastPollWallClockMs)
  const uiError = useMimironsGoldOMaticPanelStore((s) => s.uiError)
  const setUiError = useMimironsGoldOMaticPanelStore((s) => s.setUiError)
  const claimBusy = useMimironsGoldOMaticPanelStore((s) => s.claimBusy)
  const setClaimBusy = useMimironsGoldOMaticPanelStore((s) => s.setClaimBusy)
  const bumpPoll = useMimironsGoldOMaticPanelStore((s) => s.bumpPoll)

  const canPoll = authView === 'ready' && Boolean(ebsBaseUrl)
  useMgmEbsPolling(canPoll ? ebsBaseUrl : undefined)

  const countdown = useMgmSpinCountdown(roulette, lastPollWallClockMs)

  const [rulesOpen, setRulesOpen] = useState(false)
  const [devName, setDevName] = useState('')
  const [remoteVersionInfo, setRemoteVersionInfo] = useState<MimironsGoldOMaticVersionInfoDto | null>(null)
  const showDevClaim = import.meta.env.DEV

  useEffect(() => {
    if (!ebsBaseUrl) return

    let cancelled = false

    const runVersionCheck = async () => {
      try {
        const versionInfo = await fetchMimironsGoldOMaticVersionInfo(ebsBaseUrl)
        const remoteVersion = versionInfo.version?.trim()
        if (!remoteVersion) {
          console.error('MGM update-check: missing version in /api/version payload.')
          return
        }

        const shouldShowBanner = isMimironsGoldOMaticNewerVersion(
          remoteVersion,
          MIMIRONS_GOLD_O_MATIC_EXTENSION_VERSION,
        )
        if (!cancelled) {
          setRemoteVersionInfo(shouldShowBanner ? versionInfo : null)
        }
      } catch (error) {
        // Silent failure by design: extension core UI must not be blocked by update checks.
        console.error('MGM update-check: failed to fetch /api/version.', error)
      }
    }

    void runVersionCheck()
    const intervalId = setInterval(() => {
      void runVersionCheck()
    }, VERSION_CHECK_INTERVAL_MS)

    return () => {
      cancelled = true
      clearInterval(intervalId)
    }
  }, [ebsBaseUrl])

  const onRetryAuth = useCallback(() => {
    window.location.reload()
  }, [])

  const onRetryPoll = useCallback(() => {
    bumpPoll()
  }, [bumpPoll])

  const onReloadRequested = useCallback(() => {
    window.location.reload()
  }, [])

  const onDevClaim = useCallback(async () => {
    if (!ebsBaseUrl) return
    const name = devName.trim()
    if (name.length < 2) {
      setUiError({
        code: 'invalid_character_name',
        message: 'Character name too short.',
        details: {},
      })
      return
    }
    setUiError(null)
    setClaimBusy(true)
    try {
      const client = createMimironsGoldOMaticApiClient(ebsBaseUrl, getMimironsGoldOMaticExtensionJwt)
      const id =
        typeof crypto !== 'undefined' && 'randomUUID' in crypto
          ? crypto.randomUUID()
          : `dev-${Date.now()}`
      await client.postPayoutsClaim({ characterName: name, enrollmentRequestId: id })
      bumpPoll()
      setDevName('')
    } catch (e) {
      const api: MimironsGoldOMaticApiErrorBody | null =
        axios.isAxiosError(e) && e.response?.data && typeof e.response.data === 'object'
          ? (e.response.data as MimironsGoldOMaticApiErrorBody)
          : null

      if (api?.code && typeof api.code === 'string') setUiError(api)
      else
        setUiError({
          code: 'network',
          message: e instanceof Error ? e.message : 'Request failed.',
          details: {},
        })
    } finally {
      setClaimBusy(false)
    }
  }, [bumpPoll, devName, ebsBaseUrl, setClaimBusy, setUiError])

  if (authView === 'loading') {
    return (
      <div className="mgm-panel" role="status" aria-live="polite">
        <h1 className="mgm-title">Mimiron&apos;s Gold-o-Matic</h1>
        <p className="mgm-status">Loading gnome gears…</p>
      </div>
    )
  }

  if (authView === 'no_twitch_helper') {
    return (
      <div className="mgm-panel">
        <h1 className="mgm-title">Mimiron&apos;s Gold-o-Matic</h1>
        <p className="mgm-status">
          Twitch Extension helper not found. Open this panel in Twitch Dev Rig or add the Twitch
          helper script for local testing.
        </p>
        <button type="button" className="mgm-btn" onClick={onRetryAuth}>
          Retry
        </button>
      </div>
    )
  }

  if (authView === 'unauthenticated') {
    return (
      <div className="mgm-panel">
        <h1 className="mgm-title">Mimiron&apos;s Gold-o-Matic</h1>
        <p className="mgm-status">Sign in to Twitch to use this panel on a live channel.</p>
        <button type="button" className="mgm-btn" onClick={onRetryAuth}>
          Retry
        </button>
        <p className="mgm-hint mgm-mt">
          Subscribe and type <kbd className="mgm-kbd">!twgold &lt;CharacterName&gt;</kbd> in stream
          chat to join the pool (enrollment is not done in this panel).
        </p>
      </div>
    )
  }

  if (!ebsBaseUrl) {
    return (
      <div className="mgm-panel">
        <h1 className="mgm-title">Mimiron&apos;s Gold-o-Matic</h1>
        <p className="mgm-status">EBS base URL is not configured.</p>
        <p className="mgm-muted">
          Set <code className="mgm-code">VITE_MGM_EBS_BASE_URL</code> in{' '}
          <code className="mgm-code">.env.local</code> to your Backend URL (Dev Rig / local EBS).
        </p>
      </div>
    )
  }

  const pollErrUi = pollError?.body
    ? mapMimironsGoldOMaticApiErrorToUi(
        pollError.body,
        "Can't reach gnomes",
        'EBS unreachable — try again later.',
      )
    : pollError
      ? mapMimironsGoldOMaticApiErrorToUi(
          null,
          "Can't reach gnomes",
          pollError.kind === 'network'
            ? 'Network error — check connection and EBS URL.'
            : 'EBS unreachable — try again later.',
        )
      : null

  const uiErrMapped = uiError
    ? mapMimironsGoldOMaticApiErrorToUi(uiError, 'Something went wrong', uiError.message)
    : null

  return (
    <div className="mgm-panel">
      <h1 className="mgm-title">Gold pool (viewer)</h1>

      {remoteVersionInfo ? (
        <MimironsGoldOMaticUpdateBanner
          currentVersion={MIMIRONS_GOLD_O_MATIC_EXTENSION_VERSION}
          latestVersion={remoteVersionInfo.version}
          releaseNotesUrl={remoteVersionInfo.releaseNotesUrl}
          onReloadRequested={onReloadRequested}
        />
      ) : null}

      {pollErrUi ? (
        <div className="mgm-alert mgm-alert--warn" role="alert">
          <strong>{pollErrUi.title}</strong>
          <p>{pollErrUi.body}</p>
          <button type="button" className="mgm-btn mgm-btn--small" onClick={onRetryPoll}>
            Retry
          </button>
        </div>
      ) : null}

      {uiErrMapped ? (
        <div className="mgm-alert mgm-alert--warn" role="alert">
          <strong>{uiErrMapped.title}</strong>
          <p>{uiErrMapped.body}</p>
          <button type="button" className="mgm-btn mgm-btn--small" onClick={() => setUiError(null)}>
            Dismiss
          </button>
        </div>
      ) : null}

      <section className="mgm-section" aria-label="How to join">
        <h2 className="mgm-h2">How to join</h2>
        <p>
          Subscribe, then type in <strong>stream chat</strong>:{' '}
          <kbd className="mgm-kbd">!twgold YourCharacterName</kbd> (prefix is case-insensitive).
        </p>
        <p className="mgm-muted">Same realm as the streamer (MVP).</p>
        <p className="mgm-muted">One character name slot per viewer; names must be unique in the pool.</p>
        <p>
          <strong>Pool:</strong>{' '}
          {roulette ? `${roulette.poolParticipantCount} in the draw` : '…'}
          {poolMe?.isEnrolled && poolMe.characterName ? (
            <span className="mgm-chip mgm-chip--ok">
              {' '}
              You&apos;re in as <strong>{poolMe.characterName}</strong>
            </span>
          ) : (
            <span className="mgm-muted"> — not enrolled yet</span>
          )}
        </p>
        <button
          type="button"
          className="mgm-linkish"
          aria-expanded={rulesOpen}
          onClick={() => setRulesOpen((o) => !o)}
        >
          {rulesOpen ? 'Hide rules ▲' : 'View rules ▼'}
        </button>
        {rulesOpen ? (
          <ul className="mgm-rules">
            <li>Spin every 5 minutes; no early spins.</li>
            <li>Non-winners stay in the pool.</li>
            <li>Winners leave the pool when delivery is marked Sent; you can re-enter via chat.</li>
            <li>Fixed 1,000g per winning payout.</li>
          </ul>
        ) : null}
      </section>

      {roulette ? (
        <>
          <MimironsGoldOMaticRouletteVisual
            spinPhase={roulette.spinPhase}
            poolParticipantCount={roulette.poolParticipantCount}
          />
          <div className="mgm-countdown" aria-label="Next spin countdown">
            <span className="mgm-countdown__label">Next spin in</span>
            <span className="mgm-countdown__value">{countdown.labelMmSs}</span>
            <div className="mgm-countdown__track">
              <div
                className="mgm-countdown__fill"
                style={{
                  width: `${Math.min(
                    100,
                    roulette.spinIntervalSeconds > 0
                      ? (countdown.remainingSeconds / roulette.spinIntervalSeconds) * 100
                      : 0,
                  )}%`,
                }}
              />
            </div>
          </div>
        </>
      ) : (
        <p className="mgm-muted" role="status">
          Syncing with server…
        </p>
      )}

      {myLast ? (
        <section className="mgm-winner" aria-label="Your last payout">
          {myLast.status === 'Pending' || myLast.status === 'InProgress' ? (
            <>
              <div className="mgm-winner__banner">You won!</div>
              <p>
                In <strong>WoW</strong>, reply to the streamer&apos;s whisper with{' '}
                <kbd className="mgm-kbd">!twgold</kbd> (exact text, case-insensitive) to confirm you
                accept the mail reward.
              </p>
            </>
          ) : myLast.status === 'Sent' ? (
            <div className="mgm-winner__banner mgm-winner__banner--calm">Delivery complete</div>
          ) : (
            <div className="mgm-winner__banner mgm-winner__banner--calm">Last payout</div>
          )}
          <p className="mgm-status-row">
            Status:{' '}
            <span className={`mgm-chip mgm-chip--${myLast.status.toLowerCase()}`}>
              {payoutStatusLabel(myLast.status)}
            </span>
          </p>
          {myLast.status === 'Sent' ? (
            <p className="mgm-sent">{rewardSentChatAnnouncement(myLast.characterName)}</p>
          ) : myLast.status === 'Pending' || myLast.status === 'InProgress' ? (
            <p className="mgm-muted">
              When Sent: gold is confirmed after the streamer sends mail and the addon logs
              confirmation. Stream chat may show a public announcement line.
            </p>
          ) : (
            <p className="mgm-muted">Character: {myLast.characterName}</p>
          )}
        </section>
      ) : null}

      {myGift ? (
        <section className="mgm-winner" aria-label="Your gift request">
          <div className="mgm-winner__banner mgm-winner__banner--calm">Gift queue</div>
          <p className="mgm-status-row">
            Status:{' '}
            <span className={`mgm-chip mgm-chip--${myGift.state.toLowerCase()}`}>{myGift.state}</span>
          </p>
          {myGift.state === 'Pending' ? (
            <p className="mgm-muted">
              You are #{myGift.queuePosition} in queue. Estimated wait: {myGift.estimatedWaitSeconds}s.
            </p>
          ) : myGift.state === 'SelectingItem' ? (
            <p className="mgm-muted">Gift selection in progress…</p>
          ) : myGift.state === 'WaitingConfirmation' ? (
            <p className="mgm-muted">
              Waiting for your in-game whisper reply <kbd className="mgm-kbd">!twgift</kbd>.
            </p>
          ) : myGift.state === 'Completed' ? (
            <p className="mgm-muted">Gift sent successfully.</p>
          ) : (
            <p className="mgm-muted">Gift request failed: {myGift.failureReason ?? 'unknown reason'}.</p>
          )}
        </section>
      ) : (
        <section className="mgm-section" aria-label="Gift command">
          <h2 className="mgm-h2">Gift command</h2>
          <p className="mgm-muted">
            Subscribers can request one gift with <kbd className="mgm-kbd">!twgift YourCharacterName</kbd>.
          </p>
        </section>
      )}

      {showDevClaim ? (
        <section className="mgm-dev" aria-label="Developer test enrollment">
          <h2 className="mgm-h2">Dev Rig: test enroll</h2>
          <p className="mgm-muted">
            Calls <code className="mgm-code">POST /api/payouts/claim</code> with the Extension JWT.
            EBS may require <code className="mgm-code">Mgm:DevSkipSubscriberCheck</code> for local
            testing.
          </p>
          <div className="mgm-dev__row">
            <input
              className="mgm-input"
              placeholder="CharacterName"
              value={devName}
              onChange={(ev) => setDevName(ev.target.value)}
              maxLength={12}
              autoComplete="off"
            />
            <button
              type="button"
              className="mgm-btn mgm-btn--small"
              disabled={claimBusy}
              onClick={() => void onDevClaim()}
            >
              {claimBusy ? '…' : 'Claim'}
            </button>
          </div>
        </section>
      ) : null}

      <p className="mgm-hint mgm-mt">
        Enrollment is via chat; this panel shows pool size, roulette timing, and your last winner
        payout status.
      </p>
    </div>
  )
}

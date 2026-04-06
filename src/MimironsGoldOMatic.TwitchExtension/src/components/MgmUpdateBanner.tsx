interface MimironsGoldOMaticUpdateBannerProps {
  currentVersion: string
  latestVersion: string
  releaseNotesUrl?: string
  onReloadRequested: () => void
}

export function MimironsGoldOMaticUpdateBanner({
  currentVersion,
  latestVersion,
  releaseNotesUrl,
  onReloadRequested,
}: MimironsGoldOMaticUpdateBannerProps) {
  return (
    <div className="mgm-update-banner" role="status" aria-live="polite">
      <p className="mgm-update-banner__text">
        Доступна новая версия расширения Mimiron&apos;s Gold-o-Matic: v{latestVersion} (у вас v
        {currentVersion}).
      </p>
      <div className="mgm-update-banner__actions">
        <button type="button" className="mgm-btn mgm-btn--small" onClick={onReloadRequested}>
          Перезагрузить
        </button>
        {releaseNotesUrl ? (
          <a
            className="mgm-update-banner__link"
            href={releaseNotesUrl}
            target="_blank"
            rel="noopener noreferrer"
          >
            Подробнее
          </a>
        ) : null}
      </div>
    </div>
  )
}


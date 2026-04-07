import { useEffect } from 'react'
import { useMimironsGoldOMaticPanelStore } from '../state/mgmPanelStore'

let mimironsGoldOMaticSharedToken: string | null = null
let mimironsGoldOMaticChannelId: string | null = null
let mimironsGoldOMaticUserId: string | null = null

export function getMimironsGoldOMaticExtensionJwt(): string | null {
  return mimironsGoldOMaticSharedToken
}

export function getMimironsGoldOMaticChannelId(): string | null {
  return mimironsGoldOMaticChannelId
}

export function getMimironsGoldOMaticUserId(): string | null {
  return mimironsGoldOMaticUserId
}

/** Registers `onAuthorized` once; updates Zustand auth + shared JWT for the EBS client. */
export function useTwitchExtensionAuth(): void {
  const setAuthView = useMimironsGoldOMaticPanelStore((s) => s.setAuthView)

  useEffect(() => {
    const ext = window.Twitch?.ext
    if (!ext?.onAuthorized) {
      setAuthView('no_twitch_helper')
      return
    }

    ext.onAuthorized((auth) => {
      mimironsGoldOMaticSharedToken = auth.token
      mimironsGoldOMaticChannelId = auth.channelId ?? null
      mimironsGoldOMaticUserId = auth.userId ?? null
      if (!auth.userId) {
        setAuthView('unauthenticated')
        return
      }
      setAuthView('ready')
    })
  }, [setAuthView])
}

/** Minimal `window.Twitch.ext` typing for panel + Dev Rig. */

export interface MimironsGoldOMaticTwitchAuthorizedPayload {
  channelId: string
  clientId: string
  userId: string
  helixToken?: string
  token: string
}

export type MimironsGoldOMaticTwitchExt = {
  onAuthorized: (cb: (auth: MimironsGoldOMaticTwitchAuthorizedPayload) => void) => void
  version?: string
}

declare global {
  interface Window {
    Twitch?: { ext: MimironsGoldOMaticTwitchExt }
  }
}

export {}

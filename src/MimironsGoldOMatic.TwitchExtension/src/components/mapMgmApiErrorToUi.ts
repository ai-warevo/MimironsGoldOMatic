import type { MimironsGoldOMaticApiErrorBody } from '../mgmTypes'

export interface MimironsGoldOMaticUiErrorLines {
  title: string
  body: string
}

export function mapMimironsGoldOMaticApiErrorToUi(
  err: MimironsGoldOMaticApiErrorBody | null | undefined,
  fallbackTitle: string,
  fallbackBody: string,
): MimironsGoldOMaticUiErrorLines {
  if (!err) {
    return { title: fallbackTitle, body: fallbackBody }
  }

  switch (err.code) {
    case 'invalid_character_name':
      return {
        title: 'Invalid name',
        body: 'Fix your character name and try again (2–12 letters, Latin or Cyrillic).',
      }
    case 'lifetime_cap_reached':
      return {
        title: 'Gnome vault full',
        body: "You've reached the 10,000g lifetime cap for this channel.",
      }
    case 'active_payout_exists':
      return {
        title: 'Finish current win',
        body: 'You already have an active payout; check status above.',
      }
    case 'unauthorized':
      return {
        title: 'Session expired',
        body: 'Refresh the panel or re-open the stream.',
      }
    case 'not_subscriber':
      return {
        title: 'Subscriber only',
        body: 'Pool enrollment requires an active subscription. For Dev Rig, enable Mgm:DevSkipSubscriberCheck on the EBS or use chat enrollment.',
      }
    case 'character_name_taken_in_pool':
      return {
        title: 'Name taken',
        body: 'That character name is already taken in the pool by another viewer.',
      }
    default:
      return { title: fallbackTitle, body: err.message || fallbackBody }
  }
}

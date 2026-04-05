/** Aligns with EBS JSON (camelCase) and `MimironsGoldOMatic.Shared`. */

export type MimironsGoldOMaticPayoutStatus =
  | 'Pending'
  | 'InProgress'
  | 'Sent'
  | 'Failed'
  | 'Cancelled'
  | 'Expired'

export type MimironsGoldOMaticSpinPhase =
  | 'idle'
  | 'collecting'
  | 'spinning'
  | 'verification'
  | 'completed'

export interface MimironsGoldOMaticRouletteState {
  nextSpinAt: string
  serverNow: string
  spinIntervalSeconds: number
  poolParticipantCount: number
  spinPhase: MimironsGoldOMaticSpinPhase
  currentSpinCycleId: string | null
}

export interface MimironsGoldOMaticPoolMe {
  isEnrolled: boolean
  characterName: string | null
}

export interface MimironsGoldOMaticPayoutDto {
  id: string
  twitchUserId: string
  twitchDisplayName: string
  characterName: string
  goldAmount: number
  enrollmentRequestId: string
  status: MimironsGoldOMaticPayoutStatus
  createdAt: string
  isRewardSentAnnouncedToChat: boolean
}

export interface MimironsGoldOMaticApiErrorBody {
  code: string
  message: string
  details: unknown
}

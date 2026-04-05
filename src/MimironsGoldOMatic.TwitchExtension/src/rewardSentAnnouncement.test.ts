import { describe, expect, it } from '@jest/globals'
import { rewardSentChatAnnouncement } from './rewardSentAnnouncement'

describe('rewardSentChatAnnouncement', () => {
  it('embeds the winner character name in the normative Russian line', () => {
    expect(rewardSentChatAnnouncement('Norinn')).toBe(
      'Награда отправлена персонажу Norinn на почту, проверяй ящик!',
    )
  })
})

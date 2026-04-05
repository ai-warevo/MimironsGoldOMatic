/**
 * Normative Russian copy for Twitch panel + broadcast chat when gold mail is confirmed (Sent).
 * Single source in repo docs: docs/overview/SPEC.md §11
 */
export function rewardSentChatAnnouncement(winnerCharacterName: string): string {
  return `Награда отправлена персонажу ${winnerCharacterName} на почту, проверяй ящик!`
}

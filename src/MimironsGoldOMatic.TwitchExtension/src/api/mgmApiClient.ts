import { MimironsGoldOMaticApiClient } from './client'

export function createMimironsGoldOMaticApiClient(
  baseUrl: string,
  tokenProvider: () => string | null,
): MimironsGoldOMaticApiClient {
  return new MimironsGoldOMaticApiClient(baseUrl, tokenProvider)
}


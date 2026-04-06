import { createMimironsGoldOMaticEbsClient } from './mgmEbsClient'
import { getMimironsGoldOMaticExtensionJwt } from '../hooks/useTwitchExtensionAuth'
import type { MimironsGoldOMaticVersionInfoDto } from '../mgmTypes'

export async function fetchMimironsGoldOMaticVersionInfo(
  ebsBaseUrl: string,
): Promise<MimironsGoldOMaticVersionInfoDto> {
  const client = createMimironsGoldOMaticEbsClient(ebsBaseUrl, getMimironsGoldOMaticExtensionJwt)
  const { data } = await client.get<MimironsGoldOMaticVersionInfoDto>('/api/version')
  return data
}


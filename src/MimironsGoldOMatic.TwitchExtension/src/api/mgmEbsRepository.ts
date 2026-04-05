import axios from 'axios'
import type { AxiosInstance } from 'axios'
import type {
  MimironsGoldOMaticApiErrorBody,
  MimironsGoldOMaticPoolMe,
  MimironsGoldOMaticPayoutDto,
  MimironsGoldOMaticRouletteState,
} from '../mgmTypes'

export interface MimironsGoldOMaticEbsRepository {
  getRouletteState(): Promise<MimironsGoldOMaticRouletteState>
  getPoolMe(): Promise<MimironsGoldOMaticPoolMe>
  getMyLastPayout(): Promise<MimironsGoldOMaticPayoutDto | null>
  postClaim(characterName: string, enrollmentRequestId: string): Promise<void>
}

function isApiErrorBody(x: unknown): x is MimironsGoldOMaticApiErrorBody {
  return (
    typeof x === 'object' &&
    x !== null &&
    'code' in x &&
    typeof (x as { code: unknown }).code === 'string'
  )
}

export function createMimironsGoldOMaticEbsRepository(
  client: AxiosInstance,
): MimironsGoldOMaticEbsRepository {
  return {
    async getRouletteState() {
      const { data } = await client.get<MimironsGoldOMaticRouletteState>('/api/roulette/state')
      return data
    },

    async getPoolMe() {
      const { data } = await client.get<MimironsGoldOMaticPoolMe>('/api/pool/me')
      return data
    },

    async getMyLastPayout() {
      try {
        const { data } = await client.get<MimironsGoldOMaticPayoutDto>('/api/payouts/my-last')
        return data
      } catch (e) {
        if (axios.isAxiosError(e) && e.response?.status === 404) {
          return null
        }
        throw e
      }
    },

    async postClaim(characterName: string, enrollmentRequestId: string) {
      try {
        await client.post('/api/payouts/claim', {
          characterName,
          enrollmentRequestId,
        })
      } catch (e) {
        if (axios.isAxiosError(e) && e.response?.data && isApiErrorBody(e.response.data)) {
          const err = new Error(e.response.data.message) as Error & { api?: MimironsGoldOMaticApiErrorBody }
          err.api = e.response.data
          throw err
        }
        throw e
      }
    },
  }
}

import MockAdapter from 'axios-mock-adapter'
import { afterEach, describe, expect, it } from '@jest/globals'
import { createMimironsGoldOMaticEbsClient } from './mgmEbsClient'

describe('createMimironsGoldOMaticEbsClient (integration)', () => {
  let mock: MockAdapter | undefined

  afterEach(() => {
    mock?.restore()
    mock = undefined
  })

  it('sends Authorization bearer from getToken and parses JSON', async () => {
    const client = createMimironsGoldOMaticEbsClient('http://ebs.test/mgm/', () => 'jwt-abc')
    mock = new MockAdapter(client)
    mock.onGet('/ping').reply((config) => {
      expect(config.headers?.Authorization).toBe('Bearer jwt-abc')
      return [200, { ok: true }]
    })

    const { data } = await client.get<{ ok: boolean }>('/ping')
    expect(data).toEqual({ ok: true })
  })

  it('omits Authorization when getToken returns null', async () => {
    const client = createMimironsGoldOMaticEbsClient('http://ebs.test', () => null)
    mock = new MockAdapter(client)
    mock.onGet('/status').reply((config) => {
      expect(config.headers?.Authorization).toBeUndefined()
      return [200, { status: 'up' }]
    })

    await client.get('/status')
  })
})

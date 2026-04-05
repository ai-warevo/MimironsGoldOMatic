import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { createMimironsGoldOMaticEbsClient } from './mgmEbsClient'
import { server } from '../test/msw/server'

describe('createMimironsGoldOMaticEbsClient (integration)', () => {
  it('sends Authorization bearer from getToken and parses JSON', async () => {
    let capturedAuth: string | undefined
    server.use(
      http.get('http://ebs.test/mgm/ping', ({ request }) => {
        capturedAuth = request.headers.get('Authorization') ?? undefined
        return HttpResponse.json({ ok: true })
      }),
    )

    const client = createMimironsGoldOMaticEbsClient('http://ebs.test/mgm/', () => 'jwt-abc')
    const { data } = await client.get<{ ok: boolean }>('/ping')

    expect(data).toEqual({ ok: true })
    expect(capturedAuth).toBe('Bearer jwt-abc')
  })

  it('omits Authorization when getToken returns null', async () => {
    let capturedAuth: string | null = 'unset'
    server.use(
      http.get('http://ebs.test/status', ({ request }) => {
        capturedAuth = request.headers.get('Authorization')
        return HttpResponse.json({ status: 'up' })
      }),
    )

    const client = createMimironsGoldOMaticEbsClient('http://ebs.test', () => null)
    await client.get('/status')

    expect(capturedAuth).toBeNull()
  })
})

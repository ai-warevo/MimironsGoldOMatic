import axios, { type AxiosInstance } from 'axios'

export function createMimironsGoldOMaticEbsClient(
  baseURL: string,
  getToken: () => string | null,
): AxiosInstance {
  const client = axios.create({
    baseURL: baseURL.replace(/\/$/, ''),
    timeout: 25_000,
  })

  client.interceptors.request.use((config) => {
    const t = getToken()
    if (t) {
      config.headers.Authorization = `Bearer ${t}`
    }
    return config
  })

  return client
}

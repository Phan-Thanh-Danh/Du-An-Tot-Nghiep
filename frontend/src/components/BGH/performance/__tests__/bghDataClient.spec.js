import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createBghDataClient } from '../bghDataClient'

function installStorage(user = { role: 'Principal', userId: 15, campusId: 1 }) {
  const storage = {
    getItem: vi.fn((key) => key === 'lms_auth_user' ? JSON.stringify(user) : null),
  }
  vi.stubGlobal('localStorage', storage)
  vi.stubGlobal('sessionStorage', storage)
}

describe('BGH data client', () => {
  beforeEach(() => installStorage())
  afterEach(() => vi.unstubAllGlobals())

  it('deduplicates concurrent requests and serves a fresh cache hit', async () => {
    const request = vi.fn(async () => ({ data: { value: 1 } }))
    const client = createBghDataClient(request)

    const [first, second] = await Promise.all([
      client.get('/api/bgh/dashboard'),
      client.get('/api/bgh/dashboard'),
    ])
    const warm = await client.get('/api/bgh/dashboard')

    expect(first).toEqual(second)
    expect(warm).toEqual(first)
    expect(request).toHaveBeenCalledTimes(1)
    expect(client.getMetrics()).toMatchObject({ deduped: 1, hits: 1, requests: 1 })
  })

  it('returns stale data immediately and revalidates in the background', async () => {
    let value = 0
    const request = vi.fn(async () => ({ data: { value: ++value } }))
    const client = createBghDataClient(request)

    const first = await client.get('/api/bgh/academic/gpa', { freshMs: 0, staleMs: 60_000 })
    await new Promise((resolve) => setTimeout(resolve, 2))
    const stale = await client.get('/api/bgh/academic/gpa', { freshMs: 0, staleMs: 60_000 })
    await vi.waitFor(() => expect(request).toHaveBeenCalledTimes(2))

    expect(first.data.value).toBe(1)
    expect(stale.data.value).toBe(1)
    expect(client.getMetrics().staleHits).toBe(1)
  })

  it('cancels an in-flight route scope', async () => {
    const request = vi.fn((_path, options) => new Promise((resolve, reject) => {
      options.signal.addEventListener('abort', () => reject(options.signal.reason || new Error('Aborted')), { once: true })
      setTimeout(() => resolve({ data: 'late' }), 500)
    }))
    const client = createBghDataClient(request)

    const pending = client.get('/api/bgh/users', { scope: '/bgh/users' })
    client.abortScope('/bgh/users')
    await expect(pending).rejects.toBeDefined()

    await expect(client.get('/api/bgh/users', { scope: '/bgh/users' })).resolves.toEqual({ data: 'late' })
    expect(request).toHaveBeenCalledTimes(1)
  })

  it('does not reuse cached business data after user or campus changes', async () => {
    let value = 0
    const request = vi.fn(async () => ({ data: ++value }))
    const client = createBghDataClient(request)

    expect(await client.get('/api/bgh/dashboard')).toEqual({ data: 1 })
    installStorage({ role: 'Principal', userId: 16, campusId: 2 })
    expect(await client.get('/api/bgh/dashboard')).toEqual({ data: 2 })
    expect(request).toHaveBeenCalledTimes(2)
  })

  it('records a prefetched response as used by the next foreground request', async () => {
    const request = vi.fn(async () => ({ data: 'prefetched' }))
    const client = createBghDataClient(request)

    await client.prefetch('/api/bgh/dashboard')
    await expect(client.get('/api/bgh/dashboard')).resolves.toEqual({ data: 'prefetched' })

    expect(request).toHaveBeenCalledTimes(1)
    expect(client.getMetrics()).toMatchObject({
      prefetchAttempts: 1,
      prefetchUsed: 1,
      prefetchUseRate: 1,
    })
  })

  it('skips prefetch when the browser requests data saving', async () => {
    const request = vi.fn(async () => ({ data: 'unused' }))
    const client = createBghDataClient(request)
    vi.stubGlobal('navigator', { connection: { saveData: true, effectiveType: '4g' } })

    await expect(client.prefetch('/api/bgh/dashboard')).resolves.toBeNull()

    expect(request).not.toHaveBeenCalled()
    expect(client.getMetrics()).toMatchObject({ prefetchAttempts: 1, prefetchSkipped: 1 })
  })
})

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

const { prefetch } = vi.hoisted(() => ({
  prefetch: vi.fn(() => Promise.resolve()),
}))

vi.mock('@/services/bghApi', () => ({ bghApi: { prefetch } }))

import {
  cancelBghRoutePrefetch,
  getBghPrefetchRoutes,
  prefetchBghRouteChunk,
  scheduleBghRoutePrefetch,
} from '../bghRoutePrefetch'

describe('BGH route prefetch', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    prefetch.mockClear()
  })

  afterEach(() => {
    cancelBghRoutePrefetch()
    vi.useRealTimers()
  })

  it('waits for hover intent before loading a route chunk and read-only APIs', async () => {
    const chunkLoader = vi.fn(() => Promise.resolve())

    scheduleBghRoutePrefetch('/bgh/academic/gpa', 180, chunkLoader)
    await vi.advanceTimersByTimeAsync(179)
    expect(chunkLoader).not.toHaveBeenCalled()
    expect(prefetch).not.toHaveBeenCalled()

    await vi.advanceTimersByTimeAsync(1)
    expect(chunkLoader).toHaveBeenCalledTimes(1)
    expect(prefetch).toHaveBeenCalledWith('/api/bgh/academic/gpa')
  })

  it('cancels intent before the threshold', async () => {
    const chunkLoader = vi.fn()
    scheduleBghRoutePrefetch('/bgh/dashboard', 180, chunkLoader)
    cancelBghRoutePrefetch()

    await vi.advanceTimersByTimeAsync(180)
    expect(chunkLoader).not.toHaveBeenCalled()
    expect(prefetch).not.toHaveBeenCalled()
  })

  it('keeps the API registry read-only and BGH-scoped', async () => {
    for (const route of getBghPrefetchRoutes()) {
      scheduleBghRoutePrefetch(route, 0)
      await vi.runOnlyPendingTimersAsync()
    }

    expect(prefetch).toHaveBeenCalled()
    for (const [path] of prefetch.mock.calls) {
      expect(path).toMatch(/^\/api\//)
      expect(path).not.toMatch(/approve|reject|delete|update|create/i)
    }
  })

  it('resolves and invokes the lazy route component', async () => {
    const component = vi.fn(() => Promise.resolve({ default: {} }))
    const router = {
      resolve: vi.fn(() => ({ matched: [{ components: { default: component } }] })),
    }

    await prefetchBghRouteChunk(router, '/bgh/dashboard')

    expect(router.resolve).toHaveBeenCalledWith('/bgh/dashboard')
    expect(component).toHaveBeenCalledTimes(1)
  })
})

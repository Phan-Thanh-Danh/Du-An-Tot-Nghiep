import { apiRequest } from '@/services/apiClient'

const DEFAULT_FRESH_MS = 30_000
const DEFAULT_STALE_MS = 120_000

function readAuthScope() {
  try {
    const raw = localStorage.getItem('lms_auth_user') || sessionStorage.getItem('lms_auth_user')
    const user = raw ? JSON.parse(raw) : null
    const userId = user?.id ?? user?.userId ?? user?.Id ?? user?.UserId ?? 0
    const campusId = user?.campusId ?? user?.CampusId ?? 0
    const role = user?.role ?? user?.Role ?? 'anonymous'
    return `${role}:${userId}:${campusId}`
  } catch {
    return 'anonymous:0:0'
  }
}

function abortError() {
  return new DOMException('Request was cancelled.', 'AbortError')
}

function raceWithSignal(promise, signal) {
  if (!signal) return promise
  if (signal.aborted) return Promise.reject(abortError())

  return new Promise((resolve, reject) => {
    const onAbort = () => reject(abortError())
    signal.addEventListener('abort', onAbort, { once: true })
    promise.then(resolve, reject).finally(() => signal.removeEventListener('abort', onAbort))
  })
}

export function createBghDataClient(request = apiRequest) {
  const cache = new Map()
  const inflight = new Map()
  const controllers = new Map()
  const queues = { critical: [], prefetch: [] }
  const active = { critical: 0, prefetch: 0 }
  const limits = { critical: 6, prefetch: 2 }
  const metrics = {
    hits: 0,
    staleHits: 0,
    misses: 0,
    deduped: 0,
    requests: 0,
    prefetchAttempts: 0,
    prefetchSkipped: 0,
    prefetchUsed: 0,
  }
  let authScope = readAuthScope()

  function syncScope() {
    const nextScope = readAuthScope()
    if (nextScope !== authScope) {
      clear()
      authScope = nextScope
    }
    return authScope
  }

  function schedule(task, priority) {
    const queueName = priority === 'prefetch' ? 'prefetch' : 'critical'
    return new Promise((resolve, reject) => {
      queues[queueName].push({ task, resolve, reject })
      drain(queueName)
    })
  }

  function drain(queueName) {
    while (active[queueName] < limits[queueName] && queues[queueName].length) {
      const item = queues[queueName].shift()
      active[queueName] += 1
      Promise.resolve()
        .then(item.task)
        .then(item.resolve, item.reject)
        .finally(() => {
          active[queueName] -= 1
          drain(queueName)
        })
    }
  }

  function notify(path, value) {
    if (typeof window !== 'undefined') {
      window.dispatchEvent(new CustomEvent('bgh:data-updated', { detail: { path, value } }))
    }
  }

  function fetchAndCache(key, path, options) {
    const existing = inflight.get(key)
    if (existing && !(options.priority === 'critical' && existing.priority === 'prefetch')) {
      metrics.deduped += 1
      return existing.task
    }

    const controller = new AbortController()
    const scope = options.scope || (options.priority === 'prefetch' ? 'prefetch' : (typeof window !== 'undefined' ? window.location.pathname : 'server'))
    const task = schedule(async () => {
      if (controller.signal.aborted) throw abortError()
      metrics.requests += 1
      const value = await request(path, { signal: controller.signal })
      cache.set(key, {
        value,
        updatedAt: Date.now(),
        freshMs: options.freshMs,
        staleMs: options.staleMs,
        prefetched: options.priority === 'prefetch',
      })
      notify(path, value)
      return value
    }, options.priority)

    inflight.set(key, { task, priority: options.priority })
    controllers.set(key, { controller, scope })
    task.finally(() => {
      if (inflight.get(key)?.task === task) inflight.delete(key)
      if (controllers.get(key)?.controller === controller) controllers.delete(key)
    }).catch(() => {})
    return task
  }

  function get(path, options = {}) {
    const normalizedOptions = {
      freshMs: options.freshMs ?? DEFAULT_FRESH_MS,
      staleMs: options.staleMs ?? DEFAULT_STALE_MS,
      priority: options.priority === 'prefetch' ? 'prefetch' : 'critical',
      scope: options.scope,
    }
    const key = `${syncScope()}:${path}`
    const entry = cache.get(key)
    const age = entry ? Date.now() - entry.updatedAt : Number.POSITIVE_INFINITY

    if (!options.force && entry && age <= entry.freshMs) {
      metrics.hits += 1
      if (entry.prefetched && normalizedOptions.priority !== 'prefetch') {
        metrics.prefetchUsed += 1
        entry.prefetched = false
      }
      return raceWithSignal(Promise.resolve(entry.value), options.signal)
    }

    if (!options.force && entry && age <= entry.staleMs) {
      metrics.staleHits += 1
      fetchAndCache(key, path, normalizedOptions).catch(() => {})
      return raceWithSignal(Promise.resolve(entry.value), options.signal)
    }

    metrics.misses += 1
    const p = fetchAndCache(key, path, normalizedOptions)
    if (normalizedOptions.priority === 'critical') {
      return raceWithSignal(p, options.signal).catch((err) => {
        if (err?.name === 'AbortError' || err?.message?.includes('cancelled') || err?.message?.includes('aborted')) {
          cache.delete(key)
          return request(path).then((val) => {
            cache.set(key, {
              value: val,
              updatedAt: Date.now(),
              freshMs: normalizedOptions.freshMs,
              staleMs: normalizedOptions.staleMs,
              prefetched: false,
            })
            return val
          }).catch(() => null)
        }
        throw err
      })
    }
    return raceWithSignal(p, options.signal)
  }

  function prefetch(path, options = {}) {
    metrics.prefetchAttempts += 1
    if (typeof navigator !== 'undefined') {
      const connection = navigator.connection
      if (connection?.saveData || ['slow-2g', '2g'].includes(connection?.effectiveType)) {
        metrics.prefetchSkipped += 1
        return Promise.resolve(null)
      }
    }
    return get(path, { ...options, priority: 'prefetch', scope: 'prefetch' }).catch(() => null)
  }

  function invalidate(pathPrefix = '') {
    const scopedPrefix = `${syncScope()}:${pathPrefix}`
    for (const key of cache.keys()) {
      if (key.startsWith(scopedPrefix)) cache.delete(key)
    }
  }

  function abortScope(scope) {
    for (const [key, entry] of controllers) {
      if (entry.scope === scope) {
        entry.controller.abort()
        controllers.delete(key)
      }
    }
  }

  function clear() {
    for (const entry of controllers.values()) entry.controller.abort()
    for (const queue of Object.values(queues)) {
      while (queue.length) queue.shift().reject(abortError())
    }
    cache.clear()
    inflight.clear()
    controllers.clear()
  }

  function getMetrics() {
    return {
      ...metrics,
      cachedEntries: cache.size,
      inflightRequests: inflight.size,
      criticalQueue: queues.critical.length,
      prefetchQueue: queues.prefetch.length,
      prefetchUseRate: metrics.prefetchAttempts > 0
        ? Number((metrics.prefetchUsed / metrics.prefetchAttempts).toFixed(3))
        : 0,
    }
  }

  return { get, prefetch, invalidate, abortScope, clear, getMetrics }
}

export const bghDataClient = createBghDataClient()

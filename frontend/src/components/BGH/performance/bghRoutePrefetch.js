import { bghApi } from '@/services/bghApi'

const routeDataRegistry = {
  '/bgh/dashboard': [
    '/api/bgh/dashboard',
    '/api/bgh/evaluations/ranking',
    '/api/bgh/academic/pass-fail/filters',
    '/api/bgh/academic/pass-fail',
  ],
  '/bgh/organizations': ['/api/organizations', '/api/organizations/tree'],
  '/bgh/users': ['/api/bgh/users?pageIndex=1&pageSize=15', '/api/bgh/rbac/roles', '/api/organizations'],
  '/bgh/academic-programs': ['/api/bgh/master-data/training-programs'],
  '/bgh/curriculum': ['/api/bgh/master-data/training-programs', '/api/bgh/master-data/subjects'],
  '/bgh/academic-terms': ['/api/bgh/master-data/academic-terms', '/api/bgh/master-data/cohorts'],
  '/bgh/academic/overview': ['/api/bgh/academic/overview'],
  '/bgh/academic/gpa': ['/api/bgh/academic/gpa'],
  '/bgh/academic/at-risk': ['/api/bgh/academic/at-risk?pageIndex=1&pageSize=20'],
  '/bgh/academic/reports': ['/api/bgh/academic/reports'],
  '/bgh/academic/pass-fail': ['/api/bgh/academic/pass-fail/filters', '/api/bgh/academic/pass-fail'],
  '/bgh/evaluations': ['/api/bgh/evaluations', '/api/bgh/evaluations/ranking'],
  '/bgh/evaluations/ranking': ['/api/bgh/evaluations/ranking'],
  '/bgh/evaluations/overview': ['/api/bgh/evaluations/overview'],
  '/bgh/evaluations/ai-analysis': ['/api/bgh/evaluations/ai-analysis'],
  '/bgh/facilities': ['/api/bgh/master-data/buildings', '/api/bgh/master-data/floors', '/api/bgh/master-data/rooms'],
  '/bgh/audit-logs': ['/api/bgh/audit-logs?pageIndex=1&pageSize=15'],
}

let intentTimer = null

export function prefetchBghRouteChunk(router, route) {
  const resolved = router.resolve(route)
  const component = resolved.matched.at(-1)?.components?.default
  if (typeof component !== 'function') return Promise.resolve(null)

  return Promise.resolve(component()).catch(() => null)
}

export function scheduleBghRoutePrefetch(route, delayMs = 180, chunkLoader = null) {
  cancelBghRoutePrefetch()
  const paths = routeDataRegistry[route]
  if (!paths?.length) return

  intentTimer = window.setTimeout(() => {
    intentTimer = null
    chunkLoader?.()
    paths.forEach((path) => bghApi.prefetch(path))
  }, delayMs)
}

export function cancelBghRoutePrefetch() {
  if (intentTimer !== null) {
    window.clearTimeout(intentTimer)
    intentTimer = null
  }
}

export function getBghPrefetchRoutes() {
  return Object.keys(routeDataRegistry)
}

import { normalizeRole } from './roleRoutes'

export function isSafeInternalPath(path) {
  if (typeof path !== 'string') return false
  if (!path.startsWith('/')) return false
  if (path.startsWith('//')) return false
  if (path.includes('://')) return false
  return true
}

export function getRequiredRoleFromMatchedRoutes(matchedRoutes) {
  for (let index = matchedRoutes.length - 1; index >= 0; index -= 1) {
    const role = matchedRoutes[index]?.meta?.role
    if (role) return role
  }
  return null
}

export function routeRequiresAuthentication(matchedRoutes) {
  return matchedRoutes.some((record) => record.meta?.requiresAuth)
}

export function resolveSafeRoleRedirect({ router, redirectPath, actualRole }) {
  if (!isSafeInternalPath(redirectPath)) return null

  let resolved
  try {
    resolved = router.resolve(redirectPath)
  } catch {
    return null
  }

  const hasMatched = resolved.matched && resolved.matched.length > 0
  const isNotFound =
    resolved.name === 'not-found' ||
    resolved.name === undefined ||
    resolved.name === null

  if (!hasMatched || isNotFound) return null

  if (!routeRequiresAuthentication(resolved.matched)) return null

  const requiredRole = getRequiredRoleFromMatchedRoutes(resolved.matched)
  if (!requiredRole) return null

  const requiredRoles = Array.isArray(requiredRole) ? requiredRole : [requiredRole]
  if (!requiredRoles.some((item) => normalizeRole(actualRole) === normalizeRole(item))) {
    return null
  }

  return redirectPath
}

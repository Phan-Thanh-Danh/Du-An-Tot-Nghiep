export const ROLE_HOME_ROUTES = Object.freeze({
  Student: '/student/dashboard',
  Teacher: '/teacher/dashboard',
  AcademicStaff: '/staff/dashboard',
  Principal: '/bgh/dashboard',
  Parent: '/parent/dashboard',
  HoiDongQuanLyNoiDung: '/content-council/subjects',
  SuperAdmin: '/super-admin/dashboard',
})

export function normalizeRole(role) {
  return String(role || '').trim().toLowerCase()
}

export function getHomeRouteByRole(role) {
  const normalized = normalizeRole(role)

  const entry = Object.entries(ROLE_HOME_ROUTES).find(
    ([key]) => normalizeRole(key) === normalized,
  )

  return entry?.[1] || null
}

export function isKnownRole(role) {
  return Boolean(getHomeRouteByRole(role))
}

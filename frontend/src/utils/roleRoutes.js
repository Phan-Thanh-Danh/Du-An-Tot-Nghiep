export const ROLE_HOME_ROUTES = Object.freeze({
  Student: '/student/dashboard',
  Teacher: '/teacher/dashboard',
  Lecturer: '/teacher/dashboard',
  AcademicStaff: '/staff/dashboard',
  TrainingDepartment: '/staff/dashboard',
  Faculty: '/staff/dashboard',
  AcademicDepartment: '/staff/dashboard',
  Principal: '/bgh/dashboard',
  Parent: '/parent/dashboard',
  HoiDongQuanLyNoiDung: '/content-council/subjects',
  Admin: '/super-admin/dashboard',
  SuperAdmin: '/super-admin/dashboard',
})

export function normalizeRole(role) {
  const normalized = String(role || '').trim().toLowerCase()
  const aliases = {
    lecturer: 'teacher',
    trainingdepartment: 'academicstaff',
    faculty: 'academicstaff',
    academicdepartment: 'academicstaff',
  }

  return aliases[normalized] || normalized
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

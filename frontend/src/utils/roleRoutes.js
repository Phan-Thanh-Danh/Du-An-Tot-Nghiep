export const ROLE_HOME_ROUTES = Object.freeze({
  Student: '/student/dashboard',

  Teacher: '/teacher/dashboard',
  Lecturer: '/teacher/dashboard',

  AcademicStaff: '/staff/dashboard',
  TrainingDepartment: '/staff/dashboard',
  Faculty: '/staff/dashboard',
  AcademicDepartment: '/staff/dashboard',

  Principal: '/bgh/dashboard',
  Chairman: '/bgh/dashboard',

  Parent: '/parent/dashboard',

  Admin: '/super-admin/dashboard',
  SuperAdmin: '/super-admin/dashboard',
  CampusAdmin: '/super-admin/dashboard',
  SubCampusAdmin: '/super-admin/dashboard',

  HoiDongQuanLyNoiDung: '/content-council/subjects',

  FinanceAdmin: '/super-admin/finance/payments',
  CampusAccountant: '/super-admin/finance/payments',
  CampusChiefAccountant: '/super-admin/finance/payments',
})

export function normalizeRole(role) {
  const normalized = String(role || '').trim().toLowerCase()
  const aliases = {
    lecturer: 'teacher',
    trainingdepartment: 'academicstaff',
    faculty: 'academicstaff',
    academicdepartment: 'academicstaff',
    campusadministrator: 'campusadmin',
    subcampusadministrator: 'subcampusadmin',
    contentcouncil: 'hoidongquanlynoidung',
    financeadministrator: 'financeadmin',
    accountant: 'campusaccountant',
    chiefaccountant: 'campuschiefaccountant',
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

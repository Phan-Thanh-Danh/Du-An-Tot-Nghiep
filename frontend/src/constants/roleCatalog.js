export const ROLE_CATALOG = Object.freeze([
  {
    role: 'Admin',
    dbCode: 'quan_tri',
    portal: 'super-admin',
    label: 'Quản trị viên',
    homeRoute: '/super-admin/dashboard',
    group: 'staff',
    enabled: true,
    permissions: ['users', 'roles', 'training', 'finance', 'audit', 'system'],
  },
  {
    role: 'Student',
    dbCode: 'hoc_sinh',
    portal: 'student',
    label: 'Sinh viên',
    homeRoute: '/student/dashboard',
    group: 'primary',
    enabled: true,
    permissions: [
      'courses', 'assignments', 'exams', 'grades',
      'schedule', 'attendance', 'registrations',
      'tuition', 'requests', 'discipline',
    ],
  },
  {
    role: 'Teacher',
    dbCode: 'giao_vien',
    portal: 'teacher',
    label: 'Giảng viên',
    homeRoute: '/teacher/dashboard',
    group: 'primary',
    enabled: true,
    permissions: ['classes', 'attendance', 'grading', 'exams', 'requests'],
  },
  {
    role: 'Parent',
    dbCode: 'phu_huynh',
    portal: 'parent',
    label: 'Phụ huynh',
    homeRoute: '/parent/dashboard',
    group: 'primary',
    enabled: true,
    permissions: [],
  },
  {
    role: 'AcademicStaff',
    dbCode: 'nhan_vien',
    portal: 'staff',
    label: 'Giáo vụ',
    homeRoute: '/staff/dashboard',
    group: 'staff',
    enabled: true,
    permissions: ['schedule', 'assignments', 'requests', 'notices', 'discipline'],
  },
  {
    role: 'Principal',
    dbCode: 'hieu_truong',
    portal: 'bgh',
    label: 'Ban giám hiệu',
    homeRoute: '/bgh/dashboard',
    group: 'staff',
    enabled: true,
    permissions: ['organizations', 'users', 'academic-programs', 'evaluations', 'facilities', 'audit'],
  },
  {
    role: 'Chairman',
    dbCode: 'chu_tich',
    portal: 'bgh',
    label: 'Chủ tịch HĐQT',
    homeRoute: '/bgh/dashboard',
    group: 'staff',
    enabled: true,
    permissions: ['organizations', 'users', 'academic-programs', 'evaluations', 'audit'],
  },
  {
    role: 'SuperAdmin',
    dbCode: 'sieu_quan_tri',
    portal: 'super-admin',
    label: 'Quản trị hệ thống',
    homeRoute: '/super-admin/dashboard',
    group: 'staff',
    enabled: true,
    allowedRoles: ['SuperAdmin', 'Admin'],
    permissions: ['organizations', 'users', 'roles', 'training', 'finance', 'audit', 'system'],
  },
  {
    role: 'CampusAdmin',
    dbCode: 'quan_tri_co_so',
    portal: 'super-admin',
    label: 'Quản trị cơ sở',
    homeRoute: '/super-admin/dashboard',
    group: 'staff',
    enabled: true,
    permissions: ['users', 'training', 'finance', 'audit'],
  },
  {
    role: 'SubCampusAdmin',
    dbCode: 'quan_tri_co_so_con',
    portal: 'super-admin',
    label: 'Quản trị cơ sở con',
    homeRoute: '/super-admin/dashboard',
    group: 'staff',
    enabled: true,
    permissions: ['users', 'training'],
  },
  {
    role: 'FinanceAdmin',
    dbCode: 'admin_tai_chinh',
    portal: 'super-admin',
    label: 'Quản trị tài chính',
    homeRoute: '/super-admin/finance/payments',
    group: 'staff',
    enabled: true,
    permissions: ['finance'],
  },
  {
    role: 'CampusAccountant',
    dbCode: 'ke_toan_co_so',
    portal: 'super-admin',
    label: 'Kế toán cơ sở',
    homeRoute: '/super-admin/finance/payments',
    group: 'staff',
    enabled: true,
    permissions: ['finance'],
  },
  {
    role: 'CampusChiefAccountant',
    dbCode: 'ke_toan_truong_co_so',
    portal: 'super-admin',
    label: 'Kế toán trưởng cơ sở',
    homeRoute: '/super-admin/finance/payments',
    group: 'staff',
    enabled: true,
    permissions: ['finance'],
  },
  {
    role: 'HoiDongQuanLyNoiDung',
    dbCode: 'hoidong_quanly_noidung',
    portal: 'content-council',
    label: 'Hội đồng nội dung',
    homeRoute: '/content-council/subjects',
    group: 'staff',
    enabled: true,
    permissions: [],
  },
])

const ROLE_ALIASES = {
  lecturer: 'Teacher',
  trainingdepartment: 'AcademicStaff',
  faculty: 'AcademicStaff',
  academicdepartment: 'AcademicStaff',
  campusadmin: 'CampusAdmin',
  subcampusadmin: 'SubCampusAdmin',
  chairman: 'Chairman',
  financeadmin: 'FinanceAdmin',
  campusaccountant: 'CampusAccountant',
  campuschiefaccountant: 'CampusChiefAccountant',
  bgh: 'Principal',
  staff: 'AcademicStaff',
  contentcouncil: 'HoiDongQuanLyNoiDung',
}

export function normalizeRole(role) {
  const normalized = String(role || '').trim().toLowerCase()
  const alias = ROLE_ALIASES[normalized]
  const target = alias ? alias.toLowerCase() : normalized
  return ROLE_CATALOG.find(r => r.role.toLowerCase() === target)?.role || (alias || normalized)
}

export function getRoleConfig(role) {
  if (!role) return null
  const normalized = normalizeRole(role).toLowerCase()
  return (
    ROLE_CATALOG.find(
      (entry) =>
        entry.role.toLowerCase() === normalized ||
        (entry.allowedRoles || []).some((ar) => ar.toLowerCase() === normalized),
    ) || null
  )
}

export function getHomeRouteByRole(role) {
  return getRoleConfig(role)?.homeRoute || null
}

export function isKnownRole(role) {
  return Boolean(getRoleConfig(role))
}

export function isRoleEnabled(role) {
  return getRoleConfig(role)?.enabled === true
}

export function getDisabledReason(role) {
  return getRoleConfig(role)?.disabledReason || null
}

export function getRoleLabel(role) {
  return getRoleConfig(role)?.label || role
}

export function getPortalByRole(role) {
  return getRoleConfig(role)?.portal || null
}

export const ROLE_HOME_ROUTES = Object.freeze(
  Object.fromEntries(ROLE_CATALOG.map((entry) => [entry.role, entry.homeRoute])),
)

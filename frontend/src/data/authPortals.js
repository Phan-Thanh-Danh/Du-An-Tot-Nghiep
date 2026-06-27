export const SHOW_DEMO_ACCOUNTS =
  import.meta.env.DEV ||
  import.meta.env.VITE_ENABLE_DEMO_ACCOUNTS === 'true'

export const AUTH_PORTALS = Object.freeze({
  student: {
    slug: 'student',
    backendRole: 'Student',
    label: 'Sinh viên',
    shortLabel: 'SV',
    eyebrow: 'Không gian học tập',
    headline: 'Không gian học tập của bạn',
    description:
      'Truy cập khóa học, bài tập, lịch học và theo dõi kết quả học tập cá nhân.',
    accent: 'blue',
    icon: 'GraduationCap',
    featured: true,
    features: ['Khóa học', 'Bài tập', 'Kết quả'],
    featureDetails: [
      { label: 'Khóa học', icon: 'BookOpen', description: 'Theo dõi thời khóa biểu và phòng học' },
      { label: 'Bài tập', icon: 'ClipboardList', description: 'Quản lý hạn nộp và tiến độ' },
      { label: 'Kết quả', icon: 'Award', description: 'Nhận cập nhật từ nhà trường' },
    ],
    audience: 'Dành cho sinh viên đang theo học',
    group: 'primary',
    homeRoute: '/student/dashboard',
    enabled: true,
    demoUsername: 'student',
  },
  teacher: {
    slug: 'teacher',
    backendRole: 'Teacher',
    label: 'Giảng viên',
    shortLabel: 'GV',
    eyebrow: 'Không gian giảng dạy',
    headline: 'Tổ chức giảng dạy hiệu quả',
    description:
      'Quản lý lớp học, điểm danh, chấm điểm và theo dõi tiến độ sinh viên.',
    accent: 'indigo',
    icon: 'Presentation',
    features: ['Lớp học', 'Điểm danh', 'Chấm điểm'],
    featureDetails: [
      { label: 'Lớp học', icon: 'Presentation' },
      { label: 'Điểm danh', icon: 'ClipboardList' },
      { label: 'Chấm điểm', icon: 'Award' },
    ],
    audience: 'Dành cho giảng viên và giám thị',
    group: 'primary',
    homeRoute: '/teacher/dashboard',
    enabled: true,
    demoUsername: 'teacher',
  },
  parent: {
    slug: 'parent',
    backendRole: 'Parent',
    label: 'Phụ huynh',
    shortLabel: 'PH',
    eyebrow: 'Đồng hành học tập',
    headline: 'Theo dõi hành trình học tập của con',
    description:
      'Theo dõi kết quả, chuyên cần, lịch học và thông báo từ nhà trường.',
    accent: 'cyan',
    icon: 'HeartHandshake',
    features: ['Kết quả', 'Chuyên cần', 'Học phí'],
    featureDetails: [
      { label: 'Kết quả', icon: 'BarChart' },
      { label: 'Chuyên cần', icon: 'ClipboardList' },
      { label: 'Học phí', icon: 'Wallet' },
    ],
    audience: 'Dành cho phụ huynh sinh viên',
    group: 'primary',
    homeRoute: '/parent/dashboard',
    enabled: true,
    demoUsername: 'parent',
  },
  staff: {
    slug: 'staff',
    backendRole: 'AcademicStaff',
    label: 'Giáo vụ',
    shortLabel: 'GVụ',
    eyebrow: 'Vận hành học vụ',
    headline: 'Điều phối đào tạo chính xác và liền mạch',
    description:
      'Quản lý thời khóa biểu, đăng ký môn, lớp học phần, đơn từ và thông báo học vụ.',
    accent: 'teal',
    icon: 'ClipboardList',
    features: ['Thời khóa biểu', 'Đăng ký môn', 'Đơn từ'],
    featureDetails: [
      { label: 'Thời khóa biểu', icon: 'Calendar' },
      { label: 'Đăng ký môn', icon: 'ClipboardList' },
      { label: 'Đơn từ', icon: 'FileText' },
    ],
    audience: 'Dành cho cán bộ giáo vụ',
    group: 'staff',
    homeRoute: '/staff/dashboard',
    enabled: true,
    demoUsername: 'staff',
  },
  bgh: {
    slug: 'bgh',
    backendRole: 'Principal',
    label: 'Ban giám hiệu',
    shortLabel: 'BGH',
    eyebrow: 'Quản trị chiến lược',
    headline: 'Tổng quan chất lượng đào tạo',
    description:
      'Theo dõi báo cáo, phê duyệt, chất lượng giảng dạy và cảnh báo rủi ro toàn hệ thống.',
    accent: 'navy',
    icon: 'Landmark',
    features: ['Báo cáo', 'Phê duyệt', 'Rủi ro'],
    featureDetails: [
      { label: 'Báo cáo', icon: 'BarChart' },
      { label: 'Phê duyệt', icon: 'CheckCheck' },
      { label: 'Rủi ro', icon: 'AlertTriangle' },
    ],
    audience: 'Dành cho ban giám hiệu',
    group: 'staff',
    homeRoute: '/bgh/dashboard',
    enabled: true,
    demoUsername: 'bgh',
  },
  'content-board': {
    slug: 'content-board',
    backendRole: 'ContentBoard',
    label: 'Hội đồng nội dung',
    shortLabel: 'HĐND',
    eyebrow: 'Quản trị học thuật',
    headline: 'Kiểm duyệt nội dung và chất lượng học thuật',
    description:
      'Quản lý học liệu, ngân hàng câu hỏi, đề thi và quy trình phê duyệt nội dung.',
    accent: 'violet',
    icon: 'BookOpenCheck',
    features: ['Học liệu', 'Ngân hàng câu hỏi', 'Duyệt nội dung'],
    audience: 'Dành cho hội đồng nội dung',
    group: 'staff',
    homeRoute: '/content-board/dashboard',
    enabled: false,
    demoUsername: undefined,
  },
  'super-admin': {
    slug: 'super-admin',
    backendRole: 'SuperAdmin',
    label: 'Quản trị hệ thống',
    shortLabel: 'Admin',
    eyebrow: 'Quản trị kỹ thuật',
    headline: 'Cấu hình và bảo vệ toàn bộ hệ thống',
    description:
      'Quản lý tổ chức, tài khoản, phân quyền, audit và cấu hình vận hành EduLMS.',
    accent: 'slate',
    icon: 'ShieldCheck',
    features: ['Tài khoản', 'Phân quyền', 'Audit'],
    featureDetails: [
      { label: 'Tài khoản', icon: 'Users' },
      { label: 'Phân quyền', icon: 'ShieldCheck' },
      { label: 'Audit', icon: 'FileSearch' },
    ],
    audience: 'Dành cho quản trị viên hệ thống',
    group: 'staff',
    homeRoute: '/super-admin/dashboard',
    enabled: true,
    demoUsername: 'admin',
  },
})

export function getPortalConfig(slug) {
  return AUTH_PORTALS[slug] || null
}

export function getPortalByRole(role) {
  const normalizedRole = String(role || '').trim().toLowerCase()

  return (
    Object.values(AUTH_PORTALS).find(
      (portal) => portal.backendRole.toLowerCase() === normalizedRole,
    ) || null
  )
}

export function isValidPortal(slug) {
  const config = getPortalConfig(slug)
  return config !== null && config.enabled === true
}

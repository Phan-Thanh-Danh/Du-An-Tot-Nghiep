// ============================================================
// Menu Data - BGH (Hiệu trưởng / Ban giám hiệu)
// ============================================================

export const bghMenuGroups = [
  {
    id: 'dashboard',
    label: 'Dashboard',
    icon: 'LayoutDashboard',
    route: '/bgh/dashboard',
    children: [],
  },

  // ── CƠ CẤU TỔ CHỨC ─────────────────────────────────────────
  {
    id: 'co-cau-to-chuc',
    label: 'Cơ cấu tổ chức',
    icon: 'Network',
    children: [
      { id: 'human-resources', label: 'Nhân sự Giảng viên', icon: 'UserCheck', route: '/bgh/human-resources' },
      { id: 'organizations', label: 'Quản lý Đơn vị', icon: 'Building2', route: '/bgh/organizations' },
      { id: 'users', label: 'Quản lý Người dùng', icon: 'Users', route: '/bgh/users' },
    ],
  },

  // ── ĐÀO TẠO & CHƯƠNG TRÌNH ────────────────────────────────
  {
    id: 'dao-tao-chuong-trinh',
    label: 'Đào tạo & Chương trình',
    icon: 'GraduationCap',
    children: [
      { id: 'academic-programs', label: 'Ngành & Chuyên ngành', icon: 'BookOpen', route: '/bgh/academic-programs', permission: 'training.read' },
      { id: 'curriculum', label: 'Khung chương trình', icon: 'Library', route: '/bgh/curriculum', permission: 'training.manage_curriculum' },
      { id: 'academic-terms', label: 'Học kỳ & Khóa', icon: 'CalendarDays', route: '/bgh/academic-terms', permission: 'training.read' },
      // Báo cáo học tập
      { id: 'academic-overview', label: 'Tổng quan kết quả học tập', icon: 'BarChart3', route: '/bgh/academic/overview', permission: 'reports.read' },
      { id: 'academic-gpa', label: 'Báo cáo GPA', icon: 'Award', route: '/bgh/academic/gpa', permission: 'reports.read' },
      { id: 'academic-at-risk', label: 'SV nguy cơ rớt môn', icon: 'AlertTriangle', route: '/bgh/academic/at-risk', permission: 'reports.ai_analysis' },
      { id: 'academic-reports', label: 'Báo cáo chi tiết', icon: 'FileText', route: '/bgh/academic/reports', permission: 'reports.read' },
      { id: 'academic-pass-fail', label: 'Tỷ lệ Pass/Fail', icon: 'TrendingUp', route: '/bgh/academic/pass-fail', permission: 'reports.read' },
    ],
  },

  // ── ĐÁNH GIÁ & KHEN THƯỞNG ────────────────────────────────
  {
    id: 'phe-duyet-danh-gia',
    label: 'Đánh giá & Khen thưởng',
    icon: 'ClipboardCheck',
    children: [
      { id: 'evaluations', label: 'Đánh giá Giảng viên', icon: 'Star', route: '/bgh/evaluations' },
      { id: 'evaluations-ranking', label: 'Xếp hạng giảng viên', icon: 'Trophy', route: '/bgh/evaluations/ranking' },
      { id: 'evaluations-overview', label: 'Tổng quan đánh giá', icon: 'PieChart', route: '/bgh/evaluations/overview' },
      { id: 'awards', label: 'Quản lý Khen thưởng', icon: 'Award', route: '/bgh/awards' },
      { id: 'bgh-certificate-templates', label: 'Cấu hình giấy khen', icon: 'FileCheck', route: '/bgh/awards/certificate-templates' },
    ],
  },

  // ── CƠ SỞ VẬT CHẤT ─────────────────────────────────────────
  {
    id: 'co-so-vat-chat',
    label: 'Cơ sở vật chất',
    icon: 'MapPin',
    children: [
      { id: 'facilities', label: 'Tòa nhà & Phòng học', icon: 'DoorOpen', route: '/bgh/facilities' },
    ],
  },

  // ── GIÁM SÁT HỆ THỐNG ─────────────────────────────────────
  {
    id: 'giam-sat-he-thong',
    label: 'Giám sát hệ thống',
    icon: 'Activity',
    children: [
      { id: 'audit-logs', label: 'Nhật ký kiểm toán', icon: 'History', route: '/bgh/audit-logs' },
    ],
  },

  // ── CÁ NHÂN ───────────────────────────────────────────────
  {
    id: 'ca-nhan',
    label: 'Cá nhân',
    icon: 'User',
    children: [
      { id: 'profile', label: 'Hồ sơ', icon: 'UserCircle', route: '/bgh/profile' },
    ],
  },
]

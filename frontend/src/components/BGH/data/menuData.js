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
      { id: 'organizations', label: 'Quản lý Đơn vị', icon: 'Building2', route: '/bgh/organizations' },
      { id: 'users', label: 'Quản lý Người dùng', icon: 'Users', route: '/bgh/users' },
      { id: 'roles', label: 'Vai trò & Phân quyền', icon: 'ShieldCheck', route: '/bgh/roles' },
    ],
  },

  // ── ĐÀO TẠO & CHƯƠNG TRÌNH ────────────────────────────────
  {
    id: 'dao-tao-chuong-trinh',
    label: 'Đào tạo & Chương trình',
    icon: 'GraduationCap',
    children: [
      { id: 'academic-programs', label: 'Ngành & Chuyên ngành', icon: 'BookOpen', route: '/bgh/academic-programs' },
      { id: 'curriculum', label: 'Khung chương trình', icon: 'Library', route: '/bgh/curriculum' },
      { id: 'academic-terms', label: 'Học kỳ & Khóa', icon: 'CalendarDays', route: '/bgh/academic-terms' },
      // Báo cáo học tập
      { id: 'academic-overview', label: 'Tổng quan kết quả học tập', icon: 'BarChart3', route: '/bgh/academic/overview' },
      { id: 'academic-gpa', label: 'Báo cáo GPA', icon: 'Award', route: '/bgh/academic/gpa' },
      { id: 'academic-at-risk', label: 'SV nguy cơ rớt môn', icon: 'AlertTriangle', route: '/bgh/academic/at-risk' },
      { id: 'academic-reports', label: 'Báo cáo chi tiết', icon: 'FileText', route: '/bgh/academic/reports' },
      { id: 'academic-pass-fail', label: 'Tỷ lệ Pass/Fail', icon: 'TrendingUp', route: '/bgh/academic/pass-fail' },
    ],
  },

  // ── PHÊ DUYỆT & ĐÁNH GIÁ ──────────────────────────────────
  {
    id: 'phe-duyet-danh-gia',
    label: 'Phê duyệt & Đánh giá',
    icon: 'ClipboardCheck',
    children: [
      { id: 'schedule-pending', label: 'Duyệt Thời khóa biểu', icon: 'CalendarClock', route: '/bgh/schedule/pending' },
      { id: 'schedule-conflicts', label: 'Xung đột lịch học', icon: 'AlertTriangle', route: '/bgh/schedule/conflicts' },
      { id: 'schedule-published', label: 'TKB đã duyệt', icon: 'CalendarCheck', route: '/bgh/schedule/published' },
      { id: 'schedule-changes', label: 'Thay đổi & Dạy bù', icon: 'ArrowLeftRight', route: '/bgh/schedule/changes' },
      { id: 'evaluations', label: 'Đánh giá Giảng viên', icon: 'Star', route: '/bgh/evaluations' },
      { id: 'evaluations-ranking', label: 'Xếp hạng giảng viên', icon: 'Trophy', route: '/bgh/evaluations/ranking' },
      { id: 'evaluations-overview', label: 'Tổng quan đánh giá', icon: 'PieChart', route: '/bgh/evaluations/overview' },
      { id: 'evaluations-ai-analysis', label: 'Phân tích Feedback AI', icon: 'Brain', route: '/bgh/evaluations/ai-analysis' },
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

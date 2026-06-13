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
    ],
  },

  // ── PHÊ DUYỆT & ĐÁNH GIÁ ──────────────────────────────────
  {
    id: 'phe-duyet-danh-gia',
    label: 'Phê duyệt & Đánh giá',
    icon: 'ClipboardCheck',
    children: [
      { id: 'schedule-pending', label: 'Duyệt Thời khóa biểu', icon: 'CalendarClock', route: '/bgh/schedule/pending' },
      { id: 'evaluations', label: 'Đánh giá Giảng viên', icon: 'Star', route: '/bgh/evaluations' },
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

export const mockBGH = {
  name: 'Nguyễn Văn Hiệu Trưởng',
  staffId: 'BGH2024001',
  email: 'hieutruong@lms.edu.vn',
  avatar: null,
  initials: 'HT',
  department: 'Ban Giám Hiệu',
  campus: 'Cơ sở chính',
}

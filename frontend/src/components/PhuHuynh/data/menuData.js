// ============================================================
// Menu Data - PhuHuynh Sidebar
// ============================================================

export const phuHuynhMenuGroups = [
  // ── DASHBOARD ─────────────────────────────────────────────
  {
    id: 'dashboard',
    label: 'Dashboard',
    icon: 'LayoutDashboard',
    route: '/parent/dashboard',
    children: [],
  },

  // ── CON CỦA TÔI ───────────────────────────────────────────
  {
    id: 'con-cua-toi',
    label: 'Con của tôi',
    icon: 'Users',
    children: [
      {
        id: 'children-list',
        label: 'Danh sách học sinh',
        icon: 'Contact',
        route: '/parent/children/list',
      },
      {
        id: 'children-overview',
        label: 'Tổng quan học tập',
        icon: 'LineChart',
        route: '/parent/children/overview',
      },
    ],
  },

  // ── HỌC TẬP ───────────────────────────────────────────────
  {
    id: 'hoc-tap',
    label: 'Học tập',
    icon: 'GraduationCap',
    children: [
      {
        id: 'learning-grades',
        label: 'Bảng điểm',
        icon: 'Award',
        route: '/parent/learning/grades',
      },
      {
        id: 'learning-attendance',
        label: 'Điểm danh',
        icon: 'UserCheck',
        route: '/parent/learning/attendance',
      },
      {
        id: 'learning-schedule',
        label: 'Thời khóa biểu',
        icon: 'Calendar',
        route: '/parent/learning/schedule',
      },
      {
        id: 'learning-alerts',
        label: 'Cảnh báo học tập',
        icon: 'AlertTriangle',
        route: '/parent/learning/alerts',
      },
    ],
  },

  // ── TÀI CHÍNH ─────────────────────────────────────────────
  {
    id: 'tai-chinh',
    label: 'Tài chính',
    icon: 'Wallet',
    children: [
      {
        id: 'finance-tuition',
        label: 'Công nợ học phí',
        icon: 'BadgeAlert',
        route: '/parent/finance/tuition',
      },
      {
        id: 'finance-payment',
        label: 'Thanh toán học phí',
        icon: 'CreditCard',
        route: '/parent/finance/payment',
      },
      {
        id: 'finance-transactions',
        label: 'Lịch sử giao dịch',
        icon: 'History',
        route: '/parent/finance/transactions',
      },
      {
        id: 'finance-invoices',
        label: 'Hóa đơn',
        icon: 'FileText',
        route: '/parent/finance/invoices',
      },
    ],
  },

  // ── THÔNG BÁO ─────────────────────────────────────────────
  {
    id: 'thong-bao',
    label: 'Thông báo',
    icon: 'Bell',
    children: [
      {
        id: 'notifications-system',
        label: 'Cảnh báo từ hệ thống',
        icon: 'ShieldAlert',
        route: '/parent/notifications/system',
      },
      {
        id: 'notifications-history',
        label: 'Lịch sử thông báo',
        icon: 'MessageSquare',
        route: '/parent/notifications/history',
      },
    ],
  },

  // ── CÁ NHÂN ───────────────────────────────────────────────
  {
    id: 'ca-nhan',
    label: 'Cá nhân',
    icon: 'User',
    children: [
      {
        id: 'profile-info',
        label: 'Hồ sơ phụ huynh',
        icon: 'UserCircle',
        route: '/parent/profile/info',
      },
      {
        id: 'profile-access-rights',
        label: 'Quyền truy cập được cấp',
        icon: 'ShieldCheck',
        route: '/parent/profile/access-rights',
      },
    ],
  },
]

export const parentUserFallback = {
  name: 'Phụ huynh',
  email: '',
  avatar: null,
  initials: 'PH',
  relation: 'Phụ huynh học sinh',
  campus: 'EduLMS',
}

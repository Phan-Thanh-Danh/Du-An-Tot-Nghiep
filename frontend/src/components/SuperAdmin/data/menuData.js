// ============================================================
// Mock Menu Data - SuperAdmin Sidebar
// ============================================================

export const superAdminMenuGroups = [
  // ── 1. DASHBOARD & TỔNG QUAN ──────────────────────────────
  {
    id: 'dashboard-group',
    label: 'Dashboard & Tổng quan',
    icon: 'LayoutDashboard',
    children: [
      {
        id: 'dashboard',
        label: 'Dashboard tổng hệ thống',
        icon: 'BarChart3',
        route: '/super-admin/dashboard',
      },
      {
        id: 'profile',
        label: 'Hồ sơ cá nhân Admin',
        icon: 'UserCircle',
        route: '/super-admin/profile',
      },
    ],
  },

  // ── 2. QUẢN LÝ CƠ SỞ (ORGANIZATION HIERARCHY) ─────────────
  {
    id: 'organizations-group',
    label: 'Quản lý Cơ sở',
    icon: 'Building2',
    children: [
      {
        id: 'org-tree',
        label: 'Quản lý cây tổ chức',
        icon: 'Network',
        route: '/super-admin/organizations',
      },


    ],
  },

  // ── 3. TÀI KHOẢN VÀ PHÂN QUYỀN (RBAC) ─────────────────────
  {
    id: 'users-group',
    label: 'Tài khoản & Phân quyền',
    icon: 'Users',
    children: [
      {
        id: 'users-list',
        label: 'Danh sách người dùng',
        icon: 'UserCog',
        route: '/super-admin/users',
      },

      {
        id: 'roles-permissions',
        label: 'Vai trò & Quyền hạn',
        icon: 'ShieldAlert',
        route: '/super-admin/roles-permissions',
      },
      {
        id: 'login-history',
        label: 'Lịch sử đăng nhập',
        icon: 'History',
        route: '/super-admin/login-history',
      },
    ],
  },

  // ── 4. QUẢN LÝ ĐÀO TẠO VÀ HỌC VỤ ─────────────────────────
  {
    id: 'training-group',
    label: 'Đào tạo & Học vụ',
    icon: 'GraduationCap',
    children: [
      {
        id: 'semesters',
        label: 'Cấu hình học kỳ',
        icon: 'CalendarRange',
        route: '/super-admin/training/semesters',
      },
      {
        id: 'programs',
        label: 'Cấu trúc chương trình',
        icon: 'BookMarked',
        route: '/super-admin/training/programs',
      },
      {
        id: 'subjects',
        label: 'Quản lý môn học',
        icon: 'BookOpen',
        route: '/super-admin/training/subjects',
      },
      {
        id: 'courses',
        label: 'Quản lý khóa học',
        icon: 'Library',
        route: '/super-admin/training/courses',
      },
      {
        id: 'exam-periods',
        label: 'Mở/Đóng giai đoạn thi',
        icon: 'ClipboardCheck',
        route: '/super-admin/training/exam-periods',
      },
      {
        id: 'attendance-policy',
        label: 'Quỹ vắng & Chuyên cần',
        icon: 'UserCheck',
        route: '/super-admin/operations/attendance-policy',
      },
      {
        id: 'registration-periods',
        label: 'Mở/Đóng đăng ký môn',
        icon: 'ListPlus',
        route: '/super-admin/operations/registration-periods',
      },
      {
        id: 'pass-fail-rules',
        label: 'Điều kiện Pass/Fail',
        icon: 'SlidersHorizontal',
        route: '/super-admin/operations/pass-fail-rules',
      },
    ],
  },

  // ── 5. TÀI CHÍNH VÀ HỌC PHÍ ───────────────────────────────
  {
    id: 'finance-group',
    label: 'Tài chính & Học phí',
    icon: 'Wallet',
    children: [
      {
        id: 'tuition-config',
        label: 'Cấu hình học phí',
        icon: 'BadgePercent',
        route: '/super-admin/finance/tuition-config',
      },
      {
        id: 'student-debts',
        label: 'Công nợ sinh viên',
        icon: 'Receipt',
        route: '/super-admin/finance/student-debts',
      },
      {
        id: 'payments',
        label: 'Theo dõi thanh toán',
        icon: 'CreditCard',
        route: '/super-admin/finance/payments',
      },
      {
        id: 'refunds',
        label: 'Hoàn phí/Bảo lưu',
        icon: 'Undo2',
        route: '/super-admin/finance/refunds',
      },
    ],
  },

  // ── 6. HỖ TRỢ, ĐƠN TỪ VÀ ĐÁNH GIÁ ──────────────────────────
  {
    id: 'support-group',
    label: 'Hỗ trợ & Đơn từ',
    icon: 'LifeBuoy',
    children: [
      {
        id: 'tickets',
        label: 'Ticket hỗ trợ',
        icon: 'MessageCircleHelp',
        route: '/super-admin/support/tickets',
      },
      {
        id: 'faq',
        label: 'Quản lý FAQ',
        icon: 'FileQuestion',
        route: '/super-admin/support/faq',
      },
      {
        id: 'approvals-requests',
        label: 'Đơn cần duyệt',
        icon: 'Inbox',
        route: '/super-admin/approvals/requests',
      },
      {
        id: 'approvals-history',
        label: 'Lịch sử duyệt đơn',
        icon: 'FolderCheck',
        route: '/super-admin/approvals/history',
      },
      {
        id: 'evaluations-config',
        label: 'Cấu hình đánh giá GV',
        icon: 'Star',
        route: '/super-admin/evaluations/config',
      },
      {
        id: 'evaluations-results',
        label: 'Kết quả đánh giá GV',
        icon: 'BarChart',
        route: '/super-admin/evaluations/results',
      },
      {
        id: 'awards',
        label: 'Khen thưởng',
        icon: 'Award',
        route: '/super-admin/awards',
      },
      {
        id: 'discipline',
        label: 'Kỷ luật',
        icon: 'AlertOctagon',
        route: '/super-admin/discipline',
      },
    ],
  },

  // ── 7. BÁO CÁO VÀ PHÂN TÍCH (ANALYTICS) ───────────────────
  {
    id: 'reports-group',
    label: 'Báo cáo & Phân tích',
    icon: 'TrendingUp',
    children: [
      {
        id: 'education-overview',
        label: 'Tổng quan đào tạo',
        icon: 'PieChart',
        route: '/super-admin/reports/education-overview',
      },
      {
        id: 'reports-learning',
        label: 'Báo cáo học tập',
        icon: 'LineChart',
        route: '/super-admin/reports/learning',
      },
      {
        id: 'reports-attendance',
        label: 'Báo cáo chuyên cần',
        icon: 'CalendarCheck2',
        route: '/super-admin/reports/attendance',
      },
      {
        id: 'campus-comparison',
        label: 'So sánh cơ sở',
        icon: 'GitCompare',
        route: '/super-admin/reports/campus-comparison',
      },
      {
        id: 'reports-export',
        label: 'Export dữ liệu',
        icon: 'FileDown',
        route: '/super-admin/reports/export',
      },
    ],
  },

  // ── 8. TRUNG TÂM THÔNG BÁO (NOTIFICATION HUB) ────────────
  {
    id: 'notifications-hub',
    label: 'Trung tâm Thông báo',
    icon: 'Bell',
    children: [
      {
        id: 'notif-templates',
        label: 'Template thông báo',
        icon: 'MailOpen',
        route: '/super-admin/notifications/templates',
      },
      {
        id: 'notif-send',
        label: 'Gửi thông báo',
        icon: 'Send',
        route: '/super-admin/notifications/send',
      },
      {
        id: 'notif-history',
        label: 'Lịch sử thông báo',
        icon: 'FileText',
        route: '/super-admin/notifications/history',
      },
    ],
  },

  // ── 9. QUẢN TRỊ HỆ THỐNG, AUDIT VÀ BẢO MẬT ─────────────────
  {
    id: 'system-security',
    label: 'Quản trị hệ thống',
    icon: 'Settings2',
    children: [
      {
        id: 'audit-logs',
        label: 'Audit Log',
        icon: 'ScrollText',
        route: '/super-admin/audit/logs',
      },
      {
        id: 'security-alerts',
        label: 'Security Alert',
        icon: 'ShieldAlert',
        route: '/super-admin/security/alerts',
      },
      {
        id: 'system-modules',
        label: 'Bật/Tắt module',
        icon: 'ToggleLeft',
        route: '/super-admin/system/modules',
      },
      {
        id: 'ai-automation',
        label: 'Cấu hình AI & Automation',
        icon: 'Cpu',
        route: '/super-admin/system/ai-automation',
      },
    ],
  },
]

// ── Mock admin user ────────────────────────────────────────
export const mockAdminUser = {
  name: 'Super Admin',
  email: 'admin@edu.vn',
  avatar: null,
  initials: 'SA',
  role: 'Super Admin',
  campus: 'Toàn hệ thống',
}

// ── Mock notifications cho admin ───────────────────────────
export const mockAdminNotifications = [
  {
    id: 1,
    type: 'security',
    title: 'Phát hiện đăng nhập bất thường',
    description: '3 lần đăng nhập thất bại từ IP 192.168.1.50',
    time: '5 phút trước',
    read: false,
    icon: 'ShieldAlert',
    color: 'red',
    link: '/super-admin/security/alerts',
  },
  {
    id: 2,
    type: 'system',
    title: 'Backup hoàn tất',
    description: 'Sao lưu tự động lúc 02:00 đã thành công',
    time: '6 giờ trước',
    read: false,
    icon: 'CheckCircle',
    color: 'green',
    link: '/super-admin/system/modules',
  },
  {
    id: 3,
    type: 'user',
    title: 'Tài khoản mới cần duyệt',
    description: '12 tài khoản giảng viên đang chờ kích hoạt',
    time: 'Hôm nay',
    read: true,
    icon: 'UserPlus',
    color: 'blue',
    link: '/super-admin/users',
  },
  {
    id: 4,
    type: 'system',
    title: 'Cập nhật hệ thống khả dụng',
    description: 'Phiên bản v2.4.1 đã sẵn sàng để cài đặt',
    time: '2 ngày trước',
    read: true,
    icon: 'RefreshCw',
    color: 'yellow',
    link: '/super-admin/system/ai-automation',
  },
]

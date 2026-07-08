// Menu Data - SinhVien Sidebar

export const sinhVienMenuGroups = [
  // ── DASHBOARD ─────────────────────────────────────────────
  {
    id: 'dashboard',
    label: 'Tổng quan',
    icon: 'LayoutDashboard',
    route: '/student/dashboard',
    children: [],
  },

  // ── HỌC TẬP ───────────────────────────────────────────────
  {
    id: 'hoc-tap',
    label: 'Học tập',
    icon: 'GraduationCap',
    children: [
      {
        id: 'courses',
        label: 'Khóa học',
        icon: 'BookOpen',
        route: '/student/courses',
      },
      {
        id: 'curriculum',
        label: 'Khung chương trình',
        icon: 'Map',
        route: '/student/curriculum',
      },
      {
        id: 'assignments',
        label: 'Bài tập',
        icon: 'ClipboardList',
        route: '/student/assignments',
      },
    ],
  },

  // ── KẾT QUẢ ──────────────────────────────────────────────
  {
    id: 'ket-qua',
    label: 'Kết quả',
    icon: 'BarChart3',
    children: [
      {
        id: 'exams',
        label: 'Thi / Kiểm tra',
        icon: 'FileCheck',
        route: '/student/exams',
      },
      {
        id: 'grades',
        label: 'Bảng điểm',
        icon: 'BarChart2',
        route: '/student/grades',
      },
    ],
  },

  // ── LỊCH HỌC ──────────────────────────────────────────────
  {
    id: 'lich-hoc',
    label: 'Lịch học',
    icon: 'Calendar',
    children: [
      {
        id: 'schedule',
        label: 'Thời khóa biểu',
        icon: 'CalendarDays',
        route: '/student/schedule',
      },
      {
        id: 'attendance',
        label: 'Điểm danh',
        icon: 'UserCheck',
        route: '/student/attendance',
      },
    ],
  },

  // ── ĐĂNG KÝ ───────────────────────────────────────────────
  {
    id: 'dang-ky',
    label: 'Đăng ký',
    icon: 'FormInput',
    children: [
      {
        id: 'registrations',
        label: 'Đăng ký môn',
        icon: 'ListPlus',
        route: '/student/registrations',
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
        id: 'tuition',
        label: 'Học phí / Thanh toán',
        icon: 'CreditCard',
        route: '/student/tuition',
      },
    ],
  },

  // ── HỖ TRỢ ────────────────────────────────────────────────
  {
    id: 'ho-tro',
    label: 'Hỗ trợ',
    icon: 'LifeBuoy',
    children: [
      {
        id: 'support-tickets',
        label: 'Hỗ trợ / Ticket',
        icon: 'MessageCircleHelp',
        route: '/student/support-tickets',
      },
      {
        id: 'requests',
        label: 'Đơn từ',
        icon: 'FileText',
        route: '/student/requests',
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
        id: 'rewards',
        label: 'Thành tích',
        icon: 'Award',
        route: '/student/rewards',
      },
      {
        id: 'discipline',
        label: 'Kỷ luật',
        icon: 'ShieldAlert',
        route: '/student/discipline',
      },
      {
        id: 'evaluations',
        label: 'Đánh giá giảng viên',
        icon: 'Star',
        route: '/student/evaluations',
      },
      {
        id: 'profile',
        label: 'Hồ sơ cá nhân',
        icon: 'UserCircle',
        route: '/student/profile',
      }
    ],
  },
]

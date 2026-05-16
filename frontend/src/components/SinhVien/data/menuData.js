// ============================================================
// Mock Menu Data - SinhVien Sidebar
// ============================================================

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
      },
      {
        id: 'parent-links',
        label: 'Liên kết phụ huynh',
        icon: 'Users',
        route: '/student/parent-links',
      },
    ],
  },
]

// ── Mock user info ─────────────────────────────────────────
export const mockUser = {
  name: 'Sinh Viên Demo',
  studentId: 'SV2024001',
  email: 'an.nguyen@student.edu.vn',
  avatar: null, // null = dùng initials
  initials: 'SV',
  class: 'CNTT K26A',
  campus: 'Cơ sở Hà Nội',
}

// ── Mock notifications ─────────────────────────────────────
export const mockNotifications = [
  {
    id: 1,
    type: 'deadline',
    title: 'Bài tập CTDL&GT sắp hết hạn',
    description: 'Hạn nộp: Hôm nay 23:59',
    time: '30 phút trước',
    read: false,
    icon: 'AlertCircle',
    color: 'red',
  },
  {
    id: 2,
    type: 'grade',
    title: 'Điểm môn Toán đã được cập nhật',
    description: 'Điểm kiểm tra giữa kỳ: 8.5',
    time: '2 giờ trước',
    read: false,
    icon: 'CheckCircle',
    color: 'green',
  },
  {
    id: 3,
    type: 'schedule',
    title: 'Lịch học thay đổi',
    description: 'Môn Vật lý dời từ P101 sang P205',
    time: 'Hôm qua',
    read: true,
    icon: 'Calendar',
    color: 'blue',
  },
  {
    id: 4,
    type: 'tuition',
    title: 'Nhắc nhở học phí',
    description: 'Học phí kỳ 2 chưa thanh toán',
    time: '2 ngày trước',
    read: true,
    icon: 'Wallet',
    color: 'yellow',
  },
]

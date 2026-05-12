// ============================================================
// Mock Menu Data - GiaoVu Sidebar
// ============================================================

export const giaoVuMenuGroups = [
  {
    id: 'dashboard',
    label: 'Dashboard',
    icon: 'LayoutDashboard',
    route: '/staff/dashboard',
    children: [],
  },

  // ── THỜI KHÓA BIỂU ─────────────────────────────────────────
  {
    id: 'thoi-khoa-bieu',
    label: 'Thời khóa biểu',
    icon: 'Calendar',
    children: [
      { id: 'schedule-mgmt', label: 'Quản lý TKB', icon: 'Settings', route: '/staff/schedule' },
      { id: 'lecturer-assignment', label: 'Phân công giảng viên', icon: 'UserPlus', route: '/staff/assignments' },
      { id: 'room-mgmt', label: 'Phòng học', icon: 'MapPin', route: '/staff/rooms' },
      { id: 'conflict-check', label: 'Kiểm tra xung đột', icon: 'AlertTriangle', route: '/staff/conflicts' },
    ],
  },

  // ── ĐĂNG KÝ MÔN HỌC ───────────────────────────────────────
  {
    id: 'dang-ky-mon',
    label: 'Đăng ký môn học',
    icon: 'FormInput',
    children: [
      { id: 'reg-periods', label: 'Đợt đăng ký', icon: 'Clock', route: '/staff/registrations' },
      { id: 'section-classes', label: 'Lớp học phần', icon: 'Layers', route: '/staff/sections' },
      { id: 'reg-list', label: 'Danh sách đăng ký', icon: 'List', route: '/staff/registration-list' },
    ],
  },

  // ── ĐƠN TỪ ────────────────────────────────────────────────
  {
    id: 'don-tu',
    label: 'Đơn từ',
    icon: 'FileStack',
    children: [
      { id: 'pending-requests', label: 'Đơn cần xử lý', icon: 'FileClock', route: '/staff/requests' },
      { id: 'request-history', label: 'Đơn đã xử lý', icon: 'History', route: '/staff/requests-history' },
      { id: 'workflow-config', label: 'Cấu hình quy trình', icon: 'GitBranch', route: '/staff/workflow' },
    ],
  },

  // ── THÔNG BÁO ─────────────────────────────────────────────
  {
    id: 'thong-bao',
    label: 'Thông báo',
    icon: 'Bell',
    children: [
      { id: 'send-notice', label: 'Gửi thông báo', icon: 'Send', route: '/staff/notices/send' },
      { id: 'notice-history', label: 'Lịch sử thông báo', icon: 'MessageSquare', route: '/staff/notices/history' },
    ],
  },

  // ── CÁ NHÂN ───────────────────────────────────────────────
  {
    id: 'ca-nhan',
    label: 'Cá nhân',
    icon: 'User',
    children: [
      { id: 'profile', label: 'Hồ sơ', icon: 'UserCircle', route: '/staff/profile' },
    ],
  },
]

export const mockStaff = {
  name: 'Trần Thị Giáo Vụ',
  staffId: 'GVU2024001',
  email: 'giaovu@lms.edu.vn',
  avatar: null,
  initials: 'TV',
  department: 'Phòng Đào tạo',
  campus: 'Cơ sở chính',
}

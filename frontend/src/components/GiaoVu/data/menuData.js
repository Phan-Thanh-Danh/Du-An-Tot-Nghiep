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
      { id: 'conflict-check', label: 'Kiểm tra xung đột', icon: 'AlertTriangle', route: '/staff/conflicts' },
      { id: 'schedule-pending', label: 'Lịch chờ duyệt', icon: 'Clock', route: '/staff/schedule/pending' },
      { id: 'schedule-published', label: 'Lịch đã publish', icon: 'CheckSquare', route: '/staff/schedule/published' },
    ],
  },

  // ── QUẢN LÝ KHÓA HỌC ─────────────────────────────────────
  {
    id: 'quan-ly-khoa-hoc',
    label: 'Quản lý khóa học',
    icon: 'BookOpen',
    children: [
      { id: 'academic-terms', label: 'Học kỳ', icon: 'Calendar', route: '/staff/academic-terms' },
      { id: 'subject-list', label: 'Môn học', icon: 'Book', route: '/staff/subjects' },
      { id: 'course-list', label: 'Danh sách khóa học', icon: 'List', route: '/staff/courses' },
    ],
  },

  // ── CƠ SỞ VẬT CHẤT ────────────────────────────────────────
  {
    id: 'co-so-vat-chat',
    label: 'Cơ sở vật chất',
    icon: 'Building',
    children: [
      { id: 'building-mgmt', label: 'Tòa nhà', icon: 'Building', route: '/staff/buildings' },
      { id: 'floor-mgmt', label: 'Lầu', icon: 'Layers', route: '/staff/floors' },
      { id: 'room-mgmt', label: 'Phòng học', icon: 'MapPin', route: '/staff/rooms' },
      { id: 'shift-mgmt', label: 'Ca học', icon: 'Clock', route: '/staff/shifts' },
    ],
  },

  // ── ĐĂNG KÝ MÔN HỌC ───────────────────────────────────────
  {
    id: 'dang-ky-mon',
    label: 'Đăng ký môn học',
    icon: 'FormInput',
    children: [
      { id: 'reg-periods', label: 'Đợt đăng ký', icon: 'Clock', route: '/staff/registrations' },

      { id: 'capacity-adjust', label: 'Điều chỉnh sức chứa', icon: 'Maximize', route: '/staff/capacity' },
      { id: 'course-toggle', label: 'Hủy / mở lớp', icon: 'Power', route: '/staff/course-status' },
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

  // ── LỚP & NGƯỜI DÙNG ──────────────────────────────────────
  {
    id: 'lop-nguoi-dung',
    label: 'Lớp & Người dùng',
    icon: 'Users',
    children: [
      { id: 'class-mgmt', label: 'Lớp hành chính', icon: 'Users', route: '/staff/classes' },
      { id: 'account-mgmt', label: 'Tài khoản', icon: 'UserCog', route: '/staff/accounts' },
    ],
  },

  // ── BÁO CÁO ───────────────────────────────────────────────
  {
    id: 'bao-cao',
    label: 'Báo cáo',
    icon: 'PieChart',
    children: [
      { id: 'general-reports', label: 'Báo cáo tổng hợp', icon: 'BarChart3', route: '/staff/reports' },
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

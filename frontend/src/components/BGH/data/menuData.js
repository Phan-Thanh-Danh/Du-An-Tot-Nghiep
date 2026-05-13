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

  // ── THỜI KHÓA BIỂU ─────────────────────────────────────────
  {
    id: 'thoi-khoa-bieu',
    label: 'Thời khóa biểu',
    icon: 'Calendar',
    children: [
      { id: 'schedule-pending', label: 'TKB chờ duyệt', icon: 'Clock', route: '/bgh/schedule/pending' },
      { id: 'schedule-approved', label: 'TKB đã duyệt', icon: 'CheckCircle', route: '/bgh/schedule/approved' },
      { id: 'conflict-check', label: 'Xung đột lịch', icon: 'AlertTriangle', route: '/bgh/schedule/conflicts' },
      { id: 'schedule-changes', label: 'Lịch thay đổi / dạy bù', icon: 'RefreshCw', route: '/bgh/schedule/changes' },
    ],
  },

  // ── HỌC TẬP & ĐIỂM SỐ ─────────────────────────────────────
  {
    id: 'hoc-tap-diem-so',
    label: 'Học tập & Điểm số',
    icon: 'BarChart2',
    children: [
      { id: 'academic-overview', label: 'Tổng quan kết quả', icon: 'PieChart', route: '/bgh/academic/overview' },
      { id: 'pass-fail-rate', label: 'Tỷ lệ Pass / Fail', icon: 'TrendingUp', route: '/bgh/academic/pass-fail' },
      { id: 'gpa-report', label: 'Báo cáo GPA', icon: 'Award', route: '/bgh/academic/gpa' },
      { id: 'at-risk-students', label: 'SV nguy cơ rớt môn', icon: 'UserMinus', route: '/bgh/academic/at-risk' },
      { id: 'detailed-reports', label: 'Báo cáo chi tiết', icon: 'FileText', route: '/bgh/academic/reports' },
    ],
  },

  // ── ĐÁNH GIÁ GIẢNG VIÊN ───────────────────────────────────
  {
    id: 'danh-gia-giang-vien',
    label: 'Đánh giá giảng viên',
    icon: 'Star',
    children: [
      { id: 'eval-overview', label: 'Tổng quan đánh giá', icon: 'Activity', route: '/bgh/evaluations/overview' },
      { id: 'eval-ranking', label: 'Ranking giảng viên', icon: 'ListOrdered', route: '/bgh/evaluations/ranking' },
      { id: 'eval-details', label: 'Chi tiết đánh giá', icon: 'UserCheck', route: '/bgh/evaluations/details' },
      { id: 'eval-ai-feedback', label: 'Phân tích feedback AI', icon: 'Sparkles', route: '/bgh/evaluations/ai-feedback' },
    ],
  },

  // ── BÁO CÁO CHIẾN LƯỢC ────────────────────────────────────
  {
    id: 'bao-cao-chien-luoc',
    label: 'Báo cáo chiến lược',
    icon: 'LineChart',
    children: [
      { id: 'strategic-dashboard', label: 'Dashboard chất lượng', icon: 'Target', route: '/bgh/strategic/dashboard' },
      { id: 'compare-semesters', label: 'So sánh theo học kỳ', icon: 'GitCompare', route: '/bgh/strategic/semesters' },
      { id: 'compare-campuses', label: 'So sánh theo cơ sở', icon: 'Map', route: '/bgh/strategic/campuses' },
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

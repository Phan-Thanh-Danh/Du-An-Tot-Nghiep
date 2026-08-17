// ============================================================
// Menu Data - GiangVien Sidebar (FULL VERSION)
// ============================================================

export const giangVienMenuGroups = [
  {
    id: 'dashboard',
    label: 'Trang chủ',
    icon: 'LayoutDashboard',
    route: '/teacher/dashboard',
    children: [],
  },

  // ── GIẢNG DẠY & NỘI DUNG ───────────────────────────────────
  {
    id: 'giang-day',
    label: 'Giảng dạy',
    icon: 'GraduationCap',
    permission: 'training.read',
    children: [
      { id: 'schedule', label: 'Lịch giảng dạy', icon: 'Calendar', route: '/teacher/schedule', permission: 'schedules.read' },
      { id: 'courses', label: 'Khóa học', icon: 'Book', route: '/teacher/courses', permission: 'training.read' },
      { id: 'lessons', label: 'Bài học', icon: 'FileVideo', route: '/teacher/lessons', permission: 'training.read' },
    ],
  },

  // ── QUẢN LÝ LỚP HỌC ───────────────────────────────────────
  {
    id: 'quan-ly-lop',
    label: 'Quản lý lớp học',
    icon: 'Users',
    children: [
      { id: 'classes', label: 'Danh sách lớp', icon: 'LayoutList', route: '/teacher/classes', permission: 'training.read' },
      { id: 'progress', label: 'Tiến độ học tập', icon: 'Activity', route: '/teacher/class-progress', permission: 'training.read' },
      { id: 'attendance-history', label: 'Chuyên cần lớp', icon: 'UserCheck', route: '/teacher/class-attendance', permission: 'schedules.read' },
      { id: 'class-grades', label: 'Điểm lớp', icon: 'Table', route: '/teacher/class-grades', permission: 'exams.read' },
    ],
  },

  // ── BÀI TẬP & CHẤM ĐIỂM ────────────────────────────────────
  {
    id: 'bai-tap',
    label: 'Bài tập',
    icon: 'ClipboardList',
    permission: 'exams.read',
    children: [
      { id: 'assignments', label: 'Quản lý bài tập', icon: 'FileText', route: '/teacher/assignments', permission: 'exams.read' },
    ],
  },

  // ── THI CỬ & CANH THI ─────────────────────────────────────
  {
    id: 'thi-cu',
    label: 'Thi cử',
    icon: 'ShieldCheck',
    permission: 'exams.read',
    children: [
      { id: 'exam-results', label: 'Kết quả bài thi', icon: 'Award', route: '/teacher/exam-results', permission: 'exams.read' },
      { id: 'proctoring', label: 'Canh thi', icon: 'Video', route: '/teacher/proctoring', badge: true, permission: 'exams.read' },
    ],
  },

  // ── ĐIỂM DANH & ĐIỂM SỐ ────────────────────────────────────
  {
    id: 'diem-danh-diem-so',
    label: 'Điểm danh & Điểm',
    icon: 'Star',
    permission: 'schedules.read',
    children: [
      { id: 'attendance-history', label: 'Lịch sử điểm danh', icon: 'History', route: '/teacher/attendance-history', permission: 'schedules.read' },
    ],
  },

  // ── TƯƠNG TÁC ─────────────────────────────────────────────
  {
    id: 'tuong-tac',
    label: 'Thảo luận',
    icon: 'MessageSquare',
    children: [
      { id: 'student-questions', label: 'Câu hỏi học sinh', icon: 'HelpCircle', route: '/teacher/student-questions' },
      { id: 'lesson-comments', label: 'Bình luận bài học', icon: 'MessageCircle', route: '/teacher/lesson-comments' },
    ],
  },

  // ── HÀNH CHÍNH ────────────────────────────────────────────
  {
    id: 'hanh-chinh',
    label: 'Đơn từ',
    icon: 'FileStack',
    permission: 'requests.read',
    children: [
      { id: 'pending-requests', label: 'Đơn cần xử lý', icon: 'FileClock', route: '/teacher/requests', permission: 'requests.read' },
      { id: 'request-history', label: 'Lịch sử xử lý', icon: 'History', route: '/teacher/requests-history', permission: 'requests.read' },
    ],
  },

  // ── CÁ NHÂN ───────────────────────────────────────────────
  {
    id: 'ca-nhan',
    label: 'Cá nhân',
    icon: 'User',
    children: [
      { id: 'profile', label: 'Hồ sơ & Bảo mật', icon: 'UserCircle', route: '/teacher/profile' },
      { id: 'teaching-preferences', label: 'Nguyện vọng giảng dạy', icon: 'CalendarClock', route: '/teacher/teaching-preferences' },
    ],
  },
]


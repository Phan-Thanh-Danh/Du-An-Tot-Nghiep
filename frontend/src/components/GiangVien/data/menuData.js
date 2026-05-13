// ============================================================
// Mock Menu Data - GiangVien Sidebar (FULL VERSION)
// ============================================================

export const giangVienMenuGroups = [
  {
    id: 'dashboard',
    label: 'Dashboard',
    icon: 'LayoutDashboard',
    route: '/teacher/dashboard',
    children: [],
  },

  // ── GIẢNG DẠY & NỘI DUNG ───────────────────────────────────
  {
    id: 'giang-day',
    label: 'Giảng dạy',
    icon: 'GraduationCap',
    children: [
      { id: 'courses', label: 'Khóa học', icon: 'Book', route: '/teacher/courses' },
      { id: 'lessons', label: 'Bài học', icon: 'FileVideo', route: '/teacher/lessons' },
    ],
  },

  // ── QUẢN LÝ LỚP HỌC ───────────────────────────────────────
  {
    id: 'quan-ly-lop',
    label: 'Quản lý lớp học',
    icon: 'Users',
    children: [
      { id: 'classes', label: 'Danh sách lớp', icon: 'LayoutList', route: '/teacher/classes' },
      { id: 'progress', label: 'Tiến độ học tập', icon: 'Activity', route: '/teacher/class-progress' },
      { id: 'attendance-history', label: 'Chuyên cần lớp', icon: 'UserCheck', route: '/teacher/class-attendance' },
      { id: 'class-grades', label: 'Điểm lớp', icon: 'Table', route: '/teacher/class-grades' },
    ],
  },

  // ── BÀI TẬP & CHẤM ĐIỂM ────────────────────────────────────
  {
    id: 'bai-tap',
    label: 'Bài tập',
    icon: 'ClipboardList',
    children: [
      { id: 'assignments', label: 'Danh sách bài tập', icon: 'FileText', route: '/teacher/assignments' },
      { id: 'grading', label: 'Bài nộp & Chấm điểm', icon: 'CheckSquare', route: '/teacher/grading' },
    ],
  },

  // ── THI CỬ & CANH THI ─────────────────────────────────────
  {
    id: 'thi-cu',
    label: 'Thi cử',
    icon: 'ShieldCheck',
    children: [
      { id: 'question-bank', label: 'Ngân hàng câu hỏi', icon: 'Database', route: '/teacher/questions' },
      { id: 'exams', label: 'Đề thi', icon: 'FileEdit', route: '/teacher/exams' },
      { id: 'exam-results', label: 'Kết quả bài thi', icon: 'Award', route: '/teacher/exam-results' },
      { id: 'proctoring', label: 'Canh thi (Proctoring)', icon: 'Video', route: '/teacher/proctoring' },
    ],
  },

  // ── ĐIỂM DANH & ĐIỂM SỐ ────────────────────────────────────
  {
    id: 'diem-danh-diem-so',
    label: 'Điểm danh & Điểm',
    icon: 'Star',
    children: [
      { id: 'attendance-today', label: 'Điểm danh hôm nay', icon: 'CheckCircle2', route: '/teacher/attendance' },
      { id: 'attendance-history', label: 'Lịch sử điểm danh', icon: 'History', route: '/teacher/attendance-history' },
      { id: 'grading-input', label: 'Nhập điểm', icon: 'Edit3', route: '/teacher/grading-input' },
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
    children: [
      { id: 'pending-requests', label: 'Đơn cần xử lý', icon: 'FileClock', route: '/teacher/requests' },
      { id: 'request-history', label: 'Lịch sử xử lý', icon: 'History', route: '/teacher/requests-history' },
    ],
  },

  // ── CÁ NHÂN ───────────────────────────────────────────────
  {
    id: 'ca-nhan',
    label: 'Cá nhân',
    icon: 'User',
    children: [
      { id: 'profile', label: 'Hồ sơ & Bảo mật', icon: 'UserCircle', route: '/teacher/profile' },
    ],
  },
]

export const mockTeacher = {
  name: 'TS. Nguyễn Minh Khoa',
  teacherId: 'GV2024005',
  email: 'khoa.nm@lms.edu.vn',
  avatar: null,
  initials: 'MK',
  department: 'Khoa Công nghệ thông tin',
  campus: 'Cơ sở chính',
}

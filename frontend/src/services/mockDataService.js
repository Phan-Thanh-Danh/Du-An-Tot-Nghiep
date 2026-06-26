import { useAuthStore } from '@/stores/auth'
import { mockExams as baseMockExams } from '@/data/studentData.mock.js'

const majorMap = {
  IT: {
    majorName: 'Công nghệ thông tin',
    examMajorName: 'Phát triển phần mềm',
    courses: [
      { id: 'CTDL101', name: 'Cấu trúc dữ liệu và Giải thuật', instructor: 'TS. Nguyễn Minh Khoa', credits: 3, progress: 72, totalSessions: 15, completedSessions: 11, status: 'learning', lastAccessed: '2 giờ trước' },
      { id: 'TRR201', name: 'Toán rời rạc', instructor: 'ThS. Trần Thu Hà', credits: 3, progress: 45, totalSessions: 15, completedSessions: 7, status: 'learning', lastAccessed: 'Hôm qua' },
      { id: 'LTW301', name: 'Lập trình Web (Vue & Nodejs)', instructor: 'KS. Lê Văn Tâm', credits: 4, progress: 90, totalSessions: 20, completedSessions: 18, status: 'learning', lastAccessed: 'Hôm nay, 08:30' },
      { id: 'HQTCSDL401', name: 'Hệ quản trị Cơ sở dữ liệu', instructor: 'PGS. TS Lê Thị Bình', credits: 3, progress: 100, totalSessions: 15, completedSessions: 15, status: 'completed', lastAccessed: 'Tuần trước' },
      { id: 'MMT501', name: 'Mạng máy tính cơ bản', instructor: 'ThS. Phạm Hữu Vinh', credits: 3, progress: 0, totalSessions: 15, completedSessions: 0, status: 'upcoming', lastAccessed: 'Chưa bắt đầu' },
    ],
  },
  KE_TOAN: {
    majorName: 'Kế toán',
    examMajorName: 'Kế toán',
    courses: [
      { id: 'NKT101', name: 'Nguyên lý kế toán', instructor: 'TS. Phạm Thị Lan', credits: 3, progress: 68, totalSessions: 15, completedSessions: 10, status: 'learning', lastAccessed: '1 ngày trước' },
      { id: 'KTT201', name: 'Kế toán tài chính', instructor: 'ThS. Nguyễn Thị Hoa', credits: 3, progress: 52, totalSessions: 15, completedSessions: 8, status: 'learning', lastAccessed: 'Hôm qua' },
      { id: 'THU301', name: 'Thuế', instructor: 'PGS. TS Trần Văn Bình', credits: 4, progress: 30, totalSessions: 20, completedSessions: 6, status: 'learning', lastAccessed: '3 ngày trước' },
      { id: 'KTSX401', name: 'Kế toán sản xuất', instructor: 'TS. Lê Thị Hạnh', credits: 3, progress: 85, totalSessions: 15, completedSessions: 13, status: 'learning', lastAccessed: 'Hôm nay' },
      { id: 'KTV502', name: 'Kiểm toán', instructor: 'ThS. Hoàng Minh Đức', credits: 3, progress: 0, totalSessions: 15, completedSessions: 0, status: 'upcoming', lastAccessed: 'Chưa bắt đầu' },
    ],
  },
  QTKD: {
    majorName: 'Quản trị kinh doanh',
    examMajorName: 'Quản trị kinh doanh',
    courses: [
      { id: 'QTH101', name: 'Quản trị học', instructor: 'TS. Nguyễn Văn Minh', credits: 3, progress: 75, totalSessions: 15, completedSessions: 11, status: 'learning', lastAccessed: 'Hôm qua' },
      { id: 'MKT201', name: 'Marketing căn bản', instructor: 'ThS. Trần Thị Mai', credits: 3, progress: 60, totalSessions: 15, completedSessions: 9, status: 'learning', lastAccessed: '2 ngày trước' },
      { id: 'KTVM301', name: 'Kinh tế vi mô', instructor: 'PGS. TS Lê Quốc Hùng', credits: 4, progress: 40, totalSessions: 20, completedSessions: 8, status: 'learning', lastAccessed: 'Hôm nay' },
      { id: 'QTN402', name: 'Quản trị nhân lực', instructor: 'TS. Phạm Thị Ngọc', credits: 3, progress: 95, totalSessions: 15, completedSessions: 14, status: 'learning', lastAccessed: 'Hôm nay, 09:15' },
      { id: 'TCDN503', name: 'Tài chính doanh nghiệp', instructor: 'ThS. Hoàng Văn An', credits: 3, progress: 0, totalSessions: 15, completedSessions: 0, status: 'upcoming', lastAccessed: 'Chưa bắt đầu' },
    ],
  },
}

const majorExamOverrides = {
  IT: [
    { id: 'exam-toan-001', majorName: 'Công nghệ thông tin' },
    { id: 'exam-ctdl-002', majorName: 'Công nghệ thông tin' },
    { id: 'exam-ltw-003', majorName: 'Công nghệ thông tin' },
    { id: 'exam-mmt-004', majorName: 'Công nghệ thông tin' },
    { id: 'quiz-net-005', majorName: 'Công nghệ thông tin' },
    { id: 'quiz-db-009', majorName: 'Công nghệ thông tin' },
    { id: 'quiz-api-013', majorName: 'Công nghệ thông tin' },
  ],
  KE_TOAN: [
    { id: 'exam-nkt-101', majorName: 'Kế toán', subject: 'Nguyên lý kế toán', subjectCode: 'NKT101', title: 'Kiểm tra giữa kỳ Nguyên lý kế toán', classCode: 'KT-K28A', status: 'upcoming', durationMinutes: 90, totalQuestions: 30, examType: 'multiple_choice', examTypeLabel: 'Trắc nghiệm', teacher: 'TS. Phạm Thị Lan', attempts: 0, maxAttempts: 1, semesterName: 'Kỳ 1', blockName: 'Block 1', score: null, openAt: '2026-06-10T07:30:00', closeAt: '2026-06-10T09:30:00' },
    { id: 'exam-ktt-201', majorName: 'Kế toán', subject: 'Kế toán tài chính', subjectCode: 'KTT201', title: 'Thi cuối kỳ Kế toán tài chính', classCode: 'KT-K28A', status: 'open', durationMinutes: 120, totalQuestions: 40, examType: 'essay', examTypeLabel: 'Tự luận', teacher: 'ThS. Nguyễn Thị Hoa', attempts: 0, maxAttempts: 1, semesterName: 'Kỳ 1', blockName: 'Block 2', score: null, openAt: '2026-06-12T08:00:00', closeAt: '2026-06-12T10:00:00' },
    { id: 'exam-thu-301', majorName: 'Kế toán', subject: 'Thuế', subjectCode: 'THU301', title: 'Quiz Thuế xuất nhập khẩu', classCode: 'KT-K28B', status: 'open', durationMinutes: 30, totalQuestions: 20, examType: 'multiple_choice', examTypeLabel: 'Trắc nghiệm', teacher: 'PGS. TS Trần Văn Bình', attempts: 0, maxAttempts: 1, semesterName: 'Kỳ 2', blockName: 'Block 1', score: null, openAt: '2026-06-14T10:00:00', closeAt: '2026-06-14T23:00:00' },
    { id: 'exam-ktsx-401', majorName: 'Kế toán', subject: 'Kế toán sản xuất', subjectCode: 'KTSX401', title: 'Kiểm tra giữa kỳ Kế toán sản xuất', classCode: 'KT-K29A', status: 'upcoming', durationMinutes: 90, totalQuestions: 35, examType: 'mixed', examTypeLabel: 'Kết hợp', teacher: 'TS. Lê Thị Hạnh', attempts: 0, maxAttempts: 1, semesterName: 'Kỳ 2', blockName: 'Block 2', score: null, openAt: '2026-06-18T13:00:00', closeAt: '2026-06-18T15:00:00' },
    { id: 'exam-ktv-502', majorName: 'Kế toán', subject: 'Kiểm toán', subjectCode: 'KTV502', title: 'Thi cuối kỳ Kiểm toán', classCode: 'KT-K29B', status: 'not_open', durationMinutes: 120, totalQuestions: 40, examType: 'multiple_choice', examTypeLabel: 'Trắc nghiệm', teacher: 'ThS. Hoàng Minh Đức', attempts: 0, maxAttempts: 1, semesterName: 'Kỳ 3', blockName: 'Block 1', score: null, openAt: '2026-06-22T07:30:00', closeAt: '2026-06-22T10:00:00' },
  ],
  QTKD: [
    { id: 'exam-bus-008', majorName: 'Quản trị kinh doanh' },
    { id: 'exam-fin-012', majorName: 'Quản trị kinh doanh' },
    { id: 'quiz-ops-015', majorName: 'Quản trị kinh doanh' },
  ],
}

export function getStudentMajor() {
  const authStore = useAuthStore()
  return authStore.user?.major || null
}

export function getCoursesByMajor(major) {
  const data = majorMap[major]
  return data ? [...data.courses] : []
}

export function getExamsByMajor(major) {
  const data = majorMap[major]
  if (!data) return []

  const overrides = majorExamOverrides[major] || []
  if (!overrides.length) return []

  if (overrides[0]?.subject) {
    return overrides.map((exam) => ({
      ...exam,
      room: exam.room || null,
      riskScore: 0,
      attempts: exam.attempts || 0,
    }))
  }

  const overrideIds = new Set(overrides.map((o) => o.id))
  const renameMap = {}
  overrides.forEach((o) => { renameMap[o.id] = o.majorName })

  return baseMockExams
    .filter((exam) => overrideIds.has(exam.id))
    .map((exam) => ({
      ...exam,
      majorName: renameMap[exam.id] || exam.majorName,
    }))
}

export function getMajorName(major) {
  return majorMap[major]?.majorName || ''
}

export function getExamMajorName(major) {
  return majorMap[major]?.examMajorName || ''
}

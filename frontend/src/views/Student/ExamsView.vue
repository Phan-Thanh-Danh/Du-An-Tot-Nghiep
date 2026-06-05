<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import {
  AlertTriangle,
  BookOpen,
  CalendarDays,
  CheckCircle2,
  ClipboardCheck,
  Eye,
  Filter,
  GraduationCap,
  Info,
  ListChecks,
  Lock,
  PlayCircle,
  Search,
  ShieldAlert,
  Timer,
} from 'lucide-vue-next'
import { mockExams } from '@/data/exam.mock.js'
import {
  canStartLearning,
  canViewLearningResult,
  getLockedReason,
  isLocked,
  LEARNING_ACCESS,
  needsEarlyLearningConfirm,
} from '@/utils/learningAccess.js'
import { getExamAccessState, getExamStatusLabel } from '@/utils/examAccess.js'

const router = useRouter()

const searchQuery = ref('')
const selectedSemester = ref('all')
const selectedMajor = ref('all')
const selectedStatus = ref('all')
const pendingEarlyExam = ref(null)
const feedbackNotice = ref(null)
const studentContext = {
  classSectionCode: 'PM-K29A',
}

// FE-only mock enrichment while waiting for a real Student Exams API.
// Keeps the existing mockExams IDs/routes intact and adds enough cards
// to validate the compact responsive grid.
const baseExamCatalog = [
  ...mockExams.map((exam, index) => ({
    ...exam,
    majorName: ['Phát triển phần mềm', 'Phát triển phần mềm', 'Thiết kế đồ họa', 'Digital Marketing'][index] || 'Phát triển phần mềm',
    semesterName: index < 2 ? 'Kỳ 1' : 'Kỳ 2',
    blockName: index % 2 === 0 ? 'Block 1' : 'Block 2',
    score: exam.status === 'completed' ? 8.8 : null,
  })),
  {
    id: 'quiz-net-005',
    title: 'Quiz C# cơ bản',
    subject: 'Lập trình C#',
    subjectCode: 'NET101',
    classCode: 'PM-K29A',
    majorName: 'Phát triển phần mềm',
    semesterName: 'Kỳ 1',
    blockName: 'Block 2',
    openAt: '2026-05-23T08:00:00',
    closeAt: '2026-05-23T20:00:00',
    durationMinutes: 30,
    totalQuestions: 25,
    examType: 'multiple_choice',
    examTypeLabel: 'Quiz',
    status: 'open',
    attempts: 0,
    maxAttempts: 1,
    score: null,
  },
  {
    id: 'quiz-ui-006',
    title: 'Kiểm tra nguyên lý thị giác',
    subject: 'Cơ sở thiết kế đồ họa',
    subjectCode: 'DES102',
    classCode: 'TK-K29B',
    majorName: 'Thiết kế đồ họa',
    semesterName: 'Kỳ 1',
    blockName: 'Block 1',
    openAt: '2026-05-24T09:00:00',
    closeAt: '2026-05-24T11:00:00',
    durationMinutes: 45,
    totalQuestions: 20,
    examType: 'mixed',
    examTypeLabel: 'Kết hợp',
    status: 'upcoming',
    attempts: 0,
    maxAttempts: 1,
    score: null,
  },
  {
    id: 'quiz-mkt-007',
    title: 'Mini test SEO & Analytics',
    subject: 'Digital Marketing căn bản',
    subjectCode: 'MKT201',
    classCode: 'DM-K29A',
    majorName: 'Digital Marketing',
    semesterName: 'Kỳ 2',
    blockName: 'Block 1',
    openAt: '2026-05-18T13:00:00',
    closeAt: '2026-05-18T14:00:00',
    durationMinutes: 35,
    totalQuestions: 30,
    examType: 'multiple_choice',
    examTypeLabel: 'Quiz',
    status: 'completed',
    attempts: 1,
    maxAttempts: 1,
    resultId: 'result-mkt-007',
    score: 9.1,
  },
  {
    id: 'exam-bus-008',
    title: 'Thi giữa kỳ Quản trị học',
    subject: 'Quản trị học',
    subjectCode: 'BUS101',
    classCode: 'QTKD-K29A',
    majorName: 'Quản trị kinh doanh',
    semesterName: 'Kỳ 1',
    blockName: 'Block 2',
    openAt: '2026-05-25T14:00:00',
    closeAt: '2026-05-25T16:00:00',
    durationMinutes: 90,
    totalQuestions: 40,
    examType: 'multiple_choice',
    examTypeLabel: 'Thi',
    status: 'not_open',
    attempts: 0,
    maxAttempts: 1,
    score: null,
  },
  {
    id: 'quiz-db-009',
    title: 'Quiz SQL nâng cao',
    subject: 'Cơ sở dữ liệu',
    subjectCode: 'DBI202',
    classCode: 'PM-K29C',
    majorName: 'Phát triển phần mềm',
    semesterName: 'Kỳ 2',
    blockName: 'Block 2',
    openAt: '2026-05-20T08:00:00',
    closeAt: '2026-05-20T23:59:00',
    durationMinutes: 25,
    totalQuestions: 18,
    examType: 'multiple_choice',
    examTypeLabel: 'Quiz',
    status: 'expired',
    attempts: 0,
    maxAttempts: 1,
    score: null,
  },
  {
    id: 'quiz-brand-010',
    title: 'Nhận diện thương hiệu',
    subject: 'Brand Design',
    subjectCode: 'DES204',
    classCode: 'TK-K29A',
    majorName: 'Thiết kế đồ họa',
    semesterName: 'Kỳ 2',
    blockName: 'Block 2',
    openAt: '2026-05-26T09:00:00',
    closeAt: '2026-05-26T12:00:00',
    durationMinutes: 40,
    totalQuestions: 22,
    examType: 'mixed',
    examTypeLabel: 'Kết hợp',
    status: 'early_learning',
    attempts: 0,
    maxAttempts: 1,
    isEarlyLearning: true,
    score: null,
  },
  {
    id: 'quiz-crm-011',
    title: 'Kiểm tra CRM & hành vi khách hàng',
    subject: 'Marketing quan hệ khách hàng',
    subjectCode: 'MKT305',
    classCode: 'DM-K29B',
    majorName: 'Digital Marketing',
    semesterName: 'Kỳ 3',
    blockName: 'Block 1',
    openAt: '2026-05-27T15:00:00',
    closeAt: '2026-05-27T16:00:00',
    durationMinutes: 30,
    totalQuestions: 24,
    examType: 'multiple_choice',
    examTypeLabel: 'Quiz',
    status: 'upcoming',
    attempts: 0,
    maxAttempts: 1,
    score: null,
  },
  {
    id: 'exam-fin-012',
    title: 'Thi cuối kỳ Tài chính doanh nghiệp',
    subject: 'Tài chính doanh nghiệp',
    subjectCode: 'FIN301',
    classCode: 'QTKD-K29B',
    majorName: 'Quản trị kinh doanh',
    semesterName: 'Kỳ 3',
    blockName: 'Block 2',
    openAt: '2026-05-15T08:00:00',
    closeAt: '2026-05-15T10:00:00',
    durationMinutes: 120,
    totalQuestions: 35,
    examType: 'essay',
    examTypeLabel: 'Tự luận',
    status: 'awaiting_result',
    attempts: 1,
    maxAttempts: 1,
    score: null,
  },
  {
    id: 'quiz-api-013',
    title: 'REST API & Authentication',
    subject: 'Lập trình Web API',
    subjectCode: 'API302',
    classCode: 'PM-K29B',
    majorName: 'Phát triển phần mềm',
    semesterName: 'Kỳ 3',
    blockName: 'Block 1',
    openAt: '2026-05-28T10:00:00',
    closeAt: '2026-05-28T22:00:00',
    durationMinutes: 45,
    totalQuestions: 28,
    examType: 'mixed',
    examTypeLabel: 'Kết hợp',
    status: 'open',
    attempts: 0,
    maxAttempts: 2,
    score: null,
  },
  {
    id: 'quiz-ux-014',
    title: 'UX Research quick check',
    subject: 'Nghiên cứu người dùng',
    subjectCode: 'UXR201',
    classCode: 'TK-K29C',
    majorName: 'Thiết kế đồ họa',
    semesterName: 'Kỳ 3',
    blockName: 'Block 1',
    openAt: '2026-05-19T07:30:00',
    closeAt: '2026-05-19T08:30:00',
    durationMinutes: 30,
    totalQuestions: 16,
    examType: 'multiple_choice',
    examTypeLabel: 'Quiz',
    status: 'completed',
    attempts: 1,
    maxAttempts: 1,
    resultId: 'result-ux-014',
    score: 7.6,
  },
  {
    id: 'quiz-ops-015',
    title: 'Vận hành chuỗi cung ứng',
    subject: 'Quản trị vận hành',
    subjectCode: 'OPS202',
    classCode: 'QTKD-K29C',
    majorName: 'Quản trị kinh doanh',
    semesterName: 'Kỳ 2',
    blockName: 'Block 1',
    openAt: '2026-05-29T13:00:00',
    closeAt: '2026-05-29T15:00:00',
    durationMinutes: 60,
    totalQuestions: 32,
    examType: 'multiple_choice',
    examTypeLabel: 'Thi',
    status: 'not_open',
    attempts: 0,
    maxAttempts: 1,
    score: null,
  },
  {
    id: 'quiz-ads-016',
    title: 'Campaign Planning Simulation',
    subject: 'Quảng cáo số',
    subjectCode: 'ADS203',
    classCode: 'DM-K29C',
    majorName: 'Digital Marketing',
    semesterName: 'Kỳ 2',
    blockName: 'Block 2',
    openAt: '2026-05-30T09:00:00',
    closeAt: '2026-05-30T11:00:00',
    durationMinutes: 50,
    totalQuestions: 26,
    examType: 'mixed',
    examTypeLabel: 'Kết hợp',
    status: 'early_learning',
    attempts: 0,
    maxAttempts: 1,
    isEarlyLearning: true,
    score: null,
  },
]

const accessOverrides = {
  'exam-toan-001': {
    accessStatus: LEARNING_ACCESS.FUTURE_LOCKED,
    allowEarlyLearning: false,
    plannedSemesterIndex: 2,
    plannedBlockIndex: 1,
    studentCurrentSemesterIndex: 1,
    studentCurrentBlockIndex: 2,
  },
  'exam-ctdl-002': {
    accessStatus: LEARNING_ACCESS.OFFICIAL,
    allowEarlyLearning: false,
    plannedSemesterIndex: 1,
    plannedBlockIndex: 2,
    studentCurrentSemesterIndex: 1,
    studentCurrentBlockIndex: 2,
  },
  'exam-ltw-003': {
    accessStatus: LEARNING_ACCESS.COMPLETED,
    allowEarlyLearning: false,
    plannedSemesterIndex: 1,
    plannedBlockIndex: 1,
    studentCurrentSemesterIndex: 1,
    studentCurrentBlockIndex: 2,
  },
  'exam-mmt-004': {
    accessStatus: LEARNING_ACCESS.LOCKED_PREREQUISITE,
    allowEarlyLearning: false,
    lockedReason: 'Cần đạt môn tiên quyết trước khi xem kết quả.',
    prerequisiteProgress: 60,
    requiredProgress: 100,
  },
  'quiz-net-005': {
    accessStatus: LEARNING_ACCESS.OFFICIAL,
    allowEarlyLearning: false,
    plannedSemesterIndex: 1,
    plannedBlockIndex: 2,
    studentCurrentSemesterIndex: 1,
    studentCurrentBlockIndex: 2,
  },
  'quiz-ui-006': {
    accessStatus: LEARNING_ACCESS.LOCKED_PREREQUISITE,
    allowEarlyLearning: false,
    lockedReason: 'Hoàn thành bài trước để mở quiz.',
    prerequisiteProgress: 65,
    requiredProgress: 80,
  },
  'quiz-mkt-007': {
    accessStatus: LEARNING_ACCESS.COMPLETED,
    allowEarlyLearning: false,
  },
  'exam-bus-008': {
    accessStatus: LEARNING_ACCESS.FUTURE_LOCKED,
    allowEarlyLearning: false,
    plannedSemesterIndex: 2,
    plannedBlockIndex: 2,
    studentCurrentSemesterIndex: 1,
    studentCurrentBlockIndex: 2,
  },
  'quiz-db-009': {
    accessStatus: LEARNING_ACCESS.LOCKED_PREREQUISITE,
    allowEarlyLearning: false,
    lockedReason: 'Cần xem video bài trước tối thiểu 80%.',
    prerequisiteProgress: 42,
    requiredProgress: 80,
  },
  'quiz-brand-010': {
    accessStatus: LEARNING_ACCESS.EARLY_AVAILABLE,
    allowEarlyLearning: true,
    plannedSemesterIndex: 2,
    plannedBlockIndex: 2,
    studentCurrentSemesterIndex: 1,
    studentCurrentBlockIndex: 2,
  },
  'quiz-crm-011': {
    accessStatus: LEARNING_ACCESS.EARLY_AVAILABLE,
    allowEarlyLearning: true,
    plannedSemesterIndex: 3,
    plannedBlockIndex: 1,
    studentCurrentSemesterIndex: 1,
    studentCurrentBlockIndex: 2,
  },
  'exam-fin-012': {
    accessStatus: LEARNING_ACCESS.EARLY_COMPLETED,
    allowEarlyLearning: true,
    earlyScore: 8.2,
    attemptType: 'early',
  },
  'quiz-api-013': {
    accessStatus: LEARNING_ACCESS.EARLY_AVAILABLE,
    allowEarlyLearning: true,
    plannedSemesterIndex: 3,
    plannedBlockIndex: 1,
    studentCurrentSemesterIndex: 1,
    studentCurrentBlockIndex: 2,
  },
  'quiz-ux-014': {
    accessStatus: LEARNING_ACCESS.EARLY_COMPLETED,
    allowEarlyLearning: true,
    earlyScore: 7.6,
    attemptType: 'early',
  },
  'quiz-ops-015': {
    accessStatus: LEARNING_ACCESS.FUTURE_LOCKED,
    allowEarlyLearning: false,
    plannedSemesterIndex: 2,
    plannedBlockIndex: 1,
    studentCurrentSemesterIndex: 1,
    studentCurrentBlockIndex: 2,
  },
  'quiz-ads-016': {
    accessStatus: LEARNING_ACCESS.EARLY_AVAILABLE,
    allowEarlyLearning: true,
    plannedSemesterIndex: 2,
    plannedBlockIndex: 2,
    studentCurrentSemesterIndex: 1,
    studentCurrentBlockIndex: 2,
  },
}

const studentExamAccessOverrides = {
  'exam-ctdl-002': {
    classSectionCode: 'PM-K29A',
    openAt: '2026-06-04T08:00:00',
    closeAt: '2026-06-04T23:59:00',
    status: 'open',
  },
  'quiz-net-005': {
    classSectionCode: 'PM-K29A',
    openAt: '2026-06-04T08:00:00',
    closeAt: '2026-06-04T22:00:00',
    status: 'open',
  },
  'quiz-ui-006': {
    classSectionCode: 'TK-K29B',
    accessPolicy: { allowedByClassSection: false },
  },
  'quiz-db-009': {
    closeAt: '2026-06-01T23:59:00',
    status: 'closed',
  },
  'quiz-api-013': {
    classSectionCode: 'PM-K29B',
    openAt: '2026-06-04T09:00:00',
    closeAt: '2026-06-04T23:00:00',
    status: 'open',
  },
  'quiz-ops-015': {
    usedAttempts: 1,
    attempts: 1,
    maxAttempts: 1,
    status: 'open',
    openAt: '2026-06-04T07:00:00',
    closeAt: '2026-06-04T23:00:00',
  },
}

const examCatalog = baseExamCatalog.map((exam) => ({
  plannedSemesterIndex: 1,
  plannedBlockIndex: 2,
  studentCurrentSemesterIndex: 1,
  studentCurrentBlockIndex: 2,
  allowEarlyLearning: false,
  accessStatus: exam.status === 'completed' ? LEARNING_ACCESS.COMPLETED : LEARNING_ACCESS.OFFICIAL,
  lockedReason: '',
  earlyScore: null,
  attemptType: null,
  classSectionCode: exam.classCode,
  usedAttempts: exam.attempts || 0,
  accessPolicy: {
    requirePassword: false,
    allowedByClassSection: true,
    allowEarlyLearning: false,
  },
  ...exam,
  ...accessOverrides[exam.id],
  ...studentExamAccessOverrides[exam.id],
}))

const accessConfig = {
  [LEARNING_ACCESS.OFFICIAL]: {
    label: 'Đang học',
    badgeClass: 'access-official',
    icon: PlayCircle,
  },
  [LEARNING_ACCESS.EARLY_AVAILABLE]: {
    label: 'Có thể học trước',
    badgeClass: 'access-early',
    icon: AlertTriangle,
  },
  [LEARNING_ACCESS.EARLY_COMPLETED]: {
    label: 'Đã học trước',
    badgeClass: 'access-early-done',
    icon: CheckCircle2,
  },
  [LEARNING_ACCESS.LOCKED_PREREQUISITE]: {
    label: 'Bị khóa',
    badgeClass: 'access-locked',
    icon: Lock,
  },
  [LEARNING_ACCESS.FUTURE_LOCKED]: {
    label: 'Chưa mở',
    badgeClass: 'access-future',
    icon: ShieldAlert,
  },
  [LEARNING_ACCESS.COMPLETED]: {
    label: 'Đã hoàn thành',
    badgeClass: 'access-completed',
    icon: CheckCircle2,
  },
}

const semesterOptions = computed(() => uniqueOptions('semesterName'))
const majorOptions = computed(() => uniqueOptions('majorName'))

const statusOptions = [
  { key: 'all', label: 'Tất cả trạng thái học' },
  { key: LEARNING_ACCESS.OFFICIAL, label: 'Đang học' },
  { key: LEARNING_ACCESS.EARLY_AVAILABLE, label: 'Có thể học trước' },
  { key: LEARNING_ACCESS.EARLY_COMPLETED, label: 'Đã học trước' },
  { key: LEARNING_ACCESS.LOCKED_PREREQUISITE, label: 'Bị khóa' },
  { key: LEARNING_ACCESS.FUTURE_LOCKED, label: 'Chưa mở' },
  { key: LEARNING_ACCESS.COMPLETED, label: 'Đã hoàn thành' },
]

const filteredExams = computed(() => {
  const keyword = normalize(searchQuery.value)

  return examCatalog.filter((exam) => {
    const matchesKeyword = !keyword || [
      exam.title,
      exam.subject,
      exam.subjectCode,
      exam.majorName,
    ].some((value) => normalize(value).includes(keyword))

    const matchesSemester = selectedSemester.value === 'all' || exam.semesterName === selectedSemester.value
    const matchesMajor = selectedMajor.value === 'all' || exam.majorName === selectedMajor.value
    const matchesStatus = selectedStatus.value === 'all' || exam.accessStatus === selectedStatus.value

    return matchesKeyword && matchesSemester && matchesMajor && matchesStatus
  })
})

const metrics = computed(() => [
  { label: 'Đang học', value: examCatalog.filter((exam) => exam.accessStatus === LEARNING_ACCESS.OFFICIAL).length, tone: 'success' },
  { label: 'Học trước', value: examCatalog.filter((exam) => exam.accessStatus === LEARNING_ACCESS.EARLY_AVAILABLE).length, tone: 'info' },
  { label: 'Bị khóa', value: examCatalog.filter((exam) => isLocked(exam)).length, tone: 'warning' },
  { label: 'Đã xong', value: examCatalog.filter((exam) => canViewLearningResult(exam)).length, tone: 'primary' },
])

function uniqueOptions(field) {
  return [...new Set(examCatalog.map((exam) => exam[field]).filter(Boolean))]
}

function normalize(value) {
  return String(value || '')
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
}

function formatDate(iso) {
  if (!iso) return 'Chưa có lịch'
  const d = new Date(iso)
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' })
}

function actionLabel(exam) {
  const examAccess = getExamAccessState(exam, studentContext)
  if (examAccess.canViewResult) return 'Xem kết quả'
  if (exam.accessStatus === LEARNING_ACCESS.EARLY_AVAILABLE) return 'Làm trước'
  if (exam.accessStatus === LEARNING_ACCESS.EARLY_COMPLETED) return exam.resultId ? 'Xem kết quả' : 'Tiếp tục học'
  if (exam.accessStatus === LEARNING_ACCESS.COMPLETED) return 'Xem kết quả'
  if (exam.accessStatus === LEARNING_ACCESS.OFFICIAL) return 'Vào làm'
  return 'Chưa mở'
}

function canNavigate(exam) {
  const examAccess = getExamAccessState(exam, studentContext)
  return (canStartLearning(exam) && examAccess.canEnter) || canViewLearningResult(exam) || examAccess.canViewResult
}

function goToExam(exam) {
  feedbackNotice.value = null

  const examAccess = getExamAccessState(exam, studentContext)

  if (isLocked(exam)) {
    feedbackNotice.value = {
      title: accessConfig[exam.accessStatus]?.label || 'Chưa mở',
      message: getLockedReason(exam),
    }
    return
  }

  if (!examAccess.canEnter && !examAccess.canViewResult && !canViewLearningResult(exam)) {
    feedbackNotice.value = {
      title: 'Chưa thể vào thi',
      message: examAccess.reason,
    }
    return
  }

  if (examAccess.canViewResult || canViewLearningResult(exam)) {
    navigateExam(exam)
    return
  }

  if (needsEarlyLearningConfirm(exam)) {
    pendingEarlyExam.value = exam
    return
  }

  navigateExam(exam)
}

function navigateExam(exam) {
  if (!canNavigate(exam)) return

  if (canViewLearningResult(exam) && exam.resultId) {
    router.push(`/student/exams/${exam.resultId}`)
    return
  }

  router.push(`/student/exams/detail/${exam.id}`)
}

function confirmEarlyLearning() {
  if (!pendingEarlyExam.value) return
  const exam = pendingEarlyExam.value
  pendingEarlyExam.value = null
  navigateExam(exam)
}

function closeEarlyModal() {
  pendingEarlyExam.value = null
}

function displayScore(exam) {
  if (exam.accessStatus === LEARNING_ACCESS.EARLY_COMPLETED && exam.earlyScore !== null && exam.earlyScore !== undefined) {
    return `${exam.earlyScore}/10`
  }
  if (exam.score !== null && exam.score !== undefined) return `${exam.score}/10`
  return '--'
}

function progressCopy(exam) {
  if (!isLocked(exam) || !exam.requiredProgress) return ''
  return `${exam.prerequisiteProgress || 0}/${exam.requiredProgress}% điều kiện`
}

function accessReason(exam) {
  if (isLocked(exam)) return getLockedReason(exam)
  return getExamAccessState(exam, studentContext).reason
}

function accessStateLabel(exam) {
  const state = getExamAccessState(exam, studentContext)
  if (state.canEnter) return 'Được phép vào thi'
  if (state.canViewResult) return 'Chỉ xem kết quả'
  return state.reason || getExamStatusLabel(exam.status)
}
</script>

<template>
  <div class="exams-page">
    <section class="exam-toolbar">
      <div class="title-block">
        <span class="eyebrow">
          <ClipboardCheck :size="14" />
          Đánh giá
        </span>
        <div>
          <h1>Thi / Kiểm tra</h1>
          <p>Danh sách quiz, ca thi và kết quả theo học kỳ.</p>
        </div>
      </div>

      <div class="metric-strip" aria-label="Tổng quan bài thi">
        <div
          v-for="metric in metrics"
          :key="metric.label"
          class="metric-pill"
          :class="`metric-${metric.tone}`"
        >
          <span>{{ metric.value }}</span>
          {{ metric.label }}
        </div>
      </div>
    </section>

    <section class="filter-panel" aria-label="Bộ lọc bài thi">
      <div class="search-field">
        <Search :size="16" />
        <input
          v-model="searchQuery"
          type="search"
          placeholder="Tìm theo tên môn, mã môn..."
        />
      </div>

      <label class="select-field">
        <CalendarDays :size="15" />
        <select v-model="selectedSemester">
          <option value="all">Tất cả kỳ</option>
          <option v-for="semester in semesterOptions" :key="semester" :value="semester">
            {{ semester }}
          </option>
        </select>
      </label>

      <label class="select-field">
        <GraduationCap :size="15" />
        <select v-model="selectedMajor">
          <option value="all">Tất cả khoa/ngành</option>
          <option v-for="major in majorOptions" :key="major" :value="major">
            {{ major }}
          </option>
        </select>
      </label>

      <label class="select-field">
        <Filter :size="15" />
        <select v-model="selectedStatus">
          <option v-for="status in statusOptions" :key="status.key" :value="status.key">
            {{ status.label }}
          </option>
        </select>
      </label>
    </section>

    <section v-if="filteredExams.length" class="exam-grid" aria-label="Danh sách bài thi">
      <article
        v-for="exam in filteredExams"
        :key="exam.id"
        class="exam-card"
        :class="{ 'is-locked': isLocked(exam), 'is-early': exam.accessStatus === LEARNING_ACCESS.EARLY_AVAILABLE }"
      >
        <header class="card-top">
          <span class="status-badge" :class="accessConfig[exam.accessStatus]?.badgeClass">
            <component :is="accessConfig[exam.accessStatus]?.icon || Info" :size="12" />
            {{ accessConfig[exam.accessStatus]?.label || exam.accessStatus }}
          </span>
          <span class="term-chip">{{ exam.semesterName }} · {{ exam.blockName }}</span>
        </header>

        <div class="card-main">
          <h2>{{ exam.title }}</h2>
          <p class="subject-line">
            <span>{{ exam.subjectCode }}</span>
            {{ exam.subject }}
          </p>
          <p class="major-line">
            <GraduationCap :size="13" />
            {{ exam.majorName }}
          </p>
          <p class="major-line">
            <ShieldAlert :size="13" />
            {{ exam.classSectionCode || exam.classCode }} · Không cần mật khẩu đề thi
          </p>
          <div class="learning-route">
            <span>Hiện tại: Kỳ {{ exam.studentCurrentSemesterIndex }} · Block {{ exam.studentCurrentBlockIndex }}</span>
            <span>Lộ trình: Kỳ {{ exam.plannedSemesterIndex }} · Block {{ exam.plannedBlockIndex }}</span>
          </div>
        </div>

        <dl class="info-grid">
          <div>
            <dt><Timer :size="13" /> Thời lượng</dt>
            <dd>{{ exam.durationMinutes }} phút</dd>
          </div>
          <div>
            <dt><ListChecks :size="13" /> Câu hỏi</dt>
            <dd>{{ exam.totalQuestions }} câu</dd>
          </div>
          <div>
            <dt><BookOpen :size="13" /> Hình thức</dt>
            <dd>{{ exam.examTypeLabel }}</dd>
          </div>
          <div>
            <dt><Lock :size="13" /> Lần làm</dt>
            <dd>{{ exam.usedAttempts ?? exam.attempts }}/{{ exam.maxAttempts }}</dd>
          </div>
        </dl>

        <div class="card-window">
          <span>Mở {{ formatDate(exam.openAt) }}</span>
          <span>Đóng {{ formatDate(exam.closeAt) }}</span>
        </div>
        <div class="exam-access-line">
          <ShieldAlert :size="12" />
          {{ accessStateLabel(exam) }}
        </div>

        <div
          v-if="exam.accessStatus === LEARNING_ACCESS.EARLY_AVAILABLE"
          class="access-note note-early"
        >
          <AlertTriangle :size="13" />
          Nội dung này thuộc kỳ sau, nhưng bạn có thể học trước.
        </div>

        <div
          v-else-if="isLocked(exam)"
          class="access-note note-locked"
        >
          <Lock :size="13" />
          <span>
            {{ getLockedReason(exam) }}
            <small v-if="progressCopy(exam)">{{ progressCopy(exam) }}</small>
          </span>
        </div>

        <div
          v-else-if="!getExamAccessState(exam, studentContext).canEnter && !getExamAccessState(exam, studentContext).canViewResult"
          class="access-note note-locked"
        >
          <Lock :size="13" />
          <span>{{ accessReason(exam) }}</span>
        </div>

        <div
          v-else-if="exam.accessStatus === LEARNING_ACCESS.EARLY_COMPLETED"
          class="access-note note-early-done"
        >
          <CheckCircle2 :size="13" />
          Kết quả học trước đã được giữ lại.
        </div>

        <footer class="card-footer">
          <div class="score-box">
            <span>Kết quả</span>
            <strong>{{ displayScore(exam) }}</strong>
          </div>

          <div class="card-actions">
            <button
              class="action-button"
              :class="{ disabled: !canNavigate(exam) }"
              :disabled="!canNavigate(exam)"
              @click="goToExam(exam)"
            >
              <PlayCircle v-if="canStartLearning(exam)" :size="14" />
              <Eye v-else :size="14" />
              {{ actionLabel(exam) }}
            </button>
            <button
              v-if="isLocked(exam)"
              class="condition-button"
              type="button"
              @click="goToExam(exam)"
            >
              Xem điều kiện
            </button>
          </div>
        </footer>
      </article>
    </section>

    <section v-else class="empty-state">
      <ClipboardCheck :size="34" />
      <h2>Không tìm thấy bài kiểm tra</h2>
      <p>Không tìm thấy bài kiểm tra phù hợp với bộ lọc hiện tại.</p>
    </section>

    <section v-if="feedbackNotice" class="inline-notice" role="status">
      <ShieldAlert :size="16" />
      <div>
        <strong>{{ feedbackNotice.title }}</strong>
        <p>{{ feedbackNotice.message }}</p>
      </div>
      <button type="button" @click="feedbackNotice = null">Đóng</button>
    </section>

    <Teleport to="body">
      <div v-if="pendingEarlyExam" class="modal-backdrop" @click.self="closeEarlyModal">
        <section class="early-modal" role="dialog" aria-modal="true" aria-labelledby="early-learning-title">
          <div class="modal-icon">
            <AlertTriangle :size="20" />
          </div>
          <div class="modal-copy">
            <h2 id="early-learning-title">Bạn đang học trước lộ trình</h2>
            <p>
              Nội dung này thuộc {{ pendingEarlyExam.semesterName }} · {{ pendingEarlyExam.blockName }}
              trong lộ trình tương lai. Bạn vẫn có thể học trước và kết quả sẽ được ghi nhận
              ở trạng thái học trước. Khi đến đúng kỳ/block, hệ thống sẽ áp dụng theo quy định
              của môn học.
            </p>
            <div class="modal-subject">
              <strong>{{ pendingEarlyExam.title }}</strong>
              <span>{{ pendingEarlyExam.subjectCode }} · {{ pendingEarlyExam.subject }}</span>
            </div>
          </div>
          <div class="modal-actions">
            <button type="button" class="ghost-button" @click="closeEarlyModal">Quay lại</button>
            <button type="button" class="primary-button" @click="confirmEarlyLearning">
              Tiếp tục học trước
            </button>
          </div>
        </section>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.exams-page {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  width: 100%;
}

.exam-toolbar {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 1rem;
}

.title-block {
  display: flex;
  flex-direction: column;
  gap: 0.45rem;
  min-width: 0;
}

.eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  width: fit-content;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-link);
  padding: 0.25rem 0.55rem;
  font-size: 0.6875rem;
  font-weight: 800;
  text-transform: uppercase;
}

.title-block h1 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1.35rem;
  font-weight: 850;
  line-height: 1.15;
}

.title-block p {
  margin: 0.2rem 0 0;
  color: var(--text-body);
  font-size: 0.8125rem;
}

.metric-strip {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 0.45rem;
}

.metric-pill {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  min-height: 1.8rem;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-label);
  padding: 0.25rem 0.65rem;
  font-size: 0.72rem;
  font-weight: 750;
}

.metric-pill span {
  color: var(--text-heading);
  font-size: 0.875rem;
  font-weight: 850;
}

.metric-success { box-shadow: inset 3px 0 0 var(--lg-success); }
.metric-info { box-shadow: inset 3px 0 0 var(--lg-info); }
.metric-primary { box-shadow: inset 3px 0 0 var(--lg-primary); }
.metric-warning { box-shadow: inset 3px 0 0 var(--lg-warning); }

.filter-panel {
  display: grid;
  grid-template-columns: minmax(220px, 1.2fr) repeat(3, minmax(150px, 0.7fr));
  gap: 0.55rem;
  border: 1px solid var(--border-card);
  border-radius: 20px;
  background: var(--surface-card);
  padding: 0.65rem;
  box-shadow: var(--lg-shadow-sm);
  backdrop-filter: blur(18px) saturate(160%);
}

.search-field,
.select-field {
  display: flex;
  align-items: center;
  gap: 0.45rem;
  min-height: 2.35rem;
  border: 1px solid var(--border-input);
  border-radius: 14px;
  background: var(--surface-input);
  color: var(--text-placeholder);
  padding: 0 0.7rem;
}

.search-field input,
.select-field select {
  min-width: 0;
  width: 100%;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-label);
  font-size: 0.8125rem;
  font-weight: 650;
}

.search-field input::placeholder {
  color: var(--text-placeholder);
}

.select-field select {
  cursor: pointer;
}

.exam-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 0.8rem;
}

.exam-card {
  display: flex;
  min-height: 18.5rem;
  flex-direction: column;
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: 20px;
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--surface-card-strong) 88%, transparent), var(--surface-card)),
    radial-gradient(circle at 12% 0%, rgba(37, 99, 235, 0.08), transparent 32%);
  box-shadow:
    var(--lg-shadow-sm),
    inset 0 1px 0 rgba(255, 255, 255, 0.18);
  backdrop-filter: blur(18px) saturate(165%);
  transition:
    transform 180ms ease,
    border-color 180ms ease,
    box-shadow 180ms ease;
}

.exam-card:hover {
  transform: translateY(-2px);
  border-color: var(--border-input-focus);
  box-shadow: var(--lg-shadow-md);
}

.exam-card.is-locked {
  border-style: dashed;
}

.exam-card.is-early {
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--surface-card-strong) 88%, transparent), var(--surface-card)),
    radial-gradient(circle at 12% 0%, rgba(124, 58, 237, 0.12), transparent 34%);
}

.card-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.45rem;
  border-bottom: 1px solid var(--border-card);
  padding: 0.7rem 0.75rem 0.55rem;
}

.status-badge,
.term-chip {
  display: inline-flex;
  align-items: center;
  gap: 0.3rem;
  min-height: 1.4rem;
  border-radius: 999px;
  padding: 0.2rem 0.5rem;
  font-size: 0.66rem;
  font-weight: 850;
  line-height: 1;
  white-space: nowrap;
}

.term-chip {
  border: 1px solid var(--border-card);
  color: var(--text-placeholder);
  background: var(--surface-input);
}

.status-open { color: var(--color-success-text); background: var(--color-success-bg); }
.status-upcoming { color: var(--color-info-text); background: var(--color-info-bg); }
.status-completed { color: var(--text-link); background: color-mix(in srgb, var(--color-info-bg) 72%, transparent); }
.status-awaiting { color: var(--color-warning-text); background: var(--color-warning-bg); }
.status-locked { color: var(--text-placeholder); background: var(--surface-input); }
.status-early { color: #7c3aed; background: rgba(237, 233, 254, 0.82); }
.status-expired { color: var(--color-danger-text); background: var(--color-danger-bg); }

.access-official { color: var(--color-success-text); background: var(--color-success-bg); }
.access-early { color: #7c3aed; background: rgba(237, 233, 254, 0.82); }
.access-early-done { color: #6d28d9; background: rgba(237, 233, 254, 0.72); }
.access-locked { color: var(--color-warning-text); background: var(--color-warning-bg); }
.access-future { color: var(--text-placeholder); background: var(--surface-input); }
.access-completed { color: var(--text-link); background: color-mix(in srgb, var(--color-info-bg) 72%, transparent); }

:global(.dark) .status-early {
  color: #d8b4fe;
  background: rgba(88, 28, 135, 0.36);
}

:global(.dark) .access-early,
:global(.dark) .access-early-done {
  color: #d8b4fe;
  background: rgba(88, 28, 135, 0.36);
}

.card-main {
  display: flex;
  flex-direction: column;
  gap: 0.45rem;
  padding: 0.75rem 0.75rem 0.55rem;
}

.card-main h2 {
  display: -webkit-box;
  min-height: 2.35rem;
  margin: 0;
  overflow: hidden;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  color: var(--text-heading);
  font-size: 0.92rem;
  font-weight: 850;
  line-height: 1.28;
}

.subject-line {
  display: -webkit-box;
  margin: 0;
  overflow: hidden;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  color: var(--text-body);
  font-size: 0.76rem;
  font-weight: 650;
  line-height: 1.35;
}

.subject-line span {
  color: var(--text-link);
  font-weight: 900;
}

.major-line {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  margin: 0;
  color: var(--text-label);
  font-size: 0.73rem;
  font-weight: 750;
}

.learning-route {
  display: grid;
  gap: 0.25rem;
  border: 1px solid var(--border-card);
  border-radius: 12px;
  background: var(--surface-input);
  padding: 0.45rem 0.5rem;
  color: var(--text-placeholder);
  font-size: 0.64rem;
  font-weight: 750;
  line-height: 1.25;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.45rem;
  margin: 0;
  padding: 0 0.75rem;
}

.info-grid div {
  min-width: 0;
  border: 1px solid var(--border-card);
  border-radius: 12px;
  background: var(--surface-input);
  padding: 0.45rem;
}

.info-grid dt {
  display: flex;
  align-items: center;
  gap: 0.3rem;
  color: var(--text-placeholder);
  font-size: 0.64rem;
  font-weight: 800;
}

.info-grid dd {
  margin: 0.15rem 0 0;
  color: var(--text-heading);
  font-size: 0.74rem;
  font-weight: 850;
}

.card-window {
  display: flex;
  justify-content: space-between;
  gap: 0.45rem;
  margin: 0.65rem 0.75rem 0;
  color: var(--text-placeholder);
  font-size: 0.66rem;
  font-weight: 750;
}

.exam-access-line {
  display: inline-flex;
  align-items: center;
  gap: 0.3rem;
  margin: 0.35rem 0.75rem 0;
  color: var(--text-label);
  font-size: 0.66rem;
  font-weight: 800;
}

.access-note {
  display: flex;
  align-items: flex-start;
  gap: 0.35rem;
  min-height: 2.1rem;
  margin: 0.55rem 0.75rem 0;
  border: 1px solid var(--border-card);
  border-radius: 12px;
  padding: 0.45rem 0.5rem;
  font-size: 0.68rem;
  font-weight: 750;
  line-height: 1.3;
}

.access-note svg {
  flex-shrink: 0;
  margin-top: 0.05rem;
}

.access-note small {
  display: block;
  margin-top: 0.15rem;
  color: var(--text-placeholder);
  font-size: 0.62rem;
}

.note-early {
  color: #7c3aed;
  background: rgba(237, 233, 254, 0.48);
}

.note-locked {
  color: var(--color-warning-text);
  background: var(--color-warning-bg);
}

.note-early-done {
  color: var(--color-success-text);
  background: var(--color-success-bg);
}

:global(.dark) .note-early {
  color: #d8b4fe;
  background: rgba(88, 28, 135, 0.24);
}

.card-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.55rem;
  margin-top: auto;
  border-top: 1px solid var(--border-card);
  padding: 0.65rem 0.75rem;
}

.score-box {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 0.1rem;
}

.score-box span {
  color: var(--text-placeholder);
  font-size: 0.64rem;
  font-weight: 800;
}

.score-box strong {
  color: var(--text-heading);
  font-size: 0.875rem;
  font-weight: 900;
}

.card-actions {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.35rem;
}

.action-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.3rem;
  min-height: 2rem;
  border: 0;
  border-radius: 12px;
  background: linear-gradient(135deg, var(--lg-primary-dark), var(--lg-primary));
  color: #ffffff;
  cursor: pointer;
  padding: 0 0.7rem;
  font-size: 0.73rem;
  font-weight: 850;
  white-space: nowrap;
  box-shadow: 0 8px 20px rgba(37, 99, 235, 0.22);
  transition: transform 160ms ease, box-shadow 160ms ease;
}

.action-button:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 10px 24px rgba(37, 99, 235, 0.3);
}

.action-button.disabled {
  background: var(--surface-input);
  color: var(--text-placeholder);
  box-shadow: none;
  cursor: not-allowed;
}

.condition-button {
  border: 0;
  background: transparent;
  color: var(--text-link);
  cursor: pointer;
  padding: 0;
  font-size: 0.67rem;
  font-weight: 850;
}

.inline-notice {
  position: fixed;
  right: 1rem;
  bottom: 1rem;
  z-index: 40;
  display: flex;
  align-items: flex-start;
  gap: 0.65rem;
  max-width: min(26rem, calc(100vw - 2rem));
  border: 1px solid var(--border-card);
  border-radius: 18px;
  background: var(--surface-card-strong);
  color: var(--text-body);
  padding: 0.75rem;
  box-shadow: var(--lg-shadow-md);
  backdrop-filter: blur(18px) saturate(160%);
}

.inline-notice strong {
  display: block;
  color: var(--text-heading);
  font-size: 0.82rem;
}

.inline-notice p {
  margin: 0.15rem 0 0;
  color: var(--text-label);
  font-size: 0.75rem;
}

.inline-notice button {
  margin-left: auto;
  border: 0;
  background: transparent;
  color: var(--text-link);
  cursor: pointer;
  font-size: 0.72rem;
  font-weight: 850;
}

.modal-backdrop {
  position: fixed;
  inset: 0;
  z-index: 50;
  display: grid;
  place-items: center;
  background: rgba(15, 23, 42, 0.44);
  padding: 1rem;
  backdrop-filter: blur(8px);
}

.early-modal {
  width: min(30rem, 100%);
  border: 1px solid var(--border-card);
  border-radius: 22px;
  background: var(--surface-modal);
  color: var(--text-body);
  padding: 1rem;
  box-shadow: var(--lg-shadow-lg);
}

.modal-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2.5rem;
  height: 2.5rem;
  border-radius: 16px;
  color: #7c3aed;
  background: rgba(237, 233, 254, 0.78);
}

:global(.dark) .modal-icon {
  color: #d8b4fe;
  background: rgba(88, 28, 135, 0.32);
}

.modal-copy {
  margin-top: 0.8rem;
}

.modal-copy h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1.05rem;
  font-weight: 900;
}

.modal-copy p {
  margin: 0.55rem 0 0;
  color: var(--text-label);
  font-size: 0.85rem;
  line-height: 1.55;
}

.modal-subject {
  display: grid;
  gap: 0.2rem;
  margin-top: 0.75rem;
  border: 1px solid var(--border-card);
  border-radius: 14px;
  background: var(--surface-input);
  padding: 0.65rem;
}

.modal-subject strong {
  color: var(--text-heading);
  font-size: 0.85rem;
}

.modal-subject span {
  color: var(--text-label);
  font-size: 0.76rem;
  font-weight: 700;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.55rem;
  margin-top: 1rem;
}

.ghost-button,
.primary-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 2.35rem;
  border-radius: 12px;
  cursor: pointer;
  padding: 0 0.85rem;
  font-size: 0.8rem;
  font-weight: 850;
}

.ghost-button {
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-label);
}

.primary-button {
  border: 0;
  background: linear-gradient(135deg, var(--lg-primary-dark), var(--lg-primary));
  color: #ffffff;
  box-shadow: 0 8px 20px rgba(37, 99, 235, 0.22);
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  min-height: 16rem;
  border: 1px solid var(--border-card);
  border-radius: 22px;
  background: var(--surface-card);
  color: var(--text-body);
  text-align: center;
}

.empty-state h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 850;
}

.empty-state p {
  margin: 0;
  color: var(--text-label);
  font-size: 0.8125rem;
}

@media (max-width: 1023px) {
  .exam-toolbar {
    align-items: flex-start;
    flex-direction: column;
  }

  .metric-strip {
    justify-content: flex-start;
  }

  .filter-panel {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .search-field {
    grid-column: 1 / -1;
  }

  .exam-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 640px) {
  .filter-panel,
  .exam-grid {
    grid-template-columns: 1fr;
  }

  .metric-strip {
    width: 100%;
  }

  .metric-pill {
    flex: 1 1 calc(50% - 0.5rem);
    justify-content: center;
  }
}
</style>

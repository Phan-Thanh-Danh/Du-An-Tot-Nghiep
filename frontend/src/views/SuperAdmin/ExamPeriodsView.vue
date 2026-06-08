<script setup>
/**
 * ExamPeriodsView.vue - Super Admin
 * Giao diện điều phối ca thi, kiểm soát trạng thái (Scheduled -> Open -> Closed -> Published),
 * quản lý nhật ký vi phạm (Append-only) và ghi Audit Logs của hệ thống thi trực tuyến.
 */
import { ref, computed, watch } from 'vue'
import {
  Calendar,
  Search,
  Play,
  Square,
  CheckCircle,
  AlertTriangle,
  History,
  Clock,
  X,
  Save,
  AlertCircle,
  Filter,
  Plus,
  Eye,
  Info
} from 'lucide-vue-next'

// --- Mock Data ---

// Danh sách học kỳ
const semesters = ref(['Spring 2026', 'Summer 2026', 'Fall 2025'])

// Danh sách môn học
const subjects = ref([
  { code: 'PRN211', name: 'Lập trình C# (.NET)' },
  { code: 'SWD392', name: 'Thiết kế & Kiến trúc phần mềm' },
  { code: 'PRN221', name: 'Lập trình ứng dụng Windows' },
  { code: 'SWE201c', name: 'Nhập môn kỹ nghệ phần mềm' }
])

// Danh sách lớp học phần
const classSections = ref(['SE1701', 'SE1702', 'SE1703', 'IA1701'])

// Danh sách sinh viên theo lớp (cho form thêm vi phạm)
const studentsByClass = {
  'SE1701': [
    { id: 'HE170001', name: 'Nguyễn Văn An' },
    { id: 'HE170002', name: 'Trần Thị Bình' },
    { id: 'HE170003', name: 'Lê Văn Cường' },
    { id: 'HE170004', name: 'Phan Thanh Danh' }
  ],
  'SE1702': [
    { id: 'HE170101', name: 'Phạm Đức Duy' },
    { id: 'HE170102', name: 'Hoàng Minh Em' },
    { id: 'HE170103', name: 'Đỗ Thị Hạnh' },
    { id: 'HE170104', name: 'Nguyễn Thị Hương' }
  ],
  'SE1703': [
    { id: 'HE170201', name: 'Vũ Hoàng Nam' },
    { id: 'HE170202', name: 'Nguyễn Thanh Tùng' },
    { id: 'HE170203', name: 'Lương Bảo Giang' }
  ],
  'IA1701': [
    { id: 'HE170301', name: 'Bùi Gia Bảo' },
    { id: 'HE170302', name: 'Nguyễn Văn Hùng' },
    { id: 'HE170303', name: 'Trần Minh Quân' }
  ]
}

// Danh sách ca thi
const examSessions = ref([
  {
    id: 101,
    examName: 'Thi giữa kỳ Spring 2026',
    semester: 'Spring 2026',
    subjectCode: 'PRN211',
    subjectName: 'Lập trình C# (.NET)',
    className: 'SE1701',
    startTime: '2026-06-08 08:00',
    endTime: '2026-06-08 09:30',
    status: 'Open', // Scheduled, Open, Closed, Published
    violationCount: 2
  },
  {
    id: 102,
    examName: 'Thi giữa kỳ Spring 2026',
    semester: 'Spring 2026',
    subjectCode: 'SWD392',
    subjectName: 'Thiết kế & Kiến trúc phần mềm',
    className: 'SE1702',
    startTime: '2026-06-08 10:00',
    endTime: '2026-06-08 11:30',
    status: 'Scheduled',
    violationCount: 0
  },
  {
    id: 103,
    examName: 'Thi cuối kỳ Fall 2025',
    semester: 'Fall 2025',
    subjectCode: 'PRN221',
    subjectName: 'Lập trình ứng dụng Windows',
    className: 'SE1703',
    startTime: '2025-12-15 13:30',
    endTime: '2025-12-15 15:00',
    status: 'Published',
    violationCount: 4
  },
  {
    id: 104,
    examName: 'Thi thử Summer 2026',
    semester: 'Summer 2026',
    subjectCode: 'SWE201c',
    subjectName: 'Nhập môn kỹ nghệ phần mềm',
    className: 'IA1701',
    startTime: '2026-07-10 15:30',
    endTime: '2026-07-10 17:00',
    status: 'Closed',
    violationCount: 1
  },
  {
    id: 105,
    examName: 'Thi giữa kỳ Spring 2026',
    semester: 'Spring 2026',
    subjectCode: 'PRN211',
    subjectName: 'Lập trình C# (.NET)',
    className: 'SE1702',
    startTime: '2026-06-08 08:00',
    endTime: '2026-06-08 09:30',
    status: 'Closed',
    violationCount: 0
  }
])

// Chi tiết vi phạm theo ca thi (Append-only)
const violations = ref([
  {
    id: 1,
    sessionId: 101,
    studentId: 'HE170001',
    studentName: 'Nguyễn Văn An',
    type: 'Chuyển Tab',
    time: '2026-06-08 08:15:22',
    description: 'Rời khỏi màn hình thi 4 lần, chuyển tab trình duyệt sang Google Search.',
    status: 'Đang rà soát'
  },
  {
    id: 2,
    sessionId: 101,
    studentId: 'HE170002',
    studentName: 'Trần Thị Bình',
    type: 'Nhiều Khuôn Mặt',
    time: '2026-06-08 08:42:10',
    description: 'Hệ thống AI phát hiện 2 khuôn mặt xuất hiện trong khung camera giám sát.',
    status: 'Đang rà soát'
  },
  {
    id: 3,
    sessionId: 103,
    studentId: 'HE170201',
    studentName: 'Vũ Hoàng Nam',
    type: 'Mất Kết Nối',
    time: '2025-12-15 13:45:00',
    description: 'Mất kết nối camera giám sát kéo dài hơn 5 phút không lý do.',
    status: 'Đã xử lý'
  },
  {
    id: 4,
    sessionId: 104,
    studentId: 'HE170301',
    studentName: 'Bùi Gia Bảo',
    type: 'Khác',
    time: '2026-07-10 16:10:05',
    description: 'Có âm thanh trò chuyện lớn liên tục xung quanh vị trí làm bài.',
    status: 'Đang rà soát'
  }
])

// Audit Logs hoạt động điều phối thi
const auditLogs = ref([
  {
    id: 1,
    time: '2026-06-08 14:02:15',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Mở ca thi',
    details: 'Đã kích hoạt ca thi giữa kỳ PRN211 - SE1701 sang trạng thái Open',
    reason: 'Đến giờ thi theo lịch điều phối'
  },
  {
    id: 2,
    time: '2026-06-08 09:40:00',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Đóng ca thi',
    details: 'Đã đóng ca thi giữa kỳ PRN211 - SE1702 sang trạng thái Closed',
    reason: 'Hết giờ làm bài chính thức'
  },
  {
    id: 3,
    time: '2026-06-08 08:30:10',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Ghi nhận vi phạm',
    details: 'Thêm vi phạm thủ công cho sinh viên HE170002 trong ca thi 101',
    reason: 'Giám thị phòng thi ghi nhận gian lận trực tiếp'
  }
])

// --- State Bộ lọc ---
const searchQuery = ref('')
const selectedSemester = ref('all')
const selectedSubject = ref('all')
const selectedClass = ref('all')
const selectedStatus = ref('all')

// --- Thống kê KPI ---
const totalSessions = computed(() => examSessions.value.length)
const openSessionsCount = computed(() => examSessions.value.filter(s => s.status === 'Open').length)
const totalViolationsCount = computed(() => violations.value.length)
const publishedSessionsCount = computed(() => examSessions.value.filter(s => s.status === 'Published').length)
const publishedPercentage = computed(() => {
  if (totalSessions.value === 0) return 0
  return Math.round((publishedSessionsCount.value / totalSessions.value) * 100)
})

// --- Lọc danh sách ca thi ---
const filteredSessions = computed(() => {
  return examSessions.value.filter(s => {
    const matchSearch = s.examName.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                        s.subjectName.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                        s.className.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchSemester = selectedSemester.value === 'all' || s.semester === selectedSemester.value
    const matchSubject = selectedSubject.value === 'all' || s.subjectCode === selectedSubject.value
    const matchClass = selectedClass.value === 'all' || s.className === selectedClass.value
    const matchStatus = selectedStatus.value === 'all' || s.status === selectedStatus.value

    return matchSearch && matchSemester && matchSubject && matchClass && matchStatus
  })
})

// --- Trạng thái badge helper ---
const getStatusBadge = (status) => {
  switch (status) {
    case 'Scheduled':
      return { class: 'bg-amber-50 text-amber-700 border-amber-200/50 dark:bg-amber-600/10 dark:text-amber-400 dark:border-amber-500/20', label: 'Đã lên lịch' }
    case 'Open':
      return { class: 'bg-emerald-50 text-emerald-700 border-emerald-200/50 dark:bg-emerald-600/10 dark:text-emerald-400 dark:border-emerald-500/20', label: 'Đang mở' }
    case 'Closed':
      return { class: 'bg-rose-50 text-rose-700 border-rose-200/50 dark:bg-rose-600/10 dark:text-rose-400 dark:border-rose-500/20', label: 'Đã đóng' }
    case 'Published':
      return { class: 'bg-indigo-50 text-indigo-700 border-indigo-200/50 dark:bg-indigo-600/10 dark:text-indigo-400 dark:border-indigo-500/20', label: 'Đã công bố' }
    default:
      return { class: 'bg-slate-100 text-slate-700', label: status }
  }
}

// --- State Modal Xác nhận (Confirm Action Modal) ---
const isConfirmModalOpen = ref(false)
const confirmTitle = ref('')
const confirmMessage = ref('')
const actionType = ref('') // 'open', 'close', 'publish'
const activeSession = ref(null)
const confirmReason = ref('')
const confirmCommit = ref(false) // Cam kết trách nhiệm khi công bố kết quả

const openConfirmModal = (session, type) => {
  activeSession.value = session
  actionType.value = type
  confirmReason.value = ''
  confirmCommit.value = false

  if (type === 'open') {
    confirmTitle.value = 'Xác nhận mở ca thi'
    confirmMessage.value = `Bạn có chắc chắn muốn MỞ ca thi môn "${session.subjectName}" lớp "${session.className}"? Sinh viên sẽ được phép truy cập đề thi và làm bài.`
  } else if (type === 'close') {
    confirmTitle.value = 'Xác nhận đóng ca thi'
    confirmMessage.value = `Bạn có chắc chắn muốn ĐÓNG ca thi môn "${session.subjectName}" lớp "${session.className}"? Mọi sinh viên chưa nộp bài sẽ bị dừng làm bài thi lập tức.`
  } else if (type === 'publish') {
    confirmTitle.value = 'Công bố kết quả thi'
    confirmMessage.value = `Bạn có chắc chắn muốn CÔNG BỐ điểm thi của ca thi môn "${session.subjectName}" lớp "${session.className}"? Điểm số sẽ hiển thị trực tiếp trên tài khoản của sinh viên.`
  }

  isConfirmModalOpen.value = true
}

const handleConfirmAction = () => {
  if (!activeSession.value) return

  const targetSession = examSessions.value.find(s => s.id === activeSession.value.id)
  if (!targetSession) return

  const timeString = new Date().toLocaleString('vi-VN')

  if (actionType.value === 'open') {
    targetSession.status = 'Open'
    auditLogs.value.unshift({
      id: auditLogs.value.length + 1,
      time: timeString,
      actor: 'Super Admin (admin@fpt.edu.vn)',
      action: 'Mở ca thi',
      details: `Kích hoạt trạng thái Open cho ca thi ID ${targetSession.id} (${targetSession.subjectCode} - ${targetSession.className})`,
      reason: confirmReason.value || 'Điều phối vận hành hệ thống'
    })
  } else if (actionType.value === 'close') {
    targetSession.status = 'Closed'
    auditLogs.value.unshift({
      id: auditLogs.value.length + 1,
      time: timeString,
      actor: 'Super Admin (admin@fpt.edu.vn)',
      action: 'Đóng ca thi',
      details: `Đóng ca thi ID ${targetSession.id} (${targetSession.subjectCode} - ${targetSession.className}), kết thúc thời gian thi`,
      reason: confirmReason.value || 'Hết giờ làm bài hoặc kết thúc sớm'
    })
  } else if (actionType.value === 'publish') {
    targetSession.status = 'Published'
    auditLogs.value.unshift({
      id: auditLogs.value.length + 1,
      time: timeString,
      actor: 'Super Admin (admin@fpt.edu.vn)',
      action: 'Công bố kết quả',
      details: `Công bố điểm thi ca ID ${targetSession.id} (${targetSession.subjectCode} - ${targetSession.className})`,
      reason: confirmReason.value || 'Phê duyệt điểm số và công khai kết quả học vụ'
    })
  }

  isConfirmModalOpen.value = false
}

// --- State Drawer Vi phạm (Violation Drawer) ---
const isViolationDrawerOpen = ref(false)
const drawerSession = ref(null)
const drawerActiveTab = ref('list') // 'list', 'add'

// Form thêm vi phạm mới
const vForm = ref({
  studentId: '',
  type: 'Chuyển Tab',
  description: ''
})

const vErrors = ref({
  studentId: '',
  description: ''
})

const currentClassStudents = computed(() => {
  if (!drawerSession.value) return []
  return studentsByClass[drawerSession.value.className] || []
})

const currentSessionViolations = computed(() => {
  if (!drawerSession.value) return []
  return violations.value.filter(v => v.sessionId === drawerSession.value.id)
})

const openViolationDrawer = (session) => {
  drawerSession.value = session
  drawerActiveTab.value = 'list'
  resetVForm()
  isViolationDrawerOpen.value = true
}

const resetVForm = () => {
  vForm.value = {
    studentId: '',
    type: 'Chuyển Tab',
    description: ''
  }
  vErrors.value = {
    studentId: '',
    description: ''
  }
}

// Validate Real-time cho form vi phạm
watch(vForm, (newVal) => {
  vErrors.value = { studentId: '', description: '' }
  if (drawerActiveTab.value === 'add') {
    if (!newVal.studentId) {
      vErrors.value.studentId = 'Vui lòng chọn sinh viên vi phạm.'
    }
    if (!newVal.description.trim()) {
      vErrors.value.description = 'Vui lòng nhập mô tả chi tiết hành vi vi phạm.'
    } else if (newVal.description.trim().length < 10) {
      vErrors.value.description = 'Mô tả chi tiết phải có ít nhất 10 ký tự.'
    }
  }
}, { deep: true })

const isVFormValid = computed(() => {
  return vForm.value.studentId &&
         vForm.value.description.trim() &&
         vForm.value.description.trim().length >= 10
})

// Xử lý thêm vi phạm mới (Append-only, không sửa/xóa)
const handleAddViolation = () => {
  if (!isVFormValid.value || !drawerSession.value) return

  const student = currentClassStudents.value.find(s => s.id === vForm.value.studentId)
  if (!student) return

  const timeString = new Date().toLocaleString('vi-VN')
  const newViolation = {
    id: violations.value.length + 1,
    sessionId: drawerSession.value.id,
    studentId: student.id,
    studentName: student.name,
    type: vForm.value.type,
    time: timeString,
    description: vForm.value.description.trim(),
    status: 'Đang rà soát'
  }

  // Thêm vào danh sách vi phạm
  violations.value.push(newViolation)

  // Cập nhật số lượng vi phạm trong bảng ca thi
  const sessionInTable = examSessions.value.find(s => s.id === drawerSession.value.id)
  if (sessionInTable) {
    sessionInTable.violationCount += 1
  }

  // Ghi Audit Log
  auditLogs.value.unshift({
    id: auditLogs.value.length + 1,
    time: timeString,
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Ghi nhận vi phạm',
    details: `Thêm vi phạm thủ công [${vForm.value.type}] cho SV ${student.name} (${student.id}) tại ca thi ID ${drawerSession.value.id}`,
    reason: `Ghi nhận giám thị trực tiếp: ${vForm.value.description.trim()}`
  })

  // Trở lại tab danh sách
  drawerActiveTab.value = 'list'
  resetVForm()
}
</script>

<template>
  <div class="min-h-screen lg-app-bg text-heading font-sans relative pb-12">
    <!-- Quả cầu trang trí 3D mờ ảo -->
    <div class="lg-shell-orbs">
      <div class="lg-shell-orb lg-shell-orb-primary"></div>
      <div class="lg-shell-orb lg-shell-orb-secondary"></div>
    </div>

    <div class="lg-shell-content mx-auto relative z-10">
      <!-- Header Trang -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
        <div>
          <h1 class="text-2xl md:text-3xl font-extrabold tracking-tight text-heading flex items-center gap-3">
            <Calendar class="w-8 h-8 text-primary" />
            Mở/Đóng Giai Đoạn Thi
          </h1>
          <p class="text-sm text-muted mt-1">
            Điều phối ca thi trực tuyến, quản lý trạng thái, giám sát vi phạm quy chế và phê duyệt công bố điểm thi.
          </p>
        </div>

        <div class="flex items-center gap-2">
          <span class="lg-badge lg-badge-primary">
            <Info class="w-3.5 h-3.5" />
            Mỗi bài thi chỉ được phép nộp 1 lần duy nhất
          </span>
        </div>
      </div>

      <!-- KPI Dashboard Mini -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <!-- KPI 1 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-primary/10 flex items-center justify-center text-primary">
            <Calendar class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Tổng ca thi</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ totalSessions }}</div>
          </div>
        </div>

        <!-- KPI 2 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500">
            <Play class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Đang mở</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ openSessionsCount }}</div>
          </div>
        </div>

        <!-- KPI 3 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-rose-500/10 flex items-center justify-center text-rose-500">
            <AlertTriangle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Tổng vi phạm</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ totalViolationsCount }}</div>
          </div>
        </div>

        <!-- KPI 4 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-indigo-500/10 flex items-center justify-center text-indigo-500">
            <CheckCircle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Công bố điểm</div>
            <div class="text-2xl font-bold mt-0.5 text-heading flex items-baseline gap-1.5">
              <span>{{ publishedSessionsCount }}</span>
              <span class="text-xs font-normal text-muted">({{ publishedPercentage }}%)</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Khung Bộ Lọc & Tìm Kiếm -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center gap-2 mb-4 pb-3 border-b border-default">
          <Filter class="w-4 h-4 text-primary" />
          <h3 class="font-bold text-heading text-sm">Bộ lọc tìm kiếm ca thi</h3>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-3 lg:grid-cols-6 gap-3">
          <!-- Tìm kiếm văn bản -->
          <div class="relative lg:col-span-2 sm:col-span-2">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-placeholder" />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Tìm theo kỳ thi, môn học, lớp..."
              class="w-full pl-9 pr-4 lg-input text-sm"
            />
          </div>

          <!-- Lọc Học kỳ -->
          <div>
            <select v-model="selectedSemester" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả học kỳ</option>
              <option v-for="sem in semesters" :key="sem" :value="sem">{{ sem }}</option>
            </select>
          </div>

          <!-- Lọc Môn học -->
          <div>
            <select v-model="selectedSubject" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả môn học</option>
              <option v-for="sub in subjects" :key="sub.code" :value="sub.code">
                [{{ sub.code }}] {{ sub.name }}
              </option>
            </select>
          </div>

          <!-- Lọc Lớp học phần -->
          <div>
            <select v-model="selectedClass" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả lớp học phần</option>
              <option v-for="cls in classSections" :key="cls" :value="cls">{{ cls }}</option>
            </select>
          </div>

          <!-- Lọc Trạng thái ca thi -->
          <div>
            <select v-model="selectedStatus" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả trạng thái</option>
              <option value="Scheduled">Đã lên lịch</option>
              <option value="Open">Đang mở</option>
              <option value="Closed">Đã đóng</option>
              <option value="Published">Đã công bố</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Bảng ca thi (Exam Session Table) -->
      <div class="lg-table-shell overflow-x-auto mb-8">
        <table class="min-w-full divide-y divide-default">
          <thead>
            <tr class="surface-table-header">
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase tracking-wider">Mã ca</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase tracking-wider">Kỳ thi / Học kỳ</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase tracking-wider">Môn học & Lớp</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase tracking-wider">Thời gian thi</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase tracking-wider">Trạng thái</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase tracking-wider">Vi phạm</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase tracking-wider">Hành động</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-if="filteredSessions.length === 0" class="hover:bg-transparent">
              <td colspan="7" class="px-4 py-12 text-center text-muted">
                <div class="flex flex-col items-center justify-center gap-2">
                  <AlertCircle class="w-8 h-8 text-placeholder" />
                  <span class="font-medium text-sm">Không tìm thấy ca thi nào phù hợp với bộ lọc.</span>
                </div>
              </td>
            </tr>

            <tr v-for="session in filteredSessions" :key="session.id" class="transition-colors duration-150">
              <!-- Mã ca thi -->
              <td class="px-4 py-3 text-sm font-semibold text-heading">
                #{{ session.id }}
              </td>

              <!-- Kỳ thi / Học kỳ -->
              <td class="px-4 py-3 text-sm">
                <div class="font-bold text-heading">{{ session.examName }}</div>
                <div class="text-xs text-muted flex items-center gap-1 mt-0.5">
                  <Calendar class="w-3.5 h-3.5 text-placeholder" />
                  {{ session.semester }}
                </div>
              </td>

              <!-- Môn học & Lớp học phần -->
              <td class="px-4 py-3 text-sm">
                <div class="font-bold text-heading">[{{ session.subjectCode }}] {{ session.subjectName }}</div>
                <div class="text-xs text-primary font-bold mt-0.5">Lớp: {{ session.className }}</div>
              </td>

              <!-- Thời gian thi -->
              <td class="px-4 py-3 text-sm">
                <div class="flex items-center gap-1.5 text-body">
                  <Clock class="w-4 h-4 text-placeholder" />
                  <span>{{ session.startTime.split(' ')[1] }} - {{ session.endTime.split(' ')[1] }}</span>
                </div>
                <div class="text-xs text-muted mt-0.5">{{ session.startTime.split(' ')[0] }}</div>
              </td>

              <!-- Trạng thái -->
              <td class="px-4 py-3 text-center text-sm">
                <span class="lg-badge" :class="getStatusBadge(session.status).class">
                  <span class="w-1.5 h-1.5 rounded-full" 
                        :class="session.status === 'Open' ? 'bg-emerald-500' : 
                                session.status === 'Scheduled' ? 'bg-amber-500' : 
                                session.status === 'Closed' ? 'bg-rose-500' : 'bg-indigo-500'"></span>
                  {{ getStatusBadge(session.status).label }}
                </span>
              </td>

              <!-- Vi phạm -->
              <td class="px-4 py-3 text-center text-sm">
                <button 
                  @click="openViolationDrawer(session)"
                  class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-bold transition-transform active:scale-95 border"
                  :class="session.violationCount > 0 
                    ? 'bg-rose-50 border-rose-200 text-rose-700 hover:bg-rose-100 dark:bg-rose-600/10 dark:border-rose-500/20 dark:text-rose-400' 
                    : 'bg-slate-50 border-slate-200 text-slate-500 hover:bg-slate-100 dark:bg-slate-800 dark:border-slate-700 dark:text-slate-400'"
                >
                  <AlertTriangle class="w-3.5 h-3.5" />
                  <span>{{ session.violationCount }} vi phạm</span>
                </button>
              </td>

              <!-- Nút hành động -->
              <td class="px-4 py-3 text-center text-sm">
                <div class="flex items-center justify-center gap-1.5">
                  <!-- Mở ca thi -->
                  <button
                    v-if="session.status === 'Scheduled'"
                    @click="openConfirmModal(session, 'open')"
                    class="lg-btn-success text-xs px-2.5 py-1.5 flex items-center gap-1 font-bold"
                  >
                    <Play class="w-3.5 h-3.5" />
                    Mở ca thi
                  </button>

                  <!-- Đóng ca thi -->
                  <button
                    v-if="session.status === 'Open'"
                    @click="openConfirmModal(session, 'close')"
                    class="lg-btn-danger text-xs px-2.5 py-1.5 flex items-center gap-1 font-bold bg-rose-500 hover:bg-rose-600 text-white border-0 shadow-sm"
                  >
                    <Square class="w-3.5 h-3.5" />
                    Đóng ca thi
                  </button>

                  <!-- Công bố điểm -->
                  <button
                    v-if="session.status === 'Closed'"
                    @click="openConfirmModal(session, 'publish')"
                    class="lg-btn-primary text-xs px-2.5 py-1.5 flex items-center gap-1 font-bold"
                  >
                    <CheckCircle class="w-3.5 h-3.5" />
                    Công bố kết quả
                  </button>

                  <!-- Xem vi phạm -->
                  <button
                    @click="openViolationDrawer(session)"
                    class="lg-btn-secondary text-xs px-2.5 py-1.5 flex items-center gap-1"
                    title="Xem chi tiết vi phạm"
                  >
                    <Eye class="w-3.5 h-3.5" />
                    Chi tiết
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Audit Logs Panel -->
      <div class="lg-glass-soft lg-card lg-density-normal">
        <div class="flex items-center justify-between border-b border-default pb-3 mb-4">
          <div class="flex items-center gap-2">
            <History class="w-5 h-5 text-primary" />
            <h3 class="font-extrabold text-heading text-base">Nhật ký hoạt động (Audit Logs)</h3>
          </div>
          <span class="text-xs text-muted">Module M4 - Đã ghi nhận {{ auditLogs.length }} sự kiện</span>
        </div>

        <div class="space-y-3 max-h-60 overflow-y-auto pr-2">
          <div 
            v-for="log in auditLogs" 
            :key="log.id" 
            class="p-3 rounded-xl border border-default bg-surface-card flex flex-col sm:flex-row sm:items-center justify-between gap-3 text-sm"
          >
            <div class="space-y-1">
              <div class="flex items-center gap-2">
                <span class="font-bold text-primary">{{ log.action }}</span>
                <span class="text-xs text-muted">{{ log.time }}</span>
              </div>
              <p class="text-body font-medium">{{ log.details }}</p>
              <div v-if="log.reason" class="text-xs text-muted italic">
                Lý do: {{ log.reason }}
              </div>
            </div>
            <div class="text-xs font-semibold text-muted text-right">
              {{ log.actor }}
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Confirm Modal -->
    <div 
      v-if="isConfirmModalOpen" 
      class="fixed inset-0 z-[100] flex items-center justify-center p-4 lg-mobile-scrim"
    >
      <div class="lg-glass-strong surface-modal border border-default rounded-2xl w-full max-w-md p-6 shadow-2xl relative animate-in fade-in zoom-in-95 duration-200">
        <!-- Close button -->
        <button 
          @click="isConfirmModalOpen = false" 
          class="absolute top-4 right-4 p-1 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 text-muted transition-colors"
        >
          <X class="w-5 h-5" />
        </button>

        <div class="flex items-center gap-3 border-b border-default pb-3 mb-4">
          <AlertCircle class="w-6 h-6 text-primary" />
          <h3 class="text-lg font-extrabold text-heading">{{ confirmTitle }}</h3>
        </div>

        <p class="text-sm text-body leading-relaxed mb-4">
          {{ confirmMessage }}
        </p>

        <!-- Trọng số nghiệp vụ: lý do thao tác -->
        <div class="mb-4">
          <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
            Lý do thực hiện <span class="text-rose-500">*</span>
          </label>
          <textarea
            v-model="confirmReason"
            rows="3"
            placeholder="Nhập lý do điều phối của bạn..."
            class="w-full p-2.5 text-sm lg-input"
          ></textarea>
        </div>

        <!-- Cam kết nếu là Công bố kết quả -->
        <div v-if="actionType === 'publish'" class="mb-4 p-3 bg-amber-50 dark:bg-amber-950/20 border border-amber-200 dark:border-amber-900/40 rounded-xl flex items-start gap-2.5">
          <input 
            v-model="confirmCommit"
            type="checkbox"
            id="commitCheck"
            class="mt-1 w-4 h-4 rounded text-primary border-default focus:ring-primary"
          />
          <label for="commitCheck" class="text-xs text-amber-800 dark:text-amber-300 font-semibold leading-snug cursor-pointer select-none">
            Tôi xác nhận rằng điểm số đã được chấm chính xác và đồng ý công bố ngay lập tức cho sinh viên (không thể rút lại).
          </label>
        </div>

        <!-- Buttons -->
        <div class="flex items-center justify-end gap-2 mt-6">
          <button 
            @click="isConfirmModalOpen = false" 
            class="lg-btn-secondary px-4 py-2 text-sm font-semibold"
          >
            Hủy bỏ
          </button>
          <button 
            @click="handleConfirmAction" 
            :disabled="!confirmReason.trim() || (actionType === 'publish' && !confirmCommit)"
            class="lg-btn-primary px-4 py-2 text-sm font-semibold disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Xác nhận
          </button>
        </div>
      </div>
    </div>

    <!-- Violation Drawer -->
    <div 
      v-if="isViolationDrawerOpen" 
      class="fixed inset-0 z-[90] flex justify-end lg-mobile-scrim"
      @click="isViolationDrawerOpen = false"
    >
      <div 
        class="w-full max-w-lg h-full lg-glass-strong surface-modal border-l border-default shadow-2xl flex flex-col p-6 animate-in slide-in-from-right duration-300"
        @click.stop
      >
        <!-- Drawer Header -->
        <div class="flex items-center justify-between border-b border-default pb-4 mb-4">
          <div>
            <h3 class="text-lg font-extrabold text-heading flex items-center gap-2">
              <AlertTriangle class="w-5 h-5 text-rose-500" />
              Chi Tiết Vi Phạm Ca Thi #{{ drawerSession?.id }}
            </h3>
            <p class="text-xs text-muted mt-1 font-semibold">
              Môn: {{ drawerSession?.subjectName }} - Lớp: {{ drawerSession?.className }}
            </p>
          </div>
          <button 
            @click="isViolationDrawerOpen = false" 
            class="p-1 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 text-muted transition-colors"
          >
            <X class="w-5 h-5" />
          </button>
        </div>

        <!-- Drawer Navigation Tabs -->
        <div class="flex border-b border-default mb-4">
          <button 
            @click="drawerActiveTab = 'list'"
            class="flex-1 py-2 text-center text-sm font-bold border-b-2 transition-colors"
            :class="drawerActiveTab === 'list' ? 'border-primary text-primary' : 'border-transparent text-muted hover:text-heading'"
          >
            Nhật ký vi phạm ({{ currentSessionViolations.length }})
          </button>
          <button 
            @click="drawerActiveTab = 'add'"
            class="flex-1 py-2 text-center text-sm font-bold border-b-2 transition-colors flex items-center justify-center gap-1"
            :class="drawerActiveTab === 'add' ? 'border-primary text-primary' : 'border-transparent text-muted hover:text-heading'"
          >
            <Plus class="w-4 h-4" />
            Ghi nhận vi phạm mới
          </button>
        </div>

        <!-- Tab Content -->
        <div class="flex-1 overflow-y-auto pr-1">
          <!-- TAB 1: Danh sách vi phạm (Append-only) -->
          <div v-if="drawerActiveTab === 'list'" class="space-y-4">
            <div 
              v-if="currentSessionViolations.length === 0" 
              class="text-center py-12 text-muted"
            >
              <Info class="w-10 h-10 text-placeholder mx-auto mb-2" />
              <p class="text-sm font-medium">Chưa phát hiện hoặc ghi nhận vi phạm nào trong ca thi này.</p>
            </div>

            <div 
              v-for="v in currentSessionViolations" 
              :key="v.id" 
              class="p-4 rounded-xl border border-default bg-surface-card space-y-2 relative"
            >
              <div class="flex items-center justify-between">
                <div>
                  <div class="font-bold text-heading text-sm">{{ v.studentName }}</div>
                  <div class="text-xs text-primary font-semibold">MSSV: {{ v.studentId }}</div>
                </div>
                <span class="lg-badge lg-badge-danger text-xs">
                  {{ v.type }}
                </span>
              </div>
              
              <p class="text-xs text-body font-medium leading-relaxed bg-slate-50 dark:bg-slate-800/40 p-2.5 rounded-lg border border-default/50">
                {{ v.description }}
              </p>

              <div class="flex items-center justify-between text-[11px] text-muted pt-1">
                <span class="flex items-center gap-1">
                  <Clock class="w-3.5 h-3.5 text-placeholder" />
                  {{ v.time }}
                </span>
                <span class="font-bold uppercase tracking-wider text-rose-500">
                  {{ v.status }}
                </span>
              </div>

              <!-- Cảnh báo append-only: Không hiển thị nút sửa/xóa -->
              <div class="absolute top-2 right-2 flex items-center gap-1 opacity-0 hover:opacity-100 transition-opacity bg-surface-modal px-2 py-1 rounded-md border border-default text-[10px] text-muted">
                Dữ liệu append-only (Chỉ đọc)
              </div>
            </div>
          </div>

          <!-- TAB 2: Thêm vi phạm mới (Append-only form) -->
          <div v-if="drawerActiveTab === 'add'" class="space-y-4">
            <div class="p-3 bg-amber-50 dark:bg-amber-950/20 border border-amber-200 dark:border-amber-900/40 rounded-xl flex items-start gap-2 text-xs text-amber-800 dark:text-amber-300 font-semibold leading-relaxed">
              <Info class="w-4 h-4 mt-0.5 flex-shrink-0" />
              <span>
                <strong>Lưu ý quan trọng:</strong> Nhật ký vi phạm được thiết lập ở chế độ <strong>append-only</strong> (chỉ cho phép ghi thêm). Mọi bản ghi sau khi lưu sẽ không thể chỉnh sửa hoặc xóa để đảm bảo tính trung thực tối cao của kỳ thi.
              </span>
            </div>

            <!-- Sinh viên vi phạm -->
            <div>
              <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
                Chọn sinh viên vi phạm <span class="text-rose-500">*</span>
              </label>
              <select v-model="vForm.studentId" class="w-full px-3 lg-control text-sm">
                <option value="">-- Chọn sinh viên thuộc lớp {{ drawerSession?.className }} --</option>
                <option v-for="student in currentClassStudents" :key="student.id" :value="student.id">
                  {{ student.name }} ({{ student.id }})
                </option>
              </select>
              <span v-if="vErrors.studentId" class="text-xs text-rose-500 mt-1 block">
                {{ vErrors.studentId }}
              </span>
            </div>

            <!-- Loại vi phạm -->
            <div>
              <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
                Loại vi phạm <span class="text-rose-500">*</span>
              </label>
              <select v-model="vForm.type" class="w-full px-3 lg-control text-sm">
                <option value="Chuyển Tab">Chuyển Tab / Rời màn hình</option>
                <option value="Mất Kết Nối">Mất kết nối camera/micro</option>
                <option value="Nhiều Khuôn Mặt">Phát hiện khuôn mặt thứ hai</option>
                <option value="Thiếu Khuôn Mặt">Không phát hiện khuôn mặt</option>
                <option value="Khác">Vi phạm khác (Mô tả chi tiết)</option>
              </select>
            </div>

            <!-- Mô tả chi tiết -->
            <div>
              <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
                Mô tả chi tiết vi phạm <span class="text-rose-500">*</span>
              </label>
              <textarea
                v-model="vForm.description"
                rows="4"
                placeholder="Mô tả cụ thể hành động bất thường, ví dụ: chuyển sang tab tìm kiếm Google lúc 08:15..."
                class="w-full p-2.5 text-sm lg-input"
              ></textarea>
              <span v-if="vErrors.description" class="text-xs text-rose-500 mt-1 block">
                {{ vErrors.description }}
              </span>
            </div>

            <!-- Buttons -->
            <div class="flex items-center justify-end gap-2 pt-4">
              <button 
                @click="drawerActiveTab = 'list'"
                class="lg-btn-secondary px-4 py-2 text-sm font-semibold"
              >
                Hủy bỏ
              </button>
              <button 
                @click="handleAddViolation" 
                :disabled="!isVFormValid"
                class="lg-btn-primary px-5 py-2 text-sm font-semibold flex items-center gap-1 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <Save class="w-4 h-4" />
                Lưu vi phạm
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Hiệu ứng chuyển động cho Modal & Drawer */
.animate-in {
  animation-duration: 200ms;
  animation-fill-mode: both;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}
</style>

<script setup>
/**
 * RegistrationPeriodsView.vue - Super Admin
 * Giao diện quản lý vòng đời đợt đăng ký môn học (Draft -> Open -> Closed -> Finalized),
 * cấu hình hạn mức tín chỉ, hạn hủy môn, quét sĩ số tối thiểu và quản lý danh sách chờ (Waitlist).
 */
import { ref, computed } from 'vue'
import {
  Calendar,
  Search,
  Plus,
  Play,
  Square,
  CheckCircle,
  Clock,
  AlertTriangle,
  Users,
  Edit2,
  Save,
  X,
  Filter,
  Info
} from 'lucide-vue-next'

// --- Mock Data ---

// Danh sách Cơ sở (Campus)
const campuses = ref([
  { id: 10, name: 'Cơ sở Hòa Lạc' },
  { id: 11, name: 'Cơ sở Detech' },
  { id: 12, name: 'Cơ sở TP.HCM' }
])

// Danh sách Học kỳ
const semesters = ref(['Spring 2026', 'Summer 2026', 'Fall 2025'])

// Danh sách các đợt đăng ký môn học
const registrationPeriods = ref([
  {
    id: 1,
    name: 'Đăng ký học phần chính thức Học kỳ Spring 2026',
    semester: 'Spring 2026',
    campusId: 10,
    campusName: 'Cơ sở Hòa Lạc',
    startDate: '2026-01-02',
    endDate: '2026-01-15',
    withdrawDeadline: '2026-01-20',
    maxCredits: 20,
    status: 'Open' // Draft, Open, Closed, Finalized
  },
  {
    id: 2,
    name: 'Đăng ký học phần bổ sung Học kỳ Spring 2026',
    semester: 'Spring 2026',
    campusId: 10,
    campusName: 'Cơ sở Hòa Lạc',
    startDate: '2026-01-18',
    endDate: '2026-01-22',
    withdrawDeadline: '2026-01-25',
    maxCredits: 8,
    status: 'Draft'
  },
  {
    id: 3,
    name: 'Đăng ký học phần Học kỳ Fall 2025',
    semester: 'Fall 2025',
    campusId: 10,
    campusName: 'Cơ sở Hòa Lạc',
    startDate: '2025-09-02',
    endDate: '2025-09-15',
    withdrawDeadline: '2025-09-20',
    maxCredits: 20,
    status: 'Finalized'
  },
  {
    id: 4,
    name: 'Đăng ký học phần Học kỳ Spring 2026 - TP.HCM',
    semester: 'Spring 2026',
    campusId: 12,
    campusName: 'Cơ sở TP.HCM',
    startDate: '2026-01-02',
    endDate: '2026-01-15',
    withdrawDeadline: '2026-01-20',
    maxCredits: 20,
    status: 'Closed'
  }
])

// Danh sách các lớp học phần có sĩ số dưới tối thiểu (< 15 sinh viên)
const lowEnrollmentClasses = ref([
  { classCode: 'PRN211_SE1701', subjectName: 'Lập trình C# (.NET)', currentSize: 12, minSize: 15 },
  { classCode: 'SWE201c_IA1701', subjectName: 'Nhập môn kỹ nghệ phần mềm', currentSize: 9, minSize: 15 },
  { classCode: 'SWD392_SE1703', subjectName: 'Thiết kế & Kiến trúc phần mềm', currentSize: 14, minSize: 15 }
])

// Mock dữ liệu hàng đợi Waitlist cho các lớp học phần
const waitlists = ref({
  'PRN211_SE1701': {
    maxSize: 30,
    currentSize: 30,
    students: [
      { id: 'HE170001', name: 'Nguyễn Văn An', joinedAt: '2026-01-05 08:30:15' },
      { id: 'HE170002', name: 'Trần Thị Bình', joinedAt: '2026-01-05 09:12:44' },
      { id: 'HE170003', name: 'Lê Văn Cường', joinedAt: '2026-01-05 10:05:22' }
    ]
  },
  'SWE201c_IA1701': {
    maxSize: 30,
    currentSize: 30,
    students: [
      { id: 'HE170102', name: 'Hoàng Minh Em', joinedAt: '2026-01-06 14:20:10' }
    ]
  },
  'SWD392_SE1703': {
    maxSize: 30,
    currentSize: 30,
    students: []
  }
})

// Nhật ký hoạt động điều phối (Audit Logs)
const auditLogs = ref([
  {
    id: 1,
    time: '2026-06-08 15:30:00',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Mở đợt đăng ký',
    details: 'Đã mở đợt đăng ký "Đăng ký học phần chính thức Học kỳ Spring 2026" - Hòa Lạc',
    reason: 'Bắt đầu giai đoạn đăng ký môn học chính thức'
  },
  {
    id: 2,
    time: '2026-06-05 09:15:30',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Gia hạn đợt đăng ký',
    details: 'Đã gia hạn đợt đăng ký Spring 2026 cơ sở TP.HCM thêm 3 ngày đến 2026-01-18',
    reason: 'Hỗ trợ nhóm sinh viên chưa kịp nộp học phí hoàn thiện đăng ký'
  },
  {
    id: 3,
    time: '2026-06-01 10:00:22',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Tạo đợt đăng ký',
    details: 'Khởi tạo đợt "Đăng ký học phần bổ sung Học kỳ Spring 2026" ở trạng thái Draft',
    reason: 'Kế hoạch đào tạo kỳ học phụ'
  }
])

// --- State Bộ lọc ---
const searchQuery = ref('')
const selectedCampus = ref('all')
const selectedSemester = ref('Spring 2026')
const selectedStatus = ref('all')

// --- Thống kê KPI ---
const totalPeriodsCount = computed(() => registrationPeriods.value.length)
const activePeriodsCount = computed(() => registrationPeriods.value.filter(p => p.status === 'Open').length)
const lowEnrollmentClassesCount = computed(() => lowEnrollmentClasses.value.length)
const totalWaitlistCount = computed(() => {
  return Object.values(waitlists.value).reduce((sum, item) => sum + item.students.length, 0)
})

// --- Lọc danh sách đợt đăng ký ---
const filteredPeriods = computed(() => {
  return registrationPeriods.value.filter(p => {
    const matchSearch = p.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchCampus = selectedCampus.value === 'all' || p.campusId === parseInt(selectedCampus.value)
    const matchSemester = selectedSemester.value === 'all' || p.semester === selectedSemester.value
    const matchStatus = selectedStatus.value === 'all' || p.status === selectedStatus.value

    return matchSearch && matchCampus && matchSemester && matchStatus
  })
})

// --- Trạng thái badge helper ---
const getStatusBadge = (status) => {
  switch (status) {
    case 'Draft':
      return { class: 'bg-amber-50 text-amber-700 border-amber-200/50 dark:bg-amber-600/10 dark:text-amber-400 dark:border-amber-500/20', label: 'Bản nháp' }
    case 'Open':
      return { class: 'bg-emerald-50 text-emerald-700 border-emerald-200/50 dark:bg-emerald-600/10 dark:text-emerald-400 dark:border-emerald-500/20', label: 'Đang mở' }
    case 'Closed':
      return { class: 'bg-rose-50 text-rose-700 border-rose-200/50 dark:bg-rose-600/10 dark:text-rose-400 dark:border-rose-500/20', label: 'Đã đóng' }
    case 'Finalized':
      return { class: 'bg-indigo-50 text-indigo-700 border-indigo-200/50 dark:bg-indigo-600/10 dark:text-indigo-400 dark:border-indigo-500/20', label: 'Đã chốt' }
    default:
      return { class: 'bg-slate-100 text-slate-700', label: status }
  }
}

// --- State Form Modal (Tạo / Sửa / Gia hạn đợt đăng ký) ---
const isFormModalOpen = ref(false)
const isEditMode = ref(false)
const isExtendMode = ref(false)
const formErrors = ref({ dateRange: '', withdrawDate: '', name: '', maxCredits: '' })

const form = ref({
  id: null,
  name: '',
  semester: 'Spring 2026',
  campusId: '',
  startDate: '',
  endDate: '',
  withdrawDeadline: '',
  maxCredits: 20
})

const openCreateModal = () => {
  isEditMode.value = false
  isExtendMode.value = false
  formErrors.value = { dateRange: '', withdrawDate: '', name: '', maxCredits: '' }
  form.value = {
    id: null,
    name: '',
    semester: selectedSemester.value === 'all' ? 'Spring 2026' : selectedSemester.value,
    campusId: campuses.value[0]?.id || '',
    startDate: '',
    endDate: '',
    withdrawDeadline: '',
    maxCredits: 20
  }
  isFormModalOpen.value = true
}

const openEditModal = (period) => {
  isEditMode.value = true
  isExtendMode.value = false
  formErrors.value = { dateRange: '', withdrawDate: '', name: '', maxCredits: '' }
  form.value = { ...period }
  isFormModalOpen.value = true
}

const openExtendModal = (period) => {
  isEditMode.value = false
  isExtendMode.value = true
  formErrors.value = { dateRange: '', withdrawDate: '', name: '', maxCredits: '' }
  form.value = { ...period }
  isFormModalOpen.value = true
}

const validateForm = () => {
  formErrors.value = { dateRange: '', withdrawDate: '', name: '', maxCredits: '' }
  let isValid = true

  if (!form.value.name.trim()) {
    formErrors.value.name = 'Tên đợt đăng ký không được để trống.'
    isValid = false
  }

  if (form.value.maxCredits <= 0) {
    formErrors.value.maxCredits = 'Hạn mức tín chỉ phải lớn hơn 0.'
    isValid = false
  }

  if (form.value.startDate && form.value.endDate) {
    const start = new Date(form.value.startDate)
    const end = new Date(form.value.endDate)
    if (start >= end) {
      formErrors.value.dateRange = 'Ngày bắt đầu phải trước ngày kết thúc.'
      isValid = false
    }

    if (form.value.withdrawDeadline) {
      const withdraw = new Date(form.value.withdrawDeadline)
      if (withdraw < start) {
        formErrors.value.withdrawDate = 'Hạn hủy môn phải bằng hoặc sau ngày bắt đầu đăng ký.'
        isValid = false
      }
    }
  } else {
    formErrors.value.dateRange = 'Vui lòng điền đầy đủ thời gian bắt đầu và kết thúc.'
    isValid = false
  }

  return isValid
}

const handleSavePeriod = () => {
  if (!validateForm()) return

  const timeString = new Date().toLocaleString('vi-VN')
  const campus = campuses.value.find(c => c.id === parseInt(form.value.campusId))
  const campusName = campus ? campus.name : ''

  if (isExtendMode.value) {
    // Gia hạn thời gian đăng ký
    const target = registrationPeriods.value.find(p => p.id === form.value.id)
    if (target) {
      const oldEnd = target.endDate
      target.endDate = form.value.endDate
      target.withdrawDeadline = form.value.withdrawDeadline

      auditLogs.value.unshift({
        id: auditLogs.value.length + 1,
        time: timeString,
        actor: 'Super Admin (admin@fpt.edu.vn)',
        action: 'Gia hạn đăng ký',
        details: `Gia hạn đợt đăng ký ID ${target.id} từ kết thúc ngày ${oldEnd} đến ngày ${target.endDate}`,
        reason: `Điều phối gia hạn: Hạn hủy môn cập nhật thành ${target.withdrawDeadline}`
      })
    }
  } else if (isEditMode.value) {
    // Chỉnh sửa đợt đăng ký
    const idx = registrationPeriods.value.findIndex(p => p.id === form.value.id)
    if (idx !== -1) {
      registrationPeriods.value[idx] = {
        ...form.value,
        campusName
      }
      auditLogs.value.unshift({
        id: auditLogs.value.length + 1,
        time: timeString,
        actor: 'Super Admin (admin@fpt.edu.vn)',
        action: 'Chỉnh sửa đợt đăng ký',
        details: `Cập nhật thông tin cấu hình đợt đăng ký ID ${form.value.id}`,
        reason: 'Chỉnh sửa kế hoạch đào tạo'
      })
    }
  } else {
    // Tạo mới đợt đăng ký
    const newId = registrationPeriods.value.length > 0 ? Math.max(...registrationPeriods.value.map(p => p.id)) + 1 : 1
    registrationPeriods.value.push({
      ...form.value,
      id: newId,
      campusName,
      status: 'Draft'
    })
    auditLogs.value.unshift({
      id: auditLogs.value.length + 1,
      time: timeString,
      actor: 'Super Admin (admin@fpt.edu.vn)',
      action: 'Tạo đợt đăng ký',
      details: `Khởi tạo đợt đăng ký mới "${form.value.name}" cơ sở ${campusName} ở trạng thái Draft`,
      reason: 'Thiết lập kế hoạch đăng ký tín chỉ mới'
    })
  }

  isFormModalOpen.value = false
}

// --- State Confirm Status Modal ---
const isStatusModalOpen = ref(false)
const periodToChange = ref(null)
const targetStatus = ref('') // 'Open', 'Closed', 'Finalized'
const changeReason = ref('')

const openStatusModal = (period, status) => {
  periodToChange.value = period
  targetStatus.value = status
  changeReason.value = ''
  isStatusModalOpen.value = true
}

const handleConfirmStatus = () => {
  if (!periodToChange.value) return

  const target = registrationPeriods.value.find(p => p.id === periodToChange.value.id)
  if (!target) return

  const timeString = new Date().toLocaleString('vi-VN')

  if (targetStatus.value === 'Open') {
    target.status = 'Open'
    auditLogs.value.unshift({
      id: auditLogs.value.length + 1,
      time: timeString,
      actor: 'Super Admin (admin@fpt.edu.vn)',
      action: 'Mở đợt đăng ký',
      details: `Kích hoạt trạng thái Open cho đợt đăng ký ID ${target.id}`,
      reason: changeReason.value || 'Đến thời gian mở đăng ký học phần'
    })
  } else if (targetStatus.value === 'Closed') {
    target.status = 'Closed'
    auditLogs.value.unshift({
      id: auditLogs.value.length + 1,
      time: timeString,
      actor: 'Super Admin (admin@fpt.edu.vn)',
      action: 'Đóng đợt đăng ký',
      details: `Thay đổi trạng thái Closed cho đợt đăng ký ID ${target.id}. Hệ thống đã tự động quét sĩ số tối thiểu.`,
      reason: changeReason.value || 'Kết thúc đợt đăng ký học phần'
    })
  } else if (targetStatus.value === 'Finalized') {
    target.status = 'Finalized'
    auditLogs.value.unshift({
      id: auditLogs.value.length + 1,
      time: timeString,
      actor: 'Super Admin (admin@fpt.edu.vn)',
      action: 'Chốt danh sách',
      details: `Chốt danh sách đăng ký đợt ID ${target.id}, đồng bộ dữ liệu đăng ký sang phân hệ TKB (Module M7)`,
      reason: changeReason.value || 'Hoàn tất đăng ký, đóng băng dữ liệu lớp học phần'
    })
  }

  isStatusModalOpen.value = false
}

// --- State Waitlist Drawer (Quản lý hàng đợi) ---
const isWaitlistDrawerOpen = ref(false)
const selectedClassCode = ref('')
const waitlistData = computed(() => {
  return waitlists.value[selectedClassCode.value] || { maxSize: 30, currentSize: 30, students: [] }
})

const openWaitlistDrawer = (classCode) => {
  selectedClassCode.value = classCode
  isWaitlistDrawerOpen.value = true
}

// Tăng sức chứa lớp học phần (Nghiệp vụ giải phóng Waitlist)
const handleIncreaseSize = () => {
  if (!selectedClassCode.value) return
  const data = waitlists.value[selectedClassCode.value]
  if (data) {
    const oldMax = data.maxSize
    data.maxSize += 5

    // Ghi Audit Log
    const timeString = new Date().toLocaleString('vi-VN')
    auditLogs.value.unshift({
      id: auditLogs.value.length + 1,
      time: timeString,
      actor: 'Super Admin (admin@fpt.edu.vn)',
      action: 'Điều chỉnh sức chứa',
      details: `Tăng sức chứa tối đa của lớp ${selectedClassCode.value} từ ${oldMax} lên ${data.maxSize}`,
      reason: 'Giải quyết tắc nghẽn hàng đợi đăng ký lớp chuyên ngành'
    })
  }
}

// Ghép sinh viên thủ công từ Waitlist vào lớp chính thức
const handleApproveWaitlistStudent = (student) => {
  if (!selectedClassCode.value) return
  const data = waitlists.value[selectedClassCode.value]
  if (data) {
    // Xóa sinh viên khỏi hàng đợi
    data.students = data.students.filter(s => s.id !== student.id)
    // Tăng sĩ số lớp chính thức lên 1
    data.currentSize += 1

    // Ghi Audit Log
    const timeString = new Date().toLocaleString('vi-VN')
    auditLogs.value.unshift({
      id: auditLogs.value.length + 1,
      time: timeString,
      actor: 'Super Admin (admin@fpt.edu.vn)',
      action: 'Ghép học sinh thủ công',
      details: `Chấp thuận ghép sinh viên ${student.name} (${student.id}) từ danh sách chờ vào lớp chính thức ${selectedClassCode.value}`,
      reason: 'Điều phối nhân lực đào tạo phê duyệt bổ sung'
    })
  }
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
            Mở/Đóng Đăng Ký Môn
          </h1>
          <p class="text-sm text-muted mt-1">
            Điều phối và cấu hình các đợt đăng ký môn học, hạn mức tín chỉ, hạn hủy môn, quét sĩ số tối thiểu và giải phóng Waitlist.
          </p>
        </div>

        <button
          @click="openCreateModal"
          class="lg-btn-primary px-4 py-2.5 text-sm font-bold flex items-center gap-2 self-start md:self-center"
        >
          <Plus class="w-4.5 h-4.5" />
          Tạo đợt đăng ký mới
        </button>
      </div>

      <!-- KPI Dashboard Mini -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <!-- KPI 1 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-primary/10 flex items-center justify-center text-primary">
            <Calendar class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Tổng số đợt</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ totalPeriodsCount }} đợt</div>
          </div>
        </div>

        <!-- KPI 2 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500">
            <Play class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Đang hoạt động</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ activePeriodsCount }} đợt</div>
          </div>
        </div>

        <!-- KPI 3 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-rose-500/10 flex items-center justify-center text-rose-500">
            <AlertTriangle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Lớp sĩ số thấp</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ lowEnrollmentClassesCount }} lớp</div>
          </div>
        </div>

        <!-- KPI 4 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-indigo-500/10 flex items-center justify-center text-indigo-500">
            <Users class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Hàng đợi chờ (Waitlist)</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ totalWaitlistCount }} SV</div>
          </div>
        </div>
      </div>

      <!-- Khung Bộ Lọc & Tìm Kiếm -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center gap-2 mb-4 pb-3 border-b border-default">
          <Filter class="w-4 h-4 text-primary" />
          <h3 class="font-bold text-heading text-sm">Bộ lọc tìm kiếm đợt đăng ký</h3>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3">
          <!-- Tìm kiếm văn bản -->
          <div class="relative lg:col-span-2">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-placeholder" />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Tìm theo tên đợt đăng ký..."
              class="w-full pl-9 pr-4 lg-input text-sm"
            />
          </div>

          <!-- Chọn Cơ sở -->
          <div>
            <select v-model="selectedCampus" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả cơ sở</option>
              <option v-for="camp in campuses" :key="camp.id" :value="camp.id">{{ camp.name }}</option>
            </select>
          </div>

          <!-- Lọc Học kỳ -->
          <div>
            <select v-model="selectedSemester" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả học kỳ</option>
              <option v-for="sem in semesters" :key="sem" :value="sem">{{ sem }}</option>
            </select>
          </div>

          <!-- Lọc Trạng thái đợt đăng ký -->
          <div>
            <select v-model="selectedStatus" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả trạng thái</option>
              <option value="Draft">Bản nháp</option>
              <option value="Open">Đang mở</option>
              <option value="Closed">Đã đóng</option>
              <option value="Finalized">Đã chốt</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Bảng danh sách đợt đăng ký -->
      <div class="lg-table-shell overflow-x-auto mb-8">
        <table class="min-w-full divide-y divide-default">
          <thead>
            <tr class="surface-table-header">
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase tracking-wider">Đợt học / Cơ sở</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase tracking-wider">Học kỳ</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase tracking-wider whitespace-nowrap">Thời gian đăng ký</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase tracking-wider whitespace-nowrap">Hạn hủy môn</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase tracking-wider whitespace-nowrap">Hạn mức TC</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase tracking-wider whitespace-nowrap">Trạng thái</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase tracking-wider min-w-[220px] whitespace-nowrap">Hành động</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-if="filteredPeriods.length === 0" class="hover:bg-transparent">
              <td colspan="7" class="px-4 py-12 text-center text-muted">
                <div class="flex flex-col items-center justify-center gap-2">
                  <Info class="w-8 h-8 text-placeholder" />
                  <span class="font-medium text-sm">Không tìm thấy đợt đăng ký nào phù hợp.</span>
                </div>
              </td>
            </tr>

            <tr v-for="period in filteredPeriods" :key="period.id">
              <!-- Tên đợt / Cơ sở -->
              <td class="px-4 py-3 text-sm">
                <div class="font-bold text-heading">{{ period.name }}</div>
                <div class="text-xs text-primary font-semibold mt-0.5">{{ period.campusName }}</div>
              </td>

              <!-- Học kỳ -->
              <td class="px-4 py-3 text-sm font-semibold text-body">
                {{ period.semester }}
              </td>

              <!-- Thời gian đăng ký -->
              <td class="px-4 py-3 text-sm whitespace-nowrap">
                <div class="flex items-center gap-1.5 text-body font-medium">
                  <Clock class="w-4 h-4 text-placeholder" />
                  <span>{{ period.startDate }} &rarr; {{ period.endDate }}</span>
                </div>
              </td>

              <!-- Hạn hủy môn -->
              <td class="px-4 py-3 text-sm whitespace-nowrap">
                <span class="text-rose-500 font-bold bg-rose-50 dark:bg-rose-500/10 px-2.5 py-1 rounded-lg border border-rose-200/50 whitespace-nowrap">
                  {{ period.withdrawDeadline }}
                </span>
              </td>

              <!-- Hạn mức tín chỉ -->
              <td class="px-4 py-3 text-center text-sm font-bold text-heading">
                {{ period.maxCredits }} TC
              </td>

              <!-- Trạng thái -->
              <td class="px-4 py-3 text-center text-sm whitespace-nowrap">
                <span class="lg-badge whitespace-nowrap" :class="getStatusBadge(period.status).class">
                  {{ getStatusBadge(period.status).label }}
                </span>
              </td>

              <!-- Hành động -->
              <td class="px-4 py-3 text-center text-sm min-w-[220px] whitespace-nowrap">
                <div class="flex items-center justify-center gap-1.5 flex-nowrap">
                  <!-- Mở đợt đăng ký -->
                  <button
                    v-if="period.status === 'Draft'"
                    @click="openStatusModal(period, 'Open')"
                    class="lg-btn-success text-xs px-2.5 py-1.5 flex items-center gap-1 font-bold whitespace-nowrap"
                  >
                    <Play class="w-3.5 h-3.5" />
                    Mở đợt
                  </button>

                  <!-- Đóng đợt đăng ký (kèm quét sĩ số) -->
                  <button
                    v-if="period.status === 'Open'"
                    @click="openStatusModal(period, 'Closed')"
                    class="lg-btn-danger text-xs px-2.5 py-1.5 flex items-center gap-1 font-bold bg-rose-500 hover:bg-rose-600 text-white border-0 shadow-sm whitespace-nowrap"
                  >
                    <Square class="w-3.5 h-3.5" />
                    Đóng đợt
                  </button>

                  <!-- Chốt danh sách -->
                  <button
                    v-if="period.status === 'Closed'"
                    @click="openStatusModal(period, 'Finalized')"
                    class="lg-btn-primary text-xs px-2.5 py-1.5 flex items-center gap-1 font-bold whitespace-nowrap"
                  >
                    <CheckCircle class="w-3.5 h-3.5" />
                    Chốt danh sách
                  </button>

                  <!-- Gia hạn -->
                  <button
                    v-if="period.status === 'Open'"
                    @click="openExtendModal(period)"
                    class="lg-btn-secondary text-xs px-2.5 py-1.5 flex items-center gap-1 whitespace-nowrap"
                    title="Gia hạn thời gian đăng ký"
                  >
                    <Clock class="w-3.5 h-3.5" />
                    Gia hạn
                  </button>

                  <!-- Chỉnh sửa đợt (khi nháp) -->
                  <button
                    v-if="period.status === 'Draft'"
                    @click="openEditModal(period)"
                    class="lg-btn-secondary text-xs px-2.5 py-1.5 flex items-center gap-1 whitespace-nowrap"
                  >
                    <Edit2 class="w-3.5 h-3.5" />
                    Sửa
                  </button>

                  <!-- Badge thông báo đã Finalized -->
                  <span v-if="period.status === 'Finalized'" class="text-xs text-muted font-bold whitespace-nowrap">
                    Đã lưu trữ
                  </span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Khung Cảnh báo & Waitlist (Quản trị Sĩ số / Hàng chờ) -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
        <!-- Cảnh báo lớp sĩ số thấp (< 15 SV) -->
        <div class="lg-glass-soft lg-card lg-density-normal lg:col-span-2">
          <div class="flex items-center gap-2 border-b border-default pb-3 mb-4">
            <AlertTriangle class="w-5 h-5 text-amber-500" />
            <div>
              <h3 class="font-extrabold text-heading text-sm">Lớp học phần sĩ số thấp (&lt; 15 sinh viên)</h3>
              <p class="text-xs text-muted mt-0.5">Cần rà soát để gộp lớp hoặc hủy lớp khi đóng đợt đăng ký</p>
            </div>
          </div>

          <div class="lg-table-shell overflow-x-auto">
            <table class="min-w-full text-xs">
              <thead>
                <tr class="surface-table-header">
                  <th class="px-3 py-2 text-left font-bold text-label">Mã lớp học phần</th>
                  <th class="px-3 py-2 text-left font-bold text-label">Tên môn học</th>
                  <th class="px-3 py-2 text-center font-bold text-label">Sĩ số hiện tại / tối thiểu</th>
                  <th class="px-3 py-2 text-center font-bold text-label">Trạng thái xử lý</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-default">
                <tr v-for="cls in lowEnrollmentClasses" :key="cls.classCode">
                  <td class="px-3 py-2 font-bold text-heading">{{ cls.classCode }}</td>
                  <td class="px-3 py-2 font-medium text-body">{{ cls.subjectName }}</td>
                  <td class="px-3 py-2 text-center font-bold text-rose-500">
                    {{ cls.currentSize }} / {{ cls.minSize }} SV
                  </td>
                  <td class="px-3 py-2 text-center">
                    <span class="lg-badge lg-badge-danger animate-pulse">Cần xử lý</span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Quản lý hàng đợi (Waitlist) nhanh -->
        <div class="lg-glass-soft lg-card lg-density-normal">
          <div class="flex items-center gap-2 border-b border-default pb-3 mb-4">
            <Users class="w-5 h-5 text-primary" />
            <div>
              <h3 class="font-extrabold text-heading text-sm">Hàng đợi đăng ký (Waitlist)</h3>
              <p class="text-xs text-muted mt-0.5">Xử lý tắc nghẽn lớp chuyên ngành chuyên sâu</p>
            </div>
          </div>

          <div class="space-y-3">
            <div 
              v-for="(wl, key) in waitlists" 
              :key="key"
              class="p-3 rounded-xl border border-default bg-surface-card flex items-center justify-between text-xs"
            >
              <div>
                <div class="font-bold text-heading">{{ key }}</div>
                <div class="text-muted mt-0.5">Hàng chờ: <strong class="text-primary">{{ wl.students.length }} SV</strong></div>
                <div class="text-[10px] text-muted">Sức chứa: {{ wl.currentSize }}/{{ wl.maxSize }}</div>
              </div>

              <!-- Button quản lý hàng đợi -->
              <button 
                @click="openWaitlistDrawer(key)"
                class="lg-btn-secondary text-[11px] px-2.5 py-1.5"
              >
                Quản lý
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Audit Logs Panel -->
      <div class="lg-glass-soft lg-card lg-density-normal">
        <div class="flex items-center justify-between border-b border-default pb-3 mb-4">
          <div class="flex items-center gap-2">
            <History class="w-5 h-5 text-primary" />
            <h3 class="font-extrabold text-heading text-base">Nhật ký hoạt động (Audit Logs)</h3>
          </div>
          <span class="text-xs text-muted">Module M12 - Bảo toàn dữ liệu đào tạo</span>
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

    <!-- Form Modal (Tạo / Sửa / Gia hạn đợt đăng ký) -->
    <div 
      v-if="isFormModalOpen" 
      class="fixed inset-0 z-[100] flex items-center justify-center p-4 lg-mobile-scrim"
    >
      <div class="lg-glass-strong surface-modal border border-default rounded-2xl w-full max-w-md p-6 shadow-2xl relative animate-in fade-in zoom-in-95 duration-200">
        <!-- Close button -->
        <button 
          @click="isFormModalOpen = false" 
          class="absolute top-4 right-4 p-1 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 text-muted transition-colors"
        >
          <X class="w-5 h-5" />
        </button>

        <div class="flex items-center gap-3 border-b border-default pb-3 mb-4">
          <Calendar class="w-6 h-6 text-primary" />
          <h3 class="text-lg font-extrabold text-heading">
            {{ isExtendMode ? 'Gia hạn thời gian đăng ký' : (isEditMode ? 'Chỉnh sửa đợt đăng ký' : 'Tạo đợt đăng ký mới') }}
          </h3>
        </div>

        <div class="space-y-3 mb-4 text-sm">
          <!-- Tên đợt đăng ký (Disabled nếu là Gia hạn) -->
          <div>
            <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
              Tên đợt đăng ký <span class="text-rose-500">*</span>
            </label>
            <input
              v-model="form.name"
              type="text"
              placeholder="Nhập tên đợt đăng ký học phần..."
              :disabled="isExtendMode"
              class="w-full px-3 lg-control text-sm disabled:opacity-60 disabled:cursor-not-allowed"
            />
            <span v-if="formErrors.name" class="text-xs text-rose-500 mt-0.5 block">{{ formErrors.name }}</span>
          </div>

          <div class="grid grid-cols-2 gap-3" v-if="!isExtendMode">
            <!-- Học kỳ -->
            <div>
              <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
                Học kỳ áp dụng
              </label>
              <select v-model="form.semester" class="w-full px-3 lg-control text-sm">
                <option v-for="sem in semesters" :key="sem" :value="sem">{{ sem }}</option>
              </select>
            </div>

            <!-- Cơ sở -->
            <div>
              <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
                Cơ sở áp dụng
              </label>
              <select v-model="form.campusId" class="w-full px-3 lg-control text-sm">
                <option v-for="camp in campuses" :key="camp.id" :value="camp.id">{{ camp.name }}</option>
              </select>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <!-- Ngày bắt đầu (Disabled nếu là Gia hạn) -->
            <div>
              <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
                Ngày bắt đầu <span class="text-rose-500">*</span>
              </label>
              <input
                v-model="form.startDate"
                type="date"
                :disabled="isExtendMode"
                class="w-full px-3 lg-control text-sm disabled:opacity-60 disabled:cursor-not-allowed"
              />
            </div>

            <!-- Ngày kết thúc -->
            <div>
              <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
                Ngày kết thúc <span class="text-rose-500">*</span>
              </label>
              <input
                v-model="form.endDate"
                type="date"
                class="w-full px-3 lg-control text-sm"
              />
            </div>
          </div>
          <span v-if="formErrors.dateRange" class="text-xs text-rose-500 block">{{ formErrors.dateRange }}</span>

          <div class="grid grid-cols-2 gap-3">
            <!-- Hạn hủy môn (Withdraw deadline) -->
            <div>
              <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
                Hạn hủy môn <span class="text-rose-500">*</span>
              </label>
              <input
                v-model="form.withdrawDeadline"
                type="date"
                class="w-full px-3 lg-control text-sm"
              />
            </div>

            <!-- Hạn mức tín chỉ (Max credits) (Disabled nếu là Gia hạn) -->
            <div>
              <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
                Hạn mức tín chỉ <span class="text-rose-500">*</span>
              </label>
              <input
                v-model.number="form.maxCredits"
                type="number"
                min="1"
                max="30"
                :disabled="isExtendMode"
                class="w-full px-3 lg-control text-sm disabled:opacity-60 disabled:cursor-not-allowed"
              />
            </div>
          </div>
          <span v-if="formErrors.withdrawDate" class="text-xs text-rose-500 block">{{ formErrors.withdrawDate }}</span>
          <span v-if="formErrors.maxCredits" class="text-xs text-rose-500 block">{{ formErrors.maxCredits }}</span>
        </div>

        <!-- Buttons -->
        <div class="flex items-center justify-end gap-2 mt-6">
          <button 
            @click="isFormModalOpen = false" 
            class="lg-btn-secondary px-4 py-2 text-sm font-semibold"
          >
            Hủy bỏ
          </button>
          <button 
            @click="handleSavePeriod" 
            class="lg-btn-primary px-4 py-2 text-sm font-semibold"
          >
            <Save class="w-4 h-4" />
            Lưu thiết lập
          </button>
        </div>
      </div>
    </div>

    <!-- Confirm Status Modal (Xác nhận đổi trạng thái & Quét sĩ số) -->
    <div 
      v-if="isStatusModalOpen" 
      class="fixed inset-0 z-[100] flex items-center justify-center p-4 lg-mobile-scrim"
    >
      <div class="lg-glass-strong surface-modal border border-default rounded-2xl w-full max-w-md p-6 shadow-2xl relative animate-in fade-in zoom-in-95 duration-200">
        <!-- Close button -->
        <button 
          @click="isStatusModalOpen = false" 
          class="absolute top-4 right-4 p-1 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 text-muted transition-colors"
        >
          <X class="w-5 h-5" />
        </button>

        <div class="flex items-center gap-3 border-b border-default pb-3 mb-4">
          <Info class="w-6 h-6 text-primary" />
          <h3 class="text-lg font-extrabold text-heading">
            {{ targetStatus === 'Open' ? 'Xác nhận mở đợt đăng ký' : (targetStatus === 'Closed' ? 'Xác nhận đóng đợt đăng ký' : 'Xác nhận chốt danh sách') }}
          </h3>
        </div>

        <p class="text-sm text-body leading-relaxed mb-4">
          Bạn đang thực hiện thay đổi trạng thái của đợt đăng ký <strong>"{{ periodToChange?.name }}"</strong> sang 
          <span class="font-extrabold text-primary">{{ targetStatus === 'Open' ? 'Đang mở' : (targetStatus === 'Closed' ? 'Đã đóng' : 'Đã chốt (Finalized)') }}</span>.
        </p>

        <!-- RÀNG BUỘC NGHIỆP VỤ: Quét sĩ số tối thiểu khi Đóng đợt -->
        <div v-if="targetStatus === 'Closed'" class="mb-4 p-3 bg-amber-50 dark:bg-amber-950/20 border border-amber-200 dark:border-amber-900/40 rounded-xl space-y-2">
          <div class="flex items-center gap-1.5 text-xs text-amber-800 dark:text-amber-300 font-extrabold">
            <AlertTriangle class="w-4 h-4 flex-shrink-0" />
            <span>Hệ thống phát hiện các lớp học phần dưới sĩ số tối thiểu:</span>
          </div>
          <div class="text-[11px] space-y-1 text-slate-700 dark:text-slate-300 pl-5">
            <div v-for="cls in lowEnrollmentClasses" :key="cls.classCode">
              Lớp <strong>{{ cls.classCode }}</strong>: sĩ số {{ cls.currentSize }}/{{ cls.minSize }} SV (Thiếu {{ cls.minSize - cls.currentSize }} SV)
            </div>
          </div>
          <div class="text-[10px] text-muted italic pl-5">
            * Đóng đợt sẽ đánh dấu các lớp này để chuẩn bị quy trình gộp/hủy lớp ở các bước vận hành sau.
          </div>
        </div>

        <!-- Nhập lý do điều phối -->
        <div class="mb-4">
          <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
            Lý do thao tác <span class="text-rose-500">*</span>
          </label>
          <textarea
            v-model="changeReason"
            rows="3"
            placeholder="Nhập lý do thay đổi trạng thái..."
            class="w-full p-2.5 text-sm lg-input"
          ></textarea>
        </div>

        <!-- Buttons -->
        <div class="flex items-center justify-end gap-2 mt-6">
          <button 
            @click="isStatusModalOpen = false" 
            class="lg-btn-secondary px-4 py-2 text-sm font-semibold"
          >
            Hủy bỏ
          </button>
          <button 
            @click="handleConfirmStatus" 
            :disabled="!changeReason.trim()"
            class="lg-btn-primary px-4 py-2 text-sm font-semibold disabled:opacity-50"
          >
            Xác nhận thay đổi
          </button>
        </div>
      </div>
    </div>

    <!-- Waitlist Drawer (Quản lý hàng đợi) -->
    <div 
      v-if="isWaitlistDrawerOpen" 
      class="fixed inset-0 z-[90] flex justify-end lg-mobile-scrim"
      @click="isWaitlistDrawerOpen = false"
    >
      <div 
        class="w-full max-w-lg h-full lg-glass-strong surface-modal border-l border-default shadow-2xl flex flex-col p-6 animate-in slide-in-from-right duration-300"
        @click.stop
      >
        <!-- Drawer Header -->
        <div class="flex items-center justify-between border-b border-default pb-4 mb-4">
          <div>
            <h3 class="text-lg font-extrabold text-heading flex items-center gap-2">
              <Users class="w-5 h-5 text-primary" />
              Hàng đợi Waitlist Lớp {{ selectedClassCode }}
            </h3>
            <p class="text-xs text-muted mt-1 font-semibold">
              Sức chứa hiện tại: {{ waitlistData.currentSize }} / {{ waitlistData.maxSize }} SV
            </p>
          </div>
          <button 
            @click="isWaitlistDrawerOpen = false" 
            class="p-1 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 text-muted transition-colors"
          >
            <X class="w-5 h-5" />
          </button>
        </div>

        <!-- Section Tăng sức chứa lớp -->
        <div class="mb-4 p-4 rounded-xl border border-default bg-surface-card flex items-center justify-between gap-4">
          <div>
            <span class="font-bold text-xs text-label uppercase tracking-wider block">Giải phóng hàng đợi</span>
            <span class="text-[11px] text-muted mt-0.5">Tăng giới hạn sĩ số của lớp thêm 5 chỗ để chấp nhận sinh viên từ hàng chờ</span>
          </div>

          <button 
            @click="handleIncreaseSize"
            class="lg-btn-primary text-xs px-3 py-2 flex items-center gap-1.5"
          >
            <Plus class="w-4 h-4" />
            Tăng +5 chỗ
          </button>
        </div>

        <!-- Drawer Content (Danh sách hàng đợi) -->
        <div class="flex-1 overflow-y-auto pr-1">
          <div 
            v-if="waitlistData.students.length === 0" 
            class="text-center py-12 text-muted"
          >
            <CheckCircle class="w-10 h-10 text-emerald-500 mx-auto mb-2" />
            <p class="text-sm font-medium">Hàng đợi của lớp này hiện đang trống.</p>
          </div>

          <div class="space-y-3">
            <div 
              v-for="stu in waitlistData.students" 
              :key="stu.id" 
              class="p-3.5 rounded-xl border border-default bg-surface-card flex items-center justify-between gap-4"
            >
              <div>
                <div class="font-bold text-heading text-sm">{{ stu.name }}</div>
                <div class="text-xs text-primary font-bold">MSSV: {{ stu.id }}</div>
                <div class="text-[10px] text-muted flex items-center gap-1 mt-1">
                  <Clock class="w-3.5 h-3.5 text-placeholder" />
                  Đợi từ: {{ stu.joinedAt }}
                </div>
              </div>

              <!-- Ghép lớp thủ công -->
              <button 
                @click="handleApproveWaitlistStudent(stu)"
                class="lg-btn-secondary text-xs px-2.5 py-1.5 flex items-center gap-1.5 hover:bg-emerald-500/10 hover:border-emerald-500 hover:text-emerald-500"
                title="Ghép sinh viên vào lớp chính thức"
              >
                <CheckCircle class="w-3.5 h-3.5" />
                Ghép lớp
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Hiệu ứng chuyển động */
.animate-in {
  animation-duration: 200ms;
  animation-fill-mode: both;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}
</style>

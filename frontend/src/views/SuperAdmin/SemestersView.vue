<script setup>
/**
 * SemestersView.vue - Super Admin
 * Giao diện cấu hình và quản lý vòng đời học kỳ (Draft -> Active -> Closed -> Locked).
 * Hỗ trợ tạo mới, thiết lập thời gian, mở/đóng/khóa học kỳ và ghi Audit Log.
 */
import { ref, computed, watch } from 'vue'
import {
  Calendar,
  Plus,
  Search,
  Lock,
  Play,
  Square,
  Edit2,
  AlertTriangle,
  History,
  Clock,
  X,
  Save,
  RotateCcw,
  CheckCircle,
  AlertCircle
} from 'lucide-vue-next'

// --- Mock Data ---

// Danh sách cơ sở (Campus)
const campuses = ref([
  { id: 4, name: 'Cơ sở Hòa Lạc' },
  { id: 5, name: 'Cơ sở Detech' },
  { id: 3, name: 'Cơ sở TP.HCM' }
])

// Danh sách năm học
const academicYears = ref(['2023-2024', '2024-2025', '2025-2026'])

// Danh sách học kỳ
const semesters = ref([
  {
    id: 1,
    name: 'Học kỳ Spring 2026',
    academicYear: '2025-2026',
    campusId: 4,
    campusName: 'Cơ sở Hòa Lạc',
    startDate: '2026-01-02',
    endDate: '2026-04-30',
    status: 'Active',
    lockedAt: null
  },
  {
    id: 2,
    name: 'Học kỳ Summer 2026',
    academicYear: '2025-2026',
    campusId: 4,
    campusName: 'Cơ sở Hòa Lạc',
    startDate: '2026-05-02',
    endDate: '2026-08-31',
    status: 'Draft',
    lockedAt: null
  },
  {
    id: 3,
    name: 'Học kỳ Fall 2025',
    academicYear: '2025-2026',
    campusId: 4,
    campusName: 'Cơ sở Hòa Lạc',
    startDate: '2025-09-02',
    endDate: '2025-12-31',
    status: 'Closed',
    lockedAt: null
  },
  {
    id: 4,
    name: 'Học kỳ Spring 2025',
    academicYear: '2024-2025',
    campusId: 4,
    campusName: 'Cơ sở Hòa Lạc',
    startDate: '2025-01-02',
    endDate: '2025-04-30',
    status: 'Locked',
    lockedAt: '2025-05-01 09:00:00'
  },
  {
    id: 5,
    name: 'Học kỳ Spring 2026',
    academicYear: '2025-2026',
    campusId: 3,
    campusName: 'Cơ sở TP.HCM',
    startDate: '2026-01-02',
    endDate: '2026-04-30',
    status: 'Active',
    lockedAt: null
  },
  {
    id: 6,
    name: 'Học kỳ Fall 2025',
    academicYear: '2025-2026',
    campusId: 3,
    campusName: 'Cơ sở TP.HCM',
    startDate: '2025-09-02',
    endDate: '2025-12-31',
    status: 'Locked',
    lockedAt: '2026-01-05 14:30:22'
  }
])

// Audit Logs vận hành học kỳ
const auditLogs = ref([
  {
    id: 1,
    time: '2026-06-08 14:30:10',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Khóa học kỳ',
    details: 'Đã khóa Học kỳ Fall 2025 - Cơ sở TP.HCM',
    reason: 'Hoàn tất nhập điểm và phúc khảo học vụ'
  },
  {
    id: 2,
    time: '2026-06-02 08:00:00',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Mở học kỳ',
    details: 'Đã mở Học kỳ Summer 2026 - Cơ sở Hòa Lạc sang Active',
    reason: 'Bắt đầu tuần học đầu tiên của kỳ hè'
  },
  {
    id: 3,
    time: '2026-05-02 09:15:30',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Tạo mới học kỳ',
    details: 'Khởi tạo Học kỳ Summer 2026 - Cơ sở Hòa Lạc ở trạng thái Draft',
    reason: 'Cấu hình kế hoạch đào tạo năm học'
  }
])

// --- State Bộ lọc ---
const searchQuery = ref('')
const selectedCampus = ref('all')
const selectedYear = ref('all')
const selectedStatus = ref('all')

// --- Thống kê KPI ---
const totalSemesters = computed(() => semesters.value.length)
const activeSemesters = computed(() => semesters.value.filter(s => s.status === 'Active').length)
const lockedSemesters = computed(() => semesters.value.filter(s => s.status === 'Locked').length)
const draftSemesters = computed(() => semesters.value.filter(s => s.status === 'Draft').length)

// --- Lọc dữ liệu ---
const filteredSemesters = computed(() => {
  return semesters.value.filter(s => {
    const matchSearch = s.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchCampus = selectedCampus.value === 'all' || s.campusId === parseInt(selectedCampus.value)
    const matchYear = selectedYear.value === 'all' || s.academicYear === selectedYear.value
    const matchStatus = selectedStatus.value === 'all' || s.status === selectedStatus.value
    return matchSearch && matchCampus && matchYear && matchStatus
  })
})

// --- State Form Modal (Tạo / Sửa học kỳ) ---
const isFormModalOpen = ref(false)
const isEditMode = ref(false)
const form = ref({
  id: null,
  name: '',
  academicYear: '',
  campusId: '',
  startDate: '',
  endDate: ''
})

const errors = ref({
  name: '',
  academicYear: '',
  campusId: '',
  dateRange: ''
})

// Kiểm tra real-time validation cho Form
watch(form, (newVal) => {
  errors.value = { name: '', academicYear: '', campusId: '', dateRange: '' }

  if (isFormModalOpen.value) {
    if (!newVal.name.trim()) {
      errors.value.name = 'Tên học kỳ không được để trống.'
    }
    if (!newVal.academicYear) {
      errors.value.academicYear = 'Vui lòng chọn năm học.'
    }
    if (!newVal.campusId) {
      errors.value.campusId = 'Vui lòng chọn cơ sở áp dụng.'
    }
    if (newVal.startDate && newVal.endDate) {
      const start = new Date(newVal.startDate)
      const end = new Date(newVal.endDate)
      if (start >= end) {
        errors.value.dateRange = 'Ngày bắt đầu phải trước ngày kết thúc.'
      }
    } else if (!newVal.startDate || !newVal.endDate) {
      errors.value.dateRange = 'Vui lòng thiết lập đầy đủ thời gian bắt đầu và kết thúc.'
    }
  }
}, { deep: true })

const isFormValid = computed(() => {
  return form.value.name.trim() &&
         form.value.academicYear &&
         form.value.campusId &&
         form.value.startDate &&
         form.value.endDate &&
         !errors.value.dateRange
})

// --- State Lock Modal (Xác nhận Khóa học kỳ) ---
const isLockModalOpen = ref(false)
const lockReason = ref('')
const confirmCheck = ref(false)
const semesterToLock = ref(null)

// --- Trạng thái badge helper ---
const getStatusBadge = (status) => {
  switch (status) {
    case 'Draft':
      return { class: 'bg-amber-50 text-amber-700 border-amber-200/50 dark:bg-amber-600/10 dark:text-amber-400 dark:border-amber-500/20', label: 'Bản nháp' }
    case 'Active':
      return { class: 'bg-emerald-50 text-emerald-700 border-emerald-200/50 dark:bg-emerald-600/10 dark:text-emerald-400 dark:border-emerald-500/20', label: 'Đang hoạt động' }
    case 'Closed':
      return { class: 'bg-slate-100 text-slate-700 border-slate-200/50 dark:bg-slate-700/50 dark:text-slate-300 dark:border-slate-600/30', label: 'Đã đóng' }
    case 'Locked':
      return { class: 'bg-rose-50 text-rose-700 border-rose-200/50 dark:bg-rose-600/10 dark:text-rose-400 dark:border-rose-500/20', label: 'Đã khóa' }
    default:
      return { class: 'bg-slate-100 text-slate-700', label: status }
  }
}

// --- Các hàm xử lý (Handlers) ---

const openCreateModal = () => {
  isEditMode.value = false
  form.value = {
    id: null,
    name: '',
    academicYear: academicYears.value[academicYears.value.length - 1],
    campusId: campuses.value[0]?.id || '',
    startDate: '',
    endDate: ''
  }
  errors.value = { name: '', academicYear: '', campusId: '', dateRange: '' }
  isFormModalOpen.value = true
}

const openEditModal = (semester) => {
  isEditMode.value = true
  form.value = { ...semester }
  errors.value = { name: '', academicYear: '', campusId: '', dateRange: '' }
  isFormModalOpen.value = true
}

const saveSemester = () => {
  if (!isFormValid.value) return

  const campusName = campuses.value.find(c => c.id === parseInt(form.value.campusId))?.name || 'Cơ sở mới'

  if (isEditMode.value) {
    // Cập nhật
    const idx = semesters.value.findIndex(s => s.id === form.value.id)
    if (idx !== -1) {
      semesters.value[idx] = {
        ...semesters.value[idx],
        name: form.value.name,
        academicYear: form.value.academicYear,
        campusId: parseInt(form.value.campusId),
        campusName,
        startDate: form.value.startDate,
        endDate: form.value.endDate
      }
      
      // Ghi log
      addAuditLog('Cập nhật học kỳ', `Đã chỉnh sửa thông tin Học kỳ "${form.value.name}" (${campusName})`, 'Cập nhật cấu hình lịch học thuật')
    }
  } else {
    // Tạo mới
    const newId = semesters.value.length ? Math.max(...semesters.value.map(s => s.id)) + 1 : 1
    semesters.value.unshift({
      id: newId,
      name: form.value.name,
      academicYear: form.value.academicYear,
      campusId: parseInt(form.value.campusId),
      campusName,
      startDate: form.value.startDate,
      endDate: form.value.endDate,
      status: 'Draft',
      lockedAt: null
    })

    // Ghi log
    addAuditLog('Tạo mới học kỳ', `Khởi tạo Học kỳ "${form.value.name}" (${campusName}) ở trạng thái Draft`, 'Thiết lập kế hoạch đào tạo mới')
  }

  isFormModalOpen.value = false
}

// Thay đổi trạng thái sang Active
const activateSemester = (semester) => {
  // Ràng buộc: kiểm tra xem cơ sở đó đã có học kỳ nào Active chưa
  const hasActiveInCampus = semesters.value.some(s => s.campusId === semester.campusId && s.status === 'Active' && s.id !== semester.id)
  
  if (hasActiveInCampus) {
    alert(`Cơ sở [${semester.campusName}] hiện đã có một học kỳ đang hoạt động. Vui lòng đóng học kỳ hiện tại trước khi kích hoạt học kỳ mới.`)
    return
  }

  if (confirm(`Bạn có chắc chắn muốn MỞ (Active) học kỳ "${semester.name}" không? Sinh viên và giảng viên có thể bắt đầu các hoạt động học vụ.`)) {
    semester.status = 'Active'
    addAuditLog('Mở học kỳ', `Đã kích hoạt Học kỳ "${semester.name}" (${semester.campusName}) sang trạng thái Active`, 'Bắt đầu kỳ học mới')
  }
}

// Thay đổi trạng thái sang Closed
const closeSemester = (semester) => {
  if (confirm(`Bạn có chắc chắn muốn ĐÓNG học kỳ "${semester.name}" không? Các hoạt động giảng dạy sẽ kết thúc.`)) {
    semester.status = 'Closed'
    addAuditLog('Đóng học kỳ', `Đã đóng các hoạt động của Học kỳ "${semester.name}" (${semester.campusName})`, 'Kết thúc thời gian giảng dạy')
  }
}

// Mở lại học kỳ (từ Closed về Active)
const reopenSemester = (semester) => {
  const hasActiveInCampus = semesters.value.some(s => s.campusId === semester.campusId && s.status === 'Active')
  if (hasActiveInCampus) {
    alert(`Không thể mở lại. Cơ sở [${semester.campusName}] hiện đang có một học kỳ khác hoạt động.`)
    return
  }

  if (confirm(`Bạn có chắc chắn muốn mở lại học kỳ "${semester.name}"?`)) {
    semester.status = 'Active'
    addAuditLog('Mở lại học kỳ', `Đã khôi phục trạng thái hoạt động Học kỳ "${semester.name}" (${semester.campusName})`, 'Khắc phục sự cố cần chỉnh sửa hoạt động dạy học')
  }
}

// Mở modal xác nhận Khóa học kỳ
const openLockModal = (semester) => {
  semesterToLock.value = semester
  lockReason.value = ''
  confirmCheck.value = false
  isLockModalOpen.value = true
}

// Xác nhận Khóa học kỳ (Lock)
const confirmLockSemester = () => {
  if (!lockReason.value.trim()) {
    alert('Vui lòng nhập lý do khóa học kỳ để lưu Audit Log.')
    return
  }
  if (!confirmCheck.value) {
    alert('Vui lòng tích xác nhận cam kết chịu trách nhiệm để tiếp tục.')
    return
  }

  if (semesterToLock.value) {
    const sem = semesters.value.find(s => s.id === semesterToLock.value.id)
    if (sem) {
      sem.status = 'Locked'
      sem.lockedAt = new Date().toLocaleString('sv-SE', { timeZone: 'Asia/Ho_Chi_Minh' }).replace('T', ' ')
      
      // Ghi log
      addAuditLog(
        'Khóa học kỳ',
        `Đã khóa vĩnh viễn dữ liệu học kỳ "${sem.name}" (${sem.campusName})`,
        lockReason.value
      )
    }
  }

  isLockModalOpen.value = false
  semesterToLock.value = null
}

// Hàm thêm Audit Log
const addAuditLog = (action, details, reason) => {
  const now = new Date()
  const timeStr = now.toLocaleString('sv-SE', { timeZone: 'Asia/Ho_Chi_Minh' }).replace('T', ' ')
  auditLogs.value.unshift({
    id: auditLogs.value.length ? Math.max(...auditLogs.value.map(l => l.id)) + 1 : 1,
    time: timeStr,
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action,
    details,
    reason
  })
}

// Reset bộ lọc
const resetFilters = () => {
  searchQuery.value = ''
  selectedCampus.value = 'all'
  selectedYear.value = 'all'
  selectedStatus.value = 'all'
}
</script>

<template>
  <div class="semesters-page pb-12 space-y-6">
    <!-- Header -->
    <header class="page-header flex flex-col md:flex-row md:items-center justify-between gap-4 border-b border-default pb-4">
      <div>
        <h1 class="text-2xl font-bold text-heading">Cấu hình Học kỳ & Năm học (M6)</h1>
        <p class="text-sm text-label mt-1">Điều phối vòng đời hoạt động học kỳ, khóa học vụ và đồng bộ kết quả học tập toàn trường.</p>
      </div>
      <div class="flex items-center gap-3">
        <button @click="openCreateModal" class="glass-btn primary shadow-sm">
          <Plus :size="16" /> Tạo học kỳ mới
        </button>
      </div>
    </header>

    <!-- KPI Mini Panel -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-4 shadow-sm">
        <p class="text-xs font-bold text-label uppercase tracking-wide">Tổng số học kỳ</p>
        <div class="flex items-baseline gap-2 mt-1">
          <span class="text-3xl font-extrabold text-heading">{{ totalSemesters }}</span>
          <span class="text-[10px] text-placeholder">Kỳ học hệ thống</span>
        </div>
      </div>
      <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-4 shadow-sm border-l-4 border-l-emerald-500">
        <p class="text-xs font-bold text-label uppercase tracking-wide">Đang hoạt động (Active)</p>
        <div class="flex items-baseline gap-2 mt-1">
          <span class="text-3xl font-extrabold text-emerald-600 dark:text-emerald-400">{{ activeSemesters }}</span>
          <span class="text-[10px] text-placeholder">Đang mở dạy và học</span>
        </div>
      </div>
      <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-4 shadow-sm border-l-4 border-l-rose-500">
        <p class="text-xs font-bold text-label uppercase tracking-wide">Đã khóa điểm (Locked)</p>
        <div class="flex items-baseline gap-2 mt-1">
          <span class="text-3xl font-extrabold text-rose-600 dark:text-rose-400">{{ lockedSemesters }}</span>
          <span class="text-[10px] text-placeholder">Đóng băng bảng điểm</span>
        </div>
      </div>
      <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-4 shadow-sm border-l-4 border-l-amber-500">
        <p class="text-xs font-bold text-label uppercase tracking-wide">Bản nháp (Draft)</p>
        <div class="flex items-baseline gap-2 mt-1">
          <span class="text-3xl font-extrabold text-amber-600 dark:text-amber-400">{{ draftSemesters }}</span>
          <span class="text-[10px] text-placeholder">Chưa kích hoạt</span>
        </div>
      </div>
    </div>

    <!-- Filters Bar -->
    <div class="lg-glass-soft p-4 rounded-2xl border border-white/60 dark:border-white/10 shadow-sm flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div class="flex flex-wrap items-center gap-3 flex-1 min-w-0">
        <!-- Search -->
        <div class="relative w-full md:w-64">
          <Search :size="15" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Tìm theo tên học kỳ..."
            class="glass-input pl-9 w-full"
          />
        </div>

        <!-- Campus filter -->
        <div class="flex items-center gap-1.5 w-full sm:w-auto">
          <span class="text-xs text-label font-bold whitespace-nowrap hidden lg:inline">Cơ sở:</span>
          <select v-model="selectedCampus" class="glass-select w-full sm:w-40">
            <option value="all">Tất cả cơ sở</option>
            <option v-for="c in campuses" :key="c.id" :value="c.id">{{ c.name }}</option>
          </select>
        </div>

        <!-- Academic Year filter -->
        <div class="flex items-center gap-1.5 w-full sm:w-auto">
          <span class="text-xs text-label font-bold whitespace-nowrap hidden lg:inline">Năm học:</span>
          <select v-model="selectedYear" class="glass-select w-full sm:w-36">
            <option value="all">Tất cả năm học</option>
            <option v-for="y in academicYears" :key="y" :value="y">{{ y }}</option>
          </select>
        </div>

        <!-- Status filter -->
        <div class="flex items-center gap-1.5 w-full sm:w-auto">
          <span class="text-xs text-label font-bold whitespace-nowrap hidden lg:inline">Trạng thái:</span>
          <select v-model="selectedStatus" class="glass-select w-full sm:w-36">
            <option value="all">Tất cả trạng thái</option>
            <option value="Draft">Bản nháp</option>
            <option value="Active">Đang hoạt động</option>
            <option value="Closed">Đã đóng</option>
            <option value="Locked">Đã khóa</option>
          </select>
        </div>
      </div>

      <button @click="resetFilters" class="glass-btn secondary shrink-0 self-end md:self-auto justify-center">
        <RotateCcw :size="14" /> Xóa bộ lọc
      </button>
    </div>

    <!-- Main Content: Semesters Table -->
    <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 shadow-sm overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50 dark:bg-white/5 border-b border-default text-[11px] font-bold text-label uppercase tracking-widest">
              <th class="p-4">Học kỳ</th>
              <th class="p-4">Năm học</th>
              <th class="p-4">Áp dụng Cơ sở</th>
              <th class="p-4">Thời gian tổ chức</th>
              <th class="p-4 text-center">Trạng thái</th>
              <th class="p-4">Locked At (Thời điểm khóa)</th>
              <th class="p-4 text-right">Thao tác quản trị</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-if="filteredSemesters.length === 0">
              <td colspan="7" class="p-8 text-center text-placeholder text-sm">
                Không tìm thấy dữ liệu học kỳ phù hợp với bộ lọc hiện tại.
              </td>
            </tr>
            <tr
              v-for="semester in filteredSemesters"
              :key="semester.id"
              class="hover:bg-white/40 dark:hover:bg-white/5 transition-all text-sm group"
            >
              <!-- Tên học kỳ -->
              <td class="p-4 font-bold text-heading">
                <div class="flex items-center gap-2">
                  <Calendar :size="16" class="text-blue-500 shrink-0" />
                  {{ semester.name }}
                </div>
              </td>
              <!-- Năm học -->
              <td class="p-4 text-body font-semibold">{{ semester.academicYear }}</td>
              <!-- Cơ sở áp dụng -->
              <td class="p-4">
                <span class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg bg-slate-100 dark:bg-white/10 font-bold text-xs text-heading">
                  <span class="h-1.5 w-1.5 rounded-full bg-blue-500"></span>
                  {{ semester.campusName }}
                </span>
              </td>
              <!-- Thời gian start - end -->
              <td class="p-4">
                <div class="text-body text-xs flex flex-col font-medium">
                  <span>Bắt đầu: <strong>{{ semester.startDate }}</strong></span>
                  <span class="mt-0.5 text-placeholder">Kết thúc: <strong>{{ semester.endDate }}</strong></span>
                </div>
              </td>
              <!-- Trạng thái badge -->
              <td class="p-4 text-center">
                <span :class="['px-2.5 py-0.5 rounded-full text-xs font-bold border inline-block', getStatusBadge(semester.status).class]">
                  {{ getStatusBadge(semester.status).label }}
                </span>
              </td>
              <!-- Locked At -->
              <td class="p-4">
                <div v-if="semester.status === 'Locked'" class="flex items-center gap-1.5 text-xs text-rose-600 dark:text-rose-400 font-bold">
                  <Lock :size="12" />
                  {{ semester.lockedAt }}
                </div>
                <div v-else class="text-placeholder text-xs italic">Chưa khóa</div>
              </td>
              <!-- Các nút thao tác động -->
              <td class="p-4 text-right">
                <div class="flex items-center justify-end gap-1.5">
                  <!-- Draft: edit time, open -->
                  <template v-if="semester.status === 'Draft'">
                    <button @click="openEditModal(semester)" class="action-btn text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-900/30" title="Chỉnh sửa">
                      <Edit2 :size="14" />
                    </button>
                    <button @click="activateSemester(semester)" class="action-btn text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/30 font-semibold text-xs gap-1" title="Kích hoạt">
                      <Play :size="13" /> Mở kỳ
                    </button>
                  </template>

                  <!-- Active: close -->
                  <template v-else-if="semester.status === 'Active'">
                    <button @click="closeSemester(semester)" class="action-btn text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-900/30 font-semibold text-xs gap-1" title="Kết thúc kỳ học">
                      <Square :size="13" /> Đóng kỳ
                    </button>
                  </template>

                  <!-- Closed: lock, reopen, edit -->
                  <template v-else-if="semester.status === 'Closed'">
                    <button @click="openEditModal(semester)" class="action-btn text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-900/30" title="Chỉnh sửa">
                      <Edit2 :size="14" />
                    </button>
                    <button @click="reopenSemester(semester)" class="action-btn text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/30 text-xs font-semibold" title="Mở lại kỳ học">
                      Mở lại
                    </button>
                    <button @click="openLockModal(semester)" class="action-btn text-rose-600 hover:bg-rose-50 dark:hover:bg-rose-900/30 font-semibold text-xs gap-1" title="Khóa dữ liệu học vụ">
                      <Lock :size="13" /> Khóa kỳ
                    </button>
                  </template>

                  <!-- Locked: disabled -->
                  <template v-else-if="semester.status === 'Locked'">
                    <span class="inline-flex items-center gap-1 text-xs text-rose-500 font-bold px-2.5 py-1 bg-rose-50 dark:bg-rose-900/20 rounded-lg select-none">
                      <Lock :size="12" /> Dữ liệu đã đóng băng
                    </span>
                  </template>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Audit Log Section -->
    <div class="lg-glass-soft p-5 rounded-2xl border border-white/60 dark:border-white/10 shadow-sm space-y-4">
      <div class="flex items-center justify-between border-b border-default pb-3">
        <h3 class="font-bold text-heading flex items-center gap-2">
          <History :size="18" class="text-violet-500" />
          Nhật ký vận hành học kỳ (Audit Log)
        </h3>
        <span class="text-[10px] uppercase font-bold text-label bg-slate-100 dark:bg-white/10 px-2 py-0.5 rounded-md">Bắt buộc tự động ghi</span>
      </div>

      <div class="space-y-3 max-h-[280px] overflow-y-auto pr-2">
        <div
          v-for="log in auditLogs"
          :key="log.id"
          class="flex items-start gap-3 p-3 rounded-xl bg-white/40 dark:bg-white/5 border border-default text-xs"
        >
          <div class="p-2 bg-violet-50 dark:bg-violet-950 text-violet-600 dark:text-violet-400 rounded-lg mt-0.5">
            <Clock :size="14" />
          </div>
          <div class="flex-1 min-w-0">
            <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-1">
              <span class="font-bold text-heading">{{ log.action }}</span>
              <span class="text-[10px] text-placeholder font-mono">{{ log.time }}</span>
            </div>
            <p class="text-body font-semibold mt-1">{{ log.details }}</p>
            <p class="text-placeholder mt-0.5">Lý do ghi log: <span class="italic text-body font-medium">"{{ log.reason }}"</span></p>
            <p class="text-[10px] text-placeholder mt-1">Người thực hiện: <span class="font-mono">{{ log.actor }}</span></p>
          </div>
        </div>
      </div>
    </div>

    <!-- Form Modal (Tạo / Sửa học kỳ) -->
    <div v-if="isFormModalOpen" class="modal-overlay">
      <div class="modal-content max-w-lg w-full">
        <!-- Modal Header -->
        <div class="flex items-center justify-between border-b border-default pb-4 mb-5">
          <div class="flex items-center gap-2.5">
            <div class="w-9 h-9 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center">
              <Calendar :size="18" />
            </div>
            <div>
              <h3 class="text-lg font-bold text-heading">{{ isEditMode ? 'Chỉnh sửa học kỳ' : 'Tạo mới học kỳ' }}</h3>
              <p class="text-xs text-label uppercase tracking-widest font-semibold mt-0.5">Thiết lập tham số học vụ</p>
            </div>
          </div>
          <button @click="isFormModalOpen = false" class="action-btn text-placeholder hover:text-heading">
            <X :size="18" />
          </button>
        </div>

        <!-- Modal Body -->
        <div class="space-y-4">
          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1.5">Tên học kỳ *</label>
            <input
              v-model="form.name"
              type="text"
              class="glass-input w-full"
              placeholder="VD: Học kỳ Spring 2026, Học kỳ hè 2026..."
              :class="{'border-rose-300 bg-rose-50/50': errors.name}"
            />
            <p v-if="errors.name" class="text-rose-500 text-[10px] mt-1 font-semibold">{{ errors.name }}</p>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Năm học học thuật *</label>
              <select v-model="form.academicYear" class="glass-select w-full" :class="{'border-rose-300': errors.academicYear}">
                <option v-for="y in academicYears" :key="y" :value="y">{{ y }}</option>
              </select>
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Cơ sở áp dụng *</label>
              <select v-model="form.campusId" class="glass-select w-full" :class="{'border-rose-300': errors.campusId}">
                <option v-for="c in campuses" :key="c.id" :value="c.id">{{ c.name }}</option>
              </select>
            </div>
          </div>

          <!-- Date Range Form -->
          <div class="border-t border-default pt-4 space-y-3">
            <h4 class="text-xs font-bold text-heading uppercase tracking-wide">Thiết lập Thời gian (Date Range)</h4>
            <div class="grid grid-cols-2 gap-4">
              <div class="form-group">
                <label class="block text-[11px] font-bold text-label mb-1">Ngày bắt đầu *</label>
                <input
                  v-model="form.startDate"
                  type="date"
                  class="glass-input w-full"
                  :class="{'border-rose-300': errors.dateRange}"
                />
              </div>
              <div class="form-group">
                <label class="block text-[11px] font-bold text-label mb-1">Ngày kết thúc *</label>
                <input
                  v-model="form.endDate"
                  type="date"
                  class="glass-input w-full"
                  :class="{'border-rose-300': errors.dateRange}"
                />
              </div>
            </div>
            <p v-if="errors.dateRange" class="text-rose-500 text-[10px] mt-1 font-semibold flex items-center gap-1">
              <AlertCircle :size="12" />
              {{ errors.dateRange }}
            </p>
          </div>

          <!-- Validation Overview panel -->
          <div class="p-3.5 rounded-xl border flex items-start gap-2.5 text-xs" :class="isFormValid ? 'bg-emerald-50/60 border-emerald-200 text-emerald-800 dark:bg-emerald-950/20 dark:border-emerald-500/20 dark:text-emerald-400' : 'bg-rose-50/60 border-rose-200 text-rose-800 dark:bg-rose-950/20 dark:border-rose-500/20 dark:text-rose-400'">
            <component :is="isFormValid ? CheckCircle : AlertCircle" :size="16" class="shrink-0 mt-0.5" />
            <div>
              <span class="font-bold">{{ isFormValid ? 'Thông tin hợp lệ' : 'Kiểm tra dữ liệu nhập' }}</span>
              <p class="text-[10px] mt-0.5 opacity-95">
                {{ isFormValid ? 'Tất cả các trường đã được điền đúng quy định và kiểm tra hợp lệ.' : 'Vui lòng hoàn thành đầy đủ thông tin bắt buộc và đảm bảo Ngày bắt đầu nhỏ hơn Ngày kết thúc.' }}
              </p>
            </div>
          </div>
        </div>

        <!-- Modal Footer -->
        <div class="flex gap-3 justify-end border-t border-default pt-4 mt-6">
          <button @click="isFormModalOpen = false" class="glass-btn secondary">Hủy</button>
          <button
            @click="saveSemester"
            class="glass-btn primary"
            :disabled="!isFormValid"
            :class="{'opacity-50 cursor-not-allowed': !isFormValid}"
          >
            <Save :size="14" />
            {{ isEditMode ? 'Lưu cập nhật' : 'Khởi tạo Học kỳ' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Confirm Lock Modal -->
    <div v-if="isLockModalOpen" class="modal-overlay">
      <div class="modal-content max-w-md w-full border border-rose-200 dark:border-rose-500/30">
        <!-- Header -->
        <div class="flex items-center gap-3 mb-4">
          <div class="flex items-center justify-center w-10 h-10 rounded-full bg-rose-100 text-rose-600 shrink-0">
            <Lock :size="20" />
          </div>
          <div>
            <h3 class="text-lg font-bold text-heading">Xác nhận KHÓA Học kỳ</h3>
            <p class="text-xs text-rose-600 font-bold uppercase tracking-widest mt-0.5">Hành động nguy hiểm</p>
          </div>
        </div>

        <!-- Body -->
        <div class="space-y-4">
          <div class="p-3 bg-rose-50 dark:bg-rose-950/30 text-rose-800 dark:text-rose-400 rounded-xl text-xs space-y-2 border border-rose-100 dark:border-rose-500/20">
            <div class="flex items-center gap-1.5 font-bold text-sm">
              <AlertTriangle :size="15" />
              LƯU Ý CỰC KỲ QUAN TRỌNG:
            </div>
            <p class="leading-relaxed">
              Khóa học kỳ <strong class="text-heading text-rose-700 dark:text-rose-400">"{{ semesterToLock?.name }}"</strong> tại <strong class="text-heading text-rose-700 dark:text-rose-400">{{ semesterToLock?.campusName }}</strong> là hành động **không thể hoàn tác** từ giao diện quản trị thường.
            </p>
            <ul class="list-disc pl-4 space-y-1 mt-1 font-semibold">
              <li>Đóng băng toàn bộ điểm môn học của sinh viên trong kỳ.</li>
              <li>Chặn hoàn toàn việc chỉnh sửa, thay đổi bảng điểm môn và lớp học.</li>
              <li>Hệ thống tự động chạy Stored Procedure tính điểm GPA và khóa dữ liệu báo cáo PDF.</li>
            </ul>
          </div>

          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1.5">Lý do khóa học kỳ (Bắt buộc lưu Audit Log) *</label>
            <textarea
              v-model="lockReason"
              rows="3"
              class="glass-input w-full"
              placeholder="Nhập lý do chi tiết (VD: Đã kết thúc phúc khảo điểm ngày...)"
            ></textarea>
          </div>

          <label class="flex items-start gap-2.5 p-3 bg-slate-50 dark:bg-white/5 border border-default rounded-xl cursor-pointer">
            <input
              v-model="confirmCheck"
              type="checkbox"
              class="mt-1 accent-rose-600 rounded"
            />
            <div class="text-xs text-body font-semibold select-none leading-normal">
              Tôi xác nhận đã hoàn tất mọi thủ tục học vụ cho kỳ học này và cam kết chịu trách nhiệm về việc khóa dữ liệu bảng điểm.
            </div>
          </label>
        </div>

        <!-- Footer -->
        <div class="flex gap-3 justify-end mt-6 border-t border-default pt-4">
          <button @click="isLockModalOpen = false" class="glass-btn secondary">Hủy thao tác</button>
          <button
            @click="confirmLockSemester"
            class="glass-btn primary !bg-rose-600 hover:!bg-rose-700 !text-white"
            :disabled="!lockReason.trim() || !confirmCheck"
            :class="{'opacity-50 cursor-not-allowed': !lockReason.trim() || !confirmCheck}"
          >
            <Lock :size="14" />
            Xác nhận Khóa (Ghi Log)
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.semesters-page {
  font-family: inherit;
}

/* Glass panel */
.lg-glass-soft {
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  backdrop-filter: blur(12px);
}

/* Table styling */
table th {
  color: var(--text-label);
  border-bottom: 1px solid var(--border-default);
}
table td {
  border-bottom: 1px solid var(--border-default);
  color: var(--text-body);
}

/* Button & input styling */
.glass-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
}
.glass-btn.primary {
  background: var(--text-link);
  color: white;
}
.glass-btn.primary:hover {
  background: #1d4ed8;
  transform: translateY(-1px);
}
.glass-btn.primary:disabled {
  background: #93c5fd;
  transform: none;
}
.glass-btn.secondary {
  background: var(--surface-input);
  border-color: var(--border-input);
  color: var(--text-heading);
}
.glass-btn.secondary:hover {
  background: var(--surface-input-focus);
}

.glass-input, .glass-select {
  background: var(--surface-input);
  border: 1px solid var(--border-input);
  padding: 0.55rem 0.75rem;
  border-radius: 10px;
  color: var(--text-heading);
  font-size: 0.8rem;
  outline: none;
  transition: all 0.2s;
}
.glass-input:focus, .glass-select:focus {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
  background: var(--surface-input-focus);
}

/* Action button in table */
.action-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.4rem 0.6rem;
  border-radius: 8px;
  border: none;
  background: transparent;
  cursor: pointer;
  transition: all 0.2s;
}

/* Modal styling */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.4);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 50;
  padding: 1rem;
}
.modal-content {
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  border-radius: 20px;
  padding: 1.5rem;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
  backdrop-filter: blur(16px);
  animation: modalScale 0.25s ease-out;
}

@keyframes modalScale {
  from {
    opacity: 0;
    transform: scale(0.95);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}
</style>

<script setup>
/**
 * SubjectsView.vue - Super Admin
 * Quản trị danh mục môn học dùng chung cho toàn hệ thống (Module M6 & M12).
 * Hỗ trợ tạo môn học mới, chỉnh sửa thông tin, vô hiệu hóa môn học (chỉ cho phép chuyển trạng thái để lưu vết),
 * cấu hình tỷ lệ trọng số điểm thành phần và ghi Audit Log.
 */
import { ref, computed, watch } from 'vue'
import {
  BookOpen,
  Plus,
  Search,
  Edit2,
  Lock,
  Unlock,
  Settings,
  AlertCircle,
  RotateCcw,
  X,
  Save,
  Clock,
  History
} from 'lucide-vue-next'

// --- Mock Data ---

// Danh sách các Khoa (Department)
const departments = ref([
  { code: 'CNTT', name: 'Khoa Công nghệ Thông tin' },
  { code: 'KinhTe', name: 'Khoa Kinh tế' },
  { code: 'NgoaiNgu', name: 'Khoa Ngoại ngữ' },
  { code: 'KHCB', name: 'Khoa Khoa học Cơ bản' }
])

// Danh sách môn học chung
const subjects = ref([
  {
    code: 'PRF192',
    name: 'Nhập môn lập trình (C)',
    credits: 3,
    departmentCode: 'CNTT',
    departmentName: 'Khoa Công nghệ Thông tin',
    prerequisites: [],
    status: 'Active',
    gradeWeights: { attendance: 10, assignment: 20, midterm: 30, final: 40 }
  },
  {
    code: 'PRO192',
    name: 'Lập trình hướng đối tượng (Java)',
    credits: 3,
    departmentCode: 'CNTT',
    departmentName: 'Khoa Công nghệ Thông tin',
    prerequisites: ['PRF192'],
    status: 'Active',
    gradeWeights: { attendance: 10, assignment: 30, midterm: 20, final: 40 }
  },
  {
    code: 'DBI202',
    name: 'Cơ sở dữ liệu',
    credits: 3,
    departmentCode: 'CNTT',
    departmentName: 'Khoa Công nghệ Thông tin',
    prerequisites: [],
    status: 'Active',
    gradeWeights: { attendance: 10, assignment: 20, midterm: 20, final: 50 }
  },
  {
    code: 'MAE101',
    name: 'Toán cao cấp',
    credits: 3,
    departmentCode: 'KHCB',
    departmentName: 'Khoa Khoa học Cơ bản',
    prerequisites: [],
    status: 'Active',
    gradeWeights: { attendance: 10, assignment: 10, midterm: 30, final: 50 }
  },
  {
    code: 'MKT101',
    name: 'Nguyên lý Marketing',
    credits: 3,
    departmentCode: 'KinhTe',
    departmentName: 'Khoa Kinh tế',
    prerequisites: [],
    status: 'Active',
    gradeWeights: { attendance: 10, assignment: 20, midterm: 30, final: 40 }
  },
  {
    code: 'ENW492',
    name: 'Tiếng Anh chuyên ngành',
    credits: 3,
    departmentCode: 'NgoaiNgu',
    departmentName: 'Khoa Ngoại ngữ',
    prerequisites: [],
    status: 'Inactive',
    gradeWeights: { attendance: 10, assignment: 30, midterm: 20, final: 40 }
  }
])

// Audit Logs cho hoạt động thay đổi cấu hình môn
const auditLogs = ref([
  {
    id: 1,
    time: '2026-06-08 16:00:12',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    subjectCode: 'DBI202',
    details: 'Thay đổi tỷ lệ điểm thành phần: Bài tập (10% -> 20%), Cuối kỳ (60% -> 50%)',
    reason: 'Thống nhất cấu trúc đánh giá điểm của khoa'
  },
  {
    id: 2,
    time: '2026-06-03 14:15:30',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    subjectCode: 'ENW492',
    details: 'Vô hiệu hóa môn học (Chuyển sang Inactive)',
    reason: 'Môn học cũ được thay thế bằng mã môn ENW493 mới'
  },
  {
    id: 3,
    time: '2026-05-28 09:30:00',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    subjectCode: 'PRO192',
    details: 'Cập nhật môn tiên quyết mặc định thành [PRF192]',
    reason: 'Đảm bảo trình tự kiến thức môn lập trình'
  }
])

// --- State & Filters ---
const searchQuery = ref('')
const selectedDept = ref('all')
const selectedStatus = ref('all')

// --- Lọc dữ liệu ---
const filteredSubjects = computed(() => {
  return subjects.value.filter(s => {
    const matchSearch = s.code.toLowerCase().includes(searchQuery.value.toLowerCase()) || s.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchDept = selectedDept.value === 'all' || s.departmentCode === selectedDept.value
    const matchStatus = selectedStatus.value === 'all' || s.status === selectedStatus.value
    return matchSearch && matchDept && matchStatus
  })
})

// --- KPI Stats ---
const totalSubjects = computed(() => subjects.value.length)
const activeSubjects = computed(() => subjects.value.filter(s => s.status === 'Active').length)
const inactiveSubjects = computed(() => subjects.value.filter(s => s.status === 'Inactive').length)

// --- State Form Modal (Tạo / Sửa môn học) ---
const isFormModalOpen = ref(false)
const isEditMode = ref(false)
const form = ref({
  code: '',
  name: '',
  credits: 3,
  departmentCode: '',
  prerequisites: [],
  status: 'Active'
})

const errors = ref({ code: '', name: '', department: '' })

watch(form, (newVal) => {
  errors.value = { code: '', name: '', department: '' }
  if (isFormModalOpen.value) {
    if (!newVal.code.trim()) {
      errors.value.code = 'Mã môn học không được để trống.'
    } else if (!isEditMode.value && subjects.value.some(s => s.code.toUpperCase() === newVal.code.trim().toUpperCase())) {
      errors.value.code = 'Mã môn học này đã tồn tại trên hệ thống.'
    }
    if (!newVal.name.trim()) {
      errors.value.name = 'Tên môn học không được để trống.'
    }
    if (!newVal.departmentCode) {
      errors.value.department = 'Vui lòng chọn khoa quản lý.'
    }
  }
}, { deep: true })

const isFormValid = computed(() => {
  return form.value.code.trim() &&
         form.value.name.trim() &&
         form.value.departmentCode &&
         !errors.value.code
})

// --- State Grade Weight Modal (Cấu hình trọng số điểm) ---
const isWeightModalOpen = ref(false)
const targetWeightSubject = ref(null)
const weightForm = ref({
  attendance: 10,
  assignment: 20,
  midterm: 20,
  final: 50
})
const weightReason = ref('')

const totalWeightPercentage = computed(() => {
  return (weightForm.value.attendance || 0) +
         (weightForm.value.assignment || 0) +
         (weightForm.value.midterm || 0) +
         (weightForm.value.final || 0)
})

const isWeightValid = computed(() => totalWeightPercentage.value === 100)

// --- State Confirm Inactive Modal (Xác nhận vô hiệu hóa) ---
const isInactiveConfirmModalOpen = ref(false)
const targetInactiveSubject = ref(null)
const inactiveReason = ref('')

// --- Handlers ---

const openCreateModal = () => {
  isEditMode.value = false
  form.value = {
    code: '',
    name: '',
    credits: 3,
    departmentCode: departments.value[0]?.code || '',
    prerequisites: [],
    status: 'Active'
  }
  errors.value = { code: '', name: '', department: '' }
  isFormModalOpen.value = true
}

const openEditModal = (sub) => {
  isEditMode.value = true
  form.value = {
    ...sub,
    prerequisites: [...sub.prerequisites]
  }
  errors.value = { code: '', name: '', department: '' }
  isFormModalOpen.value = true
}

const saveSubject = () => {
  if (!isFormValid.value) return

  const deptName = departments.value.find(d => d.code === form.value.departmentCode)?.name || 'Khoa mới'

  if (isEditMode.value) {
    const idx = subjects.value.findIndex(s => s.code === form.value.code)
    if (idx !== -1) {
      subjects.value[idx] = {
        ...subjects.value[idx],
        name: form.value.name,
        credits: form.value.credits,
        departmentCode: form.value.departmentCode,
        departmentName: deptName,
        prerequisites: [...form.value.prerequisites]
      }
      addAuditLog(form.value.code, `Cập nhật thông tin học thuật của môn: ${form.value.name}`, 'Định kỳ sửa đổi khung chương trình đào tạo')
    }
  } else {
    const newSub = {
      code: form.value.code.trim().toUpperCase(),
      name: form.value.name.trim(),
      credits: form.value.credits,
      departmentCode: form.value.departmentCode,
      departmentName: deptName,
      prerequisites: [...form.value.prerequisites],
      status: 'Active',
      gradeWeights: { attendance: 10, assignment: 20, midterm: 20, final: 50 } // mặc định
    }
    subjects.value.unshift(newSub)
    addAuditLog(newSub.code, `Tạo mới môn học hệ thống: ${newSub.name}`, 'Bổ sung danh mục môn học đào tạo mới')
  }

  isFormModalOpen.value = false
}

// Mở modal cấu hình trọng số điểm
const openWeightModal = (sub) => {
  targetWeightSubject.value = sub
  weightForm.value = { ...sub.gradeWeights }
  weightReason.value = ''
  isWeightModalOpen.value = true
}

const saveWeights = () => {
  if (!isWeightValid.value || !targetWeightSubject.value) return
  if (!weightReason.value.trim()) {
    alert('Vui lòng nhập lý do điều chỉnh trọng số điểm để ghi nhận Audit Log.')
    return
  }

  const sub = subjects.value.find(s => s.code === targetWeightSubject.value.code)
  if (sub) {
    const oldWeights = { ...sub.gradeWeights }
    sub.gradeWeights = { ...weightForm.value }

    const detailText = `Cấu hình lại tỷ lệ điểm: Chuyên cần (${oldWeights.attendance}% -> ${sub.gradeWeights.attendance}%), Bài tập (${oldWeights.assignment}% -> ${sub.gradeWeights.assignment}%), Giữa kỳ (${oldWeights.midterm}% -> ${sub.gradeWeights.midterm}%), Cuối kỳ (${oldWeights.final}% -> ${sub.gradeWeights.final}%)`
    addAuditLog(sub.code, detailText, weightReason.value)
  }

  isWeightModalOpen.value = false
  targetWeightSubject.value = null
}

// Mở Confirm Inactive Modal
const openInactiveModal = (sub) => {
  targetInactiveSubject.value = sub
  inactiveReason.value = ''
  isInactiveConfirmModalOpen.value = true
}

// Thực thi chuyển trạng thái môn học sang Inactive / Active
const confirmToggleStatus = () => {
  if (targetInactiveSubject.value) {
    const sub = subjects.value.find(s => s.code === targetInactiveSubject.value.code)
    if (sub) {
      if (sub.status === 'Active') {
        if (!inactiveReason.value.trim()) {
          alert('Vui lòng nhập lý do vô hiệu hóa môn học.')
          return
        }
        sub.status = 'Inactive'
        addAuditLog(sub.code, 'Vô hiệu hóa môn học (Chuyển trạng thái sang Inactive)', inactiveReason.value)
      } else {
        sub.status = 'Active'
        addAuditLog(sub.code, 'Kích hoạt lại môn học (Chuyển trạng thái sang Active)', 'Khai thác lại danh mục môn học đào tạo')
      }
    }
  }

  isInactiveConfirmModalOpen.value = false
  targetInactiveSubject.value = null
}

// Helper thêm Audit Log
const addAuditLog = (subjectCode, details, reason) => {
  const now = new Date()
  const timeStr = now.toLocaleString('sv-SE', { timeZone: 'Asia/Ho_Chi_Minh' }).replace('T', ' ')
  auditLogs.value.unshift({
    id: auditLogs.value.length ? Math.max(...auditLogs.value.map(l => l.id)) + 1 : 1,
    time: timeStr,
    actor: 'Super Admin (admin@fpt.edu.vn)',
    subjectCode,
    details,
    reason
  })
}

// Trạng thái badge
const getStatusBadge = (status) => {
  if (status === 'Active') return { class: 'bg-emerald-50 text-emerald-700 border-emerald-200/50 dark:bg-emerald-600/10 dark:text-emerald-400 dark:border-emerald-500/20', label: 'Đang hoạt động' }
  return { class: 'bg-rose-50 text-rose-700 border-rose-200/50 dark:bg-rose-600/10 dark:text-rose-400 dark:border-rose-500/20', label: 'Vô hiệu hóa' }
}

const resetFilters = () => {
  searchQuery.value = ''
  selectedDept.value = 'all'
  selectedStatus.value = 'all'
}
</script>

<template>
  <div class="subjects-page pb-12 space-y-6">
    <!-- Header -->
    <header class="page-header flex flex-col md:flex-row md:items-center justify-between gap-4 border-b border-default pb-4">
      <div>
        <h1 class="text-2xl font-bold text-heading">Quản lý Môn học Hệ thống (M6 & M12)</h1>
        <p class="text-sm text-label mt-1">Danh mục môn học dùng chung toàn trường: Thiết lập tín chỉ, khoa phụ trách, môn tiên quyết mặc định và trọng số điểm.</p>
      </div>
      <div class="flex items-center gap-3">
        <button @click="openCreateModal" class="glass-btn primary shadow-sm">
          <Plus :size="16" /> Tạo môn học mới
        </button>
      </div>
    </header>

    <!-- KPI Mini Panel -->
    <div class="grid grid-cols-3 gap-4">
      <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-4 shadow-sm">
        <p class="text-xs font-bold text-label uppercase tracking-wide">Tổng số môn học</p>
        <div class="flex items-baseline gap-2 mt-1">
          <span class="text-3xl font-extrabold text-heading">{{ totalSubjects }}</span>
          <span class="text-[10px] text-placeholder">Môn học đào tạo</span>
        </div>
      </div>
      <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-4 shadow-sm border-l-4 border-l-emerald-500">
        <p class="text-xs font-bold text-label uppercase tracking-wide">Đang hoạt động (Active)</p>
        <div class="flex items-baseline gap-2 mt-1">
          <span class="text-3xl font-extrabold text-emerald-600 dark:text-emerald-400">
            {{ activeSubjects }}
          </span>
          <span class="text-[10px] text-placeholder">Có thể mở lớp học</span>
        </div>
      </div>
      <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-4 shadow-sm border-l-4 border-l-rose-500">
        <p class="text-xs font-bold text-label uppercase tracking-wide">Vô hiệu hóa (Inactive)</p>
        <div class="flex items-baseline gap-2 mt-1">
          <span class="text-3xl font-extrabold text-rose-600 dark:text-rose-400">
            {{ inactiveSubjects }}
          </span>
          <span class="text-[10px] text-placeholder">Đóng để lưu vết lịch sử</span>
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
            placeholder="Tìm theo mã hoặc tên môn..."
            class="glass-input pl-9 w-full"
          />
        </div>

        <!-- Department filter -->
        <div class="flex items-center gap-1.5 w-full sm:w-auto">
          <span class="text-xs text-label font-bold whitespace-nowrap hidden lg:inline">Khoa quản lý:</span>
          <select v-model="selectedDept" class="glass-select w-full sm:w-48">
            <option value="all">Tất cả các khoa</option>
            <option v-for="d in departments" :key="d.code" :value="d.code">{{ d.name }}</option>
          </select>
        </div>

        <!-- Status filter -->
        <div class="flex items-center gap-1.5 w-full sm:w-auto">
          <span class="text-xs text-label font-bold whitespace-nowrap hidden lg:inline">Trạng thái:</span>
          <select v-model="selectedStatus" class="glass-select w-full sm:w-36">
            <option value="all">Tất cả trạng thái</option>
            <option value="Active">Đang hoạt động</option>
            <option value="Inactive">Vô hiệu hóa</option>
          </select>
        </div>
      </div>

      <button @click="resetFilters" class="glass-btn secondary shrink-0 self-end md:self-auto justify-center">
        <RotateCcw :size="14" /> Xóa bộ lọc
      </button>
    </div>

    <!-- Subjects Table -->
    <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 shadow-sm overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse text-sm">
          <thead>
            <tr class="bg-slate-50/50 dark:bg-white/5 border-b border-default text-[11px] font-bold text-label uppercase tracking-widest">
              <th class="p-4">Mã môn</th>
              <th class="p-4">Tên môn học</th>
              <th class="p-4 text-center">Tín chỉ</th>
              <th class="p-4">Khoa phụ trách</th>
              <th class="p-4">Tiên quyết mặc định</th>
              <th class="p-4">Trọng số đánh giá điểm (%)</th>
              <th class="p-4 text-center">Trạng thái</th>
              <th class="p-4 text-right">Thao tác quản trị</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-if="filteredSubjects.length === 0">
              <td colspan="8" class="p-8 text-center text-placeholder">
                Không tìm thấy môn học nào phù hợp với bộ lọc.
              </td>
            </tr>
            <tr
              v-for="sub in filteredSubjects"
              :key="sub.code"
              class="hover:bg-white/40 dark:hover:bg-white/5 transition-all group"
            >
              <!-- Mã môn -->
              <td class="p-4 font-mono font-bold text-heading">{{ sub.code }}</td>
              <!-- Tên môn -->
              <td class="p-4 font-semibold text-body">
                <div class="flex items-center gap-2">
                  <BookOpen :size="16" class="text-blue-500 shrink-0" />
                  {{ sub.name }}
                </div>
              </td>
              <!-- Tín chỉ -->
              <td class="p-4 text-center font-bold text-heading">{{ sub.credits }}</td>
              <!-- Khoa phụ trách -->
              <td class="p-4 text-body font-medium">{{ sub.departmentName }}</td>
              <!-- Môn tiên quyết mặc định -->
              <td class="p-4">
                <div class="flex flex-wrap gap-1">
                  <template v-if="sub.prerequisites.length > 0">
                    <span
                      v-for="p in sub.prerequisites"
                      :key="p"
                      class="px-2 py-0.5 rounded bg-rose-50 text-rose-700 border border-rose-200/50 dark:bg-rose-950/20 dark:text-rose-400 font-mono text-[9px] font-bold"
                    >
                      {{ p }}
                    </span>
                  </template>
                  <span v-else class="text-placeholder text-xs italic">Không</span>
                </div>
              </td>
              <!-- Trọng số điểm -->
              <td class="p-4">
                <div class="flex flex-wrap gap-1.5 items-center text-[10px] font-semibold text-body">
                  <span class="px-1.5 py-0.5 rounded bg-blue-50 dark:bg-blue-950/30 text-blue-700 dark:text-blue-400 border border-blue-100 dark:border-blue-900/30">
                    Chuyên cần: {{ sub.gradeWeights.attendance }}%
                  </span>
                  <span class="px-1.5 py-0.5 rounded bg-indigo-50 dark:bg-indigo-950/30 text-indigo-700 dark:text-indigo-400 border border-indigo-100 dark:border-indigo-900/30">
                    Bài tập: {{ sub.gradeWeights.assignment }}%
                  </span>
                  <span class="px-1.5 py-0.5 rounded bg-amber-50 dark:bg-amber-950/30 text-amber-700 dark:text-amber-400 border border-amber-100 dark:border-amber-900/30">
                    Giữa kỳ: {{ sub.gradeWeights.midterm }}%
                  </span>
                  <span class="px-1.5 py-0.5 rounded bg-emerald-50 dark:bg-emerald-950/30 text-emerald-700 dark:text-emerald-400 border border-emerald-100 dark:border-emerald-900/30">
                    Cuối kỳ: {{ sub.gradeWeights.final }}%
                  </span>
                </div>
              </td>
              <!-- Trạng thái -->
              <td class="p-4 text-center">
                <span :class="['px-2.5 py-0.5 rounded-full text-xs font-bold border inline-block', getStatusBadge(sub.status).class]">
                  {{ getStatusBadge(sub.status).label }}
                </span>
              </td>
              <!-- Nút hành động -->
              <td class="p-4 text-right">
                <div class="flex items-center justify-end gap-1">
                  <!-- Trọng số -->
                  <button
                    @click="openWeightModal(sub)"
                    class="p-1.5 text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-900/20 rounded transition-all"
                    title="Cấu hình trọng số điểm thành phần"
                    :disabled="sub.status === 'Inactive'"
                  >
                    <Settings :size="14" />
                  </button>
                  <!-- Sửa thông tin -->
                  <button
                    @click="openEditModal(sub)"
                    class="p-1.5 text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded transition-all"
                    title="Chỉnh sửa thông tin"
                    :disabled="sub.status === 'Inactive'"
                  >
                    <Edit2 :size="14" />
                  </button>
                  <!-- Đổi trạng thái (Active / Inactive) -->
                  <button
                    @click="openInactiveModal(sub)"
                    class="p-1.5 rounded transition-all"
                    :class="sub.status === 'Active' ? 'text-rose-500 hover:bg-rose-50 dark:hover:bg-rose-900/20' : 'text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/20'"
                    :title="sub.status === 'Active' ? 'Vô hiệu hóa môn học' : 'Kích hoạt môn học'"
                  >
                    <component :is="sub.status === 'Active' ? Lock : Unlock" :size="14" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Audit Logs Section -->
    <div class="lg-glass-soft p-5 rounded-2xl border border-white/60 dark:border-white/10 shadow-sm space-y-4">
      <div class="flex items-center justify-between border-b border-default pb-3">
        <h3 class="font-bold text-heading flex items-center gap-2">
          <History :size="18" class="text-violet-500" />
          Nhật ký thay đổi môn học & trọng số (Audit Log)
        </h3>
        <span class="text-[10px] uppercase font-bold text-label bg-slate-100 dark:bg-white/10 px-2 py-0.5 rounded-md">Bảo toàn dữ liệu học vụ</span>
      </div>

      <div class="space-y-3 max-h-[250px] overflow-y-auto pr-2">
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
              <span class="font-bold text-heading">Môn học: {{ log.subjectCode }}</span>
              <span class="text-[10px] text-placeholder font-mono">{{ log.time }}</span>
            </div>
            <p class="text-body font-semibold mt-1">{{ log.details }}</p>
            <p class="text-placeholder mt-0.5">Lý do thay đổi: <span class="italic text-body font-medium">"{{ log.reason }}"</span></p>
            <p class="text-[10px] text-placeholder mt-1">Người vận hành: <span class="font-mono">{{ log.actor }}</span></p>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal: Tạo mới / Chỉnh sửa môn học -->
    <div v-if="isFormModalOpen" class="modal-overlay">
      <div class="modal-content max-w-lg w-full">
        <!-- Header -->
        <div class="flex items-center justify-between border-b border-default pb-4 mb-5">
          <div class="flex items-center gap-2.5">
            <div class="w-9 h-9 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center">
              <BookOpen :size="18" />
            </div>
            <div>
              <h3 class="text-lg font-bold text-heading">{{ isEditMode ? 'Chỉnh sửa môn học' : 'Tạo môn học mới' }}</h3>
              <p class="text-xs text-label uppercase tracking-widest font-semibold mt-0.5">Thiết lập danh mục dùng chung</p>
            </div>
          </div>
          <button @click="isFormModalOpen = false" class="p-1 hover:bg-slate-100 dark:hover:bg-white/10 rounded">
            <X :size="18" />
          </button>
        </div>

        <!-- Body -->
        <div class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Mã môn học *</label>
              <input
                v-model="form.code"
                type="text"
                class="glass-input w-full uppercase"
                placeholder="VD: DBI202, PRO192..."
                :disabled="isEditMode"
                :class="{'border-rose-300': errors.code}"
              />
              <p v-if="errors.code" class="text-rose-500 text-[10px] mt-1 font-semibold">{{ errors.code }}</p>
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Số tín chỉ (Credit) *</label>
              <input
                v-model.number="form.credits"
                type="number"
                min="1"
                max="15"
                class="glass-input w-full"
              />
            </div>
          </div>

          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1.5">Tên môn học *</label>
            <input
              v-model="form.name"
              type="text"
              class="glass-input w-full"
              placeholder="VD: Cơ sở dữ liệu, Lập trình..."
              :class="{'border-rose-300': errors.name}"
            />
            <p v-if="errors.name" class="text-rose-500 text-[10px] mt-1 font-semibold">{{ errors.name }}</p>
          </div>

          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1.5">Khoa phụ trách quản lý *</label>
            <select v-model="form.departmentCode" class="glass-select w-full" :class="{'border-rose-300': errors.department}">
              <option value="">-- Chọn Khoa phụ trách --</option>
              <option v-for="d in departments" :key="d.code" :value="d.code">{{ d.name }}</option>
            </select>
            <p v-if="errors.department" class="text-rose-500 text-[10px] mt-1 font-semibold">{{ errors.department }}</p>
          </div>

          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1.5">Môn học tiên quyết mặc định (Không bắt buộc)</label>
            <div class="p-3 border border-default rounded-xl bg-slate-50/50 dark:bg-white/5 max-h-[140px] overflow-y-auto space-y-1.5">
              <label
                v-for="sub in subjects.filter(s => s.code !== form.code)"
                :key="sub.code"
                class="flex items-center gap-2 cursor-pointer text-xs select-none"
              >
                <input
                  type="checkbox"
                  v-model="form.prerequisites"
                  :value="sub.code"
                  class="rounded accent-blue-600"
                />
                <span class="font-mono font-semibold">{{ sub.code }}</span> - <span>{{ sub.name }}</span>
              </label>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="flex gap-3 justify-end border-t border-default pt-4 mt-6">
          <button @click="isFormModalOpen = false" class="glass-btn secondary">Hủy</button>
          <button
            @click="saveSubject"
            class="glass-btn primary"
            :disabled="!isFormValid"
            :class="{'opacity-50 cursor-not-allowed': !isFormValid}"
          >
            <Save :size="14" /> {{ isEditMode ? 'Lưu cập nhật' : 'Khởi tạo môn' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal: Cấu hình trọng số điểm (Grade Weight Config) -->
    <div v-if="isWeightModalOpen" class="modal-overlay">
      <div class="modal-content max-w-md w-full">
        <!-- Header -->
        <div class="flex items-center justify-between border-b border-default pb-4 mb-5">
          <div class="flex items-center gap-2.5">
            <div class="w-9 h-9 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center">
              <Settings :size="18" />
            </div>
            <div>
              <h3 class="text-lg font-bold text-heading">Cấu hình Trọng số Điểm</h3>
              <p class="text-xs text-label uppercase tracking-widest font-semibold mt-0.5">Môn học: {{ targetWeightSubject?.code }}</p>
            </div>
          </div>
          <button @click="isWeightModalOpen = false" class="p-1 hover:bg-slate-100 dark:hover:bg-white/10 rounded">
            <X :size="18" />
          </button>
        </div>

        <!-- Body -->
        <div class="space-y-4">
          <p class="text-xs text-label leading-normal">
            Quy định tỷ lệ phần trăm điểm cho từng cột điểm thành phần của sinh viên.
          </p>

          <div class="grid grid-cols-2 gap-4">
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Điểm Chuyên cần (%)</label>
              <input
                v-model.number="weightForm.attendance"
                type="number"
                min="0"
                max="100"
                class="glass-input w-full"
              />
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Điểm Bài tập / Thực hành (%)</label>
              <input
                v-model.number="weightForm.assignment"
                type="number"
                min="0"
                max="100"
                class="glass-input w-full"
              />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Điểm Giữa kỳ (%)</label>
              <input
                v-model.number="weightForm.midterm"
                type="number"
                min="0"
                max="100"
                class="glass-input w-full"
              />
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Điểm Cuối kỳ (%)</label>
              <input
                v-model.number="weightForm.final"
                type="number"
                min="0"
                max="100"
                class="glass-input w-full"
              />
            </div>
          </div>

          <!-- Progress bar & Validation -->
          <div class="space-y-2 pt-2 border-t border-default">
            <div class="flex items-center justify-between text-xs font-bold">
              <span class="text-label">Tổng tỷ lệ phần trăm:</span>
              <span :class="isWeightValid ? 'text-emerald-600 dark:text-emerald-400' : 'text-rose-500'">
                {{ totalWeightPercentage }}% / 100%
              </span>
            </div>
            <!-- Progress Bar -->
            <div class="h-2.5 w-full rounded-full bg-slate-100 dark:bg-slate-800 overflow-hidden">
              <div
                class="h-full transition-all duration-300"
                :class="isWeightValid ? 'bg-emerald-500' : 'bg-rose-500'"
                :style="`width: ${Math.min(totalWeightPercentage, 100)}%`"
              ></div>
            </div>

            <!-- Error panel if invalid -->
            <div v-if="!isWeightValid" class="p-3 rounded-xl border border-rose-200 bg-rose-50/50 dark:border-rose-500/20 dark:bg-rose-950/10 text-[10.5px] text-rose-800 dark:text-rose-400 flex items-start gap-2.5">
              <AlertCircle :size="14" class="shrink-0 mt-0.5" />
              <div>
                <span class="font-bold">Lỗi ràng buộc học vụ:</span>
                <p class="mt-0.5 font-semibold">
                  Tổng các thành phần điểm bắt buộc phải đạt đúng **100%** để hệ thống Bảng điểm (M6) tính điểm trung bình chính xác.
                </p>
              </div>
            </div>
          </div>

          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1.5">Lý do điều chỉnh (Bắt buộc ghi Log) *</label>
            <textarea
              v-model="weightReason"
              rows="3"
              class="glass-input w-full"
              placeholder="Nhập lý do thay đổi cơ cấu điểm..."
            ></textarea>
          </div>
        </div>

        <!-- Footer -->
        <div class="flex gap-3 justify-end border-t border-default pt-4 mt-6">
          <button @click="isWeightModalOpen = false" class="glass-btn secondary">Hủy</button>
          <button
            @click="saveWeights"
            class="glass-btn primary"
            :disabled="!isWeightValid || !weightReason.trim()"
            :class="{'opacity-50 cursor-not-allowed': !isWeightValid || !weightReason.trim()}"
          >
            <Save :size="14" /> Cập nhật (Ghi Log)
          </button>
        </div>
      </div>
    </div>

    <!-- Confirm Inactive / Active Modal -->
    <div v-if="isInactiveConfirmModalOpen" class="modal-overlay">
      <div class="modal-content max-w-md w-full border" :class="targetInactiveSubject?.status === 'Active' ? 'border-rose-200 dark:border-rose-500/30' : 'border-emerald-200 dark:border-emerald-500/30'">
        <!-- Header -->
        <div class="flex items-center gap-3 mb-4">
          <div class="flex items-center justify-center w-10 h-10 rounded-full shrink-0 animate-pulse" :class="targetInactiveSubject?.status === 'Active' ? 'bg-rose-100 text-rose-600' : 'bg-emerald-100 text-emerald-600'">
            <component :is="targetInactiveSubject?.status === 'Active' ? Lock : Unlock" :size="20" />
          </div>
          <div>
            <h3 class="text-lg font-bold text-heading">
              {{ targetInactiveSubject?.status === 'Active' ? 'Vô hiệu hóa môn học' : 'Kích hoạt lại môn học' }}
            </h3>
            <p class="text-xs text-label font-bold uppercase tracking-widest mt-0.5">Môn: {{ targetInactiveSubject?.code }}</p>
          </div>
        </div>

        <!-- Body -->
        <div class="space-y-4">
          <!-- Active -> Inactive Cảnh báo ràng buộc toàn vẹn dữ liệu -->
          <template v-if="targetInactiveSubject?.status === 'Active'">
            <div class="p-3 bg-rose-50 dark:bg-rose-950/30 text-rose-800 dark:text-rose-400 rounded-xl text-xs space-y-2 border border-rose-100 dark:border-rose-500/20 leading-relaxed font-semibold">
              <div class="flex items-center gap-1.5 font-bold text-sm">
                <AlertCircle :size="15" />
                Ràng buộc bảo toàn dữ liệu học vụ:
              </div>
              <p>
                Môn học <strong class="text-rose-700 dark:text-rose-400">"{{ targetInactiveSubject?.name }}"</strong> không được phép xóa vật lý ra khỏi hệ thống nếu đã có lớp học, điểm thi hoặc nằm trong thời khóa biểu/chương trình đào tạo cũ.
              </p>
              <p>
                Thao tác này sẽ chuyển đổi trạng thái sang **Vô hiệu hóa (Inactive)**. Hệ thống sẽ chặn việc tạo mới các lớp học cho môn này nhưng vẫn bảo toàn lịch sử học tập của sinh viên cũ.
              </p>
            </div>

            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Lý do vô hiệu hóa môn học *</label>
              <textarea
                v-model="inactiveReason"
                rows="3"
                class="glass-input w-full"
                placeholder="Nhập lý do đóng môn học..."
              ></textarea>
            </div>
          </template>

          <!-- Inactive -> Active -->
          <template v-else>
            <p class="text-xs text-body font-semibold leading-relaxed">
              Xác nhận kích hoạt lại môn học <strong class="text-heading">"{{ targetInactiveSubject?.name }}"</strong>. Sau khi kích hoạt, các khoa có thể tiếp tục mở lớp giảng dạy và sinh viên được đăng ký môn học này bình thường.
            </p>
          </template>
        </div>

        <!-- Footer -->
        <div class="flex gap-3 justify-end mt-6 border-t border-default pt-4">
          <button @click="isInactiveConfirmModalOpen = false" class="glass-btn secondary">Hủy thao tác</button>
          <button
            @click="confirmToggleStatus"
            class="glass-btn primary"
            :class="targetInactiveSubject?.status === 'Active' ? '!bg-rose-600 hover:!bg-rose-700 !text-white' : '!bg-emerald-600 hover:!bg-emerald-700 !text-white'"
            :disabled="targetInactiveSubject?.status === 'Active' && !inactiveReason.trim()"
          >
            Xác nhận thay đổi
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.subjects-page {
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

<script setup>
/**
 * ProgramsView.vue - Super Admin
 * Quản lý Cấu trúc chương trình đào tạo (Training Programs - Module M6 & M12).
 * Hỗ trợ tạo mới CTĐT, quản lý danh sách môn học, cấu hình môn tiên quyết (Prerequisite Drawer),
 * sắp xếp học kỳ gợi ý và ghi Audit Log.
 */
import { ref, computed, watch } from 'vue'
import {
  BookOpen,
  Plus,
  Search,
  Folder,
  ChevronRight,
  ChevronDown,
  Trash2,
  Archive,
  Check,
  X,
  Save,
  PlusCircle,
  History,
  Clock,
  Settings,
  AlertCircle,
  CheckCircle,
  RotateCcw
} from 'lucide-vue-next'

// --- Mock Data ---

// Danh sách các Ngành học
const majors = ref([
  { code: 'IT', name: 'Công nghệ Thông tin' },
  { code: 'BA', name: 'Quản trị Kinh doanh' },
  { code: 'GD', name: 'Thiết kế Đồ họa' }
])

// Danh sách các Khóa học
const batches = ref(['K17', 'K18', 'K19'])

// Danh sách Chương trình Đào tạo (CTĐT)
const programs = ref([
  {
    id: 1,
    code: 'IT_SE_K19',
    name: 'Kỹ thuật Phần mềm - Khóa 19',
    majorCode: 'IT',
    batch: 'K19',
    status: 'Active',
    totalCredits: 120,
    metrics: { students: 350, subjects: 42 }
  },
  {
    id: 2,
    code: 'IT_IA_K19',
    name: 'An toàn Thông tin - Khóa 19',
    majorCode: 'IT',
    batch: 'K19',
    status: 'Active',
    totalCredits: 124,
    metrics: { students: 180, subjects: 43 }
  },
  {
    id: 3,
    code: 'BA_MKT_K19',
    name: 'Digital Marketing - Khóa 19',
    majorCode: 'BA',
    batch: 'K19',
    status: 'Active',
    totalCredits: 118,
    metrics: { students: 240, subjects: 38 }
  },
  {
    id: 4,
    code: 'GD_3D_K18',
    name: 'Thiết kế Mỹ thuật 3D - Khóa 18',
    majorCode: 'GD',
    batch: 'K18',
    status: 'Active',
    totalCredits: 115,
    metrics: { students: 150, subjects: 36 }
  },
  {
    id: 5,
    code: 'IT_SE_K17',
    name: 'Kỹ thuật Phần mềm - Khóa 17',
    majorCode: 'IT',
    batch: 'K17',
    status: 'Archived',
    totalCredits: 120,
    metrics: { students: 0, subjects: 42 }
  }
])

// Danh sách môn học chung của trường (dành cho việc thêm môn học mới vào chương trình)
const globalSubjects = ref([
  { code: 'OSG202', name: 'Hệ điều hành (Operating Systems)', credits: 3 },
  { code: 'NWC203c', name: 'Mạng máy tính (Computer Networks)', credits: 3 },
  { code: 'JPD111', name: 'Tiếng Nhật sơ cấp 1', credits: 4 },
  { code: 'SSG104', name: 'Kỹ năng giao tiếp chuyên nghiệp', credits: 2 },
  { code: 'IOT102', name: 'Nhập môn Internet of Things', credits: 3 },
  { code: 'PRN211', name: 'Lập trình ứng dụng với C#', credits: 3 }
])

// Chi tiết danh sách môn học của từng chương trình theo ProgramId
const programSubjects = ref({
  1: [
    { code: 'PRF192', name: 'Nhập môn lập trình (C)', credits: 3, semester: 1, prerequisites: [] },
    { code: 'MAD191', name: 'Toán rời rạc', credits: 3, semester: 1, prerequisites: [] },
    { code: 'PRO192', name: 'Lập trình hướng đối tượng (Java)', credits: 3, semester: 2, prerequisites: ['PRF192'] },
    { code: 'DBI202', name: 'Cơ sở dữ liệu', credits: 3, semester: 3, prerequisites: [] },
    { code: 'SWE302', name: 'Kỹ thuật phần mềm', credits: 3, semester: 4, prerequisites: ['PRO192'] },
    { code: 'GRD301', name: 'Đồ án tốt nghiệp SE', credits: 10, semester: 7, prerequisites: ['SWE302', 'DBI202'] }
  ],
  2: [
    { code: 'PRF192', name: 'Nhập môn lập trình (C)', credits: 3, semester: 1, prerequisites: [] },
    { code: 'MAD191', name: 'Toán rời rạc', credits: 3, semester: 1, prerequisites: [] },
    { code: 'PRO192', name: 'Lập trình hướng đối tượng (Java)', credits: 3, semester: 2, prerequisites: ['PRF192'] },
    { code: 'DBI202', name: 'Cơ sở dữ liệu', credits: 3, semester: 3, prerequisites: [] },
    { code: 'IAS201', name: 'Cơ sở an toàn thông tin', credits: 3, semester: 4, prerequisites: ['PRO192'] },
    { code: 'GRD302', name: 'Đồ án tốt nghiệp IA', credits: 10, semester: 7, prerequisites: ['IAS201', 'DBI202'] }
  ],
  3: [
    { code: 'MKT101', name: 'Nguyên lý Marketing', credits: 3, semester: 1, prerequisites: [] },
    { code: 'BAA201', name: 'Quản trị học đại cương', credits: 3, semester: 2, prerequisites: [] },
    { code: 'MKT202', name: 'Hành vi người tiêu dùng', credits: 3, semester: 3, prerequisites: ['MKT101'] },
    { code: 'DMK301', name: 'Digital Marketing Toolkits', credits: 4, semester: 4, prerequisites: ['MKT202'] },
    { code: 'GRD305', name: 'Khóa luận tốt nghiệp MKT', credits: 10, semester: 7, prerequisites: ['DMK301'] }
  ],
  4: [
    { code: 'GDG101', name: 'Mỹ thuật cơ bản', credits: 3, semester: 1, prerequisites: [] },
    { code: 'GDA201', name: 'Thiết kế đồ họa 2D', credits: 3, semester: 2, prerequisites: ['GDG101'] },
    { code: 'GDM302', name: 'Dựng hình 3D cơ bản (Maya)', credits: 4, semester: 3, prerequisites: ['GDA201'] },
    { code: 'GDM401', name: 'Hoạt hình 3D nâng cao', credits: 4, semester: 4, prerequisites: ['GDM302'] },
    { code: 'GRD308', name: 'Đồ án tốt nghiệp 3D Art', credits: 10, semester: 7, prerequisites: ['GDM401'] }
  ]
})

// Audit Logs cho hoạt động thay đổi chương trình
const auditLogs = ref([
  {
    id: 1,
    time: '2026-06-08 15:10:00',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    programCode: 'IT_SE_K19',
    details: 'Đã thêm môn tiên quyết [SWE302, DBI202] cho môn [GRD301]',
    reason: 'Đảm bảo sinh viên hoàn thành nền tảng trước khi làm đồ án'
  },
  {
    id: 2,
    time: '2026-06-05 10:20:15',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    programCode: 'BA_MKT_K19',
    details: 'Điều chỉnh số tín chỉ môn [DMK301] từ 3 tín chỉ thành 4 tín chỉ',
    reason: 'Bổ sung thêm thời lượng thực hành phần mềm Marketing'
  },
  {
    id: 3,
    time: '2026-06-01 09:00:00',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    programCode: 'IT_SE_K17',
    details: 'Lưu trữ (Archived) chương trình đào tạo Kỹ thuật Phần mềm K17',
    reason: 'Khóa sinh viên đã tốt nghiệp hoàn toàn, lưu trữ hồ sơ học thuật'
  }
])

// --- State & Filters ---
const searchQuery = ref('')
const selectedMajor = ref('all')
const selectedBatch = ref('all')
const selectedStatus = ref('all')

// Quản lý hiển thị nhánh Cây chương trình (Tree View collapse)
const expandedMajors = ref({ IT: true, BA: false, GD: false })

// Chương trình đang được lựa chọn (Active Program)
const selectedProgram = ref(programs.value[0])

// Môn học trong chương trình được chọn
const currentSubjects = computed(() => {
  if (!selectedProgram.value) return []
  return programSubjects.value[selectedProgram.value.id] || []
})

// --- Lọc Cây Chương trình ở Cột Trái ---
const filteredTreePrograms = computed(() => {
  return programs.value.filter(p => {
    const matchMajor = selectedMajor.value === 'all' || p.majorCode === selectedMajor.value
    const matchBatch = selectedBatch.value === 'all' || p.batch === selectedBatch.value
    const matchStatus = selectedStatus.value === 'all' || p.status === selectedStatus.value
    const matchText = p.name.toLowerCase().includes(searchQuery.value.toLowerCase()) || p.code.toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchMajor && matchBatch && matchStatus && matchText
  })
})

// Chia nhóm chương trình theo Ngành phục vụ hiển thị Tree View
const treeData = computed(() => {
  const result = {}
  majors.value.forEach(m => {
    result[m.code] = {
      name: m.name,
      programs: filteredTreePrograms.value.filter(p => p.majorCode === m.code)
    }
  })
  return result
})

// --- State Modal Tạo Chương trình Đào tạo ---
const isCreateProgramModalOpen = ref(false)
const newProgramForm = ref({
  code: '',
  name: '',
  majorCode: '',
  batch: '',
  status: 'Active',
  totalCredits: 120
})

const programErrors = ref({ code: '', name: '', major: '', batch: '' })

watch(newProgramForm, (newVal) => {
  programErrors.value = { code: '', name: '', major: '', batch: '' }
  if (isCreateProgramModalOpen.value) {
    if (!newVal.code.trim()) {
      programErrors.value.code = 'Mã chương trình không được để trống.'
    } else if (programs.value.some(p => p.code.toUpperCase() === newVal.code.trim().toUpperCase())) {
      programErrors.value.code = 'Mã chương trình này đã tồn tại trên hệ thống.'
    }
    if (!newVal.name.trim()) {
      programErrors.value.name = 'Tên chương trình không được để trống.'
    }
    if (!newVal.majorCode) {
      programErrors.value.major = 'Vui lòng chọn ngành học.'
    }
    if (!newVal.batch) {
      programErrors.value.batch = 'Vui lòng chọn khóa học.'
    }
  }
}, { deep: true })

const isProgramFormValid = computed(() => {
  return newProgramForm.value.code.trim() &&
         newProgramForm.value.name.trim() &&
         newProgramForm.value.majorCode &&
         newProgramForm.value.batch &&
         !programErrors.value.code
})

// --- State Modal Thêm Môn học vào CTĐT ---
const isAddSubjectModalOpen = ref(false)
const selectedGlobalSubjectCode = ref('')
const subjectCredits = ref(3)
const suggestedSemester = ref(1)

// Các môn học có thể thêm (môn học chung chưa có trong CTĐT hiện tại)
const addableSubjects = computed(() => {
  if (!selectedProgram.value) return []
  const existingCodes = currentSubjects.value.map(s => s.code)
  return globalSubjects.value.filter(s => !existingCodes.includes(s.code))
})

watch(selectedGlobalSubjectCode, (newVal) => {
  const sub = globalSubjects.value.find(s => s.code === newVal)
  if (sub) {
    subjectCredits.value = sub.credits
  }
})

// --- State Modal Sửa Môn học (Tín chỉ / Học kỳ gợi ý) ---
const isEditSubjectModalOpen = ref(false)
const editingSubject = ref(null)
const editCreditsForm = ref(3)
const editSemesterForm = ref(1)
const editReason = ref('')

// --- State Prerequisite Drawer (Cấu hình môn tiên quyết) ---
const isPrerequisiteDrawerOpen = ref(false)
const targetSubject = ref(null)
const tempPrerequisites = ref([])

// Danh sách các môn học khác trong chương trình có thể chọn làm tiên quyết
// Ràng buộc: Không được chọn chính môn học đó
const eligiblePrerequisites = computed(() => {
  if (!targetSubject.value) return []
  return currentSubjects.value.filter(s => s.code !== targetSubject.value.code)
})

// --- Hàm xử lý (Handlers) ---

const selectProgram = (prog) => {
  selectedProgram.value = prog
}

const toggleMajorExpand = (majorCode) => {
  expandedMajors.value[majorCode] = !expandedMajors.value[majorCode]
}

// 1. Quản lý trạng thái chương trình (Active / Archived)
const toggleProgramStatus = (program) => {
  const text = program.status === 'Active' ? 'Archived (Ngưng sử dụng/Lưu trữ)' : 'Active (Đang hoạt động)'
  if (confirm(`Bạn có chắc chắn muốn chuyển trạng thái chương trình "${program.name}" sang ${text}?`)) {
    program.status = program.status === 'Active' ? 'Archived' : 'Active'
    
    // Ghi Audit log
    addAuditLog(
      program.code,
      `Chuyển trạng thái hoạt động chương trình sang ${program.status}`,
      `Quản trị hệ thống thay đổi trạng thái vận hành`
    )
  }
}

// 2. Tạo mới CTĐT
const openCreateProgramModal = () => {
  newProgramForm.value = {
    code: '',
    name: '',
    majorCode: majors.value[0]?.code || '',
    batch: batches.value[batches.value.length - 1],
    status: 'Active',
    totalCredits: 120
  }
  programErrors.value = { code: '', name: '', major: '', batch: '' }
  isCreateProgramModalOpen.value = true
}

const createProgram = () => {
  if (!isProgramFormValid.value) return

  const newId = programs.value.length ? Math.max(...programs.value.map(p => p.id)) + 1 : 1
  const newProg = {
    id: newId,
    code: newProgramForm.value.code.trim().toUpperCase(),
    name: newProgramForm.value.name.trim(),
    majorCode: newProgramForm.value.majorCode,
    batch: newProgramForm.value.batch,
    status: newProgramForm.value.status,
    totalCredits: newProgramForm.value.totalCredits,
    metrics: { students: 0, subjects: 0 }
  }

  programs.value.unshift(newProg)
  // Khởi tạo danh sách môn học rỗng cho chương trình này
  programSubjects.value[newId] = []

  // Ghi log
  addAuditLog(newProg.code, `Khởi tạo khung chương trình mới: "${newProg.name}"`, 'Định hình ngành học mới và lộ trình đào tạo')

  selectedProgram.value = newProg
  isCreateProgramModalOpen.value = false
}

// 3. Thêm môn học vào CTĐT
const openAddSubjectModal = () => {
  if (addableSubjects.value.length === 0) {
    alert('Không còn môn học nào trong danh mục chung có thể thêm vào chương trình này.')
    return
  }
  selectedGlobalSubjectCode.value = addableSubjects.value[0]?.code || ''
  subjectCredits.value = addableSubjects.value[0]?.credits || 3
  suggestedSemester.value = 1
  isAddSubjectModalOpen.value = true
}

const addSubjectToProgram = () => {
  if (!selectedGlobalSubjectCode.value || !selectedProgram.value) return

  const globalSub = globalSubjects.value.find(s => s.code === selectedGlobalSubjectCode.value)
  if (!globalSub) return

  const list = programSubjects.value[selectedProgram.value.id] || []
  list.push({
    code: globalSub.code,
    name: globalSub.name,
    credits: subjectCredits.value,
    semester: suggestedSemester.value,
    prerequisites: []
  })
  programSubjects.value[selectedProgram.value.id] = list

  // Cập nhật tổng số tín chỉ của chương trình
  selectedProgram.value.totalCredits += subjectCredits.value
  selectedProgram.value.metrics.subjects += 1

  // Ghi log
  addAuditLog(
    selectedProgram.value.code,
    `Thêm môn học [${globalSub.code} - ${globalSub.name}] vào chương trình đào tạo`,
    `Cập nhật lộ trình đào tạo: Số tín chỉ=${subjectCredits.value}, Học kỳ gợi ý=${suggestedSemester.value}`
  )

  isAddSubjectModalOpen.value = false
}

// 4. Xóa môn học khỏi CTĐT
const removeSubjectFromProgram = (sub) => {
  if (!selectedProgram.value) return
  if (confirm(`Bạn có chắc chắn muốn loại bỏ môn học [${sub.code} - ${sub.name}] khỏi chương trình đào tạo này không?`)) {
    const list = programSubjects.value[selectedProgram.value.id] || []
    const idx = list.findIndex(s => s.code === sub.code)
    if (idx !== -1) {
      const removed = list.splice(idx, 1)[0]
      selectedProgram.value.totalCredits -= removed.credits
      selectedProgram.value.metrics.subjects -= 1

      // Đồng thời xoá môn học này khỏi danh sách môn tiên quyết của các môn khác trong chương trình
      list.forEach(s => {
        s.prerequisites = s.prerequisites.filter(pCode => pCode !== sub.code)
      })

      // Ghi log
      addAuditLog(
        selectedProgram.value.code,
        `Loại bỏ môn học [${sub.code}] khỏi cấu trúc chương trình`,
        `Cắt giảm khung chương trình đào tạo`
      )
    }
  }
}

// 5. Cập nhật tín chỉ và Học kỳ gợi ý
const openEditSubjectModal = (sub) => {
  editingSubject.value = sub
  editCreditsForm.value = sub.credits
  editSemesterForm.value = sub.semester
  editReason.value = ''
  isEditSubjectModalOpen.value = true
}

const saveSubjectEdits = () => {
  if (!editingSubject.value || !selectedProgram.value) return
  if (!editReason.value.trim()) {
    alert('Vui lòng nhập lý do điều chỉnh để ghi nhận Audit Log.')
    return
  }

  const list = programSubjects.value[selectedProgram.value.id] || []
  const sub = list.find(s => s.code === editingSubject.value.code)
  if (sub) {
    const oldCredits = sub.credits
    const oldSem = sub.semester

    sub.credits = editCreditsForm.value
    sub.semester = editSemesterForm.value

    // Cập nhật tổng số tín chỉ của chương trình
    selectedProgram.value.totalCredits += (editCreditsForm.value - oldCredits)

    // Ghi log
    addAuditLog(
      selectedProgram.value.code,
      `Thay đổi môn [${sub.code}]: Tín chỉ (${oldCredits} -> ${editCreditsForm.value}), Học kỳ gợi ý (${oldSem} -> ${editSemesterForm.value})`,
      editReason.value
    )
  }

  isEditSubjectModalOpen.value = false
  editingSubject.value = null
}

// 6. Cấu hình môn tiên quyết (Prerequisite Drawer)
const openPrerequisiteDrawer = (sub) => {
  targetSubject.value = sub
  tempPrerequisites.value = [...sub.prerequisites]
  isPrerequisiteDrawerOpen.value = true
}

const savePrerequisites = () => {
  if (!targetSubject.value || !selectedProgram.value) return

  const list = programSubjects.value[selectedProgram.value.id] || []
  const sub = list.find(s => s.code === targetSubject.value.code)
  if (sub) {
    const oldPrereqs = [...sub.prerequisites]
    sub.prerequisites = [...tempPrerequisites.value]

    // Ghi log
    addAuditLog(
      selectedProgram.value.code,
      `Cấu hình môn tiên quyết cho [${sub.code}]: [${oldPrereqs.join(', ')}] -> [${sub.prerequisites.join(', ')}]`,
      'Định hình lại ràng buộc đăng ký môn học'
    )
  }

  isPrerequisiteDrawerOpen.value = false
  targetSubject.value = null
}

const togglePrerequisiteSelection = (subCode) => {
  const idx = tempPrerequisites.value.indexOf(subCode)
  if (idx === -1) {
    tempPrerequisites.value.push(subCode)
  } else {
    tempPrerequisites.value.splice(idx, 1)
  }
}

// Thêm Audit Log
const addAuditLog = (programCode, details, reason) => {
  const now = new Date()
  const timeStr = now.toLocaleString('sv-SE', { timeZone: 'Asia/Ho_Chi_Minh' }).replace('T', ' ')
  auditLogs.value.unshift({
    id: auditLogs.value.length ? Math.max(...auditLogs.value.map(l => l.id)) + 1 : 1,
    time: timeStr,
    actor: 'Super Admin (admin@fpt.edu.vn)',
    programCode,
    details,
    reason
  })
}

// Trạng thái badge
const getStatusBadge = (status) => {
  if (status === 'Active') return { class: 'bg-emerald-50 text-emerald-700 border-emerald-200/50 dark:bg-emerald-600/10 dark:text-emerald-400 dark:border-emerald-500/20', label: 'Đang hoạt động' }
  return { class: 'bg-slate-100 text-slate-700 border-slate-200/50 dark:bg-slate-700/50 dark:text-slate-300 dark:border-slate-600/30', label: 'Lưu trữ' }
}

const resetFilters = () => {
  searchQuery.value = ''
  selectedMajor.value = 'all'
  selectedBatch.value = 'all'
  selectedStatus.value = 'all'
}
</script>

<template>
  <div class="programs-page pb-12 space-y-6">
    <!-- Header -->
    <header class="page-header flex flex-col md:flex-row md:items-center justify-between gap-4 border-b border-default pb-4">
      <div>
        <h1 class="text-2xl font-bold text-heading">Cấu trúc Chương trình đào tạo (M6 & M12)</h1>
        <p class="text-sm text-label mt-1">Quản lý lộ trình đào tạo, phân bổ tín chỉ, sắp xếp kỳ gợi ý và thiết lập ma trận môn học tiên quyết.</p>
      </div>
      <div class="flex items-center gap-3">
        <button @click="openCreateProgramModal" class="glass-btn primary shadow-sm">
          <Plus :size="16" /> Tạo chương trình mới
        </button>
      </div>
    </header>

    <!-- KPI Mini Panel -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-4 shadow-sm">
        <p class="text-xs font-bold text-label uppercase tracking-wide">Chương trình đào tạo</p>
        <div class="flex items-baseline gap-2 mt-1">
          <span class="text-3xl font-extrabold text-heading">{{ programs.length }}</span>
          <span class="text-[10px] text-placeholder">Khung chương trình</span>
        </div>
      </div>
      <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-4 shadow-sm border-l-4 border-l-emerald-500">
        <p class="text-xs font-bold text-label uppercase tracking-wide">Đang hoạt động (Active)</p>
        <div class="flex items-baseline gap-2 mt-1">
          <span class="text-3xl font-extrabold text-emerald-600 dark:text-emerald-400">
            {{ programs.filter(p => p.status === 'Active').length }}
          </span>
          <span class="text-[10px] text-placeholder">Đang áp dụng đào tạo</span>
        </div>
      </div>
      <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-4 shadow-sm border-l-4 border-l-amber-500">
        <p class="text-xs font-bold text-label uppercase tracking-wide">Môn học khả dụng</p>
        <div class="flex items-baseline gap-2 mt-1">
          <span class="text-3xl font-extrabold text-amber-600 dark:text-amber-400">
            {{ globalSubjects.length + 10 }}
          </span>
          <span class="text-[10px] text-placeholder">Môn học trong hệ thống</span>
        </div>
      </div>
      <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-4 shadow-sm border-l-4 border-l-blue-500">
        <p class="text-xs font-bold text-label uppercase tracking-wide">Tín chỉ trung bình</p>
        <div class="flex items-baseline gap-2 mt-1">
          <span class="text-3xl font-extrabold text-blue-600 dark:text-blue-400">119.4</span>
          <span class="text-[10px] text-placeholder">Tín chỉ/Chương trình</span>
        </div>
      </div>
    </div>

    <!-- Filters Bar -->
    <div class="lg-glass-soft p-4 rounded-2xl border border-white/60 dark:border-white/10 shadow-sm flex flex-col lg:flex-row lg:items-center justify-between gap-4">
      <div class="flex flex-wrap items-center gap-3 flex-1 min-w-0">
        <!-- Search -->
        <div class="relative w-full md:w-64">
          <Search :size="15" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Tìm mã hoặc tên chương trình..."
            class="glass-input pl-9 w-full"
          />
        </div>

        <!-- Major filter -->
        <div class="flex items-center gap-1.5 w-full sm:w-auto">
          <span class="text-xs text-label font-bold whitespace-nowrap hidden lg:inline">Ngành học:</span>
          <select v-model="selectedMajor" class="glass-select w-full sm:w-44">
            <option value="all">Tất cả ngành học</option>
            <option v-for="m in majors" :key="m.code" :value="m.code">{{ m.name }}</option>
          </select>
        </div>

        <!-- Batch filter -->
        <div class="flex items-center gap-1.5 w-full sm:w-auto">
          <span class="text-xs text-label font-bold whitespace-nowrap hidden lg:inline">Khóa học:</span>
          <select v-model="selectedBatch" class="glass-select w-full sm:w-32">
            <option value="all">Tất cả khóa</option>
            <option v-for="b in batches" :key="b" :value="b">{{ b }}</option>
          </select>
        </div>

        <!-- Status filter -->
        <div class="flex items-center gap-1.5 w-full sm:w-auto">
          <span class="text-xs text-label font-bold whitespace-nowrap hidden lg:inline">Trạng thái:</span>
          <select v-model="selectedStatus" class="glass-select w-full sm:w-36">
            <option value="all">Tất cả trạng thái</option>
            <option value="Active">Đang hoạt động</option>
            <option value="Archived">Lưu trữ</option>
          </select>
        </div>
      </div>

      <button @click="resetFilters" class="glass-btn secondary shrink-0 self-end lg:self-auto justify-center">
        <RotateCcw :size="14" /> Xóa bộ lọc
      </button>
    </div>

    <!-- Main Content Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
      
      <!-- Cột Trái: Cây Chương Trình (Program Tree) (3 cột) -->
      <div class="lg:col-span-4 lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-4 shadow-sm flex flex-col h-[600px]">
        <h3 class="font-bold text-heading text-sm mb-4 border-b border-default pb-2 flex items-center gap-2">
          <Folder :size="16" class="text-blue-500" />
          Cây Chương Trình (Program Tree)
        </h3>

        <!-- Cây phân cấp -->
        <div class="flex-1 overflow-y-auto pr-1 space-y-3">
          <div v-for="(data, majorCode) in treeData" :key="majorCode" class="space-y-1">
            <!-- Ngành học (Major Node) -->
            <div
              @click="toggleMajorExpand(majorCode)"
              class="flex items-center gap-2 px-2.5 py-2 rounded-xl hover:bg-white/40 dark:hover:bg-white/5 cursor-pointer text-xs font-bold text-heading select-none"
            >
              <component :is="expandedMajors[majorCode] ? ChevronDown : ChevronRight" :size="14" class="text-placeholder" />
              <Folder :size="14" class="text-yellow-500 shrink-0" />
              <span class="truncate">{{ data.name }}</span>
              <span class="ml-auto bg-slate-100 dark:bg-white/10 text-placeholder px-2 py-0.5 rounded text-[10px]">
                {{ data.programs.length }}
              </span>
            </div>

            <!-- Khóa/Chương trình (Program Child Nodes) -->
            <div v-if="expandedMajors[majorCode]" class="pl-4 border-l border-dashed border-default ml-4 space-y-1">
              <div v-if="data.programs.length === 0" class="text-[11px] text-placeholder italic p-2">
                Không có chương trình.
              </div>
              <div
                v-for="prog in data.programs"
                :key="prog.id"
                @click="selectProgram(prog)"
                class="flex items-center gap-2 px-2.5 py-2 rounded-xl cursor-pointer text-xs transition-all select-none"
                :class="selectedProgram?.id === prog.id ? 'bg-blue-50/80 text-blue-600 dark:bg-blue-950/20 dark:text-blue-400 font-bold border border-blue-200/50' : 'hover:bg-white/40 dark:hover:bg-white/5 text-body border border-transparent'"
              >
                <BookOpen :size="13" class="shrink-0" :class="selectedProgram?.id === prog.id ? 'text-blue-500' : 'text-placeholder'" />
                <div class="min-w-0 flex-1">
                  <div class="flex items-center justify-between">
                    <span class="truncate">{{ prog.name }}</span>
                    <span class="h-1.5 w-1.5 rounded-full shrink-0 ml-1" :class="prog.status === 'Active' ? 'bg-emerald-500' : 'bg-slate-400'"></span>
                  </div>
                  <span class="text-[10px] text-placeholder font-mono leading-none block mt-0.5">{{ prog.code }} · {{ prog.batch }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Cột Phải: Chi Tiết & Danh Sách Môn Học (Subject List) (8 cột) -->
      <div class="lg:col-span-8 space-y-6 h-full flex flex-col">
        
        <div v-if="selectedProgram" class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-5 shadow-sm space-y-5 flex-1 flex flex-col min-h-[600px]">
          <!-- Chi tiết Chương trình lựa chọn -->
          <div class="flex flex-col sm:flex-row sm:items-start justify-between gap-4 border-b border-default pb-4">
            <div>
              <div class="flex items-center gap-2 flex-wrap mb-1">
                <span class="bg-blue-50 text-blue-700 border border-blue-200/50 dark:bg-blue-600/10 dark:text-blue-400 px-2.5 py-0.5 rounded-full text-[10px] font-bold">
                  {{ selectedProgram.batch }}
                </span>
                <span :class="['px-2.5 py-0.5 rounded-full text-[10px] font-bold border', getStatusBadge(selectedProgram.status).class]">
                  {{ getStatusBadge(selectedProgram.status).label }}
                </span>
                <span class="text-xs text-placeholder font-mono">Mã CTĐT: {{ selectedProgram.code }}</span>
              </div>
              <h2 class="text-xl font-extrabold text-heading">{{ selectedProgram.name }}</h2>
              <div class="flex gap-4 mt-2 text-xs text-placeholder font-medium">
                <span>Môn học: <strong class="text-body">{{ selectedProgram.metrics.subjects }} môn</strong></span>
                <span>Khối lượng: <strong class="text-body">{{ selectedProgram.totalCredits }} tín chỉ</strong></span>
                <span v-if="selectedProgram.metrics.students > 0">Sinh viên theo học: <strong class="text-body">{{ selectedProgram.metrics.students }}</strong></span>
              </div>
            </div>

            <!-- Thao tác trên chương trình -->
            <div class="flex items-center gap-2">
              <button @click="openAddSubjectModal" class="glass-btn primary !py-1.5 !px-3 text-xs" :disabled="selectedProgram.status === 'Archived'">
                <Plus :size="14" /> Thêm môn học
              </button>
              <button
                @click="toggleProgramStatus(selectedProgram)"
                class="glass-btn secondary !py-1.5 !px-3 text-xs font-bold"
                :class="selectedProgram.status === 'Active' ? 'text-amber-600 hover:text-amber-700' : 'text-emerald-600 hover:text-emerald-700'"
              >
                <Archive :size="14" />
                {{ selectedProgram.status === 'Active' ? 'Lưu trữ' : 'Kích hoạt' }}
              </button>
            </div>
          </div>

          <!-- Bảng Danh Sách Môn Học (Subject List) -->
          <div class="flex-1 overflow-x-auto">
            <table class="w-full text-left border-collapse text-xs">
              <thead>
                <tr class="bg-slate-50/50 dark:bg-white/5 border-b border-default text-[10px] font-bold text-label uppercase tracking-widest">
                  <th class="p-3">Mã Môn</th>
                  <th class="p-3">Tên Môn Học</th>
                  <th class="p-3 text-center">Tín Chỉ</th>
                  <th class="p-3 text-center">Học Kỳ Gợi Ý</th>
                  <th class="p-3">Môn Tiên Quyết (Prerequisite)</th>
                  <th class="p-3 text-right">Hành động</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-default">
                <tr v-if="currentSubjects.length === 0">
                  <td colspan="6" class="p-8 text-center text-placeholder italic">
                    Chương trình chưa có môn học nào được thêm. Vui lòng nhấn nút "Thêm môn học" ở góc phải để xây dựng khung chương trình.
                  </td>
                </tr>
                <tr
                  v-for="sub in currentSubjects"
                  :key="sub.code"
                  class="hover:bg-white/40 dark:hover:bg-white/5 transition-all"
                >
                  <td class="p-3 font-mono font-bold text-heading">{{ sub.code }}</td>
                  <td class="p-3 font-semibold text-body max-w-[200px] truncate" :title="sub.name">
                    {{ sub.name }}
                  </td>
                  <td class="p-3 text-center font-bold text-heading">{{ sub.credits }}</td>
                  <td class="p-3 text-center font-semibold">
                    <span class="inline-block px-2 py-0.5 rounded-md bg-slate-100 dark:bg-white/10 text-placeholder font-mono text-[10px]">
                      Kỳ {{ sub.semester }}
                    </span>
                  </td>
                  <!-- Môn tiên quyết -->
                  <td class="p-3">
                    <div class="flex flex-wrap gap-1 items-center">
                      <template v-if="sub.prerequisites.length > 0">
                        <span
                          v-for="prereq in sub.prerequisites"
                          :key="prereq"
                          class="px-2 py-0.5 rounded bg-rose-50 text-rose-700 border border-rose-200/50 dark:bg-rose-950/20 dark:text-rose-400 font-mono text-[9px] font-bold"
                        >
                          {{ prereq }}
                        </span>
                      </template>
                      <span v-else class="text-placeholder text-[10px] italic">Không có</span>
                      
                      <!-- Nút cấu hình tiên quyết nhanh -->
                      <button
                        @click="openPrerequisiteDrawer(sub)"
                        class="p-1 hover:bg-slate-100 dark:hover:bg-white/10 rounded text-blue-600 hover:text-blue-700 ml-1 transition-all"
                        title="Cấu hình môn tiên quyết"
                        :disabled="selectedProgram.status === 'Archived'"
                      >
                        <Settings :size="11" />
                      </button>
                    </div>
                  </td>
                  <td class="p-3 text-right">
                    <div class="flex items-center justify-end gap-1">
                      <button
                        @click="openEditSubjectModal(sub)"
                        class="p-1.5 text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded transition-all"
                        title="Điều chỉnh cấu hình tín chỉ & học kỳ gợi ý"
                        :disabled="selectedProgram.status === 'Archived'"
                      >
                        <Edit2 :size="13" />
                      </button>
                      <button
                        @click="removeSubjectFromProgram(sub)"
                        class="p-1.5 text-rose-500 hover:bg-rose-50 dark:hover:bg-rose-900/20 rounded transition-all"
                        title="Loại bỏ môn"
                        :disabled="selectedProgram.status === 'Archived'"
                      >
                        <Trash2 :size="13" />
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Ghi chú nghiệp vụ và ma trận tích hợp -->
          <div class="p-3 rounded-xl border border-blue-100 dark:border-blue-500/20 bg-blue-50/50 dark:bg-blue-950/10 text-[10.5px] text-blue-900/80 dark:text-blue-400 flex items-start gap-2 leading-relaxed">
            <Info :size="14" class="shrink-0 mt-0.5" />
            <div>
              <span class="font-bold">Quy tắc nghiệp vụ Ma trận tiên quyết (Module M12):</span>
              <p class="mt-0.5">
                Các cấu hình môn tiên quyết tại giao diện này sẽ là dữ liệu nền để hệ thống tự động kiểm tra điều kiện (Validate) khi sinh viên đăng ký môn học trực tuyến. Sinh viên buộc phải vượt qua (pass) môn tiên quyết mới có thể đăng ký môn học tiếp theo.
              </p>
            </div>
          </div>
        </div>

        <div v-else class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-12 text-center text-placeholder italic text-sm flex-1 flex flex-col justify-center items-center">
          <BookOpen :size="48" class="text-placeholder/60 mb-3" />
          Vui lòng chọn một chương trình đào tạo cụ thể ở cây danh mục bên trái để quản lý môn học và ma trận tiên quyết.
        </div>
      </div>

    </div>

    <!-- Audit Logs Section -->
    <div class="lg-glass-soft p-5 rounded-2xl border border-white/60 dark:border-white/10 shadow-sm space-y-4">
      <div class="flex items-center justify-between border-b border-default pb-3">
        <h3 class="font-bold text-heading flex items-center gap-2">
          <History :size="18" class="text-violet-500" />
          Nhật ký thay đổi cấu trúc chương trình (Audit Log)
        </h3>
        <span class="text-[10px] uppercase font-bold text-label bg-slate-100 dark:bg-white/10 px-2 py-0.5 rounded-md">Truy vết & Bảo toàn dữ liệu học vụ</span>
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
              <span class="font-bold text-heading">CTĐT: {{ log.programCode }}</span>
              <span class="text-[10px] text-placeholder font-mono">{{ log.time }}</span>
            </div>
            <p class="text-body font-semibold mt-1">{{ log.details }}</p>
            <p class="text-placeholder mt-0.5">Lý do điều chỉnh: <span class="italic text-body font-medium">"{{ log.reason }}"</span></p>
            <p class="text-[10px] text-placeholder mt-1">Người vận hành: <span class="font-mono">{{ log.actor }}</span></p>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal: Tạo mới Chương trình đào tạo -->
    <div v-if="isCreateProgramModalOpen" class="modal-overlay">
      <div class="modal-content max-w-lg w-full">
        <!-- Header -->
        <div class="flex items-center justify-between border-b border-default pb-4 mb-5">
          <div class="flex items-center gap-2.5">
            <div class="w-9 h-9 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center">
              <Folder :size="18" />
            </div>
            <div>
              <h3 class="text-lg font-bold text-heading">Tạo mới Chương trình đào tạo (CTĐT)</h3>
              <p class="text-xs text-label uppercase tracking-widest font-semibold mt-0.5">Thiết lập lộ trình tổng thể</p>
            </div>
          </div>
          <button @click="isCreateProgramModalOpen = false" class="p-1 hover:bg-slate-100 dark:hover:bg-white/10 rounded">
            <X :size="18" />
          </button>
        </div>

        <!-- Body -->
        <div class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Mã chương trình *</label>
              <input
                v-model="newProgramForm.code"
                type="text"
                class="glass-input w-full uppercase"
                placeholder="VD: IT_SE_K19"
                :class="{'border-rose-300': programErrors.code}"
              />
              <p v-if="programErrors.code" class="text-rose-500 text-[10px] mt-1 font-semibold">{{ programErrors.code }}</p>
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Số tín chỉ tổng khóa</label>
              <input
                v-model.number="newProgramForm.totalCredits"
                type="number"
                class="glass-input w-full"
              />
            </div>
          </div>

          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1.5">Tên chương trình đào tạo *</label>
            <input
              v-model="newProgramForm.name"
              type="text"
              class="glass-input w-full"
              placeholder="VD: Kỹ thuật phần mềm - Khóa 19"
              :class="{'border-rose-300': programErrors.name}"
            />
            <p v-if="programErrors.name" class="text-rose-500 text-[10px] mt-1 font-semibold">{{ programErrors.name }}</p>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Thuộc Ngành học *</label>
              <select v-model="newProgramForm.majorCode" class="glass-select w-full" :class="{'border-rose-300': programErrors.major}">
                <option v-for="m in majors" :key="m.code" :value="m.code">{{ m.name }}</option>
              </select>
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Thuộc Khóa học *</label>
              <select v-model="newProgramForm.batch" class="glass-select w-full" :class="{'border-rose-300': programErrors.batch}">
                <option v-for="b in batches" :key="b" :value="b">{{ b }}</option>
              </select>
            </div>
          </div>

          <!-- Validation Panel -->
          <div class="p-3.5 rounded-xl border flex items-start gap-2.5 text-xs" :class="isProgramFormValid ? 'bg-emerald-50/60 border-emerald-200 text-emerald-800 dark:bg-emerald-950/20 dark:border-emerald-500/20 dark:text-emerald-400' : 'bg-rose-50/60 border-rose-200 text-rose-800 dark:bg-rose-950/20 dark:border-rose-500/20 dark:text-rose-400'">
            <component :is="isProgramFormValid ? CheckCircle : AlertCircle" :size="16" class="shrink-0 mt-0.5" />
            <div>
              <span class="font-bold">{{ isProgramFormValid ? 'Thông tin hợp lệ' : 'Yêu cầu điền đủ thông tin' }}</span>
              <p class="text-[10px] mt-0.5 opacity-95">
                {{ isProgramFormValid ? 'Khung chương trình đã sẵn sàng để khởi tạo.' : 'Vui lòng nhập đầy đủ mã, tên chương trình và chọn ngành/khóa học tương ứng.' }}
              </p>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="flex gap-3 justify-end border-t border-default pt-4 mt-6">
          <button @click="isCreateProgramModalOpen = false" class="glass-btn secondary">Hủy</button>
          <button
            @click="createProgram"
            class="glass-btn primary"
            :disabled="!isProgramFormValid"
            :class="{'opacity-50 cursor-not-allowed': !isProgramFormValid}"
          >
            <Save :size="14" /> Khởi tạo CTĐT
          </button>
        </div>
      </div>
    </div>

    <!-- Modal: Thêm môn học vào chương trình -->
    <div v-if="isAddSubjectModalOpen" class="modal-overlay">
      <div class="modal-content max-w-md w-full">
        <!-- Header -->
        <div class="flex items-center justify-between border-b border-default pb-4 mb-5">
          <div class="flex items-center gap-2.5">
            <div class="w-9 h-9 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center">
              <BookOpen :size="18" />
            </div>
            <div>
              <h3 class="text-lg font-bold text-heading">Thêm môn học vào CTĐT</h3>
              <p class="text-xs text-label uppercase tracking-widest font-semibold mt-0.5">{{ selectedProgram?.name }}</p>
            </div>
          </div>
          <button @click="isAddSubjectModalOpen = false" class="p-1 hover:bg-slate-100 dark:hover:bg-white/10 rounded">
            <X :size="18" />
          </button>
        </div>

        <!-- Body -->
        <div class="space-y-4">
          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1.5">Chọn môn học từ danh mục chung</label>
            <select v-model="selectedGlobalSubjectCode" class="glass-select w-full">
              <option v-for="sub in addableSubjects" :key="sub.code" :value="sub.code">
                {{ sub.code }} - {{ sub.name }} ({{ sub.credits }} tín chỉ)
              </option>
            </select>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Số tín chỉ thiết lập</label>
              <input
                v-model.number="subjectCredits"
                type="number"
                min="1"
                max="15"
                class="glass-input w-full"
              />
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Học kỳ gợi ý đề xuất</label>
              <select v-model.number="suggestedSemester" class="glass-select w-full">
                <option v-for="n in 9" :key="n" :value="n">Học kỳ {{ n }}</option>
              </select>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="flex gap-3 justify-end border-t border-default pt-4 mt-6">
          <button @click="isAddSubjectModalOpen = false" class="glass-btn secondary">Hủy</button>
          <button @click="addSubjectToProgram" class="glass-btn primary" :disabled="!selectedGlobalSubjectCode">
            <PlusCircle :size="14" /> Thêm vào khung
          </button>
        </div>
      </div>
    </div>

    <!-- Modal: Sửa môn học (Tín chỉ / Học kỳ gợi ý) -->
    <div v-if="isEditSubjectModalOpen" class="modal-overlay">
      <div class="modal-content max-w-md w-full">
        <!-- Header -->
        <div class="flex items-center justify-between border-b border-default pb-4 mb-5">
          <div class="flex items-center gap-2.5">
            <div class="w-9 h-9 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center">
              <Settings :size="18" />
            </div>
            <div>
              <h3 class="text-lg font-bold text-heading">Điều chỉnh thông tin môn học</h3>
              <p class="text-xs text-label uppercase tracking-widest font-semibold mt-0.5">{{ editingSubject?.code }} - {{ editingSubject?.name }}</p>
            </div>
          </div>
          <button @click="isEditSubjectModalOpen = false" class="p-1 hover:bg-slate-100 dark:hover:bg-white/10 rounded">
            <X :size="18" />
          </button>
        </div>

        <!-- Body -->
        <div class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Số tín chỉ mới</label>
              <input
                v-model.number="editCreditsForm"
                type="number"
                min="1"
                max="15"
                class="glass-input w-full"
              />
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1.5">Học kỳ gợi ý mới</label>
              <select v-model.number="editSemesterForm" class="glass-select w-full">
                <option v-for="n in 9" :key="n" :value="n">Học kỳ {{ n }}</option>
              </select>
            </div>
          </div>

          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1.5">Lý do điều chỉnh (Bắt buộc ghi Log) *</label>
            <textarea
              v-model="editReason"
              rows="3"
              class="glass-input w-full"
              placeholder="Nhập lý do chi tiết..."
            ></textarea>
          </div>
        </div>

        <!-- Footer -->
        <div class="flex gap-3 justify-end border-t border-default pt-4 mt-6">
          <button @click="isEditSubjectModalOpen = false" class="glass-btn secondary">Hủy</button>
          <button
            @click="saveSubjectEdits"
            class="glass-btn primary"
            :disabled="!editReason.trim()"
            :class="{'opacity-50 cursor-not-allowed': !editReason.trim()}"
          >
            <Save :size="14" /> Cập nhật (Ghi Log)
          </button>
        </div>
      </div>
    </div>

    <!-- Prerequisite Drawer (Cấu hình môn tiên quyết - Trượt từ bên phải) -->
    <div v-if="isPrerequisiteDrawerOpen" class="drawer-overlay" @click.self="isPrerequisiteDrawerOpen = false">
      <div class="drawer-content h-full max-w-md w-full bg-white dark:bg-slate-900 border-l border-default shadow-2xl flex flex-col">
        <!-- Header -->
        <div class="p-5 border-b border-default flex items-center justify-between">
          <div>
            <h3 class="font-bold text-heading text-base flex items-center gap-1.5">
              <Settings :size="18" class="text-blue-500" />
              Cấu hình môn tiên quyết
            </h3>
            <p class="text-xs text-label mt-0.5">Môn học: <strong class="text-heading font-mono">{{ targetSubject?.code }}</strong></p>
          </div>
          <button @click="isPrerequisiteDrawerOpen = false" class="p-1.5 hover:bg-slate-100 dark:hover:bg-white/10 rounded text-placeholder hover:text-heading">
            <X :size="18" />
          </button>
        </div>

        <!-- Search in Drawer -->
        <div class="p-4 border-b border-default bg-slate-50/50 dark:bg-white/5">
          <p class="text-xs text-label leading-normal mb-3">
            Chọn các môn học sinh viên bắt buộc phải hoàn thành trước đó. Hệ thống sẽ validate khi đăng ký ở Module M12.
          </p>
        </div>

        <!-- Body (Subject Checklist) -->
        <div class="flex-1 overflow-y-auto p-4 space-y-2.5">
          <div v-if="eligiblePrerequisites.length === 0" class="text-xs text-placeholder italic text-center py-8">
            Chương trình đào tạo này không có môn học nào khác để thiết lập.
          </div>
          <div
            v-for="sub in eligiblePrerequisites"
            :key="sub.code"
            @click="togglePrerequisiteSelection(sub.code)"
            class="flex items-start gap-3 p-3 border rounded-xl cursor-pointer transition-all select-none"
            :class="tempPrerequisites.includes(sub.code) ? 'border-blue-500 bg-blue-50/50 dark:border-blue-600/50 dark:bg-blue-950/10' : 'border-default hover:bg-slate-50 dark:hover:bg-white/5'"
          >
            <!-- Checkbox -->
            <div class="w-4 h-4 rounded border flex items-center justify-center mt-0.5 shrink-0" :class="tempPrerequisites.includes(sub.code) ? 'bg-blue-600 border-blue-600 text-white' : 'border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800'">
              <Check v-if="tempPrerequisites.includes(sub.code)" :size="12" class="stroke-[3]" />
            </div>

            <div class="min-w-0 flex-1">
              <div class="flex items-center justify-between gap-2">
                <span class="font-bold font-mono text-heading text-xs">{{ sub.code }}</span>
                <span class="text-[10px] text-placeholder bg-slate-100 dark:bg-white/10 px-1.5 py-0.5 rounded font-mono">Kỳ {{ sub.semester }}</span>
              </div>
              <p class="text-xs font-semibold text-body truncate mt-1">{{ sub.name }}</p>
              <p class="text-[10px] text-placeholder mt-0.5">{{ sub.credits }} tín chỉ</p>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="p-4 border-t border-default bg-slate-50/80 dark:bg-slate-900 flex gap-3">
          <button @click="isPrerequisiteDrawerOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy</button>
          <button @click="savePrerequisites" class="glass-btn primary flex-1 justify-center">
            <Save :size="14" /> Lưu cấu hình
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.programs-page {
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

/* Drawer styling */
.drawer-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.3);
  backdrop-filter: blur(4px);
  display: flex;
  justify-content: flex-end;
  z-index: 50;
}
.drawer-content {
  animation: slideIn 0.3s ease-out;
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

@keyframes slideIn {
  from {
    transform: translateX(100%);
  }
  to {
    transform: translateX(0);
  }
}
</style>

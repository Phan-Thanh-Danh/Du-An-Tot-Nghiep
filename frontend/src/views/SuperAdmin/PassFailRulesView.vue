<script setup>
/**
 * PassFailRulesView.vue - Super Admin
 * Giao diện quản trị điều kiện đạt/rớt môn (Pass/Fail Rules), chỉnh sửa trọng số điểm (Grade Weight Editor)
 * và lưu vết lịch sử điều phối học vụ (Audit Logs) thuộc Module M6.
 */
import { ref, computed } from 'vue'
import {
  Award,
  BookOpen,
  AlertTriangle,
  CheckCircle,
  History,
  Save,
  X,
  Plus,
  Filter,
  Pencil,
  Check,
  Info,
  Copy,
  RotateCcw,
  HelpCircle,
  Activity
} from 'lucide-vue-next'

// --- Mock Data ---

// Danh sách môn học
const subjects = ref([
  { code: 'PRN211', name: 'Lập trình C# (.NET)', department: 'Phần mềm' },
  { code: 'SWD392', name: 'Thiết kế & Kiến trúc phần mềm', department: 'Phần mềm' },
  { code: 'PRN221', name: 'Lập trình ứng dụng Windows', department: 'Phần mềm' },
  { code: 'SWE201c', name: 'Nhập môn kỹ nghệ phần mềm', department: 'Phần mềm' },
  { code: 'MAD101', name: 'Toán rời rạc', department: 'Khoa học cơ bản' }
])

// Danh sách học kỳ
const semesters = ref(['Spring 2026', 'Summer 2026', 'Fall 2025'])

// Danh sách Chương trình đào tạo
const trainingPrograms = ref([
  { code: 'SE', name: 'Kỹ thuật phần mềm' },
  { code: 'IA', name: 'An toàn thông tin' },
  { code: 'GD', name: 'Thiết kế đồ họa' }
])

// Danh sách quy tắc hiện tại (Pass/Fail Rules)
const passFailRules = ref([
  {
    id: 1,
    subjectCode: 'PRN211',
    semester: 'Spring 2026',
    program: 'SE',
    weightAttendance: 10,
    weightLab: 30,
    weightMidterm: 20,
    weightFinal: 40,
    minAttendanceScore: 0.0,
    minLabScore: 5.0,
    minMidtermScore: 3.0,
    minFinalScore: 4.0,
    maxAbsentSlots: 6, // 20% vắng mặt của môn 30 slot
    blockOnMissingLab: true,
    blockOnIncomplete: true,
    status: 'Applied',
    updatedAt: '2026-06-05 10:30:00'
  },
  {
    id: 2,
    subjectCode: 'SWD392',
    semester: 'Spring 2026',
    program: 'SE',
    weightAttendance: 10,
    weightLab: 20,
    weightMidterm: 30,
    weightFinal: 40,
    minAttendanceScore: 0.0,
    minLabScore: 4.0,
    minMidtermScore: 4.0,
    minFinalScore: 4.0,
    maxAbsentSlots: 4, // 20% vắng mặt môn 20 slot
    blockOnMissingLab: true,
    blockOnIncomplete: false,
    status: 'Applied',
    updatedAt: '2026-06-06 14:20:00'
  },
  {
    id: 3,
    subjectCode: 'PRN221',
    semester: 'Summer 2026',
    program: 'SE',
    weightAttendance: 10,
    weightLab: 40,
    weightMidterm: 10,
    weightFinal: 40,
    minAttendanceScore: 0.0,
    minLabScore: 5.0,
    minMidtermScore: 3.0,
    minFinalScore: 3.0,
    maxAbsentSlots: 6,
    blockOnMissingLab: true,
    blockOnIncomplete: true,
    status: 'Draft',
    updatedAt: '2026-06-07 09:15:00'
  },
  {
    id: 4,
    subjectCode: 'SWE201c',
    semester: 'Spring 2026',
    program: 'SE',
    weightAttendance: 10,
    weightLab: 30,
    weightMidterm: 30,
    weightFinal: 30,
    minAttendanceScore: 0.0,
    minLabScore: 4.0,
    minMidtermScore: 3.0,
    minFinalScore: 4.0,
    maxAbsentSlots: 4,
    blockOnMissingLab: false,
    blockOnIncomplete: false,
    status: 'Draft',
    updatedAt: '2026-06-08 11:00:00'
  }
])

// Danh sách Audit Logs
const auditLogs = ref([
  {
    id: 1,
    time: '2026-06-08 11:05:00',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Cập nhật bản nháp',
    details: 'Cập nhật trọng số & điểm liệt cho bản nháp môn SWE201c kỳ Spring 2026',
    reason: 'Điều chỉnh theo quyết định mới của hội đồng học thuật khoa'
  },
  {
    id: 2,
    time: '2026-06-06 14:20:00',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Áp dụng quy tắc',
    details: 'Áp dụng cấu hình đạt/rớt môn SWD392 (Spring 2026). Trọng số: CC 10%, Lab 20%, GK 30%, CK 40%',
    reason: 'Kích hoạt chính thức quy chế tính điểm học kỳ Spring 2026'
  },
  {
    id: 3,
    time: '2026-06-05 10:30:00',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Áp dụng quy tắc',
    details: 'Áp dụng cấu hình đạt/rớt môn PRN211 (Spring 2026). Trọng số: CC 10%, Lab 30%, GK 20%, CK 40%',
    reason: 'Cấu hình chuẩn hệ thống LMS cho chuyên ngành Kỹ thuật phần mềm'
  }
])

// --- State Bộ lọc ---
const filterSubject = ref('all')
const filterSemester = ref('all')
const filterProgram = ref('all')
const filterStatus = ref('all')

const filteredRules = computed(() => {
  return passFailRules.value.filter(rule => {
    const matchSubject = filterSubject.value === 'all' || rule.subjectCode === filterSubject.value
    const matchSemester = filterSemester.value === 'all' || rule.semester === filterSemester.value
    const matchProgram = filterProgram.value === 'all' || rule.program === filterProgram.value
    const matchStatus = filterStatus.value === 'all' || rule.status === filterStatus.value
    return matchSubject && matchSemester && matchProgram && matchStatus
  })
})

const resetFilters = () => {
  filterSubject.value = 'all'
  filterSemester.value = 'all'
  filterProgram.value = 'all'
  filterStatus.value = 'all'
}

// --- Thống kê KPI ---
const totalAppliedRules = computed(() => passFailRules.value.filter(r => r.status === 'Applied').length)
const totalDraftRules = computed(() => passFailRules.value.filter(r => r.status === 'Draft').length)
const systemAveragePassRate = ref(84.6)
const unconfiguredSubjectsCount = computed(() => {
  const appliedSubjectCodes = passFailRules.value
    .filter(r => r.status === 'Applied')
    .map(r => r.subjectCode)
  return subjects.value.filter(s => !appliedSubjectCodes.includes(s.code)).length
})

// --- State Modals & Quy tắc đang chỉnh sửa ---
const isRuleModalOpen = ref(false)
const editingMode = ref('create') // 'create' | 'edit'

const currentRule = ref({
  id: null,
  subjectCode: '',
  semester: '',
  program: '',
  weightAttendance: 10,
  weightLab: 30,
  weightMidterm: 20,
  weightFinal: 40,
  minAttendanceScore: 0.0,
  minLabScore: 5.0,
  minMidtermScore: 3.0,
  minFinalScore: 4.0,
  maxAbsentSlots: 6,
  blockOnMissingLab: true,
  blockOnIncomplete: true,
  status: 'Draft'
})

// --- Validation Panel Logic thời gian thực ---
const totalWeight = computed(() => {
  return (
    Number(currentRule.value.weightAttendance || 0) +
    Number(currentRule.value.weightLab || 0) +
    Number(currentRule.value.weightMidterm || 0) +
    Number(currentRule.value.weightFinal || 0)
  )
})

const isWeightValid = computed(() => totalWeight.value === 100)

const isScoresValid = computed(() => {
  const scores = [
    currentRule.value.minAttendanceScore,
    currentRule.value.minLabScore,
    currentRule.value.minMidtermScore,
    currentRule.value.minFinalScore
  ]
  return scores.every(score => {
    const num = Number(score)
    return !isNaN(num) && num >= 0 && num <= 10
  })
})

const isFormFilled = computed(() => {
  return currentRule.value.subjectCode && currentRule.value.semester && currentRule.value.program
})

const isFormValid = computed(() => {
  return isWeightValid.value && isScoresValid.value && isFormFilled.value
})

// Check từng trường điểm có lỗi không
const isAttendanceScoreError = computed(() => Number(currentRule.value.minAttendanceScore) < 0 || Number(currentRule.value.minAttendanceScore) > 10)
const isLabScoreError = computed(() => Number(currentRule.value.minLabScore) < 0 || Number(currentRule.value.minLabScore) > 10)
const isMidtermScoreError = computed(() => Number(currentRule.value.minMidtermScore) < 0 || Number(currentRule.value.minMidtermScore) > 10)
const isFinalScoreError = computed(() => Number(currentRule.value.minFinalScore) < 0 || Number(currentRule.value.minFinalScore) > 10)
const isAbsentSlotsError = computed(() => Number(currentRule.value.maxAbsentSlots) < 0 || Number(currentRule.value.maxAbsentSlots) > 20)

// --- Confirm Apply Modal States ---
const isConfirmApplyModalOpen = ref(false)
const applyTargetRule = ref(null)
const applyReason = ref('')

// --- Toast States ---
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref('success') // 'success' | 'error' | 'info'

const triggerToast = (msg, type = 'success') => {
  toastMessage.value = msg
  toastType.value = type
  showToast.value = true
  setTimeout(() => {
    showToast.value = false
  }, 4000)
}

// --- Handlers ---

const openCreateModal = () => {
  editingMode.value = 'create'
  currentRule.value = {
    id: null,
    subjectCode: subjects.value[0]?.code || '',
    semester: semesters.value[0] || '',
    program: trainingPrograms.value[0]?.code || '',
    weightAttendance: 10,
    weightLab: 30,
    weightMidterm: 20,
    weightFinal: 40,
    minAttendanceScore: 0.0,
    minLabScore: 5.0,
    minMidtermScore: 3.0,
    minFinalScore: 4.0,
    maxAbsentSlots: 6,
    blockOnMissingLab: true,
    blockOnIncomplete: true,
    status: 'Draft'
  }
  isRuleModalOpen.value = true
}

const openEditModal = (rule) => {
  editingMode.value = 'edit'
  currentRule.value = JSON.parse(JSON.stringify(rule))
  isRuleModalOpen.value = true
}

const handleCloneAsDraft = (rule) => {
  editingMode.value = 'create'
  currentRule.value = JSON.parse(JSON.stringify(rule))
  currentRule.value.id = null
  currentRule.value.status = 'Draft'
  isRuleModalOpen.value = true
  triggerToast(`Đã nhân bản quy tắc môn ${rule.subjectCode} thành bản nháp mới để điều chỉnh.`, 'info')
}

const handleSaveRule = () => {
  if (!isFormValid.value) return

  const timeString = new Date().toLocaleString('vi-VN')

  if (editingMode.value === 'create') {
    const newId = passFailRules.value.length ? Math.max(...passFailRules.value.map(r => r.id)) + 1 : 1
    const newRule = {
      ...currentRule.value,
      id: newId,
      status: 'Draft',
      updatedAt: timeString
    }
    passFailRules.value.push(newRule)

    // Ghi log
    auditLogs.value.unshift({
      id: auditLogs.value.length + 1,
      time: timeString,
      actor: 'Super Admin (admin@fpt.edu.vn)',
      action: 'Tạo bản nháp',
      details: `Khởi tạo quy tắc nháp đạt/rớt môn ${newRule.subjectCode} kỳ ${newRule.semester} [Chương trình ${newRule.program}].`,
      reason: 'Cấu hình thử nghiệm quy chế môn học.'
    })

    triggerToast(`Đã lưu thành công bản nháp quy tắc đạt/rớt môn ${newRule.subjectCode}.`, 'success')
  } else {
    const index = passFailRules.value.findIndex(r => r.id === currentRule.value.id)
    if (index !== -1) {
      passFailRules.value[index] = {
        ...currentRule.value,
        updatedAt: timeString
      }

      auditLogs.value.unshift({
        id: auditLogs.value.length + 1,
        time: timeString,
        actor: 'Super Admin (admin@fpt.edu.vn)',
        action: 'Cập nhật bản nháp',
        details: `Cập nhật cấu hình bản nháp môn ${currentRule.value.subjectCode} kỳ ${currentRule.value.semester}.`,
        reason: 'Hiệu chỉnh điểm liệt và trọng số theo yêu cầu giáo vụ.'
      })

      triggerToast(`Đã cập nhật thành công bản nháp quy tắc môn ${currentRule.value.subjectCode}.`, 'success')
    }
  }

  isRuleModalOpen.value = false
}

const openApplyModal = (rule) => {
  applyTargetRule.value = rule
  applyReason.value = ''
  isConfirmApplyModalOpen.value = true
}

const handleConfirmApply = () => {
  if (!applyTargetRule.value) return
  if (!applyReason.value.trim()) {
    triggerToast('Vui lòng nhập lý do áp dụng quy tắc này!', 'error')
    return
  }

  const target = passFailRules.value.find(r => r.id === applyTargetRule.value.id)
  if (target) {
    target.status = 'Applied'
    const timeString = new Date().toLocaleString('vi-VN')
    target.updatedAt = timeString

    // Ghi audit log
    auditLogs.value.unshift({
      id: auditLogs.value.length + 1,
      time: timeString,
      actor: 'Super Admin (admin@fpt.edu.vn)',
      action: 'Áp dụng quy tắc',
      details: `Áp dụng chính thức quy tắc đạt/rớt môn ${target.subjectCode} kỳ ${target.semester} [Chương trình ${target.program}].`,
      reason: applyReason.value
    })

    // Giả lập Stored Procedure chạy tính lại GPA cho sinh viên
    triggerToast(`Đã kích hoạt quy tắc môn ${target.subjectCode}. Stored Procedure hệ thống bắt đầu tính toán lại GPA của toàn bộ sinh viên.`, 'success')
  }

  isConfirmApplyModalOpen.value = false
}

const getSubjectName = (code) => {
  const s = subjects.value.find(sub => sub.code === code)
  return s ? s.name : code
}

const getProgramName = (code) => {
  const p = trainingPrograms.value.find(pr => pr.code === code)
  return p ? p.name : code
}
</script>

<template>
  <div class="min-h-screen lg-app-bg text-heading font-sans relative pb-12">
    <!-- Quả cầu trang trí 3D mờ ảo -->
    <div class="lg-shell-orbs">
      <div class="lg-shell-orb lg-shell-orb-primary"></div>
      <div class="lg-shell-orb lg-shell-orb-secondary"></div>
    </div>

    <!-- Toast Thông báo -->
    <div 
      v-if="showToast" 
      class="fixed bottom-5 right-5 z-[110] p-4 rounded-xl shadow-xl border flex items-center gap-3 animate-in fade-in slide-in-from-bottom duration-300"
      :class="{
        'bg-emerald-500 text-white border-emerald-400': toastType === 'success',
        'bg-rose-500 text-white border-rose-400': toastType === 'error',
        'bg-sky-500 text-white border-sky-400': toastType === 'info'
      }"
    >
      <CheckCircle v-if="toastType === 'success'" class="w-5 h-5 flex-shrink-0" />
      <AlertTriangle v-else-if="toastType === 'error'" class="w-5 h-5 flex-shrink-0" />
      <Info v-else class="w-5 h-5 flex-shrink-0" />
      <span class="text-sm font-bold">{{ toastMessage }}</span>
    </div>

    <div class="lg-shell-content mx-auto relative z-10">
      <!-- Header Trang -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
        <div>
          <h1 class="text-2xl md:text-3xl font-extrabold tracking-tight text-heading flex items-center gap-3">
            <Award class="w-8 h-8 text-primary" />
            Điều Kiện Pass / Fail
          </h1>
          <p class="text-sm text-muted mt-1">
            Thiết lập trọng số điểm thành phần, cấu hình các ngưỡng điểm liệt và điều kiện đạt/rớt môn học cho toàn bộ hệ thống LMS.
          </p>
        </div>

        <div>
          <!-- Nút tạo mới quy tắc -->
          <button
            @click="openCreateModal"
            class="lg-btn-primary px-4 py-2.5 text-sm font-bold flex items-center gap-2"
          >
            <Plus class="w-4.5 h-4.5" />
            Tạo Quy Tắc Mới
          </button>
        </div>
      </div>

      <!-- KPI Dashboard Mini -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <!-- KPI 1 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500">
            <CheckCircle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Quy tắc Áp Dụng</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ totalAppliedRules }} quy tắc</div>
          </div>
        </div>

        <!-- KPI 2 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-amber-500/10 flex items-center justify-center text-amber-500">
            <Pencil class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Quy tắc Nháp (Draft)</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ totalDraftRules }} bản nháp</div>
          </div>
        </div>

        <!-- KPI 3 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-red-500/10 flex items-center justify-center text-red-500">
            <AlertTriangle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Môn chưa cấu hình</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ unconfiguredSubjectsCount }} môn</div>
          </div>
        </div>

        <!-- KPI 4 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-violet-500/10 flex items-center justify-center text-violet-500">
            <Award class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Tỉ lệ Đạt Hệ thống</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ systemAveragePassRate }}%</div>
          </div>
        </div>
      </div>

      <!-- Khung Bộ Lọc & Tìm Kiếm -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-default">
          <div class="flex items-center gap-2">
            <Filter class="w-4.5 h-4.5 text-primary" />
            <h3 class="font-bold text-heading text-sm">Bộ lọc quy tắc đạt/rớt môn</h3>
          </div>
          <button 
            @click="resetFilters" 
            class="text-xs text-link font-bold flex items-center gap-1 hover:underline"
          >
            <RotateCcw class="w-3.5 h-3.5" />
            Xóa bộ lọc
          </button>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-4 gap-3">
          <!-- Chọn Môn học -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Môn học</label>
            <select v-model="filterSubject" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả môn học</option>
              <option v-for="sub in subjects" :key="sub.code" :value="sub.code">
                [{{ sub.code }}] {{ sub.name }}
              </option>
            </select>
          </div>

          <!-- Lọc Học kỳ -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Học kỳ</label>
            <select v-model="filterSemester" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả học kỳ</option>
              <option v-for="sem in semesters" :key="sem" :value="sem">{{ sem }}</option>
            </select>
          </div>

          <!-- Lọc Chương trình -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Chương trình đào tạo</label>
            <select v-model="filterProgram" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả chương trình</option>
              <option v-for="prog in trainingPrograms" :key="prog.code" :value="prog.code">
                {{ prog.name }} ({{ prog.code }})
              </option>
            </select>
          </div>

          <!-- Lọc Trạng thái -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Trạng thái</label>
            <select v-model="filterStatus" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả trạng thái</option>
              <option value="Applied">Đã áp dụng (Applied)</option>
              <option value="Draft">Bản nháp (Draft)</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Bảng Quy Tắc (Rule Table) -->
      <div class="lg-table-shell overflow-x-auto mb-8">
        <table class="min-w-full divide-y divide-default text-sm">
          <thead>
            <tr class="surface-table-header">
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Môn học / Chương trình</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Học kỳ</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Trọng số thành phần</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Điểm liệt / Ràng buộc</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Vắng tối đa</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Trạng thái</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Hành động</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-if="filteredRules.length === 0">
              <td colspan="7" class="px-4 py-12 text-center text-muted">
                <div class="flex flex-col items-center gap-2">
                  <Award class="w-8 h-8 text-muted" />
                  <span>Không tìm thấy quy tắc đạt/rớt môn nào phù hợp với bộ lọc.</span>
                </div>
              </td>
            </tr>

            <tr v-for="rule in filteredRules" :key="rule.id" class="transition-colors">
              <!-- Môn học & Chương trình -->
              <td class="px-4 py-4">
                <div class="font-extrabold text-heading">[{{ rule.subjectCode }}]</div>
                <div class="text-xs text-muted font-medium mt-0.5">{{ getSubjectName(rule.subjectCode) }}</div>
                <div class="mt-1.5">
                  <span class="lg-badge lg-badge-primary text-[10px] py-0.5 px-2">CT: {{ getProgramName(rule.program) }}</span>
                </div>
              </td>

              <!-- Học kỳ -->
              <td class="px-4 py-4 font-semibold text-body">
                {{ rule.semester }}
              </td>

              <!-- Trọng số thành phần -->
              <td class="px-4 py-4 min-w-[200px]">
                <div class="space-y-1.5">
                  <div class="flex items-center justify-between text-xs">
                    <span class="text-muted">CC / Lab / GK / CK:</span>
                    <span class="font-bold text-heading">
                      {{ rule.weightAttendance }}% / {{ rule.weightLab }}% / {{ rule.weightMidterm }}% / {{ rule.weightFinal }}%
                    </span>
                  </div>
                  <!-- Thanh progress biểu diễn trọng số -->
                  <div class="lg-progress-track w-full h-2 flex">
                    <div class="h-full bg-sky-500" :style="{ width: rule.weightAttendance + '%' }" title="Chuyên cần"></div>
                    <div class="h-full bg-amber-500" :style="{ width: rule.weightLab + '%' }" title="Thực hành/Lab"></div>
                    <div class="h-full bg-indigo-500" :style="{ width: rule.weightMidterm + '%' }" title="Giữa kỳ"></div>
                    <div class="h-full bg-emerald-500" :style="{ width: rule.weightFinal + '%' }" title="Cuối kỳ"></div>
                  </div>
                </div>
              </td>

              <!-- Điểm liệt / Ràng buộc -->
              <td class="px-4 py-4">
                <div class="space-y-1 text-xs">
                  <div class="flex flex-wrap gap-1.5">
                    <span class="px-2 py-0.5 rounded bg-amber-500/10 text-amber-600 dark:text-amber-400 font-bold" v-if="rule.minLabScore > 0">
                      Lab &ge; {{ rule.minLabScore.toFixed(1) }}
                    </span>
                    <span class="px-2 py-0.5 rounded bg-indigo-500/10 text-indigo-600 dark:text-indigo-400 font-bold" v-if="rule.minMidtermScore > 0">
                      GK &ge; {{ rule.minMidtermScore.toFixed(1) }}
                    </span>
                    <span class="px-2 py-0.5 rounded bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 font-bold" v-if="rule.minFinalScore > 0">
                      CK &ge; {{ rule.minFinalScore.toFixed(1) }}
                    </span>
                  </div>
                  <div class="flex flex-wrap gap-1.5 mt-1 text-[10px]">
                    <span 
                      class="px-1.5 py-0.25 rounded border"
                      :class="rule.blockOnMissingLab ? 'border-rose-300 bg-rose-500/5 text-rose-500' : 'border-slate-300 text-muted'"
                    >
                      Missing Lab: {{ rule.blockOnMissingLab ? 'Trượt' : 'Bỏ qua' }}
                    </span>
                    <span 
                      class="px-1.5 py-0.25 rounded border"
                      :class="rule.blockOnIncomplete ? 'border-rose-300 bg-rose-500/5 text-rose-500' : 'border-slate-300 text-muted'"
                    >
                      Incomplete: {{ rule.blockOnIncomplete ? 'Trượt' : 'Bỏ qua' }}
                    </span>
                  </div>
                </div>
              </td>

              <!-- Vắng tối đa -->
              <td class="px-4 py-4 text-center">
                <div class="font-extrabold text-rose-500">{{ rule.maxAbsentSlots }} buổi</div>
                <div class="text-[10px] text-muted block mt-0.5">(~20% tổng số tiết)</div>
              </td>

              <!-- Trạng thái -->
              <td class="px-4 py-4 text-center">
                <span 
                  class="lg-badge"
                  :class="rule.status === 'Applied' ? 'lg-badge-success' : 'lg-badge-warning'"
                >
                  {{ rule.status === 'Applied' ? 'Đã áp dụng' : 'Bản nháp' }}
                </span>
              </td>

              <!-- Hành động -->
              <td class="px-4 py-4 text-center">
                <div class="flex items-center justify-center gap-2">
                  <!-- Nếu là Draft thì cho sửa -->
                  <button
                    v-if="rule.status === 'Draft'"
                    @click="openEditModal(rule)"
                    class="lg-btn-secondary text-xs px-2.5 py-1.5 flex items-center gap-1"
                    title="Chỉnh sửa bản nháp"
                  >
                    <Pencil class="w-3.5 h-3.5" />
                    Sửa
                  </button>

                  <!-- Nếu là Applied thì bị khóa sửa đổi, chỉ cho nhân bản -->
                  <button
                    v-else
                    @click="handleCloneAsDraft(rule)"
                    class="lg-btn-secondary text-xs px-2.5 py-1.5 flex items-center gap-1"
                    title="Nhân bản sang bản nháp mới để chỉnh sửa"
                  >
                    <Copy class="w-3.5 h-3.5" />
                    Thay đổi
                  </button>

                  <!-- Nút áp dụng đối với Draft -->
                  <button
                    v-if="rule.status === 'Draft'"
                    @click="openApplyModal(rule)"
                    class="lg-btn-primary text-xs px-2.5 py-1.5 flex items-center gap-1 font-bold"
                    title="Áp dụng quy tắc cho học kỳ"
                  >
                    <Check class="w-3.5 h-3.5" />
                    Áp dụng
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Form Modal Tạo mới / Chỉnh sửa quy tắc & Grade Weight Editor -->
      <div 
        v-if="isRuleModalOpen" 
        class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200"
      >
        <div class="w-full max-w-4xl lg-glass-strong lg-density-spacious rounded-2xl shadow-2xl relative max-h-[90vh] overflow-y-auto">
          <!-- Close button -->
          <button 
            @click="isRuleModalOpen = false"
            class="absolute top-4 right-4 text-muted hover:text-heading transition-colors lg-icon-button p-1.5 bg-surface-card rounded-lg border border-default"
          >
            <X class="w-5 h-5" />
          </button>

          <!-- Modal Title -->
          <div class="mb-5 pb-4 border-b border-default">
            <h2 class="text-xl font-extrabold text-heading flex items-center gap-2.5">
              <Award class="w-6 h-6 text-primary" />
              {{ editingMode === 'create' ? 'Tạo Quy Tắc Đạt/Rớt Môn Mới' : 'Cập Nhật Bản Nháp Quy Tắc' }}
            </h2>
            <p class="text-xs text-muted mt-1">
              Điền các thông tin đào tạo, thiết lập tỷ lệ điểm thành phần và ngưỡng điểm liệt quy chế học vụ.
            </p>
          </div>

          <!-- Form Content (2 Columns) -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
            <!-- Cột 1: Thông tin cơ bản & Grade Weight Editor -->
            <div class="space-y-4">
              <h3 class="font-extrabold text-sm text-primary border-b border-default pb-1 flex items-center gap-1.5">
                <BookOpen class="w-4 h-4" />
                1. Thông tin đào tạo & Trọng số
              </h3>

              <!-- Môn học -->
              <div>
                <label class="block text-xs font-bold text-label mb-1.5 uppercase">Môn học áp dụng</label>
                <select 
                  v-model="currentRule.subjectCode" 
                  :disabled="editingMode === 'edit'"
                  class="w-full px-3 lg-control text-sm"
                >
                  <option v-for="sub in subjects" :key="sub.code" :value="sub.code">
                    [{{ sub.code }}] {{ sub.name }}
                  </option>
                </select>
              </div>

              <!-- Cặp Học kỳ & Chương trình -->
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-bold text-label mb-1.5 uppercase">Học kỳ</label>
                  <select 
                    v-model="currentRule.semester" 
                    :disabled="editingMode === 'edit'"
                    class="w-full px-3 lg-control text-sm"
                  >
                    <option v-for="sem in semesters" :key="sem" :value="sem">{{ sem }}</option>
                  </select>
                </div>
                <div>
                  <label class="block text-xs font-bold text-label mb-1.5 uppercase">Chương trình</label>
                  <select 
                    v-model="currentRule.program" 
                    :disabled="editingMode === 'edit'"
                    class="w-full px-3 lg-control text-sm"
                  >
                    <option v-for="prog in trainingPrograms" :key="prog.code" :value="prog.code">
                      {{ prog.code }} ({{ prog.name }})
                    </option>
                  </select>
                </div>
              </div>

              <!-- Grade Weight Editor -->
              <div class="space-y-3 pt-2">
                <label class="block text-xs font-bold text-label uppercase">Thiết lập trọng số (%)</label>
                
                <!-- Chuyên cần -->
                <div class="space-y-1">
                  <div class="flex justify-between text-xs font-semibold">
                    <span class="text-muted">Chuyên cần</span>
                    <span class="text-heading font-bold">{{ currentRule.weightAttendance }}%</span>
                  </div>
                  <div class="flex items-center gap-3">
                    <input 
                      type="range" 
                      v-model.number="currentRule.weightAttendance" 
                      min="0" 
                      max="100" 
                      step="5"
                      class="w-full accent-primary" 
                    />
                    <input 
                      type="number" 
                      v-model.number="currentRule.weightAttendance" 
                      min="0" 
                      max="100" 
                      class="w-16 px-2 py-1 lg-control text-center text-xs font-bold" 
                    />
                  </div>
                </div>

                <!-- Thực hành / Lab -->
                <div class="space-y-1">
                  <div class="flex justify-between text-xs font-semibold">
                    <span class="text-muted">Thực hành (Lab)</span>
                    <span class="text-heading font-bold">{{ currentRule.weightLab }}%</span>
                  </div>
                  <div class="flex items-center gap-3">
                    <input 
                      type="range" 
                      v-model.number="currentRule.weightLab" 
                      min="0" 
                      max="100" 
                      step="5"
                      class="w-full accent-amber" 
                    />
                    <input 
                      type="number" 
                      v-model.number="currentRule.weightLab" 
                      min="0" 
                      max="100" 
                      class="w-16 px-2 py-1 lg-control text-center text-xs font-bold" 
                    />
                  </div>
                </div>

                <!-- Giữa kỳ -->
                <div class="space-y-1">
                  <div class="flex justify-between text-xs font-semibold">
                    <span class="text-muted">Thi giữa kỳ</span>
                    <span class="text-heading font-bold">{{ currentRule.weightMidterm }}%</span>
                  </div>
                  <div class="flex items-center gap-3">
                    <input 
                      type="range" 
                      v-model.number="currentRule.weightMidterm" 
                      min="0" 
                      max="100" 
                      step="5"
                      class="w-full accent-indigo" 
                    />
                    <input 
                      type="number" 
                      v-model.number="currentRule.weightMidterm" 
                      min="0" 
                      max="100" 
                      class="w-16 px-2 py-1 lg-control text-center text-xs font-bold" 
                    />
                  </div>
                </div>

                <!-- Cuối kỳ -->
                <div class="space-y-1">
                  <div class="flex justify-between text-xs font-semibold">
                    <span class="text-muted">Thi cuối kỳ</span>
                    <span class="text-heading font-bold">{{ currentRule.weightFinal }}%</span>
                  </div>
                  <div class="flex items-center gap-3">
                    <input 
                      type="range" 
                      v-model.number="currentRule.weightFinal" 
                      min="0" 
                      max="100" 
                      step="5"
                      class="w-full accent-emerald" 
                    />
                    <input 
                      type="number" 
                      v-model.number="currentRule.weightFinal" 
                      min="0" 
                      max="100" 
                      class="w-16 px-2 py-1 lg-control text-center text-xs font-bold" 
                    />
                  </div>
                </div>

                <!-- Thanh tiến trình tổng điểm phần trăm -->
                <div class="pt-3 border-t border-default/50">
                  <div class="flex justify-between text-xs mb-1.5">
                    <span class="font-bold text-label">Tổng trọng số điểm:</span>
                    <span 
                      class="font-extrabold text-sm"
                      :class="isWeightValid ? 'text-emerald-500 dark:text-emerald-400' : 'text-rose-500'"
                    >
                      {{ totalWeight }}% / 100%
                    </span>
                  </div>
                  <div class="lg-progress-track w-full h-3">
                    <div 
                      class="lg-progress-fill transition-all duration-300"
                      :class="isWeightValid ? 'bg-emerald-500' : totalWeight > 100 ? 'bg-rose-600' : 'bg-amber-500'"
                      :style="{ width: Math.min(totalWeight, 100) + '%' }"
                    ></div>
                  </div>
                  <p 
                    v-if="!isWeightValid"
                    class="text-[11px] font-semibold text-rose-500 mt-1.5 flex items-center gap-1"
                  >
                    <AlertTriangle class="w-3.5 h-3.5 flex-shrink-0" />
                    <span>
                      {{ totalWeight > 100 ? `Thừa ${totalWeight - 100}%. Tổng trọng số bắt buộc phải bằng 100%.` : `Thiếu ${100 - totalWeight}%. Tổng trọng số bắt buộc phải bằng 100%.` }}
                    </span>
                  </p>
                  <p 
                    v-else
                    class="text-[11px] font-semibold text-emerald-500 dark:text-emerald-400 mt-1.5 flex items-center gap-1"
                  >
                    <CheckCircle class="w-3.5 h-3.5 flex-shrink-0" />
                    <span>Hợp lệ: Đã đạt đúng 100%.</span>
                  </p>
                </div>
              </div>
            </div>

            <!-- Cột 2: Ngưỡng điểm liệt & Các ràng buộc rớt môn -->
            <div class="space-y-4">
              <h3 class="font-extrabold text-sm text-primary border-b border-default pb-1 flex items-center gap-1.5">
                <AlertTriangle class="w-4 h-4" />
                2. Điểm liệt & Ràng buộc rớt môn
              </h3>

              <!-- Điểm liệt thành phần -->
              <div class="space-y-3">
                <label class="block text-xs font-bold text-label uppercase">Ngưỡng điểm liệt tối thiểu (0.0 - 10.0)</label>
                
                <div class="grid grid-cols-2 gap-3">
                  <!-- Liệt Chuyên cần -->
                  <div>
                    <label class="block text-[11px] font-semibold text-muted mb-1">Điểm liệt Chuyên cần</label>
                    <input 
                      type="number" 
                      v-model.number="currentRule.minAttendanceScore" 
                      min="0" 
                      max="10" 
                      step="0.5"
                      class="w-full px-3 lg-control text-sm font-bold"
                      :class="isAttendanceScoreError ? 'border-rose-500 focus:border-rose-500 focus:ring-rose-500/10' : ''"
                    />
                    <span v-if="isAttendanceScoreError" class="text-[10px] text-rose-500 font-medium mt-0.5 block">Phải từ 0 đến 10</span>
                  </div>

                  <!-- Liệt Lab -->
                  <div>
                    <label class="block text-[11px] font-semibold text-muted mb-1">Điểm liệt Lab/Bài tập</label>
                    <input 
                      type="number" 
                      v-model.number="currentRule.minLabScore" 
                      min="0" 
                      max="10" 
                      step="0.5"
                      class="w-full px-3 lg-control text-sm font-bold"
                      :class="isLabScoreError ? 'border-rose-500 focus:border-rose-500 focus:ring-rose-500/10' : ''"
                    />
                    <span v-if="isLabScoreError" class="text-[10px] text-rose-500 font-medium mt-0.5 block">Phải từ 0 đến 10</span>
                  </div>

                  <!-- Liệt Giữa kỳ -->
                  <div>
                    <label class="block text-[11px] font-semibold text-muted mb-1">Điểm liệt Giữa kỳ</label>
                    <input 
                      type="number" 
                      v-model.number="currentRule.minMidtermScore" 
                      min="0" 
                      max="10" 
                      step="0.5"
                      class="w-full px-3 lg-control text-sm font-bold"
                      :class="isMidtermScoreError ? 'border-rose-500 focus:border-rose-500 focus:ring-rose-500/10' : ''"
                    />
                    <span v-if="isMidtermScoreError" class="text-[10px] text-rose-500 font-medium mt-0.5 block">Phải từ 0 đến 10</span>
                  </div>

                  <!-- Liệt Cuối kỳ -->
                  <div>
                    <label class="block text-[11px] font-semibold text-muted mb-1">Điểm liệt Cuối kỳ</label>
                    <input 
                      type="number" 
                      v-model.number="currentRule.minFinalScore" 
                      min="0" 
                      max="10" 
                      step="0.5"
                      class="w-full px-3 lg-control text-sm font-bold"
                      :class="isFinalScoreError ? 'border-rose-500 focus:border-rose-500 focus:ring-rose-500/10' : ''"
                    />
                    <span v-if="isFinalScoreError" class="text-[10px] text-rose-500 font-medium mt-0.5 block">Phải từ 0 đến 10</span>
                  </div>
                </div>
              </div>

              <!-- Số buổi vắng tối đa -->
              <div>
                <label class="block text-xs font-bold text-label mb-1.5 uppercase">Số buổi vắng tối đa cho phép (Hạn mức)</label>
                <div class="flex items-center gap-3">
                  <input 
                    type="number" 
                    v-model.number="currentRule.maxAbsentSlots" 
                    min="0" 
                    max="20"
                    class="w-full px-3 lg-control text-sm font-bold"
                    :class="isAbsentSlotsError ? 'border-rose-500' : ''"
                  />
                  <span class="text-xs text-muted font-bold whitespace-nowrap">buổi (~20%)</span>
                </div>
                <span v-if="isAbsentSlotsError" class="text-[10px] text-rose-500 font-medium mt-0.5 block">Số buổi vắng không được nhỏ hơn 0</span>
              </div>

              <!-- Checkbox các điều kiện đặc biệt -->
              <div class="space-y-2.5 pt-2">
                <label class="block text-xs font-bold text-label uppercase mb-1.5">Ràng buộc đặc biệt</label>
                
                <!-- Block on Missing Lab -->
                <label class="flex items-start gap-2.5 cursor-pointer text-xs">
                  <input 
                    type="checkbox" 
                    v-model="currentRule.blockOnMissingLab"
                    class="mt-0.5 rounded text-primary focus:ring-primary border-default"
                  />
                  <div>
                    <span class="font-bold text-heading">Thiếu bài thực hành / Lab bắt buộc (Block on Missing Lab)</span>
                    <span class="block text-[10px] text-muted mt-0.5">
                      Đánh rớt môn ngay lập tức nếu sinh viên nộp thiếu các bài Lab bắt buộc dù điểm trung bình đạt chuẩn.
                    </span>
                  </div>
                </label>

                <!-- Block on Incomplete -->
                <label class="flex items-start gap-2.5 cursor-pointer text-xs">
                  <input 
                    type="checkbox" 
                    v-model="currentRule.blockOnIncomplete"
                    class="mt-0.5 rounded text-primary focus:ring-primary border-default"
                  />
                  <div>
                    <span class="font-bold text-heading">Không hoàn thành các cột điểm (Block on Incomplete)</span>
                    <span class="block text-[10px] text-muted mt-0.5">
                      Đánh rớt (Incomplete) nếu sinh viên bỏ trống bất kỳ thành phần điểm nào trong kỳ (không tham gia thi/kiểm tra).
                    </span>
                  </div>
                </label>
              </div>

              <!-- Validation Panel (Tổng hợp cảnh báo) -->
              <div class="lg-glass-soft p-3 rounded-xl border border-default space-y-2 mt-4">
                <h4 class="text-xs font-bold text-heading flex items-center gap-1.5">
                  <Activity class="w-4 h-4 text-primary" />
                  Bảng kiểm tra tính hợp lệ (Validation Panel)
                </h4>
                
                <ul class="text-[11px] space-y-1.5">
                  <!-- Check Trọng số -->
                  <li class="flex items-center justify-between font-semibold">
                    <span class="text-muted">Tổng trọng số thành phần = 100%</span>
                    <span 
                      class="px-2 py-0.5 rounded text-[9px] font-bold"
                      :class="isWeightValid ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400' : 'bg-rose-500/10 text-rose-600 dark:text-rose-400'"
                    >
                      {{ isWeightValid ? 'Đạt' : 'Chưa Đạt' }}
                    </span>
                  </li>

                  <!-- Check Thang điểm -->
                  <li class="flex items-center justify-between font-semibold">
                    <span class="text-muted">Thang điểm các cột liệt [0 - 10]</span>
                    <span 
                      class="px-2 py-0.5 rounded text-[9px] font-bold"
                      :class="isScoresValid ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400' : 'bg-rose-500/10 text-rose-600 dark:text-rose-400'"
                    >
                      {{ isScoresValid ? 'Hợp lệ' : 'Ngoài khung' }}
                    </span>
                  </li>

                  <!-- Check các trường bắt buộc -->
                  <li class="flex items-center justify-between font-semibold">
                    <span class="text-muted">Khai báo đầy đủ thông tin</span>
                    <span 
                      class="px-2 py-0.5 rounded text-[9px] font-bold"
                      :class="isFormFilled ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400' : 'bg-rose-500/10 text-rose-600 dark:text-rose-400'"
                    >
                      {{ isFormFilled ? 'Xong' : 'Thiếu' }}
                    </span>
                  </li>
                </ul>
              </div>
            </div>
          </div>

          <!-- Modal Footer (Actions) -->
          <div class="flex items-center justify-end gap-3 pt-4 border-t border-default">
            <button
              @click="isRuleModalOpen = false"
              class="lg-btn-secondary px-4 py-2 text-sm font-bold"
            >
              Hủy bỏ
            </button>
            <button
              @click="handleSaveRule"
              :disabled="!isFormValid"
              class="lg-btn-primary px-5 py-2 text-sm font-bold flex items-center gap-1.5 disabled:opacity-40 disabled:cursor-not-allowed"
            >
              <Save class="w-4 h-4" />
              Lưu Bản Nháp
            </button>
          </div>
        </div>
      </div>

      <!-- Confirm Apply Modal -->
      <div 
        v-if="isConfirmApplyModalOpen" 
        class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200"
      >
        <div class="w-full max-w-lg lg-glass-strong lg-density-spacious rounded-xl shadow-2xl relative">
          <!-- Close button -->
          <button 
            @click="isConfirmApplyModalOpen = false"
            class="absolute top-4 right-4 text-muted hover:text-heading transition-colors lg-icon-button p-1"
          >
            <X class="w-4.5 h-4.5" />
          </button>

          <!-- Warning Header -->
          <div class="flex items-start gap-3.5 mb-4">
            <div class="w-10 h-10 rounded-full bg-rose-500/10 flex items-center justify-center text-rose-500 flex-shrink-0 animate-bounce">
              <AlertTriangle class="w-5.5 h-5.5" />
            </div>
            <div>
              <h3 class="text-lg font-extrabold text-rose-600 dark:text-rose-400">Xác Nhận Áp Dụng Quy Tắc</h3>
              <p class="text-xs text-muted mt-1">Hành động này mang tính chất ảnh hưởng toàn hệ thống đào tạo.</p>
            </div>
          </div>

          <!-- Alert warning content -->
          <div class="lg-alert lg-alert-warning mb-4">
            <div class="flex gap-2">
              <Info class="w-5 h-5 flex-shrink-0 mt-0.5" />
              <div class="text-xs font-bold leading-relaxed">
                Sau khi quy tắc này được áp dụng, Stored Procedure của hệ thống sẽ tự động tính toán lại điểm trung bình GPA và trạng thái đạt/trượt cho toàn bộ sinh viên đang theo học môn học này. Hành động này không thể hoàn tác.
              </div>
            </div>
          </div>

          <!-- Yêu cầu nhập lý do -->
          <div class="mb-5">
            <label class="block text-xs font-bold text-label mb-2 uppercase flex items-center gap-1">
              Lý do điều phối quy chế (Bắt buộc)
              <HelpCircle class="w-3.5 h-3.5 text-muted" title="Nhập lý do học thuật cụ thể để lưu vết vào Audit logs" />
            </label>
            <textarea
              v-model="applyReason"
              rows="3"
              placeholder="Ví dụ: Cập nhật ngưỡng điểm liệt và cấu trúc điểm môn PRN211 theo biên bản cuộc họp Hội đồng khoa số 12/QD-HD..."
              class="w-full px-3 py-2 lg-control text-sm"
            ></textarea>
            <span v-if="!applyReason.trim()" class="text-[10px] text-rose-500 font-semibold mt-1 block">Vui lòng điền lý do điều phối để tiếp tục</span>
          </div>

          <!-- Footer Actions -->
          <div class="flex items-center justify-end gap-2.5">
            <button
              @click="isConfirmApplyModalOpen = false"
              class="lg-btn-secondary px-4 py-2 text-sm font-bold"
            >
              Hủy bỏ
            </button>
            <button
              @click="handleConfirmApply"
              :disabled="!applyReason.trim()"
              class="lg-btn-danger px-5 py-2 text-sm font-bold flex items-center gap-1.5 disabled:opacity-40 disabled:cursor-not-allowed"
            >
              <Check class="w-4 h-4" />
              Áp dụng ngay
            </button>
          </div>
        </div>
      </div>

      <!-- Khung Audit Logs (Lịch sử hoạt động) -->
      <div class="lg-glass-soft lg-card lg-density-normal">
        <div class="flex items-center gap-2 mb-4 pb-3 border-b border-default">
          <History class="w-5 h-5 text-primary" />
          <div>
            <h3 class="font-extrabold text-heading text-sm">Nhật ký hoạt động điều phối (Audit Logs)</h3>
            <p class="text-xs text-muted mt-0.5">Lịch sử thiết lập và sửa đổi quy chế điểm, đảm bảo tính minh bạch học vụ.</p>
          </div>
        </div>

        <div class="space-y-3.5">
          <div 
            v-for="log in auditLogs" 
            :key="log.id" 
            class="flex flex-col sm:flex-row sm:items-start justify-between p-3 rounded-lg bg-surface-card border border-default/30 text-xs gap-3 hover:bg-surface-card-hover transition-colors"
          >
            <div class="space-y-1">
              <div class="flex flex-wrap items-center gap-2">
                <span class="font-bold text-heading">{{ log.actor }}</span>
                <span 
                  class="lg-badge py-0.5 px-2 text-[9px] font-extrabold"
                  :class="log.action === 'Áp dụng quy tắc' ? 'lg-badge-success' : 'lg-badge-info'"
                >
                  {{ log.action }}
                </span>
              </div>
              <p class="text-body font-medium leading-relaxed">{{ log.details }}</p>
              <p class="text-[11px] text-muted italic flex items-center gap-1 mt-1">
                <Info class="w-3.5 h-3.5 flex-shrink-0" />
                <span>Lý do: {{ log.reason }}</span>
              </p>
            </div>
            
            <div class="text-[10px] text-muted font-semibold whitespace-nowrap self-end sm:self-start">
              {{ log.time }}
            </div>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<style scoped>
/* Slider & Number Control Styles */
input[type="range"] {
  height: 6px;
  border-radius: 9999px;
  background: var(--surface-input);
  outline: none;
}
input[type="number"]::-webkit-inner-spin-button,
input[type="number"]::-webkit-outer-spin-button {
  -webkit-appearance: none;
  margin: 0;
}
input[type="number"] {
  -moz-appearance: textfield;
}
</style>

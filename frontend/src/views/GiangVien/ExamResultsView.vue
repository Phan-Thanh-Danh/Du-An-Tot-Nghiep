<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  Search, Award, Clock, Download, Filter, 
  ChevronRight, User, TrendingUp, CheckCircle2, AlertCircle, Calendar,
  X, CheckCircle, XCircle, FileText, Loader2, HelpCircle
} from 'lucide-vue-next'
import { teacherApi } from '@/services/teacherApi'

const loadingSessions = ref(false)
const loadingDetail = ref(false)
const loadingStudentDetail = ref(false)
const error = ref('')

const examSessions = ref([])
const selectedExamId = ref(null)
const currentSessionDetail = ref(null)

const searchQuery = ref('')
const selectedStatusFilter = ref('all')

const isDrawerOpen = ref(false)
const selectedStudent = ref(null)
const studentExamDetail = ref(null)

// 1. Tải danh sách ca thi giảng viên phụ trách
async function loadSessions() {
  loadingSessions.value = true
  error.value = ''
  try {
    const data = await teacherApi.getExamResults()
    examSessions.value = Array.isArray(data) ? data : (data?.items ?? data?.data ?? [])
    
    if (examSessions.value.length > 0) {
      selectedExamId.value = examSessions.value[0].examId
      await loadSessionDetail(selectedExamId.value)
    }
  } catch (e) {
    error.value = e?.message || 'Không thể tải danh sách ca thi.'
    examSessions.value = []
  } finally {
    loadingSessions.value = false
  }
}

// 2. Tải danh sách kết quả thí sinh trong CaThi được chọn
async function loadSessionDetail(examId) {
  if (!examId) return
  loadingDetail.value = true
  try {
    const data = await teacherApi.getCaThiStudentResults(examId)
    currentSessionDetail.value = data
  } catch (e) {
    console.error('Lỗi khi tải chi tiết ca thi:', e)
    currentSessionDetail.value = null
  } finally {
    loadingDetail.value = false
  }
}

async function onExamChange() {
  if (selectedExamId.value) {
    await loadSessionDetail(selectedExamId.value)
  }
}

// 3. Mở drawer xem chi tiết câu hỏi & đáp án của sinh viên
async function openStudentDrawer(student) {
  selectedStudent.value = student
  isDrawerOpen.value = true
  loadingStudentDetail.value = true
  studentExamDetail.value = null

  try {
    const detail = await teacherApi.getStudentExamDetail(selectedExamId.value, student.maHocSinh)
    studentExamDetail.value = detail
  } catch (e) {
    console.error('Lỗi tải chi tiết bài thi sinh viên:', e)
    studentExamDetail.value = null
  } finally {
    loadingStudentDetail.value = false
  }
}

const closeDrawer = () => {
  isDrawerOpen.value = false
  setTimeout(() => {
    selectedStudent.value = null
    studentExamDetail.value = null
  }, 300)
}

// Stats & Filters
const activeSessionInfo = computed(() => {
  return examSessions.value.find(s => s.examId === selectedExamId.value) || currentSessionDetail.value || {}
})

// Tính toán biểu đồ Phổ điểm (Score Distribution)
const scoreDistribution = computed(() => {
  const students = currentSessionDetail.value?.students || []
  const total = students.length || 1

  const c1 = students.filter(s => (s.diem || 0) <= 2.0).length
  const c2 = students.filter(s => (s.diem || 0) > 2.0 && (s.diem || 0) < 5.0).length
  const c3 = students.filter(s => (s.diem || 0) >= 5.0 && (s.diem || 0) <= 6.4).length
  const c4 = students.filter(s => (s.diem || 0) > 6.4 && (s.diem || 0) <= 7.9).length
  const c5 = students.filter(s => (s.diem || 0) >= 8.0).length

  const maxCount = Math.max(c1, c2, c3, c4, c5, 1)

  return [
    { range: '0.0 - 2.0', label: 'Yếu / Bỏ bài', count: c1, percent: Math.round((c1 / total) * 100), barHeight: Math.round((c1 / maxCount) * 100), hexColor: '#f43f5e', bgBadge: 'bg-rose-500/10 text-rose-600 border-rose-500/30' },
    { range: '2.1 - 4.9', label: 'Chưa đạt', count: c2, percent: Math.round((c2 / total) * 100), barHeight: Math.round((c2 / maxCount) * 100), hexColor: '#f59e0b', bgBadge: 'bg-amber-500/10 text-amber-600 border-amber-500/30' },
    { range: '5.0 - 6.4', label: 'Trung bình', count: c3, percent: Math.round((c3 / total) * 100), barHeight: Math.round((c3 / maxCount) * 100), hexColor: '#0ea5e9', bgBadge: 'bg-sky-500/10 text-sky-600 border-sky-500/30' },
    { range: '6.5 - 7.9', label: 'Khá', count: c4, percent: Math.round((c4 / total) * 100), barHeight: Math.round((c4 / maxCount) * 100), hexColor: '#6366f1', bgBadge: 'bg-indigo-500/10 text-indigo-600 border-indigo-500/30' },
    { range: '8.0 - 10.0', label: 'Giỏi / Xuất sắc', count: c5, percent: Math.round((c5 / total) * 100), barHeight: Math.round((c5 / maxCount) * 100), hexColor: '#10b981', bgBadge: 'bg-emerald-500/10 text-emerald-600 border-emerald-500/30' }
  ]
})

// Tính toán biểu đồ Tròn (Donut Chart Đạt / Không đạt)
const passFailDonut = computed(() => {
  const students = currentSessionDetail.value?.students || []
  const total = students.length
  if (!total) {
    return { pass: 0, fail: 0, total: 0, passPercent: 0, failPercent: 0, strokeDash: '0 251.32' }
  }

  const pass = students.filter(s => (s.diem || 0) >= 5.0).length
  const fail = total - pass
  const passPercent = Math.round((pass / total) * 100)
  const failPercent = 100 - passPercent

  const circumference = 251.32 // 2 * PI * 40
  const passDash = (passPercent / 100) * circumference

  return {
    pass,
    fail,
    total,
    passPercent,
    failPercent,
    circumference,
    passDash
  }
})

const filteredStudents = computed(() => {
  if (!currentSessionDetail.value?.students) return []
  let list = currentSessionDetail.value.students

  if (searchQuery.value.trim()) {
    const q = searchQuery.value.toLowerCase().trim()
    list = list.filter(s => 
      (s.hoTen || '').toLowerCase().includes(q) || 
      (s.maSinhVien || '').toLowerCase().includes(q)
    )
  }

  if (selectedStatusFilter.value === 'pass') {
    list = list.filter(s => (s.diem || 0) >= 5.0)
  } else if (selectedStatusFilter.value === 'fail') {
    list = list.filter(s => (s.diem || 0) < 5.0)
  }

  return list
})

onMounted(() => {
  loadSessions()
})
</script>

<template>
  <div class="space-y-6 pb-10 text-body">
    <!-- ── Header ── -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 surface-card border border-card rounded-2xl p-5 shadow-sm relative overflow-hidden">
      <div class="relative z-10 flex items-center gap-5">
        <div class="h-12 w-12 rounded-2xl bg-(--color-info-bg) flex items-center justify-center text-(--color-info-text) border border-(--color-info-text)/20 shadow-sm">
           <Award :size="28" />
        </div>
        <div>
          <h1 class="text-xl font-semibold text-heading tracking-tight">Kết quả bài thi</h1>
          <p class="text-sm font-medium text-muted mt-1">Xem điểm số và bảng trả lời chi tiết của thí sinh theo từng ca thi.</p>
        </div>
      </div>
      <div class="relative z-10 flex items-center gap-3">
         <button class="flex items-center gap-2 rounded-2xl surface-input px-5 py-3 border border-input shadow-sm hover:text-link transition-colors font-semibold text-sm text-label">
            <Download :size="18" /> Xuất kết quả Báo cáo
         </button>
      </div>
    </div>

    <!-- Dropdown Chọn ca thi & Quick Stats -->
    <div class="flex flex-col xl:flex-row gap-4">
       <!-- Select Ca thi -->
       <div class="xl:w-1/3 rounded-2xl surface-card border border-card p-4 shadow-sm flex flex-col justify-between">
          <label class="text-xs font-semibold text-muted uppercase tracking-wider mb-2 flex items-center gap-2">
            <Calendar :size="16" class="text-link" /> Chọn Ca thi đã canh thi:
          </label>
          <div class="relative">
            <select 
              v-model="selectedExamId" 
              @change="onExamChange"
              class="w-full rounded-xl border border-input surface-input pl-4 pr-10 py-3 text-sm font-semibold text-heading outline-none focus:border-(--border-input-focus) transition-colors cursor-pointer appearance-none truncate"
            >
              <option v-for="s in examSessions" :key="s.examId" :value="s.examId">
                {{ s.examTitle }} (Môn: {{ s.subject }} - {{ s.date }})
              </option>
            </select>
            <ChevronRight :size="16" class="absolute right-4 top-1/2 -translate-y-1/2 text-placeholder rotate-90 pointer-events-none" />
          </div>
          <p v-if="activeSessionInfo?.room" class="text-xs text-muted mt-2">
            📍 {{ activeSessionInfo.room }} | ⏰ {{ activeSessionInfo.startTime || '' }} - {{ activeSessionInfo.endTime || '' }}
          </p>
       </div>

       <!-- Stats Cards -->
       <div class="flex-1 grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div class="rounded-2xl surface-card border border-card p-4 shadow-sm">
             <div class="flex items-center justify-between mb-2">
                <div class="h-9 w-9 rounded-xl bg-(--color-info-bg) flex items-center justify-center text-(--color-info-text)">
                   <TrendingUp :size="18" />
                </div>
                <span class="text-[10px] font-semibold uppercase tracking-widest text-muted bg-(--surface-input) px-2 py-1 rounded-lg">Điểm trung bình</span>
             </div>
             <p class="text-2xl font-semibold text-heading">{{ activeSessionInfo?.avgScore ?? '0.0' }}</p>
          </div>
          <div class="rounded-2xl surface-card border border-card p-4 shadow-sm">
             <div class="flex items-center justify-between mb-2">
                <div class="h-9 w-9 rounded-xl bg-(--color-success-bg) flex items-center justify-center text-(--color-success-text)">
                   <CheckCircle2 :size="18" />
                </div>
                <span class="text-[10px] font-semibold uppercase tracking-widest text-muted bg-(--surface-input) px-2 py-1 rounded-lg">Tỷ lệ Đạt</span>
             </div>
             <p class="text-2xl font-semibold text-heading">{{ activeSessionInfo?.passRate ?? 0 }}%</p>
          </div>
          <div class="rounded-2xl surface-card border border-card p-4 shadow-sm">
             <div class="flex items-center justify-between mb-2">
                <div class="h-9 w-9 rounded-xl bg-(--color-warning-bg) flex items-center justify-center text-(--color-warning-text)">
                   <Award :size="18" />
                </div>
                <span class="text-[10px] font-semibold uppercase tracking-widest text-muted bg-(--surface-input) px-2 py-1 rounded-lg">Cao nhất</span>
             </div>
             <p class="text-2xl font-semibold text-heading">{{ activeSessionInfo?.highestScore ?? '0.0' }}</p>
          </div>
       </div>
    </div>

    <!-- ── Biểu đồ thống kê Ca Thi (Compact & Sleek) ── -->
    <div v-if="currentSessionDetail?.students?.length" class="grid grid-cols-1 lg:grid-cols-3 gap-4 animate-fade-in-up">
      <!-- 📊 Biểu đồ Phổ điểm (Score Distribution Bar Chart) -->
      <div class="lg:col-span-2 surface-card border border-card rounded-2xl p-4 shadow-sm flex flex-col justify-between">
        <div class="flex items-center justify-between mb-3">
          <div class="flex items-center gap-2">
            <div class="h-7 w-7 rounded-lg bg-(--color-info-bg) text-(--color-info-text) flex items-center justify-center">
              <TrendingUp :size="15" />
            </div>
            <div>
              <h3 class="text-xs font-bold text-heading">Phổ điểm thi ca này</h3>
              <p class="text-[11px] text-muted">Phân bố số lượng thí sinh theo dải điểm</p>
            </div>
          </div>
          <span class="text-[11px] font-semibold text-muted bg-(--surface-input) px-2.5 py-0.5 rounded-full border border-default">
            Tổng: {{ passFailDonut.total }} thí sinh
          </span>
        </div>

        <!-- Sleek Histogram Bars -->
        <div class="relative pt-3 pb-2 px-3 surface-solid rounded-xl border border-default">
          <!-- Background Grid Lines -->
          <div class="absolute inset-x-3 top-7 bottom-9 flex flex-col justify-between pointer-events-none opacity-25">
            <div class="border-b border-dashed border-default w-full"></div>
            <div class="border-b border-dashed border-default w-full"></div>
          </div>

          <div class="grid grid-cols-5 gap-2 relative z-10">
            <div 
              v-for="(item, idx) in scoreDistribution" 
              :key="idx" 
              class="flex flex-col items-center group cursor-pointer"
            >
              <!-- Count & % badge -->
              <span class="text-[11px] font-bold text-heading mb-1 transition-transform group-hover:scale-110">
                {{ item.count }} <span class="text-[9px] text-muted font-normal">({{ item.percent }}%)</span>
              </span>

              <!-- Fixed Height Bar Track (h-24 = 96px height) -->
              <div class="w-full h-24 flex items-end justify-center py-1">
                <div 
                  class="w-7 rounded-none transition-all duration-500 shadow-xs group-hover:opacity-90"
                  :style="{ 
                    height: Math.max(item.barHeight, item.count > 0 ? 12 : 4) + '%',
                    backgroundColor: item.hexColor
                  }"
                ></div>
              </div>

              <!-- X-axis Label -->
              <div class="text-center pt-1.5 border-t border-default/40 w-full mt-1">
                <span class="text-[10px] font-bold text-heading block leading-tight">{{ item.range }}</span>
                <span class="text-[9px] font-medium text-muted block truncate max-w-[65px] mx-auto">{{ item.label }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- ⭕ Biểu đồ Tròn Tỷ lệ Đạt / Không đạt (Sleek Donut Chart) -->
      <div class="surface-card border border-card rounded-2xl p-4 shadow-sm flex flex-col justify-between">
        <div class="flex items-center gap-2 mb-2">
          <div class="h-7 w-7 rounded-lg bg-(--color-success-bg) text-(--color-success-text) flex items-center justify-center">
            <CheckCircle2 :size="15" />
          </div>
          <div>
            <h3 class="text-xs font-bold text-heading">Tỷ lệ Đạt / Không đạt</h3>
            <p class="text-[11px] text-muted">Tổng quan kết quả ca thi</p>
          </div>
        </div>

        <!-- SVG Donut Chart -->
        <div class="flex items-center justify-center py-1 relative">
          <svg class="w-28 h-28 transform -rotate-90" viewBox="0 0 100 100">
            <!-- Background circle (Fail / Base) -->
            <circle
              cx="50"
              cy="50"
              r="38"
              fill="transparent"
              stroke="#f43f5e"
              stroke-opacity="0.2"
              stroke-width="8"
            />
            <!-- Pass Arc -->
            <circle
              v-if="passFailDonut.passPercent > 0"
              cx="50"
              cy="50"
              r="38"
              fill="transparent"
              stroke="#10b981"
              stroke-width="8"
              :stroke-dasharray="`${(passFailDonut.passPercent / 100) * 238.76} ${238.76 - (passFailDonut.passPercent / 100) * 238.76}`"
              :stroke-dashoffset="0"
              stroke-linecap="round"
              class="transition-all duration-700"
            />
          </svg>

          <!-- Center Text -->
          <div class="absolute inset-0 flex flex-col items-center justify-center text-center">
            <span class="text-xl font-extrabold text-heading tracking-tight">{{ passFailDonut.passPercent }}%</span>
            <span class="text-[9px] font-bold uppercase tracking-wider text-emerald-600">ĐẠT CẢ CA</span>
          </div>
        </div>

        <!-- Donut Legend -->
        <div class="grid grid-cols-2 gap-2 pt-2 border-t border-default text-[11px]">
          <div class="flex items-center gap-1.5 p-1.5 rounded-lg surface-solid border border-default">
            <div class="h-2.5 w-2.5 rounded-full bg-emerald-500 shrink-0"></div>
            <div class="truncate">
              <span class="font-bold text-heading block leading-tight">Đạt: {{ passFailDonut.pass }} SV</span>
              <span class="text-[9px] text-muted font-medium">({{ passFailDonut.passPercent }}%)</span>
            </div>
          </div>
          <div class="flex items-center gap-1.5 p-1.5 rounded-lg surface-solid border border-default">
            <div class="h-2.5 w-2.5 rounded-full bg-rose-500 shrink-0"></div>
            <div class="truncate">
              <span class="font-bold text-heading block leading-tight">Trượt: {{ passFailDonut.fail }} SV</span>
              <span class="text-[9px] text-muted font-medium">({{ passFailDonut.failPercent }}%)</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Search & Filter Bar -->
    <div class="rounded-2xl surface-card border border-card p-4 shadow-sm flex flex-col sm:flex-row items-center justify-between gap-4">
      <div class="relative w-full sm:w-80">
        <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
        <input 
          v-model="searchQuery"
          type="text" 
          placeholder="Tìm sinh viên theo tên hoặc MSSV..." 
          class="w-full rounded-xl border border-input surface-input pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:border-(--border-input-focus) transition-colors"
        />
      </div>

      <div class="flex items-center gap-3 w-full sm:w-auto">
        <span class="text-xs font-semibold text-muted shrink-0 flex items-center gap-1"><Filter :size="14" /> Lọc kết quả:</span>
        <select 
          v-model="selectedStatusFilter"
          class="rounded-xl border border-input surface-input px-3 py-2 text-xs font-semibold text-label outline-none focus:border-(--border-input-focus) cursor-pointer"
        >
          <option value="all">Tất cả sinh viên</option>
          <option value="pass">Chỉ sinh viên Đạt (>= 5.0)</option>
          <option value="fail">Sinh viên Không đạt (< 5.0)</option>
        </select>
      </div>
    </div>

    <!-- Loading sessions -->
    <div v-if="loadingSessions || loadingDetail" class="flex flex-col items-center justify-center py-20">
      <Loader2 :size="32" class="animate-spin text-muted mb-4" />
      <p class="text-sm font-semibold text-muted">Đang tải kết quả ca thi...</p>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="flex flex-col items-center justify-center py-20">
      <AlertCircle :size="48" class="text-rose-400 mb-4" />
      <p class="text-sm font-semibold text-heading mb-2">Có lỗi xảy ra</p>
      <p class="text-sm text-muted mb-4">{{ error }}</p>
      <button class="btn-primary" @click="loadSessions">Thử lại</button>
    </div>

    <!-- Empty -->
    <div v-else-if="!filteredStudents.length" class="flex flex-col items-center justify-center py-16 surface-card rounded-2xl border border-card">
      <Award :size="48" class="text-placeholder mb-4" />
      <p class="text-sm font-semibold text-heading mb-1">Không có kết quả nào</p>
      <p class="text-xs text-muted">Vui lòng chọn ca thi khác hoặc thay đổi từ khóa tìm kiếm.</p>
    </div>

    <!-- Results Table -->
    <div v-else class="rounded-2xl border border-card surface-card shadow-sm overflow-hidden animate-fade-in-up">
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid border-b border-default">
              <th class="px-5 py-4 text-[11px] font-semibold uppercase tracking-widest text-muted">Thí sinh</th>
              <th class="px-4 py-4 text-[11px] font-semibold uppercase tracking-widest text-muted">Điểm số</th>
              <th class="px-4 py-4 text-[11px] font-semibold uppercase tracking-widest text-muted">Số câu đúng</th>
              <th class="px-4 py-4 text-[11px] font-semibold uppercase tracking-widest text-muted">Thời gian làm bài</th>
              <th class="px-4 py-4 text-[11px] font-semibold uppercase tracking-widest text-muted">Trạng thái</th>
              <th class="px-5 py-4 text-[11px] font-semibold uppercase tracking-widest text-muted text-right">Chi tiết</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="res in filteredStudents" :key="res.maHocSinh" class="group hover:bg-(--surface-input) transition-colors">
              <td class="px-5 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-10 w-10 rounded-2xl surface-solid border border-default flex items-center justify-center text-muted font-semibold text-sm group-hover:bg-(--color-info-bg) group-hover:text-(--color-info-text) transition-colors shadow-sm">
                    {{ (res.hoTen || '?').split(' ').pop()[0] }}
                  </div>
                  <div>
                    <p class="text-sm font-semibold text-heading group-hover:text-link transition-colors">{{ res.hoTen }}</p>
                    <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-0.5">{{ res.maSinhVien }}</p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-2">
                   <div :class="['h-9 w-9 rounded-xl flex items-center justify-center border font-bold text-sm', 
                                (res.diem || 0) >= 8 ? 'bg-(--color-success-bg) border-(--color-success-text)/20 text-(--color-success-text)' :
                                (res.diem || 0) >= 5 ? 'bg-(--color-info-bg) border-(--color-info-text)/20 text-(--color-info-text)' :
                                'bg-(--color-danger-bg) border-(--color-danger-text)/20 text-(--color-danger-text)']">
                      {{ (res.diem || 0).toFixed(1) }}
                   </div>
                </div>
              </td>
              <td class="px-4 py-4 text-sm font-medium text-heading">
                <span v-if="res.tongSoCau">{{ res.soCauDung }} / {{ res.tongSoCau }} câu</span>
                <span v-else>--</span>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-1.5 text-xs font-medium text-muted">
                   <Clock :size="14" class="text-link" />
                   {{ res.thoiGianLam }}
                </div>
              </td>
              <td class="px-4 py-4">
                <span :class="['px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-wider border',
                  (res.diem || 0) >= 5 
                    ? 'bg-(--color-success-bg) text-(--color-success-text) border-(--color-success-text)/20' 
                    : 'bg-(--color-danger-bg) text-(--color-danger-text) border-(--color-danger-text)/20']">
                  {{ (res.diem || 0) >= 5 ? 'Đạt' : 'Không đạt' }}
                </span>
              </td>
              <td class="px-5 py-4 text-right">
                <button 
                  @click="openStudentDrawer(res)" 
                  class="inline-flex items-center justify-center h-9 px-3.5 rounded-xl border border-input surface-input text-xs font-semibold text-muted hover:text-link hover:border-(--border-input-focus) transition-colors shadow-sm gap-1"
                >
                   Xem bài làm <ChevronRight :size="14" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <!-- Footer -->
      <div class="surface-solid px-5 py-3 border-t border-default flex items-center justify-between">
         <div class="flex items-center gap-2 text-xs font-semibold text-muted">
            <User :size="14" /> Hiển thị {{ filteredStudents.length }} / {{ currentSessionDetail?.students?.length || 0 }} thí sinh
         </div>
      </div>
    </div>
  </div>

  <!-- Slide-over Drawer / Modal xem bài làm chi tiết của Sinh Viên -->
  <Teleport to="body">
    <!-- Backdrop -->
    <div 
      v-if="isDrawerOpen" 
      class="fixed inset-0 z-[9998] bg-(--surface-backdrop) backdrop-blur-sm transition-opacity"
      @click="closeDrawer"
    ></div>

    <!-- Drawer Panel -->
    <div 
      class="fixed inset-y-0 right-0 z-[9999] w-full max-w-2xl surface-modal shadow-2xl border-l border-card transition-transform duration-300 ease-in-out flex flex-col"
      :class="isDrawerOpen ? 'translate-x-0' : 'translate-x-full'"
    >
      <template v-if="selectedStudent">
        <!-- Header -->
        <div class="flex items-center justify-between p-4 border-b border-default surface-solid">
          <div class="flex items-center gap-3">
            <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) text-(--color-info-text) flex items-center justify-center font-bold text-base shadow-sm border border-(--color-info-text)/20">
              {{ (selectedStudent.hoTen || '?').split(' ').pop()[0] }}
            </div>
            <div>
              <h2 class="text-base font-semibold text-heading">{{ selectedStudent.hoTen }}</h2>
              <p class="text-[11px] font-medium text-muted">MSSV: {{ selectedStudent.maSinhVien }}</p>
            </div>
          </div>
          <button @click="closeDrawer" class="h-9 w-9 rounded-xl flex items-center justify-center surface-input border border-input text-muted hover:text-heading transition-colors">
            <X :size="18" />
          </button>
        </div>

        <!-- Student Score Overview Bar -->
        <div class="p-4 bg-(--surface-input) border-b border-default flex items-center justify-between">
          <div class="flex items-center gap-4">
            <div>
              <span class="text-[10px] font-bold text-muted uppercase tracking-widest block">Điểm số</span>
              <span :class="['text-2xl font-bold', (studentExamDetail?.score || selectedStudent.diem || 0) >= 5 ? 'text-(--color-success-text)' : 'text-(--color-danger-text)']">
                {{ (studentExamDetail?.score ?? selectedStudent.diem ?? 0).toFixed(1) }} / 10
              </span>
            </div>
            <div class="h-8 w-px bg-(--border-default)"></div>
            <div>
              <span class="text-[10px] font-bold text-muted uppercase tracking-widest block">Số câu đúng</span>
              <span class="text-sm font-semibold text-heading">
                {{ studentExamDetail?.soCauDung ?? selectedStudent.soCauDung }} / {{ studentExamDetail?.tongSoCau ?? selectedStudent.tongSoCau }} câu
              </span>
            </div>
          </div>

          <div :class="['px-3 py-1.5 rounded-xl text-xs font-bold uppercase tracking-wider border',
            (studentExamDetail?.score ?? selectedStudent.diem ?? 0) >= 5 
              ? 'bg-(--color-success-bg) text-(--color-success-text) border-(--color-success-text)/20' 
              : 'bg-(--color-danger-bg) text-(--color-danger-text) border-(--color-danger-text)/20']">
            {{ (studentExamDetail?.score ?? selectedStudent.diem ?? 0) >= 5 ? 'ĐẠT' : 'KHÔNG ĐẠT' }}
          </div>
        </div>

        <!-- Drawer Content Body -->
        <div class="flex-1 overflow-y-auto p-5 space-y-6 scrollbar-hide surface-solid">
          <div v-if="loadingStudentDetail" class="flex flex-col items-center justify-center py-16">
            <Loader2 :size="28" class="animate-spin text-muted mb-3" />
            <p class="text-xs font-semibold text-muted">Đang lấy đề thi & bài làm chi tiết...</p>
          </div>

          <div v-else-if="!studentExamDetail?.questions?.length" class="text-center py-12 text-muted">
            <HelpCircle :size="36" class="mx-auto mb-2 text-placeholder" />
            <p class="text-sm font-medium">Không tìm thấy chi tiết bài làm.</p>
          </div>

          <div v-else class="space-y-6">
            <!-- Render từng câu hỏi trong đề -->
            <div 
              v-for="(q, idx) in studentExamDetail.questions" 
              :key="q.maCauHoi"
              class="rounded-2xl surface-card border p-4 shadow-sm transition-all"
              :class="q.isCorrect ? 'border-(--color-success-text)/30' : (q.isUnanswered ? 'border-card' : 'border-(--color-danger-text)/30')"
            >
              <!-- Question Header -->
              <div class="flex items-start justify-between gap-3 mb-3">
                <div class="flex items-center gap-2">
                  <span class="text-xs font-bold px-2.5 py-1 rounded-lg bg-(--surface-input) text-heading border border-default">
                    Câu {{ q.thuTu || (idx + 1) }}
                  </span>
                  <span v-if="q.isCorrect" class="inline-flex items-center gap-1 text-xs font-bold text-(--color-success-text) bg-(--color-success-bg) px-2.5 py-1 rounded-lg border border-(--color-success-text)/20">
                    <CheckCircle :size="14" /> Đúng (+{{ q.diemToiDa }}đ)
                  </span>
                  <span v-else-if="q.isUnanswered" class="inline-flex items-center gap-1 text-xs font-bold text-muted bg-(--surface-input) px-2.5 py-1 rounded-lg border border-default">
                    <HelpCircle :size="14" /> Chưa trả lời (0đ)
                  </span>
                  <span v-else class="inline-flex items-center gap-1 text-xs font-bold text-(--color-danger-text) bg-(--color-danger-bg) px-2.5 py-1 rounded-lg border border-(--color-danger-text)/20">
                    <XCircle :size="14" /> Sai (0đ)
                  </span>
                </div>
                <span class="text-xs font-semibold text-muted">{{ q.diemToiDa }} điểm</span>
              </div>

              <!-- Question Content -->
              <p class="text-sm font-semibold text-heading mb-4 leading-relaxed">{{ q.noiDung }}</p>

              <!-- Option Choices List -->
              <div class="space-y-2">
                <div 
                  v-for="opt in q.options" 
                  :key="opt.key"
                  class="flex items-center justify-between p-3 rounded-xl border text-xs font-medium transition-all"
                  :class="[
                    // Case 1: Thí sinh chọn VÀ ĐÚNG -> Xanh lá đậm
                    q.dapAnHocSinh?.includes(opt.key) && q.dapAnDung?.includes(opt.key)
                      ? 'bg-emerald-500/10 border-emerald-500 text-emerald-700 font-semibold shadow-xs'
                      : '',
                    // Case 2: Thí sinh chọn VÀ SAI -> Đỏ đậm
                    q.dapAnHocSinh?.includes(opt.key) && !q.dapAnDung?.includes(opt.key)
                      ? 'bg-rose-500/10 border-rose-500 text-rose-700 font-semibold shadow-xs'
                      : '',
                    // Case 3: Thí sinh KHÔNG chọn nhưng đây là Đáp án đúng (khi câu bị sai) -> Viền xanh lá nét đứt
                    !q.isUnanswered && !q.dapAnHocSinh?.includes(opt.key) && q.dapAnDung?.includes(opt.key)
                      ? 'bg-emerald-500/5 border-emerald-500/60 border-dashed text-emerald-700 font-semibold'
                      : '',
                    // Case 4: Thí sinh BỎ TRỐNG và đây là Đáp án đúng -> Viền Xanh Lam / Cyan nhẹ (tránh nhầm lẫn với chọn)
                    q.isUnanswered && q.dapAnDung?.includes(opt.key)
                      ? 'bg-sky-500/10 border-sky-500 text-sky-700 font-medium border-dashed'
                      : '',
                    // Case 5: Các lựa chọn bình thường khác
                    !q.dapAnHocSinh?.includes(opt.key) && !q.dapAnDung?.includes(opt.key)
                      ? 'surface-input border-input text-body'
                      : ''
                  ]"
                >
                  <div class="flex items-center gap-3">
                    <span 
                      class="h-6 w-6 rounded-lg flex items-center justify-center text-xs font-bold shrink-0 border"
                      :class="[
                        q.dapAnHocSinh?.includes(opt.key) && q.dapAnDung?.includes(opt.key) ? 'bg-emerald-600 text-white border-emerald-600' :
                        q.dapAnHocSinh?.includes(opt.key) && !q.dapAnDung?.includes(opt.key) ? 'bg-rose-600 text-white border-rose-600' :
                        !q.isUnanswered && !q.dapAnHocSinh?.includes(opt.key) && q.dapAnDung?.includes(opt.key) ? 'bg-emerald-600 text-white border-emerald-600' :
                        q.isUnanswered && q.dapAnDung?.includes(opt.key) ? 'bg-sky-600 text-white border-sky-600' :
                        'surface-solid text-label border-default'
                      ]"
                    >
                      {{ opt.key }}
                    </span>
                    <span>{{ opt.text }}</span>
                  </div>

                  <!-- Badge nhãn giải thích lựa chọn -->
                  <div class="shrink-0 flex items-center gap-1.5">
                    <!-- Thí sinh chọn + Đúng -->
                    <span 
                      v-if="q.dapAnHocSinh?.includes(opt.key) && q.dapAnDung?.includes(opt.key)"
                      class="inline-flex items-center gap-1 text-[10px] font-bold uppercase tracking-wider text-emerald-700 bg-emerald-100 dark:bg-emerald-950/40 px-2 py-0.5 rounded-md"
                    >
                      <CheckCircle2 :size="12" /> Sinh viên chọn - Đúng
                    </span>

                    <!-- Thí sinh chọn + Sai -->
                    <span 
                      v-else-if="q.dapAnHocSinh?.includes(opt.key) && !q.dapAnDung?.includes(opt.key)"
                      class="inline-flex items-center gap-1 text-[10px] font-bold uppercase tracking-wider text-rose-700 bg-rose-100 dark:bg-rose-950/40 px-2 py-0.5 rounded-md"
                    >
                      <XCircle :size="12" /> Sinh viên chọn - Sai
                    </span>

                    <!-- Đáp án đúng của đề (khi thí sinh chọn câu khác) -->
                    <span 
                      v-else-if="!q.isUnanswered && !q.dapAnHocSinh?.includes(opt.key) && q.dapAnDung?.includes(opt.key)"
                      class="inline-flex items-center gap-1 text-[10px] font-bold uppercase tracking-wider text-emerald-700 bg-emerald-100 dark:bg-emerald-950/40 px-2 py-0.5 rounded-md"
                    >
                      <CheckCircle2 :size="12" /> Đáp án đúng của đề
                    </span>

                    <!-- Đáp án đúng của đề (khi thí sinh BỎ TRỐNG) -->
                    <span 
                      v-else-if="q.isUnanswered && q.dapAnDung?.includes(opt.key)"
                      class="inline-flex items-center gap-1 text-[10px] font-bold uppercase tracking-wider text-sky-700 bg-sky-100 dark:bg-sky-950/40 px-2 py-0.5 rounded-md"
                    >
                      🔑 Đáp án đúng của đề
                    </span>
                  </div>
                </div>
              </div>

              <!-- Banner nhắc nhở khi thí sinh không trả lời -->
              <div v-if="q.isUnanswered" class="mt-3 p-2.5 rounded-xl bg-amber-500/10 border border-amber-500/30 text-xs text-amber-700 dark:text-amber-400 font-medium flex items-center gap-2">
                🚫 <span class="font-semibold">Lưu ý:</span> Thí sinh không chọn đáp án nào cho câu hỏi này.
              </div>

              <!-- Gợi ý / Giải thích đáp án nếu có -->
              <div v-if="q.giaiThich" class="mt-3 p-3 rounded-xl bg-(--surface-input) border border-default text-xs text-muted">
                💡 <span class="font-semibold text-heading">Giải thích:</span> {{ q.giaiThich }}
              </div>
            </div>
          </div>
        </div>
      </template>
    </div>
  </Teleport>
</template>

<style scoped>
@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(15px); }
  to { opacity: 1; transform: translateY(0); }
}
.animate-fade-in-up {
  animation: fade-in-up 0.3s ease-out forwards;
}

.scrollbar-hide::-webkit-scrollbar {
    display: none;
}
.scrollbar-hide {
    -ms-overflow-style: none;
    scrollbar-width: none;
}
</style>

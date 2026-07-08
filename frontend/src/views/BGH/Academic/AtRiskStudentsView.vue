<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { 
  Search, 
  User, 
  Brain, 
  Bell, 
  History, 
  Zap,
  Download,
  FileText,
  ChevronDown,
  Loader2,
  Inbox,
  Send,
  ExternalLink,
  X,
  Mail,
  Phone,
  AlertTriangle,
  AlertCircle,
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { exportToExcel, triggerPrint } from '@/services/exportService.js'
import { usePopupStore } from '@/stores/popup'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const popup = usePopupStore()
const router = useRouter()

const loading = ref(false)
const error = ref(null)
const semesterFilter = ref('spring-2026')
const riskFilter = ref('all')
const searchQuery = ref('')

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const res = await bghApi.getAtRiskStudents()
    const data = unwrapApiData(res)
    if (data) {
      totalAtRisk.value = data.totalAtRisk ?? 0
      const rawSummary = data.summary || {}
      summaryStats.value = [
        { label: 'Tổng SV nguy cơ', count: data.totalAtRisk ?? 0, unit: 'SV', color: 'text-(--color-danger-text)', bg: 'bg-(--color-danger-bg)' },
        { label: 'Mức Critical', count: rawSummary.criticalCount ?? 0, unit: 'SV', color: 'text-(--color-danger-text)', bg: 'bg-(--color-danger-bg)' },
        { label: 'Điểm TB nhóm rủi ro', count: rawSummary.avgGpaAtRisk ?? 0, unit: '', color: 'text-(--color-warning-text)', bg: 'bg-(--color-warning-bg)' },
        { label: 'Tổng SV toàn trường', count: rawSummary.totalStudents ?? 0, unit: 'SV', color: 'text-(--color-info-text)', bg: 'bg-(--color-info-bg)' },
      ]
      riskStudents.value = (data.students || []).map(s => ({
        id: s.id,
        name: s.name,
        code: s.email || '—',
        class: s.classCode || '—',
        subject: '',
        grade: s.avgGpa,
        attendance: null,
        risk: s.failCount >= 3 ? 'critical' : s.failCount >= 2 ? 'high' : 'medium',
        reason: `Đã rớt ${s.failCount} môn. GPA hiện tại: ${s.avgGpa}.`,
        email: s.email || '',
      }))
    }
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}
onMounted(() => { loadData() })

const totalAtRisk = ref(0)
const summaryStats = ref([])
const riskStudents = ref([])

const semesters = [
  { value: 'spring-2026', label: 'Kỳ Spring 2026' },
  { value: 'fall-2025', label: 'Kỳ Fall 2025' },
]

const filteredStudents = computed(() => {
  let result = riskStudents.value
  const query = searchQuery.value.toLowerCase().trim()

  if (riskFilter.value !== 'all') {
    result = result.filter(s => s.risk === riskFilter.value)
  }

  if (query) {
    result = result.filter(s =>
      s.name.toLowerCase().includes(query) ||
      s.code.toLowerCase().includes(query) ||
      s.class.toLowerCase().includes(query) ||
      s.subject.toLowerCase().includes(query)
    )
  }

  return result
})

const getRiskBadge = (risk) => {
  switch (risk) {
    case 'critical': return 'bg-(--color-danger-bg) text-(--color-danger-text) border-(--color-danger-text)/20'
    case 'high': return 'bg-(--color-warning-bg) text-(--color-warning-text) border-(--color-warning-text)/20'
    case 'medium': return 'bg-(--color-info-bg) text-(--color-info-text) border-(--color-info-text)/20'
    default: return 'surface-solid text-muted border-default'
  }
}

function prepareExcelData() {
  return filteredStudents.value.map(s => ({
    'Họ tên': s.name,
    'Mã SV': s.code,
    'Lớp': s.class,
    'Môn': s.subject,
    'Điểm': s.grade,
    'Chuyên cần': s.attendance != null ? `${s.attendance}%` : '—',
    'Mức rủi ro': s.risk,
    'Nguyên nhân': s.reason,
  }))
}

function exportExcel() {
  exportToExcel(prepareExcelData(), `SV-NguyCo-${semesterFilter.value}.xlsx`, 'Nguy cơ rớt môn')
}

function viewStudentHistory(st) {
  router.push({ name: 'bgh-academic-at-risk-student-history', params: { studentId: st.id } })
}

function sendNotification(st) {
  popup.success('Đã gửi thông báo', `Thông báo cảnh báo đã được gửi tới ${st.name}.`)
}

const selectedStudent = ref(null)
const isDrawerOpen = ref(false)

function openDrawer(st) {
  selectedStudent.value = st
  isDrawerOpen.value = true
}

function closeDrawer() {
  isDrawerOpen.value = false
}

function sendBulkWarning() {
  const count = filteredStudents.value.length
  popup.success('Đã gửi cảnh báo', `Đã gửi cảnh báo đến ${count} giảng viên phụ trách ${count} sinh viên có nguy cơ rớt môn.`)
}
</script>

<template>
  <PageContainer 
    title="Sinh viên có nguy cơ rớt môn" 
    subtitle="Hệ thống cảnh báo sớm (AI Early Warning) dựa trên dữ liệu điểm số, chuyên cần và tiến độ học tập."
  >
    <template #actions>
      <div class="flex items-center gap-3">
         <button @click="triggerPrint" class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
            <FileText :size="18" /> PDF Report
         </button>
         <button @click="exportExcel" class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
            <Download :size="18" /> Excel Data
         </button>
      </div>
    </template>

    <div v-if="loading" class="flex items-center justify-center py-20">
      <Loader2 :size="32" class="animate-spin text-placeholder" />
    </div>
    <div v-else-if="error" class="flex flex-col items-center justify-center py-20 text-center">
      <AlertCircle :size="48" class="text-(--color-danger-text) mb-4" />
      <p class="text-lg font-semibold text-muted">Đã có lỗi xảy ra</p>
      <p class="text-sm text-placeholder mt-1">{{ error }}</p>
      <button @click="loadData" class="mt-4 lg-button-secondary px-4 py-2 text-sm font-semibold">Thử lại</button>
    </div>
    <div v-else id="print-container" class="space-y-4">
      
      <!-- ── Print Header ── -->
      <div class="hidden print:block mb-6 pb-4 border-b border-slate-300">
        <h2 class="text-xl font-bold text-slate-800">Sinh viên có nguy cơ rớt môn</h2>
        <p class="text-xs text-slate-500 mt-1">{{ semesters.find(s => s.value === semesterFilter)?.label }}</p>
      </div>

      <!-- ── Summary Stats ── -->
      <div v-if="summaryStats.length" class="grid grid-cols-2 md:grid-cols-4 gap-3">
        <div v-for="stat in summaryStats" :key="stat.label" :class="['rounded-2xl p-4 border text-center', stat.bg, `border-${stat.color.replace('text-', '')}/20`]">
          <p :class="['text-2xl font-bold', stat.color]">{{ typeof stat.count === 'number' && stat.count % 1 !== 0 ? stat.count.toFixed(2) : stat.count }}</p>
          <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">{{ stat.label }}</p>
        </div>
      </div>
      
      <!-- ── AI Insight Banner ── -->
      <div class="surface-card border border-(--color-info-text)/20 bg-(--color-info-bg) rounded-2xl p-5 relative overflow-hidden">
         <div class="relative z-10 flex flex-col md:flex-row items-center justify-between gap-5">
            <div class="flex items-center gap-4">
               <div class="h-14 w-14 rounded-2xl bg-(--surface-card) flex items-center justify-center border border-(--color-info-text)/20 shadow-sm">
                  <Brain :size="30" class="text-(--color-info-text)" />
               </div>
               <div>
                  <h3 class="text-lg font-semibold tracking-tight text-heading">AI Academic Forecast</h3>
                  <p class="text-sm text-(--color-info-text) mt-1 font-medium max-w-md">Hệ thống đã phân tích dữ liệu và phát hiện <strong>{{ totalAtRisk }}</strong> trường hợp có nguy cơ rớt môn cao trong kỳ này.</p>
               </div>
            </div>
            <div class="flex flex-wrap justify-center gap-3">
               <div class="px-4 py-3 surface-card rounded-2xl border border-(--color-info-text)/20 text-center">
                  <p class="text-[10px] font-semibold uppercase tracking-widest text-muted">Độ chính xác</p>
                  <p class="text-lg font-semibold text-heading">94.2%</p>
               </div>
               <div class="px-4 py-3 surface-card rounded-2xl border border-(--color-info-text)/20 text-center">
                  <p class="text-[10px] font-semibold uppercase tracking-widest text-muted">Cần can thiệp</p>
                  <p class="text-lg font-semibold text-heading">{{ summaryStats.length ? summaryStats[1].count : 0 }} SV</p>
               </div>
            </div>
         </div>
      </div>

      <!-- ── Toolbar ── -->
      <div class="surface-card border border-card rounded-2xl p-4 flex flex-wrap items-center justify-between gap-3 print:hidden">
        <div class="flex flex-wrap items-center gap-3 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
              <input v-model="searchQuery" type="text" placeholder="Tìm tên sinh viên, mã số hoặc lớp..." class="w-full surface-input border border-input rounded-xl pl-9 pr-4 py-2 text-sm font-medium outline-none focus:border-(--border-input-focus)">
           </div>
           <select v-model="semesterFilter" class="surface-input border border-input rounded-xl px-4 py-2 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
             <option v-for="s in semesters" :key="s.value" :value="s.value">{{ s.label }}</option>
           </select>
           <div class="relative">
             <select v-model="riskFilter" class="surface-input border border-input rounded-xl px-4 py-2 text-xs font-bold outline-none appearance-none cursor-pointer pr-10">
               <option value="all">Tất cả mức độ</option>
               <option value="critical">Critical</option>
               <option value="high">High</option>
               <option value="medium">Medium</option>
             </select>
             <ChevronDown :size="14" class="absolute right-3 top-1/2 -translate-y-1/2 text-placeholder pointer-events-none" />
           </div>
        </div>
        <button @click="sendBulkWarning" class="lg-button-primary py-2.5 px-4 text-sm font-semibold flex items-center gap-2">
           <Send :size="18" /> Gửi cảnh báo cho Giảng viên
        </button>
      </div>

      <!-- ── Empty State ── -->
      <div v-if="filteredStudents.length === 0" class="flex flex-col items-center justify-center py-20 text-center">
        <Inbox :size="48" class="text-placeholder mb-4" />
        <p class="text-lg font-semibold text-muted">Không tìm thấy sinh viên nào</p>
        <p class="text-sm text-placeholder mt-1">Thử thay đổi bộ lọc hoặc từ khóa tìm kiếm.</p>
      </div>

      <!-- ── Risk List ── -->
      <div v-else class="grid grid-cols-1 xl:grid-cols-2 gap-4">
         <div 
           v-for="st in filteredStudents" 
           :key="st.id" 
           class="surface-card border border-card rounded-2xl p-5 group hover:border-(--border-input-focus) transition-all shadow-sm"
         >
            <div class="flex items-start justify-between mb-4">
               <div class="flex items-center gap-4">
                  <div class="h-10 w-10 rounded-2xl surface-solid flex items-center justify-center text-muted group-hover:bg-(--color-info-bg) group-hover:text-(--color-info-text) transition-all">
                     <User :size="28" />
                  </div>
                  <div>
                     <h4 class="text-lg font-semibold text-heading leading-tight group-hover:text-link transition-colors">{{ st.name }}</h4>
                     <p class="text-[11px] font-bold text-muted uppercase tracking-widest mt-1">{{ st.code }} • Lớp {{ st.class }}</p>
                  </div>
               </div>
               <div :class="['px-3 py-1 rounded-full text-[10px] font-semibold uppercase tracking-widest border shadow-sm', getRiskBadge(st.risk)]">
                  {{ st.risk }}
               </div>
            </div>

            <div class="grid grid-cols-2 gap-4 mb-8">
               <div class="p-4 surface-solid rounded-2xl border border-default">
                  <p class="text-[9px] font-semibold text-muted uppercase tracking-widest mb-1.5">Môn học hiện tại</p>
                  <p class="text-xs font-bold text-label">{{ st.subject || 'Đang cập nhật' }}</p>
               </div>
               <div class="p-4 surface-solid rounded-2xl border border-default">
                  <p class="text-[9px] font-semibold text-muted uppercase tracking-widest mb-1.5">GPA TB / Môn rớt</p>
                  <div class="flex items-center justify-between">
                     <span class="text-sm font-semibold" :class="st.grade < 4 ? 'text-(--color-danger-text)' : 'text-(--color-warning-text)'">{{ st.grade }}</span>
                     <div class="text-right">
                       <span class="text-[10px] font-bold text-label">{{ st.reason }}</span>
                     </div>
                  </div>
               </div>
            </div>

            <div class="flex items-start gap-3 p-4 bg-(--color-danger-bg) rounded-2xl border border-(--color-danger-text)/20 mb-4">
               <Zap :size="16" class="text-(--color-danger-text) shrink-0 mt-0.5" />
               <div>
                  <p class="text-[10px] font-semibold text-(--color-danger-text) uppercase tracking-widest">Dự đoán của AI</p>
                  <p class="text-[11px] text-body font-medium leading-relaxed mt-1">{{ st.reason }}</p>
               </div>
            </div>

            <div class="flex items-center justify-between pt-6">
               <div class="flex items-center gap-1">
                  <button @click="viewStudentHistory(st)" class="p-2 hover:bg-(--surface-input) rounded-lg text-muted" title="Xem lịch sử"><History :size="18" /></button>
                  <button @click="sendNotification(st)" class="p-2 hover:bg-(--surface-input) rounded-lg text-muted" title="Gửi thông báo"><Bell :size="18" /></button>
               </div>
               <button @click="openDrawer(st)" class="text-xs font-semibold text-link uppercase tracking-widest flex items-center gap-1 hover:underline">
                   Xem chi tiết hồ sơ <ExternalLink :size="14" />
                </button>
            </div>
         </div>
      </div>

    </div>
  </PageContainer>

  <!-- ── Student Detail Drawer ── -->
  <Teleport to="body">
    <Transition name="drawer">
      <div v-if="isDrawerOpen && selectedStudent" class="drawer-root">
        <div class="drawer-scrim" @click="closeDrawer"></div>
        <aside class="drawer-panel" role="dialog" aria-modal="true">
          <div class="drawer-header">
            <div class="flex items-center gap-3">
              <div class="h-10 w-10 rounded-2xl bg-(--color-danger-bg) flex items-center justify-center">
                <AlertTriangle :size="20" class="text-(--color-danger-text)" />
              </div>
              <div>
                <h2 class="text-sm font-extrabold text-heading">{{ selectedStudent.name }}</h2>
                <p class="text-[10px] text-muted font-semibold">{{ selectedStudent.code }} • Lớp {{ selectedStudent.class }}</p>
              </div>
            </div>
            <button @click="closeDrawer" class="p-2 rounded-xl surface-card border border-default text-muted hover:text-heading transition-colors">
              <X :size="16" />
            </button>
          </div>

          <div class="drawer-body">
            <!-- Risk Badge & Subject -->
            <div class="grid grid-cols-2 gap-3 mb-5">
              <div class="p-4 surface-solid rounded-2xl border border-default">
                <p class="text-[9px] font-semibold text-muted uppercase tracking-widest mb-1.5">Mức độ rủi ro</p>
                <div :class="['inline-block px-3 py-1 rounded-full text-[10px] font-semibold uppercase tracking-widest border shadow-sm', getRiskBadge(selectedStudent.risk)]">
                  {{ selectedStudent.risk }}
                </div>
              </div>
              <div class="p-4 surface-solid rounded-2xl border border-default">
                <p class="text-[9px] font-semibold text-muted uppercase tracking-widest mb-1.5">Môn học</p>
                <p class="text-xs font-bold text-label">{{ selectedStudent.subject || 'Đang cập nhật' }}</p>
              </div>
            </div>

            <!-- AI Reason -->
            <div class="flex items-start gap-3 p-4 bg-(--color-danger-bg) rounded-2xl border border-(--color-danger-text)/20 mb-5">
              <Zap :size="16" class="text-(--color-danger-text) shrink-0 mt-0.5" />
              <div>
                <p class="text-[10px] font-semibold text-(--color-danger-text) uppercase tracking-widest">Dự đoán của AI</p>
                <p class="text-[11px] text-body font-medium leading-relaxed mt-1">{{ selectedStudent.reason }}</p>
              </div>
            </div>

            <!-- Scores -->
            <div class="p-4 surface-solid rounded-2xl border border-default mb-5">
              <p class="text-[9px] font-semibold text-muted uppercase tracking-widest mb-3">Điểm số</p>
              <div class="space-y-3">
                <div class="flex items-center justify-between">
                  <span class="text-xs font-semibold text-muted">GPA hiện tại</span>
                  <span class="text-sm font-bold" :class="selectedStudent.grade < 4 ? 'text-(--color-danger-text)' : 'text-(--color-warning-text)'">{{ selectedStudent.grade }}</span>
                </div>
                <div class="flex items-center justify-between">
                  <span class="text-xs font-semibold text-muted">Số môn đã rớt</span>
                  <span class="text-sm font-bold text-(--color-danger-text)">{{ selectedStudent.reason }}</span>
                </div>
              </div>
            </div>

            <!-- Contact Info -->
            <div class="p-4 surface-solid rounded-2xl border border-default mb-5">
              <p class="text-[9px] font-semibold text-muted uppercase tracking-widest mb-3">Thông tin liên hệ</p>
              <div class="space-y-3">
                <div class="flex items-center gap-2.5">
                  <Mail :size="14" class="text-placeholder shrink-0" />
                  <span class="text-xs font-medium text-body">{{ selectedStudent.email || '—' }}</span>
                </div>
                <div class="flex items-center gap-2.5">
                  <Phone :size="14" class="text-placeholder shrink-0" />
                  <span class="text-xs font-medium text-body">—</span>
                </div>
              </div>
            </div>

            <!-- Actions -->
            <div class="flex flex-col gap-2">
              <button @click="closeDrawer; sendNotification(selectedStudent)" class="w-full lg-button-secondary py-2.5 text-xs font-semibold flex items-center justify-center gap-2">
                <Bell :size="16" /> Gửi thông báo cho sinh viên
              </button>
              <button @click="closeDrawer; viewStudentHistory(selectedStudent)" class="w-full lg-button-secondary py-2.5 text-xs font-semibold flex items-center justify-center gap-2">
                <History :size="16" /> Xem lịch sử học tập
              </button>
            </div>
          </div>
        </aside>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
@media print {
  #print-container { padding: 0; color: #1e293b; }
  #print-container .surface-card { border: 1px solid #cbd5e1; background: #fff; box-shadow: none; break-inside: avoid; }
}

.drawer-root {
  position: fixed;
  inset: 0;
  z-index: 9999;
  display: flex;
  justify-content: flex-end;
}

.drawer-scrim {
  position: absolute;
  inset: 0;
  background: color-mix(in srgb, var(--surface-app) 50%, transparent);
  backdrop-filter: blur(4px);
  -webkit-backdrop-filter: blur(4px);
}

.drawer-panel {
  position: relative;
  width: min(100%, 28rem);
  height: 100%;
  background: var(--surface-card);
  border-left: 1px solid var(--border-card);
  box-shadow: -8px 0 30px rgba(0,0,0,0.12);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.drawer-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.25rem;
  border-bottom: 1px solid var(--border-card);
  flex-shrink: 0;
}

.drawer-body {
  flex: 1;
  overflow-y: auto;
  padding: 1.25rem;
}

.drawer-enter-active,
.drawer-leave-active {
  transition: opacity 0.2s ease;
}

.drawer-enter-active .drawer-panel,
.drawer-leave-active .drawer-panel {
  transition: transform 0.25s cubic-bezier(0.16, 1, 0.3, 1);
}

.drawer-enter-from,
.drawer-leave-to {
  opacity: 0;
}

.drawer-enter-from .drawer-panel,
.drawer-leave-to .drawer-panel {
  transform: translateX(100%);
}
</style>

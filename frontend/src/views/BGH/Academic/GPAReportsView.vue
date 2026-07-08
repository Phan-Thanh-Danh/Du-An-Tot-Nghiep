<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  Award, 
  Search, 
  Download, 
  TrendingUp, 
  ChevronRight, 
  Users, 
  Building2, 
  MapPin, 
  FileText, 
  ArrowUpRight,
  Target,
  X,
  Eye,
  BarChart3,
  AlertCircle,
  Loader2,
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { exportToExcel, triggerPrint } from '@/services/exportService.js'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const loading = ref(false)
const error = ref(null)
const semesterFilter = ref('spring-2026')
const departmentFilter = ref('all')
const searchQuery = ref('')
const sortBy = ref('gpa-desc')

const semesters = [
  { value: 'spring-2026', label: 'Kỳ Spring 2026' },
  { value: 'fall-2025', label: 'Kỳ Fall 2025' },
  { value: 'spring-2025', label: 'Kỳ Spring 2025' },
]

const departments = [
  { value: 'all', label: 'Tất cả Khoa' },
  { value: 'cntt', label: 'Khoa CNTT' },
  { value: 'ktqt', label: 'Khoa Kinh tế & QT' },
  { value: 'nna', label: 'Khoa Ngôn ngữ Anh' },
]

const gpaStats = ref([])
const distribution = ref([])

const selectedStat = ref(null)

function openDetail(stat) {
  selectedStat.value = stat
}

function closeDetail() {
  selectedStat.value = null
}

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const res = await bghApi.getGpaReports()
    const data = unwrapApiData(res)
    if (data) {
      gpaStats.value = (data.trends || []).map(t => ({
        id: t.semester,
        group: t.semester,
        avgGpa: t.avgGpa,
        maxGpa: t.avgGpa,
        minGpa: t.avgGpa,
        warningCount: 0,
        campus: '',
        studentCount: t.studentCount || 0,
      }))
      distribution.value = (data.distribution || []).map(d => ({
        range: d.grade,
        count: d.count,
        percent: d.percent,
      }))
    }
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}
onMounted(() => { loadData() })

const getGpaColor = (gpa) => {
  if (gpa >= 3.2) return 'text-(--color-success-text)'
  if (gpa >= 2.5) return 'text-(--color-info-text)'
  return 'text-(--color-danger-text)'
}

const filteredStats = computed(() => {
  let result = [...gpaStats.value]

  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    result = result.filter(s => s.group.toLowerCase().includes(q) || s.campus.toLowerCase().includes(q))
  }

  if (departmentFilter.value !== 'all') {
    const deptMap = { cntt: 'Công nghệ thông tin', ktqt: 'Kinh tế', nna: 'Ngôn ngữ Anh' }
    const dept = deptMap[departmentFilter.value]
    result = result.filter(s => dept ? s.group.includes(dept) : true)
  }

  if (sortBy.value === 'gpa-desc') result.sort((a, b) => b.avgGpa - a.avgGpa)
  else if (sortBy.value === 'gpa-asc') result.sort((a, b) => a.avgGpa - b.avgGpa)
  else if (sortBy.value === 'warning') result.sort((a, b) => b.warningCount - a.warningCount)

  return result
})

const overallAvgGpa = computed(() => {
  if (!gpaStats.value.length) return 0
  const sum = gpaStats.value.reduce((s, g) => s + g.avgGpa, 0)
  return (sum / gpaStats.value.length).toFixed(2)
})

const highGpaRate = computed(() => {
  if (!gpaStats.value.length) return '0'
  const high = gpaStats.value.filter(g => g.avgGpa >= 3.2).length
  return ((high / gpaStats.value.length) * 100).toFixed(1)
})

const maxGpaValue = computed(() => {
  if (!gpaStats.value.length) return '0.00'
  return Math.max(...gpaStats.value.map(g => g.avgGpa)).toFixed(2)
})

function prepareExcelData() {
  return filteredStats.value.map(s => ({ 'Khoa / Lớp': s.group, 'GPA TB': s.avgGpa, 'Min GPA': s.minGpa, 'Max GPA': s.maxGpa, 'SV dưới ngưỡng': s.warningCount, 'Cơ sở': s.campus }))
}

function exportExcel() {
  exportToExcel(prepareExcelData(), `BaoCao-GPA-${semesterFilter.value}.xlsx`, 'GPA')
}
</script>

<template>
  <PageContainer 
    title="Báo cáo GPA hệ thống" 
    subtitle="Phân tích điểm trung bình tích lũy theo từng khoa, cơ sở và lớp học để đánh giá chất lượng sinh viên."
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
        <h2 class="text-xl font-bold text-slate-800">Báo cáo GPA hệ thống</h2>
        <p class="text-xs text-slate-500 mt-1">{{ semesters.find(s => s.value === semesterFilter)?.label }}</p>
      </div>

      <!-- ── Filters ── -->
      <div class="surface-card border border-card p-4 rounded-2xl flex flex-wrap items-center justify-between gap-4 print:hidden">
        <div class="flex items-center gap-4 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
              <input v-model="searchQuery" type="text" placeholder="Tìm khoa, ngành hoặc lớp..." class="w-full surface-input border border-input rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
           </div>
           <select v-model="semesterFilter" class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
             <option v-for="s in semesters" :key="s.value" :value="s.value">{{ s.label }}</option>
           </select>
           <select v-model="departmentFilter" class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
             <option v-for="d in departments" :key="d.value" :value="d.value">{{ d.label }}</option>
           </select>
        </div>
        <div class="flex items-center gap-2">
           <span class="text-[10px] font-semibold text-muted uppercase tracking-widest mr-2">Sắp xếp theo</span>
           <select v-model="sortBy" class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none">
              <option value="gpa-desc">GPA Trung bình (Cao - Thấp)</option>
              <option value="gpa-asc">GPA Trung bình (Thấp - Cao)</option>
              <option value="warning">Số lượng SV cảnh báo</option>
           </select>
        </div>
      </div>

      <!-- ── KPI Mini Grid ── -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
         <div class="surface-card border border-card rounded-2xl p-4 flex items-center gap-5">
            <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) flex items-center justify-center text-(--color-info-text) shadow-sm border border-(--color-info-text)/20">
               <Target :size="24" />
            </div>
            <div>
               <p class="text-[10px] font-semibold text-(--color-info-text) uppercase tracking-widest">GPA TB toàn hệ thống</p>
               <h3 class="text-xl font-semibold text-heading leading-tight">{{ overallAvgGpa }}</h3>
            </div>
         </div>
         <div class="surface-card border border-card rounded-2xl p-4 flex items-center gap-5">
            <div class="h-10 w-10 rounded-2xl bg-(--color-success-bg) flex items-center justify-center text-(--color-success-text) shadow-sm border border-(--color-success-text)/20">
               <TrendingUp :size="24" />
            </div>
            <div>
               <p class="text-[10px] font-semibold text-(--color-success-text) uppercase tracking-widest">Tỷ lệ GPA >= 3.2</p>
               <h3 class="text-xl font-semibold text-heading leading-tight">{{ highGpaRate }}%</h3>
            </div>
         </div>
         <div class="surface-card border border-card rounded-2xl p-4 flex items-center gap-5">
            <div class="h-10 w-10 rounded-2xl bg-(--color-warning-bg) flex items-center justify-center text-(--color-warning-text) shadow-sm border border-(--color-warning-text)/20">
               <Award :size="24" />
            </div>
            <div>
               <p class="text-[10px] font-semibold text-(--color-warning-text) uppercase tracking-widest">GPA cao nhất</p>
               <h3 class="text-xl font-semibold text-heading leading-tight">{{ maxGpaValue }}</h3>
            </div>
         </div>
      </div>

      <!-- ── GPA Distribution Bar Chart ── -->
      <div v-if="distribution.length" class="surface-card border border-card rounded-2xl p-5">
        <h4 class="text-sm font-semibold text-heading uppercase tracking-wide mb-6">Phân bố GPA</h4>
        <div class="space-y-4">
          <div v-for="item in distribution" :key="item.range">
            <div class="flex items-center justify-between mb-2">
              <span class="text-xs font-semibold text-label">{{ item.range }}</span>
              <span class="text-xs font-semibold text-heading">{{ item.count }} SV ({{ item.percent }}%)</span>
            </div>
            <div class="h-2.5 w-full bg-(--surface-input) rounded-full overflow-hidden">
              <div :style="{ width: `${item.percent}%` }" class="h-full rounded-full transition-all duration-1000" :class="item.percent >= 30 ? 'bg-(--color-success-text)' : item.percent >= 15 ? 'bg-(--color-info-text)' : 'bg-(--color-warning-text)'"></div>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Empty State ── -->
      <div v-if="filteredStats.length === 0" class="text-center py-12 surface-card border border-card rounded-2xl">
        <Search :size="40" class="mx-auto text-placeholder mb-3" />
        <p class="text-sm font-semibold text-muted">Không tìm thấy kết quả phù hợp</p>
        <p class="text-xs text-placeholder mt-1">Thử thay đổi bộ lọc hoặc từ khóa tìm kiếm</p>
      </div>

      <!-- ── Data Table ── -->
      <div v-else class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Kỳ học / Nhóm</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest">GPA Trung bình</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Min / Max GPA</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Số SV</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="stat in filteredStats" :key="stat.id" class="group hover:bg-(--surface-input) transition-colors">
              <td class="px-4 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-xl surface-solid flex items-center justify-center text-placeholder group-hover:text-link transition-all">
                    <Building2 :size="18" />
                  </div>
                  <div>
                    <p class="text-sm font-semibold text-heading leading-tight">{{ stat.group }}</p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-2">
                   <h3 :class="['text-lg font-semibold', getGpaColor(stat.avgGpa)]">{{ stat.avgGpa.toFixed(2) }}</h3>
                   <ArrowUpRight :size="14" class="text-placeholder" />
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-4">
                   <div class="text-center">
                      <p class="text-[9px] font-semibold text-muted uppercase">Min</p>
                      <p class="text-xs font-bold text-heading">{{ stat.minGpa.toFixed(2) }}</p>
                   </div>
                   <div class="h-6 w-px bg-(--border-default)"></div>
                   <div class="text-center">
                      <p class="text-[9px] font-semibold text-(--color-success-text) uppercase">Max</p>
                      <p class="text-xs font-semibold text-(--color-success-text)">{{ stat.maxGpa.toFixed(2) }}</p>
                   </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="surface-solid text-muted border-default px-2.5 py-1 rounded-lg text-[10px] font-semibold uppercase tracking-widest border w-fit shadow-sm">
                  {{ stat.studentCount || '—' }}
                </div>
              </td>
              <td class="px-4 py-4 text-right">
                <button @click="openDetail(stat)" class="p-2 hover:bg-(--color-info-bg) hover:text-link rounded-lg text-placeholder transition-all">
                  <Eye :size="18" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- ── Detail Panel ── -->
      <Teleport to="body">
        <div v-if="selectedStat" class="fixed inset-0 z-50 flex justify-end">
          <div class="absolute inset-0 bg-black/30" @click="closeDetail"></div>
          <div class="relative w-full max-w-lg surface-card h-full overflow-y-auto shadow-2xl animate-slide-in">
            <div class="sticky top-0 bg-(--surface-card) px-6 py-4 flex items-center justify-between z-10">
              <div>
                <h3 class="text-lg font-bold text-heading">Chi tiết GPA</h3>
                <p class="text-xs text-muted mt-0.5">{{ selectedStat.group }}</p>
              </div>
              <button @click="closeDetail" class="p-2 hover:bg-(--surface-input) rounded-lg text-muted transition-colors">
                <X :size="20" />
              </button>
            </div>
            <div class="p-6 space-y-6">
              <div class="grid grid-cols-2 gap-4">
                <div class="surface-solid rounded-2xl p-4 text-center border border-default">
                  <p class="text-[9px] font-semibold text-muted uppercase tracking-widest">GPA TB</p>
                  <p :class="['text-2xl font-bold mt-1', getGpaColor(selectedStat.avgGpa)]">{{ selectedStat.avgGpa.toFixed(2) }}</p>
                </div>
                <div class="surface-solid rounded-2xl p-4 text-center border border-default">
                  <p class="text-[9px] font-semibold text-muted uppercase tracking-widest">Số SV</p>
                  <p class="text-sm font-bold text-heading mt-1">{{ selectedStat.studentCount || '—' }}</p>
                </div>
                <div class="surface-solid rounded-2xl p-4 text-center border border-default">
                  <p class="text-[9px] font-semibold text-muted uppercase tracking-widest">GPA Cao nhất</p>
                  <p class="text-lg font-bold text-(--color-success-text) mt-1">{{ selectedStat.maxGpa.toFixed(2) }}</p>
                </div>
                <div class="surface-solid rounded-2xl p-4 text-center border border-default">
                  <p class="text-[9px] font-semibold text-muted uppercase tracking-widest">GPA Thấp nhất</p>
                  <p class="text-lg font-bold text-(--color-danger-text) mt-1">{{ selectedStat.minGpa.toFixed(2) }}</p>
                </div>
              </div>

              <div v-if="distribution.length" class="surface-solid rounded-2xl p-5 border border-default">
                <h4 class="text-xs font-semibold text-muted uppercase tracking-widest mb-4 flex items-center gap-2">
                  <BarChart3 :size="16" /> Phân bố điểm
                </h4>
                <div class="space-y-3">
                  <div v-for="item in distribution" :key="item.range">
                    <div class="flex justify-between text-[11px] mb-1">
                      <span class="font-semibold text-label">{{ item.range }}</span>
                      <span class="font-semibold text-heading">{{ item.count }} SV</span>
                    </div>
                    <div class="h-2 bg-(--surface-input) rounded-full overflow-hidden">
                      <div :style="{ width: `${item.percent}%` }" class="h-full rounded-full" :class="item.percent >= 30 ? 'bg-(--color-success-text)' : item.percent >= 15 ? 'bg-(--color-info-text)' : 'bg-(--color-warning-text)'"></div>
                    </div>
                  </div>
                </div>
              </div>

              <div class="bg-(--color-danger-bg) border border-(--color-danger-text)/20 rounded-2xl p-4 flex items-start gap-3">
                <Users :size="18" class="text-(--color-danger-text) shrink-0 mt-0.5" />
                <div>
                  <p class="text-[10px] font-semibold text-(--color-danger-text) uppercase tracking-widest">Cảnh báo</p>
                  <p class="text-xs text-body mt-1 font-medium">Dữ liệu GPA được tổng hợp từ hệ thống điểm số. Sử dụng bộ lọc để xem chi tiết theo kỳ và khoa.</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </Teleport>

    </div>
  </PageContainer>
</template>

<style scoped>
@keyframes slide-in {
  from { transform: translateX(100%); }
  to { transform: translateX(0); }
}
.animate-slide-in {
  animation: slide-in 0.2s ease-out;
}

@media print {
  #print-container { padding: 0; color: #1e293b; }
  #print-container .surface-card { border: 1px solid #cbd5e1; background: #fff; box-shadow: none; break-inside: avoid; }
  #print-container table { font-size: 10px; }
  #print-container th { background: #f1f5f9; color: #475569; }
}
</style>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { 
  Award, 
  Search, 
  Download, 
  TrendingUp, 
  Users, 
  Building2, 
  FileText, 
  ArrowUpRight,
  Target,
  X,
  Eye,
  BarChart3,
  AlertCircle,
  Loader2,
  Sparkles,
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import BghAiReportModal from '@/components/BGH/BghAiReportModal.vue'
import { exportBghToExcel } from '@/components/BGH/performance/bghExport.js'
import { bghApi } from '@/services/bghApi'
import { aiApi } from '@/services/aiApi'
import { unwrapApiData } from '@/services/apiClient'

const loading = ref(false)
const error = ref(null)
const semesterFilter = ref('all')
const industryFilter = ref('all')
const majorFilter = ref('all')
const campusFilter = ref('all')
const searchQuery = ref('')
const sortBy = ref('gpa-desc')

const semesters = ref([{ value: 'all', label: 'Tất cả học kỳ' }])
const industries = ref([{ value: 'all', label: 'Tất cả Ngành' }])
const specializations = ref([])
const campuses = ref([{ value: 'all', label: 'Tất cả Cơ sở' }])

const availableMajors = computed(() => {
  if (industryFilter.value === 'all') {
    return [{ value: 'all', label: 'Tất cả Chuyên ngành' }, ...specializations.value]
  }
  const filtered = specializations.value.filter(s => String(s.majorId || s.maNganh) === String(industryFilter.value))
  return [{ value: 'all', label: 'Tất cả Chuyên ngành' }, ...(filtered.length > 0 ? filtered : specializations.value)]
})

const gpaStats = ref([])
const distribution = ref([])

const selectedStat = ref(null)

function openDetail(stat) {
  selectedStat.value = stat
}

function closeDetail() {
  selectedStat.value = null
}

async function loadData(isInitial = false) {
  if (isInitial) loading.value = true
  error.value = null
  try {
    const params = {}
    if (campusFilter.value !== 'all') params.campusId = campusFilter.value
    if (semesterFilter.value !== 'all') params.semesterId = semesterFilter.value
    if (majorFilter.value !== 'all') {
      params.specializationId = majorFilter.value
    } else if (industryFilter.value !== 'all') {
      params.majorId = industryFilter.value
    }

    const [res, orgRes, filterOptionsRes] = await Promise.all([
      bghApi.getGpaReports(params).catch(() => null),
      bghApi.getOrganizations().catch(() => null),
      bghApi.getPassFailFilterOptions().catch(() => null),
    ])
    const data = unwrapApiData(res) || {}
    const orgs = unwrapApiData(orgRes) || []
    const filterOptions = unwrapApiData(filterOptionsRes) || {}

    if (orgs.length > 0) {
      campuses.value = [
        { value: 'all', label: 'Tất cả Cơ sở' },
        ...orgs.map(o => ({ value: String(o.id || o.maDonVi), label: o.name || o.tenDonVi || 'Cơ sở' })),
      ]
    }

    if (filterOptions.majors && filterOptions.majors.length > 0) {
      industries.value = [
        { value: 'all', label: 'Tất cả Ngành' },
        ...filterOptions.majors.map(m => ({
          value: String(m.id || m.maNganh),
          label: m.label || m.name || m.tenNganh || m.code || 'Ngành'
        }))
      ]
    }

    if (filterOptions.specializations && filterOptions.specializations.length > 0) {
      specializations.value = filterOptions.specializations.map(s => ({
        value: String(s.id || s.maChuyenNganh),
        label: s.label || s.name || s.tenChuyenNganh || 'Chuyên ngành',
        majorId: s.majorId || s.maNganh
      }))
    }

    gpaStats.value = (data.trends || []).map(t => ({
      id: t.semester || 'Học kỳ',
      group: t.semester || 'Học kỳ',
      avgGpa: Number(t.avgGpa) || 0,
      maxGpa: Number(t.avgGpa) || 0,
      minGpa: Number(t.avgGpa) || 0,
      warningCount: 0,
      campus: '',
      studentCount: t.studentCount || 0,
    }))
    distribution.value = (data.distribution || []).map(d => ({
      range: d.grade || '—',
      count: d.count ?? 0,
      percent: d.percent ?? 0,
    }))
    if (semesters.value.length <= 1 && gpaStats.value.length > 0) {
      semesters.value = [
        { value: 'all', label: 'Tất cả học kỳ' },
        ...gpaStats.value.map(item => ({ value: item.id, label: item.group })),
      ]
    }
  } catch {
    error.value = null
  } finally {
    if (isInitial) loading.value = false
  }
}

watch(industryFilter, () => {
  majorFilter.value = 'all'
})

// AI Strategic Report State
const aiModalOpen = ref(false)
const aiLoading = ref(false)
const aiError = ref(null)
const aiReport = ref(null)

const aiScopeBadges = computed(() => {
  const list = []
  const sem = semesters.value.find(s => s.value === semesterFilter.value)?.label || 'Tất cả học kỳ'
  list.push(`Học kỳ: ${sem}`)
  const ind = industries.value.find(i => i.value === industryFilter.value)?.label || 'Tất cả Ngành'
  list.push(`Ngành: ${ind}`)
  if (majorFilter.value !== 'all') {
    const maj = availableMajors.value.find(m => m.value === majorFilter.value)?.label
    if (maj) list.push(`Chuyên ngành: ${maj}`)
  }
  const cam = campuses.value.find(c => c.value === campusFilter.value)?.label || 'Tất cả cơ sở'
  list.push(`Cơ sở: ${cam}`)
  return list
})

async function triggerAiAnalysis() {
  aiModalOpen.value = true
  aiLoading.value = true
  aiError.value = null
  try {
    await loadData(false)
    const semId = semesterFilter.value !== 'all' ? parseInt(semesterFilter.value) : undefined
    const camId = campusFilter.value !== 'all' ? parseInt(campusFilter.value) : undefined
    const majId = industryFilter.value !== 'all' ? parseInt(industryFilter.value) : undefined
    const specId = majorFilter.value !== 'all' ? parseInt(majorFilter.value) : undefined
    
    // Lưu ý: sortBy chỉ dùng để sắp xếp UI hiển thị, KHÔNG gửi cho AI phân tích
    const res = await aiApi.generateBghReport({
      reportType: 'gpa',
      semesterId: isNaN(semId) ? undefined : semId,
      campusId: isNaN(camId) ? undefined : camId,
      majorId: isNaN(majId) ? undefined : majId,
      specializationId: isNaN(specId) ? undefined : specId,
      mode: 'deep',
      forceRefresh: true,
    })
    aiReport.value = res
  } catch (err) {
    aiError.value = err.message || 'Không thể tạo báo cáo GPA AI.'
  } finally {
    aiLoading.value = false
  }
}

onMounted(() => { loadData(true) })

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
  exportBghToExcel(prepareExcelData(), `BaoCao-GPA-${semesterFilter.value}.xlsx`, 'GPA')
}

const exportingPdf = ref(false)

async function exportPdf() {
  if (exportingPdf.value) return
  exportingPdf.value = true
  try {
    const { exportGpaReportToPdf } = await import('@/components/BGH/performance/bghExport.js')
    await exportGpaReportToPdf({
      filteredStats: filteredStats.value,
      overallAvgGpa: overallAvgGpa.value,
      highGpaRate: highGpaRate.value,
      maxGpaValue: maxGpaValue.value,
      semesterLabel: semesters.value.find(s => s.value === semesterFilter.value)?.label || 'Tất cả học kỳ',
      industryLabel: industries.value.find(i => i.value === industryFilter.value)?.label || 'Tất cả Ngành',
      campusLabel: campuses.value.find(c => c.value === campusFilter.value)?.label || 'Tất cả Cơ sở',
    })
  } finally {
    exportingPdf.value = false
  }
}
</script>

<template>
  <PageContainer 
    title="Báo cáo GPA hệ thống" 
    subtitle="Phân tích điểm trung bình tích lũy theo từng khoa, cơ sở và lớp học để đánh giá chất lượng sinh viên."
  >
    <template #actions>
      <div class="flex items-center gap-3">
         <button @click="exportPdf" :disabled="exportingPdf" class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2 disabled:opacity-50">
            <Loader2 v-if="exportingPdf" :size="18" class="animate-spin" />
            <FileText v-else :size="18" /> {{ exportingPdf ? 'Đang xuất...' : 'PDF Report' }}
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
        <div class="flex items-center gap-3 flex-1 flex-wrap">
           <div class="relative max-w-xs w-full">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
              <input v-model="searchQuery" type="text" placeholder="Tìm khoa, ngành hoặc lớp..." class="w-full surface-input border border-input rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
           </div>
           <LmsSelect v-model="semesterFilter" :options="semesters" class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)" />
           <LmsSelect v-model="industryFilter" :options="industries" class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)" />
           <LmsSelect v-model="majorFilter" :options="availableMajors" class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)" />
           <LmsSelect v-model="campusFilter" :options="campuses" class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)" />
        </div>
        <div class="flex items-center gap-3">
           <div class="flex items-center gap-2">
              <span class="text-[10px] font-semibold text-muted uppercase tracking-widest mr-2">Sắp xếp theo</span>
              <LmsSelect v-model="sortBy" class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none">
                 <option value="gpa-desc">GPA Trung bình (Cao - Thấp)</option>
                 <option value="gpa-asc">GPA Trung bình (Thấp - Cao)</option>
                 <option value="warning">Số lượng SV cảnh báo</option>
              </LmsSelect>
           </div>
           <button
             @click="triggerAiAnalysis"
             :disabled="aiLoading"
             class="px-5 py-2.5 rounded-xl bg-gradient-to-r from-blue-600 via-indigo-600 to-indigo-700 hover:from-blue-700 hover:to-indigo-800 text-white text-xs font-bold shadow-md shadow-indigo-500/20 flex items-center gap-2 transition-all active:scale-95 disabled:opacity-60 cursor-pointer shrink-0"
           >
             <Sparkles v-if="!aiLoading" :size="15" />
             <Loader2 v-else :size="15" class="animate-spin" />
             <span>{{ aiLoading ? 'ĐANG PHÂN TÍCH...' : 'PHÂN TÍCH BẰNG AI' }}</span>
           </button>
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

      <!-- ── Visual GPA Comparison Chart ── -->
      <div v-if="gpaStats.length" class="surface-card border border-card rounded-2xl p-5">
        <div class="flex items-center justify-between mb-6">
          <div>
            <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Biểu đồ so sánh GPA giữa các Học kỳ</h4>
            <p class="text-xs text-muted mt-0.5 font-bold">Điểm trung bình tích lũy quy đổi thang điểm 10.0</p>
          </div>
          <span class="text-[10px] font-extrabold uppercase px-2.5 py-1 rounded-lg bg-(--color-info-bg) text-(--color-info-text) border border-(--color-info-text)/20">
             Biểu đồ cột
          </span>
        </div>

        <div class="h-56 flex items-end gap-3 pl-12 pr-4 pt-6 pb-2 relative">
          <!-- Trục tung (Y-Axis) Mốc điểm 0.0 - 10.0 -->
          <div class="absolute left-2 top-6 bottom-8 flex flex-col justify-between items-end text-[10px] font-extrabold text-muted pr-2 pointer-events-none select-none">
             <span>10.0</span>
             <span>7.5</span>
             <span>5.0</span>
             <span>2.5</span>
             <span>0.0</span>
          </div>

          <!-- Đường lưới ngang -->
          <div class="absolute left-10 right-4 top-6 bottom-8 flex flex-col justify-between pointer-events-none opacity-15">
            <div v-for="i in 5" :key="i" class="h-px w-full bg-(--border-default) border-t border-dashed"></div>
          </div>
          <div v-for="stat in gpaStats" :key="stat.id" class="flex-1 group relative flex flex-col items-center justify-end h-full z-10">
            <span class="text-[11px] font-extrabold text-heading mb-1 transition-transform group-hover:scale-110">
              {{ (Number(stat.avgGpa) || 0).toFixed(2) }}
            </span>
            <div class="w-full flex justify-center items-end h-36">
              <div 
                :style="{ height: `${Math.min((stat.avgGpa / 10.0) * 100, 100)}%` }" 
                class="w-8 max-w-[40px] rounded-t-xl transition-all duration-700 bg-gradient-to-t from-cyan-600 via-blue-600 to-indigo-500 shadow-md group-hover:shadow-indigo-500/50 group-hover:brightness-110"
              ></div>
            </div>
            <p class="text-center text-[10px] font-bold text-muted uppercase tracking-wider mt-2 truncate max-w-full">
              {{ stat.group.replace('Học kỳ ', 'Kỳ ').trim() }}
            </p>
          </div>
        </div>
      </div>

      <!-- ── GPA Distribution Bar Chart ── -->
      <div v-if="distribution.length" class="surface-card border border-card rounded-2xl p-5">
        <h4 class="text-sm font-semibold text-heading uppercase tracking-wide mb-6">Phân bố GPA các nhóm điểm</h4>
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

    <!-- AI Strategic Report Modal -->
    <BghAiReportModal
      :is-open="aiModalOpen"
      title="Báo Cáo Phân Tích GPA AI"
      subtitle="Đánh giá phổ điểm, phân tích độ lệch chuẩn GPA và xếp hạng năng lực học thuật"
      :scope-badges="aiScopeBadges"
      :loading="aiLoading"
      :error="aiError"
      :report-content="aiReport?.aiAnalysis"
      :generated-at="aiReport?.generatedAt"
      @close="aiModalOpen = false"
      @retry="triggerAiAnalysis"
    />
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

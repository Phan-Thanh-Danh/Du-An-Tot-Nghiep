<script setup>
import { ref, computed, nextTick, onMounted } from 'vue'
import { 
  FileSearch, 
  Printer, 
  ExternalLink,
  Clock,
  Download,
  FileText,
  AlertCircle,
  X,
  Loader2,
  Sparkles,
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import BghAiReportModal from '@/components/BGH/BghAiReportModal.vue'
import { exportBghToExcel, exportAcademicReportsToPdf } from '@/components/BGH/performance/bghExport.js'
import { usePopupStore } from '@/stores/popup'
import { bghApi } from '@/services/bghApi'
import { aiApi } from '@/services/aiApi'
import { unwrapApiData } from '@/services/apiClient'
import SkeletonDashboard from '@/components/common/skeleton/SkeletonDashboard.vue'

const popup = usePopupStore()
const loading = ref(false)
const error = ref(null)

const activeTab = ref('Class')
const reportType = ref('class')
const semesterFilter = ref('all')
const campusFilter = ref('all')
const industryFilter = ref('')
const majorFilter = ref('')
const generating = ref(false)
const showViewModal = ref(false)
const selectedReport = ref(null)

const industryOptions = ref([
  { id: 'cntt', name: 'Công nghệ thông tin' },
  { id: 'kt', name: 'Kinh tế & Quản trị' },
  { id: 'nn', name: 'Ngôn ngữ & Truyền thông' }
])

const majorsByIndustry = ref({
  cntt: [
    { id: 'pm', name: 'Kỹ thuật phần mềm' },
    { id: 'mmt', name: 'Mạng máy tính & An toàn thông tin' },
    { id: 'ai', name: 'Trí tuệ nhân tạo & Khoa học dữ liệu' }
  ],
  kt: [
    { id: 'qtkd', name: 'Quản trị kinh doanh' },
    { id: 'mkt', name: 'Marketing số' },
    { id: 'tc', name: 'Tài chính - Ngân hàng' }
  ],
  nn: [
    { id: 'nna', name: 'Ngôn ngữ Anh' },
    { id: 'nnh', name: 'Ngôn ngữ Hàn' }
  ]
})

const availableMajors = computed(() => {
  if (!industryFilter.value) return []
  return majorsByIndustry.value[industryFilter.value] || []
})

const semesters = ref([{ value: 'all', label: 'Tất cả học kỳ' }])
const campuses = ref([{ value: 'all', label: 'Tất cả cơ sở' }])
const reportTypes = [
  { value: 'class', label: 'Báo cáo theo Lớp' },
  { value: 'subject', label: 'Báo cáo theo Môn học' },
  { value: 'campus', label: 'Báo cáo theo Cơ sở' },
]

const reports = ref([])
const summaryData = ref(null)
const monthlyStats = ref([])
const departmentStats = ref([])

const tabTotalClasses = ref(0)
const tabTotalTeachers = ref(0)
const tabAvgGpa = ref(0)
const tabTotalSubjects = ref(0)
const tabPassRate = ref(0)
const tabHighFailSubjects = ref(0)

const formatGpa = (val) => {
  const num = Number(val)
  return isNaN(num) ? '0.00' : num.toFixed(2)
}

async function loadData(isInitial = false) {
  if (isInitial) loading.value = true
  error.value = null
  try {
    const params = {}
    if (campusFilter.value !== 'all') params.campusId = campusFilter.value
    if (semesterFilter.value !== 'all') params.semesterId = semesterFilter.value
    params.reportType = reportType.value

    const [reportsRes, overviewRes] = await Promise.all([
      bghApi.getAcademicReports(params),
      bghApi.getAcademicOverview(params),
    ])
    const data = unwrapApiData(reportsRes) || {}
    const overview = unwrapApiData(overviewRes) || {}

    summaryData.value = data.summary || data
    monthlyStats.value = data.monthlyStats || []
    departmentStats.value = data.departmentStats || []
    const s = summaryData.value || {}
    reports.value = [
      { id: 'ACADEMIC-SUMMARY', name: `Tổng quan học tập • ${s.totalStudents || 0} SV, ${s.totalTeachers || 0} GV`, type: 'Toàn trường', date: new Date().toLocaleDateString('vi-VN'), status: 'ready' },
      { id: 'DEPT-PERFORMANCE', name: `Báo cáo hiệu suất theo Khoa / Ngành đào tạo`, type: 'Chuyên ngành', date: new Date().toLocaleDateString('vi-VN'), status: 'ready' },
      { id: 'SEMESTER-TREND', name: `Báo cáo xu hướng kết quả học tập qua các kỳ`, type: 'Học kỳ', date: new Date().toLocaleDateString('vi-VN'), status: 'ready' },
    ]
    tabTotalClasses.value = s.totalClasses ?? 0
    tabTotalTeachers.value = s.totalTeachers ?? 0
    tabAvgGpa.value = Number(s.avgGpa ?? 0)
    tabTotalSubjects.value = overview.totalSubjects ?? 0
    tabPassRate.value = Number(overview.passRate ?? 0)
    tabHighFailSubjects.value = (overview.topSubjects || []).filter(item => Number(item.failRate || 0) >= 20).length
  } catch (e) {
    error.value = e?.message || 'Không thể truy vấn báo cáo từ CSDL.'
    throw e
  } finally {
    if (isInitial) loading.value = false
  }
}

async function loadFilterOptions() {
  const [termRes, orgRes] = await Promise.all([
    bghApi.getAcademicTerms(),
    bghApi.getOrganizations(),
  ])
  semesters.value = [
    { value: 'all', label: 'Tất cả học kỳ' },
    ...(unwrapApiData(termRes) || []).map(term => ({
      value: String(term.maHocKy || term.id),
      label: `${term.tenHocKy}${term.namHoc ? ` · ${term.namHoc}` : ''}`,
    })),
  ]
  campuses.value = [
    { value: 'all', label: 'Tất cả cơ sở' },
    ...(unwrapApiData(orgRes) || []).map(org => ({
      value: String(org.id || org.maDonVi),
      label: org.name || org.tenDonVi || 'Cơ sở',
    })),
  ]
}

onMounted(async () => {
  loading.value = true
  try {
    await loadFilterOptions()
    await loadData(false)
  } catch (e) {
    error.value = e?.message || 'Không thể tải báo cáo.'
  } finally {
    loading.value = false
  }
})

// AI Strategic Report State
const aiModalOpen = ref(false)
const aiLoading = ref(false)
const aiError = ref(null)
const aiReport = ref(null)

const aiScopeBadges = computed(() => {
  const list = []
  const sem = semesters.value.find(s => String(s.value) === String(semesterFilter.value))?.label || 'Tất cả học kỳ'
  list.push(`Học kỳ: ${sem}`)
  const rep = reportTypes.find(r => r.value === reportType.value)?.label || 'Báo cáo chi tiết'
  list.push(`Phân loại: ${rep}`)
  const cam = campuses.value.find(c => String(c.value) === String(campusFilter.value))?.label || 'Tất cả cơ sở'
  list.push(`Cơ sở: ${cam}`)
  return list
})

async function generateReport() {
  if (generating.value) return
  generating.value = true
  aiLoading.value = true
  aiModalOpen.value = true
  aiError.value = null
  try {
    await loadData()
    const reportByType = {
      class: reports.value.find(item => item.id === 'ACADEMIC-SUMMARY'),
      subject: reports.value.find(item => item.id === 'DEPT-PERFORMANCE'),
      campus: reports.value.find(item => item.id === 'SEMESTER-TREND'),
    }
    selectedReport.value = reportByType[reportType.value] || reports.value[0] || null

    // Gọi AI Model 9B để phân tích sâu song song với tạo báo cáo
    const semId = semesterFilter.value !== 'all' ? parseInt(semesterFilter.value) : undefined
    const camId = campusFilter.value !== 'all' ? parseInt(campusFilter.value) : undefined
    const res = await aiApi.generateBghReport({
      reportType: 'detailed_report',
      semesterId: isNaN(semId) ? undefined : semId,
      campusId: isNaN(camId) ? undefined : camId,
      mode: 'deep',
      forceRefresh: true,
    })
    aiReport.value = res
    popup.success('Tạo báo cáo thành công', 'Báo cáo chi tiết và nhận định AI đã được tạo thành công.')
  } catch (e) {
    aiError.value = e?.message || 'Không thể tải dữ liệu phân tích AI.'
    popup.error('Lỗi làm mới báo cáo', e?.message || 'Không thể tải dữ liệu.')
  } finally {
    generating.value = false
    aiLoading.value = false
  }
}

function viewReport(rpt) {
  selectedReport.value = rpt
  showViewModal.value = true
}

async function viewReportInTable(rpt) {
  selectedReport.value = rpt
  if (rpt?.id === 'DEPT-PERFORMANCE') activeTab.value = 'Subject'
  else if (rpt?.id === 'SEMESTER-TREND') activeTab.value = 'Campus'
  else activeTab.value = 'Class'
  showViewModal.value = false
  await nextTick()
  document.getElementById('bgh-report-table')?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

function closeViewModal() {
  showViewModal.value = false
  selectedReport.value = null
}

function prepareExcelData() {
  const result = []
  result.push({ 'Hạng mục': 'TỔNG QUAN HỌC TẬP', 'Số liệu': '', 'Ghi chú': '' })
  if (summaryData.value) {
    result.push({ 'Hạng mục': 'Tổng số sinh viên', 'Số liệu': summaryData.value.totalStudents, 'Ghi chú': 'Toàn hệ thống' })
    result.push({ 'Hạng mục': 'Tổng số giảng viên', 'Số liệu': summaryData.value.totalTeachers, 'Ghi chú': 'Đang giảng dạy' })
    result.push({ 'Hạng mục': 'Tổng số lớp học phần', 'Số liệu': summaryData.value.totalClasses, 'Ghi chú': 'Mở trong kỳ' })
    result.push({ 'Hạng mục': 'GPA Trung bình', 'Số liệu': summaryData.value.avgGpa, 'Ghi chú': 'Thang điểm 10.0' })
  }
  result.push({})
  result.push({ 'Hạng mục': 'THỐNG KÊ THEO HỌC KỲ / LỚP HỌC', 'Số liệu': '', 'Ghi chú': '' })
  monthlyStats.value.forEach(m => {
    result.push({ 'Hạng mục': m.semester, 'Số liệu': `${m.totalGrades} lượt chấm`, 'Ghi chú': `${m.passCount} Pass, ${m.failCount} Fail - GPA TB: ${m.avgGpa}` })
  })
  result.push({})
  result.push({ 'Hạng mục': 'THỐNG KÊ THEO KHOA / CHUYÊN NGÀNH', 'Số liệu': '', 'Ghi chú': '' })
  departmentStats.value.forEach(d => {
    result.push({ 'Hạng mục': d.departmentName, 'Số liệu': `${d.totalGrades} lượt`, 'Ghi chú': `Pass: ${d.passRate}% - GPA TB: ${d.avgGpa}` })
  })
  return result
}

async function exportReport(rpt) {
  try {
    const data = prepareExcelData()
    if (data.length === 0) throw new Error('Báo cáo chưa có dữ liệu để xuất.')
    await exportBghToExcel(data, `${rpt ? rpt.id : 'BaoCao-ChiTiet'}.xlsx`, rpt?.type || 'Báo cáo chi tiết')
    popup.success('Xuất báo cáo thành công', 'File Excel đã được tải xuống thiết bị.')
  } catch (e) {
    popup.error('Không thể xuất báo cáo', e?.message || 'Không thể tạo file Excel.')
  }
}

async function exportExcel() {
  await exportReport(selectedReport.value)
}

const exportingPdf = ref(false)
async function exportPdf() {
  if (exportingPdf.value) return
  exportingPdf.value = true
  try {
    const semLabel = semesters.value.find(s => String(s.value) === String(semesterFilter.value))?.label || 'Tất cả học kỳ'
    const camLabel = campuses.value.find(c => String(c.value) === String(campusFilter.value))?.label || 'Tất cả cơ sở'
    
    await exportAcademicReportsToPdf({
      tabTotalClasses: tabTotalClasses.value,
      tabTotalTeachers: tabTotalTeachers.value,
      tabAvgGpa: tabAvgGpa.value,
      monthlyStats: monthlyStats.value,
      departmentStats: departmentStats.value,
      semesterLabel: semLabel,
      campusLabel: camLabel
    })
  } catch (err) {
    popup.error('Lỗi xuất báo cáo', 'Không thể tạo file PDF.')
    console.error(err)
  } finally {
    exportingPdf.value = false
  }
}
</script>

<template>
  <PageContainer 
    title="Báo cáo học tập chi tiết" 
    subtitle="Công cụ phân tích và kết xuất báo cáo đa chiều theo lớp, môn học và cơ sở đào tạo."
  >
    <template #actions>
      <div class="flex items-center gap-3">
         <button @click="exportPdf" :disabled="exportingPdf" class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
            <Loader2 v-if="exportingPdf" :size="18" class="animate-spin" />
            <FileText v-else :size="18" /> {{ exportingPdf ? 'Đang xuất...' : 'PDF Report' }}
         </button>
         <button @click="exportExcel" class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
            <Download :size="18" /> Excel Data
         </button>
      </div>
    </template>

    <!-- Loading State -->
    <div v-if="loading" class="p-4">
      <SkeletonDashboard :cards="4" :rows="3" />
    </div>
    <div v-else-if="error" class="flex flex-col items-center justify-center py-20 text-center">
      <AlertCircle :size="48" class="text-(--color-danger-text) mb-4" />
      <p class="text-lg font-semibold text-muted">Đã có lỗi xảy ra</p>
      <p class="text-sm text-placeholder mt-1">{{ error }}</p>
      <button @click="loadData" class="mt-4 lg-button-secondary px-4 py-2 text-sm font-semibold">Thử lại</button>
    </div>
    <div v-else id="print-container" class="space-y-8">
      
      <!-- ── Print Header ── -->
      <div class="hidden print:block mb-6 pb-4 border-b border-slate-300">
        <h2 class="text-xl font-bold text-slate-800">Báo cáo học tập chi tiết</h2>
        <p class="text-xs text-slate-500 mt-1">{{ semesters.find(s => s.value === semesterFilter)?.label }}</p>
      </div>

      <!-- ── Report Generator Controls ── -->
      <div class="surface-card border border-card rounded-2xl p-5">
         <div class="flex items-center gap-4 mb-8">
            <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) text-(--color-info-text) flex items-center justify-center shadow-sm border border-(--color-info-text)/20">
               <FileSearch :size="24" />
            </div>
            <div>
               <h3 class="text-xl font-semibold text-heading">Trình tạo báo cáo</h3>
               <p class="text-xs text-muted mt-0.5 font-bold uppercase tracking-widest">Tùy chỉnh các thông số để xuất dữ liệu</p>
            </div>
         </div>

         <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-3">
            <div class="space-y-1.5">
               <label class="text-[10px] font-semibold text-muted uppercase tracking-widest ml-1">Loại báo cáo</label>
               <div class="relative">
                  <LmsSelect v-model="reportType" :options="reportTypes" class="w-full surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none appearance-none cursor-pointer" />
               </div>
            </div>
            <div class="space-y-1.5">
               <label class="text-[10px] font-semibold text-muted uppercase tracking-widest ml-1">Ngành đào tạo</label>
               <div class="relative">
                  <LmsSelect v-model="industryFilter" class="w-full surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none appearance-none cursor-pointer">
                     <option value="">Tất cả Ngành</option>
                     <option v-for="ind in industryOptions" :key="ind.id" :value="ind.id">{{ ind.name }}</option>
                  </LmsSelect>
               </div>
            </div>
            <div class="space-y-1.5">
               <label class="text-[10px] font-semibold text-muted uppercase tracking-widest ml-1">Chuyên ngành</label>
               <div class="relative">
                  <LmsSelect v-model="majorFilter" :disabled="!industryFilter" class="w-full surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none appearance-none cursor-pointer disabled:opacity-50">
                     <option value="">Tất cả Chuyên ngành</option>
                     <option v-for="maj in availableMajors" :key="maj.id" :value="maj.id">{{ maj.name }}</option>
                  </LmsSelect>
               </div>
            </div>
            <div class="space-y-1.5">
               <label class="text-[10px] font-semibold text-muted uppercase tracking-widest ml-1">Học kỳ</label>
               <div class="relative">
                  <LmsSelect v-model="semesterFilter" :options="semesters" class="w-full surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none appearance-none cursor-pointer" />
               </div>
            </div>
            <div class="flex items-end">
               <button @click="generateReport" :disabled="generating" class="w-full lg-button-primary py-2.5 text-xs font-bold flex items-center justify-center gap-2 disabled:opacity-60 cursor-pointer shadow-md shadow-blue-500/20">
                  <Sparkles v-if="!generating" :size="16" />
                  <Loader2 v-else :size="16" class="animate-spin" />
                  <span>{{ generating ? 'ĐANG TẠO & PHÂN TÍCH...' : 'TẠO BÁO CÁO & PHÂN TÍCH AI' }}</span>
               </button>
            </div>
         </div>
      </div>

      <!-- ── Analysis Content ── -->
      <div id="bgh-report-table" class="space-y-4 scroll-mt-4">
         <div class="flex items-center justify-between pb-2 print:hidden">
            <div class="flex gap-8">
               <button 
                 v-for="tab in ['Class', 'Subject', 'Campus']" 
                 :key="tab"
                 @click="activeTab = tab"
                 :class="['pb-4 text-xs font-semibold uppercase tracking-widest relative transition-all', activeTab === tab ? 'text-link' : 'text-muted hover:text-heading']"
               >
                  Báo cáo {{ tab === 'Class' ? 'Lớp' : tab === 'Subject' ? 'Môn' : 'Cơ sở' }}
                  <div v-if="activeTab === tab" class="absolute bottom-0 left-0 right-0 h-1 bg-(--lg-primary) rounded-full"></div>
               </button>
            </div>
            <div class="flex items-center gap-2">
               <button @click="exportPdf" :disabled="exportingPdf" class="p-2 hover:bg-(--surface-input) rounded-lg text-muted transition-colors disabled:opacity-50" title="Xuất PDF"><Printer :size="18" /></button>
               <button @click="exportExcel" class="p-2 hover:bg-(--surface-input) rounded-lg text-muted transition-colors"><Download :size="18" /></button>
            </div>
         </div>

         <!-- Tab Content -->
         <div v-if="activeTab === 'Class'" class="space-y-4">
           <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
             <div class="surface-card border border-card rounded-2xl p-5 text-center">
               <p class="text-2xl font-bold text-heading">{{ summaryData?.totalClasses ?? tabTotalClasses }}</p>
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">Lớp học phần đang mở</p>
             </div>
             <div class="surface-card border border-card rounded-2xl p-5 text-center">
               <p class="text-2xl font-bold text-heading">{{ summaryData?.totalTeachers ?? tabTotalTeachers }}</p>
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">Giảng viên phụ trách</p>
             </div>
             <div class="surface-card border border-card rounded-2xl p-5 text-center">
               <p class="text-2xl font-bold text-heading">{{ summaryData?.avgGpa ?? tabAvgGpa }}</p>
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">GPA TB theo lớp</p>
             </div>
           </div>

           <!-- Bảng thống kê theo Học kỳ / Lớp -->
           <div class="lg-table-shell overflow-hidden">
             <table class="w-full text-left border-collapse">
               <thead>
                 <tr class="surface-solid">
                   <th class="px-4 py-3.5 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Học kỳ / Đơn vị</th>
                   <th class="px-4 py-3.5 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Tổng lượt đánh giá</th>
                   <th class="px-4 py-3.5 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Sĩ số đạt (Pass)</th>
                   <th class="px-4 py-3.5 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Số lượng rớt (Fail)</th>
                   <th class="px-4 py-3.5 text-[10px] font-semibold text-placeholder uppercase tracking-widest">GPA Trung bình</th>
                 </tr>
               </thead>
               <tbody>
                 <tr v-for="m in monthlyStats" :key="m.semester" class="hover:bg-(--surface-input) transition-colors">
                   <td class="px-4 py-3.5 font-bold text-heading text-xs">{{ m.semester }}</td>
                   <td class="px-4 py-3.5 text-xs text-muted font-medium">{{ m.totalGrades }} lượt</td>
                   <td class="px-4 py-3.5 text-xs font-bold text-(--color-success-text)">{{ m.passCount }} Pass</td>
                   <td class="px-4 py-3.5 text-xs font-bold text-(--color-danger-text)">{{ m.failCount }} Fail</td>
                   <td class="px-4 py-3.5 text-xs font-bold text-heading">{{ formatGpa(m.avgGpa) }}</td>
                 </tr>
                 <tr v-if="monthlyStats.length === 0">
                   <td colspan="5" class="py-8 text-center text-xs text-muted">Chưa có dữ liệu thống kê theo lớp học.</td>
                 </tr>
               </tbody>
             </table>
           </div>
         </div>
         <div v-if="activeTab === 'Subject'" class="space-y-4">
           <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
             <div class="surface-card border border-card rounded-2xl p-5 text-center">
               <p class="text-2xl font-bold text-heading">{{ tabTotalSubjects }}</p>
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">Tổng số môn học</p>
             </div>
             <div class="surface-card border border-card rounded-2xl p-5 text-center">
               <p class="text-2xl font-bold text-heading">{{ tabPassRate }}%</p>
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">Tỷ lệ Pass TB</p>
             </div>
             <div class="surface-card border border-card rounded-2xl p-5 text-center">
               <p class="text-2xl font-bold text-heading">{{ tabHighFailSubjects }}</p>
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">Môn tỷ lệ rớt cao</p>
             </div>
           </div>

           <!-- Bảng thống kê theo Khoa / Chuyên ngành -->
           <div class="lg-table-shell overflow-hidden">
             <table class="w-full text-left border-collapse">
               <thead>
                 <tr class="surface-solid">
                   <th class="px-4 py-3.5 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Khoa / Ngành đào tạo</th>
                   <th class="px-4 py-3.5 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Tổng lượt chấm điểm</th>
                   <th class="px-4 py-3.5 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Số bài đạt</th>
                   <th class="px-4 py-3.5 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Tỷ lệ Pass</th>
                   <th class="px-4 py-3.5 text-[10px] font-semibold text-placeholder uppercase tracking-widest">GPA TB</th>
                 </tr>
               </thead>
               <tbody>
                 <tr v-for="d in departmentStats" :key="d.departmentName" class="hover:bg-(--surface-input) transition-colors">
                   <td class="px-4 py-3.5 font-bold text-heading text-xs">{{ d.departmentName }}</td>
                   <td class="px-4 py-3.5 text-xs text-muted font-medium">{{ d.totalGrades }} bài</td>
                   <td class="px-4 py-3.5 text-xs font-bold text-(--color-success-text)">{{ d.passCount }} Pass</td>
                   <td class="px-4 py-3.5 text-xs font-bold text-(--color-info-text)">{{ d.passRate }}%</td>
                   <td class="px-4 py-3.5 text-xs font-bold text-heading">{{ formatGpa(d.avgGpa) }}</td>
                 </tr>
                 <tr v-if="departmentStats.length === 0">
                   <td colspan="5" class="py-8 text-center text-xs text-muted">Chưa có dữ liệu thống kê theo khoa/ngành.</td>
                 </tr>
               </tbody>
             </table>
           </div>
         </div>
         <div v-if="activeTab === 'Campus'" class="space-y-4">
           <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
             <div class="surface-card border border-card rounded-2xl p-5 text-center">
               <p class="text-2xl font-bold text-heading">{{ summaryData?.totalStudents ?? '—' }}</p>
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">Tổng sinh viên toàn trường</p>
             </div>
             <div class="surface-card border border-card rounded-2xl p-5 text-center">
               <p class="text-2xl font-bold text-heading">{{ summaryData?.activeCourses ?? '—' }}</p>
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">Khóa học đang hoạt động</p>
             </div>
           </div>
         </div>

         <!-- Report Cards -->
         <div v-if="reports.length" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            <div 
              v-for="rpt in reports" 
              :key="rpt.id" 
              class="surface-card border border-card rounded-2xl p-4 group hover:border-(--border-input-focus) transition-all"
            >
               <div class="flex items-start justify-between mb-4">
                  <div :class="['h-10 w-10 rounded-xl flex items-center justify-center shadow-sm border', rpt.status === 'generating' ? 'bg-(--color-warning-bg) text-(--color-warning-text) border-(--color-warning-text)/20 animate-pulse' : 'bg-(--color-info-bg) text-(--color-info-text) border-(--color-info-text)/20']">
                     <FileSearch v-if="rpt.status === 'ready'" :size="20" />
                     <Clock v-else :size="20" />
                  </div>
                  <span :class="['text-[9px] font-semibold uppercase tracking-widest px-2 py-1 rounded-lg border', rpt.status === 'ready' ? 'bg-(--color-success-bg) text-(--color-success-text) border-(--color-success-text)/20' : 'bg-(--color-warning-bg) text-(--color-warning-text) border-(--color-warning-text)/20']">
                     {{ rpt.status === 'ready' ? 'Sẵn sàng' : 'Đang tạo' }}
                  </span>
               </div>
               
               <h4 class="text-sm font-semibold text-heading leading-snug group-hover:text-link transition-colors">{{ rpt.name }}</h4>
               <p class="text-[10px] font-bold text-muted mt-2 uppercase tracking-widest">{{ rpt.type }} • {{ rpt.date }}</p>
               
                <div class="mt-6 pt-5 flex items-center justify-between">
                   <div class="flex gap-2">
                      <button @click="viewReportInTable(rpt)" class="text-[10px] font-semibold text-muted hover:text-link uppercase">View in table</button>
                      <button @click="exportReport(rpt)" class="text-[10px] font-semibold text-muted hover:text-link uppercase">Export</button>
                   </div>
                   <button @click="viewReport(rpt)" class="text-placeholder hover:text-link"><ExternalLink :size="16" /></button>
                </div>
            </div>
         </div>
         <div v-else class="py-12 text-center surface-card border border-card rounded-2xl">
           <p class="text-sm text-muted font-medium">Chưa có báo cáo nào. Sử dụng trình tạo báo cáo để tạo mới.</p>
         </div>
      </div>

      <!-- ── View Report Modal ── -->
      <Teleport to="body">
        <div v-if="showViewModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
          <div class="w-full max-w-3xl surface-card rounded-2xl shadow-2xl border border-card overflow-hidden flex flex-col max-h-[90vh]">
            <div class="p-5 bg-(--surface-card) border-b border-card flex justify-between items-center">
              <div>
                <h3 class="text-base font-bold text-heading flex items-center gap-2">
                  <FileSearch :size="20" class="text-link" />
                  {{ selectedReport?.name || 'Chi tiết báo cáo' }}
                </h3>
                <p class="text-xs text-muted mt-0.5">Mã: {{ selectedReport?.id }} · Loại: {{ selectedReport?.type }} · Ngày tạo: {{ selectedReport?.date }}</p>
              </div>
              <button @click="closeViewModal" class="p-1.5 hover:bg-(--surface-input) rounded-lg text-muted transition-colors">
                <X :size="20" />
              </button>
            </div>

            <div class="p-6 overflow-y-auto space-y-6 flex-1">
              <!-- KPI Cards -->
              <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
                <div class="surface-input p-3 rounded-xl border border-card text-center">
                  <p class="text-[10px] font-bold text-muted uppercase">Sinh viên</p>
                  <p class="text-lg font-bold text-heading mt-0.5">{{ summaryData?.totalStudents?.toLocaleString() || '—' }}</p>
                </div>
                <div class="surface-input p-3 rounded-xl border border-card text-center">
                  <p class="text-[10px] font-bold text-muted uppercase">Giảng viên</p>
                  <p class="text-lg font-bold text-heading mt-0.5">{{ summaryData?.totalTeachers?.toLocaleString() || '—' }}</p>
                </div>
                <div class="surface-input p-3 rounded-xl border border-card text-center">
                  <p class="text-[10px] font-bold text-muted uppercase">Lớp học phần</p>
                  <p class="text-lg font-bold text-heading mt-0.5">{{ summaryData?.totalClasses?.toLocaleString() || '—' }}</p>
                </div>
                <div class="surface-input p-3 rounded-xl border border-card text-center">
                  <p class="text-[10px] font-bold text-muted uppercase">GPA TB</p>
                  <p class="text-lg font-bold text-heading mt-0.5">{{ summaryData?.avgGpa ? formatGpa(summaryData.avgGpa) : '—' }}</p>
                </div>
              </div>

              <!-- Thống kê học kỳ -->
              <div>
                <h4 class="text-xs font-bold text-heading uppercase tracking-wide mb-3">Thống kê kết quả theo Học kỳ / Lớp</h4>
                <div class="lg-table-shell overflow-hidden">
                  <table class="w-full text-left text-xs border-collapse">
                    <thead>
                      <tr class="surface-solid">
                        <th class="px-3 py-2.5 font-bold text-placeholder uppercase">Học kỳ</th>
                        <th class="px-3 py-2.5 font-bold text-placeholder uppercase">Tổng lượt chấm</th>
                        <th class="px-3 py-2.5 font-bold text-placeholder uppercase">Số bài Pass</th>
                        <th class="px-3 py-2.5 font-bold text-placeholder uppercase">Số bài Fail</th>
                        <th class="px-3 py-2.5 font-bold text-placeholder uppercase">GPA TB</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="m in monthlyStats" :key="m.semester" class="hover:bg-(--surface-input)">
                        <td class="px-3 py-2.5 font-bold text-heading">{{ m.semester }}</td>
                        <td class="px-3 py-2.5 text-muted">{{ m.totalGrades }} lượt</td>
                        <td class="px-3 py-2.5 font-bold text-(--color-success-text)">{{ m.passCount }}</td>
                        <td class="px-3 py-2.5 font-bold text-(--color-danger-text)">{{ m.failCount }}</td>
                        <td class="px-3 py-2.5 font-bold text-heading">{{ formatGpa(m.avgGpa) }}</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>

              <!-- Thống kê Khoa -->
              <div>
                <h4 class="text-xs font-bold text-heading uppercase tracking-wide mb-3">Thống kê theo Khoa / Ngành đào tạo</h4>
                <div class="lg-table-shell overflow-hidden">
                  <table class="w-full text-left text-xs border-collapse">
                    <thead>
                      <tr class="surface-solid">
                        <th class="px-3 py-2.5 font-bold text-placeholder uppercase">Khoa / Ngành</th>
                        <th class="px-3 py-2.5 font-bold text-placeholder uppercase">Tổng lượt</th>
                        <th class="px-3 py-2.5 font-bold text-placeholder uppercase">Tỷ lệ Pass</th>
                        <th class="px-3 py-2.5 font-bold text-placeholder uppercase">GPA TB</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="d in departmentStats" :key="d.departmentName" class="hover:bg-(--surface-input)">
                        <td class="px-3 py-2.5 font-bold text-heading">{{ d.departmentName }}</td>
                        <td class="px-3 py-2.5 text-muted">{{ d.totalGrades }} lượt</td>
                        <td class="px-3 py-2.5 font-bold text-(--color-info-text)">{{ d.passRate }}%</td>
                        <td class="px-3 py-2.5 font-bold text-heading">{{ formatGpa(d.avgGpa) }}</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>

            <div class="p-4 bg-(--surface-card) border-t border-card flex justify-end gap-3">
              <button @click="closeViewModal" class="px-4 py-2 text-xs font-bold border border-input rounded-lg hover:bg-(--surface-input)">Đóng</button>
              <button @click="viewReportInTable(selectedReport)" class="px-4 py-2 text-xs font-bold border border-input rounded-lg hover:bg-(--surface-input)">View in table</button>
              <button @click="exportReport(selectedReport)" class="flex items-center gap-1.5 px-4 py-2 bg-(--lg-primary) text-white text-xs font-bold rounded-lg hover:bg-(--lg-primary-dark)">
                <Download :size="14" />
                <span>Tải dữ liệu Excel</span>
              </button>
            </div>
          </div>
        </div>
      </Teleport>

    </div>

    <!-- AI Strategic Report Modal -->
    <BghAiReportModal
      :is-open="aiModalOpen"
      title="Báo Cáo Học Thuật Chi Tiết AI (Qwen 9B)"
      subtitle="Tổng hợp dữ liệu đa chiều theo lớp, môn học, cơ sở và đề xuất giải pháp đào tạo"
      :scope-badges="aiScopeBadges"
      :loading="aiLoading"
      :error="aiError"
      :report-content="aiReport?.aiAnalysis"
      :generated-at="aiReport?.generatedAt"
      @close="aiModalOpen = false"
      @retry="generateReport"
    />
  </PageContainer>
</template>

<style scoped>
@media print {
  #print-container { padding: 0; color: #1e293b; }
  #print-container .surface-card { border: 1px solid #cbd5e1; background: #fff; box-shadow: none; break-inside: avoid; }
}
</style>

<script setup>
import { ref } from 'vue'
import { 
  FileSearch, 
  BarChart, 
  Printer, 
  Calendar, 
  Building2, 
  ChevronDown,
  ExternalLink,
  Clock,
  Download,
  FileText,
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { exportToExcel, triggerPrint } from '@/services/exportService.js'
import { usePopupStore } from '@/stores/popup'

const popup = usePopupStore()

const activeTab = ref('Class')
const reportType = ref('class')
const semesterFilter = ref('spring-2026')
const campusFilter = ref('all')
const generating = ref(false)

const semesters = [
  { value: 'spring-2026', label: 'Spring 2026' },
  { value: 'fall-2025', label: 'Fall 2025' },
]

const campuses = [
  { value: 'all', label: 'Tất cả cơ sở' },
  { value: 'hcm', label: 'Cơ sở Hồ Chí Minh' },
  { value: 'dn', label: 'Cơ sở Đà Nẵng' },
]

const reports = ref([
  { id: 'RPT-001', name: 'Báo cáo chất lượng học tập kỳ Spring 2026', type: 'Toàn trường', date: '12/05/2026', status: 'ready' },
  { id: 'RPT-002', name: 'Phân tích tỷ lệ Incomplete môn đồ án', type: 'Theo môn học', date: '10/05/2026', status: 'ready' },
  { id: 'RPT-003', name: 'Thống kê điểm trung bình Khoa CNTT', type: 'Theo khoa', date: '08/05/2026', status: 'generating' },
])

function generateReport() {
  generating.value = true
  const labels = { class: 'Theo Lớp', subject: 'Theo Môn học', campus: 'Theo Cơ sở' }
  const semesterLabel = semesters.find(s => s.value === semesterFilter.value)?.label || 'Unknown'
  const newReport = {
    id: `RPT-${String(reports.value.length + 1).padStart(3, '0')}`,
    name: `Báo cáo ${labels[reportType]} - ${semesterLabel}`,
    type: labels[reportType],
    date: new Date().toLocaleDateString('vi-VN'),
    status: 'generating',
  }
  reports.value.unshift(newReport)
  setTimeout(() => {
    const idx = reports.value.findIndex(r => r.id === newReport.id)
    if (idx !== -1) reports.value[idx].status = 'ready'
    generating.value = false
  }, 1500)
}

function viewReport(rpt) {
  popup.info(`Báo cáo: ${rpt.name}`, `Mã: ${rpt.id}\nLoại: ${rpt.type}\nNgày tạo: ${rpt.date}\nTrạng thái: ${rpt.status === 'ready' ? 'Sẵn sàng' : 'Đang tạo'}`)
}

function exportReport(rpt) {
  const data = [{
    'Mã báo cáo': rpt.id,
    'Tên báo cáo': rpt.name,
    'Loại': rpt.type,
    'Ngày tạo': rpt.date,
    'Trạng thái': rpt.status === 'ready' ? 'Sẵn sàng' : 'Đang tạo',
  }]
  exportToExcel(data, `${rpt.id}.xlsx`, rpt.name)
  popup.success('Xuất báo cáo', `Đã xuất "${rpt.name}" thành công.`)
}

function prepareExcelData() {
  return reports.value.map(r => ({
    'Mã báo cáo': r.id,
    'Tên báo cáo': r.name,
    'Loại': r.type,
    'Ngày tạo': r.date,
    'Trạng thái': r.status === 'ready' ? 'Sẵn sàng' : 'Đang tạo',
  }))
}

function exportExcel() {
  exportToExcel(prepareExcelData(), `BaoCao-HocTap-${semesterFilter.value}.xlsx`, 'Báo cáo học tập')
}
</script>

<template>
  <PageContainer 
    title="Báo cáo học tập chi tiết" 
    subtitle="Công cụ phân tích và kết xuất báo cáo đa chiều theo lớp, môn học và cơ sở đào tạo."
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

    <div id="print-container" class="space-y-8">
      
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

         <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <div class="space-y-2">
               <label class="text-[10px] font-semibold text-muted uppercase tracking-widest ml-1">Loại báo cáo</label>
               <div class="relative">
                  <select v-model="reportType" class="w-full surface-input border border-input rounded-2xl px-5 py-3.5 text-sm font-bold outline-none appearance-none cursor-pointer">
                     <option value="class">Báo cáo theo Lớp</option>
                     <option value="subject">Báo cáo theo Môn học</option>
                     <option value="campus">Báo cáo theo Cơ sở</option>
                  </select>
                  <ChevronDown :size="16" class="absolute right-4 top-1/2 -translate-y-1/2 text-placeholder pointer-events-none" />
               </div>
            </div>
            <div class="space-y-2">
               <label class="text-[10px] font-semibold text-muted uppercase tracking-widest ml-1">Học kỳ</label>
               <div class="relative">
                  <select v-model="semesterFilter" class="w-full surface-input border border-input rounded-2xl px-5 py-3.5 text-sm font-bold outline-none appearance-none cursor-pointer">
                     <option v-for="s in semesters" :key="s.value" :value="s.value">{{ s.label }}</option>
                  </select>
                  <Calendar :size="16" class="absolute right-4 top-1/2 -translate-y-1/2 text-placeholder pointer-events-none" />
               </div>
            </div>
            <div class="space-y-2">
               <label class="text-[10px] font-semibold text-muted uppercase tracking-widest ml-1">Cơ sở (Campus)</label>
               <div class="relative">
                  <select v-model="campusFilter" class="w-full surface-input border border-input rounded-2xl px-5 py-3.5 text-sm font-bold outline-none appearance-none cursor-pointer">
                     <option v-for="c in campuses" :key="c.value" :value="c.value">{{ c.label }}</option>
                  </select>
                  <Building2 :size="16" class="absolute right-4 top-1/2 -translate-y-1/2 text-placeholder pointer-events-none" />
               </div>
            </div>
            <div class="flex items-end">
               <button @click="generateReport" :disabled="generating" class="w-full lg-button-primary py-3.5 text-sm font-semibold flex items-center justify-center gap-2 disabled:opacity-60">
                  <BarChart :size="18" /> {{ generating ? 'ĐANG TẠO...' : 'TẠO BÁO CÁO' }}
               </button>
            </div>
         </div>
      </div>

      <!-- ── Analysis Content ── -->
      <div class="space-y-4">
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
               <button @click="triggerPrint" class="p-2 hover:bg-(--surface-input) rounded-lg text-muted transition-colors"><Printer :size="18" /></button>
               <button @click="exportExcel" class="p-2 hover:bg-(--surface-input) rounded-lg text-muted transition-colors"><Download :size="18" /></button>
            </div>
         </div>

         <!-- Tab Content -->
         <div v-if="activeTab === 'Class'" class="space-y-4">
           <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
             <div class="surface-card border border-card rounded-2xl p-5 text-center">
               <p class="text-2xl font-bold text-heading">156</p>
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">Lớp học phần đang mở</p>
             </div>
             <div class="surface-card border border-card rounded-2xl p-5 text-center">
               <p class="text-2xl font-bold text-heading">89</p>
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">Giảng viên phụ trách</p>
             </div>
             <div class="surface-card border border-card rounded-2xl p-5 text-center">
               <p class="text-2xl font-bold text-heading">3.12</p>
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">GPA TB theo lớp</p>
             </div>
           </div>
         </div>
         <div v-if="activeTab === 'Subject'" class="space-y-4">
           <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
             <div class="surface-card border border-card rounded-2xl p-5 text-center">
               <p class="text-2xl font-bold text-heading">245</p>
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">Tổng số môn học</p>
             </div>
             <div class="surface-card border border-card rounded-2xl p-5 text-center">
               <p class="text-2xl font-bold text-heading">88.4%</p>
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">Tỷ lệ Pass TB</p>
             </div>
             <div class="surface-card border border-card rounded-2xl p-5 text-center">
               <p class="text-2xl font-bold text-heading">12</p>
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">Môn tỷ lệ rớt cao</p>
             </div>
           </div>
         </div>
         <div v-if="activeTab === 'Campus'" class="space-y-4">
           <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
             <div class="surface-card border border-card rounded-2xl p-5 text-center">
               <p class="text-2xl font-bold text-heading">890</p>
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">SV Cơ sở chính</p>
             </div>
             <div class="surface-card border border-card rounded-2xl p-5 text-center">
               <p class="text-2xl font-bold text-heading">350</p>
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-1">SV Cơ sở 2</p>
             </div>
           </div>
         </div>

         <!-- Report Cards -->
         <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
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
                      <button @click="viewReport(rpt)" class="text-[10px] font-semibold text-muted hover:text-link uppercase">View</button>
                      <button @click="exportReport(rpt)" class="text-[10px] font-semibold text-muted hover:text-link uppercase">Export</button>
                   </div>
                   <button @click="viewReport(rpt)" class="text-placeholder hover:text-link"><ExternalLink :size="16" /></button>
                </div>
            </div>
         </div>
      </div>

    </div>
  </PageContainer>
</template>

<style scoped>
@media print {
  #print-container { padding: 0; color: #1e293b; }
  #print-container .surface-card { border: 1px solid #cbd5e1; background: #fff; box-shadow: none; break-inside: avoid; }
}
</style>

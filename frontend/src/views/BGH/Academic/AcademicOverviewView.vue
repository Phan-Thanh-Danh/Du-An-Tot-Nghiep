<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { 
  Users, 
  Award, 
  CheckCircle2, 
  TrendingUp, 
  AlertCircle,
  ChevronRight, 
  BarChart3, 
  PieChart, 
  FileText,
  Download,
  ChevronDown,
  BookOpen,
  TrendingDown,
  ArrowUpRight,
  GraduationCap,
  Eye,
  Loader2,
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { exportBghToExcel, printBghPage as triggerPrint } from '@/components/BGH/performance/bghExport.js'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const loading = ref(false)
const error = ref(null)

const semesterFilter = ref('all')
const industryFilter = ref('all')
const majorFilter = ref('all')
const campusFilter = ref('all')
const showExport = ref(false)

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

const kpis = ref([])
const distribution = ref([])
const chartData = ref([])
const topSubjects = ref([])
const totalTeachersVal = ref(0)
const totalClassesVal = ref(0)

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
      bghApi.getAcademicOverview(params).catch(() => null),
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

    totalTeachersVal.value = data.totalTeachers ?? 0
    totalClassesVal.value = data.totalClasses ?? 0
    kpis.value = [
      { id: 1, label: 'Tổng số sinh viên', value: (data.totalStudents ?? 0).toLocaleString(), trend: '—', icon: Users, color: 'text-(--color-info-text)', bgColor: 'bg-(--color-info-bg)' },
      { id: 2, label: 'GPA Trung bình', value: (Number(data.avgGpa) || 0).toFixed(2), trend: '—', icon: Award, color: 'text-(--color-info-text)', bgColor: 'bg-(--color-info-bg)' },
      { id: 3, label: 'Tỷ lệ đạt (Pass)', value: (Number(data.passRate) || 0).toFixed(1) + '%', trend: '—', icon: CheckCircle2, color: 'text-(--color-success-text)', bgColor: 'bg-(--color-success-bg)' },
      { id: 4, label: 'Nguy cơ rớt môn', value: (data.atRiskCount ?? 0).toString(), trend: 'Cần chú ý', icon: AlertCircle, color: 'text-(--color-danger-text)', bgColor: 'bg-(--color-danger-bg)' },
    ]
    distribution.value = (data.gradeDistribution || []).map(d => ({
      range: d.grade || '—',
      count: d.count ?? 0,
      percent: d.percent ?? 0,
      color: (d.grade || '').startsWith('A') ? 'bg-(--color-success-text)' : (d.grade || '').startsWith('B') ? 'bg-(--color-info-text)' : (d.grade || '').startsWith('C') ? 'bg-(--lg-primary)' : (d.grade || '').startsWith('D') ? 'bg-(--color-warning-text)' : 'bg-(--color-danger-text)',
    }))
    topSubjects.value = (data.topSubjects || []).map(s => ({
      name: s.subjectName || 'Môn học',
      class: '',
      teacher: '',
      total: s.total ?? 0,
      pass: s.pass ?? 0,
      failRate: s.failRate ?? 0,
      trend: (s.failRate || 0) <= 5 ? 'up' : 'down',
    }))
    chartData.value = (data.semesterTrend || []).map(t => ({
      k: t.semester || 'Học kỳ',
      toanTruong: Number(t.avgGpa) || 0
    }))
    if (filterOptions.semesters && filterOptions.semesters.length > 0) {
      semesters.value = [
        { value: 'all', label: 'Tất cả học kỳ' },
        ...filterOptions.semesters.map(s => ({
          value: String(s.id || s.maHocKy),
          label: s.label || s.tenHocKy || `Học kỳ ${s.id}`
        }))
      ]
    } else if (semesters.value.length <= 1 && chartData.value.length > 0) {
      semesters.value = [
        { value: 'all', label: 'Tất cả học kỳ' },
        ...chartData.value.map(item => ({ value: item.k, label: item.k })),
      ]
    }
  } catch (_e) {
    error.value = null
  } finally {
    if (isInitial) loading.value = false
  }
}

watch(industryFilter, () => {
  majorFilter.value = 'all'
})

watch([semesterFilter, industryFilter, majorFilter, campusFilter], () => {
  loadData(false)
})

onMounted(() => { loadData(true) })

function prepareExcelData() {
  return [
    ...kpis.value.map(k => ({ 'Chỉ tiêu': k.label, 'Giá trị': k.value, 'Xu hướng': k.trend })),
    {},
    { 'Chỉ tiêu': 'Phân phối điểm số', 'Giá trị': '', 'Xu hướng': '' },
    ...distribution.value.map(d => ({ 'Chỉ tiêu': d.range, 'Giá trị': `${d.count} SV`, 'Xu hướng': `${d.percent}%` })),
    {},
    { 'Chỉ tiêu': 'Xu hướng GPA theo kỳ', 'Giá trị': '', 'Xu hướng': '' },
    ...chartData.value.map(d => ({ 'Chỉ tiêu': d.k, 'Giá trị': `Toàn trường: ${d.toanTruong}`, 'Xu hướng': '' })),
    {},
    { 'Chỉ tiêu': 'Xếp hạng môn học', 'Giá trị': '', 'Xu hướng': '' },
    ...topSubjects.value.map(s => ({ 'Chỉ tiêu': s.name, 'Giá trị': `Lớp ${s.class} - ${s.teacher}`, 'Xu hướng': `${s.failRate}% fail` })),
  ]
}

function exportExcel() {
  exportBghToExcel(prepareExcelData(), `BaoCao-TongQuan-${semesterFilter.value}.xlsx`, 'Tổng quan')
}

const exportOptions = [
  { label: 'PDF Report', icon: FileText, action: triggerPrint },
  { label: 'Excel Data', icon: Download, action: exportExcel },
]

const getBarHeight = (gpa) => {
  if (!gpa || isNaN(gpa)) return 0
  return Math.min(Math.round((gpa / 10.0) * 100), 100)
}

const getBarColor = (index) => {
  const colors = ['bg-(--lg-primary)', 'bg-(--color-info-text)', 'bg-(--color-warning-text)', 'bg-(--color-success-text)', 'bg-(--color-danger-text)']
  return colors[index % colors.length]
}
</script>

<template>
  <PageContainer 
    title="Tổng quan kết quả học tập" 
    subtitle="Báo cáo phân tích chất lượng học tập và hiệu quả giảng dạy trên toàn hệ thống."
  >
    <template #actions>
      <div class="relative">
        <button @click="showExport = !showExport" class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
          <Download :size="18" /> Xuất báo cáo <ChevronDown :size="16" />
        </button>
        <div v-if="showExport" class="absolute right-0 top-full mt-2 surface-card border border-card rounded-2xl p-2 shadow-xl z-50 min-w-[180px]">
          <button v-for="opt in exportOptions" :key="opt.label" @click="opt.action(); showExport = false" class="w-full flex items-center gap-3 px-4 py-2.5 text-xs font-bold text-label hover:bg-(--surface-input) rounded-xl transition-colors">
            <component :is="opt.icon" :size="16" class="text-placeholder" />
            {{ opt.label }}
          </button>
        </div>
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
    <div v-else id="print-container" class="space-y-6">
      
      <!-- ── Print Header (hidden on screen) ── -->
      <div class="hidden print:block mb-8 pb-6 border-b border-slate-300">
        <h2 class="text-2xl font-bold text-slate-800">Báo cáo tổng quan kết quả học tập</h2>
        <p class="text-sm text-slate-500 mt-1">Học kỳ: {{ semesters.find(s => s.value === semesterFilter)?.label }}</p>
      </div>

      <!-- ── Filter Bar (hidden when printing) ── -->
      <div class="surface-card border border-card rounded-2xl p-4 flex flex-wrap items-center gap-3 print:hidden">
        <div class="flex items-center gap-2 text-xs font-semibold text-muted uppercase tracking-widest mr-2">
          <BarChart3 :size="16" /> Lọc
        </div>
        <LmsSelect v-model="semesterFilter" :options="semesters" class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)" />
        <LmsSelect v-model="industryFilter" :options="industries" class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)" />
        <LmsSelect v-model="majorFilter" :options="availableMajors" class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)" />
        <LmsSelect v-model="campusFilter" :options="campuses" class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)" />
      </div>

      <!-- ── KPI Cards ── -->
      <div v-if="kpis.length" class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
        <div v-for="kpi in kpis" :key="kpi.id" class="surface-card border border-card rounded-2xl p-4 group transition-all">
           <div class="flex items-center justify-between mb-4">
              <div :class="['h-10 w-10 rounded-2xl flex items-center justify-center shadow-sm border border-default', kpi.bgColor, kpi.color]">
                 <component :is="kpi.icon" :size="24" />
              </div>
              <span :class="['text-[10px] font-semibold uppercase tracking-widest px-2 py-1 rounded-lg', typeof kpi.trend === 'string' && kpi.trend.includes('+') ? 'bg-(--color-success-bg) text-(--color-success-text)' : 'bg-(--color-danger-bg) text-(--color-danger-text)']">
                 {{ kpi.trend }}
              </span>
           </div>
           <p class="text-xs font-semibold text-muted uppercase tracking-widest">{{ kpi.label }}</p>
           <h3 class="text-xl font-semibold text-heading mt-1">{{ kpi.value }}</h3>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        <!-- ── Chart Section ── -->
        <div class="lg:col-span-2 surface-card border border-card rounded-2xl p-5 overflow-hidden relative">
           <div class="flex items-center justify-between mb-8">
              <div>
                 <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Xu hướng GPA theo học kỳ</h4>
                 <p class="text-xs text-muted mt-1 font-bold">Dữ liệu so sánh các học kỳ gần nhất</p>
              </div>
           </div>

           <div v-if="chartData.length" class="h-64 flex items-end gap-3 pl-12 pr-4 pt-6 pb-2 relative">
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
              
              <div v-for="(item, i) in chartData" :key="item.k" class="flex-1 group relative cursor-pointer flex flex-col items-center justify-end h-full z-10">
                 <!-- Điểm GPA hiển thị trên đỉnh cột -->
                 <span class="text-[11px] font-extrabold text-heading mb-1 transition-transform group-hover:scale-110">
                   {{ (Number(item.toanTruong) || 0).toFixed(2) }}
                 </span>

                 <!-- Cột Gradient -->
                 <div class="w-full flex justify-center items-end h-44">
                   <div 
                     :style="{ height: `${getBarHeight(item.toanTruong)}%` }" 
                     class="w-8 max-w-[42px] rounded-t-xl transition-all duration-700 ease-out bg-gradient-to-t from-cyan-600 via-blue-600 to-indigo-500 shadow-md group-hover:shadow-indigo-500/50 group-hover:brightness-110"
                   ></div>
                 </div>

                 <!-- Tên Học kỳ -->
                 <p class="text-center text-[10px] font-bold text-muted uppercase tracking-wider mt-2.5 truncate max-w-full">
                   {{ item.k.replace('Học kỳ ', 'Kỳ ').trim() }}
                 </p>

                 <!-- Tooltip khi Hover -->
                 <div class="absolute -top-12 left-1/2 -translate-x-1/2 surface-modal text-heading text-[11px] font-bold px-3 py-1.5 rounded-xl shadow-xl opacity-0 group-hover:opacity-100 transition-all duration-200 border border-default whitespace-nowrap z-30 pointer-events-none">
                    <span class="text-link">GPA TB: {{ item.toanTruong.toFixed(2) }}</span> / 10.0
                 </div>
              </div>
           </div>
           <div v-else class="h-64 flex items-center justify-center">
             <p class="text-xs text-muted font-medium">Chưa có dữ liệu xu hướng GPA</p>
           </div>
        </div>

        <!-- ── Distribution List ── -->
        <div class="surface-card border border-card rounded-2xl p-6">
           <h4 class="text-sm font-semibold text-heading uppercase tracking-wide mb-6">Phân phối điểm số</h4>
           <div v-if="distribution.length" class="space-y-4">
              <div v-for="item in distribution" :key="item.range">
                 <div class="flex items-center justify-between mb-2">
                    <span class="text-xs font-semibold text-label uppercase tracking-tighter">{{ item.range }}</span>
                    <span class="text-xs font-semibold text-heading">{{ item.count }} SV ({{ item.percent }}%)</span>
                 </div>
                 <div class="h-2 w-full bg-(--surface-input) rounded-full overflow-hidden">
                    <div 
                      :style="{ width: `${item.percent}%` }" 
                      :class="['h-full rounded-full transition-all duration-1000', item.color]"
                    ></div>
                 </div>
              </div>
           </div>
           <div v-else class="py-8 text-center">
             <p class="text-xs text-muted font-medium">Chưa có dữ liệu phân phối điểm</p>
           </div>
           
           <div class="mt-6 pt-6">
              <div class="flex items-center gap-4 text-xs font-bold text-muted">
                 <PieChart :size="18" class="text-link shrink-0" />
                 <p>Hệ số điểm A/B chiếm <strong>{{ distribution.length ? distribution.filter(d => d.range.startsWith('A') || d.range.startsWith('B')).reduce((s, d) => s + d.percent, 0).toFixed(0) : 0 }}%</strong>, cho thấy chất lượng đào tạo đang ở mức Khá.</p>
              </div>
           </div>
        </div>

      </div>

      <!-- ── Top / Bottom Subjects ── -->
      <div class="surface-card border border-card rounded-2xl overflow-hidden">
        <div class="px-5 py-4 flex items-center justify-between">
          <div>
            <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Xếp hạng môn học theo tỷ lệ Pass</h4>
            <p class="text-xs text-muted mt-0.5 font-bold">Môn có tỷ lệ Pass cao nhất và thấp nhất trong kỳ</p>
          </div>
          <router-link to="/bgh/academic/pass-fail" class="text-[10px] font-bold text-link hover:underline flex items-center gap-1">
            Xem tất cả <ChevronRight :size="12" />
          </router-link>
        </div>
        <div v-if="topSubjects.length" class="overflow-x-auto">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="surface-solid">
                <th class="px-5 py-3.5 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Môn học & Lớp</th>
                <th class="px-5 py-3.5 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Giảng viên</th>
                <th class="px-5 py-3.5 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Sĩ số / Pass</th>
                <th class="px-5 py-3.5 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Tỷ lệ rớt</th>
                <th class="px-5 py-3.5 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Xu hướng</th>
                <th class="px-5 py-3.5 text-[10px] font-semibold text-placeholder uppercase tracking-widest"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="sub in topSubjects" :key="sub.name" class="group hover:bg-(--surface-input) transition-colors">
                <td class="px-5 py-4">
                  <div class="flex items-center gap-3">
                    <div class="h-9 w-9 rounded-xl bg-(--color-info-bg) flex items-center justify-center text-(--color-info-text) border border-(--color-info-text)/20">
                      <BookOpen :size="18" />
                    </div>
                    <div>
                      <p class="text-sm font-semibold text-heading leading-tight">{{ sub.name }}</p>
                      <p class="text-[11px] font-bold text-muted mt-0.5">{{ sub.class }}</p>
                    </div>
                  </div>
                </td>
                <td class="px-5 py-4">
                  <span class="text-xs font-bold text-label">{{ sub.teacher || '—' }}</span>
                </td>
                <td class="px-5 py-4">
                  <div class="flex items-center gap-2">
                    <Users :size="12" class="text-placeholder" />
                    <span class="text-xs font-semibold text-muted">{{ sub.total }} SV</span>
                    <span class="text-[10px] font-semibold text-(--color-success-text) ml-2">({{ sub.pass }} Pass)</span>
                  </div>
                </td>
                <td class="px-5 py-4">
                  <div :class="['px-2.5 py-1 rounded-lg text-[10px] font-semibold uppercase tracking-widest border w-fit shadow-sm', sub.failRate >= 20 ? 'bg-(--color-danger-bg) text-(--color-danger-text) border-(--color-danger-text)/20' : sub.failRate >= 10 ? 'bg-(--color-warning-bg) text-(--color-warning-text) border-(--color-warning-text)/20' : 'bg-(--color-success-bg) text-(--color-success-text) border-(--color-success-text)/20']">
                    {{ sub.failRate }}%
                  </div>
                </td>
                <td class="px-5 py-4">
                  <div v-if="sub.trend === 'up'" class="flex items-center gap-1 text-[10px] font-bold text-(--color-success-text)">
                    <ArrowUpRight :size="14" /> Cải thiện
                  </div>
                  <div v-else class="flex items-center gap-1 text-[10px] font-bold text-(--color-danger-text)">
                    <TrendingDown :size="14" /> Giảm
                  </div>
                </td>
                <td class="px-5 py-4 text-right">
                  <router-link to="/bgh/academic/pass-fail" class="p-2 hover:bg-(--color-info-bg) hover:text-link rounded-lg text-placeholder transition-all inline-flex">
                    <Eye :size="18" />
                  </router-link>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-else class="py-12 text-center">
          <p class="text-xs text-muted font-medium">Chưa có dữ liệu môn học</p>
        </div>
      </div>

      <!-- ── Quick Insights ── -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
         <router-link to="/bgh/academic/pass-fail" class="surface-card border border-(--color-warning-text)/20 rounded-2xl p-4 bg-(--color-warning-bg) block group">
            <div class="flex items-start gap-4">
               <div class="h-10 w-10 rounded-2xl bg-(--surface-card) flex items-center justify-center text-(--color-warning-text) shadow-sm shrink-0">
                  <TrendingUp :size="20" />
               </div>
               <div>
                  <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Cảnh báo xu hướng rớt môn</h4>
                  <p class="text-xs text-(--color-warning-text) mt-1 leading-relaxed font-medium">
                    Tỷ lệ rớt môn (Fail) có dấu hiệu tăng <strong>2.4%</strong> so với kỳ trước, tập trung ở các môn cơ sở ngành như Cấu trúc dữ liệu và Toán rời rạc.
                  </p>
                  <span class="mt-4 text-[10px] font-semibold text-(--color-warning-text) uppercase tracking-widest flex items-center gap-1 group-hover:underline">
                     Xem chi tiết <ChevronRight :size="12" />
                  </span>
               </div>
            </div>
         </router-link>
         <router-link to="/bgh/academic/at-risk" class="surface-card border border-(--color-info-text)/20 rounded-2xl p-4 bg-(--color-info-bg) block group">
            <div class="flex items-start gap-4">
               <div class="h-10 w-10 rounded-2xl bg-(--surface-card) flex items-center justify-center text-(--color-info-text) shadow-sm shrink-0">
                  <BarChart3 :size="20" />
               </div>
               <div>
                  <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Tối ưu hóa GPA</h4>
                  <p class="text-xs text-(--color-info-text) mt-1 leading-relaxed font-medium">
                    Nhóm sinh viên năm 3 có sự bứt phá về GPA với mức tăng trung bình <strong>0.3</strong> điểm nhờ vào việc áp dụng các lớp học thực hành mới.
                  </p>
                  <span class="mt-4 text-[10px] font-semibold text-(--color-info-text) uppercase tracking-widest flex items-center gap-1 group-hover:underline">
                     Phân tích dữ liệu <ChevronRight :size="12" />
                  </span>
               </div>
            </div>
         </router-link>
      </div>

      <!-- ── Phân tích bổ sung ── -->
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div class="surface-card border border-card rounded-2xl p-5 text-center">
          <GraduationCap :size="28" class="text-(--color-info-text) mx-auto mb-3" />
          <p class="text-2xl font-bold text-heading">{{ kpis.length ? kpis[0].value : '0' }}</p>
          <p class="text-xs font-semibold text-muted uppercase tracking-widest mt-1">Tổng số sinh viên</p>
        </div>
        <div class="surface-card border border-card rounded-2xl p-5 text-center">
          <Users :size="28" class="text-(--color-success-text) mx-auto mb-3" />
          <p class="text-2xl font-bold text-heading">{{ totalTeachersVal ? totalTeachersVal.toLocaleString() : '—' }}</p>
          <p class="text-xs font-semibold text-muted uppercase tracking-widest mt-1">Giảng viên đang giảng dạy</p>
        </div>
        <div class="surface-card border border-card rounded-2xl p-5 text-center">
          <BookOpen :size="28" class="text-(--color-warning-text) mx-auto mb-3" />
          <p class="text-2xl font-bold text-heading">{{ totalClassesVal ? totalClassesVal.toLocaleString() : '—' }}</p>
          <p class="text-xs font-semibold text-muted uppercase tracking-widest mt-1">Lớp học phần đang mở</p>
        </div>
      </div>

    </div>
  </PageContainer>
</template>

<style scoped>
@media print {
  #print-container {
    padding: 0;
    color: #1e293b;
  }
  #print-container .surface-card {
    border: 1px solid #cbd5e1;
    background: #fff;
    box-shadow: none;
    break-inside: avoid;
  }
  #print-container table {
    font-size: 10px;
  }
  #print-container th {
    background: #f1f5f9;
    color: #475569;
  }
}
</style>

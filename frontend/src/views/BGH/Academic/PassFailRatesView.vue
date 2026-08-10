<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { 
  Search, 
  Filter, 
  ChevronRight, 
  XCircle, 
  BookOpen, 
  Users, 
  ArrowUpRight,
  TrendingDown,
  Download,
  FileText,
  ChevronDown,
  X,
  BarChart3,
  AlertCircle,
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
const searchQuery = ref('')
const showFilterDetail = ref(false)
const showDetailModal = ref(false)
const selectedCourse = ref(null)

const semesters = ref([{ value: 'all', label: 'Tất cả học kỳ' }])
const industries = ref([{ value: 'all', label: 'Tất cả Ngành' }])
const specializations = ref([])

const availableMajors = computed(() => {
  if (industryFilter.value === 'all') {
    return [{ value: 'all', label: 'Tất cả Chuyên ngành' }, ...specializations.value]
  }
  const filtered = specializations.value.filter(s => String(s.majorId || s.maNganh) === String(industryFilter.value))
  return [{ value: 'all', label: 'Tất cả Chuyên ngành' }, ...(filtered.length > 0 ? filtered : specializations.value)]
})

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const filterRes = await bghApi.getPassFailFilterOptions()
    const filterData = unwrapApiData(filterRes) || {}
    semesters.value = [
      { value: 'all', label: 'Tất cả học kỳ' },
      ...(filterData.semesters || []).map(item => ({ value: String(item.id), label: item.label })),
    ]
    if (filterData.majors && filterData.majors.length > 0) {
      industries.value = [
        { value: 'all', label: 'Tất cả Ngành' },
        ...filterData.majors.map(m => ({ value: String(m.id || m.maNganh), label: m.label || m.name || m.tenNganh || 'Ngành' }))
      ]
    }
    if (filterData.specializations && filterData.specializations.length > 0) {
      specializations.value = filterData.specializations.map(s => ({
        value: String(s.id || s.maChuyenNganh),
        label: s.label || s.name || s.tenChuyenNganh || 'Chuyên ngành',
        majorId: s.majorId || s.maNganh
      }))
    }

    const apiParams = {
      semesterId: semesterFilter.value === 'all' ? undefined : semesterFilter.value,
    }
    if (majorFilter.value !== 'all') {
      apiParams.specializationId = majorFilter.value
    } else if (industryFilter.value !== 'all') {
      apiParams.majorId = industryFilter.value
    }

    const res = await bghApi.getPassFailRates(apiParams)
    const data = unwrapApiData(res)
    if (data) {
      courseStats.value = (data.courseStats || []).map((c, i) => ({
        id: i + 1,
        subject: c.subjectName,
        class: c.classCode || 'Lớp HP',
        teacher: c.teacherName || 'Giáo viên phụ trách',
        total: c.total,
        pass: c.pass,
        fail: c.fail,
        failRate: c.failRate,
        reason: c.reason || 'Điểm tổng kết dưới ngưỡng đạt',
        dept: majorFilter.value !== 'all' ? majorFilter.value : industryFilter.value,
        avgGpa: c.avgGpa,
      }))
      overallPassRate.value = data.overallPassRate ?? 0
      trendData.value = (data.semesterTrend || []).map(item => ({
        k: item.semesterName || '',
        pass: Number(item.passRate || 0),
        fail: Number(item.failRate || 0),
      }))
    }
  } catch (e) {
    if (e.name === 'AbortError' || e.message?.includes('cancelled') || e.message?.includes('aborted')) return
    error.value = e.message
  } finally {
    loading.value = false
  }
}
onMounted(() => { loadData() })

const warningThreshold = ref(0)
const sortOrder = ref('fail-desc')

const courseStats = ref([])
const trendData = ref([])
const overallPassRate = ref(0)

watch(industryFilter, () => {
  majorFilter.value = 'all'
})

watch([semesterFilter, industryFilter, majorFilter], () => {
  loadData()
})

const filteredStats = computed(() => {
  let list = courseStats.value
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(s => s.subject.toLowerCase().includes(q) || s.class.toLowerCase().includes(q) || s.teacher.toLowerCase().includes(q))
  }
  if (warningThreshold.value > 0) {
    list = list.filter(s => s.failRate >= warningThreshold.value)
  }
  if (sortOrder.value === 'fail-desc') {
    list = [...list].sort((a, b) => b.failRate - a.failRate)
  } else if (sortOrder.value === 'fail-asc') {
    list = [...list].sort((a, b) => a.failRate - b.failRate)
  } else if (sortOrder.value === 'name-asc') {
    list = [...list].sort((a, b) => a.subject.localeCompare(b.subject))
  }
  return list
})

const avgPassRate = computed(() => {
  if (!filteredStats.value.length) return 0
  const total = filteredStats.value.reduce((sum, s) => sum + s.total, 0)
  const passed = filteredStats.value.reduce((sum, s) => sum + s.pass, 0)
  return total ? ((passed / total) * 100).toFixed(1) : 0
})

const worstCourse = computed(() => {
  if (!filteredStats.value.length) return null
  return [...filteredStats.value].sort((a, b) => b.failRate - a.failRate)[0]
})

const getFailRateColor = (rate) => {
  if (rate >= 20) return 'text-(--color-danger-text) bg-(--color-danger-bg) border-(--color-danger-text)/20'
  if (rate >= 10) return 'text-(--color-warning-text) bg-(--color-warning-bg) border-(--color-warning-text)/20'
  return 'text-(--color-success-text) bg-(--color-success-bg) border-(--color-success-text)/20'
}

const failCauses = ref([
  { label: 'Chuyên cần thấp (<50%)', val: 42, color: '--color-danger-text' },
  { label: 'Chưa hoàn thành Assignment', val: 28, color: '--color-warning-text' },
  { label: 'Điểm Thi Giữa Kỳ <4.0', val: 18, color: '--color-info-text' },
  { label: 'Điểm Thi Cuối Kỳ <4.0', val: 12, color: '--color-success-text' },
])

function viewCourseDetail(stat) {
  selectedCourse.value = stat
  showDetailModal.value = true
}

function prepareExcelData() {
  return filteredStats.value.map(s => ({
    'Môn học': s.subject,
    'Lớp': s.class,
    'Giảng viên': s.teacher,
    'Sĩ số': s.total,
    'Pass': s.pass,
    'Fail': s.fail,
    'Tỷ lệ rớt (%)': s.failRate,
    'Nguyên nhân': s.reason,
  }))
}

function exportExcel() {
  exportBghToExcel(prepareExcelData(), `TyLe-PassFail-${semesterFilter.value}.xlsx`, 'Pass/Fail')
}
</script>

<template>
  <PageContainer 
    title="Tỷ lệ Pass / Fail môn học" 
    subtitle="Theo dõi và phân tích tỷ lệ qua môn, rớt môn để đánh giá độ khó và chất lượng giảng dạy."
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
        <h2 class="text-xl font-bold text-slate-800">Tỷ lệ Pass / Fail môn học</h2>
        <p class="text-xs text-slate-500 mt-1">{{ semesters.find(s => s.value === semesterFilter)?.label }}</p>
      </div>

      <!-- ── Filters ── -->
      <div class="surface-card border border-card p-4 rounded-2xl flex flex-wrap items-center justify-between gap-4 print:hidden">
        <div class="flex items-center gap-4 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
              <input v-model="searchQuery" type="text" placeholder="Tìm môn học, lớp hoặc giảng viên..." class="w-full surface-input border border-input rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
           </div>
           <LmsSelect v-model="semesterFilter" class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
             <option v-for="s in semesters" :key="s.value" :value="s.value">{{ s.label }}</option>
           </LmsSelect>
           <LmsSelect v-model="industryFilter" class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
             <option v-for="i in industries" :key="i.value" :value="i.value">{{ i.label }}</option>
           </LmsSelect>
           <LmsSelect v-model="majorFilter" class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
             <option v-for="m in availableMajors" :key="m.value" :value="m.value">{{ m.label }}</option>
           </LmsSelect>
        </div>
        <button @click="showFilterDetail = !showFilterDetail" class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2" :class="{ 'bg-(--surface-input-hover) border-(--lg-primary)': showFilterDetail }">
           <Filter :size="18" /> Lọc nâng cao <ChevronDown :size="14" :class="{ 'rotate-180': showFilterDetail }" class="transition-transform" />
        </button>
      </div>

      <!-- ── Advanced Filter Panel ── -->
      <Transition name="fade-slide">
        <div v-if="showFilterDetail" class="surface-card border border-card rounded-2xl p-5 print:hidden shadow-md">
          <div class="flex items-center justify-between mb-4">
            <h4 class="text-xs font-semibold text-muted uppercase tracking-widest flex items-center gap-2"><Filter :size="14"/> Tùy chọn lọc nâng cao</h4>
            <button @click="showFilterDetail = false" class="p-1.5 hover:bg-(--surface-input) rounded-lg text-placeholder transition-colors">
              <X :size="16" />
            </button>
          </div>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label class="block text-[10px] font-semibold text-muted uppercase tracking-widest mb-1.5">Ngưỡng cảnh báo tỷ lệ rớt</label>
              <LmsSelect v-model.number="warningThreshold" class="w-full surface-input border border-input rounded-xl px-3 py-2.5 text-xs font-semibold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
                <option :value="0">Tất cả ngưỡng tỷ lệ</option>
                <option :value="10">&ge; 10% tỷ lệ rớt</option>
                <option :value="20">&ge; 20% tỷ lệ rớt</option>
                <option :value="30">&ge; 30% tỷ lệ rớt</option>
              </LmsSelect>
            </div>
            <div>
              <label class="block text-[10px] font-semibold text-muted uppercase tracking-widest mb-1.5">Tiêu chí sắp xếp danh sách</label>
              <LmsSelect v-model="sortOrder" class="w-full surface-input border border-input rounded-xl px-3 py-2.5 text-xs font-semibold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
                <option value="fail-desc">Tỷ lệ rớt giảm dần (Cao nhất trước)</option>
                <option value="fail-asc">Tỷ lệ rớt tăng dần (Thấp nhất trước)</option>
                <option value="name-asc">Tên môn học A-Z</option>
              </LmsSelect>
            </div>
          </div>
          <div class="flex justify-end gap-2 mt-4 pt-3 border-t border-default">
            <button @click="warningThreshold = 0; sortOrder = 'fail-desc'" class="lg-button-secondary px-4 py-2 text-xs font-bold rounded-xl">Đặt lại mặc định</button>
            <button @click="showFilterDetail = false" class="lg-button-primary px-5 py-2 text-xs font-bold rounded-xl">Đóng bảng lọc</button>
          </div>
        </div>
      </Transition>

      <!-- ── Stats Summary ── -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
         <div class="surface-card border border-(--color-danger-text)/20 rounded-2xl p-4 bg-(--color-danger-bg) flex items-center gap-5">
            <div class="h-10 w-10 rounded-2xl bg-(--surface-card) flex items-center justify-center text-(--color-danger-text) shadow-sm border border-(--color-danger-text)/20">
               <TrendingDown :size="24" />
            </div>
            <div>
               <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Môn học tỷ lệ rớt cao nhất</h4>
               <p class="text-xs text-(--color-danger-text) mt-1 font-bold">
                 {{ worstCourse ? `${worstCourse.subject} (${worstCourse.failRate}% Fail)` : '—' }}
               </p>
            </div>
         </div>
         <div class="surface-card border border-(--color-success-text)/20 rounded-2xl p-4 bg-(--color-success-bg) flex items-center gap-5">
            <div class="h-10 w-10 rounded-2xl bg-(--surface-card) flex items-center justify-center text-(--color-success-text) shadow-sm border border-(--color-success-text)/20">
               <ArrowUpRight :size="24" />
            </div>
            <div>
               <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Tỷ lệ Pass trung bình</h4>
               <p class="text-xs text-(--color-success-text) mt-1 font-bold">
                 {{ filteredStats.length ? avgPassRate : overallPassRate }}%
               </p>
            </div>
         </div>
      </div>

      <!-- ── Pass/Fail Trend Chart ── -->
      <div v-if="trendData.length" class="surface-card border border-card rounded-2xl p-5">
        <div class="flex items-center justify-between mb-6">
          <div>
            <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Xu hướng Pass/Fail qua các kỳ</h4>
            <p class="text-xs text-muted mt-1 font-bold">Tỷ lệ % Pass và Fail qua 5 kỳ gần nhất</p>
          </div>
          <div class="flex items-center gap-2 text-xs">
            <span class="inline-flex items-center gap-1.5"><span class="h-2.5 w-2.5 rounded-sm bg-(--color-success-text)"></span> Pass</span>
            <span class="inline-flex items-center gap-1.5"><span class="h-2.5 w-2.5 rounded-sm bg-(--color-danger-text)"></span> Fail</span>
          </div>
        </div>
        <div class="h-64 flex items-end justify-between gap-3 px-4 relative">
          <div class="absolute inset-0 flex flex-col justify-between pointer-events-none">
            <div v-for="i in 4" :key="i" class="h-px w-full border-t border-dashed border-(--border-default)/40"></div>
          </div>
          <div v-for="item in trendData" :key="item.k" class="flex-1 h-full flex flex-col items-center justify-end gap-1.5 z-10">
            <div class="w-full flex items-end justify-center gap-1.5 relative h-48">
              <!-- Cột Pass -->
              <div 
                :style="{ height: item.pass + '%' }" 
                class="w-4 rounded-t-md bg-gradient-to-t from-emerald-600 to-emerald-400 transition-all duration-500 hover:opacity-80"
                :title="`Pass: ${item.pass}%`"
              ></div>
              <!-- Cột Fail -->
              <div 
                :style="{ height: Math.max(item.fail, 5) + '%' }" 
                class="w-4 rounded-t-md bg-gradient-to-t from-rose-600 to-rose-400 transition-all duration-500 hover:opacity-80"
                :title="`Fail: ${item.fail}%`"
              ></div>
            </div>
            <p class="text-[10px] font-semibold text-muted uppercase">{{ item.k.replace('Kỳ ', '').trim() }}</p>
            <div class="flex items-center gap-2 text-[9px] font-bold leading-tight">
              <span class="text-(--color-success-text)">{{ item.pass }}%</span>
              <span class="text-(--color-danger-text)">{{ item.fail }}%</span>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Môn học & Lớp</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Giảng viên</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Sĩ số / Pass / Fail</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Tỷ lệ rớt</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Nguyên nhân chính</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="stat in filteredStats" :key="stat.id" class="group hover:bg-(--surface-input) transition-colors">
              <td class="px-4 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-xl bg-(--color-info-bg) flex items-center justify-center text-(--color-info-text) border border-(--color-info-text)/20">
                    <BookOpen :size="18" />
                  </div>
                  <div>
                    <p class="text-sm font-semibold text-heading leading-tight">{{ stat.subject }}</p>
                    <p class="text-[11px] font-bold text-muted mt-0.5">{{ stat.class || '—' }}</p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-2">
                   <Users :size="14" class="text-placeholder" />
                   <span class="text-xs font-bold text-label">{{ stat.teacher || '—' }}</span>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex flex-col gap-1">
                   <div class="flex items-center gap-2">
                      <Users :size="12" class="text-placeholder" />
                      <span class="text-[10px] font-semibold text-muted">{{ stat.total }} SV</span>
                   </div>
                   <div class="flex items-center gap-3">
                      <span class="text-[10px] font-semibold text-(--color-success-text)">{{ stat.pass }} Pass</span>
                      <span class="text-[10px] font-semibold text-(--color-danger-text)">{{ stat.fail }} Fail</span>
                      <span class="text-[10px] font-semibold text-muted">({{ ((stat.fail / stat.total) * 100).toFixed(1) }}%)</span>
                   </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <div :class="['px-2.5 py-1 rounded-lg text-[10px] font-semibold uppercase tracking-widest border w-fit shadow-sm', getFailRateColor(stat.failRate)]">
                  {{ stat.failRate }}%
                </div>
              </td>
              <td class="px-4 py-4 max-w-[200px]">
                 <p class="text-[11px] text-muted font-medium leading-relaxed italic">{{ stat.reason || '—' }}</p>
              </td>
              <td class="px-4 py-4 text-right">
                <button @click="viewCourseDetail(stat)" class="p-2 hover:bg-(--color-info-bg) hover:text-link rounded-lg text-placeholder transition-all" title="Xem chi tiết">
                  <ChevronRight :size="18" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="filteredStats.length === 0" class="py-12 text-center">
          <XCircle :size="36" class="text-placeholder mx-auto mb-3" />
          <p class="text-xs font-semibold text-muted">Không tìm thấy môn học phù hợp</p>
          <button @click="searchQuery = ''; departmentFilter = 'all'" class="mt-2 text-[11px] font-bold text-link underline-offset-2 hover:underline">Xoá bộ lọc</button>
        </div>
      </div>

      <!-- ── Failure Causes Summary ── -->
      <div class="surface-card border border-card rounded-2xl p-6">
         <h4 class="text-xs font-semibold text-muted uppercase tracking-widest mb-5 flex items-center gap-2">
            <BarChart3 :size="16" /> Phân tích nguyên nhân rớt môn chính
         </h4>
         <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
            <div v-for="cause in failCauses" :key="cause.label" class="p-4 surface-solid rounded-2xl border border-default">
               <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mb-2">{{ cause.label }}</p>
               <div class="flex items-center gap-3">
                  <h3 class="text-2xl font-bold" :style="{ color: `var(${cause.color})` }">{{ cause.val }}%</h3>
                  <div class="h-2 flex-1 bg-(--surface-input) rounded-full overflow-hidden">
                     <div :style="{ width: `${cause.val}%`, background: `var(${cause.color})` }" class="h-full rounded-full transition-all duration-700"></div>
                  </div>
               </div>
            </div>
         </div>
      </div>
      
      <!-- ── Advanced Filter Panel ── -->
      <Transition name="fade-slide">
        <div v-if="showFilterDetail" class="surface-card border border-card rounded-2xl p-5 print:hidden">
          <div class="flex items-center justify-between mb-4">
            <h4 class="text-xs font-semibold text-muted uppercase tracking-widest">Bộ lọc nâng cao</h4>
            <button @click="showFilterDetail = false" class="p-1.5 hover:bg-(--surface-input) rounded-lg text-placeholder transition-colors">
              <X :size="16" />
            </button>
          </div>
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div>
              <label class="block text-[10px] font-semibold text-muted uppercase tracking-widest mb-1.5">Ngưỡng cảnh báo rớt</label>
              <LmsSelect v-model.number="warningThreshold" class="w-full surface-input border border-input rounded-xl px-3 py-2.5 text-xs font-semibold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
                <option :value="0">Tất cả</option>
                <option :value="10">&ge; 10%</option>
                <option :value="20">&ge; 20%</option>
                <option :value="30">&ge; 30%</option>
              </LmsSelect>
            </div>
            <div>
              <label class="block text-[10px] font-semibold text-muted uppercase tracking-widest mb-1.5">Sắp xếp</label>
              <LmsSelect v-model="sortOrder" class="w-full surface-input border border-input rounded-xl px-3 py-2.5 text-xs font-semibold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
                <option value="fail-desc">Tỷ lệ rớt giảm dần</option>
                <option value="fail-asc">Tỷ lệ rớt tăng dần</option>
                <option value="name-asc">Tên A-Z</option>
              </LmsSelect>
            </div>
          </div>
          <div class="flex justify-end gap-2 mt-4">
            <button @click="warningThreshold = 0; sortOrder = 'fail-desc'; showFilterDetail = false" class="lg-button-secondary px-4 py-2 text-xs font-bold rounded-xl">Đặt lại</button>
            <button @click="showFilterDetail = false" class="lg-button-primary px-5 py-2 text-xs font-bold rounded-xl">Áp dụng</button>
          </div>
        </div>
      </Transition>

    </div>

    <!-- ── Detail Modal ── -->
    <Teleport to="body">
      <Transition name="modal">
        <div v-if="showDetailModal && selectedCourse" class="fixed inset-0 z-(--z-modal) flex items-center justify-center p-4" role="dialog" aria-modal="true" @keydown.esc="showDetailModal = false">
          <div class="absolute inset-0 bg-(--surface-app)/60 backdrop-blur-sm" @click="showDetailModal = false" />
          <div class="surface-card rounded-2xl border border-card shadow-2xl w-full max-w-md relative overflow-hidden">
            <div class="flex items-start justify-between p-5 pb-0">
              <div>
                <p class="text-[10px] text-muted uppercase tracking-widest font-semibold">Chi tiết môn học</p>
                <h3 class="text-lg font-bold text-heading mt-0.5">{{ selectedCourse.subject }} <span class="text-sm font-semibold text-muted">({{ selectedCourse.class || '—' }})</span></h3>
              </div>
              <button @click="showDetailModal = false" class="p-1.5 hover:bg-(--surface-input) rounded-lg text-placeholder transition-colors">
                <X :size="18" />
              </button>
            </div>
            <div class="p-5 space-y-4">
              <div class="grid grid-cols-2 gap-3">
                <div class="surface-solid rounded-xl p-3 text-center">
                  <p class="text-[10px] text-muted uppercase tracking-wide">Sĩ số</p>
                  <p class="text-xl font-bold text-heading">{{ selectedCourse.total }}</p>
                </div>
                <div class="surface-solid rounded-xl p-3 text-center">
                  <p class="text-[10px] text-muted uppercase tracking-wide">Pass</p>
                  <p class="text-xl font-bold text-(--color-success-text)">{{ selectedCourse.pass }}</p>
                </div>
                <div class="surface-solid rounded-xl p-3 text-center">
                  <p class="text-[10px] text-muted uppercase tracking-wide">Fail</p>
                  <p class="text-xl font-bold text-(--color-danger-text)">{{ selectedCourse.fail }}</p>
                </div>
                <div class="surface-solid rounded-xl p-3 text-center">
                  <p class="text-[10px] text-muted uppercase tracking-wide">Tỷ lệ rớt</p>
                  <p class="text-xl font-bold" :style="{ color: `var(${selectedCourse.failRate >= 20 ? '--color-danger-text' : selectedCourse.failRate >= 10 ? '--color-warning-text' : '--color-success-text'})` }">{{ selectedCourse.failRate }}%</p>
                </div>
              </div>
              <div class="surface-solid rounded-xl p-3">
                <p class="text-[10px] text-muted uppercase tracking-wide mb-1">Nguyên nhân chính</p>
                <p class="text-sm font-semibold text-heading">{{ selectedCourse.reason || '—' }}</p>
              </div>
              <div class="flex items-center gap-2 text-xs text-muted pt-1">
                <Users :size="14" />
                <span class="font-semibold">{{ selectedCourse.teacher || '—' }}</span>
              </div>
            </div>
            <div class="px-5 pb-5">
              <button @click="showDetailModal = false" class="lg-button-primary w-full py-2.5 text-sm font-bold rounded-xl">Đóng</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </PageContainer>
</template>

<style scoped>
@media print {
  #print-container { padding: 0; color: #1e293b; }
  #print-container .surface-card { border: 1px solid #cbd5e1; background: #fff; box-shadow: none; break-inside: avoid; }
  #print-container table { font-size: 10px; }
  #print-container th { background: #f1f5f9; color: #475569; }
}

.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: all 0.25s ease-out;
}
.fade-slide-enter-from,
.fade-slide-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

.modal-enter-active,
.modal-leave-active {
  transition: all 0.2s ease-out;
}
.modal-enter-active > div:last-child,
.modal-leave-active > div:last-child {
  transition: all 0.2s ease-out;
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
.modal-enter-from > div:last-child,
.modal-leave-to > div:last-child {
  opacity: 0;
  transform: scale(0.95) translateY(8px);
}
</style>

<script setup>
import { ref, computed, onMounted } from 'vue'
import SkeletonDashboard from '@/components/common/skeleton/SkeletonDashboard.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { 
  Search, Trophy, TrendingUp, TrendingDown, Minus, Star, ChevronRight, ShieldCheck, Building2,
  AlertCircle, Sparkles, Loader2
} from 'lucide-vue-next'
import { useRouter, useRoute } from 'vue-router'
import BghAiReportModal from '@/components/BGH/BghAiReportModal.vue'
import { bghApi } from '@/services/bghApi'
import { aiApi } from '@/services/aiApi'
import { unwrapApiData } from '@/services/apiClient'

const router = useRouter()
const route = useRoute()
const searchQuery = ref('')
const industryFilter = ref('all')
const majorFilter = ref('all')
const ratingFilter = ref('all')
const rankings = ref([])
const semesters = ref([])
const industryOptions = ref([{ value: 'all', label: 'Tất cả Ngành' }])
const specializationOptions = ref([])
const loading = ref(false)
const error = ref(null)

// AI Strategic Report State
const aiModalOpen = ref(false)
const aiLoading = ref(false)
const aiError = ref(null)
const aiReport = ref(null)

async function triggerTeacherRankingAiAnalysis() {
  aiModalOpen.value = true
  aiLoading.value = true
  aiError.value = null
  try {
    const res = await aiApi.generateBghReport({
      reportType: 'teacher_eval',
      mode: 'deep',
      forceRefresh: true,
    })
    aiReport.value = res
  } catch (err) {
    aiError.value = err.message || 'Không thể phân tích xếp hạng giảng viên AI.'
  } finally {
    aiLoading.value = false
  }
}

const availableMajors = computed(() => {
  if (industryFilter.value === 'all') {
    return [{ value: 'all', label: 'Tất cả Chuyên ngành' }, ...specializationOptions.value]
  }
  const filtered = specializationOptions.value.filter(s => String(s.majorId || s.maNganh) === String(industryFilter.value))
  return [{ value: 'all', label: 'Tất cả Chuyên ngành' }, ...(filtered.length > 0 ? filtered : specializationOptions.value)]
})

const ratingOptions = [
  { value: 'all', label: 'Tất cả mức sao (1-5★)' },
  { value: '5_star', label: '5 sao (4.5 – 5.0 ★)' },
  { value: '4_star', label: '4 sao (4.0 – 4.4 ★)' },
  { value: '3_star', label: '3 sao (3.0 – 3.9 ★)' },
  { value: '2_star', label: '2 sao (2.0 – 2.9 ★)' },
  { value: '1_star', label: '1 sao (< 2.0 ★)' },
  { value: 'warning', label: 'Cảnh báo (Dưới 3.5 ★)' }
]

const filteredRankings = computed(() => {
  let list = rankings.value
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(gv => gv.name.toLowerCase().includes(q) || gv.dept.toLowerCase().includes(q))
  }
  if (majorFilter.value !== 'all') {
    list = list.filter(gv => String(gv.deptId) === String(majorFilter.value))
  } else if (industryFilter.value !== 'all') {
    const validSpecIds = specializationOptions.value
      .filter(s => String(s.majorId || s.maNganh) === String(industryFilter.value))
      .map(s => String(s.value))
    
    if (validSpecIds.length > 0) {
      list = list.filter(gv => validSpecIds.includes(String(gv.deptId)))
    } else {
      list = []
    }
  }
  if (ratingFilter.value === '5_star') {
    list = list.filter(gv => gv.avgScore >= 4.5)
  } else if (ratingFilter.value === '4_star') {
    list = list.filter(gv => gv.avgScore >= 4.0 && gv.avgScore < 4.5)
  } else if (ratingFilter.value === '3_star') {
    list = list.filter(gv => gv.avgScore >= 3.0 && gv.avgScore < 4.0)
  } else if (ratingFilter.value === '2_star') {
    list = list.filter(gv => gv.avgScore >= 2.0 && gv.avgScore < 3.0)
  } else if (ratingFilter.value === '1_star') {
    list = list.filter(gv => gv.avgScore < 2.0)
  } else if (ratingFilter.value === 'warning') {
    list = list.filter(gv => gv.avgScore < 3.5)
  }
  return list
})

const getTrendIcon = (trend) => {
  if (trend === 'up') return TrendingUp
  if (trend === 'down') return TrendingDown
  return Minus
}

const getTrendColor = (trend) => {
  if (trend === 'up') return 'text-(--color-success-text)'
  if (trend === 'down') return 'text-(--color-danger-text)'
  return 'text-muted'
}

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const [rankingRes, overviewRes, filterOptionsRes] = await Promise.all([
      bghApi.getEvaluationRanking().catch(() => null),
      bghApi.getEvaluationOverview().catch(() => null),
      bghApi.getPassFailFilterOptions().catch(() => null)
    ])
    const data = unwrapApiData(rankingRes)
    const overview = unwrapApiData(overviewRes) || {}
    const filterOptions = unwrapApiData(filterOptionsRes) || {}

    rankings.value = Array.isArray(data)
      ? data.map(item => ({
          id: item.teacherId ?? item.id,
          name: item.teacherName || item.name || '',
          dept: item.departmentName || item.dept || 'Chưa phân khoa',
          deptId: item.departmentId ?? item.deptId,
          avgScore: Number(item.avgRating ?? item.avgScore ?? 0),
          evals: item.reviewCount ?? item.evals ?? 0,
          positive: item.positive ?? 0,
          negative: item.negative ?? 0,
          trend: item.trend || 'stable'
        }))
      : []

    if (filterOptions.majors && Array.isArray(filterOptions.majors)) {
      industryOptions.value = [
        { value: 'all', label: 'Tất cả Ngành' },
        ...filterOptions.majors.map(m => ({
          value: String(m.id || m.maNganh),
          label: m.label || m.name || m.tenNganh || `Ngành ${m.id}`
        }))
      ]
    }
    if (filterOptions.specializations && Array.isArray(filterOptions.specializations)) {
      specializationOptions.value = filterOptions.specializations.map(s => ({
        value: String(s.id || s.maChuyenNganh),
        label: s.label || s.name || s.tenChuyenNganh || 'Chuyên ngành',
        majorId: s.majorId || s.maNganh
      }))
    }

    semesters.value = (overview.semesterTrend || []).map(item => item.semester).filter(Boolean)
  } catch (e) {
    error.value = e?.message || 'Không thể tải dữ liệu xếp hạng'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  if (route.query.filter === 'warning') {
    ratingFilter.value = 'warning'
  }
  loadData()
})

function viewDetail(gv) {
  router.push(`/bgh/evaluations/detail/${gv.id}`)
}
</script>

<template>
  <div class="space-y-4">
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
    <div v-else class="space-y-4">
      
      <div class="surface-card border border-(--color-info-text)/20 bg-(--color-info-bg) rounded-2xl p-5 flex items-center gap-4">
         <div class="h-10 w-10 rounded-2xl bg-(--surface-card) flex items-center justify-center text-(--color-info-text) shrink-0 border border-(--color-info-text)/20">
            <ShieldCheck :size="20" />
         </div>
         <p class="text-xs text-(--color-info-text) font-medium leading-relaxed">
           <strong>Quy tắc xếp hạng:</strong> Chỉ hiển thị kết quả cho các giảng viên có từ <strong>5 lượt đánh giá</strong> trở lên. Danh tính sinh viên được ẩn danh.
         </p>
      </div>

      <div class="surface-card border border-card rounded-2xl p-4 flex flex-wrap items-center justify-between gap-3">
        <div class="flex flex-wrap items-center gap-3 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
              <input v-model="searchQuery" type="text" placeholder="Tìm tên giảng viên hoặc khoa..." class="w-full surface-input border border-input rounded-xl pl-9 pr-4 py-2 text-sm font-medium outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
           </div>
           <LmsSelect v-model="industryFilter" :options="industryOptions" class="w-44 surface-input border border-input rounded-xl px-3 py-2 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)" />
           <LmsSelect v-model="majorFilter" :options="availableMajors" class="w-44 surface-input border border-input rounded-xl px-3 py-2 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)" />
           <LmsSelect v-model="ratingFilter" :options="ratingOptions" class="w-48 surface-input border border-input rounded-xl px-3 py-2 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)" />
        </div>
         <button
           @click="triggerTeacherRankingAiAnalysis"
           :disabled="aiLoading"
           class="px-5 py-2.5 rounded-xl bg-gradient-to-r from-blue-600 via-indigo-600 to-indigo-700 hover:from-blue-700 hover:to-indigo-800 text-white text-xs font-bold shadow-md shadow-indigo-500/20 flex items-center gap-2 transition-all active:scale-95 disabled:opacity-60 cursor-pointer shrink-0"
         >
           <Sparkles v-if="!aiLoading" :size="15" />
           <Loader2 v-else :size="15" class="animate-spin" />
           <span>{{ aiLoading ? 'ĐANG PHÂN TÍCH...' : 'PHÂN TÍCH BẰNG AI' }}</span>
         </button>
      </div>

      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest w-16 text-center">Hạng</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest">Giảng viên & Khoa</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest">Điểm Rating</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest">Phản hồi (Sentiment)</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest">Xu hướng</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(gv, index) in filteredRankings" :key="gv.id" class="group hover:bg-(--surface-input) transition-colors cursor-pointer" @click="viewDetail(gv)">
              <td class="px-4 py-3 text-center">
                 <div v-if="index < 3" class="flex justify-center">
                    <div :class="['h-8 w-8 rounded-full flex items-center justify-center shadow-sm border', index === 0 ? 'bg-(--color-warning-bg) text-(--color-warning-text) border-(--color-warning-text)/20' : index === 1 ? 'surface-solid text-muted border-default' : 'bg-(--color-info-bg) text-(--color-info-text) border-(--color-info-text)/20']">
                       <Trophy :size="16" />
                    </div>
                 </div>
                 <span v-else class="text-sm font-semibold text-muted">#{{ index + 1 }}</span>
              </td>
              <td class="px-4 py-3">
                <div class="flex items-center gap-3">
                  <div class="h-10 w-10 rounded-full surface-solid border border-default flex items-center justify-center font-semibold text-[10px] text-muted">GV</div>
                  <div>
                    <p class="text-sm font-semibold text-heading leading-tight group-hover:text-link transition-colors">{{ gv.name }}</p>
                    <p class="text-[10px] font-bold text-muted mt-1 flex items-center gap-1 uppercase tracking-tighter">
                       <Building2 :size="12" /> {{ gv.dept }}
                    </p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-3">
                 <div class="flex items-center gap-1.5">
                    <Star :size="14" class="text-(--color-warning-text) fill-(--color-warning-text)" />
                    <span class="text-sm font-semibold text-heading">{{ gv.avgScore.toFixed(2) }}</span>
                    <span class="text-[10px] font-bold text-muted ml-1">({{ gv.evals }} lượt)</span>
                 </div>
              </td>
              <td class="px-4 py-3">
                 <div class="flex flex-col gap-1 w-32">
                    <div class="flex justify-between text-[9px] font-semibold uppercase tracking-widest">
                       <span class="text-(--color-success-text)">{{ gv.positive }}% Pos</span>
                       <span class="text-(--color-danger-text)">{{ gv.negative }}% Neg</span>
                    </div>
                    <div class="h-1.5 w-full bg-(--surface-input) rounded-full overflow-hidden flex">
                       <div :style="{ width: `${gv.positive}%` }" class="bg-(--color-success-text) h-full"></div>
                       <div :style="{ width: `${gv.negative}%` }" class="bg-(--color-danger-text) h-full"></div>
                    </div>
                 </div>
              </td>
              <td class="px-4 py-3">
                 <div :class="['flex items-center gap-1.5', getTrendColor(gv.trend)]">
                    <component :is="getTrendIcon(gv.trend)" :size="16" />
                    <span class="text-[10px] font-semibold uppercase tracking-widest">{{ gv.trend }}</span>
                 </div>
              </td>
              <td class="px-4 py-3 text-right">
                <button @click.stop="viewDetail(gv)" class="p-2 hover:bg-(--color-info-bg) hover:text-(--color-info-text) rounded-lg text-muted transition-all">
                  <ChevronRight :size="18" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="filteredRankings.length === 0" class="py-12 text-center">
          <Trophy :size="36" class="text-placeholder mx-auto mb-3" />
          <p class="text-xs font-semibold text-muted">Không tìm thấy giảng viên phù hợp</p>
        </div>
      </div>

    </div>

    <!-- AI Strategic Report Modal -->
    <BghAiReportModal
      :is-open="aiModalOpen"
      title="Báo Cáo Phân Tích & Xếp Hạng Giảng Viên AI (Qwen 9B)"
      subtitle="Đánh giá năng lực sư phạm, mức độ hài lòng từ người học và khuyến nghị bồi dưỡng chuyên môn"
      :scope-badges="['Xếp hạng Giảng viên', 'Bộ lọc: Đánh giá sinh viên', 'Mô hình: Qwen 9B Deep']"
      :loading="aiLoading"
      :error="aiError"
      :report-content="aiReport?.aiAnalysis"
      :generated-at="aiReport?.generatedAt"
      @close="aiModalOpen = false"
      @retry="triggerTeacherRankingAiAnalysis"
    />
  </div>
</template>

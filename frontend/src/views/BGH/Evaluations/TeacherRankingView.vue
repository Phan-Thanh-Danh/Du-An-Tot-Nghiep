<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  Search, Filter, Trophy, TrendingUp, TrendingDown, Minus, Star, ChevronRight, ShieldCheck, Building2,
  AlertCircle, Loader2
} from 'lucide-vue-next'
import { useRouter } from 'vue-router'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const router = useRouter()
const ENABLE_MOCK_API = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'
const loading = ref(false)
const error = ref(null)
const searchQuery = ref('')
const deptFilter = ref('all')

const rankings = ref([
  { id: 1, name: 'Nguyễn Văn A', dept: 'Khoa CNTT', evals: 86, avgScore: 4.92, positive: 96, negative: 1, trend: 'up' },
  { id: 2, name: 'Trần Thị B', dept: 'Khoa Ngoại ngữ', evals: 42, avgScore: 4.85, positive: 94, negative: 2, trend: 'stable' },
  { id: 3, name: 'Lê Văn C', dept: 'Khoa Kinh tế', evals: 120, avgScore: 4.78, positive: 90, negative: 4, trend: 'up' },
  { id: 4, name: 'Hoàng Thị D', dept: 'Khoa CNTT', evals: 35, avgScore: 4.70, positive: 88, negative: 5, trend: 'down' },
  { id: 5, name: 'Phạm Minh E', dept: 'Khoa Kinh tế', evals: 58, avgScore: 4.65, positive: 85, negative: 6, trend: 'stable' },
  { id: 6, name: 'Trương Văn F', dept: 'Khoa Ngoại ngữ', evals: 47, avgScore: 4.55, positive: 82, negative: 7, trend: 'up' },
  { id: 7, name: 'Lý Thị G', dept: 'Khoa CNTT', evals: 63, avgScore: 4.42, positive: 79, negative: 9, trend: 'stable' },
  { id: 8, name: 'Đặng Minh H', dept: 'Khoa Kinh tế', evals: 29, avgScore: 4.35, positive: 76, negative: 10, trend: 'down' },
])

const filteredRankings = computed(() => {
  let list = rankings.value
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(gv => gv.name.toLowerCase().includes(q) || gv.dept.toLowerCase().includes(q))
  }
  if (deptFilter.value !== 'all') {
    list = list.filter(gv => gv.dept === deptFilter.value)
  }
  return list
})

const departments = computed(() => ['Tất cả khoa', ...new Set(rankings.value.map(gv => gv.dept))])

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
    if (!ENABLE_MOCK_API) {
      const res = await bghApi.getEvaluations()
      const data = unwrapApiData(res)
      if (data && data.length) rankings.value = data
    }
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}
onMounted(() => { loadData() })

function viewDetail(gv) {
  router.push(`/bgh/evaluations/detail/GV00${gv.id}`)
}
</script>

<template>
  <div class="space-y-4">
    <div v-if="loading" class="flex items-center justify-center py-20">
      <Loader2 :size="32" class="animate-spin text-placeholder" />
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
           <select v-model="deptFilter" class="surface-input border border-input rounded-xl px-3 py-2 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
              <option v-for="d in departments" :key="d" :value="d === 'Tất cả khoa' ? 'all' : d">{{ d }}</option>
           </select>
        </div>
        <select class="surface-input border border-input rounded-xl px-3 py-2 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
           <option>Kỳ Spring 2026</option>
           <option>Kỳ Fall 2025</option>
        </select>
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
  </div>
</template>

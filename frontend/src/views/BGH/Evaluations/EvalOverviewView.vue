<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  Users, Star, MessageCircle, ShieldAlert, PieChart, ChevronRight, CheckCircle2,
  AlertCircle, Loader2
} from 'lucide-vue-next'
import { useRouter } from 'vue-router'
import { usePopupStore } from '@/stores/popup'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const router = useRouter()
const popup = usePopupStore()
const loading = ref(false)
const error = ref(null)

const kpis = ref([])
const sentiment = ref([])
const trendHistory = ref([])

const maxTrend = computed(() => {
  const arr = trendHistory.value.map(t => t.val)
  return Math.max(...arr, 5)
})

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const res = await bghApi.getEvaluationOverview()
    const data = unwrapApiData(res)
    if (data) {
      kpis.value = data.kpis || data.Kpis || []
      sentiment.value = data.sentiment || data.Sentiment || []
      trendHistory.value = data.trendHistory || data.TrendHistory || data.trend || []
    }
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}
onMounted(() => { loadData() })

function viewWarningList() {
  router.push('/bgh/evaluations/ranking')
}
</script>

<template>
  <div class="space-y-8">
    <div v-if="loading" class="flex items-center justify-center py-20">
      <Loader2 :size="32" class="animate-spin text-placeholder" />
    </div>
    <div v-else-if="error" class="flex flex-col items-center justify-center py-20 text-center">
      <AlertCircle :size="48" class="text-(--color-danger-text) mb-4" />
      <p class="text-lg font-semibold text-muted">Đã có lỗi xảy ra</p>
      <p class="text-sm text-placeholder mt-1">{{ error }}</p>
      <button @click="loadData" class="mt-4 lg-button-secondary px-4 py-2 text-sm font-semibold">Thử lại</button>
    </div>
    <div v-else class="space-y-8">
      
      <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
        <div v-for="kpi in kpis" :key="kpi.id" class="surface-card border border-card rounded-2xl p-4 group hover:border-(--border-input-focus) transition-all">
           <div class="flex items-center justify-between mb-4">
              <div :class="['h-10 w-10 rounded-2xl flex items-center justify-center shadow-sm border border-default', kpi.bgColor, kpi.color]">
                 <component :is="kpi.icon" :size="24" />
              </div>
              <span class="text-[10px] font-semibold uppercase tracking-widest text-muted">{{ kpi.trend }}</span>
           </div>
           <p class="text-xs font-semibold text-muted uppercase tracking-widest">{{ kpi.label }}</p>
           <h3 class="text-2xl font-bold text-heading mt-1">{{ kpi.value }}</h3>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        <div class="lg:col-span-2 surface-card border border-card rounded-2xl p-5 relative overflow-hidden">
           <div class="flex items-center justify-between mb-8">
              <div>
                 <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Xu hướng điểm Rating trung bình</h4>
                 <p class="text-xs text-muted mt-1 font-bold">Dữ liệu tổng hợp qua các học kỳ</p>
              </div>
           </div>

           <div class="h-64 flex items-end justify-between gap-4 px-4">
              <div v-for="h in trendHistory" :key="h.label" class="flex-1 group relative">
                 <div :style="{ height: `${(h.val / maxTrend) * 100}%` }" 
                      class="w-full bg-gradient-to-t from-(--color-warning-text) to-(--color-warning-text)/60 rounded-t-2xl shadow-sm opacity-80 group-hover:opacity-100 transition-all duration-500 cursor-pointer"
                      @click="popup.info(h.label, `Rating: ${h.val}`)"></div>
                 <p class="text-center text-[10px] font-semibold text-muted uppercase mt-4">{{ h.label.replace('Kỳ ', '') }}</p>
                  <div class="absolute -top-8 left-1/2 -translate-x-1/2 surface-modal text-heading border border-default text-[10px] font-bold px-2 py-1 rounded opacity-100">
                     {{ h.val }} ★
                   </div>
              </div>
           </div>
        </div>

        <div class="surface-card border border-card rounded-2xl p-8">
           <h4 class="text-sm font-semibold text-heading uppercase tracking-wide mb-8">Phân tích Sentiment AI</h4>
           <div class="space-y-8">
              <div v-for="item in sentiment" :key="item.label">
                 <div class="flex items-center justify-between mb-2">
                    <span class="text-xs font-semibold text-label uppercase tracking-tighter">{{ item.label }}</span>
                    <span class="text-xs font-semibold text-heading">{{ item.value }}%</span>
                 </div>
                 <div class="h-2 w-full bg-(--surface-input) rounded-full overflow-hidden">
                    <div :style="{ width: `${item.value}%` }" :class="['h-full rounded-full transition-all duration-1000', item.color]"></div>
                 </div>
                 <p class="text-[10px] text-muted mt-2 italic font-medium leading-tight">{{ item.desc }}</p>
              </div>
           </div>

           <div class="mt-10 pt-8">
              <div class="flex items-center gap-4 text-xs font-bold text-muted">
                 <PieChart :size="18" class="text-link" />
                 <p>Hơn <strong>93%</strong> phản hồi ở mức tích cực và trung lập.</p>
              </div>
           </div>
        </div>

      </div>

      <div class="surface-card border border-(--color-danger-text)/20 bg-(--color-danger-bg) rounded-2xl p-4">
         <div class="flex items-start gap-4">
            <div class="h-10 w-10 rounded-2xl bg-(--surface-card) flex items-center justify-center text-(--color-danger-text) shadow-sm border border-(--color-danger-text)/20">
               <ShieldAlert :size="20" />
            </div>
            <div class="flex-1">
               <h4 class="text-sm font-semibold text-(--color-danger-text) uppercase tracking-wide">Cảnh báo giảng viên điểm thấp</h4>
               <p class="text-xs text-(--color-danger-text) mt-1 leading-relaxed font-medium">
                 Có <strong>04 giảng viên</strong> có điểm đánh giá trung bình dưới 3.5 và nhận nhiều phản hồi tiêu cực về phương pháp truyền đạt. BGH cần xem xét báo cáo chi tiết để có hướng hỗ trợ.
               </p>
               <button @click="viewWarningList" class="mt-4 text-[10px] font-semibold text-(--color-danger-text) uppercase tracking-widest flex items-center gap-1 hover:underline">
                  Xem danh sách cảnh báo <ChevronRight :size="12" />
               </button>
            </div>
         </div>
      </div>

    </div>
  </div>
</template>

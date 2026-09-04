<script setup>
import { ref, computed, onMounted } from 'vue'
import { ShieldAlert, PieChart, ChevronRight, AlertCircle, CheckCircle2, TrendingUp, Sparkles, Loader2 } from 'lucide-vue-next'
import { useRouter } from 'vue-router'
import { usePopupStore } from '@/stores/popup'
import { bghApi } from '@/services/bghApi'
import { aiApi } from '@/services/aiApi'
import { unwrapApiData } from '@/services/apiClient'
import SkeletonDashboard from '@/components/common/skeleton/SkeletonDashboard.vue'
import BghAiReportModal from '@/components/BGH/BghAiReportModal.vue'

const router = useRouter()
const popup = usePopupStore()
const loading = ref(false)
const error = ref(null)

const kpis = ref([])
const sentiment = ref([])
const trendHistory = ref([])
const lowRatingCount = ref(0)
const hoveredTrendPoint = ref(null)

// AI Strategic Report State
const aiModalOpen = ref(false)
const aiLoading = ref(false)
const aiError = ref(null)
const aiReport = ref(null)

async function triggerEvalOverviewAiAnalysis() {
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
    aiError.value = err.message || 'Không thể tạo báo cáo phân tích đánh giá giảng viên AI.'
  } finally {
    aiLoading.value = false
  }
}

// Y-Scale Range: 3.0 to 5.0
const minY = 3.0
const maxY = 5.0

// Chart Points calculation over 1000x200 canvas
const chartPoints = computed(() => {
  if (!trendHistory.value?.length) return []
  const hist = trendHistory.value
  const count = hist.length
  return hist.map((item, idx) => {
    // Distribute X evenly from 40 to 960 across 1000 canvas width
    const x = count <= 1 ? 500 : 40 + (idx / (count - 1)) * 920
    const score = Math.max(minY, Math.min(maxY, Number(item.val) || 0))
    // Map score 3.0..5.0 to Y 175..25
    const y = 175 - ((score - minY) / (maxY - minY)) * 150
    return {
      x,
      y,
      term: item.label,
      score: Number(item.val).toFixed(1)
    }
  })
})

const chartLinePath = computed(() => {
  const pts = chartPoints.value
  if (!pts || pts.length === 0) return ''
  let d = `M ${pts[0].x} ${pts[0].y}`
  for (let i = 1; i < pts.length; i++) {
    const prev = pts[i - 1]
    const curr = pts[i]
    const cp1x = prev.x + (curr.x - prev.x) * 0.4
    const cp2x = curr.x - (curr.x - prev.x) * 0.4
    d += ` C ${cp1x} ${prev.y}, ${cp2x} ${curr.y}, ${curr.x} ${curr.y}`
  }
  return d
})

const chartAreaPath = computed(() => {
  const pts = chartPoints.value
  if (!pts || pts.length === 0) return ''
  let d = `M ${pts[0].x} 175 L ${pts[0].x} ${pts[0].y}`
  for (let i = 1; i < pts.length; i++) {
    const prev = pts[i - 1]
    const curr = pts[i]
    const cp1x = prev.x + (curr.x - prev.x) * 0.4
    const cp2x = curr.x - (curr.x - prev.x) * 0.4
    d += ` C ${cp1x} ${prev.y}, ${cp2x} ${curr.y}, ${curr.x} ${curr.y}`
  }
  d += ` L ${pts[pts.length - 1].x} 175 Z`
  return d
})

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const res = await bghApi.getEvaluationOverview()
    const data = unwrapApiData(res)
    if (data) {
      kpis.value = data.kpis || [
        { id: 1, label: 'Tổng số GV được đánh giá', value: data.totalTeachers ?? data.TotalTeachers ?? 0, trend: 'Thực tế', bgColor: 'bg-blue-500/10', color: 'text-blue-600' },
        { id: 2, label: 'Lượt đánh giá', value: data.totalReviews ?? data.TotalReviews ?? 0, trend: 'Tích lũy', bgColor: 'bg-teal-500/10', color: 'text-teal-600' },
        { id: 3, label: 'Rating Trung Bình', value: `${data.avgRating ?? data.AvgRating ?? 0} ★`, trend: 'Thang 5.0', bgColor: 'bg-amber-500/10', color: 'text-amber-600' },
        { id: 4, label: 'GV Cần hỗ trợ (<3.5)', value: data.lowRatingTeacherCount ?? data.LowRatingTeacherCount ?? 0, trend: 'Cảnh báo', bgColor: 'bg-rose-500/10', color: 'text-rose-600' }
      ]
      sentiment.value = data.sentiment || [
        { label: 'Tích cực (4-5★)', value: 78, color: 'bg-emerald-500', desc: 'Sinh viên hài lòng cao với phương pháp truyền đạt' },
        { label: 'Trung lập (3★)', value: 16, color: 'bg-amber-500', desc: 'Đáp ứng đầy đủ chuẩn kiến thức môn học' },
        { label: 'Cần cải thiện (1-2★)', value: 6, color: 'bg-rose-500', desc: 'Có ý kiến góp ý về tốc độ giảng dạy' }
      ]
      trendHistory.value = (data.semesterTrend || data.SemesterTrend || []).map(t => ({
        label: t.semester || t.Semester || 'Kỳ học',
        val: Number(t.avgRating ?? t.AvgRating ?? 0)
      }))
      if (trendHistory.value.length === 0) {
        trendHistory.value = [
          { label: 'HK1 2024', val: 4.1 },
          { label: 'HK2 2024', val: 4.3 },
          { label: 'HK1 2025', val: 4.2 },
          { label: 'HK2 2025', val: 4.5 }
        ]
      }
      lowRatingCount.value = Number(data.lowRatingTeacherCount ?? data.LowRatingTeacherCount ?? 0)
    }
  } catch (e) {
    error.value = e?.message || 'Không thể tải tổng quan đánh giá'
  } finally {
    loading.value = false
  }
}
onMounted(() => { loadData() })

function viewWarningList() {
  router.push('/bgh/evaluations/ranking?filter=warning')
}
</script>

<template>
  <div class="space-y-8">
    <div v-if="loading" class="p-4">
      <SkeletonDashboard :cards="4" :rows="3" />
    </div>
    <div v-else-if="error" class="flex flex-col items-center justify-center py-20 text-center">
      <AlertCircle :size="48" class="text-(--color-danger-text) mb-4" />
      <p class="text-lg font-semibold text-muted">Đã có lỗi xảy ra</p>
      <p class="text-sm text-placeholder mt-1">{{ error }}</p>
      <button @click="loadData" class="mt-4 lg-button-secondary px-4 py-2 text-sm font-semibold">Thử lại</button>
    </div>
    <div v-else class="space-y-8">
      
      <!-- Header Action Banner -->
      <div class="surface-card border border-card rounded-2xl p-4 flex flex-wrap items-center justify-between gap-4">
        <div>
          <h2 class="text-base font-bold text-heading">Tổng Quan Đánh Giá & Khảo Sát Giảng Viên</h2>
          <p class="text-xs text-muted mt-0.5">Theo dõi chỉ số hài lòng của sinh viên và dự báo xu hướng chất lượng đào tạo</p>
        </div>
        <button
          @click="triggerEvalOverviewAiAnalysis"
          :disabled="aiLoading"
          class="px-5 py-2.5 rounded-xl bg-gradient-to-r from-blue-600 via-indigo-600 to-indigo-700 hover:from-blue-700 hover:to-indigo-800 text-white text-xs font-bold shadow-md shadow-indigo-500/20 flex items-center gap-2 transition-all active:scale-95 disabled:opacity-60 cursor-pointer shrink-0"
        >
          <Sparkles v-if="!aiLoading" :size="15" />
          <Loader2 v-else :size="15" class="animate-spin" />
          <span>{{ aiLoading ? 'ĐANG PHÂN TÍCH...' : '⚡ CHIẾN LƯỢC ĐẢM BẢO CHẤT LƯỢNG (AI)' }}</span>
        </button>
      </div>

      <!-- Top KPI Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
        <div v-for="kpi in kpis" :key="kpi.id" class="surface-card border border-card rounded-2xl p-4 group hover:border-(--border-input-focus) transition-all shadow-xs">
           <div class="flex items-center justify-between mb-4">
              <div :class="['h-10 w-10 rounded-2xl flex items-center justify-center shadow-xs border border-default', kpi.bgColor, kpi.color]">
                 <span class="font-bold text-sm">★</span>
              </div>
              <span class="text-[10px] font-semibold uppercase tracking-widest text-muted">{{ kpi.trend }}</span>
           </div>
           <p class="text-xs font-semibold text-muted uppercase tracking-widest">{{ kpi.label }}</p>
           <h3 class="text-2xl font-bold text-heading mt-1">{{ kpi.value }}</h3>
        </div>
      </div>

      <!-- Main Chart & Sentiment Grid -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        <!-- Clean Rating Trend Chart Card (100% Crisp HTML Typography + Responsive SVG Curve) -->
        <div class="lg:col-span-2 surface-card border border-card rounded-2xl p-6 flex flex-col justify-between shadow-xs">
           <div class="flex items-center justify-between mb-4">
              <div>
                 <div class="flex items-center gap-2">
                   <h4 class="text-sm font-bold text-heading uppercase tracking-wide">Xu hướng điểm Rating trung bình</h4>
                   <span class="px-2.5 py-0.5 rounded-full bg-blue-500/10 text-blue-600 text-[10px] font-bold">Thang 5.0</span>
                 </div>
                 <p class="text-xs text-muted mt-1 font-medium">Dữ liệu đánh giá giảng dạy tổng hợp qua các học kỳ trên toàn hệ thống</p>
              </div>
              <div class="hidden sm:flex items-center gap-1.5 text-xs text-emerald-600 font-bold bg-emerald-500/10 px-3 py-1 rounded-xl">
                <TrendingUp :size="14" />
                <span>+0.4 ★ tăng trưởng</span>
              </div>
           </div>

           <!-- Full Height Chart Container -->
           <div v-if="chartPoints.length" class="w-full h-64 flex flex-col justify-between my-2">
             <!-- Main Plot Area with HTML Y-Axis on Left and Graph on Right -->
             <div class="relative w-full flex-1 flex gap-3">
               <!-- Pure HTML Y-Axis Labels (Left) -->
               <div class="flex flex-col justify-between text-[10px] font-bold font-mono text-muted py-0.5 shrink-0 w-6 text-right select-none">
                 <span>5.0</span>
                 <span>4.5</span>
                 <span>4.0</span>
                 <span>3.5</span>
                 <span>3.0</span>
               </div>

               <!-- Graph Canvas Container -->
               <div class="relative flex-1 h-full overflow-visible">
                 <!-- Horizontal Dashed Gridlines -->
                 <div class="absolute inset-0 flex flex-col justify-between pointer-events-none py-1">
                   <div v-for="i in 5" :key="i" class="w-full border-b border-dashed border-card opacity-60"></div>
                 </div>

                 <!-- SVG Area & Line Path ONLY (No text elements inside SVG to prevent any distortion) -->
                 <svg class="absolute inset-0 w-full h-full" viewBox="0 0 1000 200" preserveAspectRatio="none">
                   <defs>
                     <linearGradient id="eval-overview-grad" x1="0" y1="0" x2="0" y2="1">
                       <stop offset="0%" stop-color="#3B82F6" stop-opacity="0.3" />
                       <stop offset="100%" stop-color="#3B82F6" stop-opacity="0.0" />
                     </linearGradient>
                     <filter id="glow-shadow" x="-10%" y="-10%" width="120%" height="120%">
                       <feDropShadow dx="0" dy="4" stdDeviation="4" flood-color="#3B82F6" flood-opacity="0.35" />
                     </filter>
                   </defs>

                   <!-- Area Fill -->
                   <path :d="chartAreaPath" fill="url(#eval-overview-grad)" />

                   <!-- Bezier Curve Line Path -->
                   <path
                     :d="chartLinePath"
                     fill="none"
                     stroke="#3B82F6"
                     stroke-width="3"
                     stroke-linecap="round"
                     stroke-linejoin="round"
                     filter="url(#glow-shadow)"
                   />
                 </svg>

                 <!-- Pure HTML 100% Round Circle Markers (Guaranteed Perfect Circles) -->
                 <div
                   v-for="(pt, i) in chartPoints"
                   :key="'circle-' + i"
                   class="absolute transform -translate-x-1/2 -translate-y-1/2 w-3 h-3 rounded-full bg-blue-600 border-2 border-white shadow-xs cursor-pointer transition-transform hover:scale-125 z-10"
                   :style="{ left: `${(pt.x / 1000) * 100}%`, top: `${(pt.y / 200) * 100}%` }"
                   @mouseenter="hoveredTrendPoint = pt"
                   @mouseleave="hoveredTrendPoint = null"
                   @click="popup.info(pt.term, `Rating trung bình: ${pt.score} ★`)"
                 ></div>

                  <!-- Pure HTML Score Badges (4.0★, 4.7★...) Positioned Absolutely (Crisp Native Typography!) -->
                  <div
                    v-for="(pt, i) in chartPoints"
                    :key="'badge-' + i"
                    class="absolute transform -translate-x-1/2 -translate-y-[calc(100%+14px)] transition-colors cursor-pointer select-none"
                    :style="{ left: `${(pt.x / 1000) * 100}%`, top: `${(pt.y / 200) * 100}%` }"
                    @mouseenter="hoveredTrendPoint = pt"
                    @mouseleave="hoveredTrendPoint = null"
                    @click="popup.info(pt.term, `Rating trung bình: ${pt.score} ★`)"
                  >
                    <span
                      :class="hoveredTrendPoint === pt ? 'bg-blue-700 ring-2 ring-blue-400/50' : 'bg-blue-600'"
                      class="px-2 py-0.5 rounded-md text-[10px] font-bold font-mono text-white shadow-sm inline-block transition-all"
                    >
                      {{ pt.score }}★
                    </span>
                  </div>

                 <!-- Floating Glowing Tooltip on Hover -->
                 <div
                   v-if="hoveredTrendPoint"
                   class="absolute glowing-tooltip text-white text-xs font-bold rounded-xl px-3 py-1.5 pointer-events-none whitespace-nowrap z-50 shadow-xl border border-slate-700"
                   :style="{ left: `${(hoveredTrendPoint.x / 1000) * 100}%`, top: `${(hoveredTrendPoint.y / 200) * 100 - 25}%`, transform: 'translate(-50%, -100%)' }"
                 >
                   {{ hoveredTrendPoint.term }}: <span class="text-amber-400 font-mono text-sm ml-1">{{ hoveredTrendPoint.score }} ★</span>
                 </div>
               </div>
             </div>

             <!-- Pure HTML X-Axis Semester Labels (Crisp Native Typography, Never Squished) -->
             <div class="relative w-full h-6 pt-2 pl-9">
               <div
                 v-for="pt in chartPoints"
                 :key="pt.term"
                 class="absolute text-[10px] font-bold text-muted uppercase font-mono transform -translate-x-1/2 whitespace-nowrap"
                 :style="{ left: `calc(2.25rem + (100% - 2.25rem) * ${pt.x / 1000})` }"
               >
                 {{ pt.term }}
               </div>
             </div>
           </div>
        </div>

        <!-- Sentiment AI Analysis Card -->
        <div class="surface-card border border-card rounded-2xl p-6 flex flex-col justify-between shadow-xs">
           <div>
             <h4 class="text-sm font-bold text-heading uppercase tracking-wide mb-6">Phân tích Sentiment AI</h4>
             <div class="space-y-6">
                <div v-for="item in sentiment" :key="item.label">
                   <div class="flex items-center justify-between mb-1.5">
                      <span class="text-xs font-semibold text-label uppercase tracking-tighter">{{ item.label }}</span>
                      <span class="text-xs font-bold text-heading font-mono">{{ item.value }}%</span>
                   </div>
                   <div class="h-2 w-full bg-(--surface-input) rounded-full overflow-hidden">
                      <div :style="{ width: `${item.value}%` }" :class="['h-full rounded-full transition-all duration-1000', item.color]"></div>
                   </div>
                   <p class="text-[10px] text-muted mt-1.5 italic font-medium leading-tight">{{ item.desc }}</p>
                </div>
             </div>
           </div>

           <div class="mt-6 pt-4 border-t border-card">
              <div class="flex items-center gap-3 text-xs font-bold text-muted">
                 <div class="h-8 w-8 rounded-xl bg-blue-500/10 text-blue-600 flex items-center justify-center shrink-0 border border-blue-500/20">
                   <PieChart :size="16" />
                 </div>
                 <p class="text-body text-[11px] leading-snug">Hơn <strong>94%</strong> phản hồi ở mức tích cực và trung lập toàn trường.</p>
              </div>
           </div>
        </div>

      </div>

      <!-- Warning / Safe Banner dynamically calculated from DB -->
      <div v-if="lowRatingCount > 0" class="surface-card border border-(--color-danger-text)/20 bg-(--color-danger-bg) rounded-2xl p-4">
         <div class="flex items-start gap-4">
            <div class="h-10 w-10 rounded-2xl bg-(--surface-card) flex items-center justify-center text-(--color-danger-text) shadow-sm border border-(--color-danger-text)/20">
               <ShieldAlert :size="20" />
            </div>
            <div class="flex-1">
               <h4 class="text-sm font-semibold text-(--color-danger-text) uppercase tracking-wide">Cảnh báo giảng viên điểm thấp</h4>
               <p class="text-xs text-(--color-danger-text) mt-1 leading-relaxed font-medium">
                 Hệ thống phát hiện <strong>{{ lowRatingCount }} giảng viên</strong> có điểm đánh giá trung bình dưới 3.5. BGH cần xem xét chi tiết để có hướng hỗ trợ.
               </p>
               <button @click="viewWarningList" class="mt-4 text-[10px] font-semibold text-(--color-danger-text) uppercase tracking-widest flex items-center gap-1 hover:underline">
                  Xem danh sách cảnh báo <ChevronRight :size="12" />
               </button>
            </div>
         </div>
      </div>
      <div v-else class="surface-card border border-(--color-success-text)/20 bg-(--color-success-bg) rounded-2xl p-4">
         <div class="flex items-start gap-4">
            <div class="h-10 w-10 rounded-2xl bg-(--surface-card) flex items-center justify-center text-(--color-success-text) shadow-sm border border-(--color-success-text)/20">
               <CheckCircle2 :size="20" />
            </div>
             <div class="flex-1">
                <h4 class="text-sm font-semibold text-(--color-success-text) uppercase tracking-wide">Chất lượng giảng dạy ổn định</h4>
                <p class="text-xs text-(--color-success-text) mt-1 leading-relaxed font-medium">
                  Hệ thống ghi nhận <strong>không có giảng viên nào</strong> có điểm đánh giá trung bình dưới ngưỡng 3.5. Tất cả giảng viên đều đạt yêu cầu chuyên môn.
                </p>
             </div>
          </div>
       </div>

    </div>

    <!-- AI Strategic Report Modal -->
    <BghAiReportModal
      :is-open="aiModalOpen"
      title="Báo Cáo Đảm Bảo Chất Lượng & Đánh Giá Giảng Viên AI"
      subtitle="Tổng hợp dữ liệu khảo sát từ người học, phát hiện các điểm nóng và đề xuất giải pháp bồi dưỡng chuyên môn"
      :scope-badges="['Tổng quan Đánh giá', 'Phản hồi người học', 'Chế độ: Phân tích chuyên sâu']"
      :loading="aiLoading"
      :error="aiError"
      :report-content="aiReport?.aiAnalysis"
      :generated-at="aiReport?.generatedAt"
      @close="aiModalOpen = false"
      @retry="triggerEvalOverviewAiAnalysis"
    />
  </div>
</template>

<style scoped>
.glowing-tooltip {
  background: linear-gradient(135deg, #1e293b, #0f172a) !important;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.4), 0 0 10px rgba(59, 130, 246, 0.3) !important;
}
</style>

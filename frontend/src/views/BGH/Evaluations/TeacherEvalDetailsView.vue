<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import SkeletonDashboard from '@/components/common/skeleton/SkeletonDashboard.vue'
import { 
  ArrowLeft, 
  Star, 
  Download, 
  ShieldAlert, 
  User, 
  BookOpen, 
  MessageSquareQuote,
  Sparkles,
  Award,
  AlertCircle,
  TrendingUp,
  Building2,
  BadgeCheck
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const route = useRoute()
const loading = ref(false)
const error = ref(null)

const teacher = ref(null)

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const teacherId = route.params.teacherId
    if (teacherId) {
      const [res, rankingRes] = await Promise.all([
        bghApi.getEvaluationDetail(teacherId),
        bghApi.getEvaluationRanking(),
      ])
      const data = unwrapApiData(res)
      const ranking = unwrapApiData(rankingRes) || []
      if (data) {
        const rawCriteria = (data.criteria || []).map(item => ({
          label: item.criterionName || item.label || '',
          score: Number(item.avgScore ?? item.score ?? 0),
          max: 5
        }))

        teacher.value = {
          id: data.teacherId,
          name: data.teacherName,
          code: `GV${data.teacherId.toString().padStart(5, '0')}`,
          email: data.email,
          dept: data.department || 'Chưa phân chuyên ngành',
          avgRating: Number(data.avgRating || 0),
          totalReviews: data.totalReviews || 0,
          positivePercentage: Number(data.positivePercentage || 0),
          rank: ranking.findIndex(item => Number(item.teacherId) === Number(data.teacherId)) + 1,
          criteria: rawCriteria,
          recentFeedback: data.recentFeedback || [],
          semesterHistory: (data.semesterHistory || []).map(s => ({
            term: s.semester || s.term,
            score: Number(s.avgRating ?? s.score ?? 0),
          }))
        }
      }
    }
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}

// Trend Line Chart SVG calculations
const hoveredTrendPoint = ref(null)

const chartPoints = computed(() => {
  if (!teacher.value?.semesterHistory?.length) return []
  const hist = teacher.value.semesterHistory
  const count = hist.length
  return hist.map((item, idx) => {
    const x = count === 1 ? 500 : 65 + (idx / (count - 1)) * 870
    // Score range 3.0 to 5.0 mapped to Y=170 to Y=30
    const score = Math.max(3.0, Math.min(5.0, item.score))
    const y = 170 - ((score - 3.0) / 2.0) * 140
    return { x, y, term: item.term, score: item.score }
  })
})

const chartLinePath = computed(() => {
  const pts = chartPoints.value
  if (!pts || pts.length === 0) return ''
  let d = `M ${pts[0].x} ${pts[0].y}`
  for (let i = 1; i < pts.length; i++) {
    d += ` L ${pts[i].x} ${pts[i].y}`
  }
  return d
})

const chartAreaPath = computed(() => {
  const pts = chartPoints.value
  if (!pts || pts.length === 0) return ''
  let d = `M ${pts[0].x} 190 L ${pts[0].x} ${pts[0].y}`
  for (let i = 1; i < pts.length; i++) {
    d += ` L ${pts[i].x} ${pts[i].y}`
  }
  d += ` L ${pts[pts.length - 1].x} 190 Z`
  return d
})

onMounted(() => { loadData() })
</script>

<template>
  <PageContainer
    :title="teacher ? `Chi tiết đánh giá: ${teacher.name}` : 'Đang tải thông tin...'"
    subtitle="Báo cáo phân tích chuyên sâu chất lượng giảng dạy của giảng viên qua các tiêu chí và học kỳ."
  >
    <template #actions>
      <div class="flex items-center gap-3">
         <router-link to="/bgh/evaluations/ranking" class="lg-button-secondary px-4 py-2.5 text-xs font-bold flex items-center gap-2 rounded-xl">
            <ArrowLeft :size="16" /> Quay lại danh sách
         </router-link>
         <button class="lg-button-primary py-2.5 px-5 text-xs font-bold flex items-center gap-2 rounded-xl shadow-sm">
            <Download :size="16" /> Xuất báo cáo PDF
         </button>
      </div>
    </template>

    <div v-if="loading" class="p-4">
      <SkeletonDashboard :cards="4" :rows="3" />
    </div>
    <div v-else-if="error" class="flex flex-col items-center justify-center py-20 text-center">
      <AlertCircle :size="48" class="text-(--color-danger-text) mb-4" />
      <p class="text-lg font-semibold text-muted">Đã có lỗi xảy ra</p>
      <p class="text-sm text-placeholder mt-1">{{ error }}</p>
      <button @click="loadData" class="mt-4 lg-button-secondary px-4 py-2 text-sm font-semibold">Thử lại</button>
    </div>
    <div v-else-if="teacher" class="grid grid-cols-1 xl:grid-cols-3 gap-6 lg:gap-8">
      
      <!-- ── Left 2 Columns: Main Analytics ── -->
      <div class="xl:col-span-2 space-y-6 lg:space-y-8">

        <!-- ── Section 1: Header Badge & 2x2 Criteria Cards Grid ── -->
        <div class="surface-card border border-card rounded-2xl p-6 lg:p-8 relative shadow-sm">
           <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8 pb-6 border-b border-default">
             <div>
               <div class="flex items-center gap-2">
                 <h3 class="text-lg font-bold text-heading">Đánh Giá Theo Tiêu Chí</h3>
                 <span class="px-2.5 py-0.5 rounded-full text-[10px] font-extrabold uppercase tracking-wider bg-emerald-500/10 text-emerald-500 border border-emerald-500/20">{{ teacher.criteria.length }} tiêu chí</span>
               </div>
               <p class="text-xs text-muted mt-1">Tổng hợp phản hồi từ {{ teacher.totalReviews }} lượt đánh giá của sinh viên</p>
             </div>

             <!-- Rating Badge Box -->
             <div class="flex items-center gap-3 bg-(--surface-input) px-4 py-2.5 rounded-xl border border-card self-start sm:self-auto shadow-2xs">
               <div class="flex items-center justify-center h-10 w-10 rounded-lg bg-amber-400/10 text-amber-400">
                 <Star :size="22" class="fill-amber-400 text-amber-400" />
               </div>
               <div>
                 <div class="flex items-baseline gap-1">
                   <span class="text-2xl font-black text-heading leading-none">{{ teacher.avgRating }}</span>
                   <span class="text-xs font-semibold text-muted">/ 5.0</span>
                 </div>
                 <p class="text-[10px] font-bold text-amber-400 uppercase tracking-widest mt-0.5">Xếp loại Xuất Sắc</p>
               </div>
             </div>
           </div>

           <!-- 2x2 Grid of Criteria Cards -->
           <div class="grid grid-cols-1 md:grid-cols-2 gap-4 lg:gap-5">
             <div
               v-for="(item, idx) in teacher.criteria"
               :key="idx"
               class="surface-input border border-card rounded-xl p-5 hover:border-amber-400/30 transition-all shadow-2xs flex flex-col justify-between"
             >
               <div>
                 <div class="flex items-start justify-between gap-2 mb-3">
                   <h4 class="text-xs font-bold text-heading leading-snug line-clamp-2">{{ item.label }}</h4>
                   <div class="flex items-center gap-1 bg-amber-400/10 text-amber-400 px-2 py-0.5 rounded-md shrink-0">
                     <Star :size="12" class="fill-amber-400 text-amber-400" />
                     <span class="text-xs font-black">{{ item.score.toFixed(1) }}</span>
                   </div>
                 </div>

                 <!-- Stars breakdown -->
                 <div class="flex items-center gap-1 mb-4">
                   <Star
                     v-for="i in 5"
                     :key="i"
                     :size="14"
                     :class="i <= Math.round(item.score) ? 'text-amber-400 fill-amber-400' : 'text-slate-600/40'"
                   />
                   <span class="text-[10px] font-semibold text-muted ml-2">({{ Math.round((item.score / 5) * 100) }}% hài lòng)</span>
                 </div>
               </div>

               <!-- Progress Bar -->
               <div class="space-y-1.5">
                 <div class="flex justify-between text-[10px] font-bold text-muted">
                   <span>Mức độ đạt được</span>
                   <span class="text-heading font-mono">{{ item.score.toFixed(2) }} / 5.0</span>
                 </div>
                 <div class="h-2 w-full bg-(--surface-card) rounded-full overflow-hidden p-0.5 border border-card">
                   <div
                     :style="{ width: `${(item.score / 5) * 100}%` }"
                     class="h-full bg-gradient-to-r from-amber-400 to-amber-500 rounded-full transition-all duration-700 shadow-2xs"
                   ></div>
                 </div>
               </div>
             </div>
           </div>
        </div>

        <!-- ── Section 2: Semester Trend Line Chart ── -->
        <div class="surface-card border border-card rounded-2xl p-6 lg:p-8 shadow-sm">
           <div class="flex items-center justify-between mb-6">
             <div>
               <h3 class="text-base font-bold text-heading flex items-center gap-2">
                 <TrendingUp :size="18" class="text-emerald-400" /> Biến Động Điểm Qua Các Học Kỳ
               </h3>
               <p class="text-xs text-muted mt-1">Xu hướng tăng trưởng và ổn định chất lượng giảng dạy qua từng kỳ học</p>
             </div>
             <span class="text-xs font-mono font-bold px-3 py-1 bg-(--surface-input) text-heading rounded-lg border border-default">3.0 - 5.0 Điểm</span>
           </div>

           <!-- SVG Line Chart Area -->
           <div v-if="chartPoints.length" class="relative w-full h-64 overflow-visible">
             <svg class="absolute inset-0 w-full h-full" viewBox="0 0 1000 200" preserveAspectRatio="xMidYMid meet">
               <defs>
                 <linearGradient id="eval-trend-grad" x1="0" y1="0" x2="0" y2="1">
                   <stop offset="0%" stop-color="#3B82F6" stop-opacity="0.25" />
                   <stop offset="100%" stop-color="#3B82F6" stop-opacity="0.0" />
                 </linearGradient>
               </defs>

               <!-- Grid Lines -->
               <line v-for="i in 4" :key="'gl-'+i" x1="45" :y1="30 + (i-1)*45" x2="955" :y2="30 + (i-1)*45" stroke="var(--border-default)" stroke-width="1" stroke-dasharray="4 4" opacity="0.4" />
               <text x="35" y="34" class="text-[9px] font-bold fill-current text-muted" text-anchor="end">5.0</text>
               <text x="35" y="79" class="text-[9px] font-bold fill-current text-muted" text-anchor="end">4.5</text>
               <text x="35" y="124" class="text-[9px] font-bold fill-current text-muted" text-anchor="end">4.0</text>
               <text x="35" y="169" class="text-[9px] font-bold fill-current text-muted" text-anchor="end">3.5</text>

               <!-- Area & Line -->
               <path :d="chartAreaPath" fill="url(#eval-trend-grad)" />
               <path :d="chartLinePath" fill="none" stroke="#3B82F6" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" />

               <!-- Points -->
               <g v-for="(pt, i) in chartPoints" :key="'pt-'+i" class="cursor-pointer"
                  @mouseenter="hoveredTrendPoint = pt" @mouseleave="hoveredTrendPoint = null">
                 <circle :cx="pt.x" :cy="pt.y" r="12" fill="transparent" />
                 <circle :cx="pt.x" :cy="pt.y" r="5" fill="#3B82F6" stroke="white" stroke-width="2" />
                 <text :x="pt.x" y="195" class="text-[10px] font-bold fill-current text-muted" text-anchor="middle">{{ pt.term }}</text>
               </g>
             </svg>

             <!-- Floating Hover Tooltip -->
             <div
               v-if="hoveredTrendPoint"
               class="absolute glowing-tooltip text-white text-[10px] font-bold rounded-lg px-3 py-1.5 pointer-events-none whitespace-nowrap z-50 shadow-xl border border-slate-700"
               :style="{ left: `${(hoveredTrendPoint.x / 1000) * 100}%`, top: `${(hoveredTrendPoint.y / 200) * 100 - 15}%`, transform: 'translate(-50%, -100%)' }"
             >
               {{ hoveredTrendPoint.term }}: <span class="text-amber-400 font-mono text-xs">{{ hoveredTrendPoint.score }} ★</span>
             </div>
           </div>
           <div v-else class="h-48 flex items-center justify-center text-sm text-muted surface-input rounded-xl border border-card">
             Chưa có dữ liệu đánh giá theo học kỳ.
           </div>
        </div>
      </div>

      <!-- ── Right 1 Column: Sidebar & AI Summary ── -->
      <div class="space-y-6 lg:space-y-8">
        
        <!-- ── Teacher Profile Card ── -->
        <div class="surface-card border border-card rounded-2xl p-6 lg:p-8 text-center shadow-sm relative overflow-hidden">
           <!-- Top Accent Background Glow -->
           <div class="absolute top-0 left-0 right-0 h-2 bg-gradient-to-r from-blue-600 via-teal-500 to-indigo-600"></div>

           <!-- Avatar Section -->
           <div class="relative w-24 h-24 mx-auto mb-4 mt-2">
             <div class="w-full h-full rounded-2xl bg-gradient-to-br from-blue-500 to-teal-500 p-1 shadow-md">
               <div class="w-full h-full rounded-[14px] bg-slate-900 flex items-center justify-center text-white font-black text-2xl">
                 {{ teacher.name.charAt(0) }}
               </div>
             </div>
             <div class="absolute -bottom-1 -right-1 bg-emerald-500 text-white rounded-full p-1 border-2 border-slate-900" title="Giảng viên chính thức">
               <BadgeCheck :size="16" />
             </div>
           </div>

           <h3 class="text-xl font-bold text-heading">{{ teacher.name }}</h3>
           <p class="text-xs font-mono font-bold text-muted mt-0.5 tracking-wider">{{ teacher.code }} · {{ teacher.email }}</p>

           <!-- Achievement Badge -->
           <div v-if="teacher.rank > 0 && teacher.rank <= 3" class="mt-4 inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-amber-400/10 text-amber-400 border border-amber-400/20 text-xs font-bold">
             <Award :size="16" />
             <span>Hạng {{ teacher.rank }} trong bảng đánh giá giảng viên</span>
           </div>
           
           <div class="mt-6 pt-6 border-t border-default space-y-3.5 text-left text-xs">
              <div class="flex items-center justify-between p-2.5 rounded-xl surface-input border border-card">
                 <span class="text-muted flex items-center gap-2 font-medium"><Building2 :size="15" /> Khoa phụ trách</span>
                 <span class="font-bold text-heading text-right">{{ teacher.dept }}</span>
              </div>
              <div class="flex items-center justify-between p-2.5 rounded-xl surface-input border border-card">
                 <span class="text-muted flex items-center gap-2 font-medium"><BookOpen :size="15" /> Đánh giá tích cực</span>
                 <span class="font-bold text-emerald-400 font-mono">{{ teacher.positivePercentage }}%</span>
              </div>
           </div>
        </div>

        <!-- ── AI Feedback Narrative Card ── -->
        <div class="surface-card border border-teal-500/20 bg-teal-500/5 rounded-2xl p-6 lg:p-7 relative shadow-sm">
           <div class="flex items-center gap-3 mb-4">
              <div class="h-10 w-10 rounded-xl bg-teal-500/15 text-teal-400 flex items-center justify-center border border-teal-500/20 shadow-2xs">
                 <Sparkles :size="20" />
              </div>
              <div>
                <h4 class="text-sm font-bold text-heading uppercase tracking-wide">Nhận xét sinh viên gần nhất</h4>
                <p class="text-[10px] text-muted font-medium">Dữ liệu đánh giá được truy vấn từ CSDL</p>
              </div>
           </div>

           <!-- Quotes Block -->
           <div class="relative pl-4 border-l-2 border-teal-500/40 my-4">
             <MessageSquareQuote :size="18" class="text-teal-400/50 absolute -top-1 left-2" />
             <p class="text-xs text-body leading-relaxed italic font-serif pl-4">
               {{ teacher.recentFeedback[0]?.comment || 'Chưa có nhận xét bằng văn bản.' }}
             </p>
           </div>

           <!-- Semi-transparent Keyword Badges -->
           <div class="mt-5 pt-4 border-t border-teal-500/20 flex flex-wrap gap-2">
              <span v-for="criterion in teacher.criteria.slice(0, 4)" :key="criterion.label" class="px-2.5 py-1 rounded-lg bg-emerald-500/15 text-emerald-400 text-[10px] font-bold border border-emerald-500/20">#{{ criterion.label }}</span>
           </div>
        </div>

        <!-- Critical Quality Alert Area -->
        <div v-if="teacher.avgRating < 3.5" class="surface-card border border-rose-500/30 bg-rose-500/10 rounded-2xl p-5 shadow-sm">
           <div class="flex items-start gap-3.5 text-rose-400">
              <ShieldAlert :size="22" class="shrink-0 mt-0.5" />
              <div>
                 <h4 class="text-xs font-bold text-rose-300 uppercase tracking-wide">Cảnh báo chất lượng</h4>
                 <p class="text-xs text-rose-400 mt-1 font-medium leading-relaxed">Giảng viên có chỉ số hài lòng dưới mức kỳ vọng. Ban Giám Hiệu cần xếp lịch trao đổi chuyên môn.</p>
              </div>
           </div>
        </div>

      </div>
    </div>

    <div v-else class="flex flex-col items-center justify-center py-20 text-center">
      <User :size="48" class="text-placeholder mb-4" />
      <p class="text-lg font-semibold text-muted">Không tìm thấy thông tin giảng viên</p>
    </div>
  </PageContainer>
</template>

<style scoped>
.glowing-tooltip {
  background: linear-gradient(135deg, #1e293b, #0f172a) !important;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.4), 0 0 10px rgba(59, 130, 246, 0.3) !important;
}
</style>

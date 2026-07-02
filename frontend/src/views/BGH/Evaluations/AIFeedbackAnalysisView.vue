<script setup>
import { ref, computed } from 'vue'
import { 
  Brain, TrendingUp, PieChart, MapPin, Zap, Search, Download, Filter, ChevronDown, X
} from 'lucide-vue-next'
import { exportToExcel } from '@/services/exportService.js'
import { usePopupStore } from '@/stores/popup'

const popup = usePopupStore()
const searchQuery = ref('')
const sentimentFilter = ref('all')
const showFilterDetail = ref(false)

const sentimentSummary = { positive: 72, neutral: 18, negative: 10 }

const topTopics = ref([
  { label: 'Giảng dạy dễ hiểu', count: 1240, sentiment: 'positive', change: '+12%' },
  { label: 'Tương tác tốt', count: 850, sentiment: 'positive', change: '+5%' },
  { label: 'Tài liệu chưa rõ ràng', count: 320, sentiment: 'negative', change: '-2%' },
  { label: 'Tốc độ giảng nhanh', count: 280, sentiment: 'negative', change: '+8%' },
  { label: 'Chấm bài chậm', count: 150, sentiment: 'negative', change: '+15%' },
  { label: 'Nhiệt tình hỗ trợ', count: 1100, sentiment: 'positive', change: '+20%' },
  { label: 'Bài tập thực hành tốt', count: 680, sentiment: 'positive', change: '+7%' },
  { label: 'Giờ giấc không ổn định', count: 95, sentiment: 'negative', change: '+3%' },
])

const campusInsights = ref([
  { campus: 'Cơ sở Hồ Chí Minh', pos: 78, neg: 8, top: 'Dễ hiểu, Nhiệt tình' },
  { campus: 'Cơ sở Đà Nẵng', pos: 65, neg: 15, top: 'Tài liệu, Tốc độ giảng' },
])

const filteredTopics = computed(() => {
  let list = topTopics.value
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(t => t.label.toLowerCase().includes(q))
  }
  if (sentimentFilter.value !== 'all') {
    list = list.filter(t => t.sentiment === sentimentFilter.value)
  }
  return list
})

function exportData() {
  const data = topTopics.value.map(t => ({
    'Chủ đề': t.label,
    'Số lượng': t.count,
    'Cảm xúc': t.sentiment,
    'Thay đổi': t.change,
  }))
  exportToExcel(data, 'AI-Feedback-Topics.xlsx', 'Topics')
  popup.success('Xuất Excel', 'Đã xuất dữ liệu chủ đề feedback.')
}
</script>

<template>
  <div class="space-y-8">
    <div class="flex items-center gap-3">
      <button @click="exportData" class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
        <Download :size="18" /> Export Excel
      </button>
    </div>
      
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        <div class="lg:col-span-2 surface-card border border-card rounded-2xl p-8 relative overflow-hidden">
           <div class="relative z-10">
              <div class="flex items-center gap-4 mb-10">
                 <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) flex items-center justify-center border border-(--color-info-text)/20 text-(--color-info-text)">
                    <PieChart :size="24" />
                 </div>
                 <h3 class="text-xl font-semibold text-heading">Phân bổ cảm xúc toàn trường</h3>
              </div>

              <div class="flex items-center gap-12">
                 <div class="relative h-48 w-48 flex items-center justify-center">
                    <svg class="h-full w-full rotate-[-90deg]">
                       <circle cx="96" cy="96" r="80" fill="transparent" stroke="currentColor" stroke-width="24" class="text-(--surface-input)" />
                       <circle cx="96" cy="96" r="80" fill="transparent" stroke="currentColor" stroke-width="24" stroke-dasharray="502" :stroke-dashoffset="502 * (1 - sentimentSummary.positive/100)" class="text-(--color-success-text)" />
                    </svg>
                    <div class="absolute inset-0 flex flex-col items-center justify-center">
                       <h2 class="text-4xl font-semibold text-heading">{{ sentimentSummary.positive }}%</h2>
                       <p class="text-[10px] font-semibold uppercase tracking-widest text-muted">Tích cực</p>
                    </div>
                 </div>

                 <div class="flex-1 space-y-4">
                    <div v-for="(val, key) in sentimentSummary" :key="key" class="space-y-2">
                       <div class="flex justify-between text-xs font-semibold uppercase tracking-widest">
                          <span :class="key === 'positive' ? 'text-(--color-success-text)' : key === 'negative' ? 'text-(--color-danger-text)' : 'text-muted'">{{ key }}</span>
                          <span class="text-heading">{{ val }}%</span>
                       </div>
                       <div class="h-1.5 w-full bg-(--surface-input) rounded-full overflow-hidden">
                          <div :style="{ width: `${val}%` }" :class="['h-full rounded-full', key === 'positive' ? 'bg-(--color-success-text)' : key === 'negative' ? 'bg-(--color-danger-text)' : 'bg-(--text-placeholder)']"></div>
                       </div>
                    </div>
                 </div>
              </div>
           </div>
        </div>

        <div class="surface-card border border-card rounded-2xl p-5 overflow-hidden relative">
           <h4 class="text-sm font-semibold text-heading uppercase tracking-wide mb-6 flex items-center gap-2">
              <Zap :size="18" class="text-(--color-warning-text)" /> Chủ đề phổ biến
           </h4>
           <div class="flex items-center gap-2 mb-4">
             <div class="relative flex-1">
               <Search :size="14" class="absolute left-2.5 top-1/2 -translate-y-1/2 text-placeholder" />
               <input v-model="searchQuery" type="text" placeholder="Tìm chủ đề..." class="w-full surface-input border border-input rounded-lg pl-8 pr-3 py-1.5 text-xs font-medium outline-none focus:ring-2 focus:ring-(--border-focus-ring)">
             </div>
             <select v-model="sentimentFilter" class="surface-input border border-input rounded-lg px-2 py-1.5 text-[10px] font-bold outline-none">
               <option value="all">All</option>
               <option value="positive">Pos</option>
               <option value="negative">Neg</option>
             </select>
           </div>
           <div class="flex flex-wrap gap-2 max-h-60 overflow-y-auto">
              <div v-for="topic in filteredTopics" :key="topic.label"
                :class="['px-3 py-2 rounded-2xl text-[11px] font-bold border transition-all cursor-pointer', topic.sentiment === 'positive' ? 'bg-(--color-success-bg) text-(--color-success-text) border-(--color-success-text)/20 hover:bg-(--surface-input)' : 'bg-(--color-danger-bg) text-(--color-danger-text) border-(--color-danger-text)/20 hover:bg-(--surface-input)']">
                 <div class="flex items-center gap-1.5">
                    {{ topic.label }}
                    <span class="text-[9px] font-semibold opacity-50">{{ topic.count }}</span>
                    <span class="text-[8px] font-bold opacity-70">{{ topic.change }}</span>
                 </div>
              </div>
           </div>
        </div>

      </div>

      <div class="grid grid-cols-1 xl:grid-cols-2 gap-8 mt-8">
         <div class="surface-card border border-card rounded-2xl p-8">
            <h4 class="text-sm font-semibold text-heading uppercase tracking-wide mb-8">Tổng hợp Insight theo Cơ sở</h4>
            <div class="space-y-4">
               <div v-for="ins in campusInsights" :key="ins.campus" class="p-4 surface-solid rounded-2xl border border-default group hover:border-(--border-input-focus) transition-all">
                  <div class="flex items-center justify-between mb-4">
                     <div class="flex items-center gap-3">
                        <MapPin :size="18" class="text-muted group-hover:text-link" />
                        <span class="text-sm font-semibold text-heading">{{ ins.campus }}</span>
                     </div>
                     <div class="flex gap-4 text-[10px] font-semibold uppercase tracking-widest">
                        <span class="text-(--color-success-text)">{{ ins.pos }}% Positive</span>
                        <span class="text-(--color-danger-text)">{{ ins.neg }}% Negative</span>
                     </div>
                  </div>
                  <div class="flex items-center gap-4">
                     <div class="flex-1 h-1.5 bg-(--surface-input) rounded-full overflow-hidden flex">
                        <div :style="{ width: `${ins.pos}%` }" class="bg-(--color-success-text) h-full rounded-l-full"></div>
                        <div :style="{ width: `${100 - ins.pos - ins.neg}%` }" class="bg-(--text-placeholder) h-full"></div>
                        <div :style="{ width: `${ins.neg}%` }" class="bg-(--color-danger-text) h-full rounded-r-full"></div>
                     </div>
                  </div>
                  <div class="flex items-start gap-2 pt-4 mt-4">
                     <Brain :size="14" class="text-link shrink-0 mt-0.5" />
                     <p class="text-xs text-muted font-medium">Keywords: <span class="font-bold text-label">{{ ins.top }}</span></p>
                  </div>
               </div>
            </div>
         </div>

         <div class="surface-card border border-(--color-info-text)/20 bg-(--color-info-bg) rounded-2xl p-5">
            <div class="flex items-center gap-4 mb-8">
               <div class="h-10 w-10 rounded-2xl bg-(--surface-card) text-(--color-info-text) flex items-center justify-center shadow-sm border border-(--color-info-text)/20">
                  <TrendingUp :size="24" />
               </div>
               <h4 class="text-lg font-semibold text-heading">Dự báo chất lượng học kỳ tới</h4>
            </div>
            <p class="text-sm text-(--color-info-text) leading-relaxed font-medium mb-8 italic">
              "Dựa trên các topic tiêu cực về 'Tốc độ giảng nhanh' và 'Tài liệu chưa rõ', AI dự báo điểm rating có thể giảm <strong>0.2</strong> nếu không có biện pháp can thiệp và chuẩn hóa học liệu số cho các môn chuyên ngành."
            </p>
            <div class="grid grid-cols-2 gap-4">
               <div class="p-4 surface-card rounded-2xl border border-(--color-info-text)/20 shadow-sm">
                  <p class="text-[9px] font-semibold text-muted uppercase tracking-widest">Action Required</p>
                  <p class="text-xs font-semibold text-heading mt-1">Chuẩn hóa LMS Docs</p>
               </div>
               <div class="p-4 surface-card rounded-2xl border border-(--color-info-text)/20 shadow-sm">
                  <p class="text-[9px] font-semibold text-muted uppercase tracking-widest">Priority</p>
                  <p class="text-xs font-semibold text-link mt-1">High (P2)</p>
               </div>
            </div>
         </div>
      </div>

  </div>
</template>

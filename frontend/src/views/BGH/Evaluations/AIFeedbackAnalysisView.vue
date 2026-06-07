<script setup>
import { ref } from 'vue'
import { 
  Brain, 
  TrendingUp, 
  PieChart, 
  MapPin,
  Zap
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const sentimentSummary = { positive: 72, neutral: 18, negative: 10 }

const topTopics = ref([
  { label: 'Giảng dạy dễ hiểu', count: 1240, sentiment: 'positive', change: '+12%' },
  { label: 'Tương tác tốt', count: 850, sentiment: 'positive', change: '+5%' },
  { label: 'Tài liệu chưa rõ ràng', count: 320, sentiment: 'negative', change: '-2%' },
  { label: 'Tốc độ giảng nhanh', count: 280, sentiment: 'negative', change: '+8%' },
  { label: 'Chấm bài chậm', count: 150, sentiment: 'negative', change: '+15%' },
  { label: 'Nhiệt tình hỗ trợ', count: 1100, sentiment: 'positive', change: '+20%' },
])

const campusInsights = ref([
  { campus: 'Cơ sở Hồ Chí Minh', pos: 78, neg: 8, top: 'Dễ hiểu, Nhiệt tình' },
  { campus: 'Cơ sở Đà Nẵng', pos: 65, neg: 15, top: 'Tài liệu, Tốc độ giảng' },
])
</script>

<template>
  <PageContainer 
    title="Phân tích Feedback AI" 
    subtitle="Sử dụng trí tuệ nhân tạo để phân tích cảm xúc và trích xuất các chủ đề thảo luận chính từ hàng ngàn nhận xét của sinh viên."
  >
    <div class="space-y-8">
      
      <!-- ── Global Sentiment Analysis ── -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        <!-- Big Sentiment Chart Mock -->
        <div class="lg:col-span-2 surface-card border border-card rounded-2xl p-8 relative overflow-hidden">
           <div class="relative z-10">
              <div class="flex items-center gap-4 mb-10">
                 <div class="h-10 w-10 rounded-2xl bg-[var(--color-info-bg)] flex items-center justify-center border border-[var(--color-info-text)]/20 text-[var(--color-info-text)]">
                    <PieChart :size="24" />
                 </div>
                 <h3 class="text-xl font-semibold text-heading">Phân bổ cảm xúc toàn trường</h3>
              </div>

              <div class="flex items-center gap-12">
                 <div class="relative h-48 w-48 flex items-center justify-center">
                    <!-- Donut Mock -->
                    <svg class="h-full w-full rotate-[-90deg]">
                       <circle cx="96" cy="96" r="80" fill="transparent" stroke="currentColor" stroke-width="24" class="text-[var(--surface-input)]" />
                       <circle cx="96" cy="96" r="80" fill="transparent" stroke="currentColor" stroke-width="24" stroke-dasharray="502" :stroke-dashoffset="502 * (1 - sentimentSummary.positive/100)" class="text-[var(--color-success-text)]" />
                    </svg>
                    <div class="absolute inset-0 flex flex-col items-center justify-center">
                       <h2 class="text-4xl font-semibold text-heading">{{ sentimentSummary.positive }}%</h2>
                       <p class="text-[10px] font-semibold uppercase tracking-widest text-muted">Tích cực</p>
                    </div>
                 </div>

                 <div class="flex-1 space-y-4">
                    <div v-for="(val, key) in sentimentSummary" :key="key" class="space-y-2">
                       <div class="flex justify-between text-xs font-semibold uppercase tracking-widest">
                          <span :class="key === 'positive' ? 'text-[var(--color-success-text)]' : key === 'negative' ? 'text-[var(--color-danger-text)]' : 'text-muted'">{{ key }}</span>
                          <span class="text-heading">{{ val }}%</span>
                       </div>
                       <div class="h-1.5 w-full bg-[var(--surface-input)] rounded-full overflow-hidden">
                          <div 
                            :style="{ width: `${val}%` }" 
                            :class="['h-full rounded-full', key === 'positive' ? 'bg-[var(--color-success-text)]' : key === 'negative' ? 'bg-[var(--color-danger-text)]' : 'bg-[var(--text-placeholder)]']"
                          ></div>
                       </div>
                    </div>
                 </div>
              </div>
           </div>
        </div>

        <!-- Topic Cloud Mock -->
        <div class="surface-card border border-card rounded-2xl p-5 overflow-hidden relative">
           <h4 class="text-sm font-semibold text-heading uppercase tracking-wide mb-8 flex items-center gap-2">
              <Zap :size="18" class="text-[var(--color-warning-text)]" /> Chủ đề phổ biến (AI Topics)
           </h4>
           <div class="flex flex-wrap gap-3">
              <div 
                v-for="topic in topTopics" 
                :key="topic.label"
                :class="[
                  'px-4 py-2.5 rounded-2xl text-xs font-bold border transition-all cursor-pointer',
                  topic.sentiment === 'positive' ? 'bg-[var(--color-success-bg)] text-[var(--color-success-text)] border-[var(--color-success-text)]/20 hover:bg-[var(--surface-input)]' : 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)] border-[var(--color-danger-text)]/20 hover:bg-[var(--surface-input)]'
                ]"
              >
                 <div class="flex items-center gap-2">
                    {{ topic.label }}
                    <span class="text-[9px] font-semibold opacity-50">{{ topic.count }}</span>
                 </div>
              </div>
           </div>
        </div>

      </div>

      <!-- ── Campus Breakdown ── -->
      <div class="grid grid-cols-1 xl:grid-cols-2 gap-8 mt-8">
         <div class="surface-card border border-card rounded-2xl p-8">
            <h4 class="text-sm font-semibold text-heading uppercase tracking-wide mb-8">Tổng hợp Insight theo Cơ sở</h4>
            <div class="space-y-4">
               <div v-for="ins in campusInsights" :key="ins.campus" class="p-4 surface-solid rounded-2xl border border-default group hover:border-[var(--border-input-focus)] transition-all">
                  <div class="flex items-center justify-between mb-4">
                     <div class="flex items-center gap-3">
                        <MapPin :size="18" class="text-muted group-hover:text-link" />
                        <span class="text-sm font-semibold text-heading">{{ ins.campus }}</span>
                     </div>
                     <div class="flex gap-4 text-[10px] font-semibold uppercase tracking-widest">
                        <span class="text-[var(--color-success-text)]">{{ ins.pos }}% Positive</span>
                        <span class="text-[var(--color-danger-text)]">{{ ins.neg }}% Negative</span>
                     </div>
                  </div>
                  <div class="flex items-start gap-2 pt-4 border-t border-default">
                     <Brain :size="14" class="text-link shrink-0 mt-0.5" />
                     <p class="text-xs text-muted font-medium">Keywords: <span class="font-bold text-label">{{ ins.top }}</span></p>
                  </div>
               </div>
            </div>
         </div>

         <!-- AI Prediction vs Goal -->
         <div class="surface-card border border-[var(--color-info-text)]/20 bg-[var(--color-info-bg)] rounded-2xl p-5">
            <div class="flex items-center gap-4 mb-8">
               <div class="h-10 w-10 rounded-2xl bg-[var(--surface-card)] text-[var(--color-info-text)] flex items-center justify-center shadow-sm border border-[var(--color-info-text)]/20">
                  <TrendingUp :size="24" />
               </div>
               <h4 class="text-lg font-semibold text-heading">Dự báo chất lượng học kỳ tới</h4>
            </div>
            <p class="text-sm text-[var(--color-info-text)] leading-relaxed font-medium mb-8 italic">
              "Dựa trên các topic tiêu cực về 'Tốc độ giảng nhanh' và 'Tài liệu chưa rõ', AI dự báo điểm rating có thể giảm <strong>0.2</strong> nếu không có biện pháp can thiệp và chuẩn hóa học liệu số cho các môn chuyên ngành."
            </p>
            <div class="grid grid-cols-2 gap-4">
               <div class="p-4 surface-card rounded-2xl border border-[var(--color-info-text)]/20 shadow-sm">
                  <p class="text-[9px] font-semibold text-muted uppercase tracking-widest">Action Required</p>
                  <p class="text-xs font-semibold text-heading mt-1">Chuẩn hóa LMS Docs</p>
               </div>
               <div class="p-4 surface-card rounded-2xl border border-[var(--color-info-text)]/20 shadow-sm">
                  <p class="text-[9px] font-semibold text-muted uppercase tracking-widest">Priority</p>
                  <p class="text-xs font-semibold text-link mt-1">High (P2)</p>
               </div>
            </div>
         </div>
      </div>

    </div>
  </PageContainer>
</template>

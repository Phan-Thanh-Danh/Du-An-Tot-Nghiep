<script setup>
import { ref } from 'vue'
import { 
  Brain, 
  Search, 
  Filter, 
  MessageSquare, 
  TrendingUp, 
  TrendingDown, 
  PieChart, 
  BarChart, 
  Building2, 
  MapPin,
  CheckCircle2,
  AlertCircle,
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
        <div class="lg:col-span-2 lg-card-glass p-10 bg-gradient-to-br from-slate-900 to-slate-800 text-white relative overflow-hidden group">
           <div class="absolute -right-20 -bottom-20 h-64 w-64 bg-blue-500/10 rounded-full blur-3xl group-hover:scale-110 transition-transform duration-1000"></div>
           
           <div class="relative z-10">
              <div class="flex items-center gap-4 mb-10">
                 <div class="h-12 w-12 rounded-2xl bg-white/10 backdrop-blur-md flex items-center justify-center border border-white/20">
                    <PieChart :size="24" class="text-blue-400" />
                 </div>
                 <h3 class="text-xl font-black">Phân bổ cảm xúc toàn trường</h3>
              </div>

              <div class="flex items-center gap-12">
                 <div class="relative h-48 w-48 flex items-center justify-center">
                    <!-- Donut Mock -->
                    <svg class="h-full w-full rotate-[-90deg]">
                       <circle cx="96" cy="96" r="80" fill="transparent" stroke="currentColor" stroke-width="24" class="text-white/5" />
                       <circle cx="96" cy="96" r="80" fill="transparent" stroke="currentColor" stroke-width="24" stroke-dasharray="502" :stroke-dashoffset="502 * (1 - sentimentSummary.positive/100)" class="text-emerald-500" />
                    </svg>
                    <div class="absolute inset-0 flex flex-col items-center justify-center">
                       <h2 class="text-4xl font-black">{{ sentimentSummary.positive }}%</h2>
                       <p class="text-[10px] font-black uppercase tracking-widest text-white/50">Tích cực</p>
                    </div>
                 </div>

                 <div class="flex-1 space-y-6">
                    <div v-for="(val, key) in sentimentSummary" :key="key" class="space-y-2">
                       <div class="flex justify-between text-xs font-black uppercase tracking-widest">
                          <span :class="key === 'positive' ? 'text-emerald-400' : key === 'negative' ? 'text-rose-400' : 'text-slate-400'">{{ key }}</span>
                          <span>{{ val }}%</span>
                       </div>
                       <div class="h-1.5 w-full bg-white/10 rounded-full overflow-hidden">
                          <div 
                            :style="{ width: `${val}%` }" 
                            :class="['h-full rounded-full', key === 'positive' ? 'bg-emerald-500' : key === 'negative' ? 'bg-rose-500' : 'bg-slate-400']"
                          ></div>
                       </div>
                    </div>
                 </div>
              </div>
           </div>
        </div>

        <!-- Topic Cloud Mock -->
        <div class="lg-card-glass p-8 overflow-hidden relative">
           <h4 class="text-sm font-black text-slate-800 uppercase tracking-wide mb-8 flex items-center gap-2">
              <Zap :size="18" class="text-amber-500" /> Chủ đề phổ biến (AI Topics)
           </h4>
           <div class="flex flex-wrap gap-3">
              <div 
                v-for="topic in topTopics" 
                :key="topic.label"
                :class="[
                  'px-4 py-2.5 rounded-2xl text-xs font-bold border transition-all cursor-pointer hover:scale-105',
                  topic.sentiment === 'positive' ? 'bg-emerald-50 text-emerald-700 border-emerald-100 hover:bg-emerald-100' : 'bg-rose-50 text-rose-700 border-rose-100 hover:bg-rose-100'
                ]"
              >
                 <div class="flex items-center gap-2">
                    {{ topic.label }}
                    <span class="text-[9px] font-black opacity-50">{{ topic.count }}</span>
                 </div>
              </div>
           </div>
        </div>

      </div>

      <!-- ── Campus Breakdown ── -->
      <div class="grid grid-cols-1 xl:grid-cols-2 gap-8 mt-8">
         <div class="lg-card-glass p-8">
            <h4 class="text-sm font-black text-slate-800 uppercase tracking-wide mb-8">Tổng hợp Insight theo Cơ sở</h4>
            <div class="space-y-4">
               <div v-for="ins in campusInsights" :key="ins.campus" class="p-6 bg-slate-50/50 rounded-[32px] border border-slate-100 group hover:border-blue-200 transition-all">
                  <div class="flex items-center justify-between mb-4">
                     <div class="flex items-center gap-3">
                        <MapPin :size="18" class="text-slate-400 group-hover:text-blue-500" />
                        <span class="text-sm font-black text-slate-800">{{ ins.campus }}</span>
                     </div>
                     <div class="flex gap-4 text-[10px] font-black uppercase tracking-widest">
                        <span class="text-emerald-600">{{ ins.pos }}% Positive</span>
                        <span class="text-rose-600">{{ ins.neg }}% Negative</span>
                     </div>
                  </div>
                  <div class="flex items-start gap-2 pt-4 border-t border-slate-100">
                     <Brain :size="14" class="text-indigo-500 shrink-0 mt-0.5" />
                     <p class="text-xs text-slate-500 font-medium">Keywords: <span class="font-bold text-slate-700">{{ ins.top }}</span></p>
                  </div>
               </div>
            </div>
         </div>

         <!-- AI Prediction vs Goal -->
         <div class="lg-card-glass p-8 bg-indigo-50/20 border-indigo-100">
            <div class="flex items-center gap-4 mb-8">
               <div class="h-12 w-12 rounded-2xl bg-indigo-600 text-white flex items-center justify-center shadow-lg">
                  <TrendingUp :size="24" />
               </div>
               <h4 class="text-lg font-black text-indigo-900">Dự báo chất lượng học kỳ tới</h4>
            </div>
            <p class="text-sm text-indigo-700/80 leading-relaxed font-medium mb-8 italic">
              "Dựa trên các topic tiêu cực về 'Tốc độ giảng nhanh' và 'Tài liệu chưa rõ', AI dự báo điểm rating có thể giảm <strong>0.2</strong> nếu không có biện pháp can thiệp và chuẩn hóa học liệu số cho các môn chuyên ngành."
            </p>
            <div class="grid grid-cols-2 gap-4">
               <div class="p-4 bg-white rounded-2xl border border-indigo-100 shadow-sm">
                  <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Action Required</p>
                  <p class="text-xs font-black text-slate-800 mt-1">Chuẩn hóa LMS Docs</p>
               </div>
               <div class="p-4 bg-white rounded-2xl border border-indigo-100 shadow-sm">
                  <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Priority</p>
                  <p class="text-xs font-black text-indigo-600 mt-1">High (P2)</p>
               </div>
            </div>
         </div>
      </div>

    </div>
  </PageContainer>
</template>

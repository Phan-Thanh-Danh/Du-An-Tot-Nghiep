<script setup>
import { ref } from 'vue'
import { 
  Users, 
  Star, 
  MessageCircle, 
  TrendingUp, 
  ShieldAlert, 
  PieChart, 
  BarChart3, 
  ChevronRight,
  Filter,
  CheckCircle2
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock KPIs ────────────────────────────────────────────────
const kpis = [
  { id: 1, label: 'GV được đánh giá', value: '142', trend: '95% tổng số', icon: Users, color: 'text-blue-600', bgColor: 'bg-blue-50' },
  { id: 2, label: 'Rating Trung bình', value: '4.65', trend: '/ 5.0', icon: Star, color: 'text-amber-600', bgColor: 'bg-amber-50' },
  { id: 3, label: 'Số lượt đánh giá', value: '3,840', trend: '+12% kỳ trước', icon: MessageCircle, color: 'text-indigo-600', bgColor: 'bg-indigo-50' },
  { id: 4, label: 'Tỷ lệ phản hồi', value: '72.4%', trend: 'Mục tiêu: 80%', icon: CheckCircle2, color: 'text-emerald-600', bgColor: 'bg-emerald-50' },
]

// ── Mock Sentiment Data ──────────────────────────────────────
const sentiment = [
  { label: 'Tích cực', value: 78, color: 'bg-emerald-500', desc: 'Hài lòng về phương pháp giảng dạy' },
  { label: 'Trung lập', value: 15, color: 'bg-slate-400', desc: 'Không có ý kiến đặc biệt' },
  { label: 'Tiêu cực', value: 7, color: 'bg-rose-500', desc: 'Tốc độ nhanh, bài tập nhiều' },
]
</script>

<template>
  <PageContainer 
    title="Tổng quan đánh giá giảng viên" 
    subtitle="Báo cáo phân tích chất lượng giảng dạy dựa trên phản hồi trực tiếp từ sinh viên."
  >
    <div class="space-y-8">
      
      <!-- ── KPI Cards ── -->
      <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-6">
        <div v-for="kpi in kpis" :key="kpi.id" class="lg-card-glass p-6 group hover:scale-[1.02] transition-all">
           <div class="flex items-center justify-between mb-4">
              <div :class="['h-12 w-12 rounded-2xl flex items-center justify-center shadow-sm border border-white/50', kpi.bgColor, kpi.color]">
                 <component :is="kpi.icon" :size="24" />
              </div>
              <span class="text-[10px] font-black uppercase tracking-widest text-slate-400">
                 {{ kpi.trend }}
              </span>
           </div>
           <p class="text-xs font-black text-slate-400 uppercase tracking-widest">{{ kpi.label }}</p>
           <h3 class="text-3xl font-black text-slate-800 mt-1">{{ kpi.value }}</h3>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        <!-- ── Rating Trends (Visual Mock) ── -->
        <div class="lg:col-span-2 lg-card-glass p-8 relative overflow-hidden">
           <div class="flex items-center justify-between mb-10">
              <div>
                 <h4 class="text-sm font-black text-slate-800 uppercase tracking-wide">Xu hướng điểm Rating trung bình</h4>
                 <p class="text-xs text-slate-400 mt-1 font-bold">Dữ liệu tổng hợp qua các học kỳ</p>
              </div>
              <button class="lg-button-secondary px-4 py-2 text-[10px] font-black uppercase tracking-widest bg-white/50 border-slate-200">Chi tiết kỳ</button>
           </div>

           <div class="h-64 flex items-end justify-between gap-6 px-4">
              <div v-for="(h, i) in [75, 78, 82, 80, 92]" :key="i" class="flex-1 group relative">
                 <div 
                   :style="{ height: `${h}%` }" 
                   class="w-full bg-gradient-to-t from-amber-500 to-orange-400 rounded-t-2xl shadow-lg shadow-amber-500/10 group-hover:from-amber-400 group-hover:to-orange-300 transition-all duration-500"
                 ></div>
                 <p class="text-center text-[10px] font-black text-slate-400 uppercase mt-4">Kỳ {{ 2021 + i }}</p>
                 <div class="absolute -top-10 left-1/2 -translate-x-1/2 bg-slate-800 text-white text-[10px] font-bold px-2 py-1 rounded opacity-0 group-hover:opacity-100 transition-opacity">
                    {{ (3.5 + (h/40)).toFixed(2) }} ★
                 </div>
              </div>
           </div>
        </div>

        <!-- ── Sentiment Analysis ── -->
        <div class="lg-card-glass p-8">
           <h4 class="text-sm font-black text-slate-800 uppercase tracking-wide mb-8">Phân tích Sentiment AI</h4>
           <div class="space-y-8">
              <div v-for="item in sentiment" :key="item.label">
                 <div class="flex items-center justify-between mb-2">
                    <span class="text-xs font-black text-slate-600 uppercase tracking-tighter">{{ item.label }}</span>
                    <span class="text-xs font-black text-slate-800">{{ item.value }}%</span>
                 </div>
                 <div class="h-2 w-full bg-slate-100 rounded-full overflow-hidden">
                    <div 
                      :style="{ width: `${item.value}%` }" 
                      :class="['h-full rounded-full transition-all duration-1000', item.color]"
                    ></div>
                 </div>
                 <p class="text-[10px] text-slate-400 mt-2 italic font-medium leading-tight">{{ item.desc }}</p>
              </div>
           </div>

           <div class="mt-10 pt-8 border-t border-slate-50">
              <div class="flex items-center gap-4 text-xs font-bold text-slate-400">
                 <PieChart :size="18" class="text-indigo-500" />
                 <p>Hơn <strong>93%</strong> phản hồi ở mức tích cực và trung lập.</p>
              </div>
           </div>
        </div>

      </div>

      <!-- ── Critical Alerts ── -->
      <div class="lg-card-glass p-6 border-rose-100 bg-rose-50/10">
         <div class="flex items-start gap-4">
            <div class="h-10 w-10 rounded-2xl bg-rose-100 flex items-center justify-center text-rose-600 shadow-sm border border-rose-200">
               <ShieldAlert :size="20" />
            </div>
            <div class="flex-1">
               <h4 class="text-sm font-black text-rose-900 uppercase tracking-wide">Cảnh báo giảng viên điểm thấp</h4>
               <p class="text-xs text-rose-700 mt-1 leading-relaxed font-medium">
                 Có <strong>04 giảng viên</strong> có điểm đánh giá trung bình dưới 3.5 và nhận nhiều phản hồi tiêu cực về phương pháp truyền đạt. BGH cần xem xét báo cáo chi tiết để có hướng hỗ trợ.
               </p>
               <button class="mt-4 text-[10px] font-black text-rose-700 uppercase tracking-widest flex items-center gap-1 hover:underline">
                  Xem danh sách cảnh báo <ChevronRight :size="12" />
               </button>
            </div>
         </div>
      </div>

    </div>
  </PageContainer>
</template>

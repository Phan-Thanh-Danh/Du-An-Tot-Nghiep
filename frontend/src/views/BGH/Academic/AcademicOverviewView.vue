<script setup>
import { ref } from 'vue'
import { 
  Users, 
  Award, 
  CheckCircle2, 
  XCircle, 
  TrendingUp, 
  AlertCircle, 
  ChevronRight, 
  BarChart3, 
  PieChart, 
  Filter,
  Download
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock KPIs ────────────────────────────────────────────────
const kpis = [
  { id: 1, label: 'Tổng số sinh viên', value: '1,240', trend: '+5.2%', icon: Users, color: 'text-blue-600', bgColor: 'bg-blue-50' },
  { id: 2, label: 'GPA Trung bình', value: '3.12', trend: '+0.05', icon: Award, color: 'text-indigo-600', bgColor: 'bg-indigo-50' },
  { id: 3, label: 'Tỷ lệ đạt (Pass)', value: '88.4%', trend: '+1.2%', icon: CheckCircle2, color: 'text-emerald-600', bgColor: 'bg-emerald-50' },
  { id: 4, label: 'Nguy cơ rớt môn', value: '42', trend: 'Cần chú ý', icon: AlertCircle, color: 'text-rose-600', bgColor: 'bg-rose-50' },
]

// ── Mock Distribution Data ──────────────────────────────────
const distribution = [
  { range: 'A (8.5 - 10)', count: 185, percent: 15, color: 'bg-emerald-500' },
  { range: 'B (7.0 - 8.4)', count: 420, percent: 34, color: 'bg-blue-500' },
  { range: 'C (5.5 - 6.9)', count: 350, percent: 28, color: 'bg-indigo-400' },
  { range: 'D (4.0 - 5.4)', count: 210, percent: 17, color: 'bg-amber-400' },
  { range: 'F (< 4.0)', count: 75, percent: 6, color: 'bg-rose-500' },
]
</script>

<template>
  <PageContainer 
    title="Tổng quan kết quả học tập" 
    subtitle="Báo cáo phân tích chất lượng học tập và hiệu quả giảng dạy trên toàn hệ thống."
  >
    <template #actions>
       <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2 bg-white/50 border-slate-200">
          <Download :size="18" /> Xuất báo cáo kỳ
       </button>
    </template>

    <div class="space-y-8">
      
      <!-- ── KPI Cards ── -->
      <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-6">
        <div v-for="kpi in kpis" :key="kpi.id" class="lg-card-glass p-6 group hover:scale-[1.02] transition-all">
           <div class="flex items-center justify-between mb-4">
              <div :class="['h-12 w-12 rounded-2xl flex items-center justify-center shadow-sm border border-white/50', kpi.bgColor, kpi.color]">
                 <component :is="kpi.icon" :size="24" />
              </div>
              <span :class="['text-[10px] font-black uppercase tracking-widest px-2 py-1 rounded-lg', kpi.trend.includes('+') ? 'bg-emerald-50 text-emerald-600' : 'bg-rose-50 text-rose-600']">
                 {{ kpi.trend }}
              </span>
           </div>
           <p class="text-xs font-black text-slate-400 uppercase tracking-widest">{{ kpi.label }}</p>
           <h3 class="text-3xl font-black text-slate-800 mt-1">{{ kpi.value }}</h3>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        <!-- ── Chart Section (Visual Mock) ── -->
        <div class="lg:col-span-2 lg-card-glass p-8 overflow-hidden relative">
           <div class="flex items-center justify-between mb-10">
              <div>
                 <h4 class="text-sm font-black text-slate-800 uppercase tracking-wide">Xu hướng GPA theo học kỳ</h4>
                 <p class="text-xs text-slate-400 mt-1 font-bold">Dữ liệu so sánh 5 học kỳ gần nhất</p>
              </div>
              <div class="flex items-center bg-slate-50 rounded-xl p-1 border border-slate-100">
                 <button class="px-4 py-1.5 text-[10px] font-black uppercase tracking-widest bg-white text-blue-600 rounded-lg shadow-sm">Toàn trường</button>
                 <button class="px-4 py-1.5 text-[10px] font-black uppercase tracking-widest text-slate-400 hover:text-slate-600 transition-colors">Theo ngành</button>
              </div>
           </div>

           <!-- Visual Chart Placeholder -->
           <div class="h-64 flex items-end justify-between gap-4 px-4 relative">
              <!-- Grid lines -->
              <div class="absolute inset-0 flex flex-col justify-between pointer-events-none opacity-5">
                 <div v-for="i in 5" :key="i" class="h-px w-full bg-slate-800"></div>
              </div>
              
              <!-- Bars -->
              <div v-for="(h, i) in [40, 55, 48, 70, 85]" :key="i" class="flex-1 group relative cursor-pointer">
                 <div 
                   :style="{ height: `${h}%` }" 
                   class="w-full bg-gradient-to-t from-blue-600 to-indigo-500 rounded-t-2xl shadow-lg shadow-blue-500/10 group-hover:from-blue-500 group-hover:to-indigo-400 transition-all duration-500"
                 ></div>
                 <p class="text-center text-[10px] font-black text-slate-400 uppercase mt-4">Kỳ {{ 2021 + i }}</p>
                 <!-- Tooltip -->
                 <div class="absolute -top-10 left-1/2 -translate-x-1/2 bg-slate-800 text-white text-[10px] font-bold px-2 py-1 rounded opacity-0 group-hover:opacity-100 transition-opacity">
                    GPA: {{ (2.5 + (h/40)).toFixed(2) }}
                 </div>
              </div>
           </div>
        </div>

        <!-- ── Distribution List ── -->
        <div class="lg-card-glass p-8">
           <h4 class="text-sm font-black text-slate-800 uppercase tracking-wide mb-8">Phân phối điểm số</h4>
           <div class="space-y-6">
              <div v-for="item in distribution" :key="item.range">
                 <div class="flex items-center justify-between mb-2">
                    <span class="text-xs font-black text-slate-600 uppercase tracking-tighter">{{ item.range }}</span>
                    <span class="text-xs font-black text-slate-800">{{ item.count }} SV ({{ item.percent }}%)</span>
                 </div>
                 <div class="h-2 w-full bg-slate-100 rounded-full overflow-hidden">
                    <div 
                      :style="{ width: `${item.percent}%` }" 
                      :class="['h-full rounded-full transition-all duration-1000', item.color]"
                    ></div>
                 </div>
              </div>
           </div>
           
           <div class="mt-10 pt-8 border-t border-slate-50">
              <div class="flex items-center gap-4 text-xs font-bold text-slate-400">
                 <PieChart :size="18" class="text-indigo-500" />
                 <p>Hệ số điểm A/B chiếm <strong>49%</strong>, cho thấy chất lượng đào tạo đang ở mức Khá.</p>
              </div>
           </div>
        </div>

      </div>

      <!-- ── Quick Insights ── -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
         <div class="lg-card-glass p-6 bg-gradient-to-br from-amber-50 to-orange-50 border-amber-100">
            <div class="flex items-start gap-4">
               <div class="h-10 w-10 rounded-2xl bg-amber-100 flex items-center justify-center text-amber-600 shadow-sm">
                  <TrendingUp :size="20" />
               </div>
               <div>
                  <h4 class="text-sm font-black text-amber-900 uppercase tracking-wide">Cảnh báo xu hướng rớt môn</h4>
                  <p class="text-xs text-amber-700 mt-1 leading-relaxed font-medium">
                    Tỷ lệ rớt môn (Fail) có dấu hiệu tăng <strong>2.4%</strong> so với kỳ trước, tập trung ở các môn cơ sở ngành như Cấu trúc dữ liệu và Toán rời rạc.
                  </p>
                  <button class="mt-4 text-[10px] font-black text-amber-700 uppercase tracking-widest flex items-center gap-1 hover:underline">
                     Xem chi tiết <ChevronRight :size="12" />
                  </button>
               </div>
            </div>
         </div>
         <div class="lg-card-glass p-6 bg-gradient-to-br from-blue-50 to-indigo-50 border-blue-100">
            <div class="flex items-start gap-4">
               <div class="h-10 w-10 rounded-2xl bg-blue-100 flex items-center justify-center text-blue-600 shadow-sm">
                  <BarChart3 :size="20" />
               </div>
               <div>
                  <h4 class="text-sm font-black text-blue-900 uppercase tracking-wide">Tối ưu hóa GPA</h4>
                  <p class="text-xs text-blue-700 mt-1 leading-relaxed font-medium">
                    Nhóm sinh viên năm 3 có sự bứt phá về GPA với mức tăng trung bình <strong>0.3</strong> điểm nhờ vào việc áp dụng các lớp học thực hành mới.
                  </p>
                  <button class="mt-4 text-[10px] font-black text-blue-700 uppercase tracking-widest flex items-center gap-1 hover:underline">
                     Phân tích dữ liệu <ChevronRight :size="12" />
                  </button>
               </div>
            </div>
         </div>
      </div>

    </div>
  </PageContainer>
</template>

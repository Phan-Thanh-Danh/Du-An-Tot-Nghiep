<script setup>
import { ref } from 'vue'
import { 
  Users, Star, MessageCircle, ShieldAlert, PieChart, ChevronRight, CheckCircle2
} from 'lucide-vue-next'
import { useRouter } from 'vue-router'
import { usePopupStore } from '@/stores/popup'

const router = useRouter()
const popup = usePopupStore()

const kpis = [
  { id: 1, label: 'GV được đánh giá', value: '142', trend: '95% tổng số', icon: Users, color: 'text-(--color-info-text)', bgColor: 'bg-(--color-info-bg)' },
  { id: 2, label: 'Rating Trung bình', value: '4.65', trend: '/ 5.0', icon: Star, color: 'text-(--color-warning-text)', bgColor: 'bg-(--color-warning-bg)' },
  { id: 3, label: 'Số lượt đánh giá', value: '3,840', trend: '+12% kỳ trước', icon: MessageCircle, color: 'text-link', bgColor: 'bg-(--color-info-bg)' },
  { id: 4, label: 'Tỷ lệ phản hồi', value: '72.4%', trend: 'Mục tiêu: 80%', icon: CheckCircle2, color: 'text-(--color-success-text)', bgColor: 'bg-(--color-success-bg)' },
]

const sentiment = [
  { label: 'Tích cực', value: 78, color: 'bg-(--color-success-text)', desc: 'Hài lòng về phương pháp giảng dạy' },
  { label: 'Trung lập', value: 15, color: 'bg-(--text-placeholder)', desc: 'Không có ý kiến đặc biệt' },
  { label: 'Tiêu cực', value: 7, color: 'bg-(--color-danger-text)', desc: 'Tốc độ nhanh, bài tập nhiều' },
]

const trendHistory = [
  { label: 'Kỳ 2021', val: 4.2 },
  { label: 'Kỳ 2022', val: 4.35 },
  { label: 'Kỳ 2023', val: 4.5 },
  { label: 'Kỳ 2024', val: 4.42 },
  { label: 'Kỳ 2025', val: 4.65 },
]

const maxTrend = Math.max(...trendHistory.map(t => t.val), 5)

function viewWarningList() {
  router.push('/bgh/evaluations/ranking')
}
</script>

<template>
  <div class="space-y-8">
    <div class="space-y-8">
      
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

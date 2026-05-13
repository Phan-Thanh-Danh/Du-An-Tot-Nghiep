<script setup>
import { ref } from 'vue'
import { 
  Search, 
  Filter, 
  Trophy, 
  TrendingUp, 
  TrendingDown, 
  Minus, 
  Star, 
  ChevronRight, 
  ShieldCheck, 
  MessageCircle,
  Building2
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const rankings = ref([
  { id: 1, name: 'Nguyễn Văn A', dept: 'Khoa CNTT', evals: 86, avgScore: 4.92, positive: 96, negative: 1, trend: 'up' },
  { id: 2, name: 'Trần Thị B', dept: 'Khoa Ngoại ngữ', evals: 42, avgScore: 4.85, positive: 94, negative: 2, trend: 'stable' },
  { id: 3, name: 'Lê Văn C', dept: 'Khoa Kinh tế', evals: 120, avgScore: 4.78, positive: 90, negative: 4, trend: 'up' },
  { id: 4, name: 'Hoàng Thị D', dept: 'Khoa CNTT', evals: 35, avgScore: 4.70, positive: 88, negative: 5, trend: 'down' },
  { id: 5, name: 'Phạm Minh E', dept: 'Khoa Kinh tế', evals: 58, avgScore: 4.65, positive: 85, negative: 6, trend: 'stable' },
])

const getTrendIcon = (trend) => {
  switch (trend) {
    case 'up': return TrendingUp
    case 'down': return TrendingDown
    default: return Minus
  }
}

const getTrendColor = (trend) => {
  switch (trend) {
    case 'up': return 'text-emerald-500'
    case 'down': return 'text-rose-500'
    default: return 'text-slate-300'
  }
}
</script>

<template>
  <PageContainer 
    title="Xếp hạng giảng viên" 
    subtitle="Bảng xếp hạng chất lượng dựa trên điểm đánh giá tổng hợp và phản hồi từ sinh viên."
  >
    <div class="space-y-6">
      
      <!-- ── Ranking Logic Info ── -->
      <div class="lg-card-glass p-5 border-blue-100 bg-blue-50/10 flex items-center gap-4">
         <div class="h-10 w-10 rounded-2xl bg-blue-100 flex items-center justify-center text-blue-600 shrink-0">
            <ShieldCheck :size="20" />
         </div>
         <p class="text-xs text-blue-700 font-medium leading-relaxed">
           <strong>Quy tắc xếp hạng:</strong> Chỉ hiển thị kết quả cho các giảng viên có từ <strong>5 lượt đánh giá</strong> trở lên để đảm bảo tính khách quan. Danh tính sinh viên đánh giá luôn được ẩn danh đối với tất cả các cấp quản lý.
         </p>
      </div>

      <!-- ── Toolbar ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex items-center gap-4 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
              <input type="text" placeholder="Tìm tên giảng viên hoặc khoa..." class="w-full bg-white border border-slate-200 rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10">
           </div>
           <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
              <Filter :size="18" /> Tất cả các Khoa
           </button>
        </div>
        <select class="bg-white border border-slate-200 rounded-xl px-4 py-2.5 text-xs font-bold outline-none">
           <option>Kỳ Spring 2026</option>
           <option>Kỳ Fall 2025</option>
        </select>
      </div>

      <!-- ── Ranking Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50">
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100 w-16 text-center">Hạng</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Giảng viên & Khoa</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Điểm Rating</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Phản hồi (Sentiment)</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Xu hướng</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="(gv, index) in rankings" :key="gv.id" class="group hover:bg-white/50 transition-colors">
              <td class="px-6 py-4 text-center">
                 <div v-if="index < 3" class="flex justify-center">
                    <div :class="['h-8 w-8 rounded-full flex items-center justify-center shadow-sm', index === 0 ? 'bg-amber-100 text-amber-600' : index === 1 ? 'bg-slate-100 text-slate-500' : 'bg-orange-50 text-orange-600']">
                       <Trophy :size="16" />
                    </div>
                 </div>
                 <span v-else class="text-sm font-black text-slate-400">#{{ index + 1 }}</span>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-10 w-10 rounded-full bg-slate-100 flex items-center justify-center font-black text-[10px] text-slate-400">GV</div>
                  <div>
                    <p class="text-sm font-black text-slate-800 leading-tight group-hover:text-blue-600 transition-colors">{{ gv.name }}</p>
                    <p class="text-[10px] font-bold text-slate-400 mt-1 flex items-center gap-1 uppercase tracking-tighter">
                       <Building2 :size="12" /> {{ gv.dept }}
                    </p>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4">
                 <div class="flex items-center gap-1.5">
                    <Star :size="14" class="text-amber-500 fill-amber-500" />
                    <span class="text-sm font-black text-slate-800">{{ gv.avgScore.toFixed(2) }}</span>
                    <span class="text-[10px] font-bold text-slate-400 ml-1">({{ gv.evals }} lượt)</span>
                 </div>
              </td>
              <td class="px-6 py-4">
                 <div class="flex flex-col gap-1 w-32">
                    <div class="flex justify-between text-[9px] font-black uppercase tracking-widest">
                       <span class="text-emerald-600">{{ gv.positive }}% Pos</span>
                       <span class="text-rose-500">{{ gv.negative }}% Neg</span>
                    </div>
                    <div class="h-1.5 w-full bg-slate-100 rounded-full overflow-hidden flex">
                       <div :style="{ width: `${gv.positive}%` }" class="bg-emerald-500 h-full"></div>
                       <div :style="{ width: `${gv.negative}%` }" class="bg-rose-500 h-full"></div>
                    </div>
                 </div>
              </td>
              <td class="px-6 py-4">
                 <div :class="['flex items-center gap-1.5', getTrendColor(gv.trend)]">
                    <component :is="getTrendIcon(gv.trend)" :size="16" />
                    <span class="text-[10px] font-black uppercase tracking-widest">{{ gv.trend }}</span>
                 </div>
              </td>
              <td class="px-6 py-4 text-right">
                <button class="p-2 hover:bg-blue-50 hover:text-blue-600 rounded-lg text-slate-400 transition-all">
                  <ChevronRight :size="18" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

    </div>
  </PageContainer>
</template>

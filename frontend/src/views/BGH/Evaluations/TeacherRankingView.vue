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
    case 'up': return 'text-(--color-success-text)'
    case 'down': return 'text-(--color-danger-text)'
    default: return 'text-muted'
  }
}
</script>

<template>
  <PageContainer 
    title="Xếp hạng giảng viên" 
    subtitle="Bảng xếp hạng chất lượng dựa trên điểm đánh giá tổng hợp và phản hồi từ sinh viên."
  >
    <div class="space-y-4">
      
      <!-- ── Ranking Logic Info ── -->
      <div class="surface-card border border-(--color-info-text)/20 bg-(--color-info-bg) rounded-2xl p-5 flex items-center gap-4">
         <div class="h-10 w-10 rounded-2xl bg-(--surface-card) flex items-center justify-center text-(--color-info-text) shrink-0 border border-(--color-info-text)/20">
            <ShieldCheck :size="20" />
         </div>
         <p class="text-xs text-(--color-info-text) font-medium leading-relaxed">
           <strong>Quy tắc xếp hạng:</strong> Chỉ hiển thị kết quả cho các giảng viên có từ <strong>5 lượt đánh giá</strong> trở lên để đảm bảo tính khách quan. Danh tính sinh viên đánh giá luôn được ẩn danh đối với tất cả các cấp quản lý.
         </p>
      </div>

      <!-- ── Toolbar ── -->
      <div class="surface-card border border-card rounded-2xl p-4 flex flex-wrap items-center justify-between gap-3">
        <div class="flex flex-wrap items-center gap-3 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
              <input type="text" placeholder="Tìm tên giảng viên hoặc khoa..." class="w-full surface-input border border-input rounded-xl pl-9 pr-4 py-2 text-sm font-medium outline-none focus:border-(--border-input-focus)">
           </div>
           <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
              <Filter :size="18" /> Tất cả các Khoa
           </button>
        </div>
        <select class="surface-input border border-input rounded-xl px-3 py-2 text-xs font-bold outline-none">
           <option>Kỳ Spring 2026</option>
           <option>Kỳ Fall 2025</option>
        </select>
      </div>

      <!-- ── Ranking Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default w-16 text-center">Hạng</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Giảng viên & Khoa</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Điểm Rating</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Phản hồi (Sentiment)</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Xu hướng</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="(gv, index) in rankings" :key="gv.id" class="group hover:bg-(--surface-input) transition-colors">
              <td class="px-4 py-3 text-center">
                 <div v-if="index < 3" class="flex justify-center">
                    <div :class="['h-8 w-8 rounded-full flex items-center justify-center shadow-sm border', index === 0 ? 'bg-(--color-warning-bg) text-(--color-warning-text) border-(--color-warning-text)/20' : index === 1 ? 'surface-solid text-muted border-default' : 'bg-(--color-info-bg) text-(--color-info-text) border-(--color-info-text)/20']">
                       <Trophy :size="16" />
                    </div>
                 </div>
                 <span v-else class="text-sm font-semibold text-muted">#{{ index + 1 }}</span>
              </td>
              <td class="px-4 py-3">
                <div class="flex items-center gap-3">
                  <div class="h-10 w-10 rounded-full surface-solid border border-default flex items-center justify-center font-semibold text-[10px] text-muted">GV</div>
                  <div>
                    <p class="text-sm font-semibold text-heading leading-tight group-hover:text-link transition-colors">{{ gv.name }}</p>
                    <p class="text-[10px] font-bold text-muted mt-1 flex items-center gap-1 uppercase tracking-tighter">
                       <Building2 :size="12" /> {{ gv.dept }}
                    </p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-3">
                 <div class="flex items-center gap-1.5">
                    <Star :size="14" class="text-(--color-warning-text) fill-(--color-warning-text)" />
                    <span class="text-sm font-semibold text-heading">{{ gv.avgScore.toFixed(2) }}</span>
                    <span class="text-[10px] font-bold text-muted ml-1">({{ gv.evals }} lượt)</span>
                 </div>
              </td>
              <td class="px-4 py-3">
                 <div class="flex flex-col gap-1 w-32">
                    <div class="flex justify-between text-[9px] font-semibold uppercase tracking-widest">
                       <span class="text-(--color-success-text)">{{ gv.positive }}% Pos</span>
                       <span class="text-(--color-danger-text)">{{ gv.negative }}% Neg</span>
                    </div>
                    <div class="h-1.5 w-full bg-(--surface-input) rounded-full overflow-hidden flex">
                       <div :style="{ width: `${gv.positive}%` }" class="bg-(--color-success-text) h-full"></div>
                       <div :style="{ width: `${gv.negative}%` }" class="bg-(--color-danger-text) h-full"></div>
                    </div>
                 </div>
              </td>
              <td class="px-4 py-3">
                 <div :class="['flex items-center gap-1.5', getTrendColor(gv.trend)]">
                    <component :is="getTrendIcon(gv.trend)" :size="16" />
                    <span class="text-[10px] font-semibold uppercase tracking-widest">{{ gv.trend }}</span>
                 </div>
              </td>
              <td class="px-4 py-3 text-right">
                <button class="p-2 hover:bg-(--color-info-bg) hover:text-(--color-info-text) rounded-lg text-muted transition-all">
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

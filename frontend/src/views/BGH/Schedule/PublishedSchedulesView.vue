<script setup>
import { CheckCircle2, Eye, Search } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
</script>
<template>
  <div class="h-full flex flex-col space-y-4">
    <div class="flex items-start justify-between">
      <div>
        <div class="flex items-center gap-2">
          <CheckCircle2 class="text-(--lg-primary)" :size="24" />
          <h1 class="text-xl font-bold text-(--text-heading)">Thời khóa biểu đã duyệt</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Tra cứu và giám sát toàn bộ lịch học chính thức đang chạy.</p>
      </div>
      <div class="flex gap-2">
        <div class="relative">
          <Search class="absolute left-2.5 top-1/2 -translate-y-1/2 text-(--text-muted)" :size="14" />
          <input type="text" placeholder="Tìm khoa, mã lớp..." class="pl-8 pr-3 h-9 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm text-(--text-body) outline-none focus:ring-2 focus:ring-(--border-focus) w-64" />
        </div>
      </div>
    </div>

    <div class="flex-1 surface-card border border-(--border-card) rounded-2xl overflow-auto p-4">
       <!-- Mock content for published schedules (Global view) -->
       <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div v-for="i in 6" :key="i" class="surface-card border border-(--border-default) rounded-xl p-4 hover:shadow-md transition-shadow relative overflow-hidden">
             <div class="absolute top-0 left-0 right-0 h-1 bg-emerald-500"></div>
             <div class="flex justify-between items-start mb-2">
               <h3 class="font-bold text-(--text-heading)">Khoa Công nghệ Thông tin</h3>
               <GlassBadge variant="success" size="xs">Đang áp dụng</GlassBadge>
             </div>
             <p class="text-xs text-(--text-muted) mb-4">Học kỳ 1 - 2026 • Chính quy</p>
             <div class="space-y-2 text-sm text-(--text-body)">
                <div class="flex justify-between"><span class="text-(--text-muted)">Số lớp HP:</span> <strong>120</strong></div>
                <div class="flex justify-between"><span class="text-(--text-muted)">Tổng số giờ:</span> <strong>3600h</strong></div>
             </div>
             <div class="mt-4 pt-3 border-t border-(--border-default)">
                <GlassButton variant="secondary" class="w-full justify-center text-link"><Eye :size="14" class="mr-1"/>Xem lịch biểu</GlassButton>
             </div>
          </div>
       </div>
    </div>
  </div>
</template>

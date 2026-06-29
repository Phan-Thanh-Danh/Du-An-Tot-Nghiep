<script setup>
import { AlertTriangle } from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
</script>
<template>
  <div class="h-full flex flex-col space-y-4">
    <div class="flex items-start justify-between">
      <div>
        <div class="flex items-center gap-2">
          <AlertTriangle class="text-(--lg-primary)" :size="24" />
          <h1 class="text-xl font-bold text-(--text-heading)">Giám sát Xung đột</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Theo dõi tổng thể các lỗi tài nguyên giảng dạy và không gian.</p>
      </div>
      <GlassButton variant="secondary"><Filter :size="15" class="mr-1" /> Bộ lọc</GlassButton>
    </div>

    <div class="flex-1 surface-card border border-(--border-card) rounded-2xl flex flex-col items-center justify-center p-8 text-center text-(--text-muted)">
       <AlertTriangle :size="48" class="text-emerald-500/50 mb-4" />
       <h3 class="text-lg font-bold text-(--text-heading)">Hệ thống hoạt động ổn định</h3>
       <p class="mt-2 text-sm max-w-md mx-auto">Chưa phát hiện xung đột lịch nào ở cấp toàn trường. Các giáo vụ khoa đã xử lý tốt ở cấp đơn vị.</p>
    </div>
  </div>
</template>

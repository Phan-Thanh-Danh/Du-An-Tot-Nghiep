<script setup>
import GlassPanel from '@/components/ui/GlassPanel.vue'
import { BarChart3, TrendingUp, Clock, CheckCircle } from 'lucide-vue-next'
</script>

<template>
  <div class="h-full bg-[var(--surface-page)] p-6">
    <div class="mb-6">
      <h1 class="text-2xl font-bold text-[var(--text-heading)]">Báo cáo đơn từ</h1>
      <p class="text-[var(--text-muted)] mt-1">Tổng quan và phân tích hiệu suất xử lý đơn từ (Advanced Reports)</p>
    </div>

    <!-- Summary Cards -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
      <GlassPanel padding="normal" class="flex items-center gap-4 border-l-4 border-[var(--lg-primary)]">
        <div class="p-3 bg-[var(--lg-primary)] bg-opacity-10 rounded-full text-[var(--lg-primary)]"><BarChart3 class="w-6 h-6" /></div>
        <div><div class="text-xs text-[var(--text-muted)] uppercase tracking-wider font-semibold mb-1">Tổng tiếp nhận</div><div class="text-2xl font-bold text-[var(--text-heading)]">1,245</div></div>
      </GlassPanel>
      <GlassPanel padding="normal" class="flex items-center gap-4 border-l-4 border-yellow-500">
        <div class="p-3 bg-yellow-500 bg-opacity-10 rounded-full text-yellow-500"><Clock class="w-6 h-6" /></div>
        <div><div class="text-xs text-[var(--text-muted)] uppercase tracking-wider font-semibold mb-1">Đang xử lý</div><div class="text-2xl font-bold text-[var(--text-heading)]">84</div></div>
      </GlassPanel>
      <GlassPanel padding="normal" class="flex items-center gap-4 border-l-4 border-[var(--color-success-border)]">
        <div class="p-3 bg-[var(--color-success-bg)] rounded-full text-[var(--color-success-text)]"><CheckCircle class="w-6 h-6" /></div>
        <div><div class="text-xs text-[var(--text-muted)] uppercase tracking-wider font-semibold mb-1">Đã hoàn thành</div><div class="text-2xl font-bold text-[var(--text-heading)]">1,120</div></div>
      </GlassPanel>
      <GlassPanel padding="normal" class="flex items-center gap-4 border-l-4 border-[var(--color-danger-border)]">
        <div class="p-3 bg-[var(--color-danger-bg)] rounded-full text-[var(--color-danger-text)]"><TrendingUp class="w-6 h-6" /></div>
        <div><div class="text-xs text-[var(--text-muted)] uppercase tracking-wider font-semibold mb-1">Quá hạn xử lý</div><div class="text-2xl font-bold text-[var(--text-heading)]">12</div></div>
      </GlassPanel>
    </div>

    <!-- Placeholder for charts -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <GlassPanel class="h-80 flex items-center justify-center text-[var(--text-muted)]">
        Biểu đồ loại đơn từ (Chưa tích hợp chart library)
      </GlassPanel>
      <GlassPanel class="h-80 flex items-center justify-center text-[var(--text-muted)]">
        Biểu đồ hiệu suất nhân sự (Chưa tích hợp chart library)
      </GlassPanel>
    </div>
  </div>
</template>

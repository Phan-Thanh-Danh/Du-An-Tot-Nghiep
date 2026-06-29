<script setup>
import { AlertCircle } from 'lucide-vue-next'
</script>
<template>
  <div class="bg-[var(--surface-hover)] border-l-4 border-[var(--lg-primary)] px-4 py-2 text-sm text-[var(--text-muted)] flex items-center gap-2 rounded-r-lg mb-4">
    <AlertCircle class="w-4 h-4 text-[var(--lg-primary)]" />
    <span>Dữ liệu demo — Chưa kết nối API thật. Báo cáo này sử dụng dữ liệu Mock.</span>
  </div>
</template>

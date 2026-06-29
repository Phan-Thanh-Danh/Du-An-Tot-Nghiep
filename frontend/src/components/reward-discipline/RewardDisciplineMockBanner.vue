<script setup>
import { AlertCircle } from 'lucide-vue-next'
</script>
<template>
  <div class="bg-(--surface-hover) border-l-4 border-(--lg-primary) px-4 py-2 text-sm text-(--text-muted) flex items-center gap-2 rounded-r-lg mb-4">
    <AlertCircle class="w-4 h-4 text-(--lg-primary)" />
    <span>Dữ liệu demo — Chưa kết nối API thật. Các thao tác chỉ có tác dụng mô phỏng (Mock UI).</span>
  </div>
</template>

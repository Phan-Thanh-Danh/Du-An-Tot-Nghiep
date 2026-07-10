<script setup>
import { AlertTriangle, ServerCog } from 'lucide-vue-next'

defineProps({
  title: { type: String, required: true },
  subtitle: { type: String, default: '' },
  requiredApi: { type: String, required: true },
  decision: { type: String, default: 'NEEDS_BE_ENDPOINT' },
})
</script>

<template>
  <section class="space-y-4">
    <header>
      <p class="text-sm font-semibold text-label">SuperAdmin</p>
      <h1 class="text-2xl font-bold text-heading">{{ title }}</h1>
      <p v-if="subtitle" class="mt-1 text-sm text-body">{{ subtitle }}</p>
    </header>

    <div class="rounded-lg border border-card surface-card p-6">
      <div class="flex items-start gap-4">
        <ServerCog class="mt-1 h-6 w-6 text-link" />
        <div class="space-y-2">
          <p class="text-base font-semibold text-heading">Chưa bật dữ liệu nghiệp vụ trên frontend</p>
          <p class="text-sm text-body">
            Màn này đã được dọn khỏi dữ liệu cục bộ. Cần bổ sung endpoint backend thật trước khi đưa vào claim full action/API coverage.
          </p>
          <div class="rounded-lg border border-card bg-[var(--surface-muted)] p-3 text-sm text-body">
            <p><span class="font-semibold text-heading">Endpoint cần có:</span> {{ requiredApi }}</p>
            <p><span class="font-semibold text-heading">P16 status:</span> {{ decision }}</p>
          </div>
        </div>
      </div>
    </div>

    <div class="rounded-lg border border-[var(--color-warning-text)] bg-[var(--color-warning-bg)] p-4 text-sm text-[var(--color-warning-text)]">
      <div class="flex items-start gap-2">
        <AlertTriangle class="mt-0.5 h-4 w-4" />
        <span>Không có nút ghi dữ liệu nào được bật cho đến khi API thật được xác nhận.</span>
      </div>
    </div>
  </section>
</template>

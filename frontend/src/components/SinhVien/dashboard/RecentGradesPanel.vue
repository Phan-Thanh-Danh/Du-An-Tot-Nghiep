<script setup>
import { ArrowRight, Award } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'

defineProps({
  grades: {
    type: Array,
    default: () => [],
  },
})

function scoreClass(score) {
  const value = Number.parseFloat(score)
  if (value >= 8) return 'bg-[var(--color-success-bg)] text-[var(--color-success-text)] border-[color-mix(in srgb,var(--color-success-text) 20%,transparent)]'
  if (value >= 6.5) return 'bg-[var(--color-warning-bg)] text-[var(--color-warning-text)] border-[color-mix(in srgb,var(--color-warning-text) 20%,transparent)]'
  return 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)] border-[color-mix(in srgb,var(--color-danger-text) 20%,transparent)]'
}
</script>

<template>
  <GlassPanel variant="strong" density="none" class="rounded-2xl">
    <div class="flex items-center justify-between gap-3 border-b border-card px-4 py-3.5">
      <div>
        <h2 class="text-base font-semibold text-heading">Điểm gần đây</h2>
        <p class="text-xs font-medium text-body">Vừa công bố</p>
      </div>
      <Award :size="18" class="text-[var(--accent-violet)]" />
    </div>

    <div class="space-y-2 p-4">
      <router-link
        v-for="grade in grades"
        :key="grade.id"
        to="/student/grades"
        class="lg-list-item flex min-h-[64px] items-center justify-between gap-3.5 p-3"
      >
        <div class="min-w-0 flex-1">
          <h3 class="truncate text-[13px] font-semibold text-heading leading-tight">{{ grade.course }}</h3>
          <p class="mt-0.5 text-xs font-medium text-body">{{ grade.type }}</p>
        </div>
        <div :class="['flex h-9 min-w-9 flex-shrink-0 flex-col items-center justify-center rounded-xl border px-2 shadow-sm', scoreClass(grade.score)]">
          <p class="text-[14px] font-semibold leading-none">{{ grade.score }}</p>
          <p class="mt-0.5 text-[10px] font-medium opacity-80">đ</p>
        </div>
      </router-link>
    </div>

    <div class="border-t border-card px-5 py-3 bg-[var(--surface-card)]">
      <router-link to="/student/grades" class="inline-flex items-center gap-1.5 text-xs font-semibold text-link transition-colors">
        Xem bảng điểm
        <ArrowRight :size="12" />
      </router-link>
    </div>
  </GlassPanel>
</template>

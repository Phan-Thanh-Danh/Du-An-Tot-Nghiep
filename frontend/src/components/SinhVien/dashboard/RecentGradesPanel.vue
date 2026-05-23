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
  if (value >= 8) return 'bg-emerald-50 dark:bg-emerald-600/20 text-emerald-700 dark:text-emerald-300 border-emerald-100 dark:border-emerald-500/30'
  if (value >= 6.5) return 'bg-amber-50 dark:bg-amber-600/20 text-amber-700 dark:text-amber-300 border-amber-100 dark:border-amber-500/30'
  return 'bg-red-50 dark:bg-red-600/20 text-red-700 dark:text-red-300 border-red-100 dark:border-red-500/30'
}
</script>

<template>
  <GlassPanel density="none" class="rounded-[28px]">
    <div class="flex items-center justify-between gap-3 border-b border-white/45 dark:border-white/10 px-4 py-3.5">
      <div>
        <h2 class="text-base font-bold text-slate-950 dark:text-slate-100">Điểm gần đây</h2>
        <p class="text-xs font-medium text-slate-500 dark:text-slate-400">Vừa công bố</p>
      </div>
      <Award :size="18" class="text-violet-700 dark:text-violet-400" />
    </div>

    <div class="space-y-2 p-4">
      <router-link
        v-for="grade in grades"
        :key="grade.id"
        to="/student/grades"
        class="lg-list-item flex min-h-[64px] items-center justify-between gap-3.5 p-3"
      >
        <div class="min-w-0 flex-1">
          <h3 class="truncate text-[13px] font-bold text-slate-950 dark:text-slate-100 leading-tight">{{ grade.course }}</h3>
          <p class="mt-0.5 text-xs font-medium text-slate-500 dark:text-slate-400">{{ grade.type }}</p>
        </div>
        <div :class="['flex h-9 min-w-9 flex-shrink-0 flex-col items-center justify-center rounded-xl border px-2 shadow-sm', scoreClass(grade.score)]">
          <p class="text-[14px] font-bold leading-none">{{ grade.score }}</p>
          <p class="mt-0.5 text-[10px] font-medium opacity-80">đ</p>
        </div>
      </router-link>
    </div>

    <div class="border-t border-white/45 dark:border-white/10 px-5 py-3 bg-white/30 dark:bg-slate-700/20">
      <router-link to="/student/grades" class="inline-flex items-center gap-1.5 text-xs font-semibold text-blue-600 dark:text-blue-400 transition-colors hover:text-blue-800 dark:hover:text-blue-300">
        Xem bảng điểm
        <ArrowRight :size="12" />
      </router-link>
    </div>
  </GlassPanel>
</template>

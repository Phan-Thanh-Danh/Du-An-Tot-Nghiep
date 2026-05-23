<script setup>
import { CalendarDays, MapPin } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

defineProps({
  schedule: {
    type: Array,
    default: () => [],
  },
})
</script>

<template>
  <GlassPanel density="none" class="rounded-[28px]">
    <div class="flex items-center justify-between gap-3 border-b border-white/45 dark:border-white/10 px-4 py-3.5">
      <div>
        <h2 class="text-base font-bold text-slate-950 dark:text-slate-100">Lịch hôm nay</h2>
        <p class="text-xs font-medium text-slate-500 dark:text-slate-400">Thứ sáu, 15/05/2026</p>
      </div>
      <CalendarDays :size="18" class="text-cyan-700 dark:text-cyan-400" />
    </div>

    <div class="space-y-2 p-4">
      <article
        v-for="item in schedule"
        :key="item.id"
        :class="[
          'grid min-h-[80px] grid-cols-[18px_minmax(0,1fr)] gap-3 rounded-[20px] border p-3.5 transition-all duration-300',
          item.current
            ? 'border-blue-200 dark:border-blue-500/30 bg-blue-50/80 dark:bg-blue-600/10 shadow-md ring-1 ring-blue-100 dark:ring-blue-500/20'
            : 'border-white/60 dark:border-white/10 bg-white/70 dark:bg-slate-700/40 hover:bg-white/90 dark:hover:bg-slate-700/60',
        ]"
      >
        <div class="flex flex-col items-center">
          <span
            :class="[
              'mt-1 h-3.5 w-3.5 rounded-full border-2 border-white dark:border-slate-600 shadow-sm transition-colors',
              item.current ? 'bg-blue-600 dark:bg-blue-500' : 'bg-slate-300 dark:bg-slate-600',
            ]"
          />
          <span class="mt-1 h-full w-px bg-gradient-to-b from-blue-200 dark:from-blue-500/30 to-transparent" />
        </div>
        <div class="flex items-start justify-between gap-2">
          <div class="min-w-0 flex-1">
            <div class="flex items-center gap-2">
              <p class="inline-flex rounded-full border border-blue-50 dark:border-blue-500/30 bg-white/80 dark:bg-slate-700/40 px-2 py-0.5 text-xs font-semibold text-blue-700 dark:text-blue-300 shadow-sm">
                {{ item.time }}
              </p>
              <span v-if="item.current" class="flex h-1.5 w-1.5 rounded-full bg-blue-600 dark:bg-blue-500" />
            </div>
            <h3 class="mt-1.5 truncate text-[13px] font-bold text-slate-950 dark:text-slate-100 leading-tight">{{ item.subject }}</h3>
            <p class="mt-1 flex items-center gap-1.5 truncate text-xs font-medium text-slate-500 dark:text-slate-400">
              <MapPin :size="11" />
              {{ item.room }} · {{ item.lecturer }}
            </p>
          </div>
          <GlassBadge :variant="item.variant" size="sm">{{ item.status }}</GlassBadge>
        </div>
      </article>
    </div>
  </GlassPanel>
</template>

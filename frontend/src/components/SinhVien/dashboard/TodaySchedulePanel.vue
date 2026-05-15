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
  <GlassPanel class="rounded-[28px] overflow-hidden" padding="p-0">
    <div class="flex items-center justify-between gap-3 border-b border-white/45 px-5 py-4">
      <div>
        <h2 class="text-base font-bold text-slate-950">Lịch hôm nay</h2>
        <p class="text-[11px] font-bold text-slate-500 uppercase tracking-wider">Thứ sáu, 15/05/2026</p>
      </div>
      <CalendarDays :size="18" class="text-cyan-700" />
    </div>

    <div class="relative space-y-2 p-4">
      <div class="absolute bottom-6 left-[27px] top-6 w-px bg-gradient-to-b from-blue-200 via-cyan-100 to-transparent" />
      <article
        v-for="item in schedule"
        :key="item.id"
        :class="[
          'relative ml-7 flex min-h-[78px] flex-col justify-center rounded-[20px] border p-3.5 transition-all duration-300',
          item.current
            ? 'border-blue-200 bg-blue-50/80 shadow-md ring-1 ring-blue-100'
            : 'border-white/60 bg-white/70 hover:bg-white/90',
        ]"
      >
        <span
          :class="[
            'absolute -left-[32px] top-1/2 h-3.5 w-3.5 -translate-y-1/2 rounded-full border-2 border-white shadow-sm transition-colors',
            item.current ? 'bg-blue-600' : 'bg-slate-300',
          ]"
        />
        <div class="flex items-start justify-between gap-2">
          <div class="min-w-0 flex-1">
            <div class="flex items-center gap-2">
              <p class="inline-flex rounded-full bg-white/80 px-2 py-0.5 text-[9px] font-bold uppercase tracking-wider text-blue-700 shadow-sm border border-blue-50">
                {{ item.time }}
              </p>
              <span v-if="item.current" class="flex h-1.5 w-1.5 rounded-full bg-blue-600 animate-pulse" />
            </div>
            <h3 class="mt-1.5 truncate text-[13px] font-bold text-slate-950 leading-tight">{{ item.subject }}</h3>
            <p class="mt-1 flex items-center gap-1.5 text-[10px] font-bold text-slate-400">
              <MapPin :size="11" />
              {{ item.room }}
            </p>
          </div>
          <GlassBadge :variant="item.variant" size="sm">{{ item.status }}</GlassBadge>
        </div>
      </article>
    </div>
  </GlassPanel>
</template>

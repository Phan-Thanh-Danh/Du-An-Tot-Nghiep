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
  <GlassPanel variant="strong" density="none" class="rounded-2xl">
    <div class="flex items-center justify-between gap-3 border-b border-card px-4 py-3.5">
      <div>
        <h2 class="text-base font-semibold text-heading">Lịch hôm nay</h2>
        <p class="text-xs font-medium text-body">Thứ sáu, 15/05/2026</p>
      </div>
      <CalendarDays :size="18" class="text-[var(--lg-cyan)]" />
    </div>

    <div class="space-y-2 p-4">
      <article
        v-for="item in schedule"
        :key="item.id"
        :class="[
          'grid min-h-[80px] grid-cols-[18px_minmax(0,1fr)] gap-3 rounded-[20px] border p-3.5 transition-all duration-300',
          item.current
            ? 'border-[color-mix(in srgb,var(--text-link) 20%,transparent)] bg-[var(--color-info-bg)] shadow-md ring-1 ring-[color-mix(in srgb,var(--text-link) 20%,transparent)]'
            : 'border-card bg-[var(--surface-card)] hover:bg-[var(--surface-card)]',
        ]"
      >
          <div class="flex flex-col items-center">
          <span
            :class="[
              'mt-1 h-3.5 w-3.5 rounded-full border-2 border-white shadow-sm transition-colors',
              item.current ? 'bg-[var(--text-link)]' : 'bg-[var(--text-placeholder)]',
            ]"
          />
          <span class="mt-1 h-full w-px bg-gradient-to-b from-[var(--text-link)]/20 to-transparent" />
        </div>
        <div class="flex items-start justify-between gap-2">
          <div class="min-w-0 flex-1">
            <div class="flex items-center gap-2">
              <p class="inline-flex rounded-full border border-[color-mix(in srgb,var(--text-link) 20%,transparent)] bg-[var(--surface-card)] px-2 py-0.5 text-xs font-semibold text-link shadow-sm">
                {{ item.time }}
              </p>
              <span v-if="item.current" class="flex h-1.5 w-1.5 rounded-full bg-[var(--text-link)]" />
            </div>
            <h3 class="mt-1.5 truncate text-[13px] font-semibold text-heading leading-tight">{{ item.subject }}</h3>
            <p class="mt-1 flex items-center gap-1.5 truncate text-xs font-medium text-body">
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

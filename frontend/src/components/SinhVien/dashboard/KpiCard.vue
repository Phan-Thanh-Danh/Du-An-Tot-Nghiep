<script setup>
import { computed } from 'vue'
import {
  ArrowUpRight,
  BookOpen,
  ClipboardList,
  GraduationCap,
  TrendingUp,
  UserCheck,
} from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'

const props = defineProps({
  item: {
    type: Object,
    required: true,
  },
})

const icons = {
  courses: BookOpen,
  assignments: ClipboardList,
  gpa: GraduationCap,
  attendance: UserCheck,
}

const IconComponent = computed(() => icons[props.item.id] || TrendingUp)

const toneClass = 'bg-[var(--accent-primary-soft)] text-[var(--text-link)] shadow-[var(--lg-shadow-sm)]'

const valueColorClass = computed(
  () =>
    ({
      blue: 'text-[var(--text-link)]',
      amber: 'text-[var(--color-warning-text)]',
      violet: 'text-[var(--accent-violet)]',
      teal: 'text-[var(--color-success-text)]',
    })[props.item.tone] || 'text-[var(--text-link)]',
)

const tintClass = 'bg-[var(--accent-primary-soft)] dark:bg-[var(--accent-primary-soft)]'

const trendClass = 'text-muted bg-[var(--surface-input)] border-[var(--border-card)]'
</script>

<template>
  <router-link :to="item.route" class="group block h-full rounded-xl focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[var(--text-link)]/30">
    <GlassPanel interactive variant="strong" density="none" class="relative h-full rounded-xl kpi-route-tile overflow-hidden border border-card shadow-[var(--lg-shadow-md)]">
      <div :class="['pointer-events-none absolute -right-6 -top-6 h-16 w-16 rounded-full blur-xl opacity-25', tintClass]" />

      <div class="flex h-full flex-col justify-between p-4">
        <div class="flex items-start justify-between mb-3">
          <div :class="['flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-[12px] transition-transform duration-300 group-hover:scale-105', toneClass]">
            <component :is="IconComponent" :size="18" stroke-width="2" />
          </div>
          <span class="flex h-6 w-6 items-center justify-center rounded-full border border-card bg-[var(--surface-card)] text-link shadow-sm transition-all duration-300 group-hover:bg-[var(--text-link)] group-hover:text-white group-hover:border-[var(--text-link)]" aria-hidden="true">
            <ArrowUpRight :size="12" stroke-width="2.5" />
          </span>
        </div>

        <div class="space-y-1 mt-auto">
          <p :class="['text-2xl font-semibold tracking-tight leading-none', valueColorClass]">
            {{ item.value }}
          </p>
          <p class="text-[13px] font-medium text-body tracking-wide">
            {{ item.label }}
          </p>
          <div class="pt-1.5 flex items-center">
            <span :class="['inline-flex items-center justify-center rounded-full border px-2 py-0.5 text-[10px] transition-colors', trendClass]">
              {{ item.trend }}
            </span>
          </div>
        </div>
      </div>
    </GlassPanel>
  </router-link>
</template>

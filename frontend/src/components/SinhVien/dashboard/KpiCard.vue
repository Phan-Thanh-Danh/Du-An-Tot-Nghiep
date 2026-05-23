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

const toneClass = computed(
  () =>
    ({
      blue: 'bg-blue-600 dark:bg-blue-600/80 text-white shadow-blue-500/20 dark:shadow-blue-500/10',
      amber: 'bg-orange-500 dark:bg-orange-500/80 text-white shadow-orange-500/20 dark:shadow-orange-500/10',
      violet: 'bg-violet-600 dark:bg-violet-600/80 text-white shadow-violet-500/20 dark:shadow-violet-500/10',
      teal: 'bg-emerald-500 dark:bg-emerald-500/80 text-white shadow-emerald-500/20 dark:shadow-emerald-500/10',
    })[props.item.tone] || 'bg-blue-600 dark:bg-blue-600/80 text-white shadow-blue-500/20 dark:shadow-blue-500/10',
)

const valueColorClass = computed(
  () =>
    ({
      blue: 'text-blue-600 dark:text-blue-300',
      amber: 'text-orange-500 dark:text-orange-300',
      violet: 'text-violet-600 dark:text-violet-300',
      teal: 'text-emerald-500 dark:text-emerald-300',
    })[props.item.tone] || 'text-blue-600 dark:text-blue-300',
)

const tintClass = computed(
  () =>
    ({
      blue: 'bg-blue-400/10 dark:bg-blue-600/15',
      amber: 'bg-orange-400/10 dark:bg-orange-600/15',
      violet: 'bg-violet-400/10 dark:bg-violet-600/15',
      teal: 'bg-emerald-400/10 dark:bg-emerald-600/15',
    })[props.item.tone] || 'bg-blue-400/10 dark:bg-blue-600/15',
)

const trendClass = computed(
  () =>
    ({
      blue: 'text-blue-600 dark:text-blue-300 bg-blue-50 dark:bg-blue-600/25 border-blue-100/50 dark:border-blue-500/30',
      amber: 'text-orange-600 dark:text-orange-300 bg-orange-50 dark:bg-orange-600/25 border-orange-100/50 dark:border-orange-500/30',
      violet: 'text-violet-600 dark:text-violet-300 bg-violet-50 dark:bg-violet-600/25 border-violet-100/50 dark:border-violet-500/30',
      teal: 'text-emerald-600 dark:text-emerald-300 bg-emerald-50 dark:bg-emerald-600/25 border-emerald-100/50 dark:border-emerald-500/30',
    })[props.item.tone] || 'text-blue-600 dark:text-blue-300 bg-blue-50 dark:bg-blue-600/25 border-blue-100/50 dark:border-blue-500/30',
)
</script>

<template>
  <router-link :to="item.route" class="group block h-full rounded-[20px] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500/30">
    <GlassPanel interactive soft density="none" class="relative h-full rounded-[20px] kpi-route-tile overflow-hidden border border-white/60 dark:border-white/10 shadow-[0_4px_20px_rgba(15,23,42,0.05),inset_0_1px_0_rgba(255,255,255,0.6)] dark:shadow-[0_4px_20px_rgba(2,6,23,0.25),inset_0_1px_0_rgba(255,255,255,0.06)]">
      <div :class="['pointer-events-none absolute -right-6 -top-6 h-20 w-20 rounded-full blur-xl opacity-50', tintClass]" />

      <div class="flex h-full flex-col justify-between p-4">
        <div class="flex items-start justify-between mb-3">
          <div :class="['flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-[12px] shadow-sm transition-transform duration-300 group-hover:scale-105', toneClass]">
            <component :is="IconComponent" :size="18" stroke-width="2" />
          </div>
          <span class="flex h-6 w-6 items-center justify-center rounded-full border border-white/60 dark:border-white/10 bg-white/80 dark:bg-slate-700/60 text-blue-600 dark:text-blue-300 shadow-sm transition-all duration-300 group-hover:bg-blue-600 dark:group-hover:bg-blue-600/80 group-hover:text-white group-hover:border-blue-600 dark:group-hover:border-blue-500" aria-hidden="true">
            <ArrowUpRight :size="12" stroke-width="2.5" />
          </span>
        </div>

        <div class="space-y-1 mt-auto">
          <p :class="['text-[28px] font-bold tracking-tight leading-none', valueColorClass]">
            {{ item.value }}
          </p>
          <p class="text-[13px] font-medium text-slate-500 dark:text-slate-400 tracking-wide">
            {{ item.label }}
          </p>
          <div class="pt-1.5 flex items-center">
            <span :class="['inline-flex items-center justify-center rounded-full border px-2 py-0.5 text-[11px] font-medium transition-colors', trendClass]">
              {{ item.trend }}
            </span>
          </div>
        </div>
      </div>
    </GlassPanel>
  </router-link>
</template>

<style scoped>
.kpi-route-tile {
  background:
    linear-gradient(135deg, rgba(255, 255, 255, 0.9) 0%, rgba(255, 255, 255, 0.6) 100%),
    radial-gradient(circle at top right, rgba(255, 255, 255, 0.8), transparent 40%);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
}

:global(.dark) .kpi-route-tile {
  background:
    linear-gradient(135deg, rgba(15, 23, 42, 0.68) 0%, rgba(15, 23, 42, 0.52) 100%),
    radial-gradient(circle at top right, rgba(15, 23, 42, 0.72), transparent 40%);
}
</style>

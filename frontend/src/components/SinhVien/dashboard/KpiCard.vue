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
      blue: 'bg-blue-600 text-white shadow-blue-500/20',
      amber: 'bg-orange-500 text-white shadow-orange-500/20',
      violet: 'bg-violet-600 text-white shadow-violet-500/20',
      teal: 'bg-emerald-500 text-white shadow-emerald-500/20',
    })[props.item.tone] || 'bg-blue-600 text-white shadow-blue-500/20',
)

const valueColorClass = computed(
  () =>
    ({
      blue: 'text-blue-600',
      amber: 'text-orange-500',
      violet: 'text-violet-600',
      teal: 'text-emerald-500',
    })[props.item.tone] || 'text-blue-600',
)

const tintClass = computed(
  () =>
    ({
      blue: 'bg-blue-400/10',
      amber: 'bg-orange-400/10',
      violet: 'bg-violet-400/10',
      teal: 'bg-emerald-400/10',
    })[props.item.tone] || 'bg-blue-400/10',
)

const trendClass = computed(
  () =>
    ({
      blue: 'text-blue-600 bg-blue-50 border-blue-100/50',
      amber: 'text-orange-600 bg-orange-50 border-orange-100/50',
      violet: 'text-violet-600 bg-violet-50 border-violet-100/50',
      teal: 'text-emerald-600 bg-emerald-50 border-emerald-100/50',
    })[props.item.tone] || 'text-blue-600 bg-blue-50 border-blue-100/50',
)
</script>

<template>
  <router-link :to="item.route" class="group block h-full rounded-[20px] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500/30">
    <GlassPanel interactive soft density="none" class="relative h-full rounded-[20px] kpi-route-tile overflow-hidden border border-white/60 shadow-[0_4px_20px_rgba(15,23,42,0.05),inset_0_1px_0_rgba(255,255,255,0.6)]">
      <div :class="['pointer-events-none absolute -right-6 -top-6 h-20 w-20 rounded-full blur-xl opacity-50', tintClass]" />
      
      <div class="flex h-full flex-col justify-between p-4">
        <div class="flex items-start justify-between mb-3">
          <div :class="['flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-[12px] shadow-sm transition-transform duration-300 group-hover:scale-105', toneClass]">
            <component :is="IconComponent" :size="18" stroke-width="2" />
          </div>
          <span class="flex h-6 w-6 items-center justify-center rounded-full border border-white/60 bg-white/80 text-blue-600 shadow-sm transition-all duration-300 group-hover:bg-blue-600 group-hover:text-white group-hover:border-blue-600" aria-hidden="true">
            <ArrowUpRight :size="12" stroke-width="2.5" />
          </span>
        </div>

        <div class="space-y-1 mt-auto">
          <p :class="['text-[28px] font-bold tracking-tight leading-none', valueColorClass]">
            {{ item.value }}
          </p>
          <p class="text-[13px] font-medium text-slate-500 tracking-wide">
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
</style>

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
      amber: 'bg-amber-500 text-white shadow-amber-500/20',
      violet: 'bg-violet-600 text-white shadow-violet-500/20',
      teal: 'bg-emerald-600 text-white shadow-emerald-500/20',
    })[props.item.tone] || 'bg-blue-600 text-white shadow-blue-500/20',
)

const tintClass = computed(
  () =>
    ({
      blue: 'bg-blue-400/15',
      amber: 'bg-amber-400/20',
      violet: 'bg-violet-400/15',
      teal: 'bg-teal-400/15',
    })[props.item.tone] || 'bg-blue-400/15',
)

const trendClass = computed(
  () =>
    ({
      blue: 'text-blue-700 bg-blue-50/90 border-blue-100',
      amber: 'text-amber-700 bg-amber-50/90 border-amber-100',
      violet: 'text-violet-700 bg-violet-50/90 border-violet-100',
      teal: 'text-teal-700 bg-teal-50/90 border-teal-100',
    })[props.item.tone] || 'text-blue-700 bg-blue-50/90 border-blue-100',
)
</script>

<template>
  <router-link :to="item.route" class="group block rounded-[24px] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500/30">
    <GlassPanel interactive soft density="compact" class="min-h-[140px] rounded-[24px] kpi-route-tile overflow-hidden">
      <div :class="['pointer-events-none absolute -right-8 -top-8 h-16 w-16 rounded-full blur-2xl opacity-60', tintClass]" />
      <div class="flex h-full min-h-[120px] flex-col justify-between p-1">
        <div class="flex items-start justify-between">
          <div :class="['flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-xl shadow-md transition-all duration-300 group-hover:scale-110 group-hover:shadow-lg', toneClass]">
            <component :is="IconComponent" :size="18" />
          </div>
          <span class="flex h-6 w-6 place-items-center rounded-full border border-white/60 bg-white/68 text-slate-400 shadow-sm transition-all duration-300 group-hover:bg-blue-600 group-hover:text-white" aria-hidden="true">
            <ArrowUpRight :size="12" />
          </span>
        </div>

        <div class="space-y-0.5">
          <p class="text-3xl font-bold tracking-tight text-slate-950 group-hover:text-blue-700 transition-colors">{{ item.value }}</p>
          <p class="text-[13px] font-bold text-slate-600 leading-tight">{{ item.label }}</p>
          <div class="pt-2 flex items-center">
            <span :class="['inline-flex rounded-full border px-2 py-0.5 text-[9px] font-bold uppercase tracking-tight shadow-sm', trendClass]">
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
    linear-gradient(135deg, rgba(255, 255, 255, 0.82), rgba(255, 255, 255, 0.58)),
    radial-gradient(circle at top right, rgba(255, 255, 255, 0.7), transparent 34%);
}
</style>

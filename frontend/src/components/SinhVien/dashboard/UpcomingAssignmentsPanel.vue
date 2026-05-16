<script setup>
import { AlertCircle, ArrowRight, ClipboardList } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

defineProps({
  assignments: {
    type: Array,
    default: () => [],
  },
})

const priorityClass = {
  high: 'bg-red-500',
  medium: 'bg-amber-500',
  low: 'bg-emerald-500',
}
</script>

<template>
  <GlassPanel class="rounded-[28px] overflow-hidden" padding="p-0">
    <div class="flex items-center justify-between gap-3 border-b border-white/45 px-5 py-4">
      <div>
        <h2 class="text-base font-bold text-slate-950">Bài tập sắp hạn</h2>
        <p class="text-[11px] font-bold text-slate-500 uppercase tracking-wider">Deadline gần nhất</p>
      </div>
      <ClipboardList :size="18" class="text-blue-600" />
    </div>

    <div class="space-y-2 p-4">
      <router-link
        v-for="assignment in assignments"
        :key="assignment.id"
        to="/student/assignments"
        class="lg-list-item group flex min-h-[82px] items-start gap-3.5 p-3.5"
      >
        <span :class="['mt-2 h-2 w-2 flex-shrink-0 rounded-full shadow-sm', priorityClass[assignment.priority]]" />
        <div class="min-w-0 flex-1">
          <div class="flex items-start justify-between gap-2">
            <h3 class="min-w-0 flex-1 text-[13px] font-bold leading-tight text-slate-950 line-clamp-2">{{ assignment.title }}</h3>
            <GlassBadge :variant="assignment.variant" size="sm">{{ assignment.status }}</GlassBadge>
          </div>
          <div class="mt-2 flex flex-wrap items-center gap-x-3 gap-y-1">
            <p class="text-[10px] font-bold uppercase tracking-wider text-slate-400">{{ assignment.course }}</p>
            <p class="inline-flex items-center gap-1.5 text-[10px] font-bold text-amber-700">
              <AlertCircle :size="11" />
              {{ assignment.deadline }}
            </p>
          </div>
        </div>
        <ArrowRight :size="12" class="mt-1 text-slate-300 transition-colors group-hover:text-blue-600" />
      </router-link>
    </div>
  </GlassPanel>
</template>

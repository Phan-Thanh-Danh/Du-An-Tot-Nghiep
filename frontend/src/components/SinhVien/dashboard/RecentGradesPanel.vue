<script setup>
import { ArrowRight, Award } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'

defineProps({
  grades: {
    type: Array,
    default: () => [],
  },
})
</script>

<template>
  <GlassPanel class="rounded-[28px] overflow-hidden" padding="p-0">
    <div class="flex items-center justify-between gap-3 border-b border-white/45 px-5 py-4">
      <div>
        <h2 class="text-base font-bold text-slate-950">Điểm gần đây</h2>
        <p class="text-[11px] font-bold text-slate-500 uppercase tracking-wider">Vừa công bố</p>
      </div>
      <Award :size="18" class="text-violet-700" />
    </div>

    <div class="space-y-2 p-4">
      <router-link
        v-for="grade in grades"
        :key="grade.id"
        to="/student/grades"
        class="lg-list-item flex min-h-[64px] items-center justify-between gap-3.5 p-3"
      >
        <div class="min-w-0 flex-1">
          <h3 class="truncate text-[13px] font-bold text-slate-950 leading-tight">{{ grade.course }}</h3>
          <p class="mt-0.5 text-[10px] font-bold uppercase tracking-wider text-slate-400">{{ grade.type }}</p>
        </div>
        <div class="flex h-9 w-9 flex-shrink-0 flex-col items-center justify-center rounded-xl bg-blue-600 text-white shadow-lg shadow-blue-500/20 border border-blue-400/30">
          <p class="text-[14px] font-bold leading-none">{{ grade.score }}</p>
          <p class="mt-0.5 text-[7px] font-bold uppercase opacity-80">đ</p>
        </div>
      </router-link>
    </div>

    <div class="border-t border-white/45 px-5 py-3 bg-white/30">
      <router-link to="/student/grades" class="inline-flex items-center gap-1.5 text-[11px] font-bold text-blue-600 hover:text-blue-800 uppercase tracking-wide transition-colors">
        Xem tất cả điểm
        <ArrowRight :size="12" />
      </router-link>
    </div>
  </GlassPanel>
</template>

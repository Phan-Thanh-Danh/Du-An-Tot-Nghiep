<script setup>
import { CalendarDays, PlayCircle, Sparkles, Trophy } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import ProgressBar from '@/components/ui/ProgressBar.vue'

defineProps({
  student: {
    type: Object,
    required: true,
  },
  summary: {
    type: Object,
    required: true,
  },
  weekProgress: {
    type: Number,
    default: 0,
  },
})
</script>

<template>
  <GlassPanel
    variant="strong"
    density="normal"
    class="relative rounded-[32px] overflow-hidden"
  >
    <div class="pointer-events-none absolute -right-20 -top-16 h-56 w-56 rounded-full bg-cyan-300/12 blur-3xl" />
    <div class="relative grid min-h-[240px] gap-6 lg:grid-cols-[1.35fr_0.9fr] lg:items-center px-6 py-5 lg:px-8">
      <div class="flex flex-col justify-center space-y-4">
        <div class="flex flex-wrap items-center gap-2">
          <div class="inline-flex items-center gap-1.5 rounded-full border border-white/60 bg-white/72 px-2.5 py-0.5 text-[9px] font-bold uppercase tracking-wider text-teal-700 backdrop-blur-xl shadow-sm">
            <Sparkles :size="10" />
            {{ student.semester }}
          </div>
          <span class="text-[11px] font-semibold text-slate-500">
            {{ summary.classesToday }} học · {{ summary.assignmentsDue }} bài tập
          </span>
        </div>

        <div class="space-y-2">
          <h2 class="text-2xl font-bold tracking-tight text-slate-950 lg:text-3xl">
            Chào mừng trở lại, {{ student.name }}
          </h2>
          <p class="max-w-md text-sm leading-relaxed text-slate-600">
            Bạn có {{ summary.classesToday }} buổi học và {{ summary.assignmentsDue }} bài tập cần xử lý trong hôm nay. Bắt đầu ngay nhé!
          </p>
        </div>

        <div class="flex flex-wrap gap-2.5 pt-1">
          <router-link to="/student/courses" class="lg-button-primary h-9 rounded-[14px] px-4 text-[11px] font-bold uppercase tracking-wide shadow-md">
            <PlayCircle :size="14" />
            Tiếp tục học
          </router-link>
          <router-link to="/student/schedule" class="lg-button-secondary h-9 rounded-[14px] px-4 text-[11px] font-bold uppercase tracking-wide">
            <CalendarDays :size="14" />
            Lịch học
          </router-link>
        </div>
      </div>

      <div class="lg-readable w-full rounded-[24px] p-4 shadow-md lg:ml-auto max-w-[340px]">
        <div class="space-y-3.5">
          <div class="flex items-center justify-between gap-2">
            <div class="flex items-center gap-3">
              <div class="flex h-10 w-10 items-center justify-center rounded-2xl bg-blue-600 text-white shadow-lg shadow-blue-500/20">
                <Trophy :size="18" />
              </div>
              <div>
                <p class="text-[9px] font-bold uppercase tracking-widest text-slate-500">Tiến độ tuần</p>
                <p class="mt-0.5 text-xl font-bold leading-none text-slate-950">{{ weekProgress }}%</p>
              </div>
            </div>
            <div class="rounded-full bg-blue-50 px-2.5 py-1 text-[9px] font-bold text-blue-700">
              {{ summary.completedThisWeek }} NV
            </div>
          </div>

          <ProgressBar :value="weekProgress" label="" class="h-1.5 shadow-inner" />

          <div class="grid grid-cols-2 gap-2">
            <div class="rounded-2xl bg-white/80 p-2.5 border border-white shadow-sm">
              <p class="text-[9px] font-bold uppercase tracking-wider text-slate-500">Deadline</p>
              <p class="mt-1 text-xs font-bold text-amber-700 truncate">{{ summary.nearestDeadline }}</p>
            </div>
            <div class="rounded-2xl bg-white/80 p-2.5 border border-white shadow-sm">
              <p class="text-[9px] font-bold uppercase tracking-wider text-slate-500">GPA</p>
              <p class="mt-1 text-xs font-bold text-blue-700">{{ summary.gpa }}/10</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </GlassPanel>
</template>

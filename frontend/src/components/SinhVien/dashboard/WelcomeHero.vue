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
    density="none"
    class="relative rounded-2xl"
  >
    <div class="relative grid min-h-[200px] gap-5 p-5 lg:grid-cols-[1.35fr_0.9fr] lg:items-center lg:p-6">
      <div class="flex flex-col justify-center space-y-4">
        <div class="flex flex-wrap items-center gap-2">
           <div class="inline-flex items-center gap-1.5 rounded-full border border-card bg-[var(--surface-card)] px-2.5 py-1 text-[11px] font-semibold text-[var(--lg-secondary)] backdrop-blur-xl shadow-sm">
            <Sparkles :size="10" />
            {{ student.semester }}
          </div>
          <span class="text-[11px] font-semibold text-body">
            {{ summary.classesToday }} học · {{ summary.assignmentsDue }} bài tập
          </span>
        </div>

        <div class="space-y-2">
          <h2 class="text-2xl font-bold tracking-tight text-heading lg:text-3xl">
            Chào mừng trở lại, {{ student.name }}
          </h2>
          <p class="max-w-md text-sm leading-relaxed text-body">
            Bạn có {{ summary.classesToday }} buổi học và {{ summary.assignmentsDue }} bài tập cần xử lý trong hôm nay. Ưu tiên tiến độ môn học và deadline gần nhất.
          </p>
        </div>

        <div class="flex flex-wrap items-end gap-2.5 pt-1">
          <div>
            <p class="mb-1.5 text-[11px] font-medium text-body">Bài học tiếp theo:</p>
            <router-link to="/student/courses" class="lg-button-primary h-9 rounded-xl px-4 text-sm font-semibold shadow-md">
              <PlayCircle :size="14" />
              Tiếp tục học
            </router-link>
          </div>
          <router-link to="/student/schedule" class="lg-button-secondary h-9 rounded-xl px-4 text-sm font-semibold">
            <CalendarDays :size="14" />
            Lịch học
          </router-link>
        </div>
      </div>

      <div class="lg-readable w-full rounded-2xl p-4 lg:ml-auto max-w-[340px]">
        <div class="space-y-3.5">
          <div class="flex items-center justify-between gap-2">
            <div class="flex items-center gap-3">
              <div class="flex h-10 w-10 items-center justify-center rounded-2xl bg-[var(--text-link)] text-white shadow-[var(--lg-shadow-md)]">
                <Trophy :size="18" />
              </div>
              <div>
                <p class="text-xs font-medium text-body">Tiến độ tuần</p>
                <p class="mt-0.5 text-xl font-semibold leading-none text-heading">{{ weekProgress }}%</p>
              </div>
            </div>
            <div class="rounded-full bg-[var(--color-info-bg)] px-2.5 py-1 text-xs font-semibold text-link">
              {{ summary.completedThisWeek }} NV
            </div>
          </div>

          <ProgressBar :value="weekProgress" label="" class="h-1.5 shadow-inner" />

          <div class="grid grid-cols-2 gap-2">
            <div class="rounded-2xl bg-[var(--surface-card)] p-2.5 border border-card shadow-sm">
              <p class="text-xs font-medium text-body">Deadline</p>
              <p class="mt-1 text-xs font-semibold text-[var(--color-warning-text)] truncate">{{ summary.nearestDeadline }}</p>
            </div>
            <div class="rounded-2xl bg-[var(--surface-card)] p-2.5 border border-card shadow-sm">
              <p class="text-xs font-medium text-body">GPA</p>
              <p class="mt-1 text-xs font-semibold text-link">{{ summary.gpa }}/10</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </GlassPanel>
</template>

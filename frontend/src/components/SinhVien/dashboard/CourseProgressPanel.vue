<script setup>
import { ArrowRight, BookOpen, CheckCircle2 } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import ProgressBar from '@/components/ui/ProgressBar.vue'

defineProps({
  courses: {
    type: Array,
    default: () => [],
  },
})
</script>

<template>
  <GlassPanel class="rounded-[28px] overflow-hidden" padding="p-0">
    <div class="flex items-center justify-between gap-3 border-b border-white/45 px-5 py-4">
      <div>
        <h2 class="text-base font-bold text-slate-950">Tiến độ khóa học</h2>
        <p class="text-[11px] font-bold text-slate-500 uppercase tracking-wider">Cập nhật 15/05/2026</p>
      </div>
      <router-link to="/student/courses" class="lg-button-ghost px-2.5 py-1.5 text-[11px] font-bold uppercase">
        Tất cả
        <ArrowRight :size="12" />
      </router-link>
    </div>

    <div class="grid gap-2.5 p-4">
      <article
        v-for="course in courses"
        :key="course.id"
        class="lg-list-item flex min-h-[88px] items-center p-3.5"
      >
        <div class="flex w-full items-start gap-4">
          <div class="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-xl bg-blue-50 text-blue-700 shadow-sm border border-blue-100">
            <BookOpen :size="17" />
          </div>
          <div class="min-w-0 flex-1">
            <div class="flex items-start justify-between gap-2">
              <div class="min-w-0">
                <h3 class="truncate text-[14px] font-bold text-slate-950 leading-tight">{{ course.name }}</h3>
                <p class="text-[10px] font-bold uppercase tracking-widest text-slate-400 mt-0.5">
                  {{ course.code }}
                </p>
              </div>
              <GlassBadge :variant="course.statusVariant" size="sm">{{ course.status }}</GlassBadge>
            </div>

            <div class="mt-2.5">
              <ProgressBar :value="course.progress" class="h-2 shadow-sm" />
            </div>
            <div class="mt-2 flex items-center justify-between gap-2 text-[10px] font-bold">
              <span class="inline-flex items-center gap-1.5 text-slate-600">
                <CheckCircle2 :size="12" class="text-emerald-600" />
                {{ course.completed }}/{{ course.total }} bài học
              </span>
              <router-link to="/student/courses" class="text-blue-700 hover:text-blue-800 transition-colors">
                Vào học →
              </router-link>
            </div>
          </div>
        </div>
      </article>
    </div>
  </GlassPanel>
</template>

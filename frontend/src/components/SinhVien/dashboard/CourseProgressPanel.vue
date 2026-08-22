<script setup>
import { computed } from 'vue'
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

const updateDateLabel = computed(() => {
  try {
    const now = new Date()
    return `Cập nhật ${new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' }).format(now)}`
  } catch (e) {
    return 'Cập nhật hôm nay'
  }
})
</script>

<template>
  <GlassPanel variant="strong" density="none" class="rounded-2xl h-full flex flex-col">
    <div class="flex items-center justify-between gap-3 border-b border-card px-4 py-3.5">
      <div>
        <h2 class="text-base font-semibold text-heading">Tiến độ khóa học</h2>
        <p class="text-xs font-medium text-body">{{ updateDateLabel }}</p>
      </div>
      <router-link to="/student/courses" class="lg-button-ghost px-2.5 py-1.5 text-xs font-semibold">
        Tất cả
        <ArrowRight :size="12" />
      </router-link>
    </div>

    <div class="grid gap-2.5 p-4 flex-1">
      <div v-if="!courses || courses.length === 0" class="flex flex-col items-center justify-center py-10 text-center text-muted space-y-2">
        <BookOpen :size="32" class="text-placeholder opacity-60" />
        <p class="text-xs font-medium text-body">Chưa có khóa học nào được xếp trong kỳ</p>
        <p class="text-[11px] text-placeholder max-w-[240px]">Các môn học sẽ hiển thị tại đây ngay khi có lịch học chính thức.</p>
      </div>

      <article
        v-for="course in courses"
        :key="course.id"
        class="lg-list-item flex min-h-[82px] items-center p-3.5"
      >
        <div class="flex w-full items-start gap-4">
          <div class="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-xl bg-(--color-info-bg) text-link shadow-sm border border-[color-mix(in srgb,var(--text-link) 20%,transparent)]">
            <BookOpen :size="17" />
          </div>
          <div class="min-w-0 flex-1">
            <div class="flex items-start justify-between gap-2">
              <div class="min-w-0">
                <h3 class="truncate text-[14px] font-semibold text-heading leading-tight">{{ course.name }}</h3>
                <p class="mt-0.5 truncate text-xs font-medium text-body">
                  {{ course.code }} · {{ course.lecturer }}
                </p>
              </div>
              <GlassBadge :variant="course.statusVariant || 'info'" size="sm">{{ course.status }}</GlassBadge>
            </div>

            <div class="mt-2.5">
              <ProgressBar :value="course.progress" class="h-2 shadow-sm" />
            </div>
            <div class="mt-2 flex items-center justify-between gap-2 text-xs font-medium">
              <span class="inline-flex items-center gap-1.5 text-body">
                <CheckCircle2 :size="12" class="text-(--color-success-text)" />
                {{ course.completed }}/{{ course.total }} bài học
              </span>
              <router-link :to="`/student/courses/${course.code || course.id}`" class="rounded-lg px-2.5 py-1 text-[12px] font-semibold text-link bg-(--color-info-bg) hover:bg-(--text-link) hover:text-white transition-colors">
                Vào học
              </router-link>
            </div>
          </div>
        </div>
      </article>
    </div>
  </GlassPanel>
</template>

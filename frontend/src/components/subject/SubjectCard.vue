<script setup lang="ts">
import { computed } from 'vue'
import { FolderTree, CirclePlay, FileText, CircleHelp, Calendar, User, Eye } from 'lucide-vue-next'
import StatusBadge from './StatusBadge.vue'
import SubjectThumbnail from './SubjectThumbnail.vue'
import SubjectMenu from './SubjectMenu.vue'
import type { Subject } from '@/types/subject'

const props = defineProps<{
  subject: Subject
}>()

const emit = defineEmits<{
  view: [id: string]
  edit: [id: string]
  preview: [id: string]
  copy: [id: string]
  archive: [id: string]
}>()

const formattedDate = computed(() => {
  const d = new Date(props.subject.updatedAt)
  return d.toLocaleDateString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
})
</script>

<template>
  <div
    class="group relative flex flex-col rounded-2xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 shadow-sm transition-all duration-200 hover:shadow-lg hover:border-blue-400 dark:hover:border-blue-500 hover:-translate-y-1 cursor-pointer overflow-hidden focus-within:ring-2 focus-within:ring-blue-500/40"
    tabindex="0"
    role="article"
    :aria-label="'Môn học: ' + subject.name"
  >
    <!-- Thumbnail -->
    <SubjectThumbnail :name="subject.name" />

    <!-- Content -->
    <div class="flex flex-col flex-1 p-4 gap-3">
      <div class="flex items-start justify-between gap-2">
        <div class="space-y-1 min-w-0 flex-1">
          <h3 class="text-sm font-bold text-slate-900 dark:text-white leading-snug truncate">
            {{ subject.name }}
          </h3>
          <p class="text-[12px] font-mono font-medium text-slate-400 dark:text-slate-500">
            {{ subject.code }}
          </p>
        </div>
        <SubjectMenu
          :subject-id="subject.id"
          @edit="emit('edit', $event)"
          @preview="emit('preview', $event)"
          @copy="emit('copy', $event)"
          @archive="emit('archive', $event)"
        />
      </div>

      <p class="text-[13px] text-slate-500 dark:text-slate-400 leading-relaxed line-clamp-2">
        {{ subject.description }}
      </p>

      <div class="border-t border-slate-100 dark:border-slate-700" />

      <div class="grid grid-cols-4 gap-2">
        <div class="flex flex-col items-center gap-0.5">
          <FolderTree :size="14" class="text-slate-400 dark:text-slate-500" aria-hidden="true" />
          <span class="text-[12px] font-semibold text-slate-700 dark:text-slate-300 tabular-nums">{{ subject.chapterCount }}</span>
          <span class="text-[10px] text-slate-400 dark:text-slate-500">Chương</span>
        </div>
        <div class="flex flex-col items-center gap-0.5">
          <CirclePlay :size="14" class="text-slate-400 dark:text-slate-500" aria-hidden="true" />
          <span class="text-[12px] font-semibold text-slate-700 dark:text-slate-300 tabular-nums">{{ subject.lessonCount }}</span>
          <span class="text-[10px] text-slate-400 dark:text-slate-500">Bài</span>
        </div>
        <div class="flex flex-col items-center gap-0.5">
          <FileText :size="14" class="text-slate-400 dark:text-slate-500" aria-hidden="true" />
          <span class="text-[12px] font-semibold text-slate-700 dark:text-slate-300 tabular-nums">{{ subject.contentCount }}</span>
          <span class="text-[10px] text-slate-400 dark:text-slate-500">ND</span>
        </div>
        <div class="flex flex-col items-center gap-0.5">
          <CircleHelp :size="14" class="text-slate-400 dark:text-slate-500" aria-hidden="true" />
          <span class="text-[12px] font-semibold text-slate-700 dark:text-slate-300 tabular-nums">{{ subject.quizCount }}</span>
          <span class="text-[10px] text-slate-400 dark:text-slate-500">Quiz</span>
        </div>
      </div>

      <div class="flex items-center justify-center">
        <StatusBadge :status="subject.status" />
      </div>

      <div class="flex items-center justify-between pt-2 border-t border-slate-100 dark:border-slate-700">
        <div class="flex flex-col gap-0.5 min-w-0">
          <div class="flex items-center gap-1">
            <User :size="11" class="text-slate-400 shrink-0" aria-hidden="true" />
            <span class="text-[11px] text-slate-400 dark:text-slate-500 truncate">{{ subject.updatedBy }}</span>
          </div>
          <div class="flex items-center gap-1">
            <Calendar :size="11" class="text-slate-400 shrink-0" aria-hidden="true" />
            <span class="text-[11px] text-slate-400 dark:text-slate-500">{{ formattedDate }}</span>
          </div>
        </div>
        <button
          type="button"
          :aria-label="'Xem chi tiết môn ' + subject.name"
          class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-[12px] font-semibold text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-500/10 transition-all duration-200 hover:bg-blue-100 dark:hover:bg-blue-500/20 hover:scale-105 active:scale-[0.97] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
          @click.stop="emit('view', subject.id)"
        >
          <Eye :size="14" aria-hidden="true" />
          <span>Chi tiết</span>
        </button>
      </div>
    </div>
  </div>
</template>

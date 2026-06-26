<script setup lang="ts">
import type { SubjectStatus } from '@/types/subject'

const props = defineProps<{
  status: SubjectStatus
}>()

const config: Record<SubjectStatus, { label: string; bg: string; text: string; dot: string }> = {
  active: {
    label: 'Đang hoạt động',
    bg: 'bg-emerald-50 dark:bg-emerald-500/10',
    text: 'text-emerald-700 dark:text-emerald-400',
    dot: 'bg-emerald-500',
  },
  draft: {
    label: 'Nháp',
    bg: 'bg-amber-50 dark:bg-amber-500/10',
    text: 'text-amber-700 dark:text-amber-400',
    dot: 'bg-amber-500',
  },
  locked: {
    label: 'Đã khóa',
    bg: 'bg-red-50 dark:bg-red-500/10',
    text: 'text-red-700 dark:text-red-400',
    dot: 'bg-red-500',
  },
}
</script>

<template>
  <span
    :class="[
      'inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-semibold leading-5 transition-colors duration-200',
      config[status].bg,
      config[status].text,
    ]"
  >
    <span :class="['w-1.5 h-1.5 rounded-full', config[status].dot]" aria-hidden="true" />
    {{ config[status].label }}
  </span>
</template>

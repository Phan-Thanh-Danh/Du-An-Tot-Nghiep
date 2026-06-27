<script setup lang="ts">
import { computed } from 'vue'
import { QuizStatus } from '@/types/content-council/quiz'
import { FileEdit, Send, PlayCircle, Lock } from 'lucide-vue-next'

const props = defineProps<{
  status: QuizStatus
}>()

const statusConfig = computed(() => {
  switch (props.status) {
    case 'draft':
      return { label: 'Bản nháp', color: 'bg-slate-100 text-slate-700 border-slate-200', icon: FileEdit }
    case 'published':
      return { label: 'Đã xuất bản', color: 'bg-blue-50 text-blue-700 border-blue-200', icon: Send }
    case 'open':
      return { label: 'Đang mở', color: 'bg-green-50 text-green-700 border-green-200', icon: PlayCircle }
    case 'closed':
      return { label: 'Đã đóng', color: 'bg-slate-100 text-slate-600 border-slate-200', icon: Lock }
    default:
      return { label: 'Unknown', color: 'bg-slate-100 text-slate-500 border-slate-200', icon: FileEdit }
  }
})
</script>

<template>
  <span 
    class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium border w-max"
    :class="statusConfig.color"
  >
    <component :is="statusConfig.icon" class="w-3.5 h-3.5" />
    {{ statusConfig.label }}
  </span>
</template>

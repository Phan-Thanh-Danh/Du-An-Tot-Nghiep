<script setup>
import { computed } from 'vue'
import { Video, FileText, AlignLeft, CircleHelp, Presentation } from 'lucide-vue-next'

const props = defineProps({
  lesson: {
    type: Object,
    required: true
  }
})

const getLessonIcon = (type) => {
  switch (type) {
    case 'video': return Video
    case 'pdf': return FileText
    case 'text': return AlignLeft
    case 'quiz': return CircleHelp
    case 'slide': return Presentation
    default: return FileText
  }
}

const getLessonColor = (type) => {
  switch (type) {
    case 'video': return 'text-red-500'
    case 'pdf': return 'text-orange-500'
    case 'text': return 'text-slate-500'
    case 'quiz': return 'text-purple-500'
    case 'slide': return 'text-blue-500'
    default: return 'text-slate-500'
  }
}

const getStatusBadgeClass = (status) => {
  switch (status) {
    case 'empty': return 'bg-orange-50 text-orange-700 border-orange-200'
    case 'draft': return 'bg-slate-100 text-slate-700 border-slate-200'
    case 'published': return 'bg-emerald-50 text-emerald-700 border-emerald-200'
    case 'hidden': return 'bg-purple-50 text-purple-700 border-purple-200'
    default: return 'bg-slate-100 text-slate-600 border-slate-200'
  }
}

const getStatusText = (status) => {
  switch (status) {
    case 'empty': return 'Chưa có nội dung'
    case 'draft': return 'Nháp'
    case 'published': return 'Đã xuất bản'
    case 'hidden': return 'Đang ẩn'
    default: return status
  }
}

const handleLessonClick = () => {
  // Alert or toast in real app, per task requirements: highlight or toast
  alert('Trình soạn bài học sẽ được triển khai ở bước tiếp theo.')
}
</script>

<template>
  <button 
    @click="handleLessonClick"
    class="w-full flex items-start sm:items-center justify-between p-1.5 sm:px-3 sm:py-1.5 transition-colors group text-left border-t border-card first:border-t-0 focus:outline-none"
  >
    <div class="flex items-start sm:items-center gap-3 flex-1 min-w-0 pr-4">
      <div class="mt-0.5 sm:mt-0 flex-shrink-0">
        <component :is="getLessonIcon(lesson.type)" class="w-5 h-5" :class="getLessonColor(lesson.type)" />
      </div>
      <div class="flex flex-col sm:flex-row sm:items-center gap-1 sm:gap-3 flex-1 min-w-0">
        <span class="text-sm font-medium text-heading truncate">
          Bài {{ lesson.order }}: {{ lesson.title }}
        </span>
        
        <!-- Mobile meta -->
        <div class="flex sm:hidden items-center gap-2 mt-1">
          <span class="text-xs text-body">{{ lesson.contentCount }} nội dung</span>
          <span class="text-xs font-medium px-1.5 py-0.5 rounded border" :class="getStatusBadgeClass(lesson.status)">
            {{ getStatusText(lesson.status) }}
          </span>
        </div>
      </div>
    </div>

    <!-- Desktop meta -->
    <div class="hidden sm:flex items-center gap-4 flex-shrink-0">
      <span class="text-sm text-body">{{ lesson.contentCount }} nội dung</span>
      <span class="text-xs font-medium px-2 py-0.5 rounded-full border min-w-[100px] text-center" :class="getStatusBadgeClass(lesson.status)">
        {{ getStatusText(lesson.status) }}
      </span>
    </div>
  </button>
</template>

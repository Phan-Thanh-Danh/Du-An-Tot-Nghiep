<script setup lang="ts">
import { inject, computed } from 'vue'
import { Edit2, Eye, MoreVertical } from 'lucide-vue-next'

const editor = inject<any>('curriculumEditor')
const lesson = computed(() => editor.selectedLesson.value)
const chapter = computed(() => editor.selectedChapter.value)

const onEdit = () => {
  if (!lesson.value) return
  editor.editingLesson.value = lesson.value
  editor.isLessonModalOpen.value = true
}

const onPreview = () => {
  alert('Chế độ xem trước đầy đủ sẽ được triển khai ở bước tiếp theo.')
}

const getStatusBadge = (status: string) => {
  switch (status) {
    case 'draft': return { text: 'Nháp', class: 'bg-slate-200 text-slate-600' }
    case 'published': return { text: 'Xuất bản', class: 'bg-green-100 text-green-700' }
    case 'hidden': return { text: 'Đang ẩn', class: 'bg-red-100 text-red-700' }
    case 'empty': return { text: 'Trống', class: 'bg-orange-100 text-orange-700' }
    default: return { text: status, class: 'bg-slate-100 text-slate-500' }
  }
}
</script>

<template>
  <div v-if="lesson" class="px-6 py-5 border-b border-card bg-slate-50/50 flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between sticky top-0 z-10 backdrop-blur-sm">
    <div class="flex-1 min-w-0">
      <div class="flex items-center gap-3 mb-1.5">
        <span class="text-[11px] font-semibold tracking-wider text-slate-500 uppercase">
          {{ chapter?.title || 'Bài học' }}
        </span>
        <span 
          class="px-2 py-0.5 text-[11px] font-semibold rounded-full uppercase"
          :class="getStatusBadge(lesson.status).class"
        >
          {{ getStatusBadge(lesson.status).text }}
        </span>
      </div>
      <h2 class="text-2xl font-bold text-slate-800 truncate" :title="lesson.title">
        {{ lesson.title }}
      </h2>
      <div class="flex items-center gap-4 mt-2 text-sm text-slate-500">
        <span>{{ lesson.contents?.length || 0 }} khối nội dung</span>
        <span>Cập nhật gần đây</span>
      </div>
    </div>

    <div class="flex items-center gap-2 shrink-0">
      <button 
        @click="onPreview"
        class="flex items-center gap-2 px-3 py-1.5 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded hover:bg-slate-50 transition-colors"
        title="Xem trước bài học"
      >
        <Eye class="w-4 h-4" />
        <span class="hidden sm:inline">Xem trước</span>
      </button>
      
      <button 
        @click="onEdit"
        class="flex items-center gap-2 px-3 py-1.5 text-sm font-medium text-blue-700 bg-blue-50 border border-transparent rounded hover:bg-blue-100 transition-colors"
      >
        <Edit2 class="w-4 h-4" />
        <span class="hidden sm:inline">Sửa bài học</span>
      </button>

      <button 
        class="w-8 h-8 flex items-center justify-center rounded hover:bg-slate-200 text-slate-500 transition-colors"
        title="Thêm thao tác"
      >
        <MoreVertical class="w-4 h-4" />
      </button>
    </div>
  </div>
</template>

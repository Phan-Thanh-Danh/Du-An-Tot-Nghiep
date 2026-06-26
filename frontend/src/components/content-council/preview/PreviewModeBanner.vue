<script setup>
import { Eye, ArrowLeft } from 'lucide-vue-next'
import { useRouter } from 'vue-router'

const router = useRouter()

const props = defineProps({
  subjectId: {
    type: [String, Number],
    required: true
  },
  currentLessonId: {
    type: [String, Number],
    default: null
  },
  showDrafts: {
    type: Boolean,
    default: true
  }
})

const emit = defineEmits(['update:showDrafts'])

const goBackToEditor = () => {
  // If there's a selected lesson, we can pass it, but SubjectEditorPage doesn't take lessonId in the URL 
  // currently. So we just go back to the editor page.
  router.push({ 
    name: 'content-council-subject-editor', 
    params: { subjectId: props.subjectId } 
  })
}
</script>

<template>
  <div class="bg-blue-50/80 backdrop-blur-sm border-b border-blue-200 px-4 py-3 sticky top-0 z-50 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
    <div class="flex items-start gap-3">
      <div class="p-2 bg-blue-100 text-blue-700 rounded-lg shrink-0 mt-0.5 sm:mt-0">
        <Eye class="w-5 h-5" />
      </div>
      <div>
        <h2 class="text-sm font-bold text-blue-900">Chế độ xem trước</h2>
        <p class="text-xs text-blue-800 mt-0.5">Bạn đang xem nội dung dưới góc nhìn của học sinh. Tiến độ và kết quả Quiz sẽ không được ghi nhận.</p>
      </div>
    </div>
    
    <div class="flex items-center gap-4 shrink-0 self-end sm:self-auto">
      <label class="flex items-center gap-2 text-sm text-blue-800 cursor-pointer">
        <input 
          type="checkbox" 
          :checked="showDrafts"
          @change="$emit('update:showDrafts', $event.target.checked)"
          class="rounded border-blue-300 text-blue-600 focus:ring-blue-500"
        >
        Hiển thị nội dung nháp
      </label>
      
      <div class="w-px h-6 bg-blue-200 hidden sm:block"></div>

      <button 
        @click="goBackToEditor"
        class="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-blue-700 bg-white hover:bg-blue-50 border border-blue-200 rounded-lg transition-colors shadow-sm"
      >
        <ArrowLeft class="w-4 h-4" />
        Quay lại trình soạn
      </button>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { ChevronLeft, ChevronRight } from 'lucide-vue-next'

const props = defineProps({
  chapters: {
    type: Array,
    required: true
  },
  currentLessonId: {
    type: [Number, String],
    required: true
  },
  showDrafts: {
    type: Boolean,
    default: true
  }
})

const emit = defineEmits(['navigate'])

const flatLessons = computed(() => {
  const lessons = []
  props.chapters.forEach(chapter => {
    if (chapter.hidden) return
    const visibleLessons = (chapter.lessons || []).filter(l => {
      if (l.hidden) return false
      if (l.status === 'draft' && !props.showDrafts) return false
      return true
    })
    visibleLessons.forEach(l => {
      lessons.push({
        id: l.id,
        title: l.title,
        chapterTitle: chapter.title
      })
    })
  })
  return lessons
})

const currentIndex = computed(() => {
  return flatLessons.value.findIndex(l => l.id === Number(props.currentLessonId))
})

const prevLesson = computed(() => {
  if (currentIndex.value > 0) {
    return flatLessons.value[currentIndex.value - 1]
  }
  return null
})

const nextLesson = computed(() => {
  if (currentIndex.value !== -1 && currentIndex.value < flatLessons.value.length - 1) {
    return flatLessons.value[currentIndex.value + 1]
  }
  return null
})

const handleNavigate = (lessonId) => {
  emit('navigate', lessonId)
  window.scrollTo({ top: 0, behavior: 'smooth' })
}
</script>

<template>
  <div class="mt-12 pt-6 border-t border-slate-200 flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-4">
    <!-- Prev -->
    <div class="flex-1">
      <button 
        v-if="prevLesson"
        @click="handleNavigate(prevLesson.id)"
        class="w-full sm:w-auto flex flex-col items-start p-4 bg-white border border-slate-200 rounded-xl hover:border-blue-300 hover:shadow-sm transition-all group"
      >
        <div class="flex items-center gap-1.5 text-sm font-medium text-slate-500 mb-1 group-hover:text-blue-600">
          <ChevronLeft class="w-4 h-4" />
          Bài trước
        </div>
        <div class="text-sm font-semibold text-slate-800 line-clamp-1 group-hover:text-blue-800 text-left">
          {{ prevLesson.title }}
        </div>
      </button>
    </div>

    <!-- Next -->
    <div class="flex-1 flex sm:justify-end">
      <button 
        v-if="nextLesson"
        @click="handleNavigate(nextLesson.id)"
        class="w-full sm:w-auto flex flex-col items-end p-4 bg-white border border-slate-200 rounded-xl hover:border-blue-300 hover:shadow-sm transition-all group"
      >
        <div class="flex items-center gap-1.5 text-sm font-medium text-slate-500 mb-1 group-hover:text-blue-600">
          Bài tiếp theo
          <ChevronRight class="w-4 h-4" />
        </div>
        <div class="text-sm font-semibold text-slate-800 line-clamp-1 group-hover:text-blue-800 text-right">
          {{ nextLesson.title }}
        </div>
      </button>
    </div>
  </div>
</template>

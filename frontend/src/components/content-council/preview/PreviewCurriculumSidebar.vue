<script setup>
import { computed, ref, watch } from 'vue'
import { ChevronDown, PlaySquare, File, FileText, HelpCircle, Video, CheckCircle } from 'lucide-vue-next'

const props = defineProps({
  chapters: {
    type: Array,
    required: true
  },
  currentLessonId: {
    type: [Number, String],
    default: null
  },
  showDrafts: {
    type: Boolean,
    default: true
  }
})

const emit = defineEmits(['select-lesson'])

const expandedChapters = ref(new Set())

// Auto expand chapter that contains current lesson
watch(() => props.currentLessonId, (newId) => {
  if (newId) {
    for (const chapter of props.chapters) {
      if (chapter.lessons?.some(l => l.id === Number(newId))) {
        expandedChapters.value.add(chapter.id)
        break
      }
    }
  }
}, { immediate: true })

const toggleChapter = (chapterId) => {
  if (expandedChapters.value.has(chapterId)) {
    expandedChapters.value.delete(chapterId)
  } else {
    expandedChapters.value.add(chapterId)
  }
}

const getIconForType = (type) => {
  switch (type) {
    case 'video': return Video
    case 'slide_html': return PlaySquare
    case 'document': return File
    case 'text': return FileText
    case 'quiz': return HelpCircle
    default: return FileText
  }
}

const visibleChapters = computed(() => {
  return props.chapters
    .filter(c => !c.hidden)
    .map(c => {
      const visibleLessons = (c.lessons || []).filter(l => {
        if (l.hidden) return false
        if (l.status === 'draft' && !props.showDrafts) return false
        return true
      })
      return { ...c, lessons: visibleLessons }
    })
    .filter(c => c.lessons.length > 0) // Hide chapters that have no visible lessons
})
</script>

<template>
  <div class="flex flex-col h-full bg-white border-r border-slate-200">
    <div class="p-4 border-b border-slate-200">
      <h3 class="font-bold text-slate-800">Chương và bài học</h3>
    </div>
    
    <div class="flex-1 overflow-y-auto p-3 space-y-2">
      <div v-if="visibleChapters.length === 0" class="text-sm text-slate-500 text-center py-4">
        Chưa có nội dung để hiển thị
      </div>

      <div 
        v-for="(chapter, index) in visibleChapters" 
        :key="chapter.id"
        class="border border-slate-200 rounded-lg overflow-hidden bg-white"
      >
        <button 
          @click="toggleChapter(chapter.id)"
          class="w-full flex items-center justify-between p-3 hover:bg-slate-50 transition-colors text-left"
        >
          <div class="flex items-center gap-2 min-w-0">
            <span class="text-sm font-semibold text-slate-800 truncate">
              Chương {{ index + 1 }}: {{ chapter.title }}
            </span>
          </div>
          <ChevronDown 
            class="w-4 h-4 text-slate-400 shrink-0 transition-transform duration-200"
            :class="{ 'rotate-180': expandedChapters.has(chapter.id) }"
          />
        </button>

        <div v-show="expandedChapters.has(chapter.id)" class="border-t border-slate-100 bg-slate-50/50">
          <div class="flex flex-col py-1">
            <button
              v-for="lesson in chapter.lessons"
              :key="lesson.id"
              @click="emit('select-lesson', lesson.id)"
              class="w-full flex items-start gap-3 px-4 py-2.5 text-left transition-colors relative"
              :class="[
                currentLessonId === lesson.id 
                  ? 'bg-blue-50' 
                  : 'hover:bg-slate-100'
              ]"
            >
              <div 
                v-if="currentLessonId === lesson.id"
                class="absolute left-0 top-0 bottom-0 w-1 bg-blue-600 rounded-r-full"
              ></div>
              
              <div class="shrink-0 mt-0.5">
                <component 
                  :is="getIconForType(lesson.type)" 
                  class="w-4 h-4" 
                  :class="currentLessonId === lesson.id ? 'text-blue-600' : 'text-slate-400'"
                />
              </div>
              
              <div class="flex-1 min-w-0">
                <div 
                  class="text-sm font-medium line-clamp-2"
                  :class="currentLessonId === lesson.id ? 'text-blue-700' : 'text-slate-700'"
                >
                  Bài {{ lesson.order }}: {{ lesson.title }}
                </div>
                
                <div class="flex items-center gap-2 mt-1">
                  <!-- Draft Badge -->
                  <span 
                    v-if="lesson.status === 'draft'" 
                    class="text-[10px] font-medium px-1.5 py-0.5 rounded bg-amber-100 text-amber-700 border border-amber-200 uppercase tracking-wider"
                  >
                    Bản nháp
                  </span>
                </div>
              </div>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

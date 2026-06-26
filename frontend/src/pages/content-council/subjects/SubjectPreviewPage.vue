<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useCurriculumEditor } from '@/composables/content-council/useCurriculumEditor'
import PreviewModeBanner from '@/components/content-council/preview/PreviewModeBanner.vue'
import PreviewCurriculumSidebar from '@/components/content-council/preview/PreviewCurriculumSidebar.vue'
import PreviewContentRenderer from '@/components/content-council/preview/PreviewContentRenderer.vue'
import LessonNavigation from '@/components/content-council/preview/LessonNavigation.vue'

const route = useRoute()
const router = useRouter()

const subjectId = Number(route.params.subjectId)
const { chapters } = useCurriculumEditor(subjectId)

const showDrafts = ref(true)
const currentLessonId = ref(null)

// Initialize lessonId from query or default to first valid lesson
onMounted(() => {
  if (route.query.lessonId) {
    currentLessonId.value = Number(route.query.lessonId)
  } else {
    // Find first valid lesson
    const firstLesson = flatLessons.value[0]
    if (firstLesson) {
      currentLessonId.value = firstLesson.id
      // Update URL without triggering navigation
      router.replace({ query: { lessonId: firstLesson.id } })
    }
  }
})

// Compute flat valid lessons based on showDrafts
const flatLessons = computed(() => {
  const lessons = []
  chapters.value.forEach(chapter => {
    if (chapter.hidden) return
    const visibleLessons = (chapter.lessons || []).filter(l => {
      if (l.hidden) return false
      if (l.status === 'draft' && !showDrafts.value) return false
      return true
    })
    visibleLessons.forEach(l => lessons.push(l))
  })
  return lessons
})

// Current lesson object
const currentLesson = computed(() => {
  if (!currentLessonId.value) return null
  return flatLessons.value.find(l => l.id === currentLessonId.value) || null
})

// Handle sidebar / navigation clicks
const handleLessonSelect = (lessonId) => {
  currentLessonId.value = lessonId
  router.push({ query: { lessonId } })
}

// Watch showDrafts: If current lesson becomes hidden, fallback to first
watch(showDrafts, () => {
  if (currentLessonId.value && !currentLesson.value) {
    const firstLesson = flatLessons.value[0]
    if (firstLesson) {
      handleLessonSelect(firstLesson.id)
    } else {
      currentLessonId.value = null
      router.push({ query: {} })
    }
  }
})

// Handle mobile sidebar toggle
const isMobileSidebarOpen = ref(false)
</script>

<template>
  <div class="h-screen w-full flex flex-col bg-slate-50 overflow-hidden font-sans">
    <PreviewModeBanner 
      :subjectId="subjectId" 
      :currentLessonId="currentLessonId"
      v-model:showDrafts="showDrafts"
    />

    <div class="flex-1 flex overflow-hidden relative">
      <!-- Sidebar Desktop -->
      <div class="hidden md:block w-80 lg:w-96 shrink-0 z-10 shadow-[4px_0_24px_rgba(0,0,0,0.02)] relative h-full">
        <PreviewCurriculumSidebar 
          :chapters="chapters"
          :currentLessonId="currentLessonId"
          :showDrafts="showDrafts"
          @select-lesson="handleLessonSelect"
        />
      </div>

      <!-- Main Content Area -->
      <div class="flex-1 overflow-y-auto relative bg-white">
        <div class="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8 sm:py-12 min-h-full flex flex-col">
          
          <template v-if="currentLesson">
            <PreviewContentRenderer 
              :lesson="currentLesson"
              :showDrafts="showDrafts"
              class="flex-1"
            />

            <LessonNavigation 
              :chapters="chapters"
              :currentLessonId="currentLessonId"
              :showDrafts="showDrafts"
              @navigate="handleLessonSelect"
              class="mt-auto"
            />
          </template>

          <template v-else>
            <div class="flex flex-col items-center justify-center flex-1 text-center py-20">
              <div class="w-16 h-16 bg-slate-100 rounded-full flex items-center justify-center mb-4">
                <span class="text-slate-400 text-2xl">?</span>
              </div>
              <h2 class="text-xl font-bold text-slate-800 mb-2">Chưa có bài học nào để xem</h2>
              <p class="text-slate-500 max-w-sm">Môn học này hiện chưa có bài học nào được công bố hoặc hiển thị.</p>
            </div>
          </template>

        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Tuỳ chỉnh font cho chế độ xem bài học giống với giao diện của học sinh */
</style>

<script setup lang="ts">
import { computed, provide, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import SubjectDetailHeader from '@/components/content-council/subjects/SubjectDetailHeader.vue'
import SubjectDetailTabs from '@/components/content-council/subjects/SubjectDetailTabs.vue'
import CurriculumEditorSidebar from '@/components/content-council/editor/CurriculumEditorSidebar.vue'
import LessonEditorCanvas from '@/components/content-council/editor/LessonEditorCanvas.vue'
import ChapterFormModal from '@/components/content-council/editor/ChapterFormModal.vue'
import LessonFormModal from '@/components/content-council/editor/LessonFormModal.vue'
import ContentFormDrawer from '@/components/content-council/editor/ContentFormDrawer.vue'
import ContentDeleteDialog from '@/components/content-council/editor/ContentDeleteDialog.vue'

import { useCurriculumEditor } from '@/composables/content-council/useCurriculumEditor'
import { useSubjectStore } from '@/stores/content-council/subjectStore'

const props = defineProps({
  subjectId: {
    type: Number,
    required: true
  }
})

const router = useRouter()
const route = useRoute()

const store = useSubjectStore()
store.init()
const subject = computed(() => store.getSubjectDetail(props.subjectId) || null)

// Initialize editor state
const editor = useCurriculumEditor(props.subjectId)
provide('curriculumEditor', editor)

onMounted(async () => {
  await store.loadSubjectDetail(props.subjectId)
  // If there's a lessonId in query, select it
  const queryLessonId = route.query.lessonId
  if (queryLessonId) {
    editor.selectLesson(Number(queryLessonId))
  }
})

const goBack = () => {
  router.push({ name: 'content-council-subjects' })
}
</script>

<template>
  <div class="h-full flex flex-col">
    <!-- Not Found State -->
    <div v-if="!subject" class="flex-1 flex flex-col items-center justify-center p-8 text-center surface-card rounded-xl border border-card">
      <div class="w-16 h-16 lg-glass-soft rounded-full flex items-center justify-center mb-4 text-placeholder">
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3Z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
      </div>
      <h2 class="text-xl font-bold text-heading mb-2">Không tìm thấy môn học</h2>
      <p class="text-body mb-6 max-w-md">Môn học bạn đang tìm không tồn tại hoặc đã bị xóa khỏi hệ thống.</p>
      <button 
        @click="goBack"
        class="px-4 py-2 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 transition-colors"
      >
        Quay lại danh sách môn học
      </button>
    </div>

    <!-- Editor Layout -->
    <div v-else class="flex-1 flex flex-col">
      <!-- Header & Tabs -->
      <SubjectDetailHeader :subject="subject" />
      <SubjectDetailTabs />

      <!-- Main Editor Workspace -->
      <div class="flex-1 flex flex-col lg:flex-row gap-5 items-start mt-2">
        <!-- Left: Canvas (70%) -->
        <div class="w-full lg:w-[70%] xl:w-[72%] order-2 lg:order-1 flex-1">
          <LessonEditorCanvas />
        </div>
        
        <!-- Right: Sidebar (30%) -->
        <div class="w-full lg:w-[30%] xl:w-[28%] order-1 lg:order-2 sticky top-[88px] max-h-[calc(100vh-100px)] overflow-y-auto rounded-xl surface-card border border-card shadow-sm hide-scrollbar">
          <CurriculumEditorSidebar />
        </div>
      </div>
    </div>

    <!-- Modals & Drawers -->
    <ChapterFormModal />
    <LessonFormModal />
    <ContentFormDrawer />
    <ContentDeleteDialog />
  </div>
</template>

<style scoped>
.hide-scrollbar::-webkit-scrollbar {
  display: none;
}
.hide-scrollbar {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
</style>

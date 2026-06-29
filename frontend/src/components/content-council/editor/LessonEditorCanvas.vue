<script setup lang="ts">
import { inject } from 'vue'
import LessonEditorHeader from './LessonEditorHeader.vue'
import ContentBlockList from './ContentBlockList.vue'
import AddContentToolbar from './AddContentToolbar.vue'

const editor = inject<any>('curriculumEditor')
</script>

<template>
  <div class="h-full bg-white rounded-xl border border-card shadow-sm flex flex-col overflow-hidden relative">
    <div v-if="!editor.selectedLessonId.value" class="flex-1 flex flex-col items-center justify-center p-8 text-center">
      <div class="w-16 h-16 bg-slate-50 rounded-full flex items-center justify-center mb-4 text-slate-300">
        <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14.5 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7.5L14.5 2z"/><polyline points="14 2 14 8 20 8"/><line x1="12" y1="18" x2="12" y2="12"/><line x1="9" y1="15" x2="15" y2="15"/></svg>
      </div>
      <h2 class="text-xl font-bold text-slate-700 mb-2">Chọn một bài học để bắt đầu biên soạn</h2>
      <p class="text-slate-500 max-w-md">Hãy chọn bài học trong danh sách Chương và bài học bên phải để hiển thị nội dung.</p>
    </div>

    <div v-else class="flex-1 flex flex-col overflow-y-auto">
      <LessonEditorHeader />
      
      <div class="p-6">
        <!-- Empty Content State -->
        <div v-if="editor.selectedLesson.value?.contents?.length === 0" class="mb-6 p-8 border-2 border-dashed border-slate-200 rounded-xl text-center">
          <h3 class="text-lg font-semibold text-slate-700 mb-2">Bài học này chưa có nội dung</h3>
          <p class="text-(--text-muted) mb-6">Thêm video, slide, tài liệu hoặc Quiz để bắt đầu xây dựng bài học.</p>
        </div>

        <!-- Content List -->
        <ContentBlockList v-else />

        <!-- Add Content Toolbar -->
        <AddContentToolbar />
      </div>
    </div>
  </div>
</template>

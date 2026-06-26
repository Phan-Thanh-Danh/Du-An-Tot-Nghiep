<script setup lang="ts">
import { inject } from 'vue'
import { Plus } from 'lucide-vue-next'
import EditorChapterItem from './EditorChapterItem.vue'

const editor = inject<any>('curriculumEditor')

const openAddChapter = () => {
  editor.editingChapter.value = null
  editor.isChapterModalOpen.value = true
}
</script>

<template>
  <div class="flex flex-col h-full bg-white rounded-xl">
    <!-- Header -->
    <div class="px-4 py-3 border-b border-card flex items-center justify-between sticky top-0 bg-white/95 backdrop-blur-sm z-10 rounded-t-xl">
      <h3 class="font-semibold text-heading">Chương và Bài học</h3>
      <button 
        @click="openAddChapter"
        class="w-8 h-8 flex items-center justify-center rounded hover:bg-slate-100 text-slate-600 transition-colors"
        title="Thêm chương mới"
      >
        <Plus class="w-5 h-5" />
      </button>
    </div>

    <!-- Chapter List -->
    <div class="flex-1 overflow-y-auto p-2 space-y-2">
      <div v-if="editor.chapters.value.length === 0" class="p-4 text-center text-sm text-placeholder">
        Chưa có chương nào. Nhấn dấu + để thêm chương.
      </div>
      
      <EditorChapterItem
        v-for="chapter in editor.chapters.value"
        :key="chapter.id"
        :chapter="chapter"
      />

      <div class="pt-2 pb-4">
        <button 
          @click="openAddChapter"
          class="w-full flex items-center justify-center gap-2 py-2.5 px-4 text-sm font-medium text-blue-600 bg-blue-50 hover:bg-blue-100 rounded-lg transition-colors border border-dashed border-blue-200"
        >
          <Plus class="w-4 h-4" />
          <span>Thêm chương</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { inject } from 'vue'
import { Video, PlaySquare, File, FileText, HelpCircle, Plus } from 'lucide-vue-next'

const editor = inject<any>('curriculumEditor')

const addContent = (type: string) => {
  editor.selectedContentType.value = type
  editor.editingContent.value = null
  editor.isContentDrawerOpen.value = true
}

const contentTypes = [
  { type: 'video', label: 'Video', icon: Video, color: 'text-red-500', bg: 'bg-red-50' },
  { type: 'slide_html', label: 'Slide HTML', icon: PlaySquare, color: 'text-orange-500', bg: 'bg-orange-50' },
  { type: 'document', label: 'Tài liệu', icon: File, color: 'text-blue-500', bg: 'bg-blue-50' },
  { type: 'quiz', label: 'Quiz', icon: HelpCircle, color: 'text-green-500', bg: 'bg-green-50' },
]
</script>

<template>
  <div class="mt-8 border-t border-dashed border-slate-200 pt-6">
    <div class="flex items-center gap-2 mb-4">
      <Plus class="w-5 h-5 text-slate-400" />
      <h3 class="font-medium text-slate-700">Thêm nội dung</h3>
    </div>
    
    <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
      <button
        v-for="item in contentTypes"
        :key="item.type"
        @click="addContent(item.type)"
        class="flex flex-col items-center justify-center gap-2 p-4 rounded-xl border border-slate-200 hover:border-blue-300 hover:shadow-sm bg-white transition-all group"
      >
        <div class="w-10 h-10 rounded-full flex items-center justify-center transition-colors" :class="[item.bg, item.color, 'group-hover:bg-blue-50 group-hover:text-blue-600']">
          <component :is="item.icon" class="w-5 h-5" />
        </div>
        <span class="text-sm font-medium text-slate-600 group-hover:text-blue-700">{{ item.label }}</span>
      </button>
    </div>
  </div>
</template>

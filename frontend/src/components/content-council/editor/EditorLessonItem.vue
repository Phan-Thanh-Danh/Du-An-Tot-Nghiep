<script setup lang="ts">
import { computed, inject, ref } from 'vue'
import { MoreVertical, Edit2, EyeOff, Eye, Trash2, FileText, Video, PlaySquare, File, HelpCircle } from 'lucide-vue-next'
import type { EditorLesson } from '@/types/content-council/curriculumEditor'
import { onClickOutside } from '@vueuse/core'

const props = defineProps<{
  lesson: EditorLesson
}>()

const editor = inject<any>('curriculumEditor')
const menuRef = ref<HTMLElement | null>(null)
const isMenuOpen = ref(false)

const isSelected = computed(() => editor.selectedLessonId.value === props.lesson.id)

const toggleMenu = () => {
  isMenuOpen.value = !isMenuOpen.value
}

onClickOutside(menuRef, () => {
  isMenuOpen.value = false
})

const onSelect = () => {
  editor.selectLesson(props.lesson.id)
}

const onEdit = () => {
  isMenuOpen.value = false
  editor.editingLesson.value = props.lesson
  editor.isLessonModalOpen.value = true
}

const onToggleStatus = (newStatus: 'draft' | 'published') => {
  isMenuOpen.value = false
  editor.saveLesson({ ...props.lesson, status: newStatus })
}

const onDelete = () => {
  isMenuOpen.value = false
  editor.deleteTarget.value = { type: 'lesson', id: props.lesson.id }
  editor.isDeleteDialogOpen.value = true
}

const getLessonIcon = (type: string) => {
  switch (type) {
    case 'video': return Video
    case 'slide': case 'slide_html': return PlaySquare
    case 'document': case 'pdf': return File
    case 'text': return FileText
    case 'quiz': return HelpCircle
    default: return FileText
  }
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
  <div 
    class="group flex items-center justify-between p-2 rounded-md cursor-pointer transition-colors"
    :class="[
      isSelected ? 'bg-blue-50 border-l-2 border-blue-600' : 'hover:bg-slate-100 border-l-2 border-transparent'
    ]"
    @click="onSelect"
  >
    <div class="flex items-center gap-2.5 flex-1 min-w-0 pl-1">
      <component :is="getLessonIcon(lesson.type)" class="w-4 h-4" :class="isSelected ? 'text-blue-600' : 'text-slate-400'" />
      <span class="truncate text-sm" :class="isSelected ? 'font-semibold text-blue-900' : 'text-slate-700'">
        {{ lesson.title }}
      </span>
      <span 
        class="px-1.5 py-0.5 text-[10px] rounded uppercase font-semibold shrink-0 ml-auto mr-1"
        :class="getStatusBadge(lesson.status).class"
      >
        {{ getStatusBadge(lesson.status).text }}
      </span>
    </div>

    <!-- Action Menu -->
    <div class="relative flex items-center" ref="menuRef" @click.stop>
      <button 
        @click="toggleMenu"
        class="w-6 h-6 flex items-center justify-center rounded transition-colors"
        :class="isSelected ? 'hover:bg-blue-200 text-blue-600' : 'hover:bg-slate-200 text-slate-400 opacity-0 group-hover:opacity-100'"
      >
        <MoreVertical class="w-4 h-4" />
      </button>

      <div v-if="isMenuOpen" class="absolute right-0 top-full mt-1 w-44 bg-white border border-card rounded-lg shadow-lg py-1 z-20">
        <button @click="onEdit" class="w-full text-left px-3 py-1.5 text-sm hover:bg-slate-50 flex items-center gap-2">
          <Edit2 class="w-4 h-4 text-slate-500" /> Sửa bài học
        </button>
        <button v-if="lesson.status !== 'draft'" @click="onToggleStatus('draft')" class="w-full text-left px-3 py-1.5 text-sm hover:bg-slate-50 flex items-center gap-2">
          <EyeOff class="w-4 h-4 text-slate-500" /> Chuyển về nháp
        </button>
        <button v-if="lesson.status !== 'published'" @click="onToggleStatus('published')" class="w-full text-left px-3 py-1.5 text-sm hover:bg-slate-50 flex items-center gap-2">
          <Eye class="w-4 h-4 text-slate-500" /> Xuất bản
        </button>
        <div class="h-px bg-slate-100 my-1"></div>
        <button @click="onDelete" class="w-full text-left px-3 py-1.5 text-sm hover:bg-red-50 text-red-600 flex items-center gap-2">
          <Trash2 class="w-4 h-4" /> Xóa bài học
        </button>
      </div>
    </div>
  </div>
</template>

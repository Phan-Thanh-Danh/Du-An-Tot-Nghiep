<script setup lang="ts">
import { inject, ref } from 'vue'
import { MoreVertical, Edit2, Copy, EyeOff, Eye, Trash2, GripVertical, Video, PlaySquare, File, FileText, HelpCircle } from 'lucide-vue-next'
import type { EditorContentBlock } from '@/types/content-council/curriculumEditor'
import { onClickOutside } from '@vueuse/core'
import { useRouter, useRoute } from 'vue-router'

const props = defineProps<{
  content: EditorContentBlock
}>()

const editor = inject<any>('curriculumEditor')
const menuRef = ref<HTMLElement | null>(null)
const isMenuOpen = ref(false)

const toggleMenu = () => {
  isMenuOpen.value = !isMenuOpen.value
}

onClickOutside(menuRef, () => {
  isMenuOpen.value = false
})

const onEdit = () => {
  isMenuOpen.value = false
  editor.editingContent.value = props.content
  editor.selectedContentType.value = props.content.type
  editor.isContentDrawerOpen.value = true
}

const onDuplicate = () => {
  isMenuOpen.value = false
  const newContent = {
    ...props.content,
    title: `${props.content.title} (Bản sao)`,
    order: props.content.order + 1 // Simple logic, might need reordering later
  }
  editor.saveContent(newContent)
}

const onToggleStatus = (newStatus: 'draft' | 'published' | 'hidden') => {
  isMenuOpen.value = false
  editor.saveContent({ ...props.content, status: newStatus })
}

const onDelete = () => {
  isMenuOpen.value = false
  editor.deleteTarget.value = { type: 'content', id: props.content.id, parentId: props.content.lessonId }
  editor.isDeleteDialogOpen.value = true
}

const router = useRouter()
const route = useRoute()

const onPreview = () => {
  isMenuOpen.value = false
  const subjectId = route.params.subjectId
  
  // Use editor's selected lesson ID instead of props.content.lessonId 
  // because props.content.lessonId might be incorrect or undefined from the backend API
  const lessonId = editor.selectedLesson.value?.id
  if (!lessonId) return

  const url = router.resolve({
    name: 'content-council-subject-preview',
    params: { subjectId: subjectId },
    query: { lessonId: lessonId }
  }).href
  
  window.open(url, '_blank')
}

const getIcon = (type: string) => {
  switch (type) {
    case 'video': return Video
    case 'slide_html': return PlaySquare
    case 'document': return File
    case 'text': return FileText
    case 'quiz': return HelpCircle
    default: return FileText
  }
}

const formatDuration = (seconds?: number) => {
  if (!seconds) return ''
  const m = Math.floor(seconds / 60)
  const s = seconds % 60
  return `${m}:${s.toString().padStart(2, '0')}`
}

const getStatusBadge = (status: string) => {
  switch (status) {
    case 'draft': return { text: 'Nháp', class: 'bg-slate-200 text-slate-600' }
    case 'published': return { text: 'Xuất bản', class: 'bg-green-100 text-green-700' }
    case 'hidden': return { text: 'Đang ẩn', class: 'bg-red-100 text-red-700' }
    default: return { text: status, class: 'bg-slate-100 text-slate-500' }
  }
}
</script>

<template>
  <div class="group bg-white border border-slate-200 rounded-xl hover:border-blue-300 hover:shadow-sm transition-all flex items-stretch">
    <!-- Drag Handle -->
    <div class="w-8 flex items-center justify-center border-r border-slate-100 cursor-grab active:cursor-grabbing text-slate-300 hover:text-slate-500 bg-slate-50 rounded-l-xl">
      <GripVertical class="w-4 h-4" />
    </div>

    <!-- Content Info -->
    <div class="flex-1 p-4 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div class="flex items-start gap-3 min-w-0">
        <div class="w-10 h-10 rounded bg-slate-100 flex items-center justify-center shrink-0 text-slate-500">
          <component :is="getIcon(content.type)" class="w-5 h-5" />
        </div>
        
        <div class="min-w-0 flex-1">
          <h4 class="font-medium text-slate-800 truncate">{{ content.title }}</h4>
          <div class="text-sm text-slate-500 flex items-center gap-2 mt-1 flex-wrap">
            <span class="capitalize">{{ content.type.replace('_', ' ') }}</span>
            <span v-if="content.durationSeconds" class="flex items-center gap-1 before:content-['·'] before:mr-1">
              {{ formatDuration(content.durationSeconds) }}
            </span>
            <span v-if="content.fileSize" class="flex items-center gap-1 before:content-['·'] before:mr-1">
              {{ (content.fileSize / 1024 / 1024).toFixed(2) }} MB
            </span>
            <span v-if="content.quizQuestionCount" class="flex items-center gap-1 before:content-['·'] before:mr-1">
              {{ content.quizQuestionCount }} câu
            </span>
          </div>
        </div>
      </div>

      <div class="flex items-center justify-between sm:justify-end gap-4 shrink-0 pl-11 sm:pl-0">
        <span 
          class="px-2 py-0.5 text-[11px] rounded uppercase font-semibold"
          :class="getStatusBadge(content.status).class"
        >
          {{ getStatusBadge(content.status).text }}
        </span>

        <!-- Action Menu -->
        <div class="relative flex items-center" ref="menuRef" @click.stop>
          <button 
            @click="toggleMenu"
            class="w-8 h-8 flex items-center justify-center rounded hover:bg-slate-100 text-slate-500 transition-colors"
          >
            <MoreVertical class="w-4 h-4" />
          </button>

          <div v-if="isMenuOpen" class="absolute right-0 top-full mt-1 w-48 bg-white border border-card rounded-lg shadow-lg py-1 z-20">
            <button @click="onPreview" class="w-full text-left px-3 py-1.5 text-sm hover:bg-slate-50 flex items-center gap-2">
              <Eye class="w-4 h-4 text-slate-500" /> Xem trước
            </button>
            <button @click="onEdit" class="w-full text-left px-3 py-1.5 text-sm hover:bg-slate-50 flex items-center gap-2">
              <Edit2 class="w-4 h-4 text-slate-500" /> Chỉnh sửa
            </button>
            <button @click="onDuplicate" class="w-full text-left px-3 py-1.5 text-sm hover:bg-slate-50 flex items-center gap-2">
              <Copy class="w-4 h-4 text-slate-500" /> Nhân bản
            </button>
            <div class="h-px bg-slate-100 my-1"></div>
            <button v-if="content.status === 'published'" @click="onToggleStatus('hidden')" class="w-full text-left px-3 py-1.5 text-sm hover:bg-slate-50 flex items-center gap-2">
              <EyeOff class="w-4 h-4 text-slate-500" /> Ẩn nội dung
            </button>
            <button v-if="content.status === 'hidden'" @click="onToggleStatus('published')" class="w-full text-left px-3 py-1.5 text-sm hover:bg-slate-50 flex items-center gap-2">
              <Eye class="w-4 h-4 text-slate-500" /> Hiện nội dung
            </button>
            <div class="h-px bg-slate-100 my-1"></div>
            <button @click="onDelete" class="w-full text-left px-3 py-1.5 text-sm hover:bg-red-50 text-red-600 flex items-center gap-2">
              <Trash2 class="w-4 h-4" /> Xóa nội dung
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

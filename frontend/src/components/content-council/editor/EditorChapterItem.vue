<script setup lang="ts">
import { computed, inject, ref } from 'vue'
import { ChevronRight, ChevronDown, MoreVertical, Plus, Edit2, EyeOff, Eye, Trash2 } from 'lucide-vue-next'
import type { EditorChapter } from '@/types/content-council/curriculumEditor'
import EditorLessonItem from './EditorLessonItem.vue'
import { onClickOutside } from '@vueuse/core'

const props = defineProps<{
  chapter: EditorChapter
}>()

const editor = inject<any>('curriculumEditor')
const menuRef = ref<HTMLElement | null>(null)
const isMenuOpen = ref(false)

const isExpanded = computed(() => editor.expandedChapterIds.value.includes(props.chapter.id))

const toggleMenu = () => {
  isMenuOpen.value = !isMenuOpen.value
}

onClickOutside(menuRef, () => {
  isMenuOpen.value = false
})

const onAddLesson = () => {
  isMenuOpen.value = false
  editor.selectedChapterId.value = props.chapter.id
  editor.editingLesson.value = null
  editor.isLessonModalOpen.value = true
}

const onEditChapter = () => {
  isMenuOpen.value = false
  editor.editingChapter.value = props.chapter
  editor.isChapterModalOpen.value = true
}

const onToggleVisibility = () => {
  isMenuOpen.value = false
  editor.saveChapter({ ...props.chapter, hidden: !props.chapter.hidden })
}

const onDeleteChapter = () => {
  isMenuOpen.value = false
  editor.deleteTarget.value = { type: 'chapter', id: props.chapter.id }
  editor.isDeleteDialogOpen.value = true
}
</script>

<template>
  <div class="border border-card rounded-lg bg-white mb-2">
    <!-- Chapter Header -->
    <div 
      class="flex items-center justify-between p-2 hover:bg-slate-50 cursor-pointer transition-colors"
      :class="isExpanded ? 'rounded-t-lg' : 'rounded-lg'"
      @click="editor.toggleChapter(chapter.id)"
    >
      <div class="flex items-center gap-2 flex-1 min-w-0">
        <div class="w-5 h-5 flex items-center justify-center text-slate-400">
          <ChevronDown v-if="isExpanded" class="w-4 h-4" />
          <ChevronRight v-else class="w-4 h-4" />
        </div>
        <div class="truncate text-sm font-medium" :class="chapter.hidden ? 'text-slate-400' : 'text-slate-800'">
          {{ chapter.title }}
        </div>
        <span 
          v-if="chapter.hidden" 
          class="px-1.5 py-0.5 text-[10px] bg-slate-100 text-slate-500 rounded uppercase font-semibold shrink-0"
        >
          Đã ẩn
        </span>
      </div>

      <!-- Action Menu -->
      <div class="relative flex items-center ml-2" ref="menuRef" @click.stop>
        <div class="text-xs text-slate-400 mr-2">{{ chapter.lessons.length }} bài</div>
        <button 
          @click="toggleMenu"
          class="w-7 h-7 flex items-center justify-center rounded hover:bg-slate-200 text-slate-500 transition-colors"
        >
          <MoreVertical class="w-4 h-4" />
        </button>

        <div v-if="isMenuOpen" class="absolute right-0 top-full mt-1 w-48 bg-white border border-card rounded-lg shadow-lg py-1 z-20">
          <button @click="onAddLesson" class="w-full text-left px-3 py-1.5 text-sm hover:bg-slate-50 flex items-center gap-2">
            <Plus class="w-4 h-4 text-slate-500" /> Thêm bài học
          </button>
          <button @click="onEditChapter" class="w-full text-left px-3 py-1.5 text-sm hover:bg-slate-50 flex items-center gap-2">
            <Edit2 class="w-4 h-4 text-slate-500" /> Đổi tên chương
          </button>
          <button @click="onToggleVisibility" class="w-full text-left px-3 py-1.5 text-sm hover:bg-slate-50 flex items-center gap-2">
            <Eye class="w-4 h-4 text-slate-500" v-if="chapter.hidden" />
            <EyeOff class="w-4 h-4 text-slate-500" v-else />
            {{ chapter.hidden ? 'Hiện chương' : 'Ẩn chương' }}
          </button>
          <div class="h-px bg-slate-100 my-1"></div>
          <button @click="onDeleteChapter" class="w-full text-left px-3 py-1.5 text-sm hover:bg-red-50 text-red-600 flex items-center gap-2">
            <Trash2 class="w-4 h-4" /> Xóa chương
          </button>
        </div>
      </div>
    </div>

    <!-- Chapter Content (Lessons) -->
    <div v-show="isExpanded" class="border-t border-card bg-slate-50/50 p-1.5 space-y-1 rounded-b-lg">
      <EditorLessonItem
        v-for="lesson in chapter.lessons"
        :key="lesson.id"
        :lesson="lesson"
      />
      
      <button 
        @click="onAddLesson"
        class="w-full flex items-center gap-2 px-3 py-2 text-sm text-slate-500 hover:text-blue-600 hover:bg-blue-50 rounded-md transition-colors group"
      >
        <Plus class="w-4 h-4 opacity-70 group-hover:opacity-100" />
        <span>Thêm bài học...</span>
      </button>
    </div>
  </div>
</template>

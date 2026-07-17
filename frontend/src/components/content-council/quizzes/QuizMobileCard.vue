<script setup lang="ts">
import { ContentCouncilQuiz } from '@/types/content-council/quiz'
import QuizStatusBadge from './QuizStatusBadge.vue'
import { 
  MoreVertical, Eye, Edit3, PenTool, Copy, Send, RotateCcw, 
  PlayCircle, Lock, Trash2, AlertTriangle, Clock, ListOrdered, CheckCircle2 
} from 'lucide-vue-next'
import { ref, onMounted, onUnmounted } from 'vue'

const props = defineProps<{
  quiz: ContentCouncilQuiz
}>()

const emit = defineEmits(['action', 'click'])

const getExamTypeLabel = (type: string) => {
  const map: Record<string, string> = {
    'lesson_quiz': 'Quiz bài học',
    'chapter_quiz': 'Quiz chương',
    'midterm': 'Giữa kỳ',
    'final': 'Cuối kỳ',
    'regular_test': 'Thường xuyên'
  }
  return map[type] || type
}

const getFormatLabel = (format: string) => {
  const map: Record<string, string> = {
    'multiple_choice': 'Trắc nghiệm',
    'essay': 'Tự luận',
    'mixed': 'Hỗn hợp'
  }
  return map[format] || format
}

const handleAction = (actionType: string) => {
  isMenuOpen.value = false
  emit('action', { type: actionType, quiz: props.quiz })
}

const isMenuOpen = ref(false)
const toggleMenu = () => {
  isMenuOpen.value = !isMenuOpen.value
}
const closeMenu = () => {
  isMenuOpen.value = false
}

onMounted(() => {
  document.addEventListener('click', closeMenu)
})

onUnmounted(() => {
  document.removeEventListener('click', closeMenu)
})
</script>

<template>
  <div 
    class="bg-white rounded-xl shadow-sm border border-slate-200 p-4 mb-3 hover:shadow transition-shadow cursor-pointer flex flex-col gap-3"
    @click="emit('click', quiz)"
  >
    <!-- Header: Status & Actions -->
    <div class="flex items-center justify-between">
      <div class="flex items-center gap-2">
        <span class="font-mono text-xs font-medium text-slate-700 bg-slate-100 px-2 py-1 rounded">
          {{ quiz.code }}
        </span>
        <QuizStatusBadge :status="quiz.status" :trangThaiDuyet="quiz.trangThaiDuyet" />
      </div>

      <div @click.stop>
        <div class="relative inline-block text-left">
          <button @click="toggleMenu" class="p-1 rounded-lg text-slate-400 hover:text-slate-600 hover:bg-slate-100 transition-colors">
            <MoreVertical class="w-5 h-5" />
          </button>
          
          <transition
            enter-active-class="transition ease-out duration-100"
            enter-from-class="transform opacity-0 scale-95"
            enter-to-class="transform opacity-100 scale-100"
            leave-active-class="transition ease-in duration-75"
            leave-from-class="transform opacity-100 scale-100"
            leave-to-class="transform opacity-0 scale-95"
          >
            <div v-if="isMenuOpen" class="absolute right-0 z-50 mt-1 w-48 origin-top-right rounded-lg bg-white shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none divide-y divide-slate-100">
              <div class="p-1">
                  <button 
                    @click="handleAction('edit')"
                    class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm disabled:opacity-50 disabled:cursor-not-allowed text-slate-700 hover:text-blue-700 hover:bg-blue-50"
                    :disabled="quiz.status === 'open'"
                  >
                    <Edit3 class="w-4 h-4" /> Chỉnh sửa
                  </button>
                  <button 
                    @click="handleAction('build')"
                    class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm disabled:opacity-50 disabled:cursor-not-allowed text-slate-700 hover:text-blue-700 hover:bg-blue-50"
                    :disabled="quiz.status === 'open'"
                  >
                    <PenTool class="w-4 h-4" /> Xây dựng đề
                  </button>
                  <button 
                    @click="handleAction('duplicate')"
                    class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-slate-700 hover:text-blue-700 hover:bg-blue-50"
                  >
                    <Copy class="w-4 h-4" /> Nhân bản
                  </button>
              </div>

              <div class="p-1">
                  <button v-if="quiz.status === 'draft' && quiz.trangThaiDuyet !== 'da_xac_thuc'"
                    @click="handleAction('validate')"
                    class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-slate-700 hover:text-emerald-700 hover:bg-emerald-50"
                  >
                    <CheckCircle2 class="w-4 h-4 text-emerald-500" /> Xác thực Quiz
                  </button>

                  <button v-else-if="quiz.status === 'draft' && quiz.trangThaiDuyet === 'da_xac_thuc'"
                    @click="handleAction('publish')"
                    class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-slate-700 hover:text-blue-700 hover:bg-blue-50"
                  >
                    <Send class="w-4 h-4 text-blue-500" /> Xuất bản
                  </button>

                  <button v-if="quiz.status === 'published' || quiz.status === 'closed'"
                    @click="handleAction('unpublish')"
                    class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-slate-700 hover:text-blue-700 hover:bg-blue-50"
                  >
                    <RotateCcw class="w-4 h-4" /> Chuyển về nháp
                  </button>

                  <button v-if="quiz.status === 'published'"
                    @click="handleAction('open')"
                    class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-green-600 hover:text-green-700 hover:bg-green-50"
                  >
                    <PlayCircle class="w-4 h-4" /> Mở Quiz
                  </button>

                  <button v-if="quiz.status === 'open'"
                    @click="handleAction('close')"
                    class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-amber-600 hover:text-amber-700 hover:bg-amber-50"
                  >
                    <Lock class="w-4 h-4" /> Đóng Quiz
                  </button>
              </div>

              <div class="p-1" v-if="quiz.status === 'draft'">
                  <button 
                    @click="handleAction('delete')"
                    class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm disabled:opacity-50 disabled:cursor-not-allowed text-red-600 hover:text-red-700 hover:bg-red-50"
                  >
                    <Trash2 class="w-4 h-4" /> Xóa
                  </button>
              </div>
            </div>
          </transition>
        </div>
      </div>
    </div>

    <!-- Title & Subject -->
    <div>
      <h3 class="font-bold text-slate-800 leading-tight mb-1">{{ quiz.title }}</h3>
      <p class="text-sm text-slate-600">
        {{ quiz.subjectCode }} • {{ getExamTypeLabel(quiz.examType) }} • {{ getFormatLabel(quiz.format) }}
      </p>
    </div>

    <!-- Stats Row -->
    <div class="flex items-center gap-4 text-sm text-slate-600 bg-slate-50 p-2.5 rounded-lg border border-slate-100">
      <div class="flex items-center gap-1.5 flex-1">
        <ListOrdered class="w-4 h-4 text-slate-400" />
        <span v-if="quiz.questionCount === 0" class="text-xs text-red-600 font-medium">0 câu</span>
        <span v-else class="font-medium">{{ quiz.questionCount }} câu</span>
      </div>
      
      <div class="flex items-center gap-1.5 flex-1">
        <Clock class="w-4 h-4 text-slate-400" />
        <span class="font-medium">{{ quiz.durationMinutes > 0 ? `${quiz.durationMinutes}'` : '---' }}</span>
      </div>

      <div class="flex items-center gap-1.5 flex-1">
        <CheckCircle2 class="w-4 h-4 text-slate-400" />
        <span class="font-medium">{{ quiz.totalScore }} điểm</span>
      </div>
    </div>
  </div>
</template>

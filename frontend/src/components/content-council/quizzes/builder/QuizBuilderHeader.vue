<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { ChevronLeft, Save, Send } from 'lucide-vue-next'
import { ContentCouncilQuiz } from '@/types/content-council/quiz'
import QuizStatusBadge from '../QuizStatusBadge.vue'

const props = defineProps<{
  quiz: ContentCouncilQuiz
  isDirty: boolean
  isSaving: boolean
  isReadOnly: boolean
  canPublish: boolean
}>()

const emit = defineEmits(['save', 'publish'])
const router = useRouter()

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

const formatString = computed(() => {
  return `${props.quiz.subjectCode} · ${getExamTypeLabel(props.quiz.examType)} · ${getFormatLabel(props.quiz.format)} · ${props.quiz.durationMinutes} phút`
})
</script>

<template>
  <div class="bg-white border-b border-slate-200">
    <div class="px-6 py-4 mx-auto w-full max-w-[1600px] flex flex-col md:flex-row md:items-center justify-between gap-4">
      
      <!-- Left: Quiz Info -->
      <div>
        <button 
          @click="router.push({ name: 'content-council-quiz-edit', params: { quizId: quiz.id } })"
          class="flex items-center gap-1 text-sm font-medium text-slate-500 hover:text-slate-800 transition-colors mb-2"
        >
          <ChevronLeft class="w-4 h-4" />
          Quay lại form thông tin
        </button>
        <div class="flex items-center gap-3 mb-1.5">
          <h1 class="text-xl font-bold text-slate-800 tracking-tight leading-none line-clamp-1">
            {{ quiz.title || 'Quiz không tiêu đề' }}
          </h1>
          <QuizStatusBadge :status="quiz.status" />
        </div>
        <div class="flex items-center gap-2 text-sm text-slate-500">
          <span>{{ formatString }}</span>
          <span class="w-1 h-1 rounded-full bg-slate-300"></span>
          <span class="font-medium text-slate-700">Tổng điểm cấu hình: {{ quiz.totalScore }}</span>
        </div>
      </div>

      <!-- Right: Actions -->
      <div class="flex items-center gap-3">
        <span v-if="isDirty" class="text-sm text-amber-600 font-medium mr-2">Có thay đổi chưa lưu</span>
        
        <button 
          v-if="!isReadOnly"
          @click="emit('save')"
          :disabled="isSaving || !isDirty"
          class="flex items-center gap-2 px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 font-medium rounded-lg transition-colors disabled:opacity-50"
        >
          <Save class="w-4 h-4" />
          {{ isSaving ? 'Đang lưu...' : 'Lưu cấu trúc' }}
        </button>

        <button 
          v-if="quiz.status === 'draft' || quiz.status === 'published'"
          @click="emit('publish')"
          :disabled="isSaving || (!canPublish && quiz.status === 'draft')"
          class="flex items-center gap-2 px-5 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors shadow-sm disabled:opacity-50"
          :class="{'opacity-50 cursor-not-allowed': !canPublish && quiz.status === 'draft'}"
        >
          <Send class="w-4 h-4" />
          {{ quiz.status === 'published' ? 'Cập nhật bản xuất bản' : 'Xuất bản' }}
        </button>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { QuizBuilderQuestion } from '@/types/content-council/quiz'
import SelectedQuestionCard from './SelectedQuestionCard.vue'

const props = defineProps<{
  questions: QuizBuilderQuestion[]
  isReadOnly: boolean
}>()

const emit = defineEmits(['preview', 'update-score', 'move-up', 'move-down', 'remove'])

// Sorting is handled by the composable, but we display them ordered
const sortedQuestions = computed(() => {
  return [...props.questions].sort((a, b) => a.order - b.order)
})

</script>

<template>
  <div class="bg-slate-50 border-r border-slate-200 h-full flex flex-col">
    <!-- Header -->
    <div class="px-5 py-4 border-b border-slate-200 bg-white shrink-0 flex items-center justify-between">
      <div>
        <h2 class="font-bold text-slate-800 text-lg flex items-center gap-2">
          Câu hỏi trong Quiz
          <span class="bg-blue-100 text-blue-700 text-xs py-0.5 px-2 rounded-full font-bold">{{ questions.length }}</span>
        </h2>
        <p class="text-xs text-slate-500 mt-0.5">Sử dụng mũi tên để sắp xếp thứ tự hoặc chỉnh sửa điểm từng câu.</p>
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="questions.length === 0" class="flex-1 flex flex-col items-center justify-center p-8 text-center text-slate-500">
      <div class="w-16 h-16 bg-slate-200 rounded-full flex items-center justify-center mb-4 opacity-50">
        <svg class="w-8 h-8 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 002-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
        </svg>
      </div>
      <h3 class="text-base font-bold text-slate-700 mb-1">Quiz chưa có câu hỏi</h3>
      <p class="text-sm">Chọn câu hỏi từ Ngân hàng câu hỏi bên phải để bắt đầu xây dựng đề.</p>
    </div>

    <!-- List -->
    <div v-else class="flex-1 overflow-y-auto p-4 space-y-3">
      <SelectedQuestionCard
        v-for="(q, index) in sortedQuestions"
        :key="q.questionId"
        :question="q"
        :is-first="index === 0"
        :is-last="index === sortedQuestions.length - 1"
        :is-read-only="isReadOnly"
        @preview="id => emit('preview', id)"
        @update-score="(id, score) => emit('update-score', id, score)"
        @move-up="idx => emit('move-up', idx)"
        @move-down="idx => emit('move-down', idx)"
        @remove="id => emit('remove', id)"
      />
      <div class="h-10"></div> <!-- padding bottom -->
    </div>
  </div>
</template>

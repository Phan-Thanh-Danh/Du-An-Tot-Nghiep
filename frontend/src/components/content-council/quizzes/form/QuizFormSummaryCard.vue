<script setup lang="ts">
import { computed } from 'vue'
import { QuizFormData } from '@/types/content-council/quizForm'
import { CheckCircle2, AlertTriangle } from 'lucide-vue-next'
import QuizStatusBadge from '../QuizStatusBadge.vue'

const props = defineProps<{
  formData: QuizFormData
  hasQuestions: boolean
}>()

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
</script>

<template>
  <div class="bg-white rounded-xl border border-slate-200 shadow-sm sticky top-24">
    <div class="px-5 py-4 border-b border-slate-100">
      <h2 class="font-bold text-slate-800">Tóm tắt cấu hình</h2>
    </div>
    
    <div class="p-5 space-y-4 text-sm">
      <div class="flex justify-between items-start gap-4">
        <span class="text-slate-500">Tên Quiz:</span>
        <span class="font-medium text-slate-800 text-right line-clamp-2">{{ formData.title || '---' }}</span>
      </div>

      <div class="flex justify-between items-start gap-4">
        <span class="text-slate-500">Trạng thái:</span>
        <QuizStatusBadge :status="formData.status" />
      </div>
      
      <div class="flex justify-between gap-4">
        <span class="text-slate-500">Loại:</span>
        <span class="font-medium text-slate-800 text-right">{{ getExamTypeLabel(formData.examType) }}</span>
      </div>
      
      <div class="flex justify-between gap-4">
        <span class="text-slate-500">Hình thức:</span>
        <span class="font-medium text-slate-800 text-right">{{ getFormatLabel(formData.format) }}</span>
      </div>
      
      <div class="flex justify-between gap-4">
        <span class="text-slate-500">Thời gian:</span>
        <span class="font-medium text-slate-800 text-right">{{ formData.durationMinutes > 0 ? `${formData.durationMinutes} phút` : '---' }}</span>
      </div>
      
      <div class="flex justify-between gap-4">
        <span class="text-slate-500">Tổng điểm:</span>
        <span class="font-medium text-slate-800 text-right">{{ formData.totalScore }}</span>
      </div>
      
      <div class="flex justify-between gap-4">
        <span class="text-slate-500">Điều kiện đạt:</span>
        <span class="font-medium text-slate-800 text-right">
          <template v-if="formData.passMethod === 'score'">
            Từ {{ formData.passingScore ?? 0 }} điểm
          </template>
          <template v-else-if="formData.passMethod === 'correct_answer_count'">
            Từ {{ formData.minimumCorrectAnswers ?? 0 }} câu
          </template>
        </span>
      </div>
      
      <div class="flex justify-between gap-4">
        <span class="text-slate-500">Lượt làm:</span>
        <span class="font-medium text-slate-800 text-right">
          {{ formData.unlimitedAttempts ? 'Không giới hạn' : `Tối đa ${formData.maximumAttempts || 1} lượt` }}
        </span>
      </div>

      <div class="flex justify-between gap-4">
        <span class="text-slate-500">Điểm cuối:</span>
        <span class="font-medium text-slate-800 text-right">
          {{ formData.finalScoreMethod === 'highest' ? 'Cao nhất' : formData.finalScoreMethod === 'latest' ? 'Lần cuối' : 'Trung bình' }}
        </span>
      </div>

      <hr class="border-slate-100" />

      <div class="flex justify-between items-center gap-4">
        <span class="text-slate-500">Câu hỏi:</span>
        <div class="flex items-center gap-1.5" :class="hasQuestions ? 'text-green-600' : 'text-amber-600'">
          <CheckCircle2 v-if="hasQuestions" class="w-4 h-4" />
          <AlertTriangle v-else class="w-4 h-4" />
          <span class="font-medium">{{ hasQuestions ? 'Đã xây dựng đề' : 'Chưa xây dựng đề' }}</span>
        </div>
      </div>
      
      <p v-if="!hasQuestions" class="text-xs text-amber-600 bg-amber-50 p-2.5 rounded-lg border border-amber-100 mt-2 leading-relaxed">
        Bạn sẽ thêm và sắp xếp câu hỏi trong Quiz Builder sau khi lưu.
      </p>

    </div>
  </div>
</template>

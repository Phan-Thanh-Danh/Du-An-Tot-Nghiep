<script setup lang="ts">
import { computed } from 'vue'
import { QuizFormData } from '@/types/content-council/quizForm'

const props = defineProps<{
  modelValue: QuizFormData
  isReadOnly: boolean
  errors: Record<string, string>
}>()

const emit = defineEmits(['update:modelValue'])

const formData = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})

const examTypes = [
  { value: 'lesson_quiz', label: 'Quiz bài học', desc: 'Kiểm tra nhanh cuối bài học' },
  { value: 'chapter_quiz', label: 'Quiz chương', desc: 'Kiểm tra tổng hợp cuối chương' },
  { value: 'regular_test', label: 'Bài kiểm tra thường xuyên', desc: 'Đánh giá định kỳ trong quá trình học' },
  { value: 'midterm', label: 'Kiểm tra giữa kỳ', desc: 'Đánh giá giữa khóa học' },
  { value: 'final', label: 'Kiểm tra cuối kỳ', desc: 'Đánh giá tổng kết khóa học' }
]

const formats = [
  { value: 'multiple_choice', label: 'Trắc nghiệm' },
  { value: 'essay', label: 'Tự luận' },
  { value: 'mixed', label: 'Hỗn hợp' }
]

const updateField = (field: keyof QuizFormData, value: any) => {
  if (props.isReadOnly) return
  
  const newData = { ...formData.value, [field]: value }
  
  // Auto sync percentages based on format
  if (field === 'format') {
    if (value === 'multiple_choice') {
      newData.multipleChoicePercentage = 100
      newData.essayPercentage = 0
    } else if (value === 'essay') {
      newData.multipleChoicePercentage = 0
      newData.essayPercentage = 100
    } else if (value === 'mixed') {
      // default mixed split
      if (newData.multipleChoicePercentage === 100 || newData.multipleChoicePercentage === 0) {
        newData.multipleChoicePercentage = 70
        newData.essayPercentage = 30
      }
    }
    
    // Auto adjust passing method if switching to essay
    if (value === 'essay' && newData.passMethod === 'correct_answer_count') {
      newData.passMethod = 'score'
      newData.minimumCorrectAnswers = null
    }
  }
  
  // Keep sum = 100 when adjusting mixed, clamping inputs from 0 to 100%
  if (field === 'multipleChoicePercentage' && newData.format === 'mixed') {
    const raw = Number(value)
    const mc = isNaN(raw) ? 0 : Math.min(100, Math.max(0, Math.round(raw)))
    newData.multipleChoicePercentage = mc
    newData.essayPercentage = 100 - mc
  } else if (field === 'essayPercentage' && newData.format === 'mixed') {
    const raw = Number(value)
    const essay = isNaN(raw) ? 0 : Math.min(100, Math.max(0, Math.round(raw)))
    newData.essayPercentage = essay
    newData.multipleChoicePercentage = 100 - essay
  }
  
  emit('update:modelValue', newData)
}
</script>

<template>
  <div class="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden mb-6" :class="{'ring-1 ring-red-500': errors['durationMinutes']}">
    <div class="px-6 py-4 border-b border-slate-100 bg-slate-50 flex items-center justify-between">
      <div>
        <h2 class="text-base font-bold text-slate-800 flex items-center gap-2">
          2. Cấu trúc và thời gian làm bài
        </h2>
        <p class="text-xs text-slate-500 mt-1">Xác định loại đề và thời lượng làm bài cho phép (Hình thức: Trắc nghiệm).</p>
      </div>
      <div v-if="errors['durationMinutes']" class="text-xs font-medium bg-red-100 text-red-700 px-2.5 py-1 rounded-full flex items-center gap-1" role="alert">
        <span class="w-1.5 h-1.5 rounded-full bg-red-600"></span>
        Có lỗi
      </div>
    </div>

    <div class="p-6 space-y-8">
      
      <!-- Loại đề -->
      <div>
        <label class="block text-sm font-medium text-slate-700 mb-3">Loại đề</label>
        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-3">
          <label 
            v-for="type in examTypes" 
            :key="type.value"
            class="relative flex flex-col p-3 rounded-lg border cursor-pointer hover:bg-slate-50 transition-colors"
            :class="[
              formData.examType === type.value ? 'bg-blue-50/50 border-blue-500 ring-1 ring-blue-500' : 'border-slate-200',
              isReadOnly ? 'opacity-70 cursor-not-allowed hover:bg-transparent' : ''
            ]"
          >
            <div class="flex items-center justify-between mb-1">
              <span class="font-medium text-sm text-slate-800">{{ type.label }}</span>
              <input 
                type="radio" 
                :value="type.value" 
                :checked="formData.examType === type.value"
                @change="updateField('examType', type.value)"
                :disabled="isReadOnly"
                class="w-4 h-4 text-blue-600 border-slate-300 focus:ring-blue-500"
              >
            </div>
            <span class="text-xs text-slate-500">{{ type.desc }}</span>
          </label>
        </div>
      </div>

      <!-- Thời gian làm bài -->
      <div>
        <label class="block text-sm font-medium text-slate-700 mb-1.5">
          Thời gian làm bài (phút) <span class="text-red-500">*</span>
        </label>
        <div class="flex items-center gap-3">
          <input 
            type="number" 
            :value="formData.durationMinutes"
            @input="updateField('durationMinutes', Number(($event.target as HTMLInputElement).value))"
            :disabled="isReadOnly"
            min="1" max="240"
            class="w-32 border rounded-lg px-3 py-2 focus:ring-2 focus:outline-none transition-colors"
            :class="[
              errors['durationMinutes'] ? 'border-red-300 focus:ring-red-500' : 'border-slate-300 focus:border-blue-500 focus:ring-blue-500/20',
              isReadOnly ? 'bg-slate-50 text-slate-500 cursor-not-allowed' : 'bg-white text-slate-700'
            ]"
          />
          <span class="text-sm text-slate-500">phút</span>
        </div>
        <p v-if="errors['durationMinutes']" class="mt-1.5 text-sm text-red-600">{{ errors['durationMinutes'] }}</p>
        <p v-else class="mt-1.5 text-xs text-slate-500">Thời gian bắt đầu tính khi học sinh mở lượt làm Quiz.</p>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { QuizFormData } from '@/types/content-council/quizForm'

const props = defineProps<{
  modelValue: QuizFormData
  isReadOnly: boolean
}>()

const emit = defineEmits(['update:modelValue'])

const formData = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})

const updateField = (field: keyof QuizFormData, value: boolean) => {
  if (props.isReadOnly) return
  
  const newData = { ...formData.value, [field]: value }
  
  if (field === 'showResultAfterSubmit' && !value) {
    newData.showCorrectAnswerAfterSubmit = false
    newData.showExplanationAfterSubmit = false
  }
  
  emit('update:modelValue', newData)
}
</script>

<template>
  <div class="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden mb-6">
    <div class="px-6 py-4 border-b border-slate-100 bg-slate-50">
      <h2 class="text-base font-bold text-slate-800 flex items-center gap-2">
        6. Xáo trộn và hiển thị kết quả
      </h2>
      <p class="text-xs text-slate-500 mt-1">Cấu hình hành vi trong và sau khi làm bài.</p>
    </div>

    <div class="p-6 space-y-8">
      
      <!-- Xáo trộn -->
      <div>
        <h3 class="text-sm font-bold text-slate-800 mb-4 border-b border-slate-100 pb-2">Xáo trộn</h3>
        <div class="space-y-4">
          <label class="flex items-start gap-3 cursor-pointer" :class="isReadOnly ? 'opacity-70 cursor-not-allowed' : ''">
            <div class="relative inline-flex items-center mt-0.5">
              <input 
                type="checkbox" 
                :checked="formData.shuffleQuestions"
                @change="updateField('shuffleQuestions', ($event.target as HTMLInputElement).checked)"
                :disabled="isReadOnly"
                class="sr-only peer"
              >
              <div class="w-9 h-5 bg-slate-200 peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-blue-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:bg-blue-600"></div>
            </div>
            <div>
              <span class="block text-sm font-medium text-slate-700">Xáo trộn thứ tự câu hỏi</span>
              <span class="block text-xs text-slate-500 mt-0.5">Mỗi lượt làm có thể nhận thứ tự câu hỏi khác nhau.</span>
            </div>
          </label>

          <label class="flex items-start gap-3 cursor-pointer" :class="isReadOnly || formData.format === 'essay' ? 'opacity-70 cursor-not-allowed' : ''">
            <div class="relative inline-flex items-center mt-0.5">
              <input 
                type="checkbox" 
                :checked="formData.shuffleAnswers"
                @change="updateField('shuffleAnswers', ($event.target as HTMLInputElement).checked)"
                :disabled="isReadOnly || formData.format === 'essay'"
                class="sr-only peer"
              >
              <div class="w-9 h-5 bg-slate-200 peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-blue-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:bg-blue-600"></div>
            </div>
            <div>
              <span class="block text-sm font-medium text-slate-700">Xáo trộn đáp án</span>
              <span class="block text-xs text-slate-500 mt-0.5">Chỉ áp dụng cho câu hỏi trắc nghiệm.</span>
            </div>
          </label>
        </div>
      </div>

      <!-- Hiển thị kết quả -->
      <div>
        <h3 class="text-sm font-bold text-slate-800 mb-4 border-b border-slate-100 pb-2">Hiển thị sau khi nộp</h3>
        <div class="space-y-4">
          <label class="flex items-start gap-3 cursor-pointer" :class="isReadOnly ? 'opacity-70 cursor-not-allowed' : ''">
            <div class="relative inline-flex items-center mt-0.5">
              <input 
                type="checkbox" 
                :checked="formData.showResultAfterSubmit"
                @change="updateField('showResultAfterSubmit', ($event.target as HTMLInputElement).checked)"
                :disabled="isReadOnly"
                class="sr-only peer"
              >
              <div class="w-9 h-5 bg-slate-200 peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-blue-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:bg-blue-600"></div>
            </div>
            <div>
              <span class="block text-sm font-medium text-slate-700">Hiển thị kết quả (Điểm số)</span>
              <span class="block text-xs text-slate-500 mt-0.5">Học sinh có thể thấy điểm ngay sau khi nộp bài.</span>
            </div>
          </label>

          <label class="flex items-start gap-3 cursor-pointer" :class="isReadOnly || !formData.showResultAfterSubmit ? 'opacity-70 cursor-not-allowed' : ''">
            <div class="relative inline-flex items-center mt-0.5">
              <input 
                type="checkbox" 
                :checked="formData.showCorrectAnswerAfterSubmit"
                @change="updateField('showCorrectAnswerAfterSubmit', ($event.target as HTMLInputElement).checked)"
                :disabled="isReadOnly || !formData.showResultAfterSubmit"
                class="sr-only peer"
              >
              <div class="w-9 h-5 bg-slate-200 peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-blue-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:bg-blue-600"></div>
            </div>
            <div>
              <span class="block text-sm font-medium text-slate-700">Hiển thị đáp án đúng</span>
              <span class="block text-xs text-slate-500 mt-0.5">Học sinh có thể xem lại bài làm và thấy đáp án đúng.</span>
            </div>
          </label>

          <label class="flex items-start gap-3 cursor-pointer" :class="isReadOnly || !formData.showResultAfterSubmit ? 'opacity-70 cursor-not-allowed' : ''">
            <div class="relative inline-flex items-center mt-0.5">
              <input 
                type="checkbox" 
                :checked="formData.showExplanationAfterSubmit"
                @change="updateField('showExplanationAfterSubmit', ($event.target as HTMLInputElement).checked)"
                :disabled="isReadOnly || !formData.showResultAfterSubmit"
                class="sr-only peer"
              >
              <div class="w-9 h-5 bg-slate-200 peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-blue-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:bg-blue-600"></div>
            </div>
            <div>
              <span class="block text-sm font-medium text-slate-700">Hiển thị giải thích đáp án</span>
              <span class="block text-xs text-slate-500 mt-0.5">Hiển thị phần giải thích cho từng câu hỏi nếu có.</span>
            </div>
          </label>
        </div>
      </div>

    </div>
  </div>
</template>

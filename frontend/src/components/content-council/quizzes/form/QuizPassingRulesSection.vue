<script setup lang="ts">
import { computed } from 'vue'
import { QuizFormData } from '@/types/content-council/quizForm'

const props = defineProps<{
  modelValue: QuizFormData
  isReadOnly: boolean
  errors: Record<string, string>
  hasQuestions: boolean
}>()

const emit = defineEmits(['update:modelValue'])

const formData = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})

const updateField = (field: keyof QuizFormData, value: any) => {
  if (props.isReadOnly) return
  
  const newData = { ...formData.value, [field]: value }
  
  if (field === 'passMethod') {
    if (value === 'score') {
      newData.minimumCorrectAnswers = null
    } else if (value === 'correct_answer_count') {
      newData.passingScore = null
    }
  }
  
  emit('update:modelValue', newData)
}
</script>

<template>
  <div class="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden mb-6" :class="{'ring-1 ring-red-500': errors['totalScore'] || errors['passingScore'] || errors['minimumCorrectAnswers']}">
    <div class="px-6 py-4 border-b border-slate-100 bg-slate-50 flex items-center justify-between">
      <div>
        <h2 class="text-base font-bold text-slate-800 flex items-center gap-2">
          3. Điểm và điều kiện đạt
        </h2>
        <p class="text-xs text-slate-500 mt-1">Cấu hình cách tính điểm và điều kiện để học sinh vượt qua Quiz.</p>
      </div>
      <div v-if="errors['totalScore'] || errors['passingScore'] || errors['minimumCorrectAnswers']" class="text-xs font-medium bg-red-100 text-red-700 px-2.5 py-1 rounded-full flex items-center gap-1" role="alert">
        <span class="w-1.5 h-1.5 rounded-full bg-red-600"></span>
        Có lỗi
      </div>
    </div>

    <div class="p-6 space-y-6">
      
      <!-- Tổng điểm -->
      <div>
        <label class="block text-sm font-medium text-slate-700 mb-1.5">
          Tổng điểm <span class="text-red-500">*</span>
        </label>
        <input 
          type="number" 
          :value="formData.totalScore"
          @input="updateField('totalScore', Number(($event.target as HTMLInputElement).value))"
          :disabled="isReadOnly"
          min="1" max="1000" step="1"
          class="w-32 border rounded-lg px-3 py-2 focus:ring-2 focus:outline-none transition-colors"
          :class="[
            errors['totalScore'] ? 'border-red-300 focus:ring-red-500' : 'border-slate-300 focus:border-blue-500 focus:ring-blue-500/20',
            isReadOnly ? 'bg-slate-50 text-slate-500 cursor-not-allowed' : 'bg-white text-slate-700'
          ]"
        />
        <p v-if="errors['totalScore']" class="mt-1.5 text-sm text-red-600">{{ errors['totalScore'] }}</p>
        <p v-else class="mt-1.5 text-xs text-slate-500">Tổng điểm cấu hình phải khớp với tổng điểm các câu trong Quiz Builder.</p>
      </div>

      <hr class="border-slate-100" />

      <!-- Cách xác định đạt -->
      <div>
        <label class="block text-sm font-medium text-slate-700 mb-3">Cách xác định đạt</label>
        <div class="flex gap-6 mb-4">
          <label class="flex items-center gap-2 cursor-pointer" :class="isReadOnly ? 'opacity-70 cursor-not-allowed' : ''">
            <input 
              type="radio" 
              value="score" 
              :checked="formData.passMethod === 'score'"
              @change="updateField('passMethod', 'score')"
              :disabled="isReadOnly"
              class="w-4 h-4 text-blue-600 border-slate-300 focus:ring-blue-500"
            >
            <span class="text-sm font-medium text-slate-700">Theo điểm</span>
          </label>

          <label class="flex items-center gap-2 cursor-pointer" :class="isReadOnly || formData.format === 'essay' ? 'opacity-70 cursor-not-allowed' : ''">
            <input 
              type="radio" 
              value="correct_answer_count" 
              :checked="formData.passMethod === 'correct_answer_count'"
              @change="updateField('passMethod', 'correct_answer_count')"
              :disabled="isReadOnly || formData.format === 'essay'"
              class="w-4 h-4 text-blue-600 border-slate-300 focus:ring-blue-500"
            >
            <span class="text-sm font-medium text-slate-700">Theo số câu đúng</span>
          </label>
        </div>

        <!-- Inputs condition -->
        <div class="bg-slate-50 p-4 rounded-lg border border-slate-200">
          <template v-if="formData.passMethod === 'score'">
            <label class="block text-sm font-medium text-slate-700 mb-1.5">Điểm đạt</label>
            <div class="flex items-center gap-3">
              <input 
                type="number" 
                :value="formData.passingScore"
                @input="updateField('passingScore', Number(($event.target as HTMLInputElement).value))"
                :disabled="isReadOnly"
                min="0" :max="formData.totalScore" step="1"
                class="w-32 border border-slate-300 rounded px-3 py-1.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                :class="[
                  errors['passingScore'] ? 'border-red-300 focus:ring-red-500' : 'border-slate-300',
                  isReadOnly ? 'bg-slate-100 text-slate-500' : 'bg-white'
                ]"
              />
              <span class="text-sm font-medium text-slate-600">/ {{ formData.totalScore }} điểm</span>
            </div>
            <p v-if="errors['passingScore']" class="mt-2 text-sm text-red-600">{{ errors['passingScore'] }}</p>
            <p v-else-if="formData.passingScore !== null && formData.passingScore >= 0" class="mt-2 text-sm text-slate-600">
              Học sinh đạt khi có điểm từ <span class="font-bold">{{ formData.passingScore }}</span> trở lên.
            </p>
          </template>

          <template v-else-if="formData.passMethod === 'correct_answer_count'">
            <label class="block text-sm font-medium text-slate-700 mb-1.5">Số câu đúng tối thiểu</label>
            <div class="flex items-center gap-3">
              <input 
                type="number" 
                :value="formData.minimumCorrectAnswers"
                @input="updateField('minimumCorrectAnswers', Number(($event.target as HTMLInputElement).value))"
                :disabled="isReadOnly"
                min="1" step="1"
                class="w-32 border border-slate-300 rounded px-3 py-1.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                :class="[
                  errors['minimumCorrectAnswers'] ? 'border-red-300 focus:ring-red-500' : 'border-slate-300',
                  isReadOnly ? 'bg-slate-100 text-slate-500' : 'bg-white'
                ]"
              />
              <span class="text-sm font-medium text-slate-600">câu</span>
            </div>
            <p v-if="errors['minimumCorrectAnswers']" class="mt-2 text-sm text-red-600">{{ errors['minimumCorrectAnswers'] }}</p>
            <p v-else-if="!hasQuestions" class="mt-2 text-sm text-amber-600">Giá trị sẽ được kiểm tra lại sau khi xây dựng đề.</p>
          </template>
        </div>
        
        <p v-if="formData.passMethod === 'correct_answer_count'" class="mt-2 text-xs text-slate-500 italic">
          * Cách tính theo số câu đúng chỉ phù hợp với đề trắc nghiệm.
        </p>

      </div>

    </div>
  </div>
</template>

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

const updateField = (field: keyof QuizFormData, value: any) => {
  if (props.isReadOnly) return
  
  const newData = { ...formData.value, [field]: value }
  
  if (field === 'unlimitedAttempts') {
    if (value === true) {
      newData.maximumAttempts = null
    } else {
      newData.maximumAttempts = 1
    }
  }
  
  emit('update:modelValue', newData)
}
</script>

<template>
  <div class="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden mb-6" :class="{'ring-1 ring-red-500': errors['maximumAttempts']}">
    <div class="px-6 py-4 border-b border-slate-100 bg-slate-50 flex items-center justify-between">
      <div>
        <h2 class="text-base font-bold text-slate-800 flex items-center gap-2">
          4. Lượt làm và cách tính điểm
        </h2>
        <p class="text-xs text-slate-500 mt-1">Cấu hình số lần học sinh được làm Quiz và điểm cuối cùng ghi nhận.</p>
      </div>
      <div v-if="errors['maximumAttempts']" class="text-xs font-medium bg-red-100 text-red-700 px-2.5 py-1 rounded-full flex items-center gap-1" role="alert">
        <span class="w-1.5 h-1.5 rounded-full bg-red-600"></span>
        Có lỗi
      </div>
    </div>

    <div class="p-6 space-y-8">
      
      <!-- Số lượt làm -->
      <div class="flex flex-col sm:flex-row gap-6">
        <div class="flex-1">
          <label class="block text-sm font-medium text-slate-700 mb-3">Số lượt làm</label>
          <label class="flex items-center gap-3 cursor-pointer" :class="isReadOnly ? 'opacity-70 cursor-not-allowed' : ''">
            <div class="relative inline-flex items-center">
              <input 
                type="checkbox" 
                :checked="formData.unlimitedAttempts"
                @change="updateField('unlimitedAttempts', ($event.target as HTMLInputElement).checked)"
                :disabled="isReadOnly"
                class="sr-only peer"
              >
              <div class="w-11 h-6 bg-slate-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-600"></div>
            </div>
            <span class="text-sm font-medium text-slate-700">Không giới hạn số lượt làm</span>
          </label>

          <div v-if="!formData.unlimitedAttempts" class="mt-4 flex items-center gap-3">
            <span class="text-sm text-slate-600">Tối đa</span>
            <input 
              type="number" 
              :value="formData.maximumAttempts"
              @input="updateField('maximumAttempts', Number(($event.target as HTMLInputElement).value))"
              :disabled="isReadOnly"
              min="1" max="100" step="1"
              class="w-24 border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:outline-none transition-colors"
              :class="[
                errors['maximumAttempts'] ? 'border-red-300 focus:ring-red-500' : 'border-slate-300 focus:border-blue-500 focus:ring-blue-500/20',
                isReadOnly ? 'bg-slate-50 text-slate-500 cursor-not-allowed' : 'bg-white text-slate-700'
              ]"
            />
            <span class="text-sm text-slate-600">lượt</span>
          </div>
          <p v-if="errors['maximumAttempts']" class="mt-1.5 text-sm text-red-600">{{ errors['maximumAttempts'] }}</p>
        </div>

        <!-- Cách tính điểm cuối -->
        <div class="flex-1">
          <label class="block text-sm font-medium text-slate-700 mb-3">Cách tính điểm cuối</label>
          <div class="flex flex-col gap-3">
            <label class="flex items-start gap-2 cursor-pointer" :class="isReadOnly || (!formData.unlimitedAttempts && formData.maximumAttempts === 1) ? 'opacity-70 cursor-not-allowed' : ''">
              <input 
                type="radio" 
                value="highest" 
                :checked="formData.finalScoreMethod === 'highest'"
                @change="updateField('finalScoreMethod', 'highest')"
                :disabled="isReadOnly || (!formData.unlimitedAttempts && formData.maximumAttempts === 1)"
                class="mt-1 w-4 h-4 text-blue-600 border-slate-300 focus:ring-blue-500"
              >
              <div>
                <span class="block text-sm font-medium text-slate-700">Lấy điểm cao nhất</span>
                <span class="block text-xs text-slate-500 mt-0.5">Phù hợp với Quiz cho phép luyện tập nhiều lần.</span>
              </div>
            </label>

            <label class="flex items-start gap-2 cursor-pointer" :class="isReadOnly || (!formData.unlimitedAttempts && formData.maximumAttempts === 1) ? 'opacity-70 cursor-not-allowed' : ''">
              <input 
                type="radio" 
                value="latest" 
                :checked="formData.finalScoreMethod === 'latest'"
                @change="updateField('finalScoreMethod', 'latest')"
                :disabled="isReadOnly || (!formData.unlimitedAttempts && formData.maximumAttempts === 1)"
                class="mt-1 w-4 h-4 text-blue-600 border-slate-300 focus:ring-blue-500"
              >
              <div>
                <span class="block text-sm font-medium text-slate-700">Lấy điểm lần cuối</span>
                <span class="block text-xs text-slate-500 mt-0.5">Dùng kết quả ở lượt làm gần nhất.</span>
              </div>
            </label>

            <label class="flex items-start gap-2 cursor-pointer" :class="isReadOnly || (!formData.unlimitedAttempts && formData.maximumAttempts === 1) ? 'opacity-70 cursor-not-allowed' : ''">
              <input 
                type="radio" 
                value="average" 
                :checked="formData.finalScoreMethod === 'average'"
                @change="updateField('finalScoreMethod', 'average')"
                :disabled="isReadOnly || (!formData.unlimitedAttempts && formData.maximumAttempts === 1)"
                class="mt-1 w-4 h-4 text-blue-600 border-slate-300 focus:ring-blue-500"
              >
              <div>
                <span class="block text-sm font-medium text-slate-700">Lấy điểm trung bình</span>
                <span class="block text-xs text-slate-500 mt-0.5">Tính trung bình tất cả lượt làm hợp lệ.</span>
              </div>
            </label>
          </div>
        </div>

      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { QuizFormData } from '@/types/content-council/quizForm'
import { X, Calendar } from 'lucide-vue-next'
import QuizDateTimePicker from './QuizDateTimePicker.vue'

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
  emit('update:modelValue', { ...formData.value, [field]: value })
}

const clearSchedule = () => {
  if (props.isReadOnly) return
  emit('update:modelValue', { ...formData.value, openAt: null, closeAt: null })
}

// TC-009: Min Dates for Open and Close
const minOpenDate = computed(() => {
  const now = new Date()
  return new Date(now.getFullYear(), now.getMonth(), now.getDate())
})

const minCloseDate = computed(() => {
  const base = formData.value.openAt ? new Date(formData.value.openAt) : new Date()
  return new Date(base.getTime() + 3 * 24 * 60 * 60 * 1000)
})

const scheduleStatus = computed(() => {
  const open = formData.value.openAt ? new Date(formData.value.openAt) : null
  const close = formData.value.closeAt ? new Date(formData.value.closeAt) : null
  const now = new Date()

  if (!open && !close) return 'Quiz chỉ mở khi Hội đồng thực hiện thao tác "Mở Quiz".'
  
  if (open && close) {
    if (now < open) return `Quiz dự kiến mở vào ${open.toLocaleTimeString('vi-VN', {hour: '2-digit', minute:'2-digit'})}, ngày ${open.toLocaleDateString('vi-VN')}.`
    if (now > close) return 'Thời gian làm Quiz đã kết thúc.'
    return 'Quiz đang trong thời gian cho phép.'
  }

  if (open && !close) {
    if (now < open) return `Quiz dự kiến mở vào ${open.toLocaleTimeString('vi-VN', {hour: '2-digit', minute:'2-digit'})}, ngày ${open.toLocaleDateString('vi-VN')}.`
    return 'Quiz đã mở và không có thời hạn đóng.'
  }

  if (!open && close) {
    if (now > close) return 'Thời gian làm Quiz đã kết thúc.'
    return `Quiz sẽ đóng vào ${close.toLocaleTimeString('vi-VN', {hour: '2-digit', minute:'2-digit'})}, ngày ${close.toLocaleDateString('vi-VN')}.`
  }

  return ''
})
</script>

<template>
  <div class="bg-white dark:bg-slate-900 rounded-xl border border-slate-200 dark:border-slate-800 shadow-sm relative z-30 mb-6" :class="{'ring-1 ring-red-500': errors['openAt'] || errors['closeAt']}">
    <div class="px-6 py-4 border-b border-slate-100 bg-slate-50 flex items-center justify-between">
      <div>
        <h2 class="text-base font-bold text-slate-800 flex items-center gap-2">
          5. Thời gian mở và đóng
        </h2>
        <p class="text-xs text-slate-500 mt-1">Lên lịch tự động trạng thái Quiz (Bấm chọn ngày trên lịch, ngày kết thúc cách tối thiểu 3 ngày).</p>
      </div>
      <div v-if="errors['openAt'] || errors['closeAt']" class="text-xs font-medium bg-red-100 text-red-700 px-2.5 py-1 rounded-full flex items-center gap-1" role="alert">
        <span class="w-1.5 h-1.5 rounded-full bg-red-600"></span>
        Có lỗi
      </div>
    </div>

    <div class="p-6 space-y-6">
      
      <div class="flex flex-col sm:flex-row gap-6">
        <!-- Mở lúc: Interactive Calendar Picker -->
        <QuizDateTimePicker 
          :model-value="formData.openAt"
          @update:model-value="updateField('openAt', $event)"
          label="Mở lúc"
          :min-date="minOpenDate"
          :disabled="isReadOnly"
          placeholder="Chọn ngày & giờ mở Quiz..."
          :error="errors['openAt']"
          helper-text="* Không thể chọn thời gian mở trong quá khứ."
          preset-type="open"
        />

        <!-- Đóng lúc: Interactive Calendar Picker -->
        <QuizDateTimePicker 
          :model-value="formData.closeAt"
          @update:model-value="updateField('closeAt', $event)"
          label="Đóng lúc"
          :min-date="minCloseDate"
          :base-date="formData.openAt"
          :disabled="isReadOnly"
          placeholder="Chọn ngày & giờ đóng Quiz..."
          :error="errors['closeAt']"
          helper-text="* Thời gian đóng cách thời gian mở tối thiểu 3 ngày."
          preset-type="close"
        />
      </div>

      <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 bg-slate-50 p-4 rounded-lg border border-slate-200">
        <div class="flex items-center gap-3">
          <div class="w-8 h-8 rounded-full bg-blue-100 flex items-center justify-center shrink-0">
            <Calendar class="w-4 h-4 text-blue-600" />
          </div>
          <div>
            <span class="block text-sm font-medium text-slate-800">{{ scheduleStatus }}</span>
            <span class="block text-xs text-slate-500 mt-0.5">Múi giờ: Asia/Ho_Chi_Minh</span>
          </div>
        </div>
        
        <button 
          v-if="(formData.openAt || formData.closeAt) && !isReadOnly"
          @click="clearSchedule"
          class="shrink-0 flex items-center gap-1 px-3 py-1.5 text-sm font-medium text-slate-600 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
        >
          <X class="w-4 h-4" />
          Xóa lịch
        </button>
      </div>

    </div>
  </div>
</template>

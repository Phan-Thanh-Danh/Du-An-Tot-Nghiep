<script setup lang="ts">
import { computed } from 'vue'
import { QuizFormData } from '@/types/content-council/quizForm'
import { X, Calendar } from 'lucide-vue-next'

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

// Convert ISO string to format YYYY-MM-DDThh:mm for datetime-local input
const formatForInput = (isoString: string | null) => {
  if (!isoString) return ''
  const d = new Date(isoString)
  if (isNaN(d.getTime())) return ''
  // Format keeping local timezone
  const year = d.getFullYear()
  const month = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  const hours = String(d.getHours()).padStart(2, '0')
  const minutes = String(d.getMinutes()).padStart(2, '0')
  return `${year}-${month}-${day}T${hours}:${minutes}`
}

// Convert input format back to ISO string
const formatToISO = (value: string) => {
  if (!value) return null
  const d = new Date(value)
  if (isNaN(d.getTime())) return null
  return d.toISOString()
}

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
  <div class="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden mb-6" :class="{'ring-1 ring-red-500': errors['closeAt']}">
    <div class="px-6 py-4 border-b border-slate-100 bg-slate-50 flex items-center justify-between">
      <div>
        <h2 class="text-base font-bold text-slate-800 flex items-center gap-2">
          5. Thời gian mở và đóng
        </h2>
        <p class="text-xs text-slate-500 mt-1">Lên lịch tự động trạng thái Quiz.</p>
      </div>
      <div v-if="errors['closeAt']" class="text-xs font-medium bg-red-100 text-red-700 px-2.5 py-1 rounded-full flex items-center gap-1" role="alert">
        <span class="w-1.5 h-1.5 rounded-full bg-red-600"></span>
        Có lỗi
      </div>
    </div>

    <div class="p-6 space-y-6">
      
      <div class="flex flex-col sm:flex-row gap-6">
        <!-- Mở lúc -->
        <div class="flex-1">
          <label class="block text-sm font-medium text-slate-700 mb-1.5">Mở lúc</label>
          <input 
            type="datetime-local" 
            :value="formatForInput(formData.openAt)"
            @change="updateField('openAt', formatToISO(($event.target as HTMLInputElement).value))"
            :disabled="isReadOnly"
            class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:outline-none transition-colors"
            :class="[
              isReadOnly ? 'bg-slate-50 text-slate-500 cursor-not-allowed border-slate-200' : 'bg-white text-slate-700 border-slate-300 focus:border-blue-500 focus:ring-blue-500/20'
            ]"
          />
        </div>

        <!-- Đóng lúc -->
        <div class="flex-1">
          <label class="block text-sm font-medium text-slate-700 mb-1.5">Đóng lúc</label>
          <input 
            type="datetime-local" 
            :value="formatForInput(formData.closeAt)"
            @change="updateField('closeAt', formatToISO(($event.target as HTMLInputElement).value))"
            :disabled="isReadOnly"
            class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:outline-none transition-colors"
            :class="[
              errors['closeAt'] ? 'border-red-300 focus:ring-red-500' : 'border-slate-300 focus:border-blue-500 focus:ring-blue-500/20',
              isReadOnly ? 'bg-slate-50 text-slate-500 cursor-not-allowed' : 'bg-white text-slate-700'
            ]"
          />
          <p v-if="errors['closeAt']" class="mt-1.5 text-sm text-red-600">{{ errors['closeAt'] }}</p>
        </div>
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

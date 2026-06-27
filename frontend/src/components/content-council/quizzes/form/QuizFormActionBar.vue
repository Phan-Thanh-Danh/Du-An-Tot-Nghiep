<script setup lang="ts">
import { computed } from 'vue'
import { Save, PenTool, X } from 'lucide-vue-next'
import { QuizFormMode } from '@/types/content-council/quizForm'

const props = defineProps<{
  mode: QuizFormMode
  isDirty: boolean
  isSaving: boolean
  isReadOnly: boolean
}>()

const emit = defineEmits(['cancel', 'save-draft', 'save-and-build'])

const statusText = computed(() => {
  if (props.isReadOnly) return 'Chỉ xem - Không có quyền chỉnh sửa'
  if (props.isSaving) return 'Đang lưu...'
  if (props.isDirty) return 'Có thay đổi chưa lưu'
  return 'Không có thay đổi'
})

const statusClass = computed(() => {
  if (props.isReadOnly) return 'text-slate-500'
  if (props.isSaving) return 'text-blue-600'
  if (props.isDirty) return 'text-amber-600'
  return 'text-slate-500'
})

</script>

<template>
  <div class="fixed bottom-0 left-0 lg:left-64 right-0 bg-white border-t border-slate-200 shadow-[0_-4px_6px_-1px_rgba(0,0,0,0.05)] z-40 transition-all">
    <div class="max-w-7xl mx-auto w-full px-6 py-4 flex flex-col sm:flex-row items-center justify-between gap-4">
      
      <!-- Status -->
      <div class="flex items-center gap-2">
        <span class="relative flex h-3 w-3" v-if="isDirty && !isSaving && !isReadOnly">
          <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-amber-400 opacity-75"></span>
          <span class="relative inline-flex rounded-full h-3 w-3 bg-amber-500"></span>
        </span>
        <span class="text-sm font-medium" :class="statusClass">{{ statusText }}</span>
        <span v-if="mode === 'create' && !isReadOnly" class="hidden sm:inline text-xs text-slate-400 ml-2 border-l border-slate-300 pl-2">
          Dữ liệu sẽ được đặt lại khi tải lại trang
        </span>
      </div>

      <!-- Actions -->
      <div class="flex items-center gap-3 w-full sm:w-auto">
        <button 
          @click="emit('cancel')"
          :disabled="isSaving"
          class="flex-1 sm:flex-none flex justify-center items-center gap-2 px-4 py-2 border border-slate-300 text-slate-700 font-medium rounded-lg hover:bg-slate-50 transition-colors disabled:opacity-50"
        >
          <X class="w-4 h-4" />
          {{ mode === 'create' ? 'Hủy' : 'Hủy thay đổi' }}
        </button>
        
        <button 
          v-if="!isReadOnly"
          @click="emit('save-draft')"
          :disabled="!isDirty || isSaving"
          class="flex-1 sm:flex-none flex justify-center items-center gap-2 px-4 py-2 bg-blue-50 text-blue-700 font-medium rounded-lg hover:bg-blue-100 transition-colors disabled:opacity-50"
        >
          <Save class="w-4 h-4" />
          {{ mode === 'create' ? 'Lưu bản nháp' : 'Lưu thay đổi' }}
        </button>

        <button 
          v-if="!isReadOnly"
          @click="emit('save-and-build')"
          :disabled="isSaving"
          class="flex-1 sm:flex-none flex justify-center items-center gap-2 px-6 py-2 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition-colors shadow-sm disabled:opacity-50"
        >
          <PenTool class="w-4 h-4" />
          {{ mode === 'create' ? 'Lưu & Xây dựng đề' : 'Lưu & Mở Builder' }}
        </button>
      </div>

    </div>
  </div>
</template>

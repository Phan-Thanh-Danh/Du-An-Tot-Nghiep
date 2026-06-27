<script setup lang="ts">
import { computed } from 'vue'
import { Plus, Eye } from 'lucide-vue-next'
import { QuestionBankItem } from '@/types/content-council/questionBank'

const props = defineProps<{
  item: QuestionBankItem
  isAdded: boolean
  isSelected: boolean
  isReadOnly: boolean
}>()

const emit = defineEmits(['preview', 'add', 'toggle-select'])

const stripHtml = (html: string) => {
  const tmp = document.createElement('DIV')
  tmp.innerHTML = html
  return tmp.textContent || tmp.innerText || ''
}

const isInactive = computed(() => props.item.status === 'inactive')
const isDisabled = computed(() => props.isAdded || isInactive.value || props.isReadOnly)

</script>

<template>
  <div class="flex items-start gap-3 p-4 border-b border-slate-100 hover:bg-slate-50 transition-colors" :class="{'opacity-60 bg-slate-50/50': isInactive, 'bg-blue-50/30': isSelected && !isDisabled}">
    
    <!-- Checkbox -->
    <div class="pt-1">
      <input 
        type="checkbox" 
        :checked="isSelected || isAdded"
        :disabled="isDisabled"
        @change="emit('toggle-select', item.id)"
        class="w-4 h-4 text-blue-600 border-slate-300 rounded focus:ring-blue-500 disabled:opacity-50"
      >
    </div>

    <!-- Content -->
    <div class="flex-1 min-w-0">
      <div class="flex flex-wrap items-center gap-2 mb-1">
        <span class="text-xs font-bold text-slate-500">{{ item.code }}</span>
        
        <span v-if="isAdded" class="text-[10px] uppercase font-bold tracking-wider px-1.5 py-0.5 rounded bg-blue-100 text-blue-700 border border-blue-200">
          Đã có trong Quiz
        </span>
        <span v-else-if="isInactive" class="text-[10px] uppercase font-bold tracking-wider px-1.5 py-0.5 rounded bg-red-100 text-red-700">
          Vô hiệu hóa
        </span>

        <span class="text-[10px] uppercase font-bold tracking-wider px-1.5 py-0.5 rounded bg-slate-100 text-slate-600 border border-slate-200">
          {{ item.type === 'multiple_choice' ? 'Trắc nghiệm' : 'Tự luận' }}
        </span>
        <span class="text-[10px] uppercase font-bold tracking-wider px-1.5 py-0.5 rounded" :class="item.difficulty === 'easy' ? 'bg-green-100 text-green-700' : item.difficulty === 'medium' ? 'bg-amber-100 text-amber-700' : 'bg-red-100 text-red-700'">
          {{ item.difficulty === 'easy' ? 'Dễ' : item.difficulty === 'medium' ? 'TB' : 'Khó' }}
        </span>
      </div>
      
      <h4 class="text-sm text-slate-800 line-clamp-2 mb-2" :title="stripHtml(item.content)">
        {{ stripHtml(item.content) }}
      </h4>

      <!-- Actions -->
      <div class="flex items-center gap-2">
        <button 
          @click="emit('preview', item.id)"
          class="flex items-center gap-1.5 text-xs font-medium text-slate-600 hover:text-blue-600 transition-colors bg-white border border-slate-200 px-2 py-1 rounded"
        >
          <Eye class="w-3.5 h-3.5" />
          Xem
        </button>
        <button 
          @click="emit('add', item)"
          :disabled="isDisabled"
          class="flex items-center gap-1.5 text-xs font-medium bg-white border border-slate-200 px-2 py-1 rounded transition-colors"
          :class="isDisabled ? 'text-slate-400 opacity-50 cursor-not-allowed' : 'text-slate-700 hover:text-blue-600 hover:border-blue-300'"
        >
          <Plus class="w-3.5 h-3.5" />
          Thêm
        </button>
      </div>
    </div>
  </div>
</template>

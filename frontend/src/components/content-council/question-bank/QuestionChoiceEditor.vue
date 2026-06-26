<script setup lang="ts">
import { ref, watch } from 'vue'
import { QuestionChoice } from '@/types/content-council/questionBank'
import { Plus, Trash2, GripVertical } from 'lucide-vue-next'

const props = defineProps<{
  choices: QuestionChoice[]
  correctAnswerIds: string[]
  selectionType: 'single' | 'multiple'
}>()

const emit = defineEmits(['update:choices', 'update:correctAnswerIds'])

const generateId = () => Math.random().toString(36).substr(2, 9)

const addChoice = () => {
  const newChoices = [...props.choices, { id: generateId(), content: '' }]
  emit('update:choices', newChoices)
}

const removeChoice = (index: number) => {
  if (props.choices.length <= 2) {
    alert('Một câu hỏi trắc nghiệm phải có ít nhất 2 lựa chọn.')
    return
  }
  const choiceToRemove = props.choices[index]
  
  if (props.correctAnswerIds.includes(choiceToRemove.id)) {
    if (!confirm('Lựa chọn này đang là đáp án đúng. Bạn có chắc muốn xóa?')) {
      return
    }
    const newCorrectIds = props.correctAnswerIds.filter(id => id !== choiceToRemove.id)
    emit('update:correctAnswerIds', newCorrectIds)
  }

  const newChoices = [...props.choices]
  newChoices.splice(index, 1)
  emit('update:choices', newChoices)
}

const toggleCorrectAnswer = (id: string) => {
  if (props.selectionType === 'single') {
    emit('update:correctAnswerIds', [id])
  } else {
    const ids = [...props.correctAnswerIds]
    const idx = ids.indexOf(id)
    if (idx > -1) ids.splice(idx, 1)
    else ids.push(id)
    emit('update:correctAnswerIds', ids)
  }
}

const updateChoiceContent = (index: number, val: string) => {
  const newChoices = [...props.choices]
  newChoices[index].content = val
  emit('update:choices', newChoices)
}

const getLabel = (index: number) => String.fromCharCode(65 + index)
</script>

<template>
  <div class="space-y-4">
    <div class="flex items-center justify-between">
      <label class="block text-sm font-medium text-slate-700">Các lựa chọn <span class="text-red-500">*</span></label>
      <span class="text-xs text-slate-500">Đánh dấu vào ô tròn/vuông để chọn đáp án đúng</span>
    </div>

    <div class="space-y-3">
      <div 
        v-for="(choice, index) in choices" 
        :key="choice.id"
        class="flex items-start gap-3 group relative"
      >
        <div class="pt-2.5 text-slate-400 cursor-move hover:text-slate-600 transition-colors">
          <GripVertical class="w-4 h-4" />
        </div>

        <div class="pt-2 text-sm font-bold text-slate-500 w-5 shrink-0 text-center">
          {{ getLabel(index) }}
        </div>

        <div class="flex-1 relative">
          <input 
            :value="choice.content"
            @input="updateChoiceContent(index, ($event.target as HTMLInputElement).value)"
            type="text" 
            class="w-full pl-3 pr-10 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all text-sm"
            :class="correctAnswerIds.includes(choice.id) ? 'border-green-300 bg-green-50/30 ring-1 ring-green-300' : 'border-slate-300 bg-white'"
            placeholder="Nhập nội dung lựa chọn..."
          >
          
          <!-- Radio/Checkbox -->
          <div class="absolute right-3 top-1/2 -translate-y-1/2 flex items-center">
            <input 
              v-if="selectionType === 'single'"
              type="radio" 
              :name="'correct_answer_single'"
              :checked="correctAnswerIds.includes(choice.id)"
              @change="toggleCorrectAnswer(choice.id)"
              class="w-4 h-4 text-green-600 focus:ring-green-500 cursor-pointer"
            >
            <input 
              v-else
              type="checkbox"
              :checked="correctAnswerIds.includes(choice.id)"
              @change="toggleCorrectAnswer(choice.id)"
              class="w-4 h-4 text-green-600 rounded focus:ring-green-500 cursor-pointer"
            >
          </div>
        </div>

        <button 
          @click="removeChoice(index)"
          class="pt-2 text-slate-400 hover:text-red-500 transition-colors opacity-0 group-hover:opacity-100 focus:opacity-100"
          title="Xóa lựa chọn"
        >
          <Trash2 class="w-5 h-5" />
        </button>
      </div>
    </div>

    <button 
      @click="addChoice"
      v-if="choices.length < 10"
      type="button"
      class="flex items-center gap-2 text-sm font-medium text-blue-600 hover:text-blue-700 hover:bg-blue-50 px-3 py-2 rounded-lg transition-colors ml-10 mt-2 border border-dashed border-blue-200"
    >
      <Plus class="w-4 h-4" /> Thêm lựa chọn
    </button>
  </div>
</template>

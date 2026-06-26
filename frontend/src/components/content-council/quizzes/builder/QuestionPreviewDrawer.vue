<script setup lang="ts">
import { ref, computed } from 'vue'
import { X, CheckCircle, Lock } from 'lucide-vue-next'
import { QuestionBankItem } from '@/types/content-council/questionBank'

const props = defineProps<{
  isOpen: boolean
  question: QuestionBankItem | null
}>()

const emit = defineEmits(['close'])

const showCorrectAnswer = ref(false)

const handleClose = () => {
  showCorrectAnswer.value = false
  emit('close')
}

// Helpers
const isMultipleChoice = computed(() => props.question?.type === 'multiple_choice')
const isEssay = computed(() => props.question?.type === 'essay')

const isCorrectChoice = (choiceId: string) => {
  if (!props.question?.correctAnswerIds) return false
  return props.question.correctAnswerIds.includes(choiceId)
}

</script>

<template>
  <div v-if="isOpen && question">
    <!-- Overlay -->
    <div 
      class="fixed inset-0 bg-slate-900/20 backdrop-blur-sm z-40 transition-opacity"
      @click="handleClose"
    ></div>

    <!-- Drawer -->
    <div class="fixed inset-y-0 right-0 z-50 w-full md:w-[600px] bg-white shadow-2xl flex flex-col animate-slide-left">
      <!-- Header -->
      <div class="px-6 py-4 border-b border-slate-200 bg-slate-50 flex items-center justify-between shrink-0">
        <h2 class="text-lg font-bold text-slate-800 flex items-center gap-2">
          Chi tiết câu hỏi
          <span class="text-sm font-normal text-slate-500 bg-slate-200 px-2 py-0.5 rounded">{{ question.code }}</span>
        </h2>
        <button 
          @click="handleClose"
          class="p-2 text-slate-400 hover:text-slate-600 hover:bg-slate-200 rounded-full transition-colors"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Body -->
      <div class="flex-1 overflow-y-auto p-6 space-y-6">
        
        <!-- Metadata -->
        <div class="flex flex-wrap gap-2 mb-2">
          <span class="text-xs uppercase font-bold tracking-wider px-2 py-1 rounded bg-slate-100 text-slate-600 border border-slate-200">
            {{ isMultipleChoice ? 'Trắc nghiệm' : 'Tự luận' }}
          </span>
          <span v-if="isMultipleChoice" class="text-xs uppercase font-bold tracking-wider px-2 py-1 rounded bg-slate-100 text-slate-600 border border-slate-200">
            {{ question.selectionType === 'single' ? 'Chọn 1' : 'Chọn nhiều' }}
          </span>
          <span class="text-xs uppercase font-bold tracking-wider px-2 py-1 rounded" :class="question.difficulty === 'easy' ? 'bg-green-100 text-green-700' : question.difficulty === 'medium' ? 'bg-amber-100 text-amber-700' : 'bg-red-100 text-red-700'">
            {{ question.difficulty === 'easy' ? 'Dễ' : question.difficulty === 'medium' ? 'TB' : 'Khó' }}
          </span>
          <span v-if="question.status === 'inactive'" class="text-xs uppercase font-bold tracking-wider px-2 py-1 rounded bg-red-100 text-red-700 border border-red-200">
            Vô hiệu hóa
          </span>
        </div>

        <!-- Content -->
        <div class="prose prose-sm max-w-none prose-slate">
          <h3 class="text-slate-500 text-xs font-bold uppercase tracking-wider mb-2">Nội dung câu hỏi</h3>
          <div class="p-4 bg-slate-50 border border-slate-200 rounded-xl" v-html="question.content"></div>
        </div>

        <!-- Choices (MCQ) -->
        <div v-if="isMultipleChoice && question.choices" class="space-y-3">
          <div class="flex items-center justify-between mb-2">
            <h3 class="text-slate-500 text-xs font-bold uppercase tracking-wider">Các lựa chọn</h3>
            <label class="flex items-center gap-2 cursor-pointer">
              <span class="text-xs font-medium" :class="showCorrectAnswer ? 'text-green-600' : 'text-slate-500'">Hiển thị đáp án đúng</span>
              <div class="relative">
                <input type="checkbox" v-model="showCorrectAnswer" class="sr-only peer">
                <div class="w-9 h-5 bg-slate-200 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:bg-green-500"></div>
              </div>
            </label>
          </div>
          
          <div 
            v-for="(choice, index) in question.choices" 
            :key="choice.id"
            class="p-3 border rounded-lg flex items-start gap-3 transition-colors"
            :class="showCorrectAnswer && isCorrectChoice(choice.id) ? 'bg-green-50 border-green-200' : 'bg-white border-slate-200'"
          >
            <div class="shrink-0 mt-0.5">
              <CheckCircle v-if="showCorrectAnswer && isCorrectChoice(choice.id)" class="w-5 h-5 text-green-500" />
              <div v-else class="w-5 h-5 rounded-full border border-slate-300 flex items-center justify-center text-xs text-slate-400 font-bold bg-slate-50">
                {{ String.fromCharCode(65 + index) }}
              </div>
            </div>
            <div class="flex-1 text-sm text-slate-700 leading-relaxed">{{ choice.content }}</div>
          </div>
        </div>

        <!-- Essay Sample -->
        <div v-if="isEssay && question.sampleAnswer">
          <h3 class="text-slate-500 text-xs font-bold uppercase tracking-wider mb-2">Đáp án mẫu / Tiêu chí chấm</h3>
          <div class="p-4 bg-blue-50 border border-blue-100 rounded-xl text-sm text-slate-700 whitespace-pre-wrap">
            {{ question.sampleAnswer }}
          </div>
        </div>

        <!-- Explanation -->
        <div v-if="showCorrectAnswer && question.answerExplanation" class="mt-6">
          <h3 class="text-slate-500 text-xs font-bold uppercase tracking-wider mb-2">Giải thích đáp án</h3>
          <div class="p-4 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-700">
            {{ question.answerExplanation }}
          </div>
        </div>
        
        <div class="flex items-center gap-2 p-3 bg-slate-50 border border-slate-200 rounded-lg mt-6">
          <Lock class="w-4 h-4 text-slate-400" />
          <span class="text-xs text-slate-500">Chế độ xem trước không cho phép chỉnh sửa nội dung câu hỏi.</span>
        </div>

      </div>
    </div>
  </div>
</template>

<style scoped>
.animate-slide-left {
  animation: slideLeft 0.3s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

@keyframes slideLeft {
  from {
    transform: translateX(100%);
  }
  to {
    transform: translateX(0);
  }
}
</style>

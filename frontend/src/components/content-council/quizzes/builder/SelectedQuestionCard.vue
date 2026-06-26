<script setup lang="ts">
import { ref } from 'vue'
import { MoreVertical, GripVertical, Eye, ArrowUp, ArrowDown, Trash2 } from 'lucide-vue-next'
import { QuizBuilderQuestion } from '@/types/content-council/quiz'
import QuestionScoreEditor from './QuestionScoreEditor.vue'

const props = defineProps<{
  question: QuizBuilderQuestion
  isFirst: boolean
  isLast: boolean
  isReadOnly: boolean
}>()

const emit = defineEmits(['preview', 'update-score', 'move-up', 'move-down', 'remove'])

const showMenu = ref(false)
const showScoreEditor = ref(false)

const handlePreview = () => {
  showMenu.value = false
  emit('preview', props.question.questionId)
}

const handleRemove = () => {
  showMenu.value = false
  emit('remove', props.question.questionId)
}

const toggleScoreEditor = () => {
  if (props.isReadOnly) return
  showMenu.value = false
  showScoreEditor.value = !showScoreEditor.value
}

const handleScoreSaved = (newScore: number) => {
  emit('update-score', props.question.questionId, newScore)
  showScoreEditor.value = false
}

// Strip HTML for short preview
const stripHtml = (html: string) => {
  const tmp = document.createElement('DIV')
  tmp.innerHTML = html
  return tmp.textContent || tmp.innerText || ''
}
</script>

<template>
  <div class="group relative flex items-stretch bg-white border border-slate-200 rounded-xl shadow-sm hover:border-blue-300 hover:shadow-md transition-all duration-200" :class="{'opacity-60 grayscale': question.status === 'inactive'}">
    
    <!-- Drag Handle -->
    <div class="w-10 bg-slate-50 border-r border-slate-200 rounded-l-xl flex flex-col items-center justify-center shrink-0 cursor-grab active:cursor-grabbing text-slate-400 group-hover:text-blue-500" :class="{'cursor-not-allowed': isReadOnly}">
      <GripVertical class="w-5 h-5" />
      <span class="text-xs font-bold mt-1" :class="question.status === 'inactive' ? 'text-red-500' : 'text-slate-500'">{{ question.order }}</span>
    </div>

    <!-- Content Area -->
    <div class="flex-1 p-4 min-w-0 flex flex-col">
      <div class="flex items-start justify-between gap-4 mb-2">
        <div class="flex-1 min-w-0">
          <div class="flex items-center gap-2 mb-1">
            <span class="text-xs font-bold text-slate-500">{{ question.questionCode }}</span>
            <span v-if="question.status === 'inactive'" class="text-[10px] uppercase font-bold tracking-wider px-1.5 py-0.5 rounded bg-red-100 text-red-700">Vô hiệu hóa</span>
            <span class="text-[10px] uppercase font-bold tracking-wider px-1.5 py-0.5 rounded bg-slate-100 text-slate-600 border border-slate-200">
              {{ question.questionType === 'multiple_choice' ? 'Trắc nghiệm' : 'Tự luận' }}
            </span>
            <span v-if="question.questionType === 'multiple_choice'" class="text-[10px] uppercase font-bold tracking-wider px-1.5 py-0.5 rounded bg-slate-100 text-slate-600 border border-slate-200">
              {{ question.selectionType === 'single' ? 'Chọn 1' : 'Chọn nhiều' }}
            </span>
            <span class="text-[10px] uppercase font-bold tracking-wider px-1.5 py-0.5 rounded" :class="question.difficulty === 'easy' ? 'bg-green-100 text-green-700' : question.difficulty === 'medium' ? 'bg-amber-100 text-amber-700' : 'bg-red-100 text-red-700'">
              {{ question.difficulty === 'easy' ? 'Dễ' : question.difficulty === 'medium' ? 'TB' : 'Khó' }}
            </span>
          </div>
          
          <h4 class="text-sm font-medium text-slate-800 line-clamp-2" :title="stripHtml(question.questionContent)">
            {{ stripHtml(question.questionContent) }}
          </h4>
        </div>

        <!-- Score Badge -->
        <div 
          @click="toggleScoreEditor"
          class="shrink-0 flex items-baseline gap-1 px-3 py-1.5 bg-blue-50 border border-blue-100 rounded-lg group-hover:bg-blue-100 group-hover:border-blue-200 transition-colors"
          :class="isReadOnly ? 'cursor-default' : 'cursor-pointer'"
          title="Nhấn để sửa điểm"
        >
          <span class="text-lg font-bold text-blue-700">{{ Number(question.score.toFixed(2)) }}</span>
          <span class="text-xs font-medium text-blue-600">điểm</span>
        </div>
      </div>
      
      <!-- Score Editor Popover (Inline) -->
      <div v-if="showScoreEditor && !isReadOnly" class="mt-2 mb-2 p-3 bg-slate-50 border border-slate-200 rounded-lg">
        <QuestionScoreEditor 
          :initial-score="question.score"
          @save="handleScoreSaved"
          @cancel="showScoreEditor = false"
        />
      </div>

    </div>

    <!-- Actions Sidebar -->
    <div class="w-12 border-l border-slate-100 flex flex-col items-stretch shrink-0">
      <button 
        @click="emit('move-up', question.order - 1)" 
        :disabled="isFirst || isReadOnly" 
        class="flex-1 flex items-center justify-center text-slate-400 hover:text-blue-600 hover:bg-blue-50 transition-colors disabled:opacity-30 disabled:hover:bg-transparent disabled:hover:text-slate-400 rounded-tr-xl"
        title="Di chuyển lên"
      >
        <ArrowUp class="w-4 h-4" />
      </button>
      
      <!-- Dropdown Menu relative container -->
      <div class="relative flex-1 flex items-center justify-center" v-click-outside="() => showMenu = false">
        <button 
          @click="showMenu = !showMenu"
          class="w-full h-full flex items-center justify-center text-slate-400 hover:text-slate-700 hover:bg-slate-100 transition-colors"
        >
          <MoreVertical class="w-4 h-4" />
        </button>
        
        <!-- Menu -->
        <div v-if="showMenu" class="absolute right-full top-0 mt-2 mr-2 w-48 bg-white rounded-xl shadow-lg border border-slate-200 py-1.5 z-10">
          <button @click="handlePreview" class="w-full px-4 py-2 text-left text-sm text-slate-700 hover:bg-slate-50 flex items-center gap-2">
            <Eye class="w-4 h-4 text-slate-400" />
            Xem trước
          </button>
          <button v-if="!isReadOnly" @click="toggleScoreEditor" class="w-full px-4 py-2 text-left text-sm text-slate-700 hover:bg-slate-50 flex items-center gap-2">
            <span class="w-4 h-4 text-slate-400 font-bold text-center leading-4 text-[10px]">#</span>
            Sửa điểm
          </button>
          <hr class="my-1 border-slate-100" v-if="!isReadOnly" />
          <button v-if="!isReadOnly" @click="handleRemove" class="w-full px-4 py-2 text-left text-sm text-red-600 hover:bg-red-50 flex items-center gap-2 font-medium">
            <Trash2 class="w-4 h-4" />
            Gỡ khỏi Quiz
          </button>
        </div>
      </div>

      <button 
        @click="emit('move-down', question.order - 1)" 
        :disabled="isLast || isReadOnly" 
        class="flex-1 flex items-center justify-center text-slate-400 hover:text-blue-600 hover:bg-blue-50 transition-colors disabled:opacity-30 disabled:hover:bg-transparent disabled:hover:text-slate-400 rounded-br-xl"
        title="Di chuyển xuống"
      >
        <ArrowDown class="w-4 h-4" />
      </button>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { QuestionBankItem } from '@/types/content-council/questionBank'
import { QuizBuilderQuestion } from '@/types/content-council/quiz'
import QuestionBankBuilderFilter from './QuestionBankBuilderFilter.vue'
import QuestionBankBuilderItem from './QuestionBankBuilderItem.vue'
import { ChevronLeft, ChevronRight } from 'lucide-vue-next'

const props = defineProps<{
  subjectCode: string
  availableQuestions: QuestionBankItem[]
  selectedQuestionIdsInQuiz: number[] // IDs already in quiz
  selectedQuestionIdsBulk: number[] // IDs selected for bulk add
  isReadOnly: boolean
}>()

const emit = defineEmits([
  'preview', 
  'add-single', 
  'toggle-bulk-select', 
  'select-all-bulk', 
  'clear-bulk-select', 
  'bulk-add-click'
])

// Pagination State
const currentPage = ref(1)
const pageSize = ref(10)

// Filters State
const currentFilters = ref({
  keyword: '',
  questionType: '',
  selectionType: '',
  difficulty: ''
})

const handleFilterChange = (filters: any) => {
  currentFilters.value = filters
  currentPage.value = 1
}

// Filtered List
const filteredQuestions = computed(() => {
  let result = props.availableQuestions

  if (currentFilters.value.keyword) {
    const kw = currentFilters.value.keyword.toLowerCase()
    result = result.filter(q => q.code.toLowerCase().includes(kw) || q.content.toLowerCase().includes(kw))
  }
  if (currentFilters.value.questionType) {
    result = result.filter(q => q.type === currentFilters.value.questionType)
  }
  if (currentFilters.value.selectionType) {
    result = result.filter(q => q.selectionType === currentFilters.value.selectionType)
  }
  if (currentFilters.value.difficulty) {
    result = result.filter(q => q.difficulty === currentFilters.value.difficulty)
  }

  return result
})

// Pagination Logic
const totalItems = computed(() => filteredQuestions.value.length)
const totalPages = computed(() => Math.ceil(totalItems.value / pageSize.value))

const paginatedQuestions = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredQuestions.value.slice(start, end)
})

const validQuestionsForBulk = computed(() => {
  return filteredQuestions.value.filter(q => q.status === 'active' && !props.selectedQuestionIdsInQuiz.includes(q.id))
})

const isAllValidSelected = computed(() => {
  if (validQuestionsForBulk.value.length === 0) return false
  return validQuestionsForBulk.value.every(q => props.selectedQuestionIdsBulk.includes(q.id))
})

const toggleSelectAll = () => {
  if (isAllValidSelected.value) {
    emit('clear-bulk-select')
  } else {
    emit('select-all-bulk', validQuestionsForBulk.value.map(q => q.id))
  }
}

const handlePageSizeChange = (e: Event) => {
  pageSize.value = Number((e.target as HTMLSelectElement).value)
  currentPage.value = 1
}

</script>

<template>
  <div class="bg-white border-l border-slate-200 h-full flex flex-col">
    <!-- Header -->
    <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 shrink-0">
      <h2 class="font-bold text-slate-800 text-lg">Ngân hàng câu hỏi</h2>
      <p class="text-xs text-slate-500 mt-0.5">Chỉ hiển thị câu hỏi thuộc môn {{ subjectCode }}.</p>
    </div>

    <QuestionBankBuilderFilter @filter-change="handleFilterChange" />

    <!-- List Area -->
    <div class="flex-1 overflow-y-auto relative bg-slate-50">
      
      <!-- Select All Bar -->
      <div v-if="filteredQuestions.length > 0 && !isReadOnly" class="sticky top-0 z-10 bg-white border-b border-slate-100 px-4 py-2 flex items-center justify-between shadow-sm">
        <label class="flex items-center gap-2 cursor-pointer">
          <input 
            type="checkbox" 
            :checked="isAllValidSelected"
            :disabled="validQuestionsForBulk.length === 0"
            @change="toggleSelectAll"
            class="w-4 h-4 text-blue-600 border-slate-300 rounded focus:ring-blue-500 disabled:opacity-50"
          >
          <span class="text-sm font-medium text-slate-700">Chọn tất cả (hiển thị)</span>
        </label>
        <span class="text-xs text-slate-500">{{ validQuestionsForBulk.length }} hợp lệ</span>
      </div>

      <div v-if="filteredQuestions.length === 0" class="p-8 text-center text-slate-500">
        <p class="text-sm">Không tìm thấy câu hỏi nào phù hợp.</p>
      </div>

      <div v-else>
        <QuestionBankBuilderItem
          v-for="item in paginatedQuestions"
          :key="item.id"
          :item="item"
          :is-added="selectedQuestionIdsInQuiz.includes(item.id)"
          :is-selected="selectedQuestionIdsBulk.includes(item.id)"
          :is-read-only="isReadOnly"
          @preview="id => emit('preview', id)"
          @add="i => emit('add-single', i)"
          @toggle-select="id => emit('toggle-bulk-select', id)"
        />
      </div>

    </div>

    <!-- Bulk Action Bar -->
    <div v-if="selectedQuestionIdsBulk.length > 0 && !isReadOnly" class="bg-blue-50 border-t border-blue-200 p-3 shrink-0 flex items-center justify-between">
      <div class="text-sm font-bold text-blue-800">
        Đã chọn {{ selectedQuestionIdsBulk.length }} câu
      </div>
      <div class="flex gap-2">
        <button 
          @click="emit('clear-bulk-select')"
          class="px-3 py-1.5 text-xs font-medium text-blue-700 hover:bg-blue-100 rounded-md transition-colors"
        >
          Bỏ chọn
        </button>
        <button 
          @click="emit('bulk-add-click')"
          class="px-3 py-1.5 text-xs font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-md shadow-sm transition-colors"
        >
          Thêm vào Quiz
        </button>
      </div>
    </div>

    <!-- Pagination Footer -->
    <div class="border-t border-slate-200 bg-white p-3 shrink-0 flex items-center justify-between">
      <div class="flex items-center gap-2">
        <select 
          :value="pageSize"
          @change="handlePageSizeChange"
          class="border border-slate-300 rounded px-2 py-1 text-xs focus:ring-2 focus:ring-blue-500 focus:outline-none bg-white"
        >
          <option :value="10">10 / trang</option>
          <option :value="20">20 / trang</option>
          <option :value="50">50 / trang</option>
        </select>
        <span class="text-xs text-slate-500 hidden sm:inline">Tổng: {{ totalItems }}</span>
      </div>

      <div class="flex items-center gap-1">
        <button 
          @click="currentPage--" 
          :disabled="currentPage === 1"
          class="p-1 border border-slate-300 rounded text-slate-600 hover:bg-slate-50 disabled:opacity-50 disabled:bg-slate-100 transition-colors"
        >
          <ChevronLeft class="w-4 h-4" />
        </button>
        <span class="text-xs font-medium text-slate-700 px-2">{{ currentPage }} / {{ totalPages || 1 }}</span>
        <button 
          @click="currentPage++" 
          :disabled="currentPage >= totalPages"
          class="p-1 border border-slate-300 rounded text-slate-600 hover:bg-slate-50 disabled:opacity-50 disabled:bg-slate-100 transition-colors"
        >
          <ChevronRight class="w-4 h-4" />
        </button>
      </div>
    </div>

  </div>
</template>

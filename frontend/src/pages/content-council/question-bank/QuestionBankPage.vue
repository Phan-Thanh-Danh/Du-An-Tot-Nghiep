<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { QuestionBankItem } from '@/types/content-council/questionBank'
import { useQuestionStore } from '@/stores/content-council/questionStore'
import { Download, Upload, Plus } from 'lucide-vue-next'

import QuestionFilterBar from '@/components/content-council/question-bank/QuestionFilterBar.vue'
import QuestionTable from '@/components/content-council/question-bank/QuestionTable.vue'
import QuestionDetailDrawer from '@/components/content-council/question-bank/QuestionDetailDrawer.vue'
import QuestionFormDrawer from '@/components/content-council/question-bank/QuestionFormDrawer.vue'
import DeleteQuestionDialog from '@/components/content-council/question-bank/DeleteQuestionDialog.vue'
import ImportQuestionModal from '@/components/content-council/question-bank/ImportQuestionModal.vue'

const router = useRouter()
const route = useRoute()

// --- State ---
const questionStore = useQuestionStore()
const selectedIds = ref<number[]>([])

const filters = ref({
  keyword: '',
  subjectId: 'all',
  questionType: 'all',
  selectionType: 'all',
  difficulty: 'all',
  status: 'all'
})

const pagination = ref({
  currentPage: 1,
  pageSize: 20
})

// --- UI State ---
const isDetailOpen = ref(false)
const isFormOpen = ref(false)
const isDeleteOpen = ref(false)
const isImportOpen = ref(false)

const formMode = ref<'create' | 'edit' | 'duplicate'>('create')
const activeQuestion = ref<QuestionBankItem | null>(null)
const toastMessage = ref('')
const toastTimeout = ref<any>(null)

// --- Initialization ---
onMounted(() => {
  // Parse URL queries
  const q = route.query
  if (q.keyword) filters.value.keyword = String(q.keyword)
  if (q.subjectId) filters.value.subjectId = String(q.subjectId)
  if (q.questionType) filters.value.questionType = String(q.questionType)
  if (q.selectionType) filters.value.selectionType = String(q.selectionType)
  if (q.difficulty) filters.value.difficulty = String(q.difficulty)
  if (q.status) filters.value.status = String(q.status)
  if (q.page) pagination.value.currentPage = Number(q.page) || 1
  if (q.pageSize) pagination.value.pageSize = Number(q.pageSize) || 20
  questionStore.init()
})

// --- Computed ---
const filteredQuestions = computed(() => {
  let result = [...questionStore.questions]
  
  const kw = filters.value.keyword.toLowerCase().trim()
  if (kw) {
    result = result.filter(q => 
      q.code.toLowerCase().includes(kw) || 
      q.content.toLowerCase().includes(kw)
    )
  }
  
  if (filters.value.subjectId !== 'all') {
    result = result.filter(q => q.subjectId.toString() === filters.value.subjectId)
  }
  
  if (filters.value.questionType !== 'all') {
    result = result.filter(q => q.type === filters.value.questionType)
  }
  
  if (filters.value.selectionType !== 'all' && filters.value.questionType !== 'essay') {
    result = result.filter(q => q.selectionType === filters.value.selectionType)
  }
  
  if (filters.value.difficulty !== 'all') {
    result = result.filter(q => q.difficulty === filters.value.difficulty)
  }
  
  if (filters.value.status !== 'all') {
    result = result.filter(q => q.status === filters.value.status)
  }
  
  // Sort by ID desc (newest first)
  return result.sort((a, b) => b.id - a.id)
})

const totalItems = computed(() => filteredQuestions.value.length)
const totalPages = computed(() => Math.ceil(totalItems.value / pagination.value.pageSize) || 1)

const paginatedQuestions = computed(() => {
  const start = (pagination.value.currentPage - 1) * pagination.value.pageSize
  const end = start + pagination.value.pageSize
  return filteredQuestions.value.slice(start, end)
})

// --- Watchers ---
watch([filters, pagination], () => {
  // Sync to URL
  const query: any = {}
  if (filters.value.keyword) query.keyword = filters.value.keyword
  if (filters.value.subjectId !== 'all') query.subjectId = filters.value.subjectId
  if (filters.value.questionType !== 'all') query.questionType = filters.value.questionType
  if (filters.value.selectionType !== 'all' && filters.value.questionType !== 'essay') query.selectionType = filters.value.selectionType
  if (filters.value.difficulty !== 'all') query.difficulty = filters.value.difficulty
  if (filters.value.status !== 'all') query.status = filters.value.status
  if (pagination.value.currentPage > 1) query.page = pagination.value.currentPage
  if (pagination.value.pageSize !== 20) query.pageSize = pagination.value.pageSize

  router.replace({ query })
}, { deep: true })

watch(filters, () => {
  // Reset page to 1 when filter changes
  pagination.value.currentPage = 1
  selectedIds.value = []
}, { deep: true })

watch(totalItems, (newTotal) => {
  // Adjust current page if items are deleted and we are out of bounds
  if (pagination.value.currentPage > totalPages.value && totalPages.value > 0) {
    pagination.value.currentPage = totalPages.value
  }
})

// --- Actions ---
const resetFilters = () => {
  filters.value = {
    keyword: '',
    subjectId: 'all',
    questionType: 'all',
    selectionType: 'all',
    difficulty: 'all',
    status: 'all'
  }
}

const showToast = (msg: string) => {
  toastMessage.value = msg
  if (toastTimeout.value) clearTimeout(toastTimeout.value)
  toastTimeout.value = setTimeout(() => {
    toastMessage.value = ''
  }, 3000)
}

const handleCreate = () => {
  formMode.value = 'create'
  activeQuestion.value = null
  isFormOpen.value = true
}

const handleEdit = (q: QuestionBankItem) => {
  formMode.value = 'edit'
  activeQuestion.value = q
  isFormOpen.value = true
}

const handleDuplicate = (q: QuestionBankItem) => {
  formMode.value = 'duplicate'
  activeQuestion.value = q
  isFormOpen.value = true
}

const handleView = (q: QuestionBankItem) => {
  activeQuestion.value = q
  isDetailOpen.value = true
}

const handleDeleteRequest = (q: QuestionBankItem) => {
  activeQuestion.value = q
  isDeleteOpen.value = true
}

const handleToggleStatus = (q: QuestionBankItem) => {
  if (!confirm(`Bạn có chắc muốn ${q.status === 'active' ? 'Vô hiệu hóa' : 'Kích hoạt'} câu hỏi này?`)) return
  questionStore.toggleStatus(q.id)
  showToast(`Đã ${q.status === 'active' ? 'Vô hiệu hóa' : 'Kích hoạt'} câu hỏi thành công.`)
}

const executeDelete = (q: QuestionBankItem) => {
  questionStore.deleteQuestion(q.id)
  selectedIds.value = selectedIds.value.filter(id => id !== q.id)
  isDeleteOpen.value = false
  isDetailOpen.value = false
  showToast('Đã xóa câu hỏi thành công.')
}

const executeSave = (formData: any) => {
  if (formMode.value === 'create' || formMode.value === 'duplicate') {
    const newQuestion = {
      ...formData,
      id: Math.max(...questionStore.questions.map(q => q.id), 0) + 1,
      code: `Q-${formData.subjectCode}-MOCK-${Math.floor(Math.random() * 1000)}`,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      usageCount: 0
    }
    questionStore.addQuestion(newQuestion)
    showToast(formMode.value === 'create' ? 'Đã tạo câu hỏi thành công.' : 'Đã nhân bản câu hỏi thành công.')
  } else if (formMode.value === 'edit') {
    questionStore.updateQuestion(formData.id, {
      ...formData,
      updatedAt: new Date().toISOString()
    })
  }
  isFormOpen.value = false
}

// Bulk
const handleBulkActivate = () => {
  if (!confirm(`Kích hoạt ${selectedIds.value.length} câu hỏi đã chọn?`)) return
  allQuestions.value.forEach(q => {
    if (selectedIds.value.includes(q.id)) q.status = 'active'
  })
  selectedIds.value = []
  showToast('Đã kích hoạt các câu hỏi được chọn.')
}

const handleBulkDeactivate = () => {
  if (!confirm(`Vô hiệu hóa ${selectedIds.value.length} câu hỏi đã chọn?`)) return
  allQuestions.value.forEach(q => {
    if (selectedIds.value.includes(q.id)) q.status = 'inactive'
  })
  selectedIds.value = []
  showToast('Đã vô hiệu hóa các câu hỏi được chọn.')
}

const handleBulkDelete = () => {
  const selectedQuestions = allQuestions.value.filter(q => selectedIds.value.includes(q.id))
  const invalidToDelete = selectedQuestions.filter(q => q.usageCount > 0)
  
  if (invalidToDelete.length > 0) {
    alert(`Không thể xóa ${invalidToDelete.length} câu hỏi vì đang được sử dụng trong Quiz. Vui lòng bỏ chọn chúng hoặc dùng chức năng Vô hiệu hóa.`)
    return
  }

  if (!confirm(`Bạn có chắc muốn xóa vĩnh viễn ${selectedIds.value.length} câu hỏi đã chọn?`)) return
  allQuestions.value = allQuestions.value.filter(q => !selectedIds.value.includes(q.id))
  selectedIds.value = []
  showToast('Đã xóa các câu hỏi được chọn.')
}

const handleImportSuccess = (count: number) => {
  showToast(`Đã mô phỏng import thành công ${count} câu hỏi hợp lệ.`)
}

const downloadTemplate = () => {
  alert('Tính năng tải file mẫu đang trong quá trình phát triển (Chờ Backend).')
}
</script>

<template>
  <div class="h-full flex flex-col bg-slate-50 relative">
    
    <!-- Toast Message -->
    <div 
      v-if="toastMessage" 
      class="fixed bottom-6 right-6 z-50 bg-slate-800 text-white px-4 py-3 rounded-lg shadow-lg flex items-center gap-2 transform transition-all animate-fade-in-up"
    >
      <div class="w-2 h-2 rounded-full bg-green-400"></div>
      <span class="text-sm font-medium">{{ toastMessage }}</span>
    </div>

    <!-- Header Section -->
    <div class="bg-white border-b border-slate-200 px-6 py-6 shrink-0 z-10">
      <div class="max-w-7xl mx-auto flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-bold text-slate-800 mb-1">Ngân hàng câu hỏi</h1>
          <p class="text-sm text-slate-500">Tạo và quản lý câu hỏi dùng chung cho Quiz và đề kiểm tra.</p>
        </div>
        
        <div class="flex items-center gap-3">
          <button @click="downloadTemplate" class="hidden sm:flex px-4 py-2 text-sm font-medium border border-slate-200 rounded-lg text-slate-700 hover:bg-slate-50 transition-colors items-center gap-2">
            <Download class="w-4 h-4" /> <span>Tải file mẫu</span>
          </button>
          
          <button @click="isImportOpen = true" class="px-4 py-2 text-sm font-medium border border-slate-200 rounded-lg text-slate-700 hover:bg-slate-50 transition-colors flex items-center gap-2">
            <Upload class="w-4 h-4" /> <span>Import Excel</span>
          </button>

          <button @click="handleCreate" class="px-4 py-2 text-sm font-medium bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2">
            <Plus class="w-4 h-4" /> <span>Tạo câu hỏi</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Main Content Area -->
    <div class="flex-1 overflow-y-auto p-6">
      <div class="max-w-7xl mx-auto">
        
        <QuestionFilterBar 
          v-model:filters="filters"
          @reset="resetFilters"
        />

        <QuestionTable 
          :questions="paginatedQuestions"
          :totalItems="totalItems"
          v-model:currentPage="pagination.currentPage"
          v-model:pageSize="pagination.pageSize"
          v-model:selectedIds="selectedIds"
          @view="handleView"
          @edit="handleEdit"
          @duplicate="handleDuplicate"
          @delete="handleDeleteRequest"
          @toggleStatus="handleToggleStatus"
          @bulkActivate="handleBulkActivate"
          @bulkDeactivate="handleBulkDeactivate"
          @bulkDelete="handleBulkDelete"
        />

      </div>
    </div>

    <!-- Modals & Drawers -->
    <QuestionDetailDrawer 
      v-model:isOpen="isDetailOpen"
      :questionData="activeQuestion"
      @edit="handleEdit"
      @duplicate="handleDuplicate"
      @delete="handleDeleteRequest"
      @toggleStatus="handleToggleStatus"
    />

    <QuestionFormDrawer 
      v-model:isOpen="isFormOpen"
      :mode="formMode"
      :questionData="activeQuestion"
      @save="executeSave"
    />

    <DeleteQuestionDialog 
      v-model:isOpen="isDeleteOpen"
      :questionData="activeQuestion"
      @confirm="executeDelete"
    />

    <ImportQuestionModal 
      v-model:isOpen="isImportOpen"
      @import="handleImportSuccess"
    />

  </div>
</template>

<style scoped>
.animate-fade-in-up {
  animation: fadeInUp 0.3s ease-out forwards;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>

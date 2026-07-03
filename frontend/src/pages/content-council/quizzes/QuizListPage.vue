<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ContentCouncilQuiz } from '@/types/content-council/quiz'
import { useQuizStore } from '@/stores/content-council/quizStore'
import QuizFilterBar from '@/components/content-council/quizzes/QuizFilterBar.vue'
import QuizTable from '@/components/content-council/quizzes/QuizTable.vue'
import QuizMobileCard from '@/components/content-council/quizzes/QuizMobileCard.vue'
import QuizDetailDrawer from '@/components/content-council/quizzes/QuizDetailDrawer.vue'
import QuizTableSkeleton from '@/components/content-council/quizzes/QuizTableSkeleton.vue'
import QuizEmptyState from '@/components/content-council/quizzes/QuizEmptyState.vue'
import { Plus, X } from 'lucide-vue-next'

const router = useRouter()
const route = useRoute()

// ─── Toast System ───────────────────────────────────────────────────────
const toasts = ref<{ id: number, message: string, type: 'success' | 'error' }[]>([])
let toastId = 0
const showToast = (message: string, type: 'success' | 'error' = 'success') => {
  const id = toastId++
  toasts.value.push({ id, message, type })
  setTimeout(() => {
    toasts.value = toasts.value.filter(t => t.id !== id)
  }, 3000)
}

// ─── State ─────────────────────────────────────────────────────────────
const quizStore = useQuizStore()
const quizzes = computed(() => quizStore.quizzes)
const isLoading = computed(() => quizStore.loading)

const filters = ref({
  keyword: '',
  subjectId: 'all',
  semesterId: 'all',
  status: 'all',
  examType: 'all',
  format: 'all'
})
const sort = ref('updated_desc')
const currentPage = ref(1)
const pageSize = ref(20)

const isDrawerOpen = ref(false)
const selectedQuiz = ref<ContentCouncilQuiz | null>(null)

// ─── Dialog State ───────────────────────────────────────────────────────
const dialogState = ref({
  isOpen: false,
  title: '',
  message: '',
  confirmText: '',
  confirmClass: '',
  actionType: '' as string,
  quiz: null as ContentCouncilQuiz | null,
  warnings: [] as string[],
  errors: [] as string[]
})

const closeDialog = () => {
  dialogState.value.isOpen = false
  dialogState.value.quiz = null
  dialogState.value.warnings = []
  dialogState.value.errors = []
}

// ─── Initialize Data ───────────────────────────────────────────────────
onMounted(() => {
  
  // Parse URL query
  if (route.query.keyword) filters.value.keyword = route.query.keyword as string
  if (route.query.subjectId) filters.value.subjectId = route.query.subjectId as string
  if (route.query.semesterId) filters.value.semesterId = route.query.semesterId as string
  if (route.query.status) filters.value.status = route.query.status as string
  if (route.query.examType) filters.value.examType = route.query.examType as string
  if (route.query.format) filters.value.format = route.query.format as string
  if (route.query.sort) sort.value = route.query.sort as string
  if (route.query.page) currentPage.value = Number(route.query.page)
  if (route.query.pageSize) pageSize.value = Number(route.query.pageSize)

  quizStore.init().finally(() => {
    isLoading.value = false
  })
})

// ─── Sync URL ─────────────────────────────────────────────────────────
watch([filters, sort, currentPage, pageSize], () => {
  const query: any = {}
  if (filters.value.keyword) query.keyword = filters.value.keyword
  if (filters.value.subjectId !== 'all') query.subjectId = filters.value.subjectId
  if (filters.value.semesterId !== 'all') query.semesterId = filters.value.semesterId
  if (filters.value.status !== 'all') query.status = filters.value.status
  if (filters.value.examType !== 'all') query.examType = filters.value.examType
  if (filters.value.format !== 'all') query.format = filters.value.format
  if (sort.value !== 'updated_desc') query.sort = sort.value
  if (currentPage.value !== 1) query.page = currentPage.value
  if (pageSize.value !== 20) query.pageSize = pageSize.value

  router.replace({ query })
}, { deep: true })

watch(filters, () => {
  currentPage.value = 1
}, { deep: true })

const resetFilters = () => {
  filters.value = {
    keyword: '',
    subjectId: 'all',
    semesterId: 'all',
    status: 'all',
    examType: 'all',
    format: 'all'
  }
  sort.value = 'updated_desc'
  currentPage.value = 1
}

// ─── Computed Data ─────────────────────────────────────────────────────
const filteredQuizzes = computed(() => {
  return quizzes.value.filter(q => {
    if (filters.value.keyword) {
      const keyword = filters.value.keyword.toLowerCase()
      const matchTitle = q.title.toLowerCase().includes(keyword)
      const matchCode = q.code.toLowerCase().includes(keyword)
      const matchSubjectCode = q.subjectCode.toLowerCase().includes(keyword)
      const matchSubjectName = q.subjectName.toLowerCase().includes(keyword)
      if (!matchTitle && !matchCode && !matchSubjectCode && !matchSubjectName) return false
    }
    if (filters.value.subjectId !== 'all' && q.subjectId.toString() !== filters.value.subjectId) return false
    if (filters.value.semesterId !== 'all' && q.semesterId?.toString() !== filters.value.semesterId) return false
    if (filters.value.status !== 'all' && q.status !== filters.value.status) return false
    if (filters.value.examType !== 'all' && q.examType !== filters.value.examType) return false
    if (filters.value.format !== 'all' && q.format !== filters.value.format) return false
    return true
  })
})

const sortedQuizzes = computed(() => {
  const sorted = [...filteredQuizzes.value]
  switch (sort.value) {
    case 'created_desc':
      return sorted.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
    case 'title_asc':
      return sorted.sort((a, b) => a.title.localeCompare(b.title))
    case 'title_desc':
      return sorted.sort((a, b) => b.title.localeCompare(a.title))
    case 'question_desc':
      return sorted.sort((a, b) => b.questionCount - a.questionCount)
    case 'duration_desc':
      return sorted.sort((a, b) => b.durationMinutes - a.durationMinutes)
    case 'updated_desc':
    default:
      return sorted.sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
  }
})

const paginatedQuizzes = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return sortedQuizzes.value.slice(start, start + pageSize.value)
})

// ─── Action Handlers ───────────────────────────────────────────────────
const handleAction = ({ type, quiz }: { type: string, quiz: ContentCouncilQuiz }) => {
  if (type === 'view') {
    selectedQuiz.value = quiz
    isDrawerOpen.value = true
  } 
  else if (type === 'edit') {
    router.push({ name: 'content-council-quiz-edit', params: { quizId: quiz.id } })
  } 
  else if (type === 'build') {
    router.push({ name: 'content-council-quiz-builder', params: { quizId: quiz.id } })
  } 
  else if (type === 'duplicate') {
    dialogState.value = {
      isOpen: true,
      title: 'Nhân bản Quiz?',
      message: `Một bản sao mới sẽ được tạo ở trạng thái Bản nháp. Cấu hình Quiz được sao chép nhưng dữ liệu làm bài không được sao chép.\n\nQuiz: ${quiz.title}`,
      confirmText: 'Tạo bản sao',
      confirmClass: 'bg-blue-600 hover:bg-blue-700 text-white',
      actionType: 'duplicate',
      quiz: quiz,
      warnings: [],
      errors: []
    }
  } 
  else if (type === 'publish') {
    // Validate publish
    const errors: string[] = []
    if (quiz.questionCount === 0) errors.push('Quiz chưa có câu hỏi.')
    if (quiz.totalScore <= 0) errors.push('Tổng điểm phải lớn hơn 0.')
    if (quiz.durationMinutes <= 0) errors.push('Thời gian làm bài phải lớn hơn 0.')
    if (!quiz.unlimitedAttempts && (!quiz.maximumAttempts || quiz.maximumAttempts <= 0)) {
      errors.push('Số lượt làm tối đa không hợp lệ.')
    }
    if (quiz.format === 'multiple_choice' && quiz.essayQuestionCount > 0) {
      errors.push('Quiz trắc nghiệm không thể chứa câu tự luận.')
    }

    if (errors.length > 0) {
      dialogState.value = {
        isOpen: true,
        title: 'Chưa thể xuất bản Quiz',
        message: 'Quiz chưa đủ điều kiện để xuất bản. Vui lòng khắc phục các lỗi sau:',
        confirmText: 'Đã hiểu',
        confirmClass: 'bg-slate-200 hover:bg-slate-300 text-slate-800',
        actionType: 'cancel_only',
        quiz: quiz,
        warnings: [],
        errors: errors
      }
    } else {
      dialogState.value = {
        isOpen: true,
        title: 'Xuất bản Quiz?',
        message: 'Sau khi xuất bản, Quiz sẵn sàng để được mở cho học sinh hoặc gắn vào nội dung bài học.',
        confirmText: 'Xuất bản',
        confirmClass: 'bg-blue-600 hover:bg-blue-700 text-white',
        actionType: 'publish',
        quiz: quiz,
        warnings: [],
        errors: []
      }
    }
  }
  else if (type === 'unpublish') {
    if (quiz.status === 'open') {
      showToast('Không thể chuyển về nháp khi Quiz đang mở.', 'error')
      return
    }
    dialogState.value = {
      isOpen: true,
      title: 'Chuyển Quiz về bản nháp?',
      message: 'Quiz sẽ không còn ở trạng thái sẵn sàng mở. Các cấu hình vẫn được giữ nguyên.',
      confirmText: 'Chuyển về bản nháp',
      confirmClass: 'bg-blue-600 hover:bg-blue-700 text-white',
      actionType: 'unpublish',
      quiz: quiz,
      warnings: [],
      errors: []
    }
  }
  else if (type === 'open') {
    dialogState.value = {
      isOpen: true,
      title: 'Mở Quiz?',
      message: 'Sau khi mở, học sinh có thể bắt đầu làm bài trong thời gian cho phép.',
      confirmText: 'Mở Quiz',
      confirmClass: 'bg-green-600 hover:bg-green-700 text-white',
      actionType: 'open',
      quiz: quiz,
      warnings: [],
      errors: []
    }
  }
  else if (type === 'close') {
    dialogState.value = {
      isOpen: true,
      title: 'Đóng Quiz?',
      message: 'Sau khi đóng, học sinh không thể bắt đầu lượt làm mới.',
      confirmText: 'Đóng Quiz',
      confirmClass: 'bg-amber-600 hover:bg-amber-700 text-white',
      actionType: 'close',
      quiz: quiz,
      warnings: [],
      errors: []
    }
  }
  else if (type === 'delete') {
    if (quiz.usageCount > 0) {
      dialogState.value = {
        isOpen: true,
        title: 'Không thể xóa Quiz',
        message: `Quiz đang được sử dụng trong ${quiz.usageCount} bài học. Hãy gỡ Quiz khỏi các bài học trước khi xóa.`,
        confirmText: 'Đã hiểu',
        confirmClass: 'bg-slate-200 hover:bg-slate-300 text-slate-800',
        actionType: 'cancel_only',
        quiz: quiz,
        warnings: [],
        errors: []
      }
    } else {
      dialogState.value = {
        isOpen: true,
        title: 'Xóa Quiz?',
        message: `Quiz "${quiz.title}" sẽ bị xóa vĩnh viễn và không thể khôi phục.`,
        confirmText: 'Xóa Quiz',
        confirmClass: 'bg-red-600 hover:bg-red-700 text-white',
        actionType: 'delete',
        quiz: quiz,
        warnings: [],
        errors: []
      }
    }
  }
}

const confirmAction = () => {
  const type = dialogState.value.actionType
  const quiz = dialogState.value.quiz
  if (!quiz) return

  if (type === 'cancel_only') {
    closeDialog()
    return
  }

  const index = quizzes.value.findIndex(q => q.id === quiz.id)
  
  if (type === 'duplicate') {
    const newQuiz = { ...quiz }
    newQuiz.id = Date.now()
    newQuiz.code = `QZ-${quiz.subjectCode}-COPY`
    newQuiz.title = `${quiz.title} (Bản sao)`
    newQuiz.status = 'draft'
    newQuiz.usageCount = 0
    newQuiz.openAt = undefined
    newQuiz.closeAt = undefined
    newQuiz.updatedAt = new Date().toISOString()
    quizStore.addQuiz(newQuiz)
    showToast('Đã tạo bản sao Quiz trong phiên thử nghiệm.', 'success')
  } 
  else if (type === 'publish') {
    if (index !== -1) {
      const updated = { ...quizzes.value[index], status: 'published' as const, updatedAt: new Date().toISOString() }
      quizStore.updateQuiz(updated)
      showToast('Đã xuất bản Quiz trong phiên thử nghiệm.', 'success')
    }
  }
  else if (type === 'unpublish') {
    if (index !== -1) {
      const updated = { ...quizzes.value[index], status: 'draft' as const, updatedAt: new Date().toISOString() }
      quizStore.updateQuiz(updated)
      showToast('Đã chuyển Quiz về bản nháp.', 'success')
    }
  }
  else if (type === 'open') {
    if (index !== -1) {
      const updated = { ...quizzes.value[index], status: 'open' as const, updatedAt: new Date().toISOString() }
      quizStore.updateQuiz(updated)
      showToast('Đã mở Quiz.', 'success')
    }
  }
  else if (type === 'close') {
    if (index !== -1) {
      const updated = { ...quizzes.value[index], status: 'closed' as const, updatedAt: new Date().toISOString() }
      quizStore.updateQuiz(updated)
      showToast('Đã đóng Quiz.', 'success')
    }
  }
  else if (type === 'delete') {
    if (index !== -1) {
      quizStore.deleteQuiz(quizzes.value[index].id)
      showToast('Đã xóa Quiz thành công.', 'success')
      // Adjust pagination
      if (paginatedQuizzes.value.length === 0 && currentPage.value > 1) {
        currentPage.value--
      }
    }
  }

  closeDialog()
  
  // Update detail drawer state if it's open
  if (isDrawerOpen.value && selectedQuiz.value && selectedQuiz.value.id === quiz.id) {
    if (type === 'delete') {
      isDrawerOpen.value = false
    } else {
      selectedQuiz.value = quizzes.value.find(q => q.id === quiz.id) || null
    }
  }
}
</script>

<template>
  <div class="h-full flex flex-col p-6 max-w-7xl mx-auto w-full">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-8 shrink-0">
      <div>
        <h1 class="text-2xl font-bold text-slate-800 tracking-tight">Quiz / Đề kiểm tra</h1>
        <p class="text-slate-500 mt-1">Quản lý Quiz, bài kiểm tra và cấu hình đánh giá trong môn học.</p>
      </div>
      
      <button 
        @click="router.push({ name: 'content-council-quiz-create' })"
        class="flex items-center gap-2 px-4 py-2.5 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg transition-colors shadow-sm"
      >
        <Plus class="w-4 h-4" />
        Tạo Quiz
      </button>
    </div>

    <QuizFilterBar 
      v-model:filters="filters"
      v-model:sort="sort"
      @reset="resetFilters"
    />

    <!-- Main Content -->
    <div class="flex-1 flex flex-col min-h-0">
      <template v-if="isLoading">
        <QuizTableSkeleton />
      </template>

      <template v-else-if="filteredQuizzes.length === 0">
        <QuizEmptyState 
          :isFilterEmpty="quizzes.length > 0" 
          @action="quizzes.length > 0 ? resetFilters() : router.push({ name: 'content-council-quiz-create' })"
        />
      </template>

      <template v-else>
        <!-- Desktop View -->
        <div class="hidden lg:flex flex-1 flex-col min-h-0">
          <QuizTable 
            :quizzes="paginatedQuizzes"
            :current-page="currentPage"
            :page-size="pageSize"
            :total-items="filteredQuizzes.length"
            @update:page="currentPage = $event"
            @update:pageSize="pageSize = $event; currentPage = 1"
            @action="handleAction"
          />
        </div>

        <!-- Mobile View -->
        <div class="flex lg:hidden flex-col gap-1">
          <QuizMobileCard 
            v-for="quiz in paginatedQuizzes" 
            :key="quiz.id" 
            :quiz="quiz"
            @action="handleAction"
            @click="handleAction({ type: 'view', quiz })"
          />
          
          <!-- Mobile Pagination Simplified -->
          <div class="mt-4 flex justify-between items-center bg-slate-50 p-3 rounded-lg border border-slate-200">
            <button 
              @click="currentPage--"
              :disabled="currentPage === 1"
              class="px-3 py-1.5 text-sm bg-white border border-slate-300 rounded disabled:opacity-50"
            >
              Trước
            </button>
            <span class="text-sm text-slate-600">Trang {{ currentPage }} / {{ Math.ceil(filteredQuizzes.length / pageSize) }}</span>
            <button 
              @click="currentPage++"
              :disabled="currentPage >= Math.ceil(filteredQuizzes.length / pageSize)"
              class="px-3 py-1.5 text-sm bg-white border border-slate-300 rounded disabled:opacity-50"
            >
              Sau
            </button>
          </div>
        </div>
      </template>
    </div>

    <!-- Quiz Detail Drawer -->
    <QuizDetailDrawer 
      v-model:is-open="isDrawerOpen"
      :quiz="selectedQuiz"
      @action="handleAction"
    />

    <!-- Generic Confirm Dialog -->
    <div v-if="dialogState.isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" @click="closeDialog"></div>
      
      <div class="relative bg-white rounded-xl shadow-xl w-full max-w-md overflow-hidden animate-fade-in-up">
        <div class="p-6">
          <h3 class="text-lg font-bold text-slate-800 mb-2">{{ dialogState.title }}</h3>
          <p class="text-slate-600 text-sm whitespace-pre-line">{{ dialogState.message }}</p>
          
          <div v-if="dialogState.errors && dialogState.errors.length > 0" class="mt-4 p-3 bg-red-50 text-red-700 text-sm rounded-lg border border-red-100">
            <ul class="list-disc pl-4 space-y-1">
              <li v-for="(err, i) in dialogState.errors" :key="i">{{ err }}</li>
            </ul>
          </div>
        </div>
        
        <div class="p-4 border-t border-slate-100 bg-slate-50 flex justify-end gap-3">
          <button 
            v-if="dialogState.actionType !== 'cancel_only'"
            @click="closeDialog"
            class="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors"
          >
            Hủy
          </button>
          <button 
            @click="confirmAction"
            class="px-4 py-2 text-sm font-medium rounded-lg transition-colors"
            :class="dialogState.confirmClass"
          >
            {{ dialogState.confirmText }}
          </button>
        </div>
      </div>
    </div>

    <!-- Toasts Container -->
    <div class="fixed bottom-6 right-6 z-50 flex flex-col gap-3 pointer-events-none">
      <transition-group name="toast">
        <div 
          v-for="toast in toasts" 
          :key="toast.id"
          class="flex items-center gap-3 px-4 py-3 rounded-lg shadow-lg pointer-events-auto w-80"
          :class="toast.type === 'success' ? 'bg-slate-800 text-white' : 'bg-red-600 text-white'"
        >
          <div class="flex-1 text-sm font-medium">{{ toast.message }}</div>
          <button @click="toasts = toasts.filter(t => t.id !== toast.id)" class="text-white/70 hover:text-white">
            <X class="w-4 h-4" />
          </button>
        </div>
      </transition-group>
    </div>
  </div>
</template>

<style scoped>
.animate-fade-in-up {
  animation: fadeInUp 0.2s ease-out forwards;
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

.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}
.toast-enter-from {
  opacity: 0;
  transform: translateX(30px);
}
.toast-leave-to {
  opacity: 0;
  transform: translateX(30px) scale(0.9);
}
</style>

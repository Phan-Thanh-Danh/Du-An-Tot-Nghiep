<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ContentCouncilQuiz } from '@/types/content-council/quiz'
import { useQuizStore } from '@/stores/content-council/quizStore'
import { usePopupStore } from '@/stores/popup'
import QuizFilterBar from '@/components/content-council/quizzes/QuizFilterBar.vue'
import QuizTable from '@/components/content-council/quizzes/QuizTable.vue'
import QuizMobileCard from '@/components/content-council/quizzes/QuizMobileCard.vue'
import QuizDetailDrawer from '@/components/content-council/quizzes/QuizDetailDrawer.vue'
import QuizTableSkeleton from '@/components/content-council/quizzes/QuizTableSkeleton.vue'
import QuizEmptyState from '@/components/content-council/quizzes/QuizEmptyState.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { Plus, X, ShieldAlert, AlertCircle } from 'lucide-vue-next'

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

const forbidden = ref(false)
const apiError = ref('')
const popupStore = usePopupStore()

// ─── Initialize Data ───────────────────────────────────────────────────
onMounted(async () => {
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

  isLoading.value = true
  forbidden.value = false
  apiError.value = ''
  try {
    await quizStore.init()
    if (quizStore.error) {
      apiError.value = quizStore.error
    }
  } catch (e: any) {
    if (e?.statusCode === 403) {
      forbidden.value = true
    } else {
      apiError.value = e?.message || 'Không thể tải danh sách đề kiểm tra.'
    }
  } finally {
    isLoading.value = false
  }
})

const reloadData = async () => {
  quizStore.reset()
  isLoading.value = true
  forbidden.value = false
  apiError.value = ''
  try {
    await quizStore.init()
  } catch (e: any) {
    if (e?.statusCode === 403) forbidden.value = true
    else apiError.value = e?.message || 'Không thể tải danh sách đề kiểm tra.'
  } finally {
    isLoading.value = false
  }
}

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
  else if (type === 'validate') {
    const errors: string[] = []
    if (quiz.questionCount === 0) errors.push('Quiz chưa có câu hỏi.')
    if (quiz.totalScore <= 0) errors.push('Tổng điểm cấu hình phải lớn hơn 0.')
    if (quiz.durationMinutes <= 0) errors.push('Thời gian làm bài phải lớn hơn 0.')

    if (errors.length > 0) {
      popupStore.error('Chưa thể xác thực Quiz', errors.join('\n'))
      return
    }

    dialogState.value = {
      isOpen: true,
      title: 'Xác thực cấu trúc Quiz?',
      message: `Bạn có chắc chắn muốn xác thực cấu trúc cho Quiz "${quiz.title}"? Quiz sẽ được kiểm tra cấu hình điểm và câu hỏi, sau đó khóa để sẵn sàng xuất bản.`,
      confirmText: 'Xác thực',
      confirmClass: 'bg-emerald-600 hover:bg-emerald-700 text-white',
      actionType: 'validate',
      quiz: quiz,
      warnings: [],
      errors: []
    }
  }
  else if (type === 'publish') {
    if (quiz.trangThaiDuyet !== 'da_xac_thuc') {
      popupStore.error('Chưa thể xuất bản', 'Quiz cần được Xác thực (Validated) trước khi xuất bản.')
      return
    }

    dialogState.value = {
      isOpen: true,
      title: 'Xuất bản Quiz?',
      message: `Bạn có chắc chắn muốn công bố Quiz "${quiz.title}"? Quiz sẽ sẵn sàng cho học sinh hoặc gắn vào nội dung bài học.`,
      confirmText: 'Xuất bản',
      confirmClass: 'bg-blue-600 hover:bg-blue-700 text-white',
      actionType: 'publish',
      quiz: quiz,
      warnings: [],
      errors: []
    }
  }
  else if (type === 'unpublish') {
    if (quiz.status === 'open') {
      popupStore.error('Không thể hủy xuất bản', 'Không thể chuyển về nháp khi Quiz đang mở.')
      return
    }
    dialogState.value = {
      isOpen: true,
      title: 'Hủy xuất bản Quiz?',
      message: `Bạn có chắc chắn muốn hủy xuất bản Quiz "${quiz.title}"? Trạng thái sẽ quay lại Bản nháp và cần xác thực lại nếu muốn xuất bản trong tương lai.`,
      confirmText: 'Chuyển về bản nháp',
      confirmClass: 'bg-amber-600 hover:bg-amber-700 text-white',
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
      popupStore.error('Không thể xóa', `Quiz đang được sử dụng trong ${quiz.usageCount} bài học. Vui lòng gỡ Quiz khỏi bài học trước.`)
      return
    }
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

const isExecutingAction = ref(false)
const confirmAction = async () => {
  const type = dialogState.value.actionType
  const quiz = dialogState.value.quiz
  if (!quiz) return

  isExecutingAction.value = true
  try {
    if (type === 'duplicate') {
      const newQuiz = { ...quiz }
      newQuiz.id = Date.now()
      newQuiz.code = `QZ-${quiz.subjectCode}-${Math.floor(Math.random() * 1000)}`
      newQuiz.title = `${quiz.title} (Bản sao)`
      newQuiz.status = 'draft'
      newQuiz.usageCount = 0
      newQuiz.openAt = undefined
      newQuiz.closeAt = undefined
      newQuiz.updatedAt = new Date().toISOString()
      await quizStore.addQuiz(newQuiz)
      popupStore.success('Thành công', 'Đã nhân bản Quiz.')
    } 
    else if (type === 'validate') {
      await quizStore.validateQuizAction(quiz.id)
      popupStore.success('Thành công', 'Đã xác thực đề kiểm tra.')
    }
    else if (type === 'publish') {
      await quizStore.publishQuizAction(quiz.id)
      popupStore.success('Thành công', 'Đã xuất bản đề kiểm tra.')
    }
    else if (type === 'unpublish') {
      await quizStore.unpublishQuizAction(quiz.id)
      popupStore.success('Thành công', 'Đã chuyển đề kiểm tra về bản nháp.')
    }
    else if (type === 'open') {
      await quizStore.updateQuiz(quiz.id, { status: 'open' })
      popupStore.success('Thành công', 'Đã mở đề kiểm tra.')
    }
    else if (type === 'close') {
      await quizStore.updateQuiz(quiz.id, { status: 'closed' })
      popupStore.success('Thành công', 'Đã đóng đề kiểm tra.')
    }
    else if (type === 'delete') {
      await quizStore.deleteQuiz(quiz.id)
      popupStore.success('Thành công', 'Đã xóa Quiz thành công.')
      if (paginatedQuizzes.value.length === 0 && currentPage.value > 1) {
        currentPage.value--
      }
    }
    closeDialog()
    
    // Update detail drawer state if it's open
    if (isDrawerOpen.value && selectedQuiz.value && selectedQuiz.value.id === quiz.id) {
      if (type === 'delete') {
        isDrawerOpen.value = false
      } else {
        selectedQuiz.value = quizStore.getQuizById(quiz.id) || null
      }
    }
  } catch (e: any) {
    popupStore.error('Lỗi thao tác', e.message || 'Không thể thực hiện hành động.')
  } finally {
    isExecutingAction.value = false
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

      <template v-else-if="forbidden">
        <div class="flex flex-col items-center justify-center py-16 bg-(--surface-card) border border-(--border-default) rounded-xl">
          <ShieldAlert :size="48" class="text-red-500 mb-4" />
          <h3 class="text-lg font-bold text-(--text-heading)">Không có quyền truy cập</h3>
          <p class="text-sm text-(--text-muted) mt-1">Tài khoản của bạn không được phân quyền quản lý đề thi.</p>
        </div>
      </template>

      <template v-else-if="apiError">
        <div class="flex flex-col items-center justify-center py-16 bg-(--surface-card) border border-(--border-default) rounded-xl">
          <AlertCircle :size="48" class="text-red-500 mb-4" />
          <h3 class="text-lg font-bold text-(--text-heading)">Đã xảy ra lỗi</h3>
          <p class="text-sm text-(--text-muted) mt-1 mb-4">{{ apiError }}</p>
          <button @click="reloadData" class="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700">Thử lại</button>
        </div>
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

    <!-- Confirm Dialog -->
    <ConfirmActionDialog
      v-model="dialogState.isOpen"
      :title="dialogState.title"
      :message="dialogState.message"
      :confirmLabel="dialogState.confirmText"
      :variant="dialogState.actionType === 'delete' ? 'danger' : dialogState.actionType === 'validate' ? 'success' : 'primary'"
      :loading="isExecutingAction"
      @confirm="confirmAction"
      @cancel="closeDialog"
    />

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

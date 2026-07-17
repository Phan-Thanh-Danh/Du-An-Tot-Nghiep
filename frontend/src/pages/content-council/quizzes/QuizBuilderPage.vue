<script setup lang="ts">
import { ref, onMounted, computed, onUnmounted } from 'vue'
import { useRoute, useRouter, onBeforeRouteLeave } from 'vue-router'
import { useQuizStore } from '@/stores/content-council/quizStore'
import { useQuizBuilder } from '@/composables/content-council/useQuizBuilder'
import { useQuizBuilderValidation } from '@/composables/content-council/useQuizBuilderValidation'
import { usePopupStore } from '@/stores/popup'
import { ContentCouncilQuiz } from '@/types/content-council/quiz'
import { QuestionBankItem } from '@/types/content-council/questionBank'

import QuizBuilderHeader from '@/components/content-council/quizzes/builder/QuizBuilderHeader.vue'
import QuizBuilderSummary from '@/components/content-council/quizzes/builder/QuizBuilderSummary.vue'
import QuizBuilderReadOnlyAlert from '@/components/content-council/quizzes/builder/QuizBuilderReadOnlyAlert.vue'
import QuizStructureValidationPanel from '@/components/content-council/quizzes/builder/QuizStructureValidationPanel.vue'
import SelectedQuestionPanel from '@/components/content-council/quizzes/builder/SelectedQuestionPanel.vue'
import QuestionBankPicker from '@/components/content-council/quizzes/builder/QuestionBankPicker.vue'
import QuizBuilderSkeleton from '@/components/content-council/quizzes/builder/QuizBuilderSkeleton.vue'
import QuizBuilderEmptyState from '@/components/content-council/quizzes/builder/QuizBuilderEmptyState.vue'
import RemoveQuestionDialog from '@/components/content-council/quizzes/builder/RemoveQuestionDialog.vue'
import BulkAddQuestionDialog from '@/components/content-council/quizzes/builder/BulkAddQuestionDialog.vue'
import PublishQuizFromBuilderDialog from '@/components/content-council/quizzes/builder/PublishQuizFromBuilderDialog.vue'
import QuestionPreviewDrawer from '@/components/content-council/quizzes/builder/QuestionPreviewDrawer.vue'

const route = useRoute()
const router = useRouter()
const quizStore = useQuizStore()
quizStore.init()
const validation = useQuizBuilderValidation()
const popupStore = usePopupStore()

const isLoading = ref(true)
const quiz = ref<ContentCouncilQuiz | null>(null)

// Dialog states
const previewQuestionId = ref<number | null>(null)
const removeQuestionId = ref<number | null>(null)
const showBulkAddDialog = ref(false)
const showPublishDialog = ref(false)

// Mobile tabs state
const activeTab = ref<'quiz' | 'bank'>('quiz')

let builder: ReturnType<typeof useQuizBuilder> | null = null

// Computed state from builder
const builderState = computed(() => builder?.state.value)
const isDirty = computed(() => builderState.value?.isDirty ?? false)
const isSaving = computed(() => builderState.value?.isSaving ?? false)
const isReadOnly = computed(() => builderState.value?.isReadOnly ?? false)
const questions = computed(() => builderState.value?.questions ?? [])
const selectedQuestionIdsBulk = computed(() => builderState.value?.selectedQuestionIds ?? [])
const bankQuestions = computed(() => builder?.bankQuestions.value ?? [])

// Computed properties for selected questions
const selectedQuestionIdsInQuiz = computed(() => questions.value.map(q => q.questionId))

// Validation
const validationResult = computed(() => {
  if (!quiz.value) return null
  return validation.validate(quiz.value, questions.value)
})

const canPublish = computed(() => validationResult.value?.canPublish ?? false)

onMounted(async () => {
  const qId = Number(route.params.quizId)
  if (!qId) {
    isLoading.value = false
    return
  }

  // Simulate network delay removed per UX standard

  const found = quizStore.getQuizById(qId)
  if (!found) {
    isLoading.value = false
    return
  }

  quiz.value = JSON.parse(JSON.stringify(found))
  
  builder = useQuizBuilder(qId, quiz.value!.subjectId)
  builder.init()

  // Set read-only if open or closed
  if (quiz.value?.status === 'open' || quiz.value?.status === 'closed') {
    builder.setReadonly(true)
  }

  isLoading.value = false
})

// Navigation Guard
onBeforeRouteLeave((to, from, next) => {
  if (isDirty.value && !isSaving.value) {
    const answer = window.confirm('Bạn có thay đổi cấu trúc chưa lưu. Các câu hỏi, điểm số hoặc thứ tự vừa thay đổi sẽ bị mất. Rời khỏi trang?')
    if (answer) {
      next()
    } else {
      next(false)
    }
  } else {
    next()
  }
})

// Handlers
const handleSave = async () => {
  if (builder && quiz.value) {
    if (!validationResult.value?.canSave) {
      popupStore.error('Cấu trúc chưa hợp lệ', 'Cấu trúc chưa hợp lệ để lưu. Vui lòng kiểm tra lại các lỗi.')
      return
    }
    
    await builder.saveStructure()
    // Sync to store if needed
    quiz.value.questionCount = questions.value.length
    quiz.value.multipleChoiceQuestionCount = validationResult.value.metrics.multipleChoiceCount
    quiz.value.essayQuestionCount = validationResult.value.metrics.essayCount
    await quizStore.updateQuiz(quiz.value.id, quiz.value)

    if (validationResult.value.errors.length > 0) {
      popupStore.warning('Đã lưu cấu trúc', 'Đã lưu cấu trúc Quiz thành công, nhưng Quiz chưa đủ điều kiện xuất bản.')
    } else {
      popupStore.success('Thành công', 'Đã lưu cấu trúc Quiz thành công.')
    }
  }
}

const handlePublish = () => {
  showPublishDialog.value = true
}

const confirmPublish = async () => {
  if (quiz.value && canPublish.value) {
    try {
      await quizStore.publishQuizAction(quiz.value.id)
      
      quiz.value.status = 'published'
      quiz.value.questionCount = questions.value.length
      quiz.value.multipleChoiceQuestionCount = validationResult.value!.metrics.multipleChoiceCount
      quiz.value.essayQuestionCount = validationResult.value!.metrics.essayCount
      quiz.value.trangThaiDuyet = 'da_xac_thuc'
      
      if (builder) {
        await builder.saveStructure() // clear dirty state
      }
      
      showPublishDialog.value = false
      popupStore.success('Xuất bản thành công', 'Đã xuất bản Quiz thành công.')
    } catch (e: any) {
      popupStore.error('Lỗi xuất bản', e.message || 'Không thể xuất bản Quiz.')
    }
  }
}

const handlePreview = (id: number) => {
  previewQuestionId.value = id
}

const handleRemoveRequest = (id: number) => {
  removeQuestionId.value = id
}

const confirmRemove = () => {
  if (removeQuestionId.value && builder) {
    builder.removeQuestion(removeQuestionId.value)
    removeQuestionId.value = null
  }
}

const getPreviewQuestionItem = computed(() => {
  if (!previewQuestionId.value) return null
  // look in current questions first
  const qb = questions.value.find(q => q.questionId === previewQuestionId.value)
  if (qb) return qb as unknown as QuestionBankItem // Compatible enough for the drawer
  // then look in bank
  return bankQuestions.value.find(q => q.id === previewQuestionId.value) || null
})

const getQuestionCodeToRemove = computed(() => {
  if (!removeQuestionId.value) return ''
  const q = questions.value.find(q => q.questionId === removeQuestionId.value)
  return q ? q.questionCode : ''
})

const handleBulkAddConfirm = (scorePerQuestion: number) => {
  if (builder) {
    const itemsToAdd = bankQuestions.value.filter(q => selectedQuestionIdsBulk.value.includes(q.id))
    builder.bulkAdd(itemsToAdd, scorePerQuestion)
    builder.clearSelection()
    showBulkAddDialog.value = false
  }
}

</script>

<template>
  <QuizBuilderSkeleton v-if="isLoading" />
  
  <QuizBuilderEmptyState v-else-if="!quiz || !builder" />

  <div v-else class="h-screen flex flex-col bg-slate-50 overflow-hidden">
    
    <QuizBuilderHeader 
      :quiz="quiz"
      :is-dirty="isDirty"
      :is-saving="isSaving"
      :is-read-only="isReadOnly"
      :can-publish="canPublish"
      @save="handleSave"
      @publish="handlePublish"
    />

    <!-- Main Content Split -->
    <div class="flex-1 overflow-hidden flex flex-col md:flex-row w-full max-w-[1600px] mx-auto">
      
      <!-- Mobile Tabs -->
      <div class="md:hidden flex border-b border-slate-200 bg-white shrink-0">
        <button 
          @click="activeTab = 'quiz'"
          class="flex-1 py-3 text-sm font-medium border-b-2 transition-colors"
          :class="activeTab === 'quiz' ? 'border-blue-600 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-700'"
        >
          Câu hỏi trong Quiz ({{ questions.length }})
        </button>
        <button 
          @click="activeTab = 'bank'"
          class="flex-1 py-3 text-sm font-medium border-b-2 transition-colors"
          :class="activeTab === 'bank' ? 'border-blue-600 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-700'"
        >
          Ngân hàng câu hỏi
        </button>
      </div>

      <!-- Left Panel: Quiz Questions -->
      <div 
        class="flex-1 flex flex-col overflow-hidden transition-all duration-300"
        :class="{'hidden md:flex': activeTab !== 'quiz'}"
      >
        <div class="flex-1 overflow-y-auto p-4 md:p-6 space-y-4">
          <QuizBuilderReadOnlyAlert :status="quiz.status" />

          <QuizBuilderSummary 
            v-if="validationResult"
            :question-count="validationResult.metrics.questionCount"
            :multiple-choice-count="validationResult.metrics.multipleChoiceCount"
            :essay-count="validationResult.metrics.essayCount"
            :current-score="validationResult.metrics.currentScore"
            :configured-score="validationResult.metrics.configuredScore"
          />

          <QuizStructureValidationPanel 
            v-if="validationResult"
            :can-save="validationResult.canSave"
            :can-publish="validationResult.canPublish"
            :errors="validationResult.errors"
            :warnings="validationResult.warnings"
          />

          <div class="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden flex flex-col min-h-[400px]">
            <SelectedQuestionPanel 
              :questions="questions"
              :is-read-only="isReadOnly"
              @preview="handlePreview"
              @update-score="builder.updateScore"
              @move-up="builder.moveUp"
              @move-down="builder.moveDown"
              @remove="handleRemoveRequest"
            />
          </div>
        </div>
      </div>

      <!-- Right Panel: Question Bank -->
      <div 
        class="w-full md:w-[420px] lg:w-[480px] flex-col shrink-0 transition-all duration-300"
        :class="activeTab === 'bank' ? 'flex h-full' : 'hidden md:flex'"
      >
        <QuestionBankPicker 
          :subject-code="quiz.subjectCode"
          :available-questions="bankQuestions"
          :selected-question-ids-in-quiz="selectedQuestionIdsInQuiz"
          :selected-question-ids-bulk="selectedQuestionIdsBulk"
          :is-read-only="isReadOnly"
          @preview="handlePreview"
          @add-single="q => builder!.addQuestion(q)"
          @toggle-bulk-select="builder.toggleSelection"
          @select-all-bulk="builder.selectAllBulk"
          @clear-bulk-select="builder.clearSelection"
          @bulk-add-click="showBulkAddDialog = true"
        />
      </div>

    </div>

    <!-- Modals & Drawers -->
    <RemoveQuestionDialog 
      :is-open="removeQuestionId !== null"
      :question-code="getQuestionCodeToRemove"
      @close="removeQuestionId = null"
      @confirm="confirmRemove"
    />

    <BulkAddQuestionDialog 
      v-if="validationResult"
      :is-open="showBulkAddDialog"
      :question-count="selectedQuestionIdsBulk.length"
      :missing-score="validationResult.metrics.configuredScore - validationResult.metrics.currentScore"
      @close="showBulkAddDialog = false"
      @confirm="handleBulkAddConfirm"
    />

    <PublishQuizFromBuilderDialog 
      v-if="validationResult && quiz"
      :is-open="showPublishDialog"
      :can-publish="validationResult.canPublish"
      :errors="validationResult.errors"
      :quiz-config="{
        questionCount: quiz.questionCount,
        totalScore: quiz.totalScore,
        durationMinutes: quiz.durationMinutes,
        passingScore: quiz.passingScore,
        minimumCorrectAnswers: quiz.minimumCorrectAnswers,
        passMethod: quiz.passMethod
      }"
      @close="showPublishDialog = false"
      @confirm="confirmPublish"
    />

    <QuestionPreviewDrawer 
      :is-open="previewQuestionId !== null"
      :question="getPreviewQuestionItem"
      @close="previewQuestionId = null"
    />

  </div>
</template>

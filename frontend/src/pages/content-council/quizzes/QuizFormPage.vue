<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue'
import { useRouter, useRoute, onBeforeRouteLeave } from 'vue-router'
import { QuizFormData, QuizFormMode } from '@/types/content-council/quizForm'
import { useQuizStore } from '@/stores/content-council/quizStore'
import { useQuizFormValidation } from '@/composables/content-council/useQuizFormValidation'

// Components
import QuizFormHeader from '@/components/content-council/quizzes/form/QuizFormHeader.vue'
import QuizGeneralInformationSection from '@/components/content-council/quizzes/form/QuizGeneralInformationSection.vue'
import QuizStructureSection from '@/components/content-council/quizzes/form/QuizStructureSection.vue'
import QuizPassingRulesSection from '@/components/content-council/quizzes/form/QuizPassingRulesSection.vue'
import QuizAttemptsSection from '@/components/content-council/quizzes/form/QuizAttemptsSection.vue'
import QuizScheduleSection from '@/components/content-council/quizzes/form/QuizScheduleSection.vue'
import QuizDisplayOptionsSection from '@/components/content-council/quizzes/form/QuizDisplayOptionsSection.vue'
import QuizFormSummaryCard from '@/components/content-council/quizzes/form/QuizFormSummaryCard.vue'
import QuizFormActionBar from '@/components/content-council/quizzes/form/QuizFormActionBar.vue'
import QuizFormSkeleton from '@/components/content-council/quizzes/form/QuizFormSkeleton.vue'
import QuizReadOnlyAlert from '@/components/content-council/quizzes/form/QuizReadOnlyAlert.vue'
import QuizUnsavedChangesDialog from '@/components/content-council/quizzes/form/QuizUnsavedChangesDialog.vue'
import { ChevronLeft } from 'lucide-vue-next'

const router = useRouter()
const route = useRoute()
const quizStore = useQuizStore()
quizStore.init()

const mode = computed<QuizFormMode>(() => route.params.quizId ? 'edit' : 'create')

// Initial Default Data
const getDefaultData = (): QuizFormData => ({
  subjectId: Number(route.query.subjectId) || null,
  semesterId: null,
  title: '',
  description: '',
  examType: 'lesson_quiz',
  format: 'multiple_choice',
  durationMinutes: 15,
  multipleChoicePercentage: 100,
  essayPercentage: 0,
  totalScore: 10,
  passMethod: 'score',
  passingScore: 5,
  minimumCorrectAnswers: null,
  unlimitedAttempts: false,
  maximumAttempts: 2,
  finalScoreMethod: 'highest',
  openAt: null,
  closeAt: null,
  shuffleQuestions: false,
  shuffleAnswers: false,
  showResultAfterSubmit: true,
  showCorrectAnswerAfterSubmit: false,
  showExplanationAfterSubmit: false,
  status: 'draft'
})

const formData = ref<QuizFormData>(getDefaultData())
const initialDataString = ref('')
const isLoading = ref(true)
const isSaving = ref(false)
const quizNotFound = ref(false)

const { 
  validationErrors, 
  sectionErrors, 
  validate, 
  resetValidation 
} = useQuizFormValidation(formData)

const isDirty = computed(() => {
  if (isLoading.value) return false
  return JSON.stringify(formData.value) !== initialDataString.value
})

const isReadOnly = computed(() => {
  if (mode.value === 'create') return false
  return formData.value.status === 'open' || formData.value.status === 'closed'
})

const hasQuestions = computed(() => {
  if (mode.value === 'create') return false
  const quiz = quizStore.getQuizById(Number(route.params.quizId))
  return quiz ? quiz.questionCount > 0 : false
})

// Load Data
onMounted(() => {
  if (mode.value === 'edit') {
    const id = Number(route.params.quizId)
    const quiz = quizStore.getQuizById(id)
    if (!quiz) {
      quizNotFound.value = true
      isLoading.value = false
      return
    }

    formData.value = {
      id: quiz.id,
      code: quiz.code,
      subjectId: quiz.subjectId,
      semesterId: quiz.semesterId || null,
      title: quiz.title,
      description: '', // mock data doesn't have desc yet
      examType: quiz.examType,
      format: quiz.format,
      durationMinutes: quiz.durationMinutes,
      multipleChoicePercentage: quiz.format === 'mixed' ? 70 : (quiz.format === 'essay' ? 0 : 100),
      essayPercentage: quiz.format === 'mixed' ? 30 : (quiz.format === 'essay' ? 100 : 0),
      totalScore: quiz.totalScore,
      passMethod: quiz.passMethod,
      passingScore: quiz.passingScore || null,
      minimumCorrectAnswers: quiz.minimumCorrectAnswers || null,
      unlimitedAttempts: quiz.unlimitedAttempts,
      maximumAttempts: quiz.maximumAttempts || null,
      finalScoreMethod: quiz.finalScoreMethod,
      openAt: quiz.openAt || null,
      closeAt: quiz.closeAt || null,
      shuffleQuestions: quiz.shuffleQuestions,
      shuffleAnswers: quiz.shuffleAnswers,
      showResultAfterSubmit: quiz.showResultAfterSubmit,
      showCorrectAnswerAfterSubmit: quiz.showCorrectAnswerAfterSubmit,
      showExplanationAfterSubmit: false,
      status: quiz.status
    }
  }

  initialDataString.value = JSON.stringify(formData.value)
  
  setTimeout(() => {
    isLoading.value = false
  }, 500)
})

// Leave Guard
const showLeaveDialog = ref(false)
let pendingRouteLocation: any = null

onBeforeRouteLeave((to, from, next) => {
  if (isDirty.value && !isSaving.value) {
    showLeaveDialog.value = true
    pendingRouteLocation = to
    next(false)
  } else {
    next()
  }
})

const confirmLeave = () => {
  showLeaveDialog.value = false
  isSaving.value = true // bypass guard
  if (pendingRouteLocation) {
    router.push(pendingRouteLocation)
  } else {
    router.push({ name: 'content-council-quizzes' })
  }
}

// Actions
const handleCancel = () => {
  if (isDirty.value) {
    showLeaveDialog.value = true
  } else {
    router.push({ name: 'content-council-quizzes' })
  }
}

const mapFormToQuizModel = () => {
  const d = formData.value
  const subjectName = d.subjectId === 1 ? 'Lập trình Web cơ bản' : d.subjectId === 2 ? 'Nhập môn CNTT' : d.subjectId === 3 ? 'Lập trình Java' : 'Hệ quản trị CSDL'
  const subjectCode = d.subjectId === 1 ? 'WEB201' : d.subjectId === 2 ? 'COM101' : d.subjectId === 3 ? 'PRO192' : 'DBI202'
  
  return {
    subjectCode,
    subjectName,
    ...d
  }
}

const saveDraft = async () => {
  const { isValid, sectionErrors: sErr } = validate()
  
  if (!isValid) {
    // scroll to first error visually
    window.scrollTo({ top: 0, behavior: 'smooth' })
    return false
  }

  isSaving.value = true
  
  // mock api call delay
  await new Promise(r => setTimeout(r, 600))
  
  const mapped = mapFormToQuizModel()
  
  if (mode.value === 'create') {
    const newQuiz: any = {
      ...mapped,
      id: Date.now(),
      code: `QZ-${mapped.subjectCode}-NEW`,
      questionCount: 0,
      multipleChoiceQuestionCount: 0,
      essayQuestionCount: 0,
      usageCount: 0,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    }
    quizStore.addQuiz(newQuiz)
    
    // Update snapshot
    formData.value.id = newQuiz.id
    formData.value.code = newQuiz.code
  } else {
    const oldQuiz = quizStore.getQuizById(mapped.id as number)
    const updatedQuiz: any = {
      ...oldQuiz,
      ...mapped,
      updatedAt: new Date().toISOString()
    }
    quizStore.updateQuiz(updatedQuiz)
  }
  
  initialDataString.value = JSON.stringify(formData.value)
  isSaving.value = false
  return true
}

const handleSaveDraft = async () => {
  const ok = await saveDraft()
  if (ok) {
    // Show toast
    const msg = document.createElement('div')
    msg.className = 'fixed bottom-24 right-6 bg-slate-800 text-white px-4 py-3 rounded-lg shadow-lg z-50 text-sm animate-fade-in-up'
    msg.innerHTML = mode.value === 'create' ? 'Đã tạo Quiz nháp trong phiên thử nghiệm.' : 'Đã cập nhật Quiz trong phiên thử nghiệm.'
    document.body.appendChild(msg)
    setTimeout(() => msg.remove(), 3000)
    
    // Optionally redirect back
    if (mode.value === 'create') {
      router.push({ name: 'content-council-quizzes' })
    }
  }
}

const handleSaveAndBuild = async () => {
  const ok = await saveDraft()
  if (ok && formData.value.id) {
    router.push({ name: 'content-council-quiz-builder', params: { quizId: formData.value.id } })
  }
}
</script>

<template>
  <div class="min-h-full pb-32">
    
    <!-- 404 Not Found -->
    <div v-if="quizNotFound" class="p-6 max-w-2xl mx-auto text-center py-20">
      <div class="bg-slate-100 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-4">
        <span class="text-3xl text-slate-400">?</span>
      </div>
      <h2 class="text-2xl font-bold text-slate-800 mb-2">Không tìm thấy Quiz</h2>
      <p class="text-slate-600 mb-6">Quiz bạn đang tìm không tồn tại hoặc đã bị xóa.</p>
      <button 
        @click="router.push({ name: 'content-council-quizzes' })"
        class="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors inline-flex items-center gap-2"
      >
        <ChevronLeft class="w-4 h-4" />
        Quay lại danh sách Quiz
      </button>
    </div>

    <!-- Main Form Content -->
    <template v-else>
      <div class="p-6 max-w-[1200px] mx-auto w-full flex flex-col">
        
        <button 
          @click="handleCancel"
          class="flex items-center gap-1 text-sm font-medium text-slate-500 hover:text-slate-800 transition-colors w-max mb-4"
        >
          <ChevronLeft class="w-4 h-4" />
          Quiz / Đề kiểm tra
        </button>

        <!-- Skeleton -->
        <QuizFormSkeleton v-if="isLoading" />

        <!-- Form Layout -->
        <div v-else>
          <QuizFormHeader :mode="mode" />
          
          <QuizReadOnlyAlert :status="formData.status" />

          <div class="grid grid-cols-1 lg:grid-cols-12 gap-8">
            
            <!-- Left Column: Form Sections -->
            <div class="lg:col-span-8 space-y-6">
              <QuizGeneralInformationSection 
                v-model="formData" 
                :is-read-only="isReadOnly"
                :errors="validationErrors"
                :has-questions="hasQuestions"
              />
              
              <QuizStructureSection 
                v-model="formData" 
                :is-read-only="isReadOnly"
                :errors="validationErrors"
              />
              
              <QuizPassingRulesSection 
                v-model="formData" 
                :is-read-only="isReadOnly"
                :errors="validationErrors"
                :has-questions="hasQuestions"
              />
              
              <QuizAttemptsSection 
                v-model="formData" 
                :is-read-only="isReadOnly"
                :errors="validationErrors"
              />
              
              <QuizScheduleSection 
                v-model="formData" 
                :is-read-only="isReadOnly"
                :errors="validationErrors"
              />
              
              <QuizDisplayOptionsSection 
                v-model="formData" 
                :is-read-only="isReadOnly"
              />
            </div>

            <!-- Right Column: Summary Sticky -->
            <div class="lg:col-span-4">
              <QuizFormSummaryCard 
                :form-data="formData"
                :has-questions="hasQuestions"
              />
            </div>

          </div>
        </div>

      </div>

      <!-- Action Bar -->
      <QuizFormActionBar 
        v-if="!quizNotFound && !isLoading"
        :mode="mode"
        :is-dirty="isDirty"
        :is-saving="isSaving"
        :is-read-only="isReadOnly"
        @cancel="handleCancel"
        @save-draft="handleSaveDraft"
        @save-and-build="handleSaveAndBuild"
      />
    </template>

    <QuizUnsavedChangesDialog 
      :is-open="showLeaveDialog"
      @close="showLeaveDialog = false"
      @confirm="confirmLeave"
    />
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
</style>

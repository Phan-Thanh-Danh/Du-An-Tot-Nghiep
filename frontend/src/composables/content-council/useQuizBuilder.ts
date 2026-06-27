import { ref, computed } from 'vue'
import { QuizBuilderQuestion } from '@/types/content-council/quiz'
import { QuestionBankItem } from '@/types/content-council/questionBank'
import { useQuizStore } from '@/stores/content-council/quizStore'
import { useQuestionStore } from '@/stores/content-council/questionStore'

export interface QuizBuilderState {
  quizId: number
  questions: QuizBuilderQuestion[]
  selectedQuestionIds: number[]
  isDirty: boolean
  isSaving: boolean
  isReadOnly: boolean
}

export function useQuizBuilder(quizId: number, subjectId: number) {
  
  const state = ref<QuizBuilderState>({
    quizId: quizId,
    questions: [],
    selectedQuestionIds: [], // For bulk actions
    isDirty: false,
    isSaving: false,
    isReadOnly: false
  })

  const quizStore = useQuizStore()
  const questionStore = useQuestionStore()
  quizStore.init()
  questionStore.init()

  // Initialize from store
  const init = () => {
    const storeQuestions = quizStore.getQuestionsForQuiz(quizId)
    // Clone to local state for builder
    state.value.questions = JSON.parse(JSON.stringify(storeQuestions))
  }

  // Question Bank Source (filtered by subject)
  const bankQuestions = computed(() => {
    return questionStore.getQuestionsBySubject(subjectId)
  })

  // Normalize order from 1 to N
  const normalizeOrder = () => {
    state.value.questions.forEach((q, index) => {
      q.order = index + 1
    })
  }

  // Actions
  const addQuestion = (bankItem: QuestionBankItem, score: number = 1) => {
    // Check if already exists
    if (state.value.questions.some(q => q.questionId === bankItem.id)) return false
    
    const newQuestion: QuizBuilderQuestion = {
      questionId: bankItem.id,
      questionCode: bankItem.code,
      questionContent: bankItem.content,
      subjectId: bankItem.subjectId,
      questionType: bankItem.type,
      selectionType: bankItem.selectionType,
      difficulty: bankItem.difficulty,
      score: score,
      order: state.value.questions.length + 1,
      status: bankItem.status,
      choices: bankItem.choices,
      correctAnswerIds: bankItem.correctAnswerIds,
      answerExplanation: bankItem.answerExplanation,
      sampleAnswer: bankItem.sampleAnswer
    }

    state.value.questions.push(newQuestion)
    normalizeOrder()
    state.value.isDirty = true
    return true
  }

  const removeQuestion = (questionId: number) => {
    const idx = state.value.questions.findIndex(q => q.questionId === questionId)
    if (idx !== -1) {
      state.value.questions.splice(idx, 1)
      normalizeOrder()
      
      // Remove from selectedIds if present
      const selectedIdx = state.value.selectedQuestionIds.indexOf(questionId)
      if (selectedIdx !== -1) {
        state.value.selectedQuestionIds.splice(selectedIdx, 1)
      }
      
      state.value.isDirty = true
      return true
    }
    return false
  }

  const updateScore = (questionId: number, score: number) => {
    const question = state.value.questions.find(q => q.questionId === questionId)
    if (question && score > 0) {
      question.score = score
      state.value.isDirty = true
      return true
    }
    return false
  }

  const reorderQuestion = (fromIndex: number, toIndex: number) => {
    if (fromIndex < 0 || fromIndex >= state.value.questions.length) return false
    if (toIndex < 0 || toIndex >= state.value.questions.length) return false
    
    const [movedItem] = state.value.questions.splice(fromIndex, 1)
    state.value.questions.splice(toIndex, 0, movedItem)
    normalizeOrder()
    state.value.isDirty = true
    return true
  }

  const moveUp = (index: number) => {
    if (index > 0) {
      reorderQuestion(index, index - 1)
    }
  }

  const moveDown = (index: number) => {
    if (index < state.value.questions.length - 1) {
      reorderQuestion(index, index + 1)
    }
  }

  // Bulk Selection
  const toggleSelection = (questionId: number) => {
    const idx = state.value.selectedQuestionIds.indexOf(questionId)
    if (idx === -1) {
      state.value.selectedQuestionIds.push(questionId)
    } else {
      state.value.selectedQuestionIds.splice(idx, 1)
    }
  }

  const selectAllBulk = (ids: number[]) => {
    state.value.selectedQuestionIds = [...ids]
  }

  const clearSelection = () => {
    state.value.selectedQuestionIds = []
  }

  const bulkAdd = (bankItems: QuestionBankItem[], scorePerQuestion: number) => {
    let added = 0
    bankItems.forEach(item => {
      if (addQuestion(item, scorePerQuestion)) {
        added++
      }
    })
    return added
  }

  const setReadonly = (value: boolean) => {
    state.value.isReadOnly = value
  }

  // Saving Simulation
  const saveStructure = async () => {
    state.value.isSaving = true
    try {
      quizStore.updateQuizQuestions(quizId, state.value.questions)
      state.value.isDirty = false
      return true
    } catch (e) {
      console.error(e)
      return false
    } finally {
      state.value.isSaving = false
    }
  }

  return {
    state,
    bankQuestions,
    init,
    addQuestion,
    removeQuestion,
    updateScore,
    reorderQuestion,
    moveUp,
    moveDown,
    toggleSelection,
    selectAllBulk,
    clearSelection,
    bulkAdd,
    setReadonly,
    saveStructure
  }
}

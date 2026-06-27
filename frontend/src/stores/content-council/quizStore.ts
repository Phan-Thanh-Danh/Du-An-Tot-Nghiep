import { defineStore } from 'pinia'
import { ref } from 'vue'
import { initializeQuizMockData } from '@/mocks/content-council'
import { ContentCouncilQuiz, QuizBuilderQuestion } from '@/types/content-council/quiz'
import { useQuestionStore } from './questionStore'

export const useQuizStore = defineStore('contentCouncilQuiz', () => {
  const quizzes = ref<ContentCouncilQuiz[]>([])
  const quizQuestions = ref<Record<number, QuizBuilderQuestion[]>>({})
  const initialized = ref(false)

  const questionStore = useQuestionStore()

  function init() {
    if (initialized.value) return
    const mock = initializeQuizMockData()
    quizzes.value = mock.quizzes
    quizQuestions.value = mock.quizQuestions
    initialized.value = true
  }

  function reset() {
    initialized.value = false
    init()
  }

  function getQuizById(id: number) {
    return quizzes.value.find(q => q.id === id)
  }

  function getQuestionsForQuiz(quizId: number) {
    return quizQuestions.value[quizId] || []
  }

  function addQuiz(q: ContentCouncilQuiz) {
    quizzes.value.unshift(q)
    quizQuestions.value[q.id] = []
  }

  function updateQuiz(id: number, payload: Partial<ContentCouncilQuiz>) {
    const idx = quizzes.value.findIndex(q => q.id === id)
    if (idx !== -1) {
      Object.assign(quizzes.value[idx], payload)
      quizzes.value[idx].updatedAt = new Date().toISOString()
    }
  }

  function deleteQuiz(id: number) {
    const idx = quizzes.value.findIndex(q => q.id === id)
    if (idx !== -1) {
      // Decrease usage count for all questions in this quiz
      const questions = quizQuestions.value[id] || []
      questions.forEach(q => {
        questionStore.adjustUsageCount(q.questionId, -1)
      })
      quizzes.value.splice(idx, 1)
      delete quizQuestions.value[id]
    }
  }

  // Builder Methods
  function updateQuizQuestions(quizId: number, questions: QuizBuilderQuestion[]) {
    // 1. Find differences to update usage count
    const oldQ = quizQuestions.value[quizId] || []
    const oldIds = new Set(oldQ.map(q => q.questionId))
    const newIds = new Set(questions.map(q => q.questionId))

    // added
    newIds.forEach(id => {
      if (!oldIds.has(id)) {
        questionStore.adjustUsageCount(id, 1)
      }
    })

    // removed
    oldIds.forEach(id => {
      if (!newIds.has(id)) {
        questionStore.adjustUsageCount(id, -1)
      }
    })

    // 2. Save
    quizQuestions.value[quizId] = questions

    // 3. Update summary on quiz
    const quiz = getQuizById(quizId)
    if (quiz) {
      quiz.questionCount = questions.length
      quiz.totalScore = questions.reduce((sum, q) => sum + q.score, 0)
      quiz.multipleChoiceQuestionCount = questions.filter(q => q.questionType === 'multiple_choice').length
      quiz.essayQuestionCount = questions.filter(q => q.questionType === 'essay').length
    }
  }

  return {
    quizzes,
    quizQuestions,
    initialized,
    init,
    reset,
    getQuizById,
    getQuestionsForQuiz,
    addQuiz,
    updateQuiz,
    deleteQuiz,
    updateQuizQuestions
  }
})

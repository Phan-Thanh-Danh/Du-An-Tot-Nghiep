import { defineStore } from 'pinia'
import { ref } from 'vue'
import { contentCouncilApi } from '@/services/contentCouncilApi'
import type { ContentCouncilQuiz, QuizBuilderQuestion } from '@/types/content-council/quiz'
import { useQuestionStore } from './questionStore'


export const useQuizStore = defineStore('contentCouncilQuiz', () => {
  const quizzes = ref<ContentCouncilQuiz[]>([])
  const quizQuestions = ref<Record<number, QuizBuilderQuestion[]>>({})
  const initialized = ref(false)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const questionStore = useQuestionStore()

  async function init() {
    if (initialized.value) return
    loading.value = true
    error.value = null
    try {
      const res = await contentCouncilApi.getQuizzes()
      const data = res?.data ?? res?.items ?? res ?? []
      quizzes.value = Array.isArray(data) ? data : []
      initialized.value = true
    } catch (e: any) {
      error.value = e?.message || 'Không thể tải bài kiểm tra'
    } finally {
      loading.value = false
    }
  }

  function reset() {
    initialized.value = false
    quizzes.value = []
    quizQuestions.value = {}
    error.value = null
    init()
  }

  function getQuizById(id: number) {
    return quizzes.value.find(q => q.id === id)
  }

  function getQuestionsForQuiz(quizId: number) {
    return quizQuestions.value[quizId] || []
  }

  async function addQuiz(q: ContentCouncilQuiz) {
    try {
      if (true) {
        await contentCouncilApi.createQuiz({
          title: q.title,
          subjectId: q.subjectId,
          examType: q.examType,
          totalScore: q.totalScore,
          duration: q.duration,
        })
      }
      quizzes.value.unshift(q)
      quizQuestions.value[q.id] = []
    } catch (e: any) {
      error.value = e?.message || 'Không thể thêm bài kiểm tra'
    }
  }

  async function updateQuiz(id: number, payload: Partial<ContentCouncilQuiz>) {
    try {
      if (true) {
        await contentCouncilApi.updateQuiz(id, payload)
      }
      const idx = quizzes.value.findIndex(q => q.id === id)
      if (idx !== -1) {
        Object.assign(quizzes.value[idx], payload)
        quizzes.value[idx].updatedAt = new Date().toISOString()
      }
    } catch (e: any) {
      error.value = e?.message || 'Không thể cập nhật bài kiểm tra'
    }
  }

  async function deleteQuiz(id: number) {
    const idx = quizzes.value.findIndex(q => q.id === id)
    if (idx === -1) return
    try {
      if (true) {
        await contentCouncilApi.deleteQuiz(id)
      }
      const questions = quizQuestions.value[id] || []
      questions.forEach(q => questionStore.adjustUsageCount(q.questionId, -1))
      quizzes.value.splice(idx, 1)
      delete quizQuestions.value[id]
    } catch (e: any) {
      error.value = e?.message || 'Không thể xóa bài kiểm tra'
    }
  }

  async function updateQuizQuestions(quizId: number, questions: QuizBuilderQuestion[]) {
    const oldQ = quizQuestions.value[quizId] || []
    const oldIds = new Set(oldQ.map(q => q.questionId))
    const newIds = new Set(questions.map(q => q.questionId))

    newIds.forEach(id => {
      if (!oldIds.has(id)) questionStore.adjustUsageCount(id, 1)
    })
    oldIds.forEach(id => {
      if (!newIds.has(id)) questionStore.adjustUsageCount(id, -1)
    })

    try {
      if (true) {
        await contentCouncilApi.assignQuestions(quizId, {
          questionIds: questions.map(q => q.questionId),
        })
      }
      quizQuestions.value[quizId] = questions
      const quiz = getQuizById(quizId)
      if (quiz) {
        quiz.questionCount = questions.length
        quiz.totalScore = questions.reduce((sum, q) => sum + q.score, 0)
        quiz.multipleChoiceQuestionCount = questions.filter(q => q.questionType === 'multiple_choice').length
        quiz.essayQuestionCount = questions.filter(q => q.questionType === 'essay').length
      }
    } catch (e: any) {
      error.value = e?.message || 'Không thể cập nhật câu hỏi cho bài kiểm tra'
    }
  }

  async function publishQuizAction(id: number) {
    try {
      if (true) {
        await contentCouncilApi.publishQuiz(id)
      }
      const q = getQuizById(id)
      if (q) q.status = 'published'
    } catch (e: any) {
      error.value = e?.message || 'Không thể xuất bản'
    }
  }

  async function unpublishQuizAction(id: number) {
    try {
      if (true) {
        await contentCouncilApi.unpublishQuiz(id)
      }
      const q = getQuizById(id)
      if (q) q.status = 'draft'
    } catch (e: any) {
      error.value = e?.message || 'Không thể hủy xuất bản'
    }
  }

  return {
    quizzes,
    quizQuestions,
    initialized,
    loading,
    error,
    init,
    reset,
    getQuizById,
    getQuestionsForQuiz,
    addQuiz,
    updateQuiz,
    deleteQuiz,
    updateQuizQuestions,
    publishQuizAction,
    unpublishQuizAction,
  }
})

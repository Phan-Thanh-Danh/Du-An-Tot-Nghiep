import { defineStore } from 'pinia'
import { ref } from 'vue'
import { initializeQuestionMockData } from '@/mocks/content-council'
import { QuestionBankItem } from '@/types/content-council/questionBank'

export const useQuestionStore = defineStore('contentCouncilQuestion', () => {
  const questions = ref<QuestionBankItem[]>([])
  const initialized = ref(false)

  function init() {
    if (initialized.value) return
    questions.value = initializeQuestionMockData()
    initialized.value = true
  }

  function reset() {
    initialized.value = false
    init()
  }

  function getQuestionsBySubject(subjectId: number) {
    return questions.value.filter(q => q.subjectId === subjectId)
  }

  function getQuestionById(id: number) {
    return questions.value.find(q => q.id === id)
  }

  function addQuestion(q: QuestionBankItem) {
    questions.value.unshift(q)
  }

  function updateQuestion(id: number, payload: Partial<QuestionBankItem>) {
    const idx = questions.value.findIndex(q => q.id === id)
    if (idx !== -1) {
      Object.assign(questions.value[idx], payload)
      questions.value[idx].updatedAt = new Date().toISOString()
    }
  }

  function deleteQuestion(id: number) {
    const idx = questions.value.findIndex(q => q.id === id)
    if (idx !== -1) {
      if (questions.value[idx].usageCount > 0) {
        throw new Error('Không thể xóa câu hỏi đang được sử dụng.')
      }
      questions.value.splice(idx, 1)
    }
  }

  function adjustUsageCount(id: number, delta: number) {
    const q = questions.value.find(q => q.id === id)
    if (q) {
      q.usageCount = Math.max(0, q.usageCount + delta)
    }
  }

  function toggleStatus(id: number) {
    const idx = questions.value.findIndex(q => q.id === id)
    if (idx !== -1) {
      questions.value[idx].status = questions.value[idx].status === 'active' ? 'inactive' : 'active'
    }
  }

  return {
    questions,
    initialized,
    init,
    reset,
    getQuestionsBySubject,
    getQuestionById,
    addQuestion,
    updateQuestion,
    deleteQuestion,
    adjustUsageCount,
    toggleStatus
  }
})

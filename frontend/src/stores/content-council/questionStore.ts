import { defineStore } from 'pinia'
import { ref } from 'vue'
import { contentCouncilApi } from '@/services/contentCouncilApi'
import type { QuestionBankItem } from '@/types/content-council/questionBank'


export const useQuestionStore = defineStore('contentCouncilQuestion', () => {
  const questions = ref<QuestionBankItem[]>([])
  const initialized = ref(false)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function init() {
    if (initialized.value) return
    loading.value = true
    error.value = null
    try {
      const res = await contentCouncilApi.getQuestions()
      const data = res?.data ?? res?.items ?? res ?? []
      questions.value = Array.isArray(data) ? data : []
      initialized.value = true
    } catch (e: any) {
      error.value = e?.message || 'Không thể tải câu hỏi'
    } finally {
      loading.value = false
    }
  }

  function reset() {
    initialized.value = false
    questions.value = []
    error.value = null
    init()
  }

  function getQuestionsBySubject(subjectId: number) {
    return questions.value.filter(q => q.subjectId === subjectId)
  }

  function getQuestionById(id: number) {
    return questions.value.find(q => q.id === id)
  }

  async function addQuestion(q: QuestionBankItem) {
    try {
      if (true) {
        await contentCouncilApi.createQuestion({
          subjectId: q.subjectId,
          type: q.type,
          content: q.content,
          choices: q.choices,
          correctAnswer: q.correctAnswer,
          difficulty: q.difficulty,
        })
      }
      questions.value.unshift(q)
    } catch (e: any) {
      error.value = e?.message || 'Không thể thêm câu hỏi'
    }
  }

  async function updateQuestion(id: number, payload: Partial<QuestionBankItem>) {
    try {
      if (true) {
        await contentCouncilApi.updateQuestion(id, payload)
      }
      const idx = questions.value.findIndex(q => q.id === id)
      if (idx !== -1) {
        Object.assign(questions.value[idx], payload)
        questions.value[idx].updatedAt = new Date().toISOString()
      }
    } catch (e: any) {
      error.value = e?.message || 'Không thể cập nhật câu hỏi'
    }
  }

  async function deleteQuestion(id: number) {
    const idx = questions.value.findIndex(q => q.id === id)
    if (idx === -1) return
    if (questions.value[idx].usageCount > 0) {
      throw new Error('Không thể xóa câu hỏi đang được sử dụng.')
    }
    try {
      if (true) {
        await contentCouncilApi.deleteQuestion(id)
      }
      questions.value.splice(idx, 1)
    } catch (e: any) {
      error.value = e?.message || 'Không thể xóa câu hỏi'
    }
  }

  function adjustUsageCount(id: number, delta: number) {
    const q = questions.value.find(q => q.id === id)
    if (q) {
      q.usageCount = Math.max(0, q.usageCount + delta)
    }
  }

  async function toggleStatus(id: number) {
    const idx = questions.value.findIndex(q => q.id === id)
    if (idx === -1) return
    const newStatus = questions.value[idx].status === 'active' ? 'inactive' : 'active'
    try {
      if (true) {
        if (newStatus === 'active') {
          await contentCouncilApi.activateQuestion(id)
        } else {
          await contentCouncilApi.deactivateQuestion(id)
        }
      }
      questions.value[idx].status = newStatus
    } catch (e: any) {
      error.value = e?.message || 'Không thể đổi trạng thái câu hỏi'
    }
  }

  return {
    questions,
    initialized,
    loading,
    error,
    init,
    reset,
    getQuestionsBySubject,
    getQuestionById,
    addQuestion,
    updateQuestion,
    deleteQuestion,
    adjustUsageCount,
    toggleStatus,
  }
})

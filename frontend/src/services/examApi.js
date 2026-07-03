import { apiRequest } from '@/services/apiClient'

function unwrapApiData(response) {
  return response?.data ?? response?.Data ?? response
}

export const examApi = {
  // ===== Student Exam Taking =====
  getStudentExams() {
    return apiRequest('/api/exam/student/list').then(unwrapApiData)
  },
  getExamSession(maPhienThi) {
    return apiRequest(`/api/exam/taking/session/${maPhienThi}`).then(unwrapApiData)
  },
  getExamQuestions(maPhienThi) {
    return apiRequest(`/api/exam/taking/session/${maPhienThi}/questions`).then(unwrapApiData)
  },
  startExam(payload) {
    return apiRequest('/api/exam/taking/start', {
      method: 'POST',
      body: JSON.stringify(payload),
    }).then(unwrapApiData)
  },
  autoSaveAnswers(payload) {
    return apiRequest('/api/exam/taking/autosave', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },
  submitExam(payload) {
    return apiRequest('/api/exam/taking/submit', {
      method: 'POST',
      body: JSON.stringify(payload),
    }).then(unwrapApiData)
  },

  // ===== Quiz Attempt Bridge =====
  getQuizAvailability(quizId) {
    return apiRequest(`/api/quiz-attempts/${quizId}/availability`).then(unwrapApiData)
  },
  startQuizAttempt(quizId) {
    return apiRequest(`/api/quiz-attempts/${quizId}/start`, {
      method: 'POST',
    }).then(unwrapApiData)
  },
  autoSaveQuizAnswers(attemptId, payload) {
    return apiRequest(`/api/quiz-attempts/sessions/${attemptId}/autosave`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },
  submitQuizAttempt(attemptId, payload) {
    return apiRequest(`/api/quiz-attempts/sessions/${attemptId}/submit`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }).then(unwrapApiData)
  },

  // ===== Teacher Proctoring =====
  getCaThis(params) {
    const query = new URLSearchParams(params).toString()
    return apiRequest(`/api/exam/ca-thi?${query}`).then(unwrapApiData)
  },
  getCaThiById(maCaThi) {
    return apiRequest(`/api/exam/ca-thi/${maCaThi}`).then(unwrapApiData)
  },
  getThiSinhsByCaThi(maCaThi) {
    return apiRequest(`/api/exam/ca-thi/${maCaThi}/thi-sinh`).then(unwrapApiData)
  },
  getViPhamsByCaThi(maCaThi) {
    return apiRequest(`/api/exam/ca-thi/${maCaThi}/vi-pham`).then(unwrapApiData)
  },
  getDiemDanh(maCaThi) {
    return apiRequest(`/api/exam/ca-thi/${maCaThi}/diem-danh`).then(unwrapApiData)
  },
  batchDiemDanh(payload) {
    return apiRequest('/api/exam/ca-thi/diem-danh', {
      method: 'POST',
      body: JSON.stringify(payload),
    }).then(unwrapApiData)
  },
  logViolation(payload) {
    return apiRequest('/api/exam/vi-pham', {
      method: 'POST',
      body: JSON.stringify(payload),
    }).then(unwrapApiData)
  },
  startCaThi(maCaThi) {
    return apiRequest(`/api/exam/ca-thi/${maCaThi}/start`, {
      method: 'POST',
    }).then(unwrapApiData)
  }
}

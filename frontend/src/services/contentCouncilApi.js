import { apiRequest } from './apiClient'

export const contentCouncilApi = {
  // Subjects
  getSubjects(params = {}) {
    const query = new URLSearchParams()
    if (params.keyword) query.append('keyword', params.keyword)
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    if (params.majorId) query.append('maNganh', params.majorId)
    if (params.specializationId) query.append('maChuyenNganh', params.specializationId)
    const qs = query.toString()
    return apiRequest(`/api/master-data/subjects${qs ? '?' + qs : ''}`)
  },

  getMajors(params = {}) {
    const query = new URLSearchParams()
    if (params.pageSize) query.append('pageSize', params.pageSize)
    else query.append('pageSize', '100') // get all
    const qs = query.toString()
    return apiRequest(`/api/master-data/majors${qs ? '?' + qs : ''}`)
  },

  getSpecializations(params = {}) {
    const query = new URLSearchParams()
    if (params.majorId) query.append('maNganh', params.majorId)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    else query.append('pageSize', '100') // get all
    const qs = query.toString()
    return apiRequest(`/api/master-data/specializations${qs ? '?' + qs : ''}`)
  },

  getSubjectById(id) {
    return apiRequest(`/api/master-data/subjects/${id}`)
  },

  createSubject(payload) {
    return apiRequest('/api/master-data/subjects', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  updateSubject(id, payload) {
    return apiRequest(`/api/master-data/subjects/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  deleteSubject(id) {
    return apiRequest(`/api/master-data/subjects/${id}`, { method: 'DELETE' })
  },

  // Curriculum chapters
  getChapters(subjectId) {
    return apiRequest(`/api/curriculum/subjects/${subjectId}/chapters`)
  },

  getChapter(id) {
    return apiRequest(`/api/curriculum/chapters/${id}`)
  },

  createChapter(payload) {
    return apiRequest('/api/curriculum/chapters', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  updateChapter(id, payload) {
    return apiRequest(`/api/curriculum/chapters/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  deleteChapter(id) {
    return apiRequest(`/api/curriculum/chapters/${id}`, { method: 'DELETE' })
  },

  reorderChapters(subjectId, payload) {
    return apiRequest(`/api/curriculum/subjects/${subjectId}/chapters/reorder`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  // Lessons
  getLessons(chapterId) {
    return apiRequest(`/api/curriculum/chapters/${chapterId}/lessons`)
  },

  getLesson(id) {
    return apiRequest(`/api/curriculum/lessons/${id}`)
  },

  createLesson(payload) {
    return apiRequest('/api/curriculum/lessons', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  updateLesson(id, payload) {
    return apiRequest(`/api/curriculum/lessons/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  deleteLesson(id) {
    return apiRequest(`/api/curriculum/lessons/${id}`, { method: 'DELETE' })
  },

  reorderLessons(chapterId, payload) {
    return apiRequest(`/api/curriculum/chapters/${chapterId}/lessons/reorder`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  // Content blocks
  getLessonContent(lessonId) {
    return apiRequest(`/api/curriculum/lessons/${lessonId}/content`)
  },

  getContentById(id) {
    return apiRequest(`/api/curriculum/content/${id}`)
  },

  createContent(payload) {
    return apiRequest('/api/curriculum/content', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  updateContent(id, payload) {
    return apiRequest(`/api/curriculum/content/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  deleteContent(id) {
    return apiRequest(`/api/curriculum/content/${id}`, { method: 'DELETE' })
  },

  reorderContent(lessonId, payload) {
    return apiRequest(`/api/curriculum/lessons/${lessonId}/content/reorder`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  // Question bank
  getQuestions(params = {}) {
    const query = new URLSearchParams()
    if (params.subjectId) query.append('subjectId', params.subjectId)
    if (params.keyword) query.append('keyword', params.keyword)
    if (params.status) query.append('status', params.status)
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return apiRequest(`/api/question-bank/questions${qs ? '?' + qs : ''}`)
  },

  getQuestionById(id) {
    return apiRequest(`/api/question-bank/questions/${id}`)
  },

  createQuestion(payload) {
    return apiRequest('/api/question-bank/questions', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  updateQuestion(id, payload) {
    return apiRequest(`/api/question-bank/questions/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  deleteQuestion(id) {
    return apiRequest(`/api/question-bank/questions/${id}`, { method: 'DELETE' })
  },

  activateQuestion(id) {
    return apiRequest(`/api/question-bank/questions/${id}/activate`, { method: 'PATCH' })
  },

  deactivateQuestion(id) {
    return apiRequest(`/api/question-bank/questions/${id}/deactivate`, { method: 'PATCH' })
  },

  importQuestions(formData) {
    return apiRequest('/api/question-bank/questions/import', {
      method: 'POST',
      body: formData,
    })
  },

  // Quizzes
  getQuizzes(params = {}) {
    const query = new URLSearchParams()
    if (params.subjectId) query.append('subjectId', params.subjectId)
    if (params.keyword) query.append('keyword', params.keyword)
    if (params.status) query.append('status', params.status)
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return apiRequest(`/api/exam/de-kiem-tra/search${qs ? '?' + qs : ''}`)
  },

  getQuizById(id) {
    return apiRequest(`/api/exam/de-kiem-tra/${id}`)
  },

  createQuiz(payload) {
    return apiRequest('/api/exam/de-kiem-tra', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  updateQuiz(id, payload) {
    return apiRequest(`/api/exam/de-kiem-tra/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  deleteQuiz(id) {
    return apiRequest(`/api/exam/de-kiem-tra/${id}`, { method: 'DELETE' })
  },

  getQuizQuestions(quizId) {
    return apiRequest(`/api/exam/de-kiem-tra/${quizId}/cau-hoi`)
  },

  assignQuestions(quizId, payload) {
    return apiRequest(`/api/exam/de-kiem-tra/${quizId}/cau-hoi`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  replaceQuestions(quizId, payload) {
    return apiRequest(`/api/exam/de-kiem-tra/${quizId}/cau-hoi`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  reorderQuizQuestions(quizId, payload) {
    return apiRequest(`/api/exam/de-kiem-tra/${quizId}/cau-hoi/reorder`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  removeQuizQuestion(quizId, questionId) {
    return apiRequest(`/api/exam/de-kiem-tra/${quizId}/cau-hoi/${questionId}`, {
      method: 'DELETE',
    })
  },

  publishQuiz(id) {
    return apiRequest(`/api/exam/de-kiem-tra/${id}/publish`, { method: 'POST' })
  },

  unpublishQuiz(id) {
    return apiRequest(`/api/exam/de-kiem-tra/${id}/unpublish`, { method: 'POST' })
  },

  openQuiz(id) {
    return apiRequest(`/api/exam/de-kiem-tra/${id}/open`, { method: 'POST' })
  },

  closeQuiz(id) {
    return apiRequest(`/api/exam/de-kiem-tra/${id}/close`, { method: 'POST' })
  },

  // Academic terms (for select options)
  getAcademicTerms() {
    return apiRequest('/api/master-data/academic-terms')
  },
}

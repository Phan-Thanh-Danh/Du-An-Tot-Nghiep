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

  getAcademicTerms(params = {}) {
    const query = new URLSearchParams()
    if (params.pageSize) query.append('pageSize', params.pageSize)
    else query.append('pageSize', '100')
    const qs = query.toString()
    return apiRequest(`/api/master-data/academic-terms${qs ? '?' + qs : ''}`)
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

  publishSubject(subjectId) {
    return apiRequest(`/api/curriculum/subjects/${subjectId}/publish`, { method: 'POST' })
  },

  unpublishSubject(subjectId) {
    return apiRequest(`/api/curriculum/subjects/${subjectId}/unpublish`, { method: 'POST' })
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
    if (params.subjectId && params.subjectId !== 'all') query.append('maMonHoc', params.subjectId)
    if (params.maMonHoc && params.maMonHoc !== 'all') query.append('maMonHoc', params.maMonHoc)
    if (params.keyword) query.append('keyword', params.keyword)
    if (params.questionType && params.questionType !== 'all') query.append('loaiCauHoi', params.questionType)
    if (params.loaiCauHoi && params.loaiCauHoi !== 'all') query.append('loaiCauHoi', params.loaiCauHoi)
    if (params.selectionType && params.selectionType !== 'all') query.append('kieuLuaChon', params.selectionType)
    if (params.kieuLuaChon && params.kieuLuaChon !== 'all') query.append('kieuLuaChon', params.kieuLuaChon)
    if (params.difficulty && params.difficulty !== 'all') query.append('doKho', params.difficulty)
    if (params.doKho && params.doKho !== 'all') query.append('doKho', params.doKho)
    if (params.status && params.status !== 'all') query.append('conHoatDong', params.status === 'active' ? 'true' : 'false')
    if (params.conHoatDong !== undefined) query.append('conHoatDong', String(params.conHoatDong))
    if (params.pageIndex || params.pageNumber) query.append('pageNumber', params.pageIndex || params.pageNumber)
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
    if (params.subjectId && params.subjectId !== 'all') query.append('maMonHoc', params.subjectId)
    if (params.maMonHoc && params.maMonHoc !== 'all') query.append('maMonHoc', params.maMonHoc)
    if (params.keyword) query.append('keyword', params.keyword)
    if (params.status && params.status !== 'all') query.append('trangThai', params.status)
    if (params.trangThai && params.trangThai !== 'all') query.append('trangThai', params.trangThai)
    if (params.pageIndex || params.pageNumber) query.append('pageNumber', params.pageIndex || params.pageNumber)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    else query.append('pageSize', '100')
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

  validateQuiz(id) {
    return apiRequest(`/api/exam/de-kiem-tra/${id}/validate`, { method: 'POST' })
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
}

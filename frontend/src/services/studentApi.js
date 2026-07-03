import { apiRequest } from './apiClient'

export const studentApi = {
  getDashboard() {
    return apiRequest('/api/student/dashboard', {
      method: 'GET',
    })
  },
  getCourses() {
    return apiRequest('/api/student/courses', {
      method: 'GET',
    })
  },
  getCourseDetail(courseId) {
    return apiRequest(`/api/student/courses/${courseId}`, {
      method: 'GET',
    })
  },
  getLessonQuiz(courseId, lessonId) {
    return apiRequest(`/api/student/courses/${courseId}/lessons/${lessonId}/quiz`, {
      method: 'GET',
    })
  },
  getLessonComments(courseId, lessonId) {
    return apiRequest(`/api/student/courses/${courseId}/lessons/${lessonId}/comments`, {
      method: 'GET',
    })
  },
  getStudentCurriculum() {
    return apiRequest('/api/student/curriculum', {
      method: 'GET',
    })
  },
  getAssignments() {
    return apiRequest('/api/student/assignments', {
      method: 'GET',
    })
  },
  getAssignmentDetail(assignmentId) {
    return apiRequest(`/api/student/assignments/${assignmentId}`, {
      method: 'GET',
    })
  },
  submitAssignment(assignmentId, formData) {
    return apiRequest(`/api/student/assignments/${assignmentId}/submit`, {
      method: 'POST',
      body: formData,
    })
  },

  getDisciplineRecords() {
    return apiRequest('/api/student/discipline-records')
  },

  getDisciplineRecord(id) {
    return apiRequest(`/api/student/discipline-records/${id}`)
  },

  submitAppeal(id, payload) {
    return apiRequest(`/api/student/discipline-records/${id}/appeals`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  getAppeal(appealId) {
    return apiRequest(`/api/student/discipline-records/appeals/${appealId}`)
  },

  getAttendance() {
    return apiRequest('/api/student/attendance')
  },

  getSchedule(params = {}) {
    const query = new URLSearchParams()
    if (params.tuan) query.append('tuan', params.tuan)
    if (params.maLop) query.append('maLop', params.maLop)
    const qs = query.toString()
    return apiRequest(`/api/thoi-khoa-bieu${qs ? '?' + qs : ''}`)
  },

  getProfile() {
    return apiRequest('/api/student/profile')
  },

  updateProfile(payload) {
    return apiRequest('/api/student/profile', {
      method: 'PATCH',
      body: JSON.stringify(payload),
    })
  },

  getGrades() {
    return apiRequest('/api/student/grades')
  },

  getEvaluations() {
    return apiRequest('/api/student/evaluations')
  },

  submitEvaluation(id, payload) {
    return apiRequest(`/api/student/evaluations/${id}/submit`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },
}

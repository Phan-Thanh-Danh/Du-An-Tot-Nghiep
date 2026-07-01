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
  }
}

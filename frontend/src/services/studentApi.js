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

  // MISSING_BACKEND: no controller implemented yet, uses mock fallback
  async getProfile() {
    try {
      return await apiRequest('/api/student/profile')
    } catch {
      return {
        success: true,
        data: {
          name: 'Nguyễn Văn A',
          code: 'SV001',
          className: 'SE1501',
          semester: 'HK2 2025-2026',
          email: 'nguyenvana@example.com',
          phone: '0901234567',
        },
      }
    }
  },

  // MISSING_BACKEND: no controller implemented yet, uses mock fallback
  async updateProfile(payload) {
    try {
      return await apiRequest('/api/student/profile', {
        method: 'PATCH',
        body: JSON.stringify(payload),
      })
    } catch {
      return { success: true, message: 'Cập nhật thành công (mock).' }
    }
  },

  // MISSING_BACKEND: no controller implemented yet, uses mock fallback
  async getGrades() {
    try {
      return await apiRequest('/api/student/grades')
    } catch {
      return {
        success: true,
        data: [
          { id: '1', course: 'Cấu trúc dữ liệu & Giải thuật', code: 'CTDL101', examType: 'Giữa kỳ', score: 8.5, date: '15/06/2026', status: 'Đã công bố' },
          { id: '2', course: 'Lập trình Web', code: 'LTW301', examType: 'Cuối kỳ', score: 7.2, date: '20/06/2026', status: 'Đã công bố' },
        ],
      }
    }
  },

  // MISSING_BACKEND: no controller implemented yet, uses mock fallback
  async getEvaluations() {
    try {
      return await apiRequest('/api/student/evaluations')
    } catch {
      return {
        success: true,
        data: [
          { id: '1', course: 'CTDL101', title: 'Đánh giá môn Cấu trúc dữ liệu', deadline: '30/07/2026', status: 'Chưa đánh giá' },
        ],
      }
    }
  },

  // MISSING_BACKEND: no controller implemented yet, uses mock fallback
  async submitEvaluation(id, payload) {
    try {
      return await apiRequest(`/api/student/evaluations/${id}/submit`, {
        method: 'POST',
        body: JSON.stringify(payload),
      })
    } catch {
      return { success: true, message: 'Đã gửi đánh giá (mock).' }
    }
  },
}

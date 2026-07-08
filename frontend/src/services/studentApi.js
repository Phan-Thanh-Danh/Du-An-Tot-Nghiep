import { apiRequest, unwrapApiData } from './apiClient'

function buildQuery(params = {}) {
  const query = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') query.append(key, value)
  })
  const qs = query.toString()
  return qs ? `?${qs}` : ''
}

function mapAccountProfile(account) {
  const data = unwrapApiData(account) || {}
  return {
    success: true,
    data: {
      id: data.id ?? data.Id,
      fullName: data.hoTen ?? data.HoTen ?? data.fullName ?? '',
      name: data.hoTen ?? data.HoTen ?? data.fullName ?? '',
      code: data.id ? `USER-${data.id}` : '',
      studentId: data.id ? `USER-${data.id}` : '',
      className: 'Chưa có dữ liệu học vụ',
      semester: 'Chưa có dữ liệu học vụ',
      major: 'Chưa có dữ liệu học vụ',
      campus: 'Chưa có dữ liệu học vụ',
      email: data.email ?? data.Email ?? '',
      phone: data.soDienThoai ?? data.SoDienThoai ?? '',
      address: '',
      status: data.trangThai ?? data.TrangThai ?? '',
      role: data.vaiTroChinh ?? data.VaiTroChinh ?? '',
    },
  }
}

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
  getSubmissions() {
    return apiRequest('/api/student/submissions', {
      method: 'GET',
    })
  },
  getSubmissionDetail(submissionId) {
    return apiRequest(`/api/student/submissions/${submissionId}`, {
      method: 'GET',
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

  async getProfile() {
    const response = await apiRequest('/api/account/me')
    return mapAccountProfile(response)
  },

  async updateProfile(payload) {
    const response = await apiRequest('/api/account/profile', {
      method: 'PUT',
      body: JSON.stringify({
        email: payload.email,
        hoTen: payload.fullName ?? payload.hoTen,
        soDienThoai: payload.phone ?? payload.soDienThoai,
      }),
    })
    return mapAccountProfile(response)
  },

  getGrades() {
    return apiRequest('/api/student/grades', { method: 'GET' })
  },

  getEvaluations() {
    return apiRequest('/api/student/evaluations', { method: 'GET' })
  },

  getEvaluationDetail(evaluationId) {
    return apiRequest(`/api/student/evaluations/${evaluationId}`, { method: 'GET' })
  },

  submitEvaluation(id, payload) {
    return apiRequest('/api/student/evaluations/submit', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  getRewards(params = {}) {
    return apiRequest(`/api/student/rewards${buildQuery(params)}`)
  },

  getRewardDetail(rewardId) {
    return apiRequest(`/api/student/rewards/${rewardId}`)
  },

  async downloadRewardCertificate(rewardId) {
    const token = localStorage.getItem('lms_access_token') || sessionStorage.getItem('lms_access_token') || ''
    const baseUrl = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '')
    const response = await fetch(`${baseUrl}/api/student/rewards/${rewardId}/certificate/download`, {
      method: 'GET',
      headers: token ? { Authorization: `Bearer ${token}` } : {},
    })

    if (!response.ok) {
      let message = 'Không thể tải bằng khen.'
      try {
        const data = await response.json()
        message = data?.message || data?.Message || data?.title || message
      } catch {
        // Response may be plain text or empty.
      }
      throw new Error(message)
    }

    return response.blob()
  },

  getSupportTickets(params = {}) {
    return apiRequest(`/api/student/support-tickets${buildQuery(params)}`)
  },

  getSupportTicketDetail(ticketId) {
    return apiRequest(`/api/student/support-tickets/${ticketId}`)
  },

  createSupportTicket(payload) {
    return apiRequest('/api/student/support-tickets', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  sendSupportTicketMessage(ticketId, payload) {
    return apiRequest(`/api/student/support-tickets/${ticketId}/messages`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  closeSupportTicket(ticketId, payload = {}) {
    return apiRequest(`/api/student/support-tickets/${ticketId}/close`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  async changePassword(payload) {
    return apiRequest('/api/account/change-password', {
      method: 'PUT',
      body: JSON.stringify({
        currentPassword: payload.currentPassword,
        newPassword: payload.newPassword,
        confirmPassword: payload.confirmPassword,
      }),
    })
  },

  inviteParent(payload) {
    return apiRequest('/api/student/parent-links/invite', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  updateParentPermission(linkId, key, value) {
    return apiRequest(`/api/student/parent-links/${linkId}/permissions`, {
      method: 'PUT',
      body: JSON.stringify({ key, value }),
    })
  },

  removeParentLink(linkId) {
    return apiRequest(`/api/student/parent-links/${linkId}`, {
      method: 'DELETE',
    })
  },

  getSupportTickets() {
    return apiRequest('/api/student/support-tickets', { method: 'GET' })
  },

  getSupportTicketDetail(ticketId) {
    return apiRequest(`/api/student/support-tickets/${ticketId}`, { method: 'GET' })
  },

  createSupportTicket(payload) {
    return apiRequest('/api/student/support-tickets', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  sendSupportTicketMessage(ticketId, payload) {
    return apiRequest(`/api/student/support-tickets/${ticketId}/messages`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  closeSupportTicket(ticketId, payload) {
    return apiRequest(`/api/student/support-tickets/${ticketId}/close`, {
      method: 'POST',
      body: payload ? JSON.stringify(payload) : undefined,
    })
  },
}

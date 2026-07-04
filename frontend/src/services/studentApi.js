import { apiRequest, unwrapApiData } from './apiClient'

const enableMock = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'

function buildQuery(params = {}) {
  const query = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') query.append(key, value)
  })
  const qs = query.toString()
  return qs ? `?${qs}` : ''
}

function unavailable(feature) {
  throw new Error(`${feature} chưa có backend endpoint. Chức năng đang phát triển.`)
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

  async getGrades() {
    if (enableMock) {
      return {
        success: true,
        data: [
          { id: '1', course: 'Cấu trúc dữ liệu & Giải thuật', code: 'CTDL101', examType: 'Giữa kỳ', score: 8.5, date: '15/06/2026', status: 'Đã công bố' },
          { id: '2', course: 'Lập trình Web', code: 'LTW301', examType: 'Cuối kỳ', score: 7.2, date: '20/06/2026', status: 'Đã công bố' },
        ],
      }
    }
    unavailable('Bảng điểm sinh viên')
  },

  async getEvaluations() {
    if (enableMock) {
      return {
        success: true,
        data: [
          { id: '1', course: 'CTDL101', title: 'Đánh giá môn Cấu trúc dữ liệu', deadline: '30/07/2026', status: 'Chưa đánh giá' },
        ],
      }
    }
    unavailable('Đánh giá giảng viên của sinh viên')
  },

  async submitEvaluation(id, payload) {
    if (enableMock) {
      return {
        success: true,
        message: 'Đã gửi đánh giá (DEV mock).',
        data: { id, ...payload },
      }
    }
    unavailable('Gửi đánh giá giảng viên')
  },

  async getRewards(params = {}) {
    return apiRequest(`/api/student/rewards${buildQuery(params)}`)
  },

  async getRewardDetail(rewardId) {
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

  async inviteParent() {
    if (enableMock) {
      return { success: true, message: 'Đã gửi lời mời phụ huynh (DEV mock).' }
    }
    unavailable('Liên kết phụ huynh')
  },

  async updateParentPermission() {
    if (enableMock) {
      return { success: true, message: 'Đã cập nhật quyền phụ huynh (DEV mock).' }
    }
    unavailable('Phân quyền phụ huynh')
  },

  async removeParentLink() {
    if (enableMock) {
      return { success: true, message: 'Đã hủy liên kết phụ huynh (DEV mock).' }
    }
    unavailable('Hủy liên kết phụ huynh')
  },

}

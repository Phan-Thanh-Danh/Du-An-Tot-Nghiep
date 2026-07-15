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
      className: data.className ?? data.ClassName ?? 'Chưa có dữ liệu học vụ',
      semester: data.semester ?? data.Semester ?? 'Chưa có dữ liệu học vụ',
      major: data.major ?? data.Major ?? 'Chưa có dữ liệu học vụ',
      campus: data.campus ?? data.Campus ?? 'Chưa có dữ liệu học vụ',
      email: data.email ?? data.Email ?? '',
      phone: data.soDienThoai ?? data.SoDienThoai ?? '',
      address: '',
      status: data.trangThai ?? data.TrangThai ?? '',
      role: data.vaiTroChinh ?? data.VaiTroChinh ?? '',
    },
  }
}

function mapSessionDto(item) {
  return {
    id: item.maBuoiHoc || item.MaBuoiHoc,
    courseId: item.maKhoaHoc || item.MaKhoaHoc,
    subject: item.tenMonHoc || item.TenMonHoc || '',
    courseCode: item.maCodeMonHoc || item.MaCodeMonHoc || '',
    room: item.tenPhong || item.TenPhong || '',
    teacher: item.tenGiaoVien || item.TenGiaoVien || '',
    startAt: item.ngayHoc || item.NgayHoc,
    status: item.trangThaiBuoi || item.TrangThaiBuoi || '',
    attendanceStatus: item.trangThaiDiemDanh || item.TrangThaiDiemDanh || '',
    shift: {
      start: item.gioBatDau || item.GioBatDau,
      end: item.gioKetThuc || item.GioKetThuc,
    },
    isSubstitute: item.isSubstitute || item.IsSubstitute || false,
    changeType: item.changeType || item.ChangeType || '',
    changeMessage: item.changeMessage || item.ChangeMessage || ''
  }
}

export const studentApi = {
  async getDashboard() {
    const raw = await apiRequest('/api/student/dashboard', {
      method: 'GET',
    })
    const data = unwrapApiData(raw)
    
    if (data && data.schedule) {
      data.schedule = data.schedule.map(s => {
        const mapped = mapSessionDto(s)
        return {
          ...mapped,
          time: `${mapped.shift.start?.slice(0,5)} - ${mapped.shift.end?.slice(0,5)}`,
          lecturer: mapped.teacher,
          current: false, // You would add logic to determine if current time is within shift
          variant: mapped.status === 'da_huy' ? 'danger' : 'primary'
        }
      })
    }
    
    return { success: true, data }
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

  async getAttendance(params = {}) {
    const defaultParams = { pageIndex: 1, pageSize: 1000 }
    const query = new URLSearchParams({ ...defaultParams, ...params }).toString()
    const raw = await apiRequest(`/api/student/attendance?${query}`)
    const data = unwrapApiData(raw) || {}
    
    const items = data.items || []
    
    const history = items.map(dto => ({
      id: dto.maDiemDanh || Math.random(),
      subject: dto.tenMonHoc || '',
      courseCode: dto.tieuDeKhoaHoc || '',
      shift: { 
        label: dto.tenCa || '', 
        start: dto.gioBatDau || '', 
        end: dto.gioKetThuc || '' 
      },
      room: dto.tenPhong || '',
      status: dto.trangThai || 'chua_diem_danh',
      note: '',
      attendedAt: dto.ngayHoc || new Date().toISOString()
    }))

    // Tự động tính toán subjectStats từ history
    const statsMap = {}
    history.forEach(item => {
      if (!statsMap[item.courseCode]) {
        statsMap[item.courseCode] = {
          courseId: item.courseCode,
          subject: item.subject,
          total: 0,
          absent: 0
        }
      }
      const stat = statsMap[item.courseCode]
      if (item.status !== 'chua_diem_danh') {
        stat.total++
        if (item.status === 'vang') stat.absent++
      }
    })

    const subjectStats = Object.values(statsMap).map(stat => ({
      ...stat,
      rate: stat.total > 0 ? Math.round(((stat.total - stat.absent) / stat.total) * 100) : 100
    }))

    return { history, subjectStats }
  },

  async getScheduleSummary() {
    return unwrapApiData(await apiRequest('/api/student/schedule/summary'))
  },

  async getTodaySchedule() {
    const raw = await apiRequest('/api/student/schedule/today')
    const data = unwrapApiData(raw) || []
    return data.map(mapSessionDto)
  },

  async getScheduleTerms() {
    return unwrapApiData(await apiRequest('/api/student/schedule/terms'))
  },

  async getSchedule(params = {}) {
    const query = new URLSearchParams()
    if (params.ngayTu) query.append('ngayTu', params.ngayTu)
    if (params.ngayDen) query.append('ngayDen', params.ngayDen)
    if (params.maHocKy) query.append('maHocKy', params.maHocKy)
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    
    const raw = await apiRequest(`/api/student/schedule${qs ? '?' + qs : ''}`)
    const data = unwrapApiData(raw)
    const items = data?.items || data?.Items || []
    return {
      sessions: items.map(mapSessionDto),
      totalItems: data?.totalItems || data?.TotalItems || 0,
      pageIndex: data?.pageIndex || data?.PageIndex || 1,
    }
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
}

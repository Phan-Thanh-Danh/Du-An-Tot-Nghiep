import { apiRequest, unwrapApiData } from './apiClient'

/**
 * Teacher API service
 *
 * Backend endpoint status:
 *   √ = controller exists, Teacher authorized
 *   ! = controller exists, policy MAY exclude Teacher
 *   × = MISSING_BACKEND — no controller
 *   + = CREATED in P2 (TeacherDashboard/Submissions)
 */
export const teacherApi = {
  // × MISSING_BACKEND: No TeacherDashboardController exists.
  // For now the FE calls getAttendanceToday + getSchedule to approximate.
  getDashboard() {
    return apiRequest('/api/teacher/dashboard')
  },

  // √ GET /api/teacher/attendance/today — AttendanceController
  getAttendanceToday() {
    return apiRequest('/api/teacher/attendance/today')
  },

  // √ GET /api/teacher/attendance/unlock-requests — AttendanceUnlockController
  getUnlockRequests() {
    return apiRequest('/api/teacher/attendance/unlock-requests')
  },

  // √ GET /api/courses — CoursesController, Teacher scoped to own courses
  // Returns PagedResult<KhoaHocDto> with fields: maKhoaHoc, tenLop, tenMonHoc, tieuDe, tenHocKy, tenGiaoVien
  getClasses(params = {}) {
    const query = new URLSearchParams()
    if (params.keyword) query.append('keyword', params.keyword)
    if (params.semesterId) query.append('maHocKy', params.semesterId)
    const qs = query.toString()
    return apiRequest(`/api/courses${qs ? '?' + qs : ''}`)
  },

  // √ GET /api/courses/{id} — CoursesController, Teacher authorized
  getClassById(id) {
    return apiRequest(`/api/courses/${id}`)
  },

  // ── Attendance (session-level) ──
  // √ These all exist in AttendanceController

  startAttendanceSession(sessionId) {
    return apiRequest(`/api/buoi-hoc/${sessionId}/attendance/start`, {
      method: 'POST',
    })
  },

  // √ GET /api/buoi-hoc/{buoiHocId}/attendance
  getAttendanceSession(buoiHocId) {
    return apiRequest(`/api/buoi-hoc/${buoiHocId}/attendance`)
  },

  // √ PATCH /api/buoi-hoc/{id}/attendance/{maSinhVien}
  updateAttendanceStudent(sessionId, maSinhVien, payload) {
    return apiRequest(`/api/buoi-hoc/${sessionId}/attendance/${maSinhVien}`, {
      method: 'PATCH',
      body: JSON.stringify(payload),
    })
  },

  // √ PUT /api/buoi-hoc/{id}/attendance/bulk
  bulkUpdateAttendance(sessionId, payload) {
    return apiRequest(`/api/buoi-hoc/${sessionId}/attendance/bulk`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  // √ POST /api/buoi-hoc/{id}/attendance/submit
  submitAttendance(sessionId) {
    return apiRequest(`/api/buoi-hoc/${sessionId}/attendance/submit`, {
      method: 'POST',
    })
  },

  // ── Exams (ca-thi) ──
  // √ Teacher authorized on ExamController

  getExams(params = {}) {
    const query = new URLSearchParams()
    if (params.semesterId) query.append('semesterId', params.semesterId)
    if (params.keyword) query.append('keyword', params.keyword)
    const qs = query.toString()
    return apiRequest(`/api/exam/ca-thi${qs ? '?' + qs : ''}`)
  },

  getExamDetail(id) {
    return apiRequest(`/api/exam/ca-thi/${id}`)
  },

  getExamStudents(examId) {
    return apiRequest(`/api/exam/ca-thi/${examId}/thi-sinh`)
  },

  getExamAttendance(examId) {
    return apiRequest(`/api/exam/ca-thi/${examId}/diem-danh`)
  },

  batchExamAttendance(payload) {
    return apiRequest('/api/exam/ca-thi/diem-danh', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  getExamViolations(examId) {
    return apiRequest(`/api/exam/ca-thi/${examId}/vi-pham`)
  },

  createViolation(payload) {
    return apiRequest('/api/exam/vi-pham', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  getBienBan(examId) {
    return apiRequest(`/api/exam/ca-thi/${examId}/bien-ban`)
  },

  createBienBan(payload) {
    return apiRequest('/api/exam/bien-ban', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  // ! GET /api/exam/reports/summary — policy Reports, Teacher MAY be excluded
  getExamReports(params = {}) {
    const query = new URLSearchParams()
    if (params.maCaThi) query.append('maCaThi', params.maCaThi)
    const qs = query.toString()
    return apiRequest(`/api/exam/reports/summary${qs ? '?' + qs : ''}`)
  },

  // ! GET /api/thoi-khoa-bieu — policy AcademicOperations
  async getSchedule(params = {}) {
    const query = new URLSearchParams()
    if (params.tuan) query.append('tuan', params.tuan)
    if (params.maLop) query.append('maLop', params.maLop)
    const qs = query.toString()
    return unwrapApiData(await apiRequest(`/api/thoi-khoa-bieu${qs ? '?' + qs : ''}`))
  },

  // √ TeacherSubmissionsController
  getSubmissions(params = {}) {
    const query = new URLSearchParams()
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return apiRequest(`/api/teacher/submissions${qs ? '?' + qs : ''}`)
  },

  getSubmissionDetail(id) {
    return apiRequest(`/api/teacher/submissions/${id}`)
  },

  gradeSubmission(id, payload) {
    return apiRequest(`/api/teacher/submissions/${id}/grade`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },
}

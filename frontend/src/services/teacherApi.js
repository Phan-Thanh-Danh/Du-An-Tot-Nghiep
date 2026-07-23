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
  // + P2: TeacherDashboardController created
  // P23: Teacher Schedule & Dashboard Today
  async getDashboard() {
    return unwrapApiData(await apiRequest('/api/teacher/dashboard'))
  },

  async getScheduleSummary() {
    return unwrapApiData(await apiRequest('/api/teacher/schedule/summary'))
  },

  async getTodaySchedule() {
    return unwrapApiData(await apiRequest('/api/teacher/schedule/today'))
  },

  async getScheduleTerms() {
    return unwrapApiData(await apiRequest('/api/teacher/schedule/terms'))
  },

  // √ GET /api/teacher/attendance/today — AttendanceController
  getAttendanceToday() {
    return apiRequest('/api/teacher/attendance/today')
  },

  getTeacherClassAttendance(classId) {
    return apiRequest(`/api/teacher/classes/${classId}/attendance`)
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

  async getExams(params = {}) {
    const query = new URLSearchParams()
    if (params.semesterId) query.append('semesterId', params.semesterId)
    if (params.keyword) query.append('keyword', params.keyword)
    const qs = query.toString()
    const rawRes = await apiRequest(`/api/exam/ca-thi${qs ? '?' + qs : ''}`)
    const res = unwrapApiData(rawRes)
    let items = Array.isArray(res) ? res : (res?.items || [])
    
    return items.filter(c => c.trangThai !== 'da_huy').map(c => {
      let status = 'scheduled'
      if (c.trangThai === 'dang_diem_danh') status = 'attendance'
      if (c.trangThai === 'dang_thi') status = 'monitoring'
      if (c.trangThai === 'da_ket_thuc') status = 'ended'
      
      let classCode = c.tenCaThi
      let subjectCode = 'Môn thi'
      let examTitle = c.tenCaThi
      
      const parts = c.tenCaThi.split(' - ')
      if (parts.length >= 3) {
        classCode = parts[0].trim()
        subjectCode = parts[1].trim()
        examTitle = parts.slice(2).join(' - ').trim()
      }

      return {
        id: c.maCaThi,
        subjectCode: subjectCode,
        classCode: classCode,
        examTitle: examTitle,
        room: c.tenPhong || 'Chưa xếp phòng',
        startTime: c.thoiGianBatDau,
        endTime: c.thoiGianKetThuc,
        status: status,
        totalStudents: c.soThiSinh || 0
      }
    })
  },

  getExamDetail(id) {
    return apiRequest(`/api/exam/ca-thi/${id}`)
  },

  startExamSession(id) {
    return apiRequest(`/api/exam/ca-thi/${id}/start`, { method: 'POST' })
  },

  async getExamStudents(examId) {
    const rawRes = await apiRequest(`/api/exam/ca-thi/${examId}/thi-sinh`)
    const res = unwrapApiData(rawRes)
    let items = Array.isArray(res) ? res : (res?.items || [])
    
    return items.map(c => {
      let examStatus = 'not_started'
      if (c.trangThaiDuThi === 'dang_thi') examStatus = 'in_progress'
      if (c.trangThaiDuThi === 'da_nop') examStatus = 'submitted'
      if (c.trangThaiDuThi === 'dinh_chi') examStatus = 'suspended'
      
      return {
        id: c.maThiSinhCaThi || c.maHocSinh,
        studentId: c.maHocSinh,
        studentCode: (c.email || c.maHocSinh || '').toString().split('@')[0],
        name: c.tenHocSinh || 'Thí sinh',
        attendanceStatus: 'present', // Assume present for proctoring mockup if they are in the list
        preflightStatus: 'pass',
        streamStatus: examStatus === 'in_progress' ? 'streaming' : (examStatus === 'submitted' ? 'stopped' : 'waiting'),
        examStatus: examStatus,
        logs: []
      }
    })
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

  async getExamViolations(examId) {
    const rawRes = await apiRequest(`/api/exam/ca-thi/${examId}/vi-pham`)
    const res = unwrapApiData(rawRes)
    let items = Array.isArray(res) ? res : (res?.items || [])
    return items.map(v => ({
      id: v.maViPham,
      studentId: v.maHocSinh,
      studentCode: (v.tenHocSinh || '').toString().split('@')[0], // Simplified
      type: v.loaiViPham,
      severity: v.mucDo === 'dinh_chi' ? 'critical' : (v.mucDo === 'canh_cao' ? 'high' : 'low'),
      timestamp: v.thoiDiem,
      message: v.chiTietJson,
      handled: v.soLanXuLy > 0
    }))
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

  // √ POST /api/teacher/exams — TeacherExamController, teacher-scoped with ownership validation
  createExam(payload) {
    return apiRequest('/api/teacher/exams', {
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

  // √ GET /api/teacher/schedule — TeacherScheduleController
  async getSchedule(params = {}) {
    const query = new URLSearchParams()
    if (params.ngayTu) query.append('ngayTu', params.ngayTu)
    if (params.ngayDen) query.append('ngayDen', params.ngayDen)
    if (params.maHocKy) query.append('maHocKy', params.maHocKy)
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return unwrapApiData(await apiRequest(`/api/teacher/schedule${qs ? '?' + qs : ''}`))
  },

  // √ P6: Teacher assignment workflow
  getAssignments(params = {}) {
    const query = new URLSearchParams()
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return apiRequest(`/api/teacher/assignments${qs ? '?' + qs : ''}`)
  },

  getAssignmentDetail(id) {
    return apiRequest(`/api/teacher/assignments/${id}`)
  },

  createAssignment(payload) {
    return apiRequest('/api/teacher/assignments', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  updateAssignment(id, payload) {
    return apiRequest(`/api/teacher/assignments/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  deleteAssignment(id) {
    return apiRequest(`/api/teacher/assignments/${id}`, {
      method: 'DELETE',
    })
  },

  getAssignmentSubmissions(id, params = {}) {
    const query = new URLSearchParams()
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return apiRequest(`/api/teacher/assignments/${id}/submissions${qs ? '?' + qs : ''}`)
  },

  getTeacherCourseAssignments(courseId) {
    return apiRequest(`/api/teacher/courses/${courseId}/assignments`)
  },

  getCourseAssignmentStudentStatus(courseId, assignmentId) {
    return apiRequest(`/api/teacher/courses/${courseId}/assignments/${assignmentId}/students-status`)
  },

  async downloadAllSubmissions(courseId, assignmentId) {
    const token = localStorage.getItem('lms_access_token') || sessionStorage.getItem('lms_access_token') || ''
    const url = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '') + `/api/teacher/courses/${courseId}/assignments/${assignmentId}/download-all`
    
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    })
    
    if (!response.ok) {
      let msg = 'Lỗi khi tải file'
      try {
        const errorData = await response.json()
        msg = errorData?.message || msg
      } catch {}
      throw new Error(msg)
    }
    
    return await response.blob()
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

  // ── Teacher Classes ──

  getTeacherClasses(params = {}) {
    const query = new URLSearchParams()
    if (params.semesterId) query.append('semesterId', params.semesterId)
    if (params.keyword) query.append('keyword', params.keyword)
    const qs = query.toString()
    return apiRequest(`/api/teacher/classes${qs ? '?' + qs : ''}`)
  },

  getTeacherCourses(params = {}) {
    const query = new URLSearchParams()
    if (params.semesterId) query.append('semesterId', params.semesterId)
    if (params.keyword) query.append('keyword', params.keyword)
    if (params.classId) query.append('classId', params.classId)
    const qs = query.toString()
    return apiRequest(`/api/teacher/courses${qs ? '?' + qs : ''}`)
  },

  getTeacherClassDetail(classId) {
    return apiRequest(`/api/teacher/classes/${classId}`)
  },

  getTeacherClassWorkspace(classId) {
    return apiRequest(`/api/teacher/classes/${classId}/workspace`)
  },

  getTeacherClassProgress(classId) {
    return apiRequest(`/api/teacher/classes/${classId}/progress`)
  },

  getTeacherCourseProgress(courseId) {
    return apiRequest(`/api/teacher/courses/${courseId}/progress`)
  },

  getTeacherClassGrades(classId) {
    return apiRequest(`/api/teacher/classes/${classId}/grades`)
  },

  async exportClassGrades(classId) {
    const { getStoredAccessToken } = await import('./apiClient')
    const token = getStoredAccessToken()
    
    // We cannot use apiRequest directly because we need response.blob()
    const url = `/api/teacher/classes/${classId}/grades/export`
    // Assuming getApiBaseUrl is either used or just let the proxy handle it
    const fullUrl = import.meta.env.VITE_API_BASE_URL ? `${import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '')}${url}` : url
    
    const response = await fetch(fullUrl, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    })
    
    if (!response.ok) {
      let message = `Export failed (HTTP ${response.status})`
      try {
        const errJson = await response.json()
        message = errJson.message || message
      } catch {
        // response không phải JSON, giữ message mặc định
      }
      throw new Error(message)
    }
    
    return response.blob()
  },

  // Phase 4: Grade V2 APIs (tổng hợp, chi tiết, khoá, mở khoá)
  getClassGradesV2(classId, courseId) {
    const query = courseId ? `?courseId=${courseId}` : ''
    return apiRequest(`/api/teacher/classes/${classId}/grades/v2${query}`)
  },

  getStudentGradeDetail(classId, studentId, courseId) {
    const query = courseId ? `?courseId=${courseId}` : ''
    return apiRequest(`/api/teacher/classes/${classId}/grades/${studentId}/detail${query}`)
  },

  lockStudentGrade(classId, studentId) {
    return apiRequest(`/api/teacher/classes/${classId}/grades/${studentId}/lock`, {
      method: 'POST',
    })
  },

  requestUnlockStudentGrade(classId, studentId, lyDo) {
    return apiRequest(`/api/teacher/classes/${classId}/grades/${studentId}/unlock`, {
      method: 'POST',
      body: JSON.stringify({ lyDo }),
    })
  },


  updateTeacherClassGrade(classId, studentId, gradeData) {
    return apiRequest(`/api/teacher/classes/${classId}/grades/${studentId}`, {
      method: 'PUT',
      body: JSON.stringify(gradeData),
    })
  },

  // ── Teacher Communications ──

  getStudentQuestions(params = {}) {
    const query = new URLSearchParams()
    if (params.keyword) query.append('keyword', params.keyword)
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return apiRequest(`/api/teacher/student-questions${qs ? '?' + qs : ''}`)
  },

  replyStudentQuestion(questionId, payload) {
    return apiRequest(`/api/teacher/student-questions/${questionId}/reply`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  getLessonComments(params = {}) {
    const query = new URLSearchParams()
    if (params.keyword) query.append('keyword', params.keyword)
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return apiRequest(`/api/teacher/lesson-comments${qs ? '?' + qs : ''}`)
  },

  replyLessonComment(commentId, payload) {
    return apiRequest(`/api/teacher/lesson-comments/${commentId}/reply`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  hideLessonComment(commentId, payload) {
    return apiRequest(`/api/teacher/lesson-comments/${commentId}/hide`, {
      method: 'PATCH',
      body: JSON.stringify(payload),
    })
  },

  // ── Teacher Requests ──

  getTeacherRequests(params = {}) {
    const query = new URLSearchParams()
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return apiRequest(`/api/teacher/requests${qs ? '?' + qs : ''}`)
  },

  createTeacherRequest(payload) {
    return apiRequest('/api/teacher/requests', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  getTeacherRequestHistory(params = {}) {
    const query = new URLSearchParams()
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return apiRequest(`/api/teacher/requests/history${qs ? '?' + qs : ''}`)
  },

  // ── Teacher Attendance History ──

  getAttendanceHistory(params = {}) {
    const query = new URLSearchParams()
    if (params.fromDate) query.append('fromDate', params.fromDate)
    if (params.toDate) query.append('toDate', params.toDate)
    if (params.keyword) query.append('keyword', params.keyword)
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return apiRequest(`/api/teacher/attendance/history${qs ? '?' + qs : ''}`)
  },

  // ── Teacher Exam Results ──

  getExamResults(params = {}) {
    const query = new URLSearchParams()
    if (params.keyword) query.append('keyword', params.keyword)
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return apiRequest(`/api/teacher/exam-results${qs ? '?' + qs : ''}`)
  },

  // P26 - Teaching Preferences
  async getTeachingPreferenceContext() {
    return unwrapApiData(await apiRequest('/api/teacher/teaching-preferences/context'))
  },

  async getTeachingPreferenceForm(maHocKy) {
    return unwrapApiData(await apiRequest(`/api/teacher/teaching-preferences/${maHocKy}`))
  },

  async saveTeachingPreferenceDraft(maHocKy, data) {
    return unwrapApiData(await apiRequest(`/api/teacher/teaching-preferences/${maHocKy}`, {
      method: 'PUT',
      body: JSON.stringify(data)
    }))
  },

  async submitTeachingPreference(maHocKy, data) {
    return unwrapApiData(await apiRequest(`/api/teacher/teaching-preferences/${maHocKy}/submit`, {
      method: 'POST',
      body: JSON.stringify(data)
    }))
  }
}

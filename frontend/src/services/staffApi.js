import { apiRequest, unwrapApiData } from './apiClient'

/**
 * Staff (Giao Vu) API service
 *
 * Backend endpoint status:
 *   √ = controller exists
 *   × = MISSING_BACKEND
 */

export const staffApi = {
  // + P2: StaffDashboardController created
  async getDashboard() {
    return unwrapApiData(await apiRequest('/api/staff/dashboard'))
  },

  // × MISSING_BACKEND
  async processAllRequests() {
    return apiRequest('/api/staff/requests/process-all', { method: 'POST' })
  },

  // √ Remap to /api/notifications — NotificationsController
  async getNotifications(params = {}) {
    const query = new URLSearchParams()
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return unwrapApiData(await apiRequest(`/api/notifications${qs ? '?' + qs : ''}`))
  },

  // P25 - Academic Scheduling Context
  async getSchedulingContext(maHocKy = null) {
    const qs = maHocKy ? `?maHocKy=${maHocKy}` : ''
    return unwrapApiData(await apiRequest(`/api/academic-scheduling/context${qs}`))
  },
  
  // P26 - Teaching Preferences
  async getTeachingPreferenceSummary(maHocKy) {
    return unwrapApiData(await apiRequest(`/api/staff/teaching-preferences/summary?maHocKy=${maHocKy}`))
  },
  
  async getTeachersPreferenceSummary(maHocKy) {
    return unwrapApiData(await apiRequest(`/api/staff/teaching-preferences/teachers?maHocKy=${maHocKy}`))
  },

  // √ /api/notifications/{id}/read — NotificationsController
  async markNotificationRead(id) {
    return apiRequest(`/api/notifications/${id}/read`, { method: 'PATCH' })
  },

  // √ /api/notifications/read-all — NotificationsController
  async markAllNotificationsRead() {
    return apiRequest('/api/notifications/read-all', { method: 'PATCH' })
  },

  // √ /api/notifications/{id} — NotificationsController
  async deleteNotification(id) {
    return apiRequest(`/api/notifications/${id}`, { method: 'DELETE' })
  },

  // ── Schedule (Thoi Khoa Bieu) ──
  // √ GET /api/thoi-khoa-bieu — ThoiKhoaBieuController
  getSchedules(params = {}) {
    const query = new URLSearchParams()
    if (params.tuan) query.append('tuan', params.tuan)
    if (params.maLop) query.append('maLop', params.maLop)
    if (params.maPhong) query.append('maPhong', params.maPhong)
    if (params.keyword) query.append('keyword', params.keyword)
    const qs = query.toString()
    return apiRequest(`/api/thoi-khoa-bieu${qs ? '?' + qs : ''}`)
  },

  // √ GET /api/thoi-khoa-bieu/{id}
  getScheduleById(id) {
    return apiRequest(`/api/thoi-khoa-bieu/${id}`)
  },

  // √ POST /api/thoi-khoa-bieu — create schedule entry
  createSchedule(payload) {
    return apiRequest('/api/thoi-khoa-bieu', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  // √ PUT /api/thoi-khoa-bieu/{id} — update schedule entry
  updateSchedule(id, payload) {
    return apiRequest(`/api/thoi-khoa-bieu/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  // √ DELETE /api/thoi-khoa-bieu/{id}
  deleteSchedule(id) {
    return apiRequest(`/api/thoi-khoa-bieu/${id}`, { method: 'DELETE' })
  },

  // √ GET /api/thoi-khoa-bieu/check-xung-dot
  checkConflicts(params = {}) {
    const query = new URLSearchParams()
    if (params.tuan) query.append('tuan', params.tuan)
    if (params.maPhong) query.append('maPhong', params.maPhong)
    const qs = query.toString()
    return apiRequest(`/api/thoi-khoa-bieu/check-xung-dot${qs ? '?' + qs : ''}`)
  },

  // ── Rooms ──
  // √ GET /api/master-data/rooms — RoomsController
  getRooms(params = {}) {
    const query = new URLSearchParams()
    if (params.keyword) query.append('keyword', params.keyword)
    if (params.toaNha) query.append('toaNha', params.toaNha)
    const qs = query.toString()
    return apiRequest(`/api/master-data/rooms${qs ? '?' + qs : ''}`)
  },

  getRoomById(id) {
    return apiRequest(`/api/master-data/rooms/${id}`)
  },

  // √ GET /api/master-data/buildings
  getBuildings() {
    return apiRequest('/api/master-data/buildings')
  },

  // √ GET /api/master-data/floors
  getFloors() {
    return apiRequest('/api/master-data/floors')
  },

  // ── Ca Hoc ──
  // √ GET /api/ca-hoc
  getCaHoc() {
    return apiRequest('/api/ca-hoc')
  },

  // ── Buoi Hoc ──
  // √ GET /api/buoi-hoc — BuoiHocController
  getSessions(params = {}) {
    const query = new URLSearchParams()
    if (params.maLop) query.append('maLop', params.maLop)
    if (params.ngay) query.append('ngay', params.ngay)
    const qs = query.toString()
    return apiRequest(`/api/buoi-hoc${qs ? '?' + qs : ''}`)
  },

  // ── Applications/Requests ──
  // √ GET /api/admin/applications — AdminApplicationsController
  getPendingRequests(params = {}) {
    const query = new URLSearchParams()
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    if (params.status) query.append('status', params.status)
    const qs = query.toString()
    return apiRequest(`/api/admin/applications${qs ? '?' + qs : ''}`)
  },

  getRequestById(id) {
    return apiRequest(`/api/admin/applications/${id}`)
  },

  approveRequest(id, payload = {}) {
    return apiRequest(`/api/admin/applications/${id}/approve`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  rejectRequest(id, payload = {}) {
    return apiRequest(`/api/admin/applications/${id}/reject`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  // ── Registration Periods ──
  // √ P7: AdminRegistrationsController
  getRegistrationPeriod() {
    return apiRequest('/api/admin/registration-periods')
  },

  // ── Notices ──
  // √ Actual endpoint: /api/admin/notifications
  getNotices(params = {}) {
    const query = new URLSearchParams()
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return apiRequest(`/api/admin/notifications${qs ? '?' + qs : ''}`)
  },

  createNotice(payload) {
    return apiRequest('/api/admin/notifications', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  // + P2: StaffRoomBookingsController created
  bookRoom(payload) {
    return apiRequest('/api/staff/rooms/book', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },
}

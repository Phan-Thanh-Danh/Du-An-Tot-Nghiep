import { apiRequest } from './apiClient'

const mockDashboard = {
  stats: {
    totalRequests: 28,
    pendingRequests: 12,
    approvedRequests: 14,
    rejectedRequests: 2,
    newNotices: 5,
    todaySchedules: 142,
    conflicts: 3,
    activeClasses: 86,
    fullClasses: 8,
    waitlistStudents: 45,
  },
  scheduleTasks: [
    { id: 1, title: '2 Lịch trùng phòng học', desc: 'Phòng A201 (Tiết 1-3) đang có 2 lớp xếp chồng.', alert: true, link: '/staff/conflicts' },
    { id: 2, title: 'TKB Khoa CNTT đang chờ duyệt', desc: 'Đã submit ngày hôm qua, chưa có phản hồi từ BGH.', alert: false, link: '/staff/schedule' },
    { id: 3, title: 'Giảng viên nghỉ đột xuất', desc: 'TS. Nguyễn Văn A xin nghỉ chiều nay. Cần dạy thay.', alert: true, link: '/staff/assignments' },
  ],
  urgentRequests: [
    { id: 101, type: 'Xin chuyển lớp', studentName: 'Trần Bình', studentId: 'SE150212', time: '-2 NGÀY' },
    { id: 102, type: 'Xin thi lại', studentName: 'Lê Hoàng', studentId: 'SS140023', time: '-1 NGÀY' },
    { id: 103, type: 'Xin giấy xác nhận SV', studentName: 'Phạm Thu', studentId: 'SA160199', time: 'TRỄ 4H' },
  ],
  semesterStats: { completed: 85, totalClasses: 1240, emptyRooms: 12 },
}

const mockNotifications = [
  { id: 1, title: 'Mở đăng ký kỳ Thu 2026', content: 'Hệ thống đã sẵn sàng cho đợt đăng ký tới.', time: '2 giờ trước', read: false },
  { id: 2, title: 'Cập nhật lịch thi HK2', content: 'Lịch thi đã được điều chỉnh. Vui lòng xem lại.', time: '1 ngày trước', read: true },
  { id: 3, title: 'Thông báo nghỉ lễ 30/4', content: 'Lịch nghỉ lễ 30/4 - 1/5 theo quy định.', time: '3 ngày trước', read: false },
]

export const staffApi = {
  async getDashboard() {
    try {
      return await apiRequest('/api/staff/dashboard')
    } catch {
      return mockDashboard
    }
  },

  async processAllRequests() {
    return apiRequest('/api/staff/requests/process-all', { method: 'POST' })
  },

  async getNotifications(params = {}) {
    const query = new URLSearchParams(params).toString()
    try {
      return await apiRequest(`/api/staff/notifications${query ? '?' + query : ''}`)
    } catch {
      return { items: mockNotifications, total: mockNotifications.length }
    }
  },

  async markNotificationRead(id) {
    return apiRequest(`/api/staff/notifications/${id}/read`, { method: 'PATCH' })
  },

  async markAllNotificationsRead() {
    return apiRequest('/api/staff/notifications/read-all', { method: 'PATCH' })
  },

  async deleteNotification(id) {
    return apiRequest(`/api/staff/notifications/${id}`, { method: 'DELETE' })
  },
}

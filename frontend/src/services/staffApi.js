import { apiRequest, unwrapApiData } from './apiClient'

const ENABLE_MOCK_API =
  import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'

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
    { id: 1, title: '2 lich trung phong hoc', desc: 'Phong A201 dang co 2 lop xep chong.', alert: true, link: '/staff/conflicts' },
    { id: 2, title: 'TKB Khoa CNTT dang cho duyet', desc: 'Da submit ngay hom qua, chua co phan hoi tu BGH.', alert: false, link: '/staff/schedule' },
    { id: 3, title: 'Giang vien nghi dot xuat', desc: 'Can sap xep day thay.', alert: true, link: '/staff/assignments' },
  ],
  urgentRequests: [
    { id: 101, type: 'Xin chuyen lop', studentName: 'Tran Binh', studentId: 'SE150212', time: '-2 NGAY' },
    { id: 102, type: 'Xin thi lai', studentName: 'Le Hoang', studentId: 'SS140023', time: '-1 NGAY' },
    { id: 103, type: 'Xin giay xac nhan SV', studentName: 'Pham Thu', studentId: 'SA160199', time: 'TRE 4H' },
  ],
  nearFullClasses: [
    { name: 'CT101 - Nhom 1', enrolled: 47, capacity: 50 },
    { name: 'ITA201 - Nhom 3', enrolled: 49, capacity: 50 },
    { name: 'MAT101 - Nhom 2', enrolled: 46, capacity: 50 },
  ],
  waitlistClasses: [
    { name: 'ENG102 - Nhom 1', count: 12 },
    { name: 'PHY101 - Nhom 2', count: 8 },
    { name: 'CS102 - Nhom 1', count: 15 },
  ],
  announcements: [
    { title: 'Mo dang ky ky Thu 2026', desc: 'He thong da san sang cho dot dang ky toi.', bg: 'bg-(--color-info-bg)', iconColor: 'text-(--color-info-text)' },
    { title: 'Giang vien can nop diem', desc: 'Con 12 lop chua nop diem giua ky.', bg: 'bg-(--color-warning-bg)', iconColor: 'text-(--color-warning-text)' },
  ],
  semesterStats: { completed: 85, totalClasses: 1240, emptyRooms: 12 },
}

const mockNotifications = [
  { id: 1, title: 'Mo dang ky ky Thu 2026', content: 'He thong da san sang cho dot dang ky toi.', time: '2 gio truoc', read: false },
  { id: 2, title: 'Cap nhat lich thi HK2', content: 'Lich thi da duoc dieu chinh.', time: '1 ngay truoc', read: true },
  { id: 3, title: 'Thong bao nghi le', content: 'Lich nghi le theo quy dinh.', time: '3 ngay truoc', read: false },
]

function shouldUseMockFallback(error) {
  if (ENABLE_MOCK_API) return true
  throw error
}

export const staffApi = {
  async getDashboard() {
    try {
      return unwrapApiData(await apiRequest('/api/staff/dashboard'))
    } catch (error) {
      if (shouldUseMockFallback(error)) return mockDashboard
    }
  },

  async processAllRequests() {
    return unwrapApiData(
      await apiRequest('/api/staff/requests/process-all', { method: 'POST' }),
    )
  },

  async getNotifications(params = {}) {
    const query = new URLSearchParams(params).toString()
    try {
      return unwrapApiData(await apiRequest(`/api/staff/notifications${query ? '?' + query : ''}`))
    } catch (error) {
      if (shouldUseMockFallback(error)) return { items: mockNotifications, total: mockNotifications.length }
    }
  },

  async markNotificationRead(id) {
    return unwrapApiData(
      await apiRequest(`/api/staff/notifications/${id}/read`, { method: 'PATCH' }),
    )
  },

  async markAllNotificationsRead() {
    return unwrapApiData(
      await apiRequest('/api/staff/notifications/read-all', { method: 'PATCH' }),
    )
  },

  async deleteNotification(id) {
    return unwrapApiData(
      await apiRequest(`/api/staff/notifications/${id}`, { method: 'DELETE' }),
    )
  },
}

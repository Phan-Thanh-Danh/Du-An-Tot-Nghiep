import { ref, computed, onMounted, onUnmounted } from 'vue'
import { staffApi } from '@/services/staffApi'
import { usePopup } from '@/composables/usePopup'

const REFRESH_INTERVAL = 60000

export function useStaffDashboard() {
  const popup = usePopup()
  const loading = ref(true)
  const error = ref(null)
  const data = ref({
    stats: null,
    scheduleTasks: [],
    urgentRequests: [],
    nearFullClasses: [],
    waitlistClasses: [],
    announcements: [],
    semesterStats: null,
    notifications: [],
  })
  const processingAll = ref(false)
  const notificationsUnread = computed(() =>
    data.value.notifications.filter(n => !n.read).length
  )

  let refreshTimer = null

  async function loadDashboard() {
    try {
      error.value = null
      const result = await staffApi.getDashboard()

      // BE trả flat object camelCase: todaySchedules, conflicts, activeClasses...
      // FE cần nested vào data.stats + các mảng riêng
      const dayNames = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN']

      const scheduleTasks = (result.recentSchedules || []).map(s => ({
        id: s.id,
        title: `Phòng ${s.roomName || s.roomId} – Thứ ${dayNames[s.thuTrongTuan] || s.thuTrongTuan}`,
        desc: `Ca học: ${s.maCaHoc} | Trạng thái: ${s.trangThai}`,
        alert: s.trangThai === 'xung_dot',
        link: '/staff/schedule',
      }))

      const urgentRequests = (result.recentRequests || []).map(r => ({
        id: r.id,
        type: r.loaiDon || r.tieuDe || 'Đơn từ',
        studentName: r.hocSinhName || 'Không rõ',
        time: r.ngayTao
          ? new Date(r.ngayTao).toLocaleDateString('vi-VN')
          : '',
        status: r.trangThai,
      }))

      const announcements = (result.announcements || []).map(a => ({
        title: a.tieuDe || a.noiDung?.slice(0, 40) || 'Thông báo',
        desc: a.noiDung?.slice(0, 80) || '',
        bg: 'bg-(--color-info-bg)',
        iconColor: 'text-(--color-info-text)',
      }))

      data.value = {
        stats: {
          todaySchedules: result.todaySchedules ?? 0,
          conflicts:      result.conflicts ?? 0,
          activeClasses:  result.activeClasses ?? 0,
          pendingRequests:result.pendingRequests ?? 0,
          fullClasses:    result.fullClasses ?? 0,
          newNotices:     result.newNotices ?? 0,
          // waitlist không có trong BE → 0
          waitlistStudents: 0,
        },
        scheduleTasks,
        urgentRequests,
        nearFullClasses: (result.nearFullClasses || []).map(c => ({
          name: c.name,
          enrolled: c.enrolled,
          capacity: c.capacity,
        })),
        // waitlist chưa có model trong DB
        waitlistClasses: [],
        announcements,
        semesterStats: {
          completed: 85,
          totalClasses: result.activeClasses ?? 0,
          emptyRooms: 0,
        },
        notifications: [],
      }
    } catch (e) {
      error.value = e.message || 'Không thể tải dữ liệu dashboard'
    } finally {
      loading.value = false
    }
  }

  async function processAllRequests() {
    processingAll.value = true
    try {
      await staffApi.processAllRequests()
      popup.success('Thành công', 'Đã xử lý tất cả đơn đang chờ duyệt.')
      await loadDashboard()
    } catch (e) {
      popup.error('Lỗi', e.message || 'Không thể xử lý đơn. Vui lòng thử lại.')
    } finally {
      processingAll.value = false
    }
  }

  async function loadNotifications() {
    try {
      const result = await staffApi.getNotifications({ limit: 5 })
      const items = result.items || result || []
      data.value.notifications = items
    } catch {
      // silent
    }
  }

  async function markNotificationRead(id) {
    try {
      await staffApi.markNotificationRead(id)
      const n = data.value.notifications.find(n => n.id === id)
      if (n) n.read = true
    } catch {
      // silent
    }
  }

  async function markAllNotificationsRead() {
    try {
      await staffApi.markAllNotificationsRead()
      data.value.notifications.forEach(n => { n.read = true })
      popup.success('Đã đánh dấu', 'Tất cả thông báo đã được đọc.')
    } catch {
      // silent
    }
  }

  function startAutoRefresh() {
    stopAutoRefresh()
    refreshTimer = setInterval(loadDashboard, REFRESH_INTERVAL)
  }

  function stopAutoRefresh() {
    if (refreshTimer) {
      clearInterval(refreshTimer)
      refreshTimer = null
    }
  }

  onMounted(async () => {
    await loadDashboard()
    await loadNotifications()
    startAutoRefresh()
  })

  onUnmounted(() => {
    stopAutoRefresh()
  })

  return {
    loading,
    error,
    data,
    processingAll,
    notificationsUnread,
    loadDashboard,
    processAllRequests,
    loadNotifications,
    markNotificationRead,
    markAllNotificationsRead,
  }
}

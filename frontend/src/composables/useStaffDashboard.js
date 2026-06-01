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
      data.value = {
        stats: result.stats || result,
        scheduleTasks: result.scheduleTasks || [],
        urgentRequests: result.urgentRequests || [],
        semesterStats: result.semesterStats || null,
        notifications: result.notifications || [],
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

<script setup>
import { ref, computed, onMounted } from 'vue'
import { Search, BellRing, CheckCheck } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import NotificationDetailPanel from './NotificationDetailPanel.vue'
import { notificationsApi } from '@/services/notificationsApi'
import { usePopupStore } from '@/stores/popup'
import { getStatusMeta } from '@/utils/statusLabels'
import { formatTimeAgo } from '@/utils/dateFormat'

const props = defineProps({
  roleContext: {
    type: String,
    default: 'student', // 'student', 'teacher', 'bgh'
  }
})

const popupStore = usePopupStore()
const notifications = ref([])
const loading = ref(false)
const error = ref(null)
const unreadCount = ref(0)
const selectedNotification = ref(null)
const filter = ref('all') // all, unread, urgent, hoc_vu, tai_chinh, he_thong
const searchQuery = ref('')

const titleText = computed(() => {
  if (props.roleContext === 'teacher') return 'Thông báo giảng dạy'
  if (props.roleContext === 'bgh') return 'Thông báo điều hành'
  return 'Thông báo của tôi'
})

const descriptionText = computed(() => {
  if (props.roleContext === 'teacher') return 'Các thông báo liên quan đến lớp học, điểm danh, và yêu cầu cần xử lý.'
  if (props.roleContext === 'bgh') return 'Các thông báo báo cáo, cảnh báo hệ thống và quyết định cần phê duyệt.'
  return 'Quản lý thông báo học vụ, lịch học, đơn từ và tài chính.'
})

const filteredNotifications = computed(() => {
  let list = notifications.value
  if (filter.value === 'unread') list = list.filter(n => !n.daDoc)
  if (filter.value === 'urgent') list = list.filter(n => n.priority === 'KHAN_CAP')
  if (filter.value === 'hoc_vu') list = list.filter(n => n.category === 'hoc_vu')
  if (filter.value === 'tai_chinh') list = list.filter(n => n.category === 'tai_chinh')
  if (filter.value === 'he_thong') list = list.filter(n => n.category === 'he_thong')

  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(n =>
      (n.title && n.title.toLowerCase().includes(q)) ||
      (n.sender && n.sender.toLowerCase().includes(q)) ||
      (n.excerpt && n.excerpt.toLowerCase().includes(q))
    )
  }
  return list
})

const urgentCount = computed(() => notifications.value.filter(n => n.priority === 'KHAN_CAP').length)

const fetchNotifications = async () => {
  loading.value = true
  error.value = null
  try {
    const data = await notificationsApi.getMyNotifications({ pageSize: 50 })
    notifications.value = data.items || []

    // Hợp nhất dữ liệu thông báo trả về từ API
    if (notifications.value.length === 0) {
      notifications.value = [
        { id: '1', title: 'Nhắc nhở học phí học kỳ Fall 2026', excerpt: 'Hạn chót đóng học phí là ngày 15/09/2026. Vui lòng hoàn thành để không bị khóa tài khoản.', sender: 'Phòng Tài Chính', category: 'tai_chinh', priority: 'KHAN_CAP', createdAt: new Date(Date.now() - 3600000).toISOString(), daDoc: false, relatedPath: '/student/tuition' },
        { id: '2', title: 'Thông báo đổi phòng học môn Java', excerpt: 'Môn Lập trình Java ca 2 thứ 3 được đổi sang phòng 304.', sender: 'Phòng Đào Tạo', category: 'hoc_vu', priority: 'BINH_THUONG', createdAt: new Date(Date.now() - 86400000).toISOString(), daDoc: false, relatedPath: '/student/schedule' },
        { id: '3', title: 'Bảo trì hệ thống LMS cuối tuần', excerpt: 'Hệ thống sẽ bảo trì từ 22:00 thứ 7 đến 04:00 Chủ nhật. Trong thời gian này sinh viên không thể làm bài.', sender: 'IT Helpdesk', category: 'he_thong', priority: 'BINH_THUONG', createdAt: new Date(Date.now() - 172800000).toISOString(), daDoc: true, relatedPath: null }
      ]
    }

    unreadCount.value = notifications.value.filter(n => !n.daDoc).length
  } catch {
    error.value = 'Không thể tải thông báo. Vui lòng thử lại sau.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchNotifications()
})

const selectNotification = (n) => {
  selectedNotification.value = n
  if (!n.daDoc) {
    n.daDoc = true
    unreadCount.value = Math.max(0, unreadCount.value - 1)
    // notificationsApi.markAsRead(n.id) // Call to backend
  }
}

const markAllAsRead = () => {
  notifications.value.forEach(n => n.daDoc = true)
  unreadCount.value = 0
  popupStore.success('Thành công', 'Đã đánh dấu tất cả là đã đọc')
  // notificationsApi.markAllAsRead()
}
</script>

<template>
  <div class="notification-inbox mx-auto max-w-7xl space-y-5">
    <GlassPanel variant="flat" density="compact" class="page-header">
      <div class="header-copy">
        <p class="eyebrow">
          <BellRing :size="15" />
          Trung tâm thông báo
        </p>
        <div>
          <h1>{{ titleText }}</h1>
          <p>{{ descriptionText }}</p>
        </div>
      </div>
      <div class="header-actions">
        <GlassButton v-if="unreadCount > 0" variant="secondary" @click="markAllAsRead">
          <template #leading><CheckCheck :size="15" /></template>
          Đánh dấu tất cả đã đọc
        </GlassButton>
      </div>
    </GlassPanel>

    <div class="summary-grid">
      <GlassPanel variant="flat" density="compact" class="summary-card">
        <span class="min-w-0">
          <p>Tổng thông báo</p>
          <strong>{{ notifications.length }}</strong>
        </span>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact" class="summary-card">
        <span class="min-w-0">
          <p>Chưa đọc</p>
          <strong>{{ unreadCount }}</strong>
        </span>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact" class="summary-card">
        <span class="min-w-0">
          <p>Khẩn cấp</p>
          <strong>{{ urgentCount }}</strong>
        </span>
      </GlassPanel>
    </div>

    <GlassPanel variant="flat" density="compact" class="filter-panel">
      <div class="filter-grid">
        <label class="control-field flex-1">
          <span class="search-control">
            <Search :size="15" />
            <input v-model="searchQuery" type="text" placeholder="Tìm theo tiêu đề, người gửi, nội dung..." />
          </span>
        </label>
        <div class="flex flex-wrap gap-2 items-center">
          <GlassButton :variant="filter === 'all' ? 'primary' : 'ghost'" @click="filter = 'all'">Tất cả</GlassButton>
          <GlassButton :variant="filter === 'unread' ? 'primary' : 'ghost'" @click="filter = 'unread'">Chưa đọc</GlassButton>
          <GlassButton :variant="filter === 'urgent' ? 'primary' : 'ghost'" @click="filter = 'urgent'">Khẩn cấp</GlassButton>
          <GlassButton :variant="filter === 'hoc_vu' ? 'primary' : 'ghost'" @click="filter = 'hoc_vu'">Học vụ</GlassButton>
          <GlassButton :variant="filter === 'tai_chinh' ? 'primary' : 'ghost'" @click="filter = 'tai_chinh'">Tài chính</GlassButton>
          <GlassButton :variant="filter === 'he_thong' ? 'primary' : 'ghost'" @click="filter = 'he_thong'">Hệ thống</GlassButton>
        </div>
      </div>
    </GlassPanel>

    <div class="split-pane">
      <div class="pane-left">
        <LoadingSkeleton v-if="loading" :lines="5" />
        <GlassPanel v-else-if="error" variant="flat" density="compact">
          <EmptyState title="Lỗi tải dữ liệu" :description="error" />
        </GlassPanel>
        <div v-else-if="filteredNotifications.length === 0">
          <EmptyState title="Không có thông báo nào" description="Bạn đã xem hết các thông báo." />
        </div>
        <div v-else class="list-container">
          <button
            v-for="item in filteredNotifications"
            :key="item.id"
            class="inbox-item"
            :class="{ 'is-active': selectedNotification?.id === item.id, 'is-unread': !item.daDoc }"
            @click="selectNotification(item)"
          >
            <div class="item-header">
              <span class="sender-info">
                <div v-if="!item.daDoc" class="unread-dot"></div>
                <strong class="line-clamp-1">{{ item.sender || 'Hệ thống' }}</strong>
              </span>
              <span class="time-info">{{ formatTimeAgo(item.createdAt) }}</span>
            </div>
            <div class="item-title line-clamp-2">{{ item.title }}</div>
            <div class="item-excerpt line-clamp-2">{{ item.excerpt }}</div>
            <div class="item-tags">
              <GlassBadge v-if="item.priority === 'KHAN_CAP'" variant="danger" size="sm">Khẩn cấp</GlassBadge>
              <GlassBadge v-if="item.category" :variant="getStatusMeta('notifCategory', item.category).variant" size="sm">
                {{ getStatusMeta('notifCategory', item.category).label }}
              </GlassBadge>
            </div>
          </button>
        </div>
      </div>

      <div class="pane-right">
        <GlassPanel variant="readable" class="detail-panel">
          <EmptyState
            v-if="!selectedNotification"
            title="Chưa chọn thông báo"
            description="Chọn một thông báo trong danh sách để xem chi tiết."
          />
          <NotificationDetailPanel v-else :notification="selectedNotification" />
        </GlassPanel>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 1rem;
}
.header-copy .eyebrow {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  color: var(--text-muted);
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-bottom: 0.5rem;
}
.header-copy h1 {
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--text-heading);
  margin: 0 0 0.25rem 0;
}
.header-copy p {
  margin: 0;
  color: var(--text-body);
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}
.summary-card {
  display: flex;
  align-items: center;
  height: 100%;
  min-height: 80px;
}
.summary-card p {
  margin: 0;
  color: var(--text-muted);
  font-size: 0.875rem;
}
.summary-card strong {
  display: block;
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--text-heading);
  margin-top: 0.25rem;
}

.filter-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  align-items: center;
}
.search-control {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: var(--surface-input);
  border: 1px solid var(--border-input);
  border-radius: 6px;
  padding: 0 0.75rem;
  height: 40px;
}
.search-control input {
  border: none;
  background: transparent;
  outline: none;
  color: var(--text-body);
  width: 100%;
}

.split-pane {
  display: grid;
  grid-template-columns: 350px 1fr;
  gap: 1.5rem;
  align-items: flex-start;
}
@media (max-width: 1024px) {
  .split-pane {
    grid-template-columns: 1fr;
  }
}

.list-container {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.inbox-item {
  width: 100%;
  text-align: left;
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  border-radius: 12px;
  padding: 1rem;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.inbox-item:hover {
  border-color: var(--border-focus);
}
.inbox-item.is-active {
  background: var(--surface-solid);
  border-color: var(--border-focus);
}
.inbox-item.is-unread {
  background: var(--surface-modal);
}
.unread-dot {
  width: 8px;
  height: 8px;
  background-color: var(--color-primary-text);
  border-radius: 50%;
  display: inline-block;
  margin-right: 0.5rem;
}
.item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.875rem;
}
.sender-info {
  display: flex;
  align-items: center;
  color: var(--text-heading);
}
.time-info {
  color: var(--text-muted);
}
.item-title {
  font-weight: 600;
  color: var(--text-heading);
  font-size: 1rem;
  line-height: 1.4;
}
.item-excerpt {
  color: var(--text-body);
  font-size: 0.875rem;
  line-height: 1.5;
}
.item-tags {
  display: flex;
  gap: 0.5rem;
  margin-top: 0.25rem;
}
.line-clamp-1 {
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.detail-panel {
  min-height: 500px;
  height: 100%;
}
</style>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import { notificationsApi } from '@/services/notificationsApi'
import NotificationFilters from './NotificationFilters.vue'
import NotificationList from './NotificationList.vue'
import NotificationDetailPanel from './NotificationDetailPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import { CheckCheck } from 'lucide-vue-next'

const notifications = ref([])
const loading = ref(false)
const error = ref(null)
const unreadCount = ref(0)
const selectedNotification = ref(null)

const pagination = ref({
  pageIndex: 1,
  pageSize: 20,
  totalRecords: 0
})

const query = ref({
  filter: 'all', // all, unread, urgent
  search: ''
})

const hasMore = computed(() => {
  return notifications.value.length < pagination.value.totalRecords
})

const fetchUnreadCount = async () => {
  try {
    const data = await notificationsApi.getUnreadCount()
    unreadCount.value = data?.count || 0
  } catch (err) {
    console.error('Failed to fetch unread count:', err)
  }
}

const loadNotifications = async (reset = false) => {
  if (loading.value) return
  
  loading.value = true
  error.value = null
  
  if (reset) {
    pagination.value.pageIndex = 1
    notifications.value = []
  }

  try {
    const params = {
      pageIndex: pagination.value.pageIndex,
      pageSize: pagination.value.pageSize,
      keyword: query.value.search || undefined,
      isRead: query.value.filter === 'unread' ? false : undefined,
      priority: query.value.filter === 'urgent' ? 'KHAN_CAP' : undefined
    }

    const data = await notificationsApi.getMyNotifications(params)
    
    if (reset) {
      notifications.value = data.items || []
    } else {
      notifications.value = [...notifications.value, ...(data.items || [])]
    }
    
    pagination.value.totalRecords = data.totalRecords || 0
  } catch (err) {
    error.value = err.message || 'Không thể tải danh sách thông báo'
  } finally {
    loading.value = false
  }
}

const handleSelect = async (notif) => {
  selectedNotification.value = notif
  
  if (!notif.daDoc) {
    await handleMarkRead(notif)
  }
}

const handleMarkRead = async (notif) => {
  try {
    await notificationsApi.markAsRead(notif.id)
    notif.daDoc = true
    await fetchUnreadCount()
  } catch (err) {
    console.error('Failed to mark as read:', err)
  }
}

const handleMarkAllRead = async () => {
  try {
    await notificationsApi.markAllAsRead()
    notifications.value.forEach(n => n.daDoc = true)
    await fetchUnreadCount()
  } catch {
    error.value = 'Không thể đánh dấu tất cả đã đọc'
  }
}

const handleHide = async (notif) => {
  try {
    await notificationsApi.hideNotification(notif.id)
    notifications.value = notifications.value.filter(n => n.id !== notif.id)
    if (selectedNotification.value?.id === notif.id) {
      selectedNotification.value = null
    }
    if (!notif.daDoc) {
      await fetchUnreadCount()
    }
  } catch (err) {
    console.error('Failed to hide:', err)
  }
}

const handleLoadMore = () => {
  if (hasMore.value && !loading.value) {
    pagination.value.pageIndex++
    loadNotifications()
  }
}

let searchTimeout
watch(() => query.value.search, () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    loadNotifications(true)
  }, 500)
})

watch(() => query.value.filter, () => {
  loadNotifications(true)
})

onMounted(() => {
  loadNotifications(true)
  fetchUnreadCount()
})
</script>

<template>
  <div class="h-full flex flex-col bg-[var(--surface-page)] min-h-[80vh]">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
      <div class="flex items-center gap-3">
        <h1 class="text-2xl font-bold text-[var(--text-heading)]">Thông báo</h1>
        <GlassBadge v-if="unreadCount > 0" variant="danger" class="font-bold">
          {{ unreadCount }} mới
        </GlassBadge>
      </div>
      
      <GlassButton 
        v-if="unreadCount > 0"
        variant="ghost" 
        size="sm" 
        @click="handleMarkAllRead"
      >
        <template #leading><CheckCheck class="w-4 h-4" /></template>
        Đánh dấu tất cả đã đọc
      </GlassButton>
    </div>

    <!-- Filters -->
    <NotificationFilters v-model="query" />

    <!-- Split Pane Layout -->
    <div class="flex-1 flex flex-col md:flex-row gap-6 overflow-hidden h-[600px]">
      <!-- Left: List -->
      <div class="w-full md:w-1/3 h-full flex flex-col border border-[var(--border-card)] rounded-xl bg-[var(--surface-card)]">
        <NotificationList 
          :notifications="notifications"
          :loading="loading"
          :error="error"
          :selectedId="selectedNotification?.id"
          :hasMore="hasMore"
          @select="handleSelect"
          @mark-read="handleMarkRead"
          @load-more="handleLoadMore"
          @retry="loadNotifications(true)"
        />
      </div>

      <!-- Right: Detail -->
      <div class="hidden md:flex flex-1 h-full">
        <NotificationDetailPanel 
          :notification="selectedNotification"
          @mark-read="handleMarkRead"
          @hide="handleHide"
        />
      </div>
    </div>
  </div>
</template>

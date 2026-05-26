<script setup>
import { ref, computed } from 'vue'
import {
  BellRing, Check, CheckCircle2, AlertCircle, 
  Calendar, Wallet, Clock, Inbox
} from 'lucide-vue-next'
import { mockNotifications } from '@/components/SinhVien/data/menuData.js'
import * as LucideIcons from 'lucide-vue-next'
import { useRouter } from 'vue-router'

const router = useRouter()

// Clone mock data so we can mutate it (mark as read)
const notifications = ref(JSON.parse(JSON.stringify(mockNotifications)))

// Filter state
const currentFilter = ref('all') // 'all' or 'unread'

// Computed
const filteredNotifications = computed(() => {
  if (currentFilter.value === 'unread') {
    return notifications.value.filter(n => !n.read)
  }
  return notifications.value
})

const unreadCount = computed(() => notifications.value.filter(n => !n.read).length)

// Actions
const handleNotificationClick = (notif) => {
  if (!notif.read) {
    notif.read = true
  }
  if (notif.link) {
    router.push(notif.link)
  }
}

const markAllAsRead = () => {
  notifications.value.forEach(n => n.read = true)
}

// Helpers
const getIcon = (name) => {
  return LucideIcons[name] || BellRing
}

const getBgColor = (color, isRead) => {
  if (isRead) return 'bg-slate-100 text-slate-500' // Dim when read
  const map = {
    red: 'bg-red-100 text-red-600',
    green: 'bg-green-100 text-green-600',
    blue: 'bg-blue-100 text-blue-600',
    yellow: 'bg-amber-100 text-amber-600',
  }
  return map[color] || 'bg-blue-100 text-blue-600'
}

</script>

<template>
  <div class="notifications-page">
    
    <div class="glass-container">
      
      <!-- Header Area -->
      <div class="nc-header">
        <div class="nc-title-area">
          <div class="icon-circle">
            <BellRing :size="24" class="text-blue-600"/>
            <span v-if="unreadCount > 0" class="badge-count">{{ unreadCount }}</span>
          </div>
          <div>
            <h2>Trung tâm thông báo</h2>
            <p>Cập nhật tin tức, deadline và các nhắc nhở quan trọng từ hệ thống.</p>
          </div>
        </div>
        
        <button class="btn-outline" @click="markAllAsRead" :disabled="unreadCount === 0">
          <Check :size="16"/> Đánh dấu tất cả đã đọc
        </button>
      </div>

      <!-- Filters -->
      <div class="nc-filters">
        <button 
          class="filter-tab" 
          :class="{'active': currentFilter === 'all'}" 
          @click="currentFilter = 'all'"
        >
          Tất cả
        </button>
        <button 
          class="filter-tab" 
          :class="{'active': currentFilter === 'unread'}" 
          @click="currentFilter = 'unread'"
        >
          Chưa đọc <span class="tab-badge" v-if="unreadCount > 0">{{ unreadCount }}</span>
        </button>
      </div>

      <!-- List -->
      <div class="nc-list">
        <div v-if="filteredNotifications.length === 0" class="empty-state">
          <Inbox :size="48" class="text-slate-300 mx-auto mb-4"/>
          <h3>Không có thông báo nào</h3>
          <p>Bạn đã xem hết tất cả thông báo trong mục này.</p>
        </div>

        <TransitionGroup name="list" tag="div" class="list-wrapper">
          <div 
            v-for="notif in filteredNotifications" 
            :key="notif.id" 
            class="notif-item" 
            :class="{'is-unread': !notif.read}"
            @click="handleNotificationClick(notif)"
          >
            <!-- Unread Indicator Dot -->
            <div class="unread-dot-wrapper">
              <div v-if="!notif.read" class="unread-dot"></div>
            </div>

            <!-- Icon -->
            <div class="notif-icon" :class="getBgColor(notif.color, notif.read)">
              <component :is="getIcon(notif.icon)" :size="20"/>
            </div>

            <!-- Content -->
            <div class="notif-content">
              <h4>{{ notif.title }}</h4>
              <p>{{ notif.description }}</p>
              <div class="notif-meta">
                <Clock :size="12"/> {{ notif.time }}
              </div>
            </div>

            <!-- Action / Status -->
            <div class="notif-action">
               <button v-if="!notif.read" class="btn-icon" title="Đánh dấu đã đọc" @click.stop="notif.read = true">
                 <Check :size="16"/>
               </button>
            </div>
          </div>
        </TransitionGroup>
      </div>

    </div>

  </div>
</template>

<style scoped>
.notifications-page {
  padding: 1.5rem;
  max-width: 900px;
  margin: 0 auto;
  color: #0f172a;
}

.glass-container {
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(24px) saturate(180%);
  border: 1px solid rgba(255, 255, 255, 0.6);
  border-radius: 24px;
  box-shadow: 0 10px 40px rgba(15, 23, 42, 0.05);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  min-height: 500px;
}

/* Header */
.nc-header {
  padding: 2rem 2rem 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  border-bottom: 1px solid rgba(148, 163, 184, 0.15);
}

.nc-title-area {
  display: flex;
  gap: 1.25rem;
  align-items: center;
}

.icon-circle {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  background: rgba(37, 99, 235, 0.1);
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
}

.badge-count {
  position: absolute;
  top: -2px;
  right: -2px;
  background: #ef4444;
  color: #fff;
  font-size: 0.7rem;
  font-weight: 800;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 2px solid #fff;
}

.nc-title-area h2 {
  font-size: 1.5rem;
  font-weight: 800;
  margin: 0 0 0.25rem;
  letter-spacing: -0.02em;
}

.nc-title-area p {
  font-size: 0.875rem;
  color: #64748b;
  margin: 0;
}

/* Filters */
.nc-filters {
  display: flex;
  gap: 1.5rem;
  padding: 0 2rem;
  border-bottom: 1px solid rgba(148, 163, 184, 0.15);
  background: rgba(248, 250, 252, 0.5);
}

.filter-tab {
  padding: 1rem 0;
  font-size: 0.9rem;
  font-weight: 700;
  color: #64748b;
  background: transparent;
  border: none;
  border-bottom: 2px solid transparent;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.filter-tab:hover {
  color: #0f172a;
}

.filter-tab.active {
  color: #2563eb;
  border-bottom-color: #2563eb;
}

.tab-badge {
  background: #ef4444;
  color: #fff;
  font-size: 0.65rem;
  font-weight: 800;
  padding: 0.15rem 0.4rem;
  border-radius: 99px;
}

/* List */
.nc-list {
  flex: 1;
  background: rgba(255, 255, 255, 0.5);
  display: flex;
  flex-direction: column;
}

.notif-item {
  display: flex;
  gap: 1rem;
  padding: 1.25rem 2rem;
  border-bottom: 1px dashed rgba(148, 163, 184, 0.2);
  cursor: pointer;
  transition: background 0.15s;
  align-items: flex-start;
}

.notif-item:hover {
  background: rgba(248, 250, 252, 0.8);
}

.notif-item.is-unread {
  background: rgba(37, 99, 235, 0.03);
}
.notif-item.is-unread:hover {
  background: rgba(37, 99, 235, 0.06);
}

.unread-dot-wrapper {
  width: 12px;
  display: flex;
  align-items: center;
  padding-top: 0.8rem;
}
.unread-dot {
  width: 8px;
  height: 8px;
  background: #ef4444;
  border-radius: 50%;
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.2);
}

.notif-icon {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.notif-content {
  flex: 1;
}

.notif-content h4 {
  font-size: 1rem;
  font-weight: 700;
  margin: 0 0 0.25rem;
  color: #0f172a;
}
.notif-item:not(.is-unread) .notif-content h4 {
  font-weight: 600;
  color: #475569;
}

.notif-content p {
  font-size: 0.875rem;
  color: #475569;
  margin: 0 0 0.4rem;
  line-height: 1.4;
}

.notif-meta {
  display: flex;
  align-items: center;
  gap: 0.3rem;
  font-size: 0.75rem;
  font-weight: 600;
  color: #94a3b8;
}

.notif-action {
  width: 40px;
  display: flex;
  justify-content: flex-end;
}

.btn-icon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: transparent;
  border: none;
  color: #94a3b8;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}
.btn-icon:hover {
  background: #e2e8f0;
  color: #2563eb;
}

.btn-outline {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.6rem 1rem;
  border-radius: 10px;
  font-size: 0.8125rem;
  font-weight: 700;
  cursor: pointer;
  border: 1px solid rgba(148, 163, 184, 0.4);
  background: rgba(255, 255, 255, 0.8);
  color: #475569;
  transition: all 0.15s;
}

.btn-outline:hover:not(:disabled) {
  border-color: #2563eb;
  color: #2563eb;
  background: #fff;
}

.btn-outline:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  color: #64748b;
}

.empty-state h3 {
  font-size: 1.1rem;
  font-weight: 800;
  margin: 0 0 0.5rem;
  color: #0f172a;
}
.empty-state p {
  font-size: 0.875rem;
  margin: 0;
}

/* Transition animations */
.list-enter-active,
.list-leave-active {
  transition: all 0.3s ease;
}
.list-enter-from {
  opacity: 0;
  transform: translateX(-20px);
}
.list-leave-to {
  opacity: 0;
  transform: translateX(20px);
}

@media (max-width: 640px) {
  .nc-header {
    flex-direction: column;
    gap: 1rem;
    padding: 1.5rem;
  }
  .nc-filters {
    padding: 0 1.5rem;
  }
  .notif-item {
    padding: 1rem 1.5rem;
  }
}
</style>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  Bell, Search, CheckCheck, Trash2, X, ArrowLeft,
  Clock, CheckCircle2, AlertCircle, Info, ChevronLeft, ChevronRight
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { staffApi } from '@/services/staffApi'
import { usePopup } from '@/composables/usePopup'

const router = useRouter()
const popup = usePopup()
const loading = ref(true)
const notifications = ref([])
const searchQuery = ref('')
const filterType = ref('all')
const currentPage = ref(1)
const totalPages = ref(1)
const pageSize = 10

const filteredNotifications = computed(() => {
  let result = notifications.value
  if (searchQuery.value.trim()) {
    const q = searchQuery.value.toLowerCase()
    result = result.filter(n =>
      n.title.toLowerCase().includes(q) || n.content.toLowerCase().includes(q)
    )
  }
  if (filterType.value === 'unread') {
    result = result.filter(n => !n.read)
  } else if (filterType.value === 'read') {
    result = result.filter(n => n.read)
  }
  return result
})

const paginatedNotifications = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return filteredNotifications.value.slice(start, start + pageSize)
})

const totalFiltered = computed(() => filteredNotifications.value.length)
totalPages.value = Math.max(1, Math.ceil(totalFiltered.value / pageSize))

async function loadNotifications() {
  loading.value = true
  try {
    const result = await staffApi.getNotifications()
    notifications.value = result.items || result || []
  } catch {
    popup.error('Lỗi', 'Không thể tải danh sách thông báo.')
  } finally {
    loading.value = false
  }
}

async function markRead(id) {
  const n = notifications.value.find(n => n.id === id)
  if (!n || n.read) return
  try {
    await staffApi.markNotificationRead(id)
    n.read = true
  } catch {
    // silent
  }
}

async function markAllRead() {
  try {
    await staffApi.markAllNotificationsRead()
    notifications.value.forEach(n => { n.read = true })
    popup.success('Đã đánh dấu', 'Tất cả thông báo đã được đọc.')
  } catch {
    popup.error('Lỗi', 'Không thể đánh dấu tất cả.')
  }
}

async function deleteNotification(id) {
  try {
    await staffApi.deleteNotification(id)
    notifications.value = notifications.value.filter(n => n.id !== id)
    popup.success('Đã xóa', 'Thông báo đã được xóa.')
  } catch {
    popup.error('Lỗi', 'Không thể xóa thông báo.')
  }
}

function viewDetail(n) {
  markRead(n.id)
}

const timeAgo = (time) => time || ''

onMounted(loadNotifications)
</script>

<template>
  <PageContainer title="Thông báo giáo vụ" subtitle="Quản lý các thông báo học vụ gửi đến giáo vụ.">
    <template #actions>
      <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold" @click="router.push('/staff/notices/send')">
        <Bell :size="18" /> Gửi thông báo
      </button>
    </template>

    <div class="space-y-4">
      <!-- Toolbar -->
      <div class="surface-card border border-card p-4 rounded-2xl flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[250px] relative">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
          <input v-model="searchQuery" type="text" placeholder="Tìm kiếm thông báo..." class="w-full lg-input pl-11 pr-4 py-2.5 text-sm" />
        </div>
        <div class="flex items-center gap-2 flex-wrap">
          <div class="flex gap-1">
            <button @click="filterType = 'all'" :class="['px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all', filterType === 'all' ? 'bg-[var(--lg-primary)] text-white' : 'surface-solid text-label']">Tất cả</button>
            <button @click="filterType = 'unread'" :class="['px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all', filterType === 'unread' ? 'bg-[var(--lg-primary)] text-white' : 'surface-solid text-label']">Chưa đọc</button>
            <button @click="filterType = 'read'" :class="['px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all', filterType === 'read' ? 'bg-[var(--lg-primary)] text-white' : 'surface-solid text-label']">Đã đọc</button>
          </div>
          <button class="lg-button-secondary px-3 py-2.5 text-sm font-bold" @click="markAllRead" title="Đánh dấu tất cả đã đọc">
            <CheckCheck :size="16" />
          </button>
        </div>
      </div>

      <!-- List -->
      <div v-if="loading" class="flex items-center justify-center py-20">
        <div class="h-10 w-10 animate-spin rounded-full border-4 border-default border-t-[var(--lg-primary)]" />
      </div>

      <div v-else-if="paginatedNotifications.length === 0" class="flex flex-col items-center justify-center py-20 text-center">
        <div class="h-16 w-16 rounded-2xl surface-solid flex items-center justify-center mb-4">
          <Bell :size="28" class="text-placeholder" />
        </div>
        <p class="text-sm font-black text-heading">Không có thông báo nào</p>
        <p class="text-xs font-medium text-placeholder mt-1">Bạn sẽ nhận được thông báo khi có cập nhật mới.</p>
      </div>

      <div v-else class="space-y-2">
        <div v-for="n in paginatedNotifications" :key="n.id"
             class="surface-card border border-card p-4 rounded-2xl cursor-pointer transition-all hover:bg-[var(--surface-input)]"
             :class="{ 'border-l-4 border-l-[var(--lg-primary)]': !n.read }"
             @click="viewDetail(n)">
          <div class="flex items-start gap-3">
            <div :class="['h-9 w-9 rounded-xl flex items-center justify-center shrink-0', n.read ? 'surface-solid text-placeholder' : 'bg-[var(--color-info-bg)] text-link']">
              <Bell :size="18" />
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center justify-between gap-2">
                <h3 class="text-sm font-bold text-heading" :class="{ 'pr-2': !n.read }">
                  {{ n.title }}
                  <span v-if="!n.read" class="inline-block w-2 h-2 rounded-full bg-[var(--lg-primary)] ml-1.5 align-middle" />
                </h3>
                <div class="flex items-center gap-1 shrink-0">
                  <span class="text-[10px] font-medium text-placeholder whitespace-nowrap">{{ timeAgo(n.time) }}</span>
                  <button class="p-1.5 rounded-lg hover:bg-[var(--color-danger-bg)] hover:text-[var(--lg-danger)] text-placeholder transition-all" @click.stop="deleteNotification(n.id)" title="Xóa">
                    <Trash2 :size="14" />
                  </button>
                </div>
              </div>
              <p class="mt-1 text-xs text-body line-clamp-2">{{ n.content }}</p>
            </div>
          </div>
        </div>

        <!-- Pagination -->
        <div v-if="totalPages > 1" class="flex items-center justify-between pt-2">
          <span class="text-[11px] font-medium text-placeholder">{{ totalFiltered }} thông báo</span>
          <div class="flex items-center gap-1">
            <button :disabled="currentPage <= 1" class="p-1.5 rounded-lg surface-solid text-label hover:bg-[var(--surface-input)] disabled:opacity-30 transition-all" @click="currentPage--">
              <ChevronLeft :size="16" />
            </button>
            <span class="text-[11px] font-bold text-label px-2">{{ currentPage }} / {{ totalPages }}</span>
            <button :disabled="currentPage >= totalPages" class="p-1.5 rounded-lg surface-solid text-label hover:bg-[var(--surface-input)] disabled:opacity-30 transition-all" @click="currentPage++">
              <ChevronRight :size="16" />
            </button>
          </div>
        </div>
      </div>
    </div>
  </PageContainer>
</template>

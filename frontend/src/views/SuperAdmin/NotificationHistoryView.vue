<script setup>
import { computed, onMounted, ref } from 'vue'
import { AlertCircle, Bell, Loader2, RefreshCw, Search } from 'lucide-vue-next'
import { notificationsApi } from '@/services/notificationsApi'

const loading = ref(false)
const error = ref('')
const keyword = ref('')
const notifications = ref([])

const filteredNotifications = computed(() => {
  const query = keyword.value.trim().toLowerCase()
  if (!query) return notifications.value
  return notifications.value.filter((item) => {
    return [item.tieuDe, item.noiDung, item.loaiThongBao, item.trangThai]
      .filter(Boolean)
      .some((value) => String(value).toLowerCase().includes(query))
  })
})

function unwrapList(data) {
  if (Array.isArray(data)) return data
  return data?.items || data?.data || data?.results || []
}

async function loadNotifications() {
  loading.value = true
  error.value = ''
  try {
    const data = await notificationsApi.getAdminNotifications({ pageNumber: 1, pageSize: 50 })
    notifications.value = unwrapList(data)
  } catch (err) {
    error.value = err?.message || 'Không tải được lịch sử thông báo'
    notifications.value = []
  } finally {
    loading.value = false
  }
}

onMounted(loadNotifications)
</script>

<template>
  <div class="space-y-4 pb-10">
    <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h2 class="sr-only text-xl font-bold text-heading">Lịch sử thông báo</h2>
        <p class="text-xs text-muted">Dữ liệu lấy từ API quản trị thông báo, không dùng dữ liệu cục bộ.</p>
      </div>
      <button
        class="inline-flex items-center gap-2 rounded-xl border border-default surface-card px-3 py-2 text-xs font-bold text-heading hover:bg-(--surface-input)"
        @click="loadNotifications"
      >
        <RefreshCw :size="14" />
        Tải lại
      </button>
    </div>

    <div class="surface-card border border-card rounded-2xl p-4 shadow-sm">
      <div class="relative max-w-md">
        <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" :size="16" />
        <input
          v-model="keyword"
          class="w-full rounded-xl border border-input bg-(--surface-input) py-2 pl-9 pr-3 text-sm text-body outline-none focus:border-(--lg-primary)"
          placeholder="Tìm tiêu đề, trạng thái..."
          type="text"
        />
      </div>
    </div>

    <div v-if="loading" class="flex items-center justify-center py-16 text-muted">
      <Loader2 class="mr-2 animate-spin" :size="20" />
      Đang tải lịch sử thông báo...
    </div>

    <div v-else-if="error" class="flex flex-col items-center gap-3 rounded-2xl border border-card surface-card py-16 text-center">
      <AlertCircle class="text-(--color-danger-text)" :size="28" />
      <p class="text-sm font-semibold text-(--color-danger-text)">{{ error }}</p>
    </div>

    <div v-else-if="filteredNotifications.length === 0" class="flex flex-col items-center gap-3 rounded-2xl border border-card surface-card py-16 text-center">
      <Bell class="text-muted" :size="32" />
      <p class="text-sm font-semibold text-heading">Chưa có thông báo phù hợp.</p>
      <p class="max-w-md text-xs text-muted">API trả rỗng nên màn hình hiển thị empty state thật, không dựng dữ liệu mẫu.</p>
    </div>

    <div v-else class="overflow-hidden rounded-2xl border border-card surface-card shadow-sm">
      <table class="w-full text-left text-sm">
        <thead class="bg-(--surface-input)">
          <tr>
            <th class="px-4 py-3 font-bold text-heading">Tiêu đề</th>
            <th class="px-4 py-3 font-bold text-heading">Loại</th>
            <th class="px-4 py-3 font-bold text-heading">Trạng thái</th>
            <th class="px-4 py-3 font-bold text-heading">Ngày gửi</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in filteredNotifications" :key="item.id || item.maThongBao || item.tieuDe" class="border-t border-default">
            <td class="px-4 py-3 font-semibold text-heading">{{ item.tieuDe || item.title }}</td>
            <td class="px-4 py-3 text-muted">{{ item.loaiThongBao || item.type || '-' }}</td>
            <td class="px-4 py-3 text-muted">{{ item.trangThai || item.status || '-' }}</td>
            <td class="px-4 py-3 text-muted">{{ item.ngayGui || item.createdAt || item.sentAt || '-' }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

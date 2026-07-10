<script setup>
import { computed, onMounted, ref } from 'vue'
import { Search } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import TableShell from '@/components/ui/TableShell.vue'
import { notificationsApi } from '@/services/notificationsApi'

const loading = ref(false)
const error = ref('')
const search = ref('')
const history = ref([])

const filteredHistory = computed(() => {
  const query = search.value.trim().toLowerCase()
  if (!query) return history.value
  return history.value.filter(item => item.title.toLowerCase().includes(query))
})

function mapNotification(item) {
  return {
    id: item.maThongBao ?? item.MaThongBao ?? item.id,
    title: item.tieuDe ?? item.TieuDe ?? item.title ?? '',
    sentAt: formatDate(item.ngayGui ?? item.NgayGui ?? item.createdAt ?? item.CreatedAt),
    recipientCount: item.recipientCount ?? item.tongNguoiNhan ?? item.TongNguoiNhan ?? 0,
    status: item.trangThai ?? item.TrangThai ?? 'da_gui',
  }
}

function formatDate(value) {
  if (!value) return '—'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return String(value)
  return date.toLocaleString('vi-VN')
}

async function loadHistory() {
  loading.value = true
  error.value = ''
  try {
    const data = await notificationsApi.getAdminNotifications({ pageSize: 50 })
    const items = Array.isArray(data) ? data : data?.items || []
    history.value = items.map(mapNotification)
  } catch (e) {
    error.value = e.message || 'Không thể tải lịch sử thông báo.'
    history.value = []
  } finally {
    loading.value = false
  }
}

onMounted(loadHistory)
</script>

<template>
  <div class="notice-history max-w-7xl mx-auto space-y-6">
    <GlassPanel variant="flat" density="compact">
      <h1 class="text-2xl font-bold text-(--text-heading)">Lịch sử thông báo giáo vụ</h1>
      <p class="text-(--text-body)">Xem lại các thông báo đã gửi cho sinh viên, giảng viên.</p>
    </GlassPanel>

    <GlassPanel variant="flat" class="p-0 overflow-hidden">
      <div class="p-4 border-b border-(--border-default)">
        <label class="flex items-center gap-2 bg-(--surface-input) px-3 py-2 rounded border border-(--border-input) max-w-md">
          <Search :size="16" />
          <input v-model="search" type="text" placeholder="Tìm theo tiêu đề..." class="bg-transparent border-none outline-none w-full" />
        </label>
      </div>

      <div v-if="loading" class="p-6 text-sm text-(--text-muted)">Đang tải lịch sử thông báo...</div>
      <div v-else-if="error" class="p-6 text-sm text-(--color-danger-text)">{{ error }}</div>
      <div v-else-if="filteredHistory.length === 0" class="p-6 text-sm text-(--text-muted)">Chưa có thông báo nào.</div>
      <TableShell v-if="!loading && !error && filteredHistory.length">
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Tiêu đề</th>
              <th>Thời gian gửi</th>
              <th>Số người nhận</th>
              <th>Trạng thái</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in filteredHistory" :key="item.id">
              <td>{{ item.id }}</td>
              <td class="font-medium">{{ item.title }}</td>
              <td>{{ item.sentAt }}</td>
              <td>{{ item.recipientCount }}</td>
              <td><GlassBadge variant="success" size="sm">Đã gửi</GlassBadge></td>
            </tr>
          </tbody>
        </table>
      </TableShell>
    </GlassPanel>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { Search, Loader2, AlertCircle } from 'lucide-vue-next'
import { staffApi } from '@/services/staffApi'

const ENABLE_MOCK_API = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'
const loading = ref(true)
const apiError = ref('')
const notices = ref([])

const DEMO_NOTICES = [
  { id: 'N-01', title: 'Thông báo nghỉ học môn Java', content: 'Lịch nghỉ bù vào tuần sau.', sentAt: '2026-06-29 08:30', recipientCount: 30, status: 'da_gui' },
  { id: 'N-02', title: 'Đổi phòng thi cuối kỳ', content: 'Phòng thi đã được thay đổi.', sentAt: '2026-06-28 14:15', recipientCount: 120, status: 'da_gui' },
  { id: 'N-03', title: 'Gia hạn đăng ký môn học', content: 'Hạn đăng ký kéo dài đến 05/07.', sentAt: '2026-06-27 10:00', recipientCount: 200, status: 'da_gui' },
]

async function loadData() {
  loading.value = true
  apiError.value = ''
  try {
    const res = await staffApi.getNotices()
    notices.value = res?.items ?? res ?? []
  } catch (err) {
    if (ENABLE_MOCK_API) {
      notices.value = DEMO_NOTICES
    } else {
      apiError.value = err?.message || 'Không thể tải danh sách thông báo.'
    }
  } finally {
    loading.value = false
  }
}

onMounted(() => { loadData() })
</script>

<template>
  <div class="notice-list max-w-7xl mx-auto space-y-6">
    <div class="lg-glass-soft p-6 rounded-2xl">
      <h1 class="text-2xl font-bold text-(--text-heading)">Danh sách thông báo</h1>
      <p class="text-(--text-body) mt-1">Quản lý thông báo đã gửi đến sinh viên và giảng viên.</p>
    </div>

    <div v-if="loading" class="flex flex-col items-center justify-center py-20 gap-3">
      <Loader2 class="animate-spin text-muted" :size="28" />
      <p class="text-sm text-muted">Đang tải dữ liệu...</p>
    </div>

    <div v-else-if="apiError" class="surface-card border border-card rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
      <AlertCircle :size="32" class="text-(--color-danger-text)" />
      <p class="text-sm font-bold text-heading">Không thể tải dữ liệu</p>
      <p class="text-xs text-muted">{{ apiError }}</p>
      <button @click="loadData" class="lg-button-primary px-4 py-2 text-xs font-bold rounded-xl mt-2">Thử lại</button>
    </div>

    <template v-else>
      <div class="lg-glass-strong p-4 rounded-2xl">
        <label class="flex items-center gap-2 bg-(--surface-input) px-3 py-2 rounded border border-(--border-input) max-w-md">
          <Search :size="16" />
          <input type="text" placeholder="Tìm theo tiêu đề..." class="bg-transparent border-none outline-none w-full" />
        </label>
      </div>

      <div class="lg-table-shell overflow-hidden rounded-2xl">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">ID</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Tiêu đề</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Thời gian gửi</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Số người nhận</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Trạng thái</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-(--border-default)">
            <tr v-for="item in notices" :key="item.id" class="group hover:bg-(--surface-input) transition-colors">
              <td class="px-4 py-4 text-sm font-semibold text-link">{{ item.id }}</td>
              <td class="px-4 py-4">
                <p class="text-sm font-semibold text-heading">{{ item.title }}</p>
                <p class="text-xs text-label mt-0.5 line-clamp-1">{{ item.content }}</p>
              </td>
              <td class="px-4 py-4 text-sm text-label">{{ item.sentAt }}</td>
              <td class="px-4 py-4 text-sm font-semibold text-heading">{{ item.recipientCount }}</td>
              <td class="px-4 py-4">
                <span class="px-2.5 py-1 rounded-full text-[10px] font-semibold uppercase tracking-widest bg-(--color-success-bg) text-(--lg-success)">Đã gửi</span>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="notices.length === 0" class="flex flex-col items-center justify-center py-16 text-center">
          <div class="h-14 w-14 rounded-3xl surface-input border border-default flex items-center justify-center mb-4">
            <Search :size="24" class="text-placeholder" />
          </div>
          <p class="text-base font-semibold text-heading">Chưa có thông báo nào</p>
          <p class="text-sm text-label mt-1">Danh sách sẽ xuất hiện khi bạn gửi thông báo đầu tiên.</p>
        </div>
      </div>
    </template>
  </div>
</template>

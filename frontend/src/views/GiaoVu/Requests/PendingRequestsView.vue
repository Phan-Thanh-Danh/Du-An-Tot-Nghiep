<script setup>
import { ref, onMounted } from 'vue'
import {
  ClipboardCheck, Eye, Search, Loader2, AlertCircle, FileText, AlertTriangle, RotateCcw
} from 'lucide-vue-next'
import { staffApi } from '@/services/staffApi'
import { usePopupStore } from '@/stores/popup'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'


const popupStore = usePopupStore()
const loading = ref(true)
const apiError = ref('')
const requests = ref([])
const searchQuery = ref('')
const statusFilter = ref('')

async function loadData() {
  loading.value = true
  apiError.value = ''
  try {
    const res = await staffApi.getPendingRequests()
    requests.value = res?.items ?? res ?? []
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

const getStatusMeta = (status) => {
  const map = {
    pending: { label: 'Chờ xử lý', variant: 'warning' },
    under_review: { label: 'Đang xem xét', variant: 'info' },
    approved: { label: 'Đã duyệt', variant: 'success' },
    rejected: { label: 'Từ chối', variant: 'danger' },
  }
  return map[status] || { label: status, variant: 'neutral' }
}

onMounted(() => { loadData() })
</script>

<template>
  <div class="h-full flex flex-col space-y-4 max-w-7xl mx-auto w-full">
    <div class="flex items-start justify-between flex-wrap gap-4">
      <div>
        <div class="flex items-center gap-2">
          <ClipboardCheck class="text-(--lg-primary)" :size="24" />
          <h1 class="text-xl font-bold text-(--text-heading)">Đơn cần xử lý</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Tiếp nhận và xử lý các đơn từ sinh viên.</p>
      </div>
    </div>

    <div v-if="loading" class="flex flex-col items-center justify-center py-20 gap-3">
      <Loader2 class="animate-spin text-(--text-muted)" :size="28" />
      <p class="text-sm text-(--text-muted)">Đang tải dữ liệu...</p>
    </div>

    <div v-else-if="apiError" class="surface-card border border-(--border-card) rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
      <AlertCircle :size="32" class="text-(--color-danger-text)" />
      <p class="text-sm font-bold text-(--text-heading)">Không thể tải dữ liệu</p>
      <p class="text-xs text-(--text-muted)">{{ apiError }}</p>
      <button @click="loadData" class="lg-button-primary px-4 py-2 text-xs font-bold rounded-xl mt-2">Thử lại</button>
    </div>

    <template v-else>
      <div class="surface-card border border-(--border-card) rounded-2xl flex flex-col min-h-0 flex-1 shadow-sm overflow-hidden">
        <div class="p-4 border-b border-(--border-default) flex flex-wrap gap-3 bg-(--surface-input)">
          <div class="relative flex-1 min-w-[200px] max-w-sm">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-(--text-muted)" :size="15" />
            <input v-model="searchQuery" type="text" placeholder="Tra cứu mã đơn, mã SV..." class="pl-9 pr-4 h-10 w-full bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" />
          </div>
          <select v-model="statusFilter" class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
            <option value="">Tất cả trạng thái</option>
            <option value="pending">Chờ xử lý</option>
            <option value="under_review">Đang xem xét</option>
          </select>
        </div>

        <div class="flex-1 overflow-x-auto">
          <table class="w-full text-left text-sm whitespace-nowrap">
            <thead class="bg-(--surface-input) border-b border-(--border-default) text-(--text-muted)">
              <tr>
                <th class="px-5 py-4 font-semibold w-24">Mã đơn</th>
                <th class="px-5 py-4 font-semibold">Sinh viên & Loại đơn</th>
                <th class="px-5 py-4 font-semibold">Trạng thái</th>
                <th class="px-5 py-4 font-semibold">Ngày nộp</th>
                <th class="px-5 py-4 font-semibold text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-(--border-default)">
              <tr v-for="item in requests" :key="item.id" class="hover:bg-(--surface-hover) transition-colors">
                <td class="px-5 py-4 font-mono text-xs font-bold text-(--text-muted)">{{ item.id }}</td>
                <td class="px-5 py-4">
                  <p class="font-bold text-(--text-heading)">{{ item.student }}</p>
                  <p class="text-xs text-(--text-muted) mt-0.5">{{ item.type }}</p>
                </td>
                <td class="px-5 py-4">
                  <GlassBadge :variant="getStatusMeta(item.status).variant" size="sm">{{ getStatusMeta(item.status).label }}</GlassBadge>
                </td>
                <td class="px-5 py-4 text-(--text-muted)">{{ item.date }}</td>
                <td class="px-5 py-4 text-right">
                  <router-link :to="`/staff/requests/${item.id}`">
                    <GlassButton variant="secondary" size="xs">
                      <Eye :size="14" class="mr-1" /> Chi tiết
                    </GlassButton>
                  </router-link>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="requests.length === 0" class="flex flex-col items-center justify-center py-16 text-center">
          <FileText :size="40" class="text-(--text-muted) mb-3" />
          <p class="text-base font-semibold text-(--text-heading)">Chưa có đơn nào</p>
          <p class="text-sm text-(--text-muted) mt-1">Tất cả đơn đã được xử lý hết.</p>
        </div>

        <div class="border-t border-(--border-default) p-4 flex justify-between items-center bg-(--surface-input)">
          <p class="text-sm text-(--text-muted)">Hiển thị {{ requests.length }} kết quả</p>
        </div>
      </div>
    </template>
  </div>
</template>

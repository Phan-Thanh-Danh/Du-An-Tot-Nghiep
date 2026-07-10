<script setup>
import { computed, onMounted, ref } from 'vue'
import {
  History, CheckCircle2, XCircle, Search, Eye, Download, ShieldCheck, Bot
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import { applicationsApi } from '@/services/applicationsApi'
import { formatDate } from '@/utils/dateFormat'

const loading = ref(false)
const error = ref('')
const search = ref('')
const resultFilter = ref('')
const history = ref([])

const filteredHistory = computed(() => {
  const query = search.value.trim().toLowerCase()
  return history.value.filter((item) => {
    const matchesSearch =
      !query ||
      item.id.toLowerCase().includes(query) ||
      item.student.toLowerCase().includes(query) ||
      item.type.toLowerCase().includes(query)
    const matchesResult = !resultFilter.value || item.result === resultFilter.value
    return matchesSearch && matchesResult
  })
})

const summaryCards = computed(() => [
  { label: 'Tổng đơn đã xử lý', value: history.value.length, tone: 'primary', icon: History },
  { label: 'Phê duyệt', value: history.value.filter((item) => item.result === 'approved').length, tone: 'success', icon: CheckCircle2 },
  { label: 'Từ chối', value: history.value.filter((item) => item.result === 'rejected').length, tone: 'danger', icon: XCircle },
  { label: 'Xử lý tự động', value: history.value.filter((item) => item.handlerType === 'auto').length, tone: 'info', icon: Bot },
])

const getResultBadge = (r) => ({
  approved: { label: 'Đã phê duyệt', variant: 'success' },
  rejected: { label: 'Đã từ chối', variant: 'danger' },
  cancelled: { label: 'Đã hủy', variant: 'neutral' },
}[r] || { label: r, variant: 'neutral' })

function unwrapList(payload) {
  const data = payload?.data ?? payload?.Data ?? payload
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  return []
}

function mapHistory(item) {
  const status = item.trangThai ?? item.TrangThai ?? ''
  const result = status === 'da_duyet' ? 'approved' : status === 'tu_choi' ? 'rejected' : 'cancelled'
  const handler = item.tenNguoiXuLy ?? item.TenNguoiXuLy ?? item.nguoiXuLy ?? item.NguoiXuLy ?? 'Chưa ghi nhận'
  return {
    id: String(item.maDonTu ?? item.MaDonTu ?? item.id ?? ''),
    student: item.tenSinhVien ?? item.TenSinhVien ?? item.sinhVien ?? item.SinhVien ?? 'Sinh viên',
    type: item.tenLoaiDon ?? item.TenLoaiDon ?? item.loaiDon ?? item.LoaiDon ?? 'Không xác định',
    result,
    handler,
    handlerType: String(handler).toLowerCase().includes('auto') ? 'auto' : 'manual',
    date: item.ngayDuyet ?? item.NgayDuyet ?? item.ngayCapNhat ?? item.NgayCapNhat,
    sla: item.slaTrangThai ?? item.SlaTrangThai ?? '—',
  }
}

async function loadHistory() {
  loading.value = true
  error.value = ''
  try {
    const [approved, rejected] = await Promise.all([
      applicationsApi.getAdminApplications({ trangThai: 'da_duyet', pageSize: 50 }),
      applicationsApi.getAdminApplications({ trangThai: 'tu_choi', pageSize: 50 }),
    ])
    history.value = [...unwrapList(approved), ...unwrapList(rejected)].map(mapHistory)
  } catch (e) {
    error.value = e.message || 'Không tải được lịch sử đơn đã xử lý.'
    history.value = []
  } finally {
    loading.value = false
  }
}

onMounted(loadHistory)
</script>

<template>
  <div class="h-full flex flex-col space-y-4 max-w-7xl mx-auto w-full">
    <div class="flex items-start justify-between flex-wrap gap-4">
      <div>
        <div class="flex items-center gap-2">
          <History class="text-(--lg-primary)" :size="24" />
          <h1 class="text-xl font-bold text-(--text-heading)">Đơn đã xử lý</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Tra cứu các đơn đã phê duyệt, từ chối hoặc hoàn tất xử lý.</p>
      </div>
      <div class="flex gap-2">
        <GlassButton variant="secondary" disabled>
          <Download :size="15" class="mr-1" /> Xuất báo cáo
        </GlassButton>
      </div>
    </div>

    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="card in summaryCards" :key="card.label" class="surface-card border border-(--border-card) rounded-2xl p-5 flex items-center justify-between shadow-sm h-full">
        <div>
          <p class="text-xs font-bold text-(--text-muted) uppercase tracking-wide">{{ card.label }}</p>
          <p class="text-3xl font-bold text-(--text-heading) mt-2">{{ card.value }}</p>
        </div>
        <div class="w-12 h-12 rounded-full flex items-center justify-center"
             :class="{
               'bg-(--accent-primary-soft) text-(--lg-primary)': card.tone === 'primary',
               'bg-(--color-danger-bg) text-(--color-danger-text)': card.tone === 'danger',
               'bg-(--color-success-bg) text-(--color-success-text)': card.tone === 'success',
               'bg-(--color-info-bg) text-(--color-info-text)': card.tone === 'info'
             }">
          <component :is="card.icon" :size="24" />
        </div>
      </div>
    </div>

    <div class="surface-card border border-(--border-card) rounded-2xl flex flex-col min-h-0 flex-1 shadow-sm overflow-hidden">
      <div class="p-4 border-b border-(--border-default) flex flex-wrap gap-3 bg-(--surface-input)">
        <div class="relative flex-1 min-w-[200px] max-w-sm">
          <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-(--text-muted)" :size="15" />
          <input v-model="search" type="text" placeholder="Tra cứu mã đơn, sinh viên, loại đơn..." class="pl-9 pr-4 h-10 w-full bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" />
        </div>
        <select v-model="resultFilter" class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
          <option value="">Kết quả xử lý</option>
          <option value="approved">Phê duyệt</option>
          <option value="rejected">Từ chối</option>
        </select>
        <GlassButton variant="secondary" :disabled="loading" @click="loadHistory">Làm mới</GlassButton>
      </div>

      <LoadingSkeleton v-if="loading" :lines="6" class="p-5" />
      <div v-else-if="error" class="p-6 text-sm text-(--color-danger-text)">{{ error }}</div>
      <div v-else-if="!filteredHistory.length" class="p-6 text-sm text-(--text-muted)">Chưa có đơn đã xử lý phù hợp.</div>

      <div v-else class="flex-1 overflow-x-auto">
        <table class="w-full text-left text-sm whitespace-nowrap">
          <thead class="bg-(--surface-input) border-b border-(--border-default) text-(--text-muted)">
            <tr>
              <th class="px-5 py-4 font-semibold w-24">Mã đơn</th>
              <th class="px-5 py-4 font-semibold">Sinh viên & Loại đơn</th>
              <th class="px-5 py-4 font-semibold">Kết quả</th>
              <th class="px-5 py-4 font-semibold">Người xử lý</th>
              <th class="px-5 py-4 font-semibold">Ngày xử lý</th>
              <th class="px-5 py-4 font-semibold">SLA</th>
              <th class="px-5 py-4 font-semibold text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-(--border-default)">
            <tr v-for="item in filteredHistory" :key="item.id" class="hover:bg-(--surface-hover) transition-colors">
              <td class="px-5 py-4 font-mono text-xs font-bold text-(--text-muted)">{{ item.id }}</td>
              <td class="px-5 py-4">
                <p class="font-bold text-(--text-heading)">{{ item.student }}</p>
                <p class="text-xs text-(--text-muted) mt-0.5">{{ item.type }}</p>
              </td>
              <td class="px-5 py-4">
                <GlassBadge :variant="getResultBadge(item.result).variant" size="sm">{{ getResultBadge(item.result).label }}</GlassBadge>
              </td>
              <td class="px-5 py-4">
                <div class="flex items-center gap-1.5 font-medium text-(--text-body)">
                  <Bot v-if="item.handlerType === 'auto'" :size="14" class="text-blue-500" />
                  <ShieldCheck v-else :size="14" class="text-emerald-500" />
                  {{ item.handler }}
                </div>
              </td>
              <td class="px-5 py-4 text-(--text-muted)">{{ formatDate(item.date, 'Chưa ghi nhận') }}</td>
              <td class="px-5 py-4">
                <span class="inline-flex px-2 py-0.5 rounded bg-(--surface-input) text-xs font-mono font-medium">{{ item.sla }}</span>
              </td>
              <td class="px-5 py-4 text-right">
                <GlassButton variant="secondary" size="sm" disabled>
                  <Eye :size="14" class="mr-1" /> Chi tiết
                </GlassButton>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="border-t border-(--border-default) p-4 flex justify-between items-center bg-(--surface-input)">
         <p class="text-sm text-(--text-muted)">Hiển thị {{ filteredHistory.length }} kết quả</p>
      </div>
    </div>
  </div>
</template>
